using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public sealed class KadexExplorer : AsyncDisposableBase, IKadexExplorer
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly List<IConnector> _connectors = new List<IConnector>();
        private readonly IBufferPool<byte> _bufferPool;

        internal sealed class ExplorerFactory : IKadexExplorerFactory
        {
            public async ValueTask<IKadexExplorer> CreateAsync(string configPath, IEnumerable<IConnector> connectors, IBufferPool<byte> bufferPool)
            {
                var result = new KadexExplorer(configPath, connectors, bufferPool);
                await result.InitAsync();

                return result;
            }
        }

        public static IKadexExplorerFactory Factory { get; } = new ExplorerFactory();

        internal KadexExplorer(string configPath, IEnumerable<IConnector> connectors, IBufferPool<byte> bufferPool)
        {
            _configPath = configPath;
            _connectors.AddRange(connectors);
            _bufferPool = bufferPool;
        }

        internal async ValueTask InitAsync()
        {

        }

        protected override async ValueTask OnDisposeAsync()
        {

        }

        public IEnumerable<NodeProfile> FindNodeProfiles(Span<byte> id)
        {
            throw new NotImplementedException();
        }
    }
}
