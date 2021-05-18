using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Extensions;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Extensions;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Engines.Internal;
using Omnius.Xeus.Engines.Exchangers.Internal.Models;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages;
using Omnius.Xeus.Engines.Storages.Primitives;

namespace Omnius.Xeus.Engines.Exchangers
{
    public sealed class ContentExchanger : AsyncDisposableBase, IContentExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ContentExchangerOptions _options;
        private readonly List<IConnector> _connectors = new();
        private readonly ICkadMediator _ckadMediator;
        private readonly IContentPublisher _publisher;
        private readonly IContentSubscriber _subscriber;
        private readonly IBytesPool _bytesPool;

        private readonly HashSet<ConnectionStatus> _connectionStatusSet = new();

        private readonly HashSet<ResourceTag> _resourceTags = new();
        private readonly object _resourceTagsLockObject = new();

        private Task _connectLoopTask = null!;
        private Task _acceptLoopTask = null!;
        private Task _sendLoopTask = null!;
        private Task _receiveLoopTask = null!;
        private Task _computeLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly object _lockObject = new();

        private const string EngineName = "content-exchanger";

        internal sealed class ContentExchangerFactory : IContentExchangerFactory
        {
            public async ValueTask<IContentExchanger> CreateAsync(ContentExchangerOptions options, IEnumerable<IConnector> connectors,
                ICkadMediator nodeFinder, IContentPublisher pushStorage, IContentSubscriber wantStorage, IBytesPool bytesPool, CancellationToken cancellationToken = default)
            {
                var result = new ContentExchanger(options, connectors, nodeFinder, pushStorage, wantStorage, bytesPool);
                await result.InitAsync(cancellationToken);

                return result;
            }
        }

        public static IContentExchangerFactory Factory { get; } = new ContentExchangerFactory();

        internal ContentExchanger(ContentExchangerOptions options, IEnumerable<IConnector> connectors,
            ICkadMediator nodeFinder, IContentPublisher pushStorage, IContentSubscriber wantStorage, IBytesPool bytesPool)
        {
            _options = options;
            _connectors.AddRange(connectors);
            _ckadMediator = nodeFinder;
            _publisher = pushStorage;
            _subscriber = wantStorage;
            _bytesPool = bytesPool;
        }

        public async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
            _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
            _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
            _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);

            _ckadMediator.GetPublishResourceTags += (append) =>
            {
                ResourceTag[] resourceTags;

                lock (_resourceTagsLockObject)
                {
                    resourceTags = _resourceTags.ToArray();
                }

                foreach (var resourceTag in resourceTags)
                {
                    append.Invoke(resourceTag);
                }
            };
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_connectLoopTask, _acceptLoopTask, _sendLoopTask, _receiveLoopTask, _computeLoopTask);
            _cancellationTokenSource.Dispose();
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

                    lock (_lockObject)
                    {
                        _connectedAddressSet.Refresh();

                        int connectionCount = _connectionStatusSet.Select(n => n.HandshakeType == ConnectionHandshakeType.Connected).Count();

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2)) continue;
                    }

                    foreach (var hash in await _subscriber.GetRootHashesAsync(cancellationToken))
                    {
                        var tag = HashToResourceTag(hash);

                        NodeProfile? targetNodeProfile = null;
                        {
                            var nodeProfiles = await _ckadMediator.FindNodeProfilesAsync(tag, cancellationToken);
                            random.Shuffle(nodeProfiles);

                            var ignoreAddressSet = new HashSet<OmniAddress>();
                            lock (_lockObject)
                            {
                                ignoreAddressSet.UnionWith(_connectionStatusSet.Select(n => n.Address));
                                ignoreAddressSet.UnionWith(_connectedAddressSet);
                            }

                            targetNodeProfile = nodeProfiles
                                .Where(n => !n.Addresses.Any(n => ignoreAddressSet.Contains(n)))
                                .FirstOrDefault();
                        }

                        if (targetNodeProfile == null) continue;

                        foreach (var targetAddress in targetNodeProfile.Addresses)
                        {
                            foreach (var connector in _connectors)
                            {
                                var connection = await connector.ConnectAsync(targetAddress, EngineName, cancellationToken);
                                if (connection is null) continue;

                                _connectedAddressSet.Add(targetAddress);

                                if (await this.TryAddConnectionAsync(connection, targetAddress, ConnectionHandshakeType.Connected, hash, cancellationToken))
                                {
                                    goto End;
                                }
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
                        int connectionCount = _connectionStatusSet.Select(n => n.HandshakeType == ConnectionHandshakeType.Accepted).Count();

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2)) continue;
                    }

                    foreach (var connector in _connectors)
                    {
                        var result = await connector.AcceptAsync(EngineName, cancellationToken);
                        if (result.Connection is null || result.Address is null) continue;

                        await this.TryAddConnectionAsync(result.Connection, result.Address, ConnectionHandshakeType.Accepted, null, cancellationToken);
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

        private async ValueTask<bool> TryAddConnectionAsync(IConnection connection, OmniAddress address,
            ConnectionHandshakeType handshakeType, OmniHash? rootHash, CancellationToken cancellationToken = default)
        {
            try
            {
                ContentExchangerVersion? version = 0;

                // バージョン情報の交換
                {
                    var myHelloMessage = new ContentExchangerHelloMessage(new[] { ContentExchangerVersion.Version1 });

                    var enqueueTask = connection.EnqueueAsync(myHelloMessage, cancellationToken).AsTask();
                    var dequeueTask = connection.DequeueAsync<ContentExchangerHelloMessage>(cancellationToken).AsTask();
                    await Task.WhenAll(enqueueTask, dequeueTask);

                    var otherHelloMessage = dequeueTask.Result;
                    if (otherHelloMessage == null) throw new ContentExchangerException();

                    version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
                }

                if (version == ContentExchangerVersion.Version1)
                {
                    if (handshakeType == ConnectionHandshakeType.Connected)
                    {
                        if (rootHash is null) throw new ArgumentNullException(nameof(rootHash));

                        var requestExchangeMessage = new ContentExchangerHandshakeRequestMessage(rootHash.Value);

                        await connection.EnqueueAsync(requestExchangeMessage, cancellationToken);
                        var requestExchangeResultMessage = await connection.DequeueAsync<ContentExchangerHandshakeResultMessage>(cancellationToken);

                        if (requestExchangeResultMessage == null || requestExchangeResultMessage.Type != ContentExchangerHandshakeResultType.Accepted) throw new ContentExchangerException();
                    }
                    else if (handshakeType == ConnectionHandshakeType.Accepted)
                    {
                        var requestExchangeMessage = await connection.DequeueAsync<ContentExchangerHandshakeRequestMessage>(cancellationToken);
                        if (requestExchangeMessage == null) throw new ContentExchangerException();

                        rootHash = requestExchangeMessage.RootHash;

                        bool accepted = await _publisher.ContainsContentAsync(rootHash.Value, cancellationToken) || await _subscriber.ContainsContentAsync(rootHash.Value, cancellationToken);
                        var requestExchangeResultMessage = new ContentExchangerHandshakeResultMessage(accepted ? ContentExchangerHandshakeResultType.Accepted : ContentExchangerHandshakeResultType.Rejected);
                        await connection.EnqueueAsync(requestExchangeResultMessage, cancellationToken);

                        if (!accepted) throw new ContentExchangerException();
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    lock (_lockObject)
                    {
                        var status = new ConnectionStatus(connection, address, handshakeType, rootHash.Value);
                        _connectionStatusSet.Add(status);
                    }

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

                connection.Dispose();
            }
            catch (Exception e)
            {
                _logger.Warn(e);

                connection.Dispose();
            }

            return false;
        }

        private async Task SendLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                while (cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1, cancellationToken);

                    ConnectionStatus[] connectionStatuses;
                    lock (_lockObject)
                    {
                        connectionStatuses = _connectionStatusSet.ToArray();
                    }

                    foreach (var connectionStatus in connectionStatuses)
                    {
                        try
                        {
                            lock (connectionStatus.LockObject)
                            {
                                var dataMessage = connectionStatus.SendingDataMessage;
                                if (dataMessage is not null)
                                {
                                    if (connectionStatus.Connection.TryEnqueue(dataMessage))
                                    {
                                        foreach (var block in dataMessage.GiveBlocks)
                                        {
                                            block.Dispose();
                                        }

                                        connectionStatus.SendingDataMessage = null;
                                    }
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            lock (_lockObject)
                            {
                                _connectionStatusSet.Remove(connectionStatus);
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

                    ConnectionStatus[] connectionStatuses;
                    lock (_lockObject)
                    {
                        connectionStatuses = _connectionStatusSet.ToArray();
                    }

                    foreach (var connectionStatus in connectionStatuses)
                    {
                        try
                        {
                            if (connectionStatus.Connection.TryDequeue<ContentExchangerDataMessage>(out var dataMessage))
                            {
                                try
                                {
                                    lock (connectionStatus.LockObject)
                                    {
                                        connectionStatus.ReceivedWantBlockHashes = dataMessage.WantBlockHashes.ToArray();
                                    }

                                    foreach (var block in dataMessage.GiveBlocks)
                                    {
                                        await _subscriber.WriteBlockAsync(connectionStatus.RootHash, block.Hash, block.Value, cancellationToken);
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
                                _connectionStatusSet.Remove(connectionStatus);
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

                    await this.UpdateResourceTagsAsync(cancellationToken);

                    ConnectionStatus[] connectionStatuses;
                    lock (_lockObject)
                    {
                        connectionStatuses = _connectionStatusSet.ToArray();
                    }

                    foreach (var connectionStatus in connectionStatuses)
                    {
                        OmniHash[] wantBlockHashes;
                        {
                            wantBlockHashes = (await _subscriber.GetBlockHashesAsync(connectionStatus.RootHash, false, cancellationToken))
                                .Randomize()
                                .Take(ContentExchangerDataMessage.MaxWantBlockHashesCount)
                                .ToArray();
                        }

                        var giveBlocks = new List<Block>();
                        {
                            var receivedWantBlockHashSet = new HashSet<OmniHash>();
                            lock (connectionStatus.LockObject)
                            {
                                receivedWantBlockHashSet.UnionWith(connectionStatus.ReceivedWantBlockHashes ?? Array.Empty<OmniHash>());
                            }

                            foreach (var contentStorage in new IReadOnlyContents[] { _subscriber, _publisher })
                            {
                                foreach (var hash in (await contentStorage.GetBlockHashesAsync(connectionStatus.RootHash, true, cancellationToken)).Randomize())
                                {
                                    var memoryOwner = await contentStorage.ReadBlockAsync(connectionStatus.RootHash, hash, cancellationToken);
                                    if (memoryOwner is null) continue;

                                    giveBlocks.Add(new Block(hash, memoryOwner));
                                    if (giveBlocks.Count >= ContentExchangerDataMessage.MaxGiveBlocksCount)
                                    {
                                        goto End;
                                    }
                                }
                            }

                        End:;
                        }

                        lock (connectionStatus.LockObject)
                        {
                            connectionStatus.SendingDataMessage = new ContentExchangerDataMessage(wantBlockHashes, giveBlocks.ToArray());
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

        private async ValueTask UpdateResourceTagsAsync(CancellationToken cancellationToken = default)
        {
            var resourceTags = new List<ResourceTag>();
            foreach (var hash in await _subscriber.GetRootHashesAsync(cancellationToken))
            {
                var tag = HashToResourceTag(hash);
                resourceTags.Add(tag);
            }

            lock (_resourceTagsLockObject)
            {
                _resourceTags.Clear();
                _resourceTags.UnionWith(resourceTags);
            }
        }

        private static ResourceTag HashToResourceTag(OmniHash hash)
        {
            return new ResourceTag(hash, EngineName);
        }

        private enum ConnectionHandshakeType
        {
            Connected,
            Accepted,
        }

        private sealed class ConnectionStatus : ISynchronized
        {
            public ConnectionStatus(IConnection connection, OmniAddress address, ConnectionHandshakeType handshakeType, OmniHash rootHash)
            {
                this.Connection = connection;
                this.Address = address;
                this.HandshakeType = handshakeType;
                this.RootHash = rootHash;
            }

            public object LockObject { get; } = new object();

            public IConnection Connection { get; }

            public OmniAddress Address { get; }

            public ConnectionHandshakeType HandshakeType { get; }

            public OmniHash RootHash { get; }

            public ContentExchangerDataMessage? SendingDataMessage { get; set; }

            public OmniHash[]? ReceivedWantBlockHashes { get; set; }
        }
    }

    public sealed class ContentExchangerException : Exception
    {
        public ContentExchangerException()
            : base()
        {
        }

        public ContentExchangerException(string message)
            : base(message)
        {
        }

        public ContentExchangerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
