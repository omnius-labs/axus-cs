using System.Buffers;
using System.Collections.Immutable;
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

    private readonly Task _connectLoopTask;
    private readonly Task _acceptLoopTask;
    private readonly Task _sendLoopTask;
    private readonly Task _receiveLoopTask;
    private readonly Task _computeLoopTask;

    private readonly Random _random = new();

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly CompositeDisposable _disposables = new();

    private readonly object _lockObject = new();

    private const string ServiceName = "file_exchanger";

    public static async ValueTask<FileExchanger> CreateAsync(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
        IPublishedFileStorage publishedFileStorage, ISubscribedFileStorage subscribedFileStorage, IBatchActionDispatcher batchActionDispatcher,
        IBytesPool bytesPool, FileExchangerOptions options, CancellationToken cancellationToken = default)
    {
        var fileExchanger = new FileExchanger(sessionConnector, sessionAccepter, nodeFinder, publishedFileStorage, subscribedFileStorage, batchActionDispatcher, bytesPool, options);
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

        _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
        _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
        _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
        _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
        _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);

        _nodeFinder.GetEvents().GetPushContentClues.Subscribe(() => this.GetPushContentClues()).AddTo(_disposables);
        _nodeFinder.GetEvents().GetWantContentClues.Subscribe(() => this.GetWantContentClues()).AddTo(_disposables);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await Task.WhenAll(_connectLoopTask, _acceptLoopTask, _sendLoopTask, _receiveLoopTask, _computeLoopTask);
        _cancellationTokenSource.Dispose();

        foreach (var sessionStatus in _sessionStatusSet)
        {
            await sessionStatus.DisposeAsync();
        }
        _sessionStatusSet = _sessionStatusSet.Clear();

        _connectedAddressSet.Dispose();

        _disposables.Dispose();
    }

    public async ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default)
    {
        var sessionReports = new List<SessionReport>();

        foreach (var status in _sessionStatusSet)
        {
            sessionReports.Add(new SessionReport(ServiceName, status.Session.HandshakeType, status.Session.Address));
        }

        return sessionReports.ToArray();
    }

    public IEnumerable<ContentClue> GetPushContentClues()
    {
        return _pushContentClues;
    }

    public IEnumerable<ContentClue> GetWantContentClues()
    {
        return _wantContentClues;
    }

    private async Task ConnectLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Connected).Count();
                if (connectionCount > (_options.MaxSessionCount / 2)) continue;

                var wantContentHashes = await _subscribedFileStorage.GetWantContentHashesAsync(cancellationToken);

                var rootHash = wantContentHashes.Randomize().FirstOrDefault();
                if (rootHash == default) continue;

                var nodeLocation = await this.FindNodeLocationForConnecting(rootHash, cancellationToken);
                if (nodeLocation == null) continue;

                foreach (var targetAddress in nodeLocation.Addresses)
                {
                    _connectedAddressSet.Add(targetAddress);

                    var session = await _sessionConnector.ConnectAsync(targetAddress, ServiceName, cancellationToken);
                    if (session is null) continue;

                    if (await this.TryAddConnectedSessionAsync(session, rootHash, cancellationToken))
                    {
                        _logger.Debug("Connected: {0}", targetAddress);
                    }

                    break;
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
        }
    }

    private async ValueTask<NodeLocation?> FindNodeLocationForConnecting(OmniHash rootHash, CancellationToken cancellationToken)
    {
        var contentClue = RootHashToContentClue(rootHash);

        var nodeLocations = await _nodeFinder.FindNodeLocationsAsync(contentClue, cancellationToken);
        _random.Shuffle(nodeLocations);

        var ignoredAddressSet = await this.GetIgnoredAddressSet(cancellationToken);

        return nodeLocations
            .Where(n => !n.Addresses.Any(n => ignoredAddressSet.Contains(n)))
            .FirstOrDefault();
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

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Accepted).Count();
                if (connectionCount > (_options.MaxSessionCount / 2)) continue;

                var session = await _sessionAccepter.AcceptAsync(ServiceName, cancellationToken);
                if (session is null) continue;

                if (await this.TryAddAcceptedSessionAsync(session, cancellationToken))
                {
                    _logger.Debug("Accepted: {0}", session.Address);
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
        }
    }

    private async ValueTask<bool> TryAddConnectedSessionAsync(ISession session, OmniHash rootHash, CancellationToken cancellationToken = default)
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

                var sessionStatus = new SessionStatus(session, rootHash, _batchActionDispatcher);
                if (!this.TryAddSessionStatus(sessionStatus)) throw new Exception("Handshake failed.");

                return true;
            }

            throw new NotSupportedException();
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);

            await session.DisposeAsync();
        }
        catch (Exception e)
        {
            _logger.Warn(e);

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

                bool accepted = false;
                accepted |= await _publishedFileStorage.ContainsPushContentAsync(requestMessage.RootHash, cancellationToken);
                accepted |= await _subscribedFileStorage.ContainsWantContentAsync(requestMessage.RootHash, cancellationToken);

                if (!accepted)
                {
                    var rejectedResultMessage = new FileExchangerHandshakeResultMessage(FileExchangerHandshakeResultType.Rejected);
                    await session.Connection.Sender.SendAsync(rejectedResultMessage, cancellationToken);
                    throw new Exception("Handshake failed.");
                }

                var acceptedResultMessage = new FileExchangerHandshakeResultMessage(FileExchangerHandshakeResultType.Accepted);
                await session.Connection.Sender.SendAsync(acceptedResultMessage, cancellationToken);

                var sessionStatus = new SessionStatus(session, requestMessage.RootHash, _batchActionDispatcher);
                if (!this.TryAddSessionStatus(sessionStatus)) throw new Exception("Handshake failed.");

                return true;
            }

            throw new NotSupportedException();
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);

            await session.DisposeAsync();
        }
        catch (Exception e)
        {
            _logger.Warn(e);

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
        // 既に接続済みの場合は接続しない
        if (_sessionStatusSet.Any(n => n.Session.Signature == sessionStatus.Session.Signature && n.RootHash == sessionStatus.RootHash)) return false;
        _sessionStatusSet = _sessionStatusSet.Add(sessionStatus);

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

                        _logger.Debug($"Sent data message: {sessionStatus.Session.Address}");

                        foreach (var block in dataMessage.GiveBlocks)
                        {
                            _logger.Debug($"Sent block: ({block.Hash.ToString(ConvertStringType.Base58)})");

                            block.Value.Dispose();
                        }

                        sessionStatus.SentBlockHashes.UnionWith(dataMessage.GiveBlocks.Select(n => n.Hash));
                        sessionStatus.SendingDataMessage = null;
                    }
                    catch (Exception e)
                    {
                        _logger.Debug(e);

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
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
                    catch (Exception e)
                    {
                        _logger.Debug(e);

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
        }
    }

    private async Task ComputeLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken).ConfigureAwait(false);

            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken);

                await this.RemoveDeadSessionsAsync(cancellationToken);
                await this.RemoveUnnecessarySessionsAsync(cancellationToken);
                await this.ComputePushContentCluesAsync(cancellationToken);
                await this.ComputeWantContentCluesAsync(cancellationToken);
                await this.ComputeSendingDataMessage(cancellationToken);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
        }
    }

    private async ValueTask RemoveDeadSessionsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var sessionStatus in _sessionStatusSet)
        {
            var elapsed = (DateTime.UtcNow - sessionStatus.LastReceivedTime);
            if (elapsed.TotalMinutes < 3) continue;

            _logger.Debug($"Remove dead session: {sessionStatus.Session.Address}");

            await this.RemoveSessionStatusAsync(sessionStatus);
        }
    }

    private async ValueTask RemoveUnnecessarySessionsAsync(CancellationToken cancellationToken = default)
    {
        foreach (var sessionStatus in _sessionStatusSet)
        {
            bool necessary = false;
            necessary |= await _publishedFileStorage.ContainsPushContentAsync(sessionStatus.RootHash, cancellationToken);
            necessary |= await _subscribedFileStorage.ContainsWantContentAsync(sessionStatus.RootHash, cancellationToken);

            if (necessary) continue;

            _logger.Debug($"Remove unnecessary session: {sessionStatus.Session.Address}");

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

        foreach (var rootHash in await _publishedFileStorage.GetPushContentHashesAsync(cancellationToken))
        {
            var contentClue = RootHashToContentClue(rootHash);
            builder.Add(contentClue);
        }

        _pushContentClues = builder.ToImmutable();
    }

    private async ValueTask ComputeWantContentCluesAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

        foreach (var rootHash in await _subscribedFileStorage.GetWantContentHashesAsync(cancellationToken))
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
        return new ContentClue(ServiceName, rootHash);
    }
}
