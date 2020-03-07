using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;
using System.Collections.Generic;
using System.Threading;

namespace Omnius.Xeus.Service
{
    public interface INegotiatorFactory
    {
        ValueTask<INegotiator> CreateAsync(string configPath, NegotiatorOptions negotiatorOptions, IEnumerable<IConnector> connectors, 
            IEnumerable<IWantStorage> wantStorages, IEnumerable<IPublishStorage> publishStorages, IBytesPool bytesPool);
    }

    public interface INegotiator
    {
    }
}
