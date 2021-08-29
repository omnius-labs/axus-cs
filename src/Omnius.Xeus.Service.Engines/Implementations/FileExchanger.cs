using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Xeus.Service.Engines.Internal;
using Omnius.Xeus.Service.Engines.Internal.Models;
using Omnius.Xeus.Service.Engines.Primitives;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class FileExchanger : AsyncDisposableBase, IFileExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ISessionConnector _sessionConnector;
        private readonly ISessionAccepter _sessionAccepter;
        private readonly INodeFinder _nodeFinder;
        private readonly IPublishedFileStorage _publishedFileStorage;
        private readonly ISubscribedFileStorage _subscribedFileStorage;
        private readonly IBytesPool _bytesPool;
        private readonly FileExchangerOptions _options;

        private ImmutableHashSet<SessionStatus> _sessionStatusSet = ImmutableHashSet<SessionStatus>.Empty;

        private ImmutableHashSet<ContentClue> _pushContentClues = ImmutableHashSet<ContentClue>.Empty;
        private ImmutableHashSet<ContentClue> _wantContentClues = ImmutableHashSet<ContentClue>.Empty;

        private Task _connectLoopTask = null!;
        private Task _acceptLoopTask = null!;
        private Task _sendLoopTask = null!;
        private Task _receiveLoopTask = null!;
        private Task _computeLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly object _lockObject = new();

        private const string ServiceName = "file_exchanger";

        public FileExchanger(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
            IPublishedFileStorage publishedFileStorage, ISubscribedFileStorage subscribedFileStorage, IBytesPool bytesPool, FileExchangerOptions options)
        {
            _sessionConnector = sessionConnector;
            _sessionAccepter = sessionAccepter;
            _nodeFinder = nodeFinder;
            _publishedFileStorage = publishedFileStorage;
            _subscribedFileStorage = subscribedFileStorage;
            _bytesPool = bytesPool;
            _options = options;

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

        private readonly VolatileHashSet<OmniAddress> _connectedAddressSet = new(TimeSpan.FromMinutes(30));

        private async Task ConnectLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);

                    _connectedAddressSet.Refresh();

                    int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Connected).Count();
                    if (_sessionStatusSet.Count > (_options.MaxSessionCount / 2)) continue;

                    foreach (var contentHash in await _subscribedFileStorage.GetRootHashesAsync(cancellationToken))
                    {
                        var contentClue = HashToContentClue(contentHash);

                        NodeLocation? targetNodeLocation = null;
                        {
                            var nodeLocations = await _nodeFinder.FindNodeLocationsAsync(contentClue, cancellationToken);
                            random.Shuffle(nodeLocations);

                            var ignoreAddressSet = new HashSet<OmniAddress>();
                            lock (_lockObject)
                            {
                                ignoreAddressSet.UnionWith(_sessionStatusSet.Select(n => n.Session.Address));
                                ignoreAddressSet.UnionWith(_connectedAddressSet);
                            }

                            targetNodeLocation = nodeLocations
                                .Where(n => !n.Addresses.Any(n => ignoreAddressSet.Contains(n)))
                                .FirstOrDefault();
                        }

                        if (targetNodeLocation == null) continue;

                        foreach (var targetAddress in targetNodeLocation.Addresses)
                        {
                            var session = await _sessionConnector.ConnectAsync(targetAddress, ServiceName, cancellationToken);
                            if (session is null) continue;

                            _connectedAddressSet.Add(targetAddress);

                            if (await this.TryAddConnectedSessionAsync(session, contentHash, cancellationToken))
                            {
                                goto End;
                            }
                        }

                    End:;
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

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);

                    lock (_lockObject)
                    {
                        int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Accepted).Count();

                        if (_sessionStatusSet.Count > (_options.MaxSessionCount / 2)) continue;
                    }

                    var session = await _sessionAccepter.AcceptAsync(ServiceName, cancellationToken);
                    if (session is null) continue;

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

                if (version == FileExchangerVersion.Version1)
                {
                    var requestMessage = new FileExchangerHandshakeRequestMessage(rootHash);
                    var resultMessage = await session.Connection.SendAndReceiveAsync<FileExchangerHandshakeRequestMessage, FileExchangerHandshakeResultMessage>(requestMessage, cancellationToken);

                    if (resultMessage.Type != FileExchangerHandshakeResultType.Accepted) return false;

                    var status = new SessionStatus(session, rootHash);
                    _sessionStatusSet = _sessionStatusSet.Add(status);

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

                if (version == FileExchangerVersion.Version1)
                {
                    var requestMessage = await session.Connection.Receiver.ReceiveAsync<FileExchangerHandshakeRequestMessage>(cancellationToken);
                    var rootHash = requestMessage.RootHash;

                    bool accepted = false;
                    accepted |= await _publishedFileStorage.ContainsFileAsync(rootHash, cancellationToken);
                    accepted |= await _subscribedFileStorage.ContainsFileAsync(rootHash, cancellationToken);

                    var resultMessage = new FileExchangerHandshakeResultMessage(accepted ? FileExchangerHandshakeResultType.Accepted : FileExchangerHandshakeResultType.Rejected);
                    await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                    if (!accepted) return false;

                    var status = new SessionStatus(session, rootHash);
                    _sessionStatusSet = _sessionStatusSet.Add(status);

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
                while (cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1, cancellationToken);

                    foreach (var sessionStatus in _sessionStatusSet)
                    {
                        try
                        {
                            lock (sessionStatus.LockObject)
                            {
                                var dataMessage = sessionStatus.SendingDataMessage;
                                if (dataMessage is not null)
                                {
                                    if (sessionStatus.Session.Connection.Sender.TrySend(dataMessage))
                                    {
                                        foreach (var block in dataMessage.GiveBlocks)
                                        {
                                            block.Dispose();
                                        }

                                        sessionStatus.SendingDataMessage = null;
                                    }
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            lock (_lockObject)
                            {
                                _sessionStatusSet.Remove(sessionStatus);
                            }
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
                while (cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1, cancellationToken);

                    SessionStatus[] connectionStatuses;
                    lock (_lockObject)
                    {
                        connectionStatuses = _sessionStatusSet.ToArray();
                    }

                    foreach (var connectionStatus in connectionStatuses)
                    {
                        try
                        {
                            if (connectionStatus.Session.Connection.Receiver.TryReceive<FileExchangerDataMessage>(out var dataMessage))
                            {
                                try
                                {
                                    lock (connectionStatus.LockObject)
                                    {
                                        connectionStatus.ReceivedWantBlockHashes = dataMessage.WantBlockHashes.ToArray();
                                    }

                                    foreach (var block in dataMessage.GiveBlocks)
                                    {
                                        await _subscribedFileStorage.WriteBlockAsync(connectionStatus.ContentHash, block.Hash, block.Value, cancellationToken);
                                    }
                                }
                                finally
                                {
                                    foreach (var block in dataMessage.GiveBlocks)
                                    {
                                        block.Dispose();
                                    }
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            lock (_lockObject)
                            {
                                _sessionStatusSet.Remove(connectionStatus);
                            }
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
                var random = new Random();

                while (cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

                    await this.UpdatePushContentCluesAsync(cancellationToken);
                    await this.UpdateWantContentCluesAsync(cancellationToken);

                    SessionStatus[] connectionStatuses;
                    lock (_lockObject)
                    {
                        connectionStatuses = _sessionStatusSet.ToArray();
                    }

                    foreach (var connectionStatus in connectionStatuses)
                    {
                        OmniHash[] wantBlockHashes;
                        {
                            wantBlockHashes = (await _subscribedFileStorage.GetBlockHashesAsync(connectionStatus.ContentHash, false, cancellationToken))
                                .Randomize()
                                .Take(FileExchangerDataMessage.MaxWantBlockHashesCount)
                                .ToArray();
                        }

                        var giveBlocks = new List<Block>();
                        {
                            var receivedWantBlockHashSet = new HashSet<OmniHash>();
                            lock (connectionStatus.LockObject)
                            {
                                receivedWantBlockHashSet.UnionWith(connectionStatus.ReceivedWantBlockHashes ?? Array.Empty<OmniHash>());
                            }

                            foreach (var contentStorage in new IReadOnlyFileStorage[] { _publishedFileStorage, _subscribedFileStorage })
                            {
                                foreach (var hash in (await contentStorage.GetBlockHashesAsync(connectionStatus.ContentHash, true, cancellationToken)).Randomize())
                                {
                                    var memoryOwner = await contentStorage.ReadBlockAsync(connectionStatus.ContentHash, hash, cancellationToken);
                                    if (memoryOwner is null) continue;

                                    giveBlocks.Add(new Block(hash, memoryOwner));
                                    if (giveBlocks.Count >= FileExchangerDataMessage.MaxGiveBlocksCount)
                                    {
                                        goto End;
                                    }
                                }
                            }

                        End:;
                        }

                        lock (connectionStatus.LockObject)
                        {
                            connectionStatus.SendingDataMessage = new FileExchangerDataMessage(wantBlockHashes, giveBlocks.ToArray());
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

        private async ValueTask UpdatePushContentCluesAsync(CancellationToken cancellationToken = default)
        {
            var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

            foreach (var contentHash in await _publishedFileStorage.GetRootHashesAsync(cancellationToken))
            {
                var contentClue = HashToContentClue(contentHash);
                builder.Add(contentClue);
            }

            _pushContentClues = builder.ToImmutable();
        }

        private async ValueTask UpdateWantContentCluesAsync(CancellationToken cancellationToken = default)
        {
            var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

            foreach (var contentHash in await _subscribedFileStorage.GetRootHashesAsync(cancellationToken))
            {
                var contentClue = HashToContentClue(contentHash);
                builder.Add(contentClue);
            }

            _wantContentClues = builder.ToImmutable();
        }

        private static ContentClue HashToContentClue(OmniHash hash)
        {
            return new ContentClue(ServiceName, hash);
        }
    }
}
