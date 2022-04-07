using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using Omnius.Axis.Engines.Internal.Models;
using Omnius.Axis.Models;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Serialization;
using Omnius.Core.Tasks;

namespace Omnius.Axis.Engines;

public sealed partial class FileExchanger : AsyncDisposableBase, IFileExchanger
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ISessionConnector _sessionConnector;
    private readonly ISessionAccepter _sessionAccepter;
    private readonly INodeFinder _nodeFinder;
    private readonly IPublishedFileStorage _publishedFileStorage;
    private readonly ISubscribedFileStorage _subscribedFileStorage;
    private readonly IBatchActionDispatcher _batchActionDispatcher;
    private readonly IBytesPool _bytesPool;
    private readonly FileExchangerOptions _options;

    private readonly VolatileHashSet<OmniAddress> _connectedAddressSet;

    private volatile ImmutableHashSet<SessionStatus> _sessionStatusSet = ImmutableHashSet<SessionStatus>.Empty;

    private volatile ImmutableHashSet<ContentClue> _pushContentClues = ImmutableHashSet<ContentClue>.Empty;
    private volatile ImmutableHashSet<ContentClue> _wantContentClues = ImmutableHashSet<ContentClue>.Empty;

    private Task? _connectForPublishLoopTask;
    private Task? _connectForSubscribeLoopTask;
    private Task? _acceptLoopTask;
    private Task? _sendLoopTask;
    private Task? _receiveLoopTask;
    private Task? _computeLoopTask;
    private readonly IDisposable _getPushContentCluesListenerRegister;
    private readonly IDisposable _getWantContentCluesListenerRegister;

    private readonly Random _random = new();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly CompositeDisposable _disposables = new();

    private readonly object _lockObject = new();

    private const string Schema = "file_exchanger";

    public static async ValueTask<FileExchanger> CreateAsync(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
        IPublishedFileStorage publishedFileStorage, ISubscribedFileStorage subscribedFileStorage, IBatchActionDispatcher batchActionDispatcher,
        IBytesPool bytesPool, FileExchangerOptions options, CancellationToken cancellationToken = default)
    {
        var fileExchanger = new FileExchanger(sessionConnector, sessionAccepter, nodeFinder, publishedFileStorage, subscribedFileStorage, batchActionDispatcher, bytesPool, options);
        await fileExchanger.InitAsync(cancellationToken);
        return fileExchanger;
    }

    private FileExchanger(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
        IPublishedFileStorage publishedFileStorage, ISubscribedFileStorage subscribedFileStorage, IBatchActionDispatcher batchActionDispatcher,
        IBytesPool bytesPool, FileExchangerOptions options)
    {
        _sessionConnector = sessionConnector;
        _sessionAccepter = sessionAccepter;
        _nodeFinder = nodeFinder;
        _publishedFileStorage = publishedFileStorage;
        _subscribedFileStorage = subscribedFileStorage;
        _batchActionDispatcher = batchActionDispatcher;
        _bytesPool = bytesPool;
        _options = options;

        _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), _batchActionDispatcher);

        _getPushContentCluesListenerRegister = _nodeFinder.GetEvents().GetPushContentCluesListener.Listen(() => _pushContentClues);
        _getWantContentCluesListenerRegister = _nodeFinder.GetEvents().GetWantContentCluesListener.Listen(() => _wantContentClues);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        _connectForPublishLoopTask = this.ConnectForPublishLoopAsync(_cancellationTokenSource.Token);
        _connectForSubscribeLoopTask = this.ConnectForSubscribeLoopAsync(_cancellationTokenSource.Token);
        _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
        _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
        _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
        _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await Task.WhenAll(_connectForPublishLoopTask!, _connectForSubscribeLoopTask!, _acceptLoopTask!, _sendLoopTask!, _receiveLoopTask!, _computeLoopTask!);
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

    private async Task ConnectForPublishLoopAsync(CancellationToken cancellationToken = default)
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

                foreach (var rootHash in await _publishedFileStorage.GetPushRootHashesAsync(cancellationToken))
                {
                    foreach (var nodeLocation in await this.FindNodeLocationsForConnecting(rootHash, cancellationToken))
                    {
                        var result = await this.TryConnectAsync(nodeLocation, ExchangeType.Published, rootHash, cancellationToken);
                        if (result) break;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async Task ConnectForSubscribeLoopAsync(CancellationToken cancellationToken = default)
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

                foreach (var rootHash in await _subscribedFileStorage.GetWantRootHashesAsync(cancellationToken))
                {
                    foreach (var nodeLocation in await this.FindNodeLocationsForConnecting(rootHash, cancellationToken))
                    {
                        var result = await this.TryConnectAsync(nodeLocation, ExchangeType.Subscribed, rootHash, cancellationToken);
                        if (result) break;
                    }
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async ValueTask<IEnumerable<NodeLocation>> FindNodeLocationsForConnecting(OmniHash rootHash, CancellationToken cancellationToken)
    {
        var contentClue = RootHashToContentClue(rootHash);

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

    private async ValueTask<bool> TryConnectAsync(NodeLocation nodeLocation, ExchangeType exchangeType, OmniHash rootHash, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var targetAddress in nodeLocation.Addresses)
            {
                _connectedAddressSet.Add(targetAddress);

                var session = await _sessionConnector.ConnectAsync(targetAddress, Schema, cancellationToken);
                if (session is null) continue;

                var result = await this.TryAddConnectedSessionAsync(session, exchangeType, rootHash, cancellationToken);
                if (!result) continue;
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");
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

                await this.TryAddAcceptedSessionAsync(session, cancellationToken);
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");
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
            if (version is null) throw new Exception("Handshake failed.");

            if (version == FileExchangerVersion.Version1)
            {
                var requestMessage = new FileExchangerHandshakeRequestMessage(rootHash);
                var resultMessage = await session.Connection.SendAndReceiveAsync<FileExchangerHandshakeRequestMessage, FileExchangerHandshakeResultMessage>(requestMessage, cancellationToken);

                if (resultMessage.Type != FileExchangerHandshakeResultType.Accepted) throw new Exception("Handshake failed.");

                var sessionStatus = new SessionStatus(session, exchangeType, rootHash, _batchActionDispatcher);
                if (!this.TryAddSessionStatus(sessionStatus)) throw new Exception("Handshake failed.");

                return true;
            }

            throw new NotSupportedException();
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");

            await session.DisposeAsync();
        }
        catch (ConnectionException)
        {
            _logger.Debug("Connection Exception");

            await session.DisposeAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");

            await session.DisposeAsync();
        }

        return false;
    }

    private async ValueTask<bool> TryAddAcceptedSessionAsync(ISession session, CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await this.HandshakeVersionAsync(session.Connection, cancellationToken);
            if (version is null) throw new Exception("Handshake failed.");

            if (version == FileExchangerVersion.Version1)
            {
                var requestMessage = await session.Connection.Receiver.ReceiveAsync<FileExchangerHandshakeRequestMessage>(cancellationToken);

                if (await _publishedFileStorage.ContainsPushContentAsync(requestMessage.RootHash, cancellationToken))
                {
                    var resultMessage = new FileExchangerHandshakeResultMessage(FileExchangerHandshakeResultType.Accepted);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    var sessionStatus = new SessionStatus(session, ExchangeType.Published, requestMessage.RootHash, _batchActionDispatcher);
                    return this.TryAddSessionStatus(sessionStatus);
                }
                else if (await _subscribedFileStorage.ContainsWantContentAsync(requestMessage.RootHash, cancellationToken))
                {
                    var resultMessage = new FileExchangerHandshakeResultMessage(FileExchangerHandshakeResultType.Accepted);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    var sessionStatus = new SessionStatus(session, ExchangeType.Subscribed, requestMessage.RootHash, _batchActionDispatcher);
                    return this.TryAddSessionStatus(sessionStatus);
                }
                else
                {
                    var resultMessage = new FileExchangerHandshakeResultMessage(FileExchangerHandshakeResultType.Rejected);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);
                    return false;
                }
            }

            throw new NotSupportedException();
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");

            await session.DisposeAsync();
        }
        catch (ConnectionException)
        {
            _logger.Debug("Connection Exception");

            await session.DisposeAsync();
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");

            await session.DisposeAsync();
        }

        return false;
    }

    private async ValueTask<FileExchangerVersion?> HandshakeVersionAsync(IConnection connection, CancellationToken cancellationToken = default)
    {
        var myHelloMessage = new FileExchangerHelloMessage(new[] { FileExchangerVersion.Version1 });
        var otherHelloMessage = await connection.ExchangeAsync(myHelloMessage, cancellationToken);

        var version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
        return version;
    }

    private bool TryAddSessionStatus(SessionStatus sessionStatus)
    {
        lock (_lockObject)
        {
            // 既に接続済みの場合は接続しない
            if (_sessionStatusSet.Any(n => n.Session.Signature == sessionStatus.Session.Signature && n.RootHash == sessionStatus.RootHash)) return false;
            _sessionStatusSet = _sessionStatusSet.Add(sessionStatus);
        }

        return true;
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

                        _logger.Debug($"[{nameof(FileExchanger)}] Sent Messsage ({sessionStatus.Session.Address})");

                        foreach (var block in dataMessage.GiveBlocks)
                        {
                            _logger.Debug($"[{nameof(FileExchanger)}] Sent Block ({sessionStatus.Session.Address}) ({block.Hash.ToString(ConvertStringType.Base16)})");

                            block.Value.Dispose();
                        }

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
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");
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

                        _logger.Debug($"Received data message: {sessionStatus.Session.Address}");

                        sessionStatus.LastReceivedTime = DateTime.UtcNow;

                        try
                        {
                            sessionStatus.ReceivedWantBlockHashes.UnionWith(dataMessage.WantBlockHashes);

                            foreach (var block in dataMessage.GiveBlocks)
                            {
                                _logger.Debug($"Received block: ({block.Hash.ToString(ConvertStringType.Base58)})");

                                await _subscribedFileStorage.WriteBlockAsync(sessionStatus.RootHash, block.Hash, block.Value.Memory, cancellationToken);
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
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    private async Task ComputeLoopAsync(CancellationToken cancellationToken)
    {
        var trimSessionsStopwatch = new Stopwatch();
        var computeContentCluesStopwatch = new Stopwatch();
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

                if (computeContentCluesStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromSeconds(30)))
                {
                    await this.ComputePushContentCluesAsync(cancellationToken);
                    await this.ComputeWantContentCluesAsync(cancellationToken);
                }

                if (computeSendingDataStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromSeconds(10)))
                {
                    await this.ComputeSendingDataMessage(cancellationToken);
                }
            }
        }
        catch (OperationCanceledException)
        {
            _logger.Debug("Operation Canceled");
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
            bool necessary = false;
            necessary |= await _publishedFileStorage.ContainsPushContentAsync(sessionStatus.RootHash, cancellationToken);
            necessary |= await _subscribedFileStorage.ContainsWantContentAsync(sessionStatus.RootHash, cancellationToken);

            if (necessary) continue;

            _logger.Debug($"[{nameof(FileExchanger)}] Trim unnecessary session ({sessionStatus.Session.Address})");

            await this.RemoveSessionStatusAsync(sessionStatus);
        }
    }

    private async ValueTask RemoveSessionStatusAsync(SessionStatus sessionStatus)
    {
        _sessionStatusSet = _sessionStatusSet.Remove(sessionStatus);
        await sessionStatus.DisposeAsync();
    }

    private async ValueTask ComputePushContentCluesAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

        foreach (var rootHash in await _publishedFileStorage.GetPushRootHashesAsync(cancellationToken))
        {
            var contentClue = RootHashToContentClue(rootHash);
            builder.Add(contentClue);
        }

        _pushContentClues = builder.ToImmutable();
    }

    private async ValueTask ComputeWantContentCluesAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

        foreach (var rootHash in await _subscribedFileStorage.GetWantRootHashesAsync(cancellationToken))
        {
            var contentClue = RootHashToContentClue(rootHash);
            builder.Add(contentClue);
        }

        _wantContentClues = builder.ToImmutable();
    }

    private async ValueTask ComputeSendingDataMessage(CancellationToken cancellationToken = default)
    {
        foreach (var sessionStatus in _sessionStatusSet)
        {
            if (sessionStatus.SendingDataMessage is not null) continue;

            var wantBlockHashes = await this.ComputeWantBlockHashes(sessionStatus, cancellationToken);
            var giveBlocks = await this.ComputeGiveBlocks(sessionStatus, cancellationToken);

            sessionStatus.SendingDataMessage = new FileExchangerDataMessage(wantBlockHashes, giveBlocks.ToArray());
        }
    }

    private async ValueTask<OmniHash[]> ComputeWantBlockHashes(SessionStatus sessionStatus, CancellationToken cancellationToken)
    {
        var wantBlockHashes = await _subscribedFileStorage.GetWantBlockHashesAsync(sessionStatus.RootHash, cancellationToken);

        var results = wantBlockHashes.Randomize().Take(FileExchangerDataMessage.MaxWantBlockHashesCount).ToArray();
        return results;
    }

    private async ValueTask<Block[]> ComputeGiveBlocks(SessionStatus sessionStatus, CancellationToken cancellationToken)
    {
        var results = new List<Block>();

        var receivedWantBlockHashSet = sessionStatus.ReceivedWantBlockHashes;
        receivedWantBlockHashSet.ExceptWith(sessionStatus.SentBlockHashes);

        foreach (var blockHash in receivedWantBlockHashSet)
        {
            IMemoryOwner<byte>? value = null;

            if (await _publishedFileStorage.ContainsPushBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken))
            {
                value = await _publishedFileStorage.ReadBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken);
            }
            else if (await _subscribedFileStorage.ContainsWantBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken))
            {
                value = await _subscribedFileStorage.ReadBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken);
            }

            if (value is null) continue;

            results.Add(new Block(blockHash, value));

            if (results.Count >= FileExchangerDataMessage.MaxGiveBlocksCount)
            {
                goto End;
            }
        }

    End:

        receivedWantBlockHashSet.ExceptWith(results.Select(n => n.Hash));

        return results.ToArray();
    }

    private static ContentClue RootHashToContentClue(OmniHash rootHash)
    {
        return new ContentClue(Schema, rootHash);
    }
}
