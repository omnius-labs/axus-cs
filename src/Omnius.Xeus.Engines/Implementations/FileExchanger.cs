using System.Buffers;
using System.Collections.Immutable;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Tasks;
using Omnius.Xeus.Engines.Internal.Models;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines;

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
    private ImmutableHashSet<SessionStatus> _sessionStatusSet = ImmutableHashSet<SessionStatus>.Empty;

    private ImmutableHashSet<ContentClue> _pushContentClues = ImmutableHashSet<ContentClue>.Empty;
    private ImmutableHashSet<ContentClue> _wantContentClues = ImmutableHashSet<ContentClue>.Empty;

    private readonly Task _connectLoopTask;
    private readonly Task _acceptLoopTask;
    private readonly Task _sendLoopTask;
    private readonly Task _receiveLoopTask;
    private readonly Task _computeLoopTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly Random _random = new();

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

        _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromMinutes(3), _batchActionDispatcher);

        _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
        _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
        _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
        _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
        _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await Task.WhenAll(_connectLoopTask, _acceptLoopTask, _sendLoopTask, _receiveLoopTask, _computeLoopTask);
        _cancellationTokenSource.Dispose();

        _connectedAddressSet.Dispose();
    }

    public async ValueTask<FileExchangerReport> GetReportAsync(CancellationToken cancellationToken = default)
    {
        var sessionReports = new List<SessionReport>();

        foreach (var status in _sessionStatusSet)
        {
            sessionReports.Add(new SessionReport(ServiceName, status.Session.HandshakeType, status.Session.Address));
        }

        return new FileExchangerReport(0, 0, sessionReports.ToArray());
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
            var random = new Random();

            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Connected).Count();
                if (_sessionStatusSet.Count > (_options.MaxSessionCount / 2)) continue;

                var rootHash = (await _subscribedFileStorage.GetRootHashesAsync(cancellationToken)).Randomize().FirstOrDefault();
                if (rootHash == default) continue;

                var nodeLocation = await this.FindNodeLocationToConnecting(rootHash, cancellationToken);
                if (nodeLocation == null) continue;

                foreach (var targetAddress in nodeLocation.Addresses)
                {
                    _logger.Debug("Connecting: {0}", targetAddress);

                    var session = await _sessionConnector.ConnectAsync(targetAddress, ServiceName, cancellationToken);
                    if (session is null) continue;

                    _logger.Debug("Connected: {0}", targetAddress);

                    _connectedAddressSet.Add(targetAddress);

                    if (await this.TryAddConnectedSessionAsync(session, rootHash, cancellationToken))
                    {
                        break;
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

    private async ValueTask<NodeLocation?> FindNodeLocationToConnecting(OmniHash rootHash, CancellationToken cancellationToken)
    {
        var contentClue = RootHashToContentClue(rootHash);

        var nodeLocations = await _nodeFinder.FindNodeLocationsAsync(contentClue, cancellationToken);
        _random.Shuffle(nodeLocations);

        var ignoreAddressSet = new HashSet<OmniAddress>();
        lock (_lockObject)
        {
            ignoreAddressSet.UnionWith(_sessionStatusSet.Select(n => n.Session.Address));
            ignoreAddressSet.UnionWith(_connectedAddressSet);
        }

        return nodeLocations
            .Where(n => !n.Addresses.Any(n => ignoreAddressSet.Contains(n)))
            .FirstOrDefault();
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Accepted).Count();
                if (_sessionStatusSet.Count > (_options.MaxSessionCount / 2)) continue;

                _logger.Debug("Accepting");

                var session = await _sessionAccepter.AcceptAsync(ServiceName, cancellationToken);
                if (session is null) continue;

                _logger.Debug("Accepted: {0}", session.Address);

                await this.TryAddAcceptedSessionAsync(session, cancellationToken);
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
            if (version is null) return false;

            _logger.Debug("Handshake version: {0}", version);

            if (version == FileExchangerVersion.Version1)
            {
                var requestMessage = new FileExchangerHandshakeRequestMessage(rootHash);
                var resultMessage = await session.Connection.SendAndReceiveAsync<FileExchangerHandshakeRequestMessage, FileExchangerHandshakeResultMessage>(requestMessage, cancellationToken);

                _logger.Debug("Handshake send request and receive result: {0}", resultMessage.Type);

                if (resultMessage.Type != FileExchangerHandshakeResultType.Accepted) return false;

                var status = new SessionStatus(session, rootHash);
                _sessionStatusSet = _sessionStatusSet.Add(status);

                _logger.Debug("Added session");

                return true;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Warn(e);
        }

        return false;
    }

    private async ValueTask<bool> TryAddAcceptedSessionAsync(ISession session, CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await this.HandshakeVersionAsync(session.Connection, cancellationToken);
            if (version is null) return false;

            _logger.Debug("Handshake version: {0}", version);

            if (version == FileExchangerVersion.Version1)
            {
                var requestMessage = await session.Connection.Receiver.ReceiveAsync<FileExchangerHandshakeRequestMessage>(cancellationToken);
                var rootHash = requestMessage.RootHash;

                _logger.Debug("Handshake receive request: {0}", rootHash);

                bool accepted = false;
                accepted |= await _publishedFileStorage.ContainsFileAsync(rootHash, cancellationToken);
                accepted |= await _subscribedFileStorage.ContainsFileAsync(rootHash, cancellationToken);

                var resultMessage = new FileExchangerHandshakeResultMessage(accepted ? FileExchangerHandshakeResultType.Accepted : FileExchangerHandshakeResultType.Rejected);
                await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                _logger.Debug("Handshake send result: {0}", resultMessage.Type);

                if (!accepted) return false;

                var status = new SessionStatus(session, rootHash);
                _sessionStatusSet = _sessionStatusSet.Add(status);

                _logger.Debug("Added session");

                return true;
            }
            else
            {
                throw new NotSupportedException();
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Warn(e);
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

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                foreach (var sessionStatus in _sessionStatusSet)
                {
                    try
                    {
                        var dataMessage = sessionStatus.SendingDataMessage;
                        if (dataMessage is null || !sessionStatus.Session.Connection.Sender.TrySend(dataMessage)) continue;

                        _logger.Debug($"Send data message: {sessionStatus.Session.Address}");

                        foreach (var block in dataMessage.GiveBlocks)
                        {
                            block.Value.Dispose();
                        }

                        sessionStatus.SentBlockHashes.UnionWith(dataMessage.GiveBlocks.Select(n => n.Hash));
                        sessionStatus.SendingDataMessage = null;
                    }
                    catch (Exception e)
                    {
                        _logger.Debug(e);

                        _sessionStatusSet = _sessionStatusSet.Remove(sessionStatus);
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

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                foreach (var sessionStatus in _sessionStatusSet)
                {
                    try
                    {
                        if (!sessionStatus.Session.Connection.Receiver.TryReceive<FileExchangerDataMessage>(out var dataMessage)) continue;

                        _logger.Debug($"Received data message: {sessionStatus.Session.Address}");

                        try
                        {
                            sessionStatus.ReceivedWantBlockHashes = new LockedSet<OmniHash>(dataMessage.WantBlockHashes.ToHashSet());

                            foreach (var block in dataMessage.GiveBlocks)
                            {
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

                        _sessionStatusSet = _sessionStatusSet.Remove(sessionStatus);
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
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);

                await this.UpdatePushContentCluesAsync(cancellationToken);
                await this.UpdateWantContentCluesAsync(cancellationToken);
                await this.UpdateSendingDataMessage(cancellationToken);
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

    private async ValueTask UpdatePushContentCluesAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

        foreach (var rootHash in await _publishedFileStorage.GetRootHashesAsync(cancellationToken))
        {
            var contentClue = RootHashToContentClue(rootHash);
            builder.Add(contentClue);
        }

        _pushContentClues = builder.ToImmutable();
    }

    private async ValueTask UpdateWantContentCluesAsync(CancellationToken cancellationToken = default)
    {
        var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

        foreach (var rootHash in await _subscribedFileStorage.GetRootHashesAsync(cancellationToken))
        {
            var contentClue = RootHashToContentClue(rootHash);
            builder.Add(contentClue);
        }

        _wantContentClues = builder.ToImmutable();
    }

    private async ValueTask UpdateSendingDataMessage(CancellationToken cancellationToken = default)
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
        var wantBlockHashes = await _subscribedFileStorage.GetBlockHashesAsync(sessionStatus.RootHash, false, cancellationToken);

        var results = wantBlockHashes.Randomize().Take(FileExchangerDataMessage.MaxWantBlockHashesCount).ToArray();
        return results;
    }

    private async ValueTask<Block[]> ComputeGiveBlocks(SessionStatus sessionStatus, CancellationToken cancellationToken)
    {
        var results = new List<Block>();

        var receivedWantBlockHashSet = sessionStatus.ReceivedWantBlockHashes;
        receivedWantBlockHashSet.ExceptWith(sessionStatus.SentBlockHashes);

        foreach (var contentStorage in new IReadOnlyFileStorage[] { _publishedFileStorage, _subscribedFileStorage })
        {
            foreach (var blockHash in receivedWantBlockHashSet)
            {
                if (!await contentStorage.ContainsBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken)) continue;

                var memoryOwner = await contentStorage.ReadBlockAsync(sessionStatus.RootHash, blockHash, cancellationToken);
                if (memoryOwner is null) continue;

                results.Add(new Block(blockHash, memoryOwner));

                if (results.Count >= FileExchangerDataMessage.MaxGiveBlocksCount)
                {
                    goto End;
                }
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
