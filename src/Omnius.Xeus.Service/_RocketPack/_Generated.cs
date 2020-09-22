#nullable enable

namespace Omnius.Xeus.Service
{


    public interface IXeusService
    {
        global::System.Threading.Tasks.ValueTask GetMyNodeProfileAsync(global::System.Threading.CancellationToken cancellationToken);
        global::System.Threading.Tasks.ValueTask AddCloudNodeProfilesAsync(global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam param, global::System.Threading.CancellationToken cancellationToken);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult> FindNodeProfilesAsync(global::Omnius.Xeus.Service.Models.FindNodeProfilesParam param, global::System.Threading.CancellationToken cancellationToken);
    }
    public class XeusServiceSender : global::Omnius.Core.AsyncDisposableBase, global::Omnius.Xeus.Service.IXeusService
    {
        private readonly global::Omnius.Xeus.Service.IXeusService _impl;
        private readonly global::Omnius.Core.Network.Connections.IConnection _connection;
        private readonly global::Omnius.Core.IBytesPool _bytesPool;
        private readonly global::Omnius.Core.RocketPack.Remoting.RocketPackRpc _rpc;
        public XeusServiceSender(global::Omnius.Xeus.Service.IXeusService impl, global::Omnius.Core.Network.Connections.IConnection connection, global::Omnius.Core.IBytesPool bytesPool)
        {
            _impl = impl;
            _connection = connection;
            _bytesPool = bytesPool;
            _rpc = new global::Omnius.Core.RocketPack.Remoting.RocketPackRpc(_connection, _bytesPool);
        }
        protected override async global::System.Threading.Tasks.ValueTask OnDisposeAsync()
        {
            await _rpc.DisposeAsync();
        }
        public async global::System.Threading.Tasks.ValueTask GetMyNodeProfileAsync(global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(0, cancellationToken);
            await stream.CallActionAsync(cancellationToken);
        }
        public async global::System.Threading.Tasks.ValueTask AddCloudNodeProfilesAsync(global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam param, global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(1, cancellationToken);
            await stream.CallActionAsync<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam>(param, cancellationToken);
        }
        public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult> FindNodeProfilesAsync(global::Omnius.Xeus.Service.Models.FindNodeProfilesParam param, global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(2, cancellationToken);
            return await stream.CallFunctionAsync<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam, global::Omnius.Xeus.Service.Models.FindNodeProfilesResult>(param, cancellationToken);
        }
    }
    public class XeusServiceReceiver : global::Omnius.Core.AsyncDisposableBase
    {
        private readonly global::Omnius.Xeus.Service.IXeusService _impl;
        private readonly global::Omnius.Core.Network.Connections.IConnection _connection;
        private readonly global::Omnius.Core.IBytesPool _bytesPool;
        private readonly global::Omnius.Core.RocketPack.Remoting.RocketPackRpc _rpc;
        public XeusServiceReceiver(global::Omnius.Xeus.Service.IXeusService impl, global::Omnius.Core.Network.Connections.IConnection connection, global::Omnius.Core.IBytesPool bytesPool)
        {
            _impl = impl;
            _connection = connection;
            _bytesPool = bytesPool;
            _rpc = new global::Omnius.Core.RocketPack.Remoting.RocketPackRpc(_connection, _bytesPool);
        }
        protected override async global::System.Threading.Tasks.ValueTask OnDisposeAsync()
        {
            await _rpc.DisposeAsync();
        }
        public async global::System.Threading.Tasks.Task EventLoop(global::System.Threading.CancellationToken cancellationToken = default)
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                cancellationToken.ThrowIfCancellationRequested();
                using var stream = await _rpc.AcceptAsync(cancellationToken);
                switch (stream.CallId)
                {
                    case 0:
                        {
                            await stream.ListenActionAsync(_impl.GetMyNodeProfileAsync, cancellationToken);
                        }
                        break;
                    case 1:
                        {
                            await stream.ListenActionAsync<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam>(_impl.AddCloudNodeProfilesAsync, cancellationToken);
                        }
                        break;
                    case 2:
                        {
                            await stream.ListenFunctionAsync<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam, global::Omnius.Xeus.Service.Models.FindNodeProfilesResult>(_impl.FindNodeProfilesAsync, cancellationToken);
                        }
                        break;
                }
            }
        }
    }

}
