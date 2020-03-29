using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Drivers;

namespace Omnius.Xeus.Service.Engines
{
    public interface IBlockExchangerFactory
    {
        ValueTask<IBlockExchanger> CreateAsync(string configPath, BlockExchangerOptions options,
            IObjectStoreFactory objectStoreFactory, IConnectionController connectionController,
            INodeExplorer nodeExplorer, IPublishStorage publishStorage, IWantStorage wantStorage, IBytesPool bytesPool);
    }

    public interface IBlockExchanger
    {
    }
}
