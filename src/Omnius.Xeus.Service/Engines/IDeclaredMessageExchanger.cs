using System.Collections.Generic;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Connectors;

namespace Omnius.Xeus.Service.Engines
{
    public interface IDeclaredMessageExchangerFactory
    {
        ValueTask<IDeclaredMessageExchanger> CreateAsync(DeclaredMessageExchangerOptions options, IEnumerable<IConnector> connectors,
            INodeFinder nodeFinder, IPublishDeclaredMessageStorage publishStorage, IWantDeclaredMessageStorage wantStorage,
            IBytesPool bytesPool);
    }

    public interface IDeclaredMessageExchanger
    {
    }
}
