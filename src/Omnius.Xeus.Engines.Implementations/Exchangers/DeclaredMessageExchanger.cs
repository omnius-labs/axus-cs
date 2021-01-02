using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Extensions;
using Omnius.Core.Helpers;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Connections.Extensions;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Engines.Connectors.Primitives;
using Omnius.Xeus.Engines.Engines.Internal;
using Omnius.Xeus.Engines.Exchangers.Internal.Models;
using Omnius.Xeus.Engines.Mediators;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Engines.Storages;

namespace Omnius.Xeus.Engines.Exchangers
{
    public sealed class DeclaredMessageExchanger : AsyncDisposableBase, IDeclaredMessageExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly DeclaredMessageExchangerOptions _options;
        private readonly List<IConnector> _connectors = new();
        private readonly ICkadMediator _nodeFinder;
        private readonly IPushDeclaredMessageStorage _pushStorage;
        private readonly IWantDeclaredMessageStorage _wantStorage;
        private readonly IBytesPool _bytesPool;

        private readonly HashSet<ConnectionStatus> _connectionStatusSet = new();

        private readonly HashSet<ResourceTag> _resourceTags = new();
        private readonly object _resourceTagsLockObject = new();

        private Task _connectLoopTask = null!;
        private Task _acceptLoopTask = null!;
        private Task _computeLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly object _lockObject = new();

        internal sealed class DeclaredMessageExchangerFactory : IDeclaredMessageExchangerFactory
        {
            public async ValueTask<IDeclaredMessageExchanger> CreateAsync(DeclaredMessageExchangerOptions options, IEnumerable<IConnector> connectors, ICkadMediator nodeFinder, IPushDeclaredMessageStorage pushStorage, IWantDeclaredMessageStorage wantStorage, IBytesPool bytesPool)
            {
                var result = new DeclaredMessageExchanger(options, connectors, nodeFinder, pushStorage, wantStorage, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        private const string EngineName = "declared-message-exchanger";

        public static IDeclaredMessageExchangerFactory Factory { get; } = new DeclaredMessageExchangerFactory();

        internal DeclaredMessageExchanger(DeclaredMessageExchangerOptions options, IEnumerable<IConnector> connectors, ICkadMediator nodeFinder, IPushDeclaredMessageStorage pushStorage, IWantDeclaredMessageStorage wantStorage, IBytesPool bytesPool)
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
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);

            _nodeFinder.GetPushResourceTags += (append) =>
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
            await Task.WhenAll(_connectLoopTask!, _acceptLoopTask!);
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
                        int connectionCount = _connectionStatusSet.Select(n => n.HandshakeType == ConnectionHandshakeType.Connected).Count();

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2))
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

                        if (targetNodeProfile == null)
                        {
                            continue;
                        }

                        foreach (var targetAddress in targetNodeProfile.Addresses)
                        {
                            foreach (var connector in _connectors)
                            {
                                var connection = await connector.ConnectAsync(targetAddress, EngineName, cancellationToken);
                                if (connection is null)
                                {
                                    continue;
                                }

                                _connectedAddressSet.Add(targetAddress);

                                if (await this.TryAddConnectionAsync(connection, targetAddress, ConnectionHandshakeType.Connected, signature, cancellationToken))
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

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2))
                        {
                            continue;
                        }
                    }

                    foreach (var connector in _connectors)
                    {
                        var result = await connector.AcceptAsync(EngineName, cancellationToken);
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
            foreach (var signature in await _wantStorage.GetSignaturesAsync(cancellationToken))
            {
                var tag = SignatureToResourceTag(signature);
                resourceTags.Add(tag);
            }

            lock (_resourceTagsLockObject)
            {
                _resourceTags.Clear();
                _resourceTags.UnionWith(resourceTags);
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
                    if (otherHelloMessage == null)
                    {
                        throw new DeclaredMessageExchangerException();
                    }

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
                _logger.Warn(e);

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
                _connectionStatusSet.Add(status);
            }

            try
            {
                if (handshakeType == ConnectionHandshakeType.Connected)
                {
                    if (signature == null)
                    {
                        throw new ArgumentNullException(nameof(signature));
                    }

                    status.Signature = signature;

                    var messageCreationTime = await this.ReadMessageCreationTimeAsync(signature, cancellationToken);
                    if (messageCreationTime == null)
                    {
                        messageCreationTime = DateTime.MinValue;
                    }

                    var fetchMessage = new DeclaredMessageExchangerFetchMessage(signature, Timestamp.FromDateTime(messageCreationTime.Value));
                    await connection.EnqueueAsync(fetchMessage, cancellationToken);

                    var fetchResultMessage = await connection.DequeueAsync<DeclaredMessageExchangerFetchResultMessage>(cancellationToken);
                    if (fetchResultMessage == null)
                    {
                        throw new DeclaredMessageExchangerException();
                    }

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
                        if (message is null)
                        {
                            throw new DeclaredMessageExchangerException();
                        }

                        var postMessage = new DeclaredMessageExchangerPostMessage(message);
                        await connection.EnqueueAsync(postMessage, cancellationToken: cancellationToken);
                    }
                }
                else if (handshakeType == ConnectionHandshakeType.Accepted)
                {
                    var fetchMessage = await connection.DequeueAsync<DeclaredMessageExchangerFetchMessage>(cancellationToken);
                    if (fetchMessage == null)
                    {
                        throw new DeclaredMessageExchangerException();
                    }

                    signature = fetchMessage.Signature;
                    status.Signature = signature;

                    var messageCreationTime = await this.ReadMessageCreationTimeAsync(fetchMessage.Signature, cancellationToken);
                    if (messageCreationTime == null)
                    {
                        messageCreationTime = DateTime.MinValue;
                    }

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
                            await _wantStorage.WriteMessageAsync(postMessage.DeclaredMessage, cancellationToken);
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
            finally
            {
                connection.Dispose();

                lock (_lockObject)
                {
                    _connectionStatusSet.Remove(status);
                }
            }
        }

        private async ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken)
        {
            var wantStorageCreationTime = await _wantStorage.ReadMessageCreationTimeAsync(signature, cancellationToken);
            var pushStorageCreationTime = await _pushStorage.ReadMessageCreationTimeAsync(signature, cancellationToken);
            if (wantStorageCreationTime is null && pushStorageCreationTime is null)
            {
                return null;
            }

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
            if (wantStorageCreationTime is null && pushStorageCreationTime is null)
            {
                return null;
            }

            if ((wantStorageCreationTime ?? DateTime.MinValue) < (pushStorageCreationTime ?? DateTime.MinValue))
            {
                return await _pushStorage.ReadMessageAsync(signature, cancellationToken);
            }
            else
            {
                return await _wantStorage.ReadMessageAsync(signature, cancellationToken);
            }
        }

        private static ResourceTag SignatureToResourceTag(OmniSignature signature)
        {
            using var hub = new BytesHub();
            signature.Export(hub.Writer, BytesPool.Shared);
            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(hub.Reader.GetSequence()));
            return new ResourceTag(hash, EngineName);
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

            public OmniSignature? Signature { get; set; }
        }
    }

    public sealed class DeclaredMessageExchangerException : Exception
    {
        public DeclaredMessageExchangerException()
            : base()
        {
        }

        public DeclaredMessageExchangerException(string message)
            : base(message)
        {
        }

        public DeclaredMessageExchangerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
