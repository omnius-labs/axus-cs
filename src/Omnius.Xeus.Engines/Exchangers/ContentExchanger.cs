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
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Connections.Extensions;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Engines.Internal;
using Omnius.Xeus.Engines.Exchangers.Internal.Models;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Engines.Engines
{
    public sealed class ContentExchanger : AsyncDisposableBase, IContentExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ContentExchangerOptions _options;
        private readonly List<IConnector> _connectors = new List<IConnector>();
        private readonly ICkadMediator _nodeFinder;
        private readonly IPushContentStorage _pushStorage;
        private readonly IWantContentStorage _wantStorage;
        private readonly IBytesPool _bytesPool;

        private readonly HashSet<ConnectionStatus> _connections = new HashSet<ConnectionStatus>();

        private Task? _connectLoopTask;
        private Task? _acceptLoopTask;
        private Task? _sendLoopTask;
        private Task? _receiveLoopTask;
        private Task? _computeLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly object _lockObject = new object();

        internal sealed class ContentExchangerFactory : IContentExchangerFactory
        {
            public async ValueTask<IContentExchanger> CreateAsync(ContentExchangerOptions options, IEnumerable<IConnector> connectors,
                ICkadMediator nodeFinder, IPushContentStorage pushStorage, IWantContentStorage wantStorage, IBytesPool bytesPool)
            {
                var result = new ContentExchanger(options, connectors, nodeFinder, pushStorage, wantStorage, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public string EngineName => "content-exchanger";

        public static IContentExchangerFactory Factory { get; } = new ContentExchangerFactory();

        internal ContentExchanger(ContentExchangerOptions options, IEnumerable<IConnector> connectors,
                ICkadMediator nodeFinder, IPushContentStorage pushStorage, IWantContentStorage wantStorage, IBytesPool bytesPool)
        {
            _options = options;
            _connectors.AddRange(connectors);
            _nodeFinder = nodeFinder;
            _pushStorage = pushStorage;
            _wantStorage = wantStorage;
            _bytesPool = bytesPool;
        }

        public async ValueTask InitAsync()
        {
            _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
            _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
            _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
            _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);

            _nodeFinder.GetPushFetchResourceTags += (append) =>
            {
                // TODO
            };
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_connectLoopTask!, _acceptLoopTask!, _sendLoopTask!, _receiveLoopTask!, _computeLoopTask!);

            _cancellationTokenSource.Dispose();
        }

        private readonly VolatileHashSet<OmniAddress> _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromMinutes(30));

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
                        int connectionCount = _connections.Select(n => n.HandshakeType == ConnectionHandshakeType.Connected).Count();

                        if (_connections.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    foreach (var hash in await _wantStorage.GetContentHashesAsync(cancellationToken))
                    {
                        var tag = HashToResourceTag(hash);

                        NodeProfile? targetNodeProfile = null;
                        {
                            var nodeProfiles = await _nodeFinder.FindNodeProfilesAsync(tag, cancellationToken);
                            random.Shuffle(nodeProfiles);

                            lock (_lockObject)
                            {
                                var ignoreAddressSet = new HashSet<OmniAddress>();
                                ignoreAddressSet.Union(_connections.Select(n => n.Address));
                                ignoreAddressSet.Union(_connectedAddressSet);

                                foreach (var nodeProfile in nodeProfiles)
                                {
                                    if (nodeProfile.Addresses.Any(n => ignoreAddressSet.Contains(n))) continue;
                                    targetNodeProfile = nodeProfile;
                                    break;
                                }
                            }
                        }

                        if (targetNodeProfile == null) continue;

                        foreach (var targetAddress in targetNodeProfile.Addresses)
                        {
                            IConnection? connection = null;

                            foreach (var connector in _connectors)
                            {
                                connection = await connector.ConnectAsync(targetAddress, this.EngineName, cancellationToken);
                                if (connection == null) continue;
                            }

                            if (connection == null) continue;

                            _connectedAddressSet.Add(targetAddress);

                            if (await this.TryAddConnectionAsync(connection, targetAddress, ConnectionHandshakeType.Connected, hash, cancellationToken))
                            {
                                break;
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
                        int connectionCount = _connections.Select(n => n.HandshakeType == ConnectionHandshakeType.Accepted).Count();

                        if (_connections.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    foreach (var connector in _connectors)
                    {
                        var result = await connector.AcceptAsync(this.EngineName, cancellationToken);
                        if (result.Connection != null && result.Address != null)
                        {
                            await this.TryAddConnectionAsync(result.Connection, result.Address, ConnectionHandshakeType.Accepted, null, cancellationToken);
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
                    if (otherHelloMessage == null) throw new Exception();

                    version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
                }

                if (version == ContentExchangerVersion.Version1)
                {
                    if (handshakeType == ConnectionHandshakeType.Connected)
                    {
                        if (rootHash is null) throw new Exception();

                        var requestExchangeMessage = new ContentExchangerRequestExchangeMessage(rootHash.Value);

                        await connection.EnqueueAsync(requestExchangeMessage, cancellationToken);
                        var requestExchangeResultMessage = await connection.DequeueAsync<ContentExchangerRequestExchangeResultMessage>(cancellationToken);

                        if (requestExchangeResultMessage == null || requestExchangeResultMessage.Type != ContentExchangerRequestExchangeResultType.Accepted) throw new Exception();
                    }
                    else if (handshakeType == ConnectionHandshakeType.Accepted)
                    {
                        var requestExchangeMessage = await connection.DequeueAsync<ContentExchangerRequestExchangeMessage>(cancellationToken);
                        if (requestExchangeMessage == null) throw new Exception();

                        rootHash = requestExchangeMessage.Hash;

                        bool accepted = await _pushStorage.ContainsContentAsync(rootHash.Value) || await _wantStorage.ContainsContentAsync(rootHash.Value);
                        var requestExchangeResultMessage = new ContentExchangerRequestExchangeResultMessage(accepted ? ContentExchangerRequestExchangeResultType.Accepted : ContentExchangerRequestExchangeResultType.Rejected);
                        await connection.EnqueueAsync(requestExchangeResultMessage, cancellationToken);

                        if (!accepted) throw new Exception();
                    }
                    else
                    {
                        throw new NotSupportedException();
                    }

                    lock (_lockObject)
                    {
                        var status = new ConnectionStatus(connection, address, handshakeType, rootHash.Value);
                        _connections.Add(status);
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
                _logger.Error(e);

                connection.Dispose();
            }

            return false;
        }

        private async Task SendLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    foreach (var status in _connections.ToArray())
                    {
                        try
                        {
                            lock (status.LockObject)
                            {
                                if (status.SendingDataMessage != null)
                                {
                                    if (status.Connection.TryEnqueue(status.SendingDataMessage))
                                    {
                                        status.SendingDataMessage = null;
                                    }
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            lock (_lockObject)
                            {
                                _connections.Remove(status);
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
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);

                    foreach (var status in _connections.ToArray())
                    {
                        try
                        {
                            if (status.Connection.TryDequeue<ContentExchangerDataMessage>(out var dataMessage))
                            {
                                lock (status.LockObject)
                                {
                                    status.ReceivedOwnedContentBlockFlags = dataMessage.OwnedContentBlockFlags.ToArray();
                                    status.ReceivedWantContentBlockHashes = dataMessage.WantContentBlockHashes.ToArray();
                                }

                                foreach (var contentBlock in dataMessage.GiveContentBlocks)
                                {
                                    await _wantStorage.WriteBlockAsync(status.RootHash, contentBlock.Hash, contentBlock.Value, cancellationToken);
                                }
                            }
                        }
                        catch (ObjectDisposedException)
                        {
                            lock (_lockObject)
                            {
                                _connections.Remove(status);
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
            }
        }

        private async Task ComputeLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    lock (_lockObject)
                    {
                        foreach (var connectionStatus in _connections)
                        {
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
            }
        }

        private ResourceTag HashToResourceTag(OmniHash hash)
        {
            return new ResourceTag(this.EngineName, hash);
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

            public ContentExchangerDataMessage? SendingDataMessage { get; set; } = null;
            public ContentBlockFlags[]? ReceivedOwnedContentBlockFlags { get; set; } = null;
            public OmniHash[]? ReceivedWantContentBlockHashes { get; set; } = null;
        }
    }
}
