using Omnius.Core.Cryptography;
using Omnius.Core.Network;

#nullable enable

namespace Omnius.Xeus.Rpc
{

    public sealed partial class RpcParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.RpcParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcParam>.Formatter;
        public static global::Omnius.Xeus.Rpc.RpcParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcParam>.Empty;

        static RpcParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcParam>.Empty = new global::Omnius.Xeus.Rpc.RpcParam(0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public RpcParam(int p1)
        {
            this.P1 = p1;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (p1 != default) ___h.Add(p1.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public int P1 { get; }

        public static global::Omnius.Xeus.Rpc.RpcParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Rpc.RpcParam? left, global::Omnius.Xeus.Rpc.RpcParam? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Rpc.RpcParam? left, global::Omnius.Xeus.Rpc.RpcParam? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Rpc.RpcParam)) return false;
            return this.Equals((global::Omnius.Xeus.Rpc.RpcParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Rpc.RpcParam? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.P1 != target.P1) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.RpcParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Rpc.RpcParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.P1 != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.P1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Rpc.RpcParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                int p_p1 = 0;

                for (;;)
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_p1 = r.GetInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Rpc.RpcParam(p_p1);
            }
        }
    }
    public sealed partial class RpcResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.RpcResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcResult>.Formatter;
        public static global::Omnius.Xeus.Rpc.RpcResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcResult>.Empty;

        static RpcResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.RpcResult>.Empty = new global::Omnius.Xeus.Rpc.RpcResult(0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public RpcResult(int r1)
        {
            this.R1 = r1;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (r1 != default) ___h.Add(r1.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public int R1 { get; }

        public static global::Omnius.Xeus.Rpc.RpcResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Rpc.RpcResult? left, global::Omnius.Xeus.Rpc.RpcResult? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Rpc.RpcResult? left, global::Omnius.Xeus.Rpc.RpcResult? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Rpc.RpcResult)) return false;
            return this.Equals((global::Omnius.Xeus.Rpc.RpcResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Rpc.RpcResult? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.R1 != target.R1) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.RpcResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Rpc.RpcResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.R1 != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.R1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Rpc.RpcResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                int p_r1 = 0;

                for (;;)
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_r1 = r.GetInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Rpc.RpcResult(p_r1);
            }
        }
    }

    public interface IXeusRpc
    {
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Rpc.RpcResult> F1Async(global::Omnius.Xeus.Rpc.RpcParam param, global::System.Threading.CancellationToken cancellationToken);
        global::System.Threading.Tasks.ValueTask F2Async(global::Omnius.Xeus.Rpc.RpcParam param, global::System.Threading.CancellationToken cancellationToken);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Rpc.RpcResult> F3Async(global::System.Threading.CancellationToken cancellationToken);
        global::System.Threading.Tasks.ValueTask F4Async(global::System.Threading.CancellationToken cancellationToken);
    }
    public class XeusRpcSender : global::Omnius.Core.AsyncDisposableBase, global::Omnius.Xeus.Rpc.IXeusRpc
    {
        private readonly global::Omnius.Xeus.Rpc.IXeusRpc _impl;
        private readonly global::Omnius.Core.Network.Connections.IConnection _connection;
        private readonly global::Omnius.Core.IBytesPool _bytesPool;
        private readonly global::Omnius.Core.RocketPack.Remoting.RocketPackRpc _rpc;
        public XeusRpcSender(global::Omnius.Xeus.Rpc.IXeusRpc impl, global::Omnius.Core.Network.Connections.IConnection connection, global::Omnius.Core.IBytesPool bytesPool)
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
        public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Rpc.RpcResult> F1Async(global::Omnius.Xeus.Rpc.RpcParam param, global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(0, cancellationToken);
            return await stream.CallFunctionAsync<global::Omnius.Xeus.Rpc.RpcParam, global::Omnius.Xeus.Rpc.RpcResult>(param, cancellationToken);
        }
        public async global::System.Threading.Tasks.ValueTask F2Async(global::Omnius.Xeus.Rpc.RpcParam param, global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(1, cancellationToken);
            await stream.CallActionAsync<global::Omnius.Xeus.Rpc.RpcParam>(param, cancellationToken);
        }
        public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Rpc.RpcResult> F3Async(global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(2, cancellationToken);
            return await stream.CallFunctionAsync<global::Omnius.Xeus.Rpc.RpcResult>(cancellationToken);
        }
        public async global::System.Threading.Tasks.ValueTask F4Async(global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(3, cancellationToken);
            await stream.CallActionAsync(cancellationToken);
        }
    }
    public class XeusRpcReceiver : global::Omnius.Core.AsyncDisposableBase
    {
        private readonly global::Omnius.Xeus.Rpc.IXeusRpc _impl;
        private readonly global::Omnius.Core.Network.Connections.IConnection _connection;
        private readonly global::Omnius.Core.IBytesPool _bytesPool;
        private readonly global::Omnius.Core.RocketPack.Remoting.RocketPackRpc _rpc;
        public XeusRpcReceiver(global::Omnius.Xeus.Rpc.IXeusRpc impl, global::Omnius.Core.Network.Connections.IConnection connection, global::Omnius.Core.IBytesPool bytesPool)
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
                            await stream.ListenFunctionAsync<global::Omnius.Xeus.Rpc.RpcParam, global::Omnius.Xeus.Rpc.RpcResult>(_impl.F1Async, cancellationToken);
                        }
                        break;
                    case 1:
                        {
                            await stream.ListenActionAsync<global::Omnius.Xeus.Rpc.RpcParam>(_impl.F2Async, cancellationToken);
                        }
                        break;
                    case 2:
                        {
                            await stream.ListenFunctionAsync<global::Omnius.Xeus.Rpc.RpcResult>(_impl.F3Async, cancellationToken);
                        }
                        break;
                    case 3:
                        {
                            await stream.ListenActionAsync(_impl.F4Async, cancellationToken);
                        }
                        break;
                }
            }
        }
    }

}
