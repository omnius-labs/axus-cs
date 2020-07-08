using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Drivers;

namespace Omnius.Xeus.Service.Engines
{
    public interface IMessageExchangerFactory
    {
        ValueTask<IMessageExchanger> CreateAsync(MessageExchangerOptions options,
            IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
            INodeFinder nodeFinder, IPublishMessageStorage publishStorage, IWantMessageStorage wantStorage, IBytesPool bytesPool);
    }

    public interface IMessageExchanger
    {
    }
}
