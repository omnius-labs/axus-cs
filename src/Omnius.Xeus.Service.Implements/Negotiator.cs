using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public sealed partial class Negotiator : AsyncDisposableBase, INegotiator
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly string _configPath;
        private readonly NegotiatorOptions _negotiatorOptions;
        private readonly List<IConnector> _connectors = new List<IConnector>();
        private readonly List<IWantStorage> _wantStorages = new List<IWantStorage>();
        private readonly List<IPublishStorage> _publishStorages = new List<IPublishStorage>();
        private readonly IBytesPool _bytesPool;

        internal sealed class NegotiatorFactory : INegotiatorFactory
        {
            public async ValueTask<INegotiator> CreateAsync(string configPath, NegotiatorOptions negotiatorOptions,
                IEnumerable<IConnector> connectors, IEnumerable<IWantStorage> wantStorages, IEnumerable<IPublishStorage> publishStorages, IBytesPool bytesPool)
            {
                var result = new Negotiator(configPath, negotiatorOptions, connectors, wantStorages, publishStorages, bytesPool);
                await result.InitAsync();

                return result;
            }
        }

        public static INegotiatorFactory Factory { get; } = new NegotiatorFactory();

        internal Negotiator(string configPath, NegotiatorOptions negotiatorOptions,
            IEnumerable<IConnector> connectors, IEnumerable<IWantStorage> wantStorages, IEnumerable<IPublishStorage> publishStorages, IBytesPool bytesPool)
        {
            _configPath = configPath;
            _negotiatorOptions = negotiatorOptions;
            _connectors.AddRange(connectors);
            _wantStorages.AddRange(wantStorages);
            _publishStorages.AddRange(publishStorages);
            _bytesPool = bytesPool;
        }

        internal async ValueTask InitAsync()
        {

        }

        protected override async ValueTask OnDisposeAsync()
        {

        }
    }
}
