using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using Omnius.Axus.Core.Engine;
using Omnius.Axus.Core.Engine.Models;
using Omnius.Axus.Core.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Tasks;

namespace Omnius.Axus.Core.Engine.Services;

public sealed partial class FileExchanger : AsyncDisposableBase, IFileExchanger
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ISessionConnector _sessionConnector;
    private readonly ISessionAccepter _sessionAccepter;
    private readonly INodeFinder _nodeFinder;
    private readonly IFilePublisherStorage _publishedFileStorage;
    private readonly IFileSubscriberStorage _subscribedFileStorage;
    private readonly ISystemClock _systemClock;
    private readonly IBytesPool _bytesPool;
    private readonly FileExchangerOptions _options;

    private readonly IBatchActionDispatcher _batchActionDispatcher = new BatchActionDispatcher(TimeSpan.FromTicks(100));

    private readonly VolatileHashSet<OmniAddress> _connectedAddressSet;

    private ImmutableHashSet<SessionStatus> _sessionStatusSet = ImmutableHashSet<SessionStatus>.Empty;

    private ImmutableHashSet<ContentKey> _pushContentKeys = ImmutableHashSet<ContentKey>.Empty;
    private ImmutableHashSet<ContentKey> _wantContentKeys = ImmutableHashSet<ContentKey>.Empty;

    private Task? _publishConnectLoopTask;
    private Task? _subscribeConnectLoopTask;
    private Task? _acceptLoopTask;
    private Task? _sendLoopTask;
    private Task? _receiveLoopTask;
    private Task? _computeLoopTask;
    private readonly IDisposable _getPushContentKeysListenerRegister;
    private readonly IDisposable _getWantContentKeysListenerRegister;

    private readonly Random _random = new();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly CompositeDisposable _disposables = new();

    private readonly object _lockObject = new();

    private const string Schema = "file-exchanger-v1";

    public static async ValueTask<FileExchanger> CreateAsync(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
        IFilePublisherStorage publishedFileStorage, IFileSubscriberStorage subscribedFileStorage,
        ISystemClock systemClock, IBytesPool bytesPool, FileExchangerOptions options, CancellationToken cancellationToken = default)
    {
        var fileExchanger = new FileExchanger(sessionConnector, sessionAccepter, nodeFinder, publishedFileStorage, subscribedFileStorage, systemClock, bytesPool, options);
        await fileExchanger.InitAsync(cancellationToken);
        return fileExchanger;
    }

    private FileExchanger(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
        IFilePublisherStorage publishedFileStorage, IFileSubscriberStorage subscribedFileStorage,
        ISystemClock systemClock, IBytesPool bytesPool, FileExchangerOptions options)
    {
        _sessionConnector = sessionConnector;
        _sessionAccepter = sessionAccepter;
        _nodeFinder = nodeFinder;
        _publishedFileStorage = publishedFileStorage;
        _subscribedFileStorage = subscribedFileStorage;
        _systemClock = systemClock;
        _bytesPool = bytesPool;
        _options = options;

        _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), _systemClock, _batchActionDispatcher);

        _getPushContentKeysListenerRegister = _nodeFinder.OnGetPushContentKeys.Listen(() => _pushContentKeys);
        _getWantContentKeysListenerRegister = _nodeFinder.OnGetWantContentKeys.Listen(() => _wantContentKeys);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        _publishConnectLoopTask = this.PublishConnectLoopAsync(_cancellationTokenSource.Token);
        _subscribeConnectLoopTask = this.SubscribeConnectLoopAsync(_cancellationTokenSource.Token);
        _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
        _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
        _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
        _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await Task.WhenAll(_publishConnectLoopTask!, _subscribeConnectLoopTask!, _acceptLoopTask!, _sendLoopTask!, _receiveLoopTask!, _computeLoopTask!);
        _cancellationTokenSource.Dispose();

        foreach (var sessionStatus in _sessionStatusSet)
        {
            await sessionStatus.DisposeAsync();
        }

        _sessionStatusSet = _sessionStatusSet.Clear();

        _connectedAddressSet.Dispose();

        _getPushContentKeysListenerRegister.Dispose();
        _getWantContentKeysListenerRegister.Dispose();
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

    private async Task PublishConnectLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                var sessionStatuses = _sessionStatusSet
                    .Where(n => n.Session.HandshakeType == SessionHandshakeType.Connected)
                    .Where(n => n.ExchangeType == ExchangeType.Published).ToList();
                if (sessionStatuses.Count > _options.MaxSessionCount / 4) continue;

                var rootHashes = await _publishedFileStorage.GetPushRootHashesAsync(cancellationToken);

                foreach (var rootHash in rootHashes.Randomize())
                {
                    foreach (var nodeLocation in await this.FindNodeLocationsForConnecting(rootHash, cancellationToken))
                    {
                        var success = await this.TryConnectAsync(nodeLocation, ExchangeType.Published, rootHash, cancellationToken);
                        if (success) break;
                    }

                    goto End;
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

    private async Task SubscribeConnectLoopAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                var sessionStatuses = _sessionStatusSet
                    .Where(n => n.Session.HandshakeType == SessionHandshakeType.Connected)
                    .Where(n => n.ExchangeType == ExchangeType.Subscribed).ToList();
                if (sessionStatuses.Count > _options.MaxSessionCount / 4) continue;

                var rootHashes = await _subscribedFileStorage.GetWantRootHashesAsync(cancellationToken);

                foreach (var rootHash in rootHashes.Randomize())
                {
                    foreach (var nodeLocation in await this.FindNodeLocationsForConnecting(rootHash, cancellationToken))
                    {
                        var success = await this.TryConnectAsync(nodeLocation, ExchangeType.Subscribed, rootHash, cancellationToken);
                        if (success) break;
                    }

                    goto End;
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

    private async ValueTask<IEnumerable<NodeLocation>> FindNodeLocationsForConnecting(OmniHash rootHash, CancellationToken cancellationToken)
    {
        var ContentKey = ContentKeyConverter.ToContentKey(Schema, rootHash);

        var nodeLocations = await _nodeFinder.FindNodeLocationsAsync(ContentKey, cancellationToken);
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

    private async ValueTask<bool> TryConnectAsync(NodeLocation nodeLocation, ExchangeType exchangeType, OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var targetAddress in nodeLocation.Addresses)
            {
                _connectedAddressSet.Add(targetAddress);

                var session = await _sessionConnector.ConnectAsync(targetAddress, Schema, cancellationToken);
                if (session is null) continue;

                var success = await this.TryAddConnectedSessionAsync(session, exchangeType, rootHash, cancellationToken);
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

    private async ValueTask<bool> TryAddConnectedSessionAsync(ISession session, ExchangeType exchangeType, OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await this.HandshakeVersionAsync(session.Connection, cancellationToken);

            if (version == FileExchangerVersion.Version1)
            {
                var requestMessage = new FileExchangerHandshakeRequestMessage(rootHash);
                var resultMessage = await session.Connection.SendAndReceiveAsync<FileExchangerHandshakeRequestMessage, FileExchangerHandshakeResultMessage>(requestMessage, cancellationToken);

                if (resultMessage.Type == FileExchangerHandshakeResultType.Succeeded)
                {
                    if (exchangeType == ExchangeType.Published)
                    {
                        _logger.Debug($"ConnectedSession Succeeded (Published): (Address: {session.Address})");
                    }
                    else if (exchangeType == ExchangeType.Subscribed)
                    {
                        _logger.Debug($"ConnectedSession Succeeded (Subscribed): (Address: {session.Address})");
                    }

                    return this.InternalTryAddSession(session, exchangeType, rootHash);
                }
                else if (resultMessage.Type == FileExchangerHandshakeResultType.Rejected)
                {
                    _logger.Debug($"ConnectedSession Rejected: (Address: {session.Address})");

                    return false;
                }
            }

            _logger.Debug("ConnectedSession Unknown Version");

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

            if (version == FileExchangerVersion.Version1)
            {
                var requestMessage = await session.Connection.Receiver.ReceiveAsync<FileExchangerHandshakeRequestMessage>(cancellationToken);

                if (await _publishedFileStorage.ContainsPushContentAsync(requestMessage.RootHash, cancellationToken))
                {
                    var resultMessage = new FileExchangerHandshakeResultMessage(FileExchangerHandshakeResultType.Succeeded);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    _logger.Debug($"AcceptedSession Succeeded (Published): (Address: {session.Address})");

                    var sessionStatus = new SessionStatus(session, ExchangeType.Published, requestMessage.RootHash, _systemClock, _batchActionDispatcher);
                    return this.InternalTryAddSession(session, ExchangeType.Published, requestMessage.RootHash);
                }
                else if (await _subscribedFileStorage.ContainsWantContentAsync(requestMessage.RootHash, cancellationToken))
                {
                    var resultMessage = new FileExchangerHandshakeResultMessage(FileExchangerHandshakeResultType.Succeeded);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    _logger.Debug($"AcceptedSession Succeeded (Subscribed): (Address: {session.Address})");

                    return this.InternalTryAddSession(session, ExchangeType.Subscribed, requestMessage.RootHash);
                }
                else
                {
                    var resultMessage = new FileExchangerHandshakeResultMessage(FileExchangerHandshakeResultType.Rejected);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    _logger.Debug($"AcceptedSession Rejected: (Address: {session.Address})");

                    return false;
                }
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

    private async ValueTask<FileExchangerVersion> HandshakeVersionAsync(IConnection connection, CancellationToken cancellationToken = default)
    {
        var myHelloMessage = new FileExchangerHelloMessage(new[] { FileExchangerVersion.Version1 });
        var otherHelloMessage = await connection.ExchangeAsync(myHelloMessage, cancellationToken);

        var version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
        return version ?? FileExchangerVersion.Unknown;
    }

    private bool InternalTryAddSession(ISession session, ExchangeType exchangeType, OmniHash rootHash)
    {
        lock (_lockObject)
        {
            // 既に接続済みの場合は接続しない
            if (_sessionStatusSet.Any(n => n.Session.Signature == session.Signature && n.RootHash == rootHash))
            {
                _logger.Debug($"Already connected");
                return false;
            }

            _sessionStatusSet = _sessionStatusSet.Add(new SessionStatus(session, exchangeType, rootHash, _systemClock, _batchActionDispatcher));

            return true;
        }
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);

                foreach (var sessionStatus in _sessionStatusSet)
                {
                    try
                    {
                        var dataMessage = sessionStatus.SendingDataMessage;
                        if (dataMessage is null || !sessionStatus.Session.Connection.Sender.TrySend(dataMessage)) continue;

                        if (dataMessage.WantBlockHashes.Count > 0)
                        {
                            _logger.Debug($"Send WantBlockHashes: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.WantBlockHashes.Count})");
                        }

                        if (dataMessage.GiveBlocks.Count > 0)
                        {
                            _logger.Debug($"Send GiveBlocks: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.GiveBlocks.Count})");
                        }

                        foreach (var block in dataMessage.GiveBlocks)
                        {
                            block.Value.Dispose();
                        }

                        sessionStatus.ReceivedWantBlockHashes.ExceptWith(dataMessage.GiveBlocks.Select(n => n.Hash));

                        sessionStatus.SentWantBlockHashes.UnionWith(dataMessage.WantBlockHashes);
                        sessionStatus.SentBlockHashes.UnionWith(dataMessage.GiveBlocks.Select(n => n.Hash));

                        sessionStatus.SendingDataMessage = null;
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.Debug("Object Disposed");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                    catch (ConnectionException)
                    {
                        _logger.Debug("Connection Exception");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Unexpected Exception");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
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

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);

                foreach (var sessionStatus in _sessionStatusSet)
                {
                    try
                    {
                        if (!sessionStatus.Session.Connection.Receiver.TryReceive<FileExchangerDataMessage>(out var dataMessage)) continue;

                        sessionStatus.LastReceivedTime = DateTime.UtcNow;

                        try
                        {
                            if (dataMessage.WantBlockHashes.Count > 0)
                            {
                                sessionStatus.ReceivedWantBlockHashes.UnionWith(dataMessage.WantBlockHashes);
                                _logger.Debug($"Receive WantBlockHashes: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.WantBlockHashes.Count})");
                            }

                            if (dataMessage.GiveBlocks.Count > 0)
                            {
                                foreach (var block in dataMessage.GiveBlocks)
                                {
                                    await _subscribedFileStorage.WriteBlockAsync(sessionStatus.RootHash, block.Hash, block.Value.Memory, cancellationToken);
                                }

                                _logger.Debug($"Receive GiveBlocks: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.GiveBlocks.Count})");
                            }
                        }
                        finally
                        {
                            foreach (var block in dataMessage.GiveBlocks)
                            {
                                block.Value.Dispose();
                            }
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.Debug("Object Disposed");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                    catch (ConnectionException)
                    {
                        _logger.Debug("Connection Exception");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Unexpected Exception");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
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

    private async Task ComputeLoopAsync(CancellationToken cancellationToken)
    {
        var trimSessionsStopwatch = new Stopwatch();
        var computeContentKeysStopwatch = new Stopwatch();
        var computeSendingDataStopwatch = new Stopwatch();

        try
        {
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);

                if (trimSessionsStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromSeconds(30)))
                {
                    await this.TrimDeadSessionsAsync(cancellationToken);
                    await this.TrimUnnecessarySessionsAsync(cancellationToken);
                }

                if (computeContentKeysStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromSeconds(30)))
                {
                    await this.ComputePushContentKeysAsync(cancellationToken);
                    await this.ComputeWantContentKeysAsync(cancellationToken);
                }

                if (computeSendingDataStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromSeconds(10)))
                {
                    await this.ComputeSendingDataMessage(cancellationToken);
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

    private async ValueTask TrimDeadSessionsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var sessionStatus in _sessionStatusSet)
        {
            var elapsed = (DateTime.UtcNow - sessionStatus.LastReceivedTime);
            if (elapsed.TotalMinutes < 3) continue;

            _logger.Debug($"[{nameof(FileExchanger)}] Trim dead session ({sessionStatus.Session.Address})");

            await this.RemoveSessionStatusAsync(sessionStatus);
        }
    }

    private async ValueTask TrimUnnecessarySessionsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var sessionStatus in _sessionStatusSet)
        {
            bool exists = false;
            exists |= await _publishedFileStorage.ContainsPushContentAsync(sessionStatus.RootHash, cancellationToken);
            exists |= await _subscribedFileStorage.ContainsWantContentAsync(sessionStatus.RootHash, cancellationToken);

            if (exists) continue;

            _logger.Debug($"[{nameof(FileExchanger)}] Trim unnecessary session ({sessionStatus.Session.Address})");

            await this.RemoveSessionStatusAsync(sessionStatus);
        }
    }

    private async ValueTask RemoveSessionStatusAsync(SessionStatus sessionStatus)
    {
        _sessionStatusSet = _sessionStatusSet.Remove(sessionStatus);
        await sessionStatus.DisposeAsync();
    }

    private async ValueTask ComputePushContentKeysAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentKey>();

        foreach (var rootHash in await _publishedFileStorage.GetPushRootHashesAsync(cancellationToken))
        {
            var contentKey = ContentKeyConverter.ToContentKey(Schema, rootHash);
            builder.Add(contentKey);
        }

        _pushContentKeys = builder.ToImmutable();
    }

    private async ValueTask ComputeWantContentKeysAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentKey>();

        foreach (var rootHash in await _subscribedFileStorage.GetWantRootHashesAsync(cancellationToken))
        {
            var contentKey = ContentKeyConverter.ToContentKey(Schema, rootHash);
            builder.Add(contentKey);
        }

        _wantContentKeys = builder.ToImmutable();
    }

    private async ValueTask ComputeSendingDataMessage(CancellationToken cancellationToken = default)
    {
        foreach (var sessionStatus in _sessionStatusSet)
        {
            if (sessionStatus.SendingDataMessage is not null) continue;

            var wantBlockHashes = await this.ComputeSendingWantBlockHashes(sessionStatus, cancellationToken);
            var giveBlocks = await this.ComputeSendingGiveBlocks(sessionStatus, cancellationToken);

            sessionStatus.SendingDataMessage = new FileExchangerDataMessage(wantBlockHashes, giveBlocks.ToArray());
        }
    }

    private async ValueTask<OmniHash[]> ComputeSendingWantBlockHashes(SessionStatus sessionStatus, CancellationToken cancellationToken)
    {
        var wantBlockHashes = new HashSet<OmniHash>(await _subscribedFileStorage.GetWantBlockHashesAsync(sessionStatus.RootHash, cancellationToken));
        wantBlockHashes.ExceptWith(sessionStatus.SentWantBlockHashes);

        var results = wantBlockHashes.Randomize().Take(FileExchangerDataMessage.MaxWantBlockHashesCount).ToArray();
        return results;
    }

    private async ValueTask<Block[]> ComputeSendingGiveBlocks(SessionStatus sessionStatus, CancellationToken cancellationToken)
    {
        var results = new List<Block>();

        foreach (var blockHash in sessionStatus.ReceivedWantBlockHashes)
        {
            IMemoryOwner<byte>? value = null;

            if (await _publishedFileStorage.ContainsPushBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken))
            {
                value = await _publishedFileStorage.TryReadBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken);
            }
            else if (await _subscribedFileStorage.ContainsWantBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken))
            {
                value = await _subscribedFileStorage.TryReadBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken);
            }

            if (value is null) continue;

            results.Add(new Block(blockHash, value));

            if (results.Count >= FileExchangerDataMessage.MaxGiveBlocksCount)
            {
                break;
            }
        }

        sessionStatus.ReceivedWantBlockHashes.ExceptWith(results.Select(n => n.Hash));

        return results.ToArray();
    }
}
