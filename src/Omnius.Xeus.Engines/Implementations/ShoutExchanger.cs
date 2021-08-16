using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.RocketPack;

namespace Omnius.Xeus.Engines.Exchangers
{
    public interface IShoutExchangerFactory
    {
        ValueTask<IShoutExchanger> CreateAsync(ShoutExchangerOptions options, CancellationToken cancellationToken = default);
    }

    public record ShoutExchangerOptions
    {
        public IReadOnlyCollection<IConnectionConnector>? Connectors { get; init; }

        public INodeFinder? NodeFinder { get; init; }

        public IPublishedShoutStorage? PublishedShoutStorage { get; init; }

        public ISubscribedShoutStorage? SubscribedShoutStorage { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public sealed class ShoutExchanger : AsyncDisposableBase, IShoutExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ShoutExchangerOptions _options;
        private readonly List<Connectors.Primitives.IConnectionConnector> _connectors = new();
        private readonly IMediator _ckadMediator;
        private readonly IPublishedShoutStorage _publisher;
        private readonly ISubscribedShoutStorage _subscriber;
        private readonly IBytesPool _bytesPool;

        private readonly HashSet<ConnectionStatus> _connectionStatusSet = new();

        private readonly HashSet<ContentLocation> _resourceTags = new();
        private readonly object _resourceTagsLockObject = new();

        private Task _connectLoopTask = null!;
        private Task _acceptLoopTask = null!;
        private Task _computeLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly object _lockObject = new();

        internal sealed class ShoutExchangerFactory : IShoutExchangerFactory
        {
            public async ValueTask<IShoutExchanger> CreateAsync(ShoutExchangerOptions options, IEnumerable<Connectors.Primitives.IConnectionConnector> connectors,
                IMediator nodeFinder, IPublishedShoutStorage pushStorage, ISubscribedShoutStorage wantStorage, IBytesPool bytesPool, CancellationToken cancellationToken = default)
            {
                var result = new ShoutExchanger(options, connectors, nodeFinder, pushStorage, wantStorage, bytesPool);
                await result.InitAsync(cancellationToken);

                return result;
            }
        }

        private const string EngineName = "declared-message-exchanger";

        public static IShoutExchangerFactory Factory { get; } = new ShoutExchangerFactory();

        internal ShoutExchanger(ShoutExchangerOptions options, IEnumerable<Connectors.Primitives.IConnectionConnector> connectors,
            IMediator nodeFinder, IPublishedShoutStorage pushStorage, ISubscribedShoutStorage wantStorage, IBytesPool bytesPool, CancellationToken cancellationToken = default)
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
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);

            _ckadMediator.GetPublishResourceTags += (append) =>
            {
                ContentLocation[] resourceTags;

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

        public async ValueTask<ShoutExchangerReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            var connectionReports = new List<ConnectionReport>();

            foreach (var status in _connectionStatusSet)
            {
                connectionReports.Add(new ConnectionReport(status.HandshakeType, status.Address));
            }

            return new ShoutExchangerReport(0, 0, connectionReports.ToArray());
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

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2)) continue;
                    }

                    foreach (var signature in await _subscriber.GetSignaturesAsync(cancellationToken))
                    {
                        var tag = SignatureToResourceTag(signature);

                        NodeLocation? targetNodeLocation = null;
                        {
                            var nodeLocations = await _ckadMediator.FindNodeLocationsAsync(tag, cancellationToken);
                            random.Shuffle(nodeLocations);

                            var ignoreAddressSet = new HashSet<OmniAddress>();
                            lock (_lockObject)
                            {
                                ignoreAddressSet.UnionWith(_connectionStatusSet.Select(n => n.Address));
                                ignoreAddressSet.UnionWith(_connectedAddressSet);
                            }

                            targetNodeLocation = nodeLocations
                                .Where(n => !n.Addresses.Any(n => ignoreAddressSet.Contains(n)))
                                .FirstOrDefault();
                        }

                        if (targetNodeLocation == null) continue;

                        foreach (var targetAddress in targetNodeLocation.Addresses)
                        {
                            foreach (var connector in _connectors)
                            {
                                var connection = await connector.ConnectAsync(targetAddress, EngineName, cancellationToken);
                                if (connection is null) continue;

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

                        if (_connectionStatusSet.Count > (_options.MaxConnectionCount / 2)) continue;
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
            var resourceTags = new List<ContentLocation>();
            foreach (var signature in await _subscriber.GetSignaturesAsync(cancellationToken))
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
                ShoutExchangerVersion? version = 0;

                // バージョン情報の交換
                {
                    var myHelloMessage = new ShoutExchangerHelloMessage(new[] { ShoutExchangerVersion.Version1 });

                    var enqueueTask = connection.EnqueueAsync(myHelloMessage, cancellationToken).AsTask();
                    var dequeueTask = connection.DequeueAsync<ShoutExchangerHelloMessage>(cancellationToken).AsTask();
                    await Task.WhenAll(enqueueTask, dequeueTask);

                    var otherHelloMessage = dequeueTask.Result;
                    if (otherHelloMessage == null) throw new ShoutExchangerException();

                    version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
                }

                if (version == ShoutExchangerVersion.Version1)
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
                    if (signature == null) throw new ArgumentNullException(nameof(signature));

                    status.Signature = signature;

                    var messageCreationTime = await this.ReadMessageCreationTimeAsync(signature, cancellationToken);
                    if (messageCreationTime == null)
                    {
                        messageCreationTime = DateTime.MinValue;
                    }

                    var fetchMessage = new ShoutExchangerFetchRequestMessage(signature, Timestamp.FromDateTime(messageCreationTime.Value));
                    await connection.EnqueueAsync(fetchMessage, cancellationToken);

                    var fetchResultMessage = await connection.DequeueAsync<ShoutExchangerFetchResultMessage>(cancellationToken);
                    if (fetchResultMessage == null) throw new ShoutExchangerException();

                    if (fetchResultMessage.Type == ShoutExchangerFetchResultType.Found)
                    {
                        if (fetchResultMessage.Shout != null)
                        {
                            await _subscriber.WriteMessageAsync(fetchResultMessage.Shout, cancellationToken);
                        }
                    }
                    else if (fetchResultMessage.Type == ShoutExchangerFetchResultType.NotFound)
                    {
                        var message = await this.ReadMessageAsync(signature, cancellationToken);
                        if (message is null) throw new ShoutExchangerException();

                        var postMessage = new ShoutExchangerPostMessage(message);
                        await connection.EnqueueAsync(postMessage, cancellationToken: cancellationToken);
                    }
                }
                else if (handshakeType == ConnectionHandshakeType.Accepted)
                {
                    var fetchMessage = await connection.DequeueAsync<ShoutExchangerFetchRequestMessage>(cancellationToken);
                    if (fetchMessage == null) throw new ShoutExchangerException();

                    signature = fetchMessage.Signature;
                    status.Signature = signature;

                    var messageCreationTime = await this.ReadMessageCreationTimeAsync(fetchMessage.Signature, cancellationToken);
                    if (messageCreationTime == null)
                    {
                        messageCreationTime = DateTime.MinValue;
                    }

                    if (fetchMessage.CreationTime.ToDateTime() == messageCreationTime.Value)
                    {
                        var fetchResultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.Same, null);
                        await connection.EnqueueAsync(fetchResultMessage, cancellationToken);
                    }
                    else if (fetchMessage.CreationTime.ToDateTime() < messageCreationTime.Value)
                    {
                        var message = await this.ReadMessageAsync(fetchMessage.Signature, cancellationToken);
                        var fetchResultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.Found, message);
                        await connection.EnqueueAsync(fetchResultMessage, cancellationToken);
                    }
                    else
                    {
                        var fetchResultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.NotFound, null);
                        await connection.EnqueueAsync(fetchResultMessage, cancellationToken);

                        var postMessage = await connection.DequeueAsync<ShoutExchangerPostMessage>(cancellationToken);
                        if (postMessage.Shout != null)
                        {
                            await _subscriber.WriteMessageAsync(postMessage.Shout, cancellationToken);
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
            var wantStorageCreationTime = await _subscriber.ReadShoutCreationTimeAsync(signature, cancellationToken);
            var pushStorageCreationTime = await _publisher.ReadShoutCreationTimeAsync(signature, cancellationToken);
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

        public async ValueTask<Shout?> ReadMessageAsync(OmniSignature signature, CancellationToken cancellationToken)
        {
            var wantStorageCreationTime = await _subscriber.ReadShoutCreationTimeAsync(signature, cancellationToken);
            var pushStorageCreationTime = await _publisher.ReadShoutCreationTimeAsync(signature, cancellationToken);
            if (wantStorageCreationTime is null && pushStorageCreationTime is null) return null;

            if ((wantStorageCreationTime ?? DateTime.MinValue) < (pushStorageCreationTime ?? DateTime.MinValue))
            {
                return await _publisher.ReadShoutAsync(signature, cancellationToken);
            }
            else
            {
                return await _subscriber.ReadShoutAsync(signature, cancellationToken);
            }
        }

        private static ContentLocation SignatureToResourceTag(OmniSignature signature)
        {
            using var hub = new BytesPipe();
            signature.Export(hub.Writer, BytesPool.Shared);
            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(hub.Reader.GetSequence()));
            return new ContentLocation(hash, EngineName);
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

    public sealed class ShoutExchangerException : Exception
    {
        public ShoutExchangerException()
            : base()
        {
        }

        public ShoutExchangerException(string message)
            : base(message)
        {
        }

        public ShoutExchangerException(string message, Exception innerException)
            : base(message, innerException)
        {
        }
    }
}
