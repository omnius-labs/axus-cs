using System;
using System.Collections.Generic;
using System.Data;
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
using Omnius.Core.RocketPack;
using Omnius.Xeus.Components.Connectors;
using Omnius.Xeus.Components.Engines.Internal;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Components.Storages;

namespace Omnius.Xeus.Components.Engines
{
    public sealed class DeclaredMessageExchanger : AsyncDisposableBase, IDeclaredMessageExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly DeclaredMessageExchangerOptions _options;
        private readonly List<IConnector> _connectors = new List<IConnector>();
        private readonly INodeFinder _nodeFinder;
        private readonly IPushDeclaredMessageStorage _pushStorage;
        private readonly IWantDeclaredMessageStorage _wantStorage;
        private readonly IBytesPool _bytesPool;

        private readonly HashSet<ConnectionStatus> _connections = new HashSet<ConnectionStatus>();

        private Task? _connectLoopTask;
        private Task? _acceptLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly object _lockObject = new object();

        internal sealed class DeclaredMessageExchangerFactory : IDeclaredMessageExchangerFactory
        {
            public async ValueTask<IDeclaredMessageExchanger> CreateAsync(DeclaredMessageExchangerOptions options,
                IEnumerable<IConnector> connectors, INodeFinder nodeFinder, IPushDeclaredMessageStorage pushStorage, IWantDeclaredMessageStorage wantStorage, IBytesPool bytesPool)
            {
                var result = new DeclaredMessageExchanger(options, connectors, nodeFinder, pushStorage, wantStorage, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public string EngineName => "declared-message-exchanger";

        public static IDeclaredMessageExchangerFactory Factory { get; } = new DeclaredMessageExchangerFactory();

        internal DeclaredMessageExchanger(DeclaredMessageExchangerOptions options,
                IEnumerable<IConnector> connectors, INodeFinder nodeFinder, IPushDeclaredMessageStorage pushStorage, IWantDeclaredMessageStorage wantStorage, IBytesPool bytesPool)
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
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_connectLoopTask!, _acceptLoopTask!);

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

                    foreach (var signature in await _wantStorage.GetSignaturesAsync(cancellationToken))
                    {
                        var tag = SignatureToResourceTag(signature);

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

                            if (await this.TryAddConnectionAsync(connection, targetAddress, ConnectionHandshakeType.Connected, signature, cancellationToken))
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
            ConnectionHandshakeType handshakeType, OmniSignature? signature, CancellationToken cancellationToken = default)
        {
            try
            {
                DeclaredMessageExchangerVersion? version = 0;

                // バージョン情報の交換
                {
                    var myHelloMessage = new DeclaredMessageExchangerHelloMessage(new[] { DeclaredMessageExchangerVersion.Version1 });

                    var enqueueTask = connection.EnqueueAsync(myHelloMessage, cancellationToken).AsTask();
                    var dequeueTask = connection.DequeueAsync<DeclaredMessageExchangerHelloMessage>(cancellationToken).AsTask();
                    await Task.WhenAll(enqueueTask, dequeueTask);

                    var otherHelloMessage = dequeueTask.Result;
                    if (otherHelloMessage == null) throw new Exception();

                    version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
                }

                if (version == DeclaredMessageExchangerVersion.Version1)
                {
                    _ = this.ExchangeAsync(connection, address, handshakeType, signature, cancellationToken);

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

        private async Task ExchangeAsync(IConnection connection, OmniAddress address,
            ConnectionHandshakeType handshakeType, OmniSignature? signature, CancellationToken cancellationToken = default)
        {
            var status = new ConnectionStatus(connection, address, handshakeType);

            lock (_lockObject)
            {
                _connections.Add(status);
            }

            try
            {
                if (handshakeType == ConnectionHandshakeType.Connected)
                {
                    if (signature == null) throw new ArgumentNullException(nameof(signature));

                    status.Signature = signature;

                    var messageCreationTime = await this.ReadMessageCreationTimeAsync(signature, cancellationToken);
                    if (messageCreationTime == null) messageCreationTime = DateTime.MinValue;

                    var fetchMessage = new DeclaredMessageExchangerFetchMessage(signature, Timestamp.FromDateTime(messageCreationTime.Value));
                    await connection.EnqueueAsync(fetchMessage, cancellationToken);

                    var fetchResultMessage = await connection.DequeueAsync<DeclaredMessageExchangerFetchResultMessage>(cancellationToken);
                    if (fetchResultMessage == null) throw new Exception();

                    if (fetchResultMessage.Type == DeclaredMessageExchangerFetchResultType.Found)
                    {
                        if (fetchResultMessage.DeclaredMessage != null)
                        {
                            await _wantStorage.WriteMessageAsync(fetchResultMessage.DeclaredMessage, cancellationToken);
                        }
                    }
                    else if (fetchResultMessage.Type == DeclaredMessageExchangerFetchResultType.NotFound)
                    {
                        var message = await this.ReadMessageAsync(signature, cancellationToken);
                        if (message is null) throw new Exception();

                        var postMessage = new DeclaredMessageExchangerPostMessage(message);
                        await connection.EnqueueAsync(postMessage);
                    }
                }
                else if (handshakeType == ConnectionHandshakeType.Accepted)
                {
                    var fetchMessage = await connection.DequeueAsync<DeclaredMessageExchangerFetchMessage>(cancellationToken);
                    if (fetchMessage == null) throw new Exception();

                    signature = fetchMessage.Signature;
                    status.Signature = signature;

                    var messageCreationTime = await this.ReadMessageCreationTimeAsync(fetchMessage.Signature, cancellationToken);
                    if (messageCreationTime == null) messageCreationTime = DateTime.MinValue;

                    if (fetchMessage.CreationTime.ToDateTime() == messageCreationTime.Value)
                    {
                        var fetchResultMessage = new DeclaredMessageExchangerFetchResultMessage(DeclaredMessageExchangerFetchResultType.Same, null);
                        await connection.EnqueueAsync(fetchResultMessage, cancellationToken);
                    }
                    else if (fetchMessage.CreationTime.ToDateTime() < messageCreationTime.Value)
                    {
                        var message = await this.ReadMessageAsync(fetchMessage.Signature, cancellationToken);
                        var fetchResultMessage = new DeclaredMessageExchangerFetchResultMessage(DeclaredMessageExchangerFetchResultType.Found, message);
                        await connection.EnqueueAsync(fetchResultMessage, cancellationToken);
                    }
                    else
                    {
                        var fetchResultMessage = new DeclaredMessageExchangerFetchResultMessage(DeclaredMessageExchangerFetchResultType.NotFound, null);
                        await connection.EnqueueAsync(fetchResultMessage, cancellationToken);

                        var postMessage = await connection.DequeueAsync<DeclaredMessageExchangerPostMessage>(cancellationToken);
                        if (postMessage.DeclaredMessage != null)
                        {
                            await _wantStorage.WriteMessageAsync(postMessage.DeclaredMessage);
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
            finally
            {
                connection.Dispose();

                lock (_lockObject)
                {
                    _connections.Remove(status);
                }
            }
        }

        private async ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken)
        {
            var wantStorageCreationTime = await _wantStorage.ReadMessageCreationTimeAsync(signature, cancellationToken);
            var pushStorageCreationTime = await _pushStorage.ReadMessageCreationTimeAsync(signature, cancellationToken);
            if (wantStorageCreationTime is null && pushStorageCreationTime is null) return null;

            if ((wantStorageCreationTime ?? DateTime.MinValue) < (pushStorageCreationTime ?? DateTime.MinValue))
            {
                return pushStorageCreationTime;
            }
            else
            {
                return wantStorageCreationTime;
            }
        }

        public async ValueTask<DeclaredMessage?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken)
        {
            var wantStorageCreationTime = await _wantStorage.ReadMessageCreationTimeAsync(signature, cancellationToken);
            var pushStorageCreationTime = await _pushStorage.ReadMessageCreationTimeAsync(signature, cancellationToken);
            if (wantStorageCreationTime is null && pushStorageCreationTime is null) return null;

            if ((wantStorageCreationTime ?? DateTime.MinValue) < (pushStorageCreationTime ?? DateTime.MinValue))
            {
                return await _pushStorage.ReadMessageAsync(signature, cancellationToken);
            }
            else
            {
                return await _wantStorage.ReadMessageAsync(signature, cancellationToken);
            }
        }

        private ResourceTag SignatureToResourceTag(OmniSignature signature)
        {
            using var hub = new BytesHub();
            signature.Export(hub.Writer, BytesPool.Shared);
            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(hub.Reader.GetSequence()));
            return new ResourceTag(this.EngineName, hash);
        }

        private enum ConnectionHandshakeType
        {
            Connected,
            Accepted,
        }

        private sealed class ConnectionStatus
        {
            public ConnectionStatus(IConnection connection, OmniAddress address, ConnectionHandshakeType handshakeType)
            {
                this.Connection = connection;
                this.Address = address;
                this.HandshakeType = handshakeType;
            }

            public IConnection Connection { get; }
            public OmniAddress Address { get; }
            public ConnectionHandshakeType HandshakeType { get; }

            public OmniSignature Signature { get; set; }
        }
    }
}
