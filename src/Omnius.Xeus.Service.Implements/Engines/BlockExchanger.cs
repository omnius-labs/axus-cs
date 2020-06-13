using System;
using System.Buffers.Binary;
using System.Collections.Generic;
using System.Drawing;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Core.Network.Connections;
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines.Internal;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class BlockExchanger : AsyncDisposableBase, IBlockExchanger
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly BlockExchangerOptions _options;
        private readonly IObjectStoreFactory _objectStoreFactory;
        private readonly IConnectionController _connectionController;
        private readonly INodeExplorer _nodeExplorer;
        private readonly IPublishStorage _publishStorage;
        private readonly IWantStorage _wantStorage;
        private readonly IBytesPool _bytesPool;

        private IObjectStore _objectStore;

        private Task _connectLoopTask;
        private Task _acceptLoopTask;
        private Task _sendLoopTask;
        private Task _receiveLoopTask;
        private Task _computeLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly AsyncLock _asyncLock = new AsyncLock();

        public const string ServiceName = "BlockExchanger`";

        internal sealed class BlockExchangerFactory : IBlockExchangerFactory
        {
            public async ValueTask<IBlockExchanger> CreateAsync(string configPath, BlockExchangerOptions options,
                IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
                INodeExplorer nodeExplorer, IPublishStorage publishStorage, IWantStorage wantStorage, IBytesPool bytesPool)
            {
                var result = new BlockExchanger(configPath, options, objectStoreFactory, connectionController, nodeExplorer, publishStorage, wantStorage, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IBlockExchangerFactory Factory { get; } = new BlockExchangerFactory();

        internal BlockExchanger(string configPath, BlockExchangerOptions options,
                IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
                INodeExplorer nodeExplorer, IPublishStorage publishStorage, IWantStorage wantStorage, IBytesPool bytesPool)
        {
            _configPath = configPath;
            _options = options;
            _objectStoreFactory = objectStoreFactory;
            _connectionController = connectionController;
            _nodeExplorer = nodeExplorer;
            _publishStorage = publishStorage;
            _wantStorage = wantStorage;
            _bytesPool = bytesPool;
        }

        internal async ValueTask InitAsync()
        {
            _objectStore = await _objectStoreFactory.CreateAsync(_configPath, _bytesPool);

            await this.LoadAsync();

            _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
            _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
            _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
            _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
            _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {

        }

        private async ValueTask LoadAsync()
        {

        }

        private async ValueTask SaveAsync()
        {

        }

        private async Task ConnectLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();
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

        private Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task SendLoopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
        }

        private Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            throw new NotImplementedException();
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
                        _receivedPushContentLocationMap.Refresh();
                        _receivedGiveContentLocationMap.Refresh();

                        foreach (var connectionStatus in _connectionStatusSet)
                        {
                            connectionStatus.Refresh();
                        }
                    }

                    // 自分のノードプロファイル
                    var myNodeProfile = await this.GetMyNodeProfile(cancellationToken);

                    // ノード情報
                    var elements = new List<KademliaElement<ComputingNodeElement>>();

                    // 送信するノードプロファイル
                    var sendingPushNodeProfiles = new List<NodeProfile>();

                    // コンテンツのロケーション情報
                    var contentLocationMap = new Dictionary<OmniHash, HashSet<NodeProfile>>();

                    // 送信するコンテンツのロケーション情報
                    var sendingPushContentLocationMap = new Dictionary<OmniHash, HashSet<NodeProfile>>();

                    // 送信するコンテンツのロケーションリクエスト情報
                    var sendingWantContentLocationSet = new HashSet<OmniHash>();

                    lock (_lockObject)
                    {
                        foreach (var connectionStatus in _connectionStatusSet)
                        {
                            elements.Add(new KademliaElement<ComputingNodeElement>(connectionStatus.Id, new ComputingNodeElement(connectionStatus)));
                        }
                    }

                    lock (_lockObject)
                    {
                        sendingPushNodeProfiles.AddRange(_cloudNodeProfiles);
                    }

                    foreach (var publishReport in await _publishStorage.GetReportsAsync(cancellationToken))
                    {
                        contentLocationMap.GetOrAdd(publishReport.Tag, (_) => new HashSet<NodeProfile>())
                             .Add(myNodeProfile);

                        sendingPushContentLocationMap.GetOrAdd(publishReport.Tag, (_) => new HashSet<NodeProfile>())
                             .Add(myNodeProfile);
                    }

                    foreach (var wantReport in await _wantStorage.GetReportsAsync(cancellationToken))
                    {
                        sendingWantContentLocationSet.Add(wantReport.Tag);
                    }

                    lock (_lockObject)
                    {
                        foreach (var (tag, nodeProfiles) in _receivedPushContentLocationMap)
                        {
                            contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);

                            sendingPushContentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);
                        }

                        foreach (var (tag, nodeProfiles) in _receivedGiveContentLocationMap)
                        {
                            contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeProfile>())
                                 .UnionWith(nodeProfiles);
                        }
                    }

                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            foreach (var tag in element.Value.ConnectionStatus.ReceivedWantContentLocations)
                            {
                                sendingWantContentLocationSet.Add(tag);
                            }
                        }
                    }

                    // Compute PushNodeProfiles
                    foreach (var element in elements)
                    {
                        element.Value.SendingPushNodeProfiles.AddRange(sendingPushNodeProfiles);
                    }

                    // Compute PushContentLocations
                    foreach (var (tag, nodeProfiles) in sendingPushContentLocationMap)
                    {
                        foreach (var element in Kademlia.Search(_myId.Span, tag.Value.Span, elements, 1))
                        {
                            element.Value.SendingPushContentLocations[tag] = nodeProfiles.ToList();
                        }
                    }

                    // Compute WantContentLocations
                    foreach (var tag in sendingWantContentLocationSet)
                    {
                        foreach (var element in Kademlia.Search(_myId.Span, tag.Value.Span, elements, 1))
                        {
                            element.Value.SendingWantContentLocations.Add(tag);
                        }
                    }

                    // Compute GiveContentLocations
                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            foreach (var tag in element.Value.ConnectionStatus.ReceivedWantContentLocations)
                            {
                                if (!contentLocationMap.TryGetValue(tag, out var nodeProfiles))
                                {
                                    continue;
                                }

                                element.Value.SendingGiveContentLocations[tag] = nodeProfiles.ToList();
                            }
                        }
                    }

                    foreach (var element in elements)
                    {
                        lock (element.Value.ConnectionStatus.LockObject)
                        {
                            element.Value.ConnectionStatus.SendingDataMessage =
                                new NodeExplorerDataMessage(
                                    element.Value.SendingPushNodeProfiles.Take(NodeExplorerDataMessage.MaxPushNodeProfilesCount).ToArray(),
                                    element.Value.SendingPushContentLocations.Select(n => new ContentLocation(n.Key, n.Value.Take(ContentLocation.MaxNodeProfilesCount).ToArray())).Take(NodeExplorerDataMessage.MaxPushContentLocationsCount).ToArray(),
                                    element.Value.SendingWantContentLocations.Take(NodeExplorerDataMessage.MaxWantContentLocationsCount).ToArray(),
                                    element.Value.SendingGiveContentLocations.Select(n => new ContentLocation(n.Key, n.Value.Take(ContentLocation.MaxNodeProfilesCount).ToArray())).Take(NodeExplorerDataMessage.MaxGiveContentLocationsCount).ToArray());
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

        private static uint GolombCodedSetComputeHash(OmniHash omniHash)
        {
            uint result = 0;
            const int unit = sizeof(uint);

            int loopCount = omniHash.Value.Length / unit;
            for (int i = 0; i < loopCount; i++)
            {
                result ^= BinaryPrimitives.ReadUInt32BigEndian(omniHash.Value.Span.Slice(i * unit, unit));
            }

            return result;
        }

        private enum ConnectionHandshakeType
        {
            Connected,
            Accepted,
        }

        private sealed class ConnectionStatus : ISynchronized
        {
            public ConnectionStatus(IConnection connection, OmniAddress address,
                ConnectionHandshakeType handshakeType, NodeProfile nodeProfile, OmniHash tag)
            {
                this.Connection = connection;
                this.Address = address;
                this.HandshakeType = handshakeType;
                this.NodeProfile = nodeProfile;
                this.Tag = tag;
            }

            public object LockObject { get; } = new object();

            public IConnection Connection { get; }
            public OmniAddress Address { get; }
            public ConnectionHandshakeType HandshakeType { get; }

            public NodeProfile NodeProfile { get; }
            public OmniHash Tag { get; }

            public VolatileSet<OmniHash> ReceivedWantBlocks { get; } = new VolatileSet<OmniHash>(TimeSpan.FromMinutes(30));
            public GolombCodedSet<OmniHash>? OwnedBlocksFilter { get; set; }

            public void Refresh()
            {
                this.ReceivedWantBlocks.Refresh();
            }
        }
    }
}
