using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Drivers;

namespace Omnius.Xeus.Service.Engines
{
    public interface IContentExchangerFactory
    {
        ValueTask<IContentExchanger> CreateAsync(ContentExchangerOptions options,
            IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
            INodeFinder nodeFinder, IPublishContentStorage publishStorage, IWantContentStorage wantStorage, IBytesPool bytesPool);
    }

    public interface IContentExchanger
    {
    }
}
