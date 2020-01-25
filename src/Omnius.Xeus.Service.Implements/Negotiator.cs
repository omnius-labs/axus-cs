using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public sealed class Negotiator : AsyncDisposableBase, INegotiator
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly List<IExplorer> _explorers = new List<IExplorer>();
        private readonly List<IWantStorage> _wantStorages = new List<IWantStorage>();
        private readonly List<IPublishStorage> _publishStorages = new List<IPublishStorage>();
        private readonly IBufferPool<byte> _bufferPool;

        internal sealed class NegotiatorFactory : INegotiatorFactory
        {
            public async ValueTask<INegotiator> CreateAsync(string configPath, IEnumerable<IExplorer> explorers,
                IEnumerable<IWantStorage> wantStorages, IEnumerable<IPublishStorage> publishStorages, IBufferPool<byte> bufferPool)
            {
                var result = new Negotiator(configPath, explorers, wantStorages, publishStorages, bufferPool);
                await result.InitAsync();

                return result;
            }
        }

        public static INegotiatorFactory Factory { get; } = new NegotiatorFactory();
      
        internal Negotiator(string configPath, IEnumerable<IExplorer> explorers,
            IEnumerable<IWantStorage> wantStorages, IEnumerable<IPublishStorage> publishStorages, IBufferPool<byte> bufferPool)
        {
            _configPath = configPath;
            _explorers.AddRange(explorers);
            _wantStorages.AddRange(wantStorages);
            _publishStorages.AddRange(publishStorages);
            _bufferPool = bufferPool;
        }

        internal async ValueTask InitAsync()
        {

        }

        protected override async ValueTask OnDisposeAsync()
        {

        }

        public int ConnectionCountUpperLimit { get; }
        public int BytesSendLimitPerSecond { get; }
        public int BytesReceiveLimitPerSecond { get; }
    }
}
