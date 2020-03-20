using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Network.Connections;
using Omnius.Xeus.Service.Internal;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public sealed partial class NodeExplorer : AsyncDisposableBase, INodeExplorer
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly ExplorerOptions _explorerOptions;
        private readonly IObjectStoreFactory _objectStoreFactory;
        private readonly List<IConnector> _connectors = new List<IConnector>();
        private readonly List<IPublishStorage> _publishStorages = new List<IPublishStorage>();
        private readonly List<IWantStorage> _wantStorages = new List<IWantStorage>();
        private readonly IBytesPool _bytesPool;

        private IObjectStore _objectStore;

        private readonly HashSet<NodeProfile> _nodeProfileSet = new HashSet<NodeProfile>();
        private readonly HashSet<ConnectionStatus> _connectionStatusSet = new HashSet<ConnectionStatus>();

        private Task _connectLoopTask;
        private Task _acceptLoopTask;
        private Task _sendLoopTask;
        private Task _receiveLoopTask;
        private readonly CancellationTokenSource _cancellationTokenSource = new CancellationTokenSource();

        private readonly AsyncLock _asyncLock = new AsyncLock();

        private const int MaxBucketLength = 20;

        internal sealed class NodeExplorerFactory : INodeExplorerFactory
        {
            public async ValueTask<INodeExplorer> CreateAsync(string configPath, ExplorerOptions explorerOptions,
                IObjectStoreFactory objectStoreFactory, IEnumerable<IConnector> connectors,
                IEnumerable<IPublishStorage> publishStorages, IEnumerable<IWantStorage> wantStorages,
                IBytesPool bytesPool)
            {
                var result = new NodeExplorer(configPath, explorerOptions, objectStoreFactory, connectors, publishStorages, wantStorages, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static INodeExplorerFactory Factory { get; } = new NodeExplorerFactory();

        internal NodeExplorer(string configPath, ExplorerOptions explorerOptions,
            IObjectStoreFactory objectStoreFactory, IEnumerable<IConnector> connectors,
            IEnumerable<IPublishStorage> publishStorages, IEnumerable<IWantStorage> wantStorages,
            IBytesPool bytesPool)
        {
            _configPath = configPath;
            _explorerOptions = explorerOptions;
            _objectStoreFactory = objectStoreFactory;
            _connectors.AddRange(connectors);
            _publishStorages.AddRange(publishStorages);
            _wantStorages.AddRange(wantStorages);
            _bytesPool = bytesPool;
        }

        internal async ValueTask InitAsync()
        {
            _objectStore = await _objectStoreFactory.CreateAsync(_configPath, _bytesPool);

            await this.LoadAsync();

            _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
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

        private async Task ConnectLoopAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    foreach (var connector in _connectors)
                    {
                        connector.
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

        private enum MessageType
        {
            PostNodeProfiles = 0,
            GetContentLocations = 1,
        }

        private async Task SendLoopAsync(CancellationToken cancellationToken = default)
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

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                for (; ; )
                {
                    cancellationToken.ThrowIfCancellationRequested();

                    _connectionStatusSet.
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

        public async IAsyncEnumerable<NodeProfile> FindNodeProfilesAsync(OmniHash rootHash, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }

        private class ConnectionStatus : ISynchronized
        {
            public ConnectionStatus(NodeProfile nodeProfile, IConnection connection)
            {
                this.NodeProfile = nodeProfile;
                this.Connection = connection;
            }

            public object LockObject { get; } = new object();

            public NodeProfile NodeProfile { get; }
            public IConnection Connection { get; }
        }
    }
}
