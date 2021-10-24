using System;
using System.Collections.Generic;
using System.Collections.Immutable;
using System.Data;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;
using Omnius.Core.Tasks;
using Omnius.Xeus.Service.Engines.Internal.Models;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class ShoutExchanger : AsyncDisposableBase, IShoutExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ISessionConnector _sessionConnector;
        private readonly ISessionAccepter _sessionAccepter;
        private readonly INodeFinder _nodeFinder;
        private readonly IPublishedShoutStorage _publishedShoutStorage;
        private readonly ISubscribedShoutStorage _subscribedShoutStorage;
        private readonly IBatchActionDispatcher _batchActionDispatcher;
        private readonly IBytesPool _bytesPool;
        private readonly ShoutExchangerOptions _options;

        private readonly VolatileHashSet<OmniAddress> _connectedAddressSet;

        private ImmutableHashSet<SessionStatus> _sessionStatusSet = ImmutableHashSet<SessionStatus>.Empty;

        private ImmutableHashSet<ContentClue> _pushContentClues = ImmutableHashSet<ContentClue>.Empty;
        private ImmutableHashSet<ContentClue> _wantContentClues = ImmutableHashSet<ContentClue>.Empty;

        private Task _connectLoopTask = null!;
        private Task _acceptLoopTask = null!;
        private Task _computeLoopTask = null!;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly object _lockObject = new();

        private const string ServiceName = "shout_exchanger";

        public static async ValueTask<ShoutExchanger> CreateAsync(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
            IPublishedShoutStorage publishedShoutStorage, ISubscribedShoutStorage subscribedShoutStorage, IBatchActionDispatcher batchActionDispatcher,
            IBytesPool bytesPool, ShoutExchangerOptions options, CancellationToken cancellationToken = default)
        {
            var shoutExchanger = new ShoutExchanger(sessionConnector, sessionAccepter, nodeFinder, publishedShoutStorage, subscribedShoutStorage, batchActionDispatcher, bytesPool, options);
            return shoutExchanger;
        }

        private ShoutExchanger(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, INodeFinder nodeFinder,
            IPublishedShoutStorage publishedShoutStorage, ISubscribedShoutStorage subscribedShoutStorage, IBatchActionDispatcher batchActionDispatcher,
            IBytesPool bytesPool, ShoutExchangerOptions options)
        {
            _sessionConnector = sessionConnector;
            _sessionAccepter = sessionAccepter;
            _nodeFinder = nodeFinder;
            _publishedShoutStorage = publishedShoutStorage;
            _subscribedShoutStorage = subscribedShoutStorage;
            _batchActionDispatcher = batchActionDispatcher;
            _bytesPool = bytesPool;
            _options = options;

            _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromMinutes(3), _batchActionDispatcher);

            _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
            _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await Task.WhenAll(_connectLoopTask!, _acceptLoopTask!);
            _cancellationTokenSource.Dispose();

            _connectedAddressSet.Dispose();
        }

        public async ValueTask<ShoutExchangerReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            var sessionReports = new List<SessionReport>();

            foreach (var status in _sessionStatusSet)
            {
                sessionReports.Add(new SessionReport(ServiceName, status.Session.HandshakeType, status.Session.Address));
            }

            return new ShoutExchangerReport(0, 0, sessionReports.ToArray());
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
                    await Task.Delay(1000, cancellationToken);

                    int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Connected).Count();
                    if (_sessionStatusSet.Count > (_options.MaxSessionCount / 2)) continue;

                    foreach (var signature in await _subscribedShoutStorage.GetSignaturesAsync(cancellationToken))
                    {
                        var contentClue = SignatureToContentClue(signature);

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

                            if (await this.TryAddConnectedSessionAsync(session, signature, cancellationToken))
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

        private async ValueTask<bool> TryAddConnectedSessionAsync(ISession session, OmniSignature signature, CancellationToken cancellationToken = default)
        {
            try
            {
                var version = await this.HandshakeVersionAsync(session.Connection, cancellationToken);
                if (version is null) return false;

                if (version == ShoutExchangerVersion.Version1)
                {
                    var messageCreationTime = await this.ReadMessageCreationTimeAsync(signature, cancellationToken);
                    if (messageCreationTime == null) messageCreationTime = DateTime.MinValue;

                    var requestMessage = new ShoutExchangerFetchRequestMessage(signature, Timestamp.FromDateTime(messageCreationTime.Value));
                    var resultMessage = await session.Connection.SendAndReceiveAsync<ShoutExchangerFetchRequestMessage, ShoutExchangerFetchResultMessage>(requestMessage, cancellationToken);

                    if (resultMessage.Type == ShoutExchangerFetchResultType.Found && resultMessage.Shout is not null)
                    {
                        await _subscribedShoutStorage.WriteShoutAsync(resultMessage.Shout, cancellationToken);
                    }
                    else if (resultMessage.Type == ShoutExchangerFetchResultType.NotFound)
                    {
                        var message = await this.ReadMessageAsync(signature, cancellationToken);
                        if (message is null) throw new ShoutExchangerException();

                        var postMessage = new ShoutExchangerPostMessage(message);
                        await session.Connection.Sender.SendAsync(postMessage, cancellationToken);
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

                if (version == ShoutExchangerVersion.Version1)
                {
                    var requestMessage = await session.Connection.Receiver.ReceiveAsync<ShoutExchangerFetchRequestMessage>(cancellationToken);

                    var messageCreationTime = await this.ReadMessageCreationTimeAsync(requestMessage.Signature, cancellationToken);
                    if (messageCreationTime == null) messageCreationTime = DateTime.MinValue;

                    if (requestMessage.CreationTime.ToDateTime() == messageCreationTime.Value)
                    {
                        var resultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.Same, null);
                        await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);
                    }
                    else if (requestMessage.CreationTime.ToDateTime() < messageCreationTime.Value)
                    {
                        var message = await this.ReadMessageAsync(requestMessage.Signature, cancellationToken);
                        var resultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.Found, message);
                        await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);
                    }
                    else
                    {
                        var resultMessage = new ShoutExchangerFetchResultMessage(ShoutExchangerFetchResultType.NotFound, null);
                        await session.Connection.Sender.SendAsync(resultMessage, cancellationToken);

                        var postMessage = await session.Connection.Receiver.ReceiveAsync<ShoutExchangerPostMessage>(cancellationToken);
                        if (postMessage.Shout is null) return false;

                        await _subscribedShoutStorage.WriteShoutAsync(postMessage.Shout, cancellationToken);
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
            }
            catch (Exception e)
            {
                _logger.Warn(e);
            }

            return false;
        }

        private async ValueTask<ShoutExchangerVersion?> HandshakeVersionAsync(IConnection connection, CancellationToken cancellationToken = default)
        {
            var myHelloMessage = new ShoutExchangerHelloMessage(new[] { ShoutExchangerVersion.Version1 });
            var otherHelloMessage = await connection.ExchangeAsync(myHelloMessage, cancellationToken);

            var version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
            return version;
        }

        private async ValueTask<DateTime?> ReadMessageCreationTimeAsync(OmniSignature signature, CancellationToken cancellationToken)
        {
            var wantStorageCreationTime = await _subscribedShoutStorage.ReadShoutCreationTimeAsync(signature, cancellationToken);
            var pushStorageCreationTime = await _publishedShoutStorage.ReadShoutCreationTimeAsync(signature, cancellationToken);
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
            var wantStorageCreationTime = await _subscribedShoutStorage.ReadShoutCreationTimeAsync(signature, cancellationToken);
            var pushStorageCreationTime = await _publishedShoutStorage.ReadShoutCreationTimeAsync(signature, cancellationToken);
            if (wantStorageCreationTime is null && pushStorageCreationTime is null) return null;

            if ((wantStorageCreationTime ?? DateTime.MinValue) < (pushStorageCreationTime ?? DateTime.MinValue))
            {
                return await _publishedShoutStorage.ReadShoutAsync(signature, cancellationToken);
            }
            else
            {
                return await _subscribedShoutStorage.ReadShoutAsync(signature, cancellationToken);
            }
        }

        private async Task ComputeLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

                    await this.UpdatePushContentCluesAsync(cancellationToken);
                    await this.UpdateWantContentCluesAsync(cancellationToken);
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

            foreach (var rootHash in await _publishedShoutStorage.GetSignaturesAsync(cancellationToken))
            {
                var contentClue = SignatureToContentClue(rootHash);
                builder.Add(contentClue);
            }

            _pushContentClues = builder.ToImmutable();
        }

        private async ValueTask UpdateWantContentCluesAsync(CancellationToken cancellationToken = default)
        {
            var builder = ImmutableHashSet.CreateBuilder<ContentClue>();

            foreach (var rootHash in await _subscribedShoutStorage.GetSignaturesAsync(cancellationToken))
            {
                var contentClue = SignatureToContentClue(rootHash);
                builder.Add(contentClue);
            }

            _wantContentClues = builder.ToImmutable();
        }

        private static ContentClue SignatureToContentClue(OmniSignature signature)
        {
            using var bytesPipe = new BytesPipe();
            signature.Export(bytesPipe.Writer, BytesPool.Shared);
            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(bytesPipe.Reader.GetSequence()));
            return new ContentClue(ServiceName, hash);
        }
    }
}
