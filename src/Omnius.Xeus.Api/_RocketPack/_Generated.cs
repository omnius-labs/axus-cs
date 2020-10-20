using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Api.Models;
using Omnius.Xeus.Engines.Models;

#nullable enable

namespace Omnius.Xeus.Api
{
    public interface IXeusService
    {
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult> GetMyNodeProfileAsync(global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask AddCloudNodeProfilesAsync(global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult> GetPushContentsReportAsync(global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.RegisterPushContentResult> RegisterPushContentAsync(global::Omnius.Xeus.Api.Models.RegisterPushContentParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask UnregisterPushContentAsync(global::Omnius.Xeus.Api.Models.UnregisterPushContentParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult> GetWantContentsReportAsync(global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask RegisterWantContentAsync(global::Omnius.Xeus.Api.Models.RegisterWantContentParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask UnregisterWantContentAsync(global::Omnius.Xeus.Api.Models.UnregisterWantContentParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask ExportWantContentAsync(global::Omnius.Xeus.Api.Models.ExportWantContentParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult> GetPushDeclaredMessagesReportAsync(global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask RegisterPushDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask UnregisterPushDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask RegisterWantDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask UnregisterWantDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken = default);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.ExportWantDeclaredMessageResult> ExportWantDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.ExportWantDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken = default);
    }
    public class XeusService
    {
        public class Client : global::Omnius.Core.AsyncDisposableBase, global::Omnius.Xeus.Api.IXeusService
        {
            private readonly global::Omnius.Xeus.Api.IXeusService _service;
            private readonly global::Omnius.Core.Network.Connections.IConnection _connection;
            private readonly global::Omnius.Core.IBytesPool _bytesPool;
            private readonly global::Omnius.Core.RocketPack.Remoting.RocketPackRpc _rpc;
            public Client(global::Omnius.Xeus.Api.IXeusService service, global::Omnius.Core.Network.Connections.IConnection connection, global::Omnius.Core.IBytesPool bytesPool)
            {
                _service = service;
                _connection = connection;
                _bytesPool = bytesPool;
                _rpc = new global::Omnius.Core.RocketPack.Remoting.RocketPackRpc(_connection, _bytesPool);
            }
            protected override async global::System.Threading.Tasks.ValueTask OnDisposeAsync()
            {
                await _rpc.DisposeAsync();
            }
            public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult> GetMyNodeProfileAsync(global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(1, cancellationToken);
                return await stream.CallFunctionAsync<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult>(cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask AddCloudNodeProfilesAsync(global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(2, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult> GetPushContentsReportAsync(global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(3, cancellationToken);
                return await stream.CallFunctionAsync<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult>(cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.RegisterPushContentResult> RegisterPushContentAsync(global::Omnius.Xeus.Api.Models.RegisterPushContentParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(4, cancellationToken);
                return await stream.CallFunctionAsync<global::Omnius.Xeus.Api.Models.RegisterPushContentParam, global::Omnius.Xeus.Api.Models.RegisterPushContentResult>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask UnregisterPushContentAsync(global::Omnius.Xeus.Api.Models.UnregisterPushContentParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(5, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult> GetWantContentsReportAsync(global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(6, cancellationToken);
                return await stream.CallFunctionAsync<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult>(cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask RegisterWantContentAsync(global::Omnius.Xeus.Api.Models.RegisterWantContentParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(7, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.RegisterWantContentParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask UnregisterWantContentAsync(global::Omnius.Xeus.Api.Models.UnregisterWantContentParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(8, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask ExportWantContentAsync(global::Omnius.Xeus.Api.Models.ExportWantContentParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(9, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.ExportWantContentParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult> GetPushDeclaredMessagesReportAsync(global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(10, cancellationToken);
                return await stream.CallFunctionAsync<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult>(cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask RegisterPushDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(11, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask UnregisterPushDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(12, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask RegisterWantDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(14, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask UnregisterWantDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(15, cancellationToken);
                await stream.CallActionAsync<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam>(param, cancellationToken);
            }
            public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Api.Models.ExportWantDeclaredMessageResult> ExportWantDeclaredMessageAsync(global::Omnius.Xeus.Api.Models.ExportWantDeclaredMessageParam param, global::System.Threading.CancellationToken cancellationToken)
            {
                using var stream = await _rpc.ConnectAsync(16, cancellationToken);
                return await stream.CallFunctionAsync<global::Omnius.Xeus.Api.Models.ExportWantDeclaredMessageParam, global::Omnius.Xeus.Api.Models.ExportWantDeclaredMessageResult>(param, cancellationToken);
            }
        }
        public class Server : global::Omnius.Core.AsyncDisposableBase
        {
            private readonly global::Omnius.Xeus.Api.IXeusService _service;
            private readonly global::Omnius.Core.Network.Connections.IConnection _connection;
            private readonly global::Omnius.Core.IBytesPool _bytesPool;
            private readonly global::Omnius.Core.RocketPack.Remoting.RocketPackRpc _rpc;
            public Server(global::Omnius.Xeus.Api.IXeusService service, global::Omnius.Core.Network.Connections.IConnection connection, global::Omnius.Core.IBytesPool bytesPool)
            {
                _service = service;
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
                        case 1:
                            {
                                await stream.ListenFunctionAsync<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult>(_service.GetMyNodeProfileAsync, cancellationToken);
                            }
                            break;
                        case 2:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam>(_service.AddCloudNodeProfilesAsync, cancellationToken);
                            }
                            break;
                        case 3:
                            {
                                await stream.ListenFunctionAsync<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult>(_service.GetPushContentsReportAsync, cancellationToken);
                            }
                            break;
                        case 4:
                            {
                                await stream.ListenFunctionAsync<global::Omnius.Xeus.Api.Models.RegisterPushContentParam, global::Omnius.Xeus.Api.Models.RegisterPushContentResult>(_service.RegisterPushContentAsync, cancellationToken);
                            }
                            break;
                        case 5:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam>(_service.UnregisterPushContentAsync, cancellationToken);
                            }
                            break;
                        case 6:
                            {
                                await stream.ListenFunctionAsync<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult>(_service.GetWantContentsReportAsync, cancellationToken);
                            }
                            break;
                        case 7:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.RegisterWantContentParam>(_service.RegisterWantContentAsync, cancellationToken);
                            }
                            break;
                        case 8:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam>(_service.UnregisterWantContentAsync, cancellationToken);
                            }
                            break;
                        case 9:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.ExportWantContentParam>(_service.ExportWantContentAsync, cancellationToken);
                            }
                            break;
                        case 10:
                            {
                                await stream.ListenFunctionAsync<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult>(_service.GetPushDeclaredMessagesReportAsync, cancellationToken);
                            }
                            break;
                        case 11:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam>(_service.RegisterPushDeclaredMessageAsync, cancellationToken);
                            }
                            break;
                        case 12:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam>(_service.UnregisterPushDeclaredMessageAsync, cancellationToken);
                            }
                            break;
                        case 13:
                            {
                            }
                            break;
                        case 14:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam>(_service.RegisterWantDeclaredMessageAsync, cancellationToken);
                            }
                            break;
                        case 15:
                            {
                                await stream.ListenActionAsync<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam>(_service.UnregisterWantDeclaredMessageAsync, cancellationToken);
                            }
                            break;
                        case 16:
                            {
                                await stream.ListenFunctionAsync<global::Omnius.Xeus.Api.Models.ExportWantDeclaredMessageParam, global::Omnius.Xeus.Api.Models.ExportWantDeclaredMessageResult>(_service.ExportWantDeclaredMessageAsync, cancellationToken);
                            }
                            break;
                    }
                }
            }
        }
    }
}
