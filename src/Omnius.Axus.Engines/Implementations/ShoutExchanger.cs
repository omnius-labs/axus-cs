using System.Collections.Immutable;
using System.Data;
using System.Diagnostics;
using Omnius.Axus.Engines.Internal;
using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Engines.Models;
using Omnius.Axus.Models;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.RocketPack;
using Omnius.Core.Tasks;

namespace Omnius.Axus.Engines;

public sealed partial class ShoutExchanger : AsyncDisposableBase, IShoutExchanger
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ISessionConnector _sessionConnector;
    private readonly ISessionAccepter _sessionAccepter;
    private readonly INodeFinder _nodeFinder;
    private readonly IPublishedShoutStorage _publishedShoutStorage;
    private readonly ISubscribedShoutStorage _subscribedShoutStorage;
    private readonly IBatchActionDispatcher _batchActionDispatcher;
    private readonly IBytesPool _bytesPool;
    private readonly ShoutExchangerOptions _options;

    private readonly VolatileHashSet<OmniAddress> _connectedAddressSet;

    private ImmutableHashSet<SessionStatus> _sessionStatusSet = ImmutableHashSet<SessionStatus>.Empty;

    private ImmutableHashSet<ContentClue> _pushContentClues = ImmutableHashSet<ContentClue>.Empty;
    private ImmutableHashSet<ContentClue> _wantContentClues = ImmutableHashSet<ContentClue>.Empty;

    private Task? _connectLoopTask;
    private Task? _acceptLoopTask;
    private Task? _computeLoopTask;
    private readonly IDisposable _getPushContentCluesListenerRegister;
    private readonly IDisposable _getWantContentCluesListenerRegister;

    private readonly Random _random = new();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly CompositeDisposable _disposables = new();

    private readonly object _lockObject = new();

    private const string Schema = "shout_exchanger";

    public static async ValueTask<ShoutExchanger> CreateAsync(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
        IPublishedShoutStorage publishedShoutStorage, ISubscribedShoutStorage subscribedShoutStorage, IBatchActionDispatcher batchActionDispatcher,
        IBytesPool bytesPool, ShoutExchangerOptions options, CancellationToken cancellationToken = default)
    {
        var shoutExchanger = new ShoutExchanger(sessionConnector, sessionAccepter, nodeFinder, publishedShoutStorage, subscribedShoutStorage, batchActionDispatcher, bytesPool, options);
        await shoutExchanger.InitAsync(cancellationToken);
        return shoutExchanger;
    }

    private ShoutExchanger(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
        IPublishedShoutStorage publishedShoutStorage, ISubscribedShoutStorage subscribedShoutStorage, IBatchActionDispatcher batchActionDispatcher,
        IBytesPool bytesPool, ShoutExchangerOptions options)
    {
        _sessionConnector = sessionConnector;
        _sessionAccepter = sessionAccepter;
        _nodeFinder = nodeFinder;
        _publishedShoutStorage = publishedShoutStorage;
        _subscribedShoutStorage = subscribedShoutStorage;
        _batchActionDispatcher = batchActionDispatcher;
        _bytesPool = bytesPool;
        _options = options;

        _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), _batchActionDispatcher);

        _getPushContentCluesListenerRegister = _nodeFinder.OnGetPushContentClues.Listen(() => _pushContentClues);
        _getWantContentCluesListenerRegister = _nodeFinder.OnGetWantContentClues.Listen(() => _wantContentClues);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
        _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
        _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await Task.WhenAll(_connectLoopTask!, _acceptLoopTask!, _computeLoopTask!);
        _cancellationTokenSource.Dispose();

        foreach (var sessionStatus in _sessionStatusSet)
        {
            await sessionStatus.DisposeAsync();
        }

        _sessionStatusSet = _sessionStatusSet.Clear();

        _connectedAddressSet.Dispose();

        _getPushContentCluesListenerRegister.Dispose();
        _getWantContentCluesListenerRegister.Dispose();
    }

    public async ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default)
    {
        var sessionReports = new List<SessionReport>();

        foreach (var status in _sessionStatusSet)
        {
            sessionReports.Add(new SessionReport(Schema, status.Session.HandshakeType, status.Session.Address));
        }

        return sessionReports.ToArray();
    }

    private async Task ConnectLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                var sessionStatuses = _sessionStatusSet
                    .Where(n => n.Session.HandshakeType == SessionHandshakeType.Connected).ToList();
                if (sessionStatuses.Count > _options.MaxSessionCount / 4) continue;

                var keys = await _subscribedShoutStorage.GetKeysAsync(cancellationToken);

                foreach (var (signature, channel) in keys.Randomize())
                {
                    foreach (var nodeLocation in await this.FindNodeLocationsForConnecting(signature, channel, cancellationToken))
                    {
                        var success = await this.TryConnectAsync(nodeLocation, signature, channel, cancellationToken);
                        if (success) goto End;
                    }
                }

            End:;
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask<IEnumerable<NodeLocation>> FindNodeLocationsForConnecting(OmniSignature signature, string channel, CancellationToken cancellationToken)
    {
        var contentClue = ContentClueConverter.ToContentClue(Schema, signature, channel);

        var nodeLocations = await _nodeFinder.FindNodeLocationsAsync(contentClue, cancellationToken);
        _random.Shuffle(nodeLocations);

        var ignoredAddressSet = await this.GetIgnoredAddressSet(cancellationToken);

        return nodeLocations
            .Where(n => !n.Addresses.Any(n => ignoredAddressSet.Contains(n)))
            .ToArray();
    }

    private async ValueTask<HashSet<OmniAddress>> GetIgnoredAddressSet(CancellationToken cancellationToken)
    {
        var myNodeLocation = await _nodeFinder.GetMyNodeLocationAsync();

        var set = new HashSet<OmniAddress>();
        set.UnionWith(myNodeLocation.Addresses);
        set.UnionWith(_sessionStatusSet.Select(n => n.Session.Address));
        set.UnionWith(_connectedAddressSet);

        return set;
    }

    private async ValueTask<bool> TryConnectAsync(NodeLocation nodeLocation, OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var targetAddress in nodeLocation.Addresses)
            {
                _connectedAddressSet.Add(targetAddress);

                var session = await _sessionConnector.ConnectAsync(targetAddress, Schema, cancellationToken);
                if (session is null) continue;

                var success = await this.TryAddConnectedSessionAsync(session, signature, channel, cancellationToken);
                if (success) return true;

                await session.DisposeAsync();
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }

        return false;
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                var sessionStatuses = _sessionStatusSet
                    .Where(n => n.Session.HandshakeType == SessionHandshakeType.Accepted).ToList();
                if (sessionStatuses.Count > _options.MaxSessionCount / 2) continue;

                var session = await _sessionAccepter.AcceptAsync(Schema, cancellationToken);
                if (session is null) continue;

                bool success = await this.TryAddAcceptedSessionAsync(session, cancellationToken);
                if (!success) await session.DisposeAsync();
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask<bool> TryAddConnectedSessionAsync(ISession session, OmniSignature signature, string channel, CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await this.HandshakeVersionAsync(session.Connection, cancellationToken);

            if (version == ShoutExchangerVersion.Version1)
            {
                var shoutUpdatedTime = await this.ReadShoutUpdatedTimeAsync(signature, channel, cancellationToken);

                var requestMessage = new ShoutExchangerFetchRequestMessage(signature, channel, Timestamp64.FromDateTime(shoutUpdatedTime.ToUniversalTime()));
                var resultMessage = await session.Connection.SendAndReceiveAsync<ShoutExchangerFetchRequestMessage, ShoutExchangerFetchResultMessage>(requestMessage, cancellationToken);

                _logger.Debug($"Send ShoutFetchRequest: (Signature: {requestMessage.Signature.ToString()})");

                if (resultMessage.Type == ShoutExchangerFetchResultType.Found && resultMessage.Shout is not null)
                {
                    _logger.Debug($"Receive ShoutFetchResult: (Type: Found, Signature: {requestMessage.Signature.ToString()})");

                    await _subscribedShoutStorage.WriteShoutAsync(resultMessage.Shout, cancellationToken);
                }
                else if (resultMessage.Type == ShoutExchangerFetchResultType.NotFound)
                {
                    _logger.Debug($"Receive ShoutFetchResult: (Type: NotFound, Signature: {requestMessage.Signature.ToString()})");

                    var shout = await this.TryReadShoutAsync(signature, channel, DateTime.MinValue, cancellationToken);

                    if (shout is not null)
                    {
                        var postMessage = new ShoutExchangerPostMessage(ShoutExchangerPostType.Found, null);
                        await session.Connection.Sender.SendAsync(postMessage, cancellationToken);

                        _logger.Debug($"Send ShoutPost: (Type: Found, Signature: {requestMessage.Signature.ToString()})");
                    }
                    else
                    {
                        var postMessage = new ShoutExchangerPostMessage(ShoutExchangerPostType.NotFound, null);
                        await session.Connection.Sender.SendAsync(postMessage, cancellationToken);

                        _logger.Debug($"Send ShoutPost: (Type: NotFound, Signature: {requestMessage.Signature.ToString()})");
                    }
                }
                else if (resultMessage.Type == ShoutExchangerFetchResultType.Rejected)
                {
                    _logger.Debug($"Receive ShoutFetchResult: (Type: Rejected, Signature: {requestMessage.Signature.ToString()})");
                }
                else if (resultMessage.Type == ShoutExchangerFetchResultType.Same)
                {
                    _logger.Debug($"Receive ShoutFetchResult: (Type: Same, Signature: {requestMessage.Signature.ToString()})");
                }

                return true;
            }

            _logger.Debug("Unknown Version");

            return false;
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (ConnectionException)
        {
            _logger.Debug("Connection Exception");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }

        return false;
    }

    private async ValueTask<bool> TryAddAcceptedSessionAsync(ISession session, CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await this.HandshakeVersionAsync(session.Connection, cancellationToken);

            if (version == ShoutExchangerVersion.Version1)
            {
                var requestMessage = await session.Connection.Receiver.ReceiveAsync<ShoutExchangerFetchRequestMessage>(cancellationToken);

                _logger.Debug($"Receive ShoutFetchRequest: (Signature: {requestMessage.Signature.ToString()})");

                var shoutUpdatedTime = await this.ReadShoutUpdatedTimeAsync(requestMessage.Signature, requestMessage.Channel, cancellationToken);

                if (requestMessage.ShoutUpdatedTime.ToDateTime() == shoutUpdatedTime)
                {
                    using var resultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.Same, null);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    _logger.Debug($"Send ShoutFetchResult: (Type: Same, Signature: {requestMessage.Signature.ToString()})");
                }
                else if (requestMessage.ShoutUpdatedTime.ToDateTime() < shoutUpdatedTime)
                {
                    using var shout = await this.TryReadShoutAsync(requestMessage.Signature, requestMessage.Channel, requestMessage.ShoutUpdatedTime.ToDateTime(), cancellationToken);
                    using var resultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.Found, shout);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    _logger.Debug($"Send ShoutFetchResult: (Type: Found, Signature: {requestMessage.Signature.ToString()})");
                }
                else
                {
                    using var resultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.NotFound, null);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    _logger.Debug($"Send ShoutFetchResult: (Type: NotFound, Signature: {requestMessage.Signature.ToString()})");

                    using var postMessage = await session.Connection.Receiver.ReceiveAsync<ShoutExchangerPostMessage>(cancellationToken);

                    if (postMessage.Type == ShoutExchangerPostType.Found && postMessage.Shout is not null)
                    {
                        _logger.Debug($"Send ShoutPost: (Type: Found, Signature: {requestMessage.Signature.ToString()})");

                        await _subscribedShoutStorage.WriteShoutAsync(postMessage.Shout, cancellationToken);
                    }
                    else if (postMessage.Type == ShoutExchangerPostType.NotFound)
                    {
                        _logger.Debug($"Send ShoutPost: (Type: NotFound, Signature: {requestMessage.Signature.ToString()})");
                    }
                }

                return true;
            }

            _logger.Debug("Unknown Version");

            return false;
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (ConnectionException)
        {
            _logger.Debug("Connection Exception");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }

        return false;
    }

    private async ValueTask<ShoutExchangerVersion> HandshakeVersionAsync(IConnection connection, CancellationToken cancellationToken = default)
    {
        var myHelloMessage = new ShoutExchangerHelloMessage(new[] { ShoutExchangerVersion.Version1 });
        var otherHelloMessage = await connection.ExchangeAsync(myHelloMessage, cancellationToken);

        var version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
        return version ?? ShoutExchangerVersion.Unknown;
    }

    private async ValueTask<DateTime> ReadShoutUpdatedTimeAsync(OmniSignature signature, string channel, CancellationToken cancellationToken)
    {
        var wantShoutUpdatedTime = await _subscribedShoutStorage.ReadShoutUpdatedTimeAsync(signature, channel, cancellationToken);
        var pushShoutUpdatedTime = await _publishedShoutStorage.ReadShoutUpdatedTimeAsync(signature, channel, cancellationToken);

        return wantShoutUpdatedTime > pushShoutUpdatedTime ? wantShoutUpdatedTime : pushShoutUpdatedTime;
    }

    public async ValueTask<Shout?> TryReadShoutAsync(OmniSignature signature, string channel, DateTime updatedTime, CancellationToken cancellationToken)
    {
        var result = await _subscribedShoutStorage.TryReadShoutAsync(signature, channel, updatedTime, cancellationToken);
        if (result is not null) return result;

        result = await _publishedShoutStorage.TryReadShoutAsync(signature, channel, updatedTime, cancellationToken);
        return result;
    }

    private async Task ComputeLoopAsync(CancellationToken cancellationToken)
    {
        var computeContentCluesStopwatch = new Stopwatch();

        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

                if (computeContentCluesStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromSeconds(30)))
                {
                    await this.UpdatePushContentCluesAsync(cancellationToken);
                    await this.UpdateWantContentCluesAsync(cancellationToken);
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask UpdatePushContentCluesAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

        foreach (var (signature, channel) in await _publishedShoutStorage.GetKeysAsync(cancellationToken))
        {
            var contentClue = ContentClueConverter.ToContentClue(Schema, signature, channel);
            builder.Add(contentClue);
        }

        _pushContentClues = builder.ToImmutable();
    }

    private async ValueTask UpdateWantContentCluesAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

        foreach (var (signature, channel) in await _subscribedShoutStorage.GetKeysAsync(cancellationToken))
        {
            var contentClue = ContentClueConverter.ToContentClue(Schema, signature, channel);
            builder.Add(contentClue);
        }

        _wantContentClues = builder.ToImmutable();
    }
}
