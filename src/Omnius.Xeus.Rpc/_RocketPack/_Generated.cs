using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Components.Models;

#nullable enable

namespace Omnius.Xeus.Rpc
{

    public readonly partial struct AddCloudNodeProfilesResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult>.Formatter;
        public static global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult>.Empty;

        static AddCloudNodeProfilesResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult>.Empty = new global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult(global::System.Array.Empty<NodeProfile>());
        }

        private readonly int ___hashCode;

        public static readonly int MaxNodeProfilesCount = 2147483647;

        public AddCloudNodeProfilesResult(NodeProfile[] nodeProfiles)
        {
            if (nodeProfiles is null) throw new global::System.ArgumentNullException("nodeProfiles");
            if (nodeProfiles.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("nodeProfiles");
            foreach (var n in nodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.NodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(nodeProfiles);

            {
                var ___h = new global::System.HashCode();
                foreach (var n in nodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                ___hashCode = ___h.ToHashCode();
            }
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> NodeProfiles { get; }

        public static global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult left, global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult left, global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult)) return false;
            return this.Equals((global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfiles, target.NodeProfiles)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)value.NodeProfiles.Count);
                foreach (var n in value.NodeProfiles)
                {
                    global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                }
            }

            public global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                NodeProfile[] p_nodeProfiles = global::System.Array.Empty<NodeProfile>();

                {
                    var length = r.GetUInt32();
                    p_nodeProfiles = new NodeProfile[length];
                    for (int i = 0; i < p_nodeProfiles.Length; i++)
                    {
                        p_nodeProfiles[i] = global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                    }
                }
                return new global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult(p_nodeProfiles);
            }
        }
    }
    public readonly partial struct FindNodeProfilesParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.FindNodeProfilesParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesParam>.Formatter;
        public static global::Omnius.Xeus.Rpc.FindNodeProfilesParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesParam>.Empty;

        static FindNodeProfilesParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesParam>.Empty = new global::Omnius.Xeus.Rpc.FindNodeProfilesParam(ResourceTag.Empty);
        }

        private readonly int ___hashCode;

        public FindNodeProfilesParam(ResourceTag resourceTag)
        {
            if (resourceTag is null) throw new global::System.ArgumentNullException("resourceTag");

            this.ResourceTag = resourceTag;

            {
                var ___h = new global::System.HashCode();
                if (resourceTag != default) ___h.Add(resourceTag.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public ResourceTag ResourceTag { get; }

        public static global::Omnius.Xeus.Rpc.FindNodeProfilesParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Rpc.FindNodeProfilesParam left, global::Omnius.Xeus.Rpc.FindNodeProfilesParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Rpc.FindNodeProfilesParam left, global::Omnius.Xeus.Rpc.FindNodeProfilesParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Rpc.FindNodeProfilesParam)) return false;
            return this.Equals((global::Omnius.Xeus.Rpc.FindNodeProfilesParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Rpc.FindNodeProfilesParam target)
        {
            if (this.ResourceTag != target.ResourceTag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.FindNodeProfilesParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Rpc.FindNodeProfilesParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                global::Omnius.Xeus.Components.Models.ResourceTag.Formatter.Serialize(ref w, value.ResourceTag, rank + 1);
            }

            public global::Omnius.Xeus.Rpc.FindNodeProfilesParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                ResourceTag p_resourceTag = ResourceTag.Empty;

                {
                    p_resourceTag = global::Omnius.Xeus.Components.Models.ResourceTag.Formatter.Deserialize(ref r, rank + 1);
                }
                return new global::Omnius.Xeus.Rpc.FindNodeProfilesParam(p_resourceTag);
            }
        }
    }
    public readonly partial struct FindNodeProfilesResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.FindNodeProfilesResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesResult>.Formatter;
        public static global::Omnius.Xeus.Rpc.FindNodeProfilesResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesResult>.Empty;

        static FindNodeProfilesResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Rpc.FindNodeProfilesResult>.Empty = new global::Omnius.Xeus.Rpc.FindNodeProfilesResult(global::System.Array.Empty<NodeProfile>());
        }

        private readonly int ___hashCode;

        public static readonly int MaxNodeProfilesCount = 2147483647;

        public FindNodeProfilesResult(NodeProfile[] nodeProfiles)
        {
            if (nodeProfiles is null) throw new global::System.ArgumentNullException("nodeProfiles");
            if (nodeProfiles.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("nodeProfiles");
            foreach (var n in nodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.NodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(nodeProfiles);

            {
                var ___h = new global::System.HashCode();
                foreach (var n in nodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                ___hashCode = ___h.ToHashCode();
            }
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> NodeProfiles { get; }

        public static global::Omnius.Xeus.Rpc.FindNodeProfilesResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Rpc.FindNodeProfilesResult left, global::Omnius.Xeus.Rpc.FindNodeProfilesResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Rpc.FindNodeProfilesResult left, global::Omnius.Xeus.Rpc.FindNodeProfilesResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Rpc.FindNodeProfilesResult)) return false;
            return this.Equals((global::Omnius.Xeus.Rpc.FindNodeProfilesResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Rpc.FindNodeProfilesResult target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfiles, target.NodeProfiles)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Rpc.FindNodeProfilesResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Rpc.FindNodeProfilesResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)value.NodeProfiles.Count);
                foreach (var n in value.NodeProfiles)
                {
                    global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                }
            }

            public global::Omnius.Xeus.Rpc.FindNodeProfilesResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                NodeProfile[] p_nodeProfiles = global::System.Array.Empty<NodeProfile>();

                {
                    var length = r.GetUInt32();
                    p_nodeProfiles = new NodeProfile[length];
                    for (int i = 0; i < p_nodeProfiles.Length; i++)
                    {
                        p_nodeProfiles[i] = global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                    }
                }
                return new global::Omnius.Xeus.Rpc.FindNodeProfilesResult(p_nodeProfiles);
            }
        }
    }

    public interface IXeusService
    {
        global::System.Threading.Tasks.ValueTask GetMyNodeProfileAsync(global::System.Threading.CancellationToken cancellationToken);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult> AddCloudNodeProfilesAsync(global::System.Threading.CancellationToken cancellationToken);
        global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Rpc.FindNodeProfilesResult> FindNodeProfilesAsync(global::Omnius.Xeus.Rpc.FindNodeProfilesParam param, global::System.Threading.CancellationToken cancellationToken);
    }
    public class XeusServiceSender : global::Omnius.Core.AsyncDisposableBase, global::Omnius.Xeus.Rpc.IXeusService
    {
        private readonly global::Omnius.Xeus.Rpc.IXeusService _impl;
        private readonly global::Omnius.Core.Network.Connections.IConnection _connection;
        private readonly global::Omnius.Core.IBytesPool _bytesPool;
        private readonly global::Omnius.Core.RocketPack.Remoting.RocketPackRpc _rpc;
        public XeusServiceSender(global::Omnius.Xeus.Rpc.IXeusService impl, global::Omnius.Core.Network.Connections.IConnection connection, global::Omnius.Core.IBytesPool bytesPool)
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
        public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult> AddCloudNodeProfilesAsync(global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(1, cancellationToken);
            return await stream.CallFunctionAsync<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult>(cancellationToken);
        }
        public async global::System.Threading.Tasks.ValueTask<global::Omnius.Xeus.Rpc.FindNodeProfilesResult> FindNodeProfilesAsync(global::Omnius.Xeus.Rpc.FindNodeProfilesParam param, global::System.Threading.CancellationToken cancellationToken)
        {
            using var stream = await _rpc.ConnectAsync(2, cancellationToken);
            return await stream.CallFunctionAsync<global::Omnius.Xeus.Rpc.FindNodeProfilesParam, global::Omnius.Xeus.Rpc.FindNodeProfilesResult>(param, cancellationToken);
        }
    }
    public class XeusServiceReceiver : global::Omnius.Core.AsyncDisposableBase
    {
        private readonly global::Omnius.Xeus.Rpc.IXeusService _impl;
        private readonly global::Omnius.Core.Network.Connections.IConnection _connection;
        private readonly global::Omnius.Core.IBytesPool _bytesPool;
        private readonly global::Omnius.Core.RocketPack.Remoting.RocketPackRpc _rpc;
        public XeusServiceReceiver(global::Omnius.Xeus.Rpc.IXeusService impl, global::Omnius.Core.Network.Connections.IConnection connection, global::Omnius.Core.IBytesPool bytesPool)
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
                            await stream.ListenFunctionAsync<global::Omnius.Xeus.Rpc.AddCloudNodeProfilesResult>(_impl.AddCloudNodeProfilesAsync, cancellationToken);
                        }
                        break;
                    case 2:
                        {
                            await stream.ListenFunctionAsync<global::Omnius.Xeus.Rpc.FindNodeProfilesParam, global::Omnius.Xeus.Rpc.FindNodeProfilesResult>(_impl.FindNodeProfilesAsync, cancellationToken);
                        }
                        break;
                }
            }
        }
    }

}
