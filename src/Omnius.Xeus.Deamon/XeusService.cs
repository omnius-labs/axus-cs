using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Rpc;
using Omnius.Xeus.Components.Engines;
using Omnius.Xeus.Components.Models;
using Omnius.Xeus.Deamon.Models;
using System.IO;
using Omnius.Core;

namespace Omnius.Xeus.Deamon
{
    public record XeusServiceOptions
    {
        public XeusConfig Config;
    }

    public class XeusService : IXeusService
    {
        private readonly INodeFinderFactory _nodeFinderFactory;
        private readonly IContentExchangerFactory _contentExchangerFactory;
        private readonly IDeclaredMessageExchangerFactory _declaredMessageExchangerFactory;
        private readonly IBytesPool _bytesPool;
        private readonly XeusConfig _xeusConfig;

        public static async ValueTask<XeusService> CreateAsync(INodeFinderFactory nodeFinderFactory, IContentExchangerFactory contentExchangerFactory, IDeclaredMessageExchangerFactory declaredMessageExchangerFactory, XeusConfig xeusConfig)
        {
            var service = new XeusService(nodeFinderFactory, contentExchangerFactory, declaredMessageExchangerFactory, xeusConfig);
            await service.InitAsync();
            return service;
        }

        private XeusService(INodeFinderFactory nodeFinderFactory, IContentExchangerFactory contentExchangerFactory, IDeclaredMessageExchangerFactory declaredMessageExchangerFactory, XeusConfig xeusConfig)
        {
            _nodeFinderFactory = nodeFinderFactory;
            _contentExchangerFactory = contentExchangerFactory;
            _declaredMessageExchangerFactory = declaredMessageExchangerFactory;
            _xeusConfig = xeusConfig;
        }

        private async ValueTask InitAsync(CancellationToken cancellationToken = default)
        {
            var nodeFinderOptions = new NodeFinderOptions(Path.Combine(_xeusConfig.WorkingDirectory, "node_finder"), 10);
            await _nodeFinderFactory.CreateAsync(nodeFinderOptions, _bytesPool);

            var
            }

        public ValueTask<AddCloudNodeProfilesResult> AddCloudNodeProfilesAsync(CancellationToken cancellationToken)
        {

        }

        public ValueTask<FindNodeProfilesResult> FindNodeProfilesAsync(FindNodeProfilesParam param, CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }

        public ValueTask GetMyNodeProfileAsync(CancellationToken cancellationToken)
        {
            throw new System.NotImplementedException();
        }
    }
}
