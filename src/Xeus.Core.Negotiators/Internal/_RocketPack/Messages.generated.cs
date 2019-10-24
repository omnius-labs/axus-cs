using Omnix.Cryptography;
using Omnix.Network;
using Xeus.Core;

#nullable enable

namespace Xeus.Core.Negotiators.Internal
{
    internal enum ProtocolVersion : byte
    {
        Version1 = 1,
    }

    internal sealed partial class HelloMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<HelloMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<HelloMessage> Formatter { get; }
        public static HelloMessage Empty { get; }

        static HelloMessage()
        {
            HelloMessage.Formatter = new ___CustomFormatter();
            HelloMessage.Empty = new HelloMessage(global::System.Array.Empty<ProtocolVersion>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxProtocolVersionsCount = 32;

        public HelloMessage(ProtocolVersion[] protocolVersions)
        {
            if (protocolVersions is null) throw new global::System.ArgumentNullException("protocolVersions");
            if (protocolVersions.Length > 32) throw new global::System.ArgumentOutOfRangeException("protocolVersions");

            this.ProtocolVersions = new global::Omnix.Collections.ReadOnlyListSlim<ProtocolVersion>(protocolVersions);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in protocolVersions)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.Collections.ReadOnlyListSlim<ProtocolVersion> ProtocolVersions { get; }

        public static HelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(HelloMessage? left, HelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(HelloMessage? left, HelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is HelloMessage)) return false;
            return this.Equals((HelloMessage)other);
        }
        public bool Equals(HelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ProtocolVersions, target.ProtocolVersions)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<HelloMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in HelloMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ProtocolVersions.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ProtocolVersions.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.ProtocolVersions.Count);
                    foreach (var n in value.ProtocolVersions)
                    {
                        w.Write((ulong)n);
                    }
                }
            }

            public HelloMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ProtocolVersion[] p_protocolVersions = global::System.Array.Empty<ProtocolVersion>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_protocolVersions = new ProtocolVersion[length];
                                for (int i = 0; i < p_protocolVersions.Length; i++)
                                {
                                    p_protocolVersions[i] = (ProtocolVersion)r.GetUInt64();
                                }
                                break;
                            }
                    }
                }

                return new HelloMessage(p_protocolVersions);
            }
        }
    }

    internal sealed partial class ProfileMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<ProfileMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ProfileMessage> Formatter { get; }
        public static ProfileMessage Empty { get; }

        static ProfileMessage()
        {
            ProfileMessage.Formatter = new ___CustomFormatter();
            ProfileMessage.Empty = new ProfileMessage(global::System.ReadOnlyMemory<byte>.Empty, OmniAddress.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxIdLength = 32;

        public ProfileMessage(global::System.ReadOnlyMemory<byte> id, OmniAddress address)
        {
            if (id.Length > 32) throw new global::System.ArgumentOutOfRangeException("id");
            if (address is null) throw new global::System.ArgumentNullException("address");

            this.Id = id;
            this.Address = address;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (!id.IsEmpty) ___h.Add(global::Omnix.Base.Helpers.ObjectHelper.GetHashCode(id.Span));
                if (address != default) ___h.Add(address.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public global::System.ReadOnlyMemory<byte> Id { get; }
        public OmniAddress Address { get; }

        public static ProfileMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ProfileMessage? left, ProfileMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ProfileMessage? left, ProfileMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ProfileMessage)) return false;
            return this.Equals((ProfileMessage)other);
        }
        public bool Equals(ProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.BytesOperations.SequenceEqual(this.Id.Span, target.Id.Span)) return false;
            if (this.Address != target.Address) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ProfileMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in ProfileMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (!value.Id.IsEmpty)
                    {
                        propertyCount++;
                    }
                    if (value.Address != OmniAddress.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (!value.Id.IsEmpty)
                {
                    w.Write((uint)0);
                    w.Write(value.Id.Span);
                }
                if (value.Address != OmniAddress.Empty)
                {
                    w.Write((uint)1);
                    OmniAddress.Formatter.Serialize(ref w, value.Address, rank + 1);
                }
            }

            public ProfileMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.ReadOnlyMemory<byte> p_id = global::System.ReadOnlyMemory<byte>.Empty;
                OmniAddress p_address = OmniAddress.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_id = r.GetMemory(32);
                                break;
                            }
                        case 1:
                            {
                                p_address = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new ProfileMessage(p_id, p_address);
            }
        }
    }

    internal sealed partial class NodeAddressesMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<NodeAddressesMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<NodeAddressesMessage> Formatter { get; }
        public static NodeAddressesMessage Empty { get; }

        static NodeAddressesMessage()
        {
            NodeAddressesMessage.Formatter = new ___CustomFormatter();
            NodeAddressesMessage.Empty = new NodeAddressesMessage(global::System.Array.Empty<OmniAddress>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValuesCount = 256;

        public NodeAddressesMessage(OmniAddress[] values)
        {
            if (values is null) throw new global::System.ArgumentNullException("values");
            if (values.Length > 256) throw new global::System.ArgumentOutOfRangeException("values");
            foreach (var n in values)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Values = new global::Omnix.Collections.ReadOnlyListSlim<OmniAddress>(values);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in values)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.Collections.ReadOnlyListSlim<OmniAddress> Values { get; }

        public static NodeAddressesMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeAddressesMessage? left, NodeAddressesMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeAddressesMessage? left, NodeAddressesMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeAddressesMessage)) return false;
            return this.Equals((NodeAddressesMessage)other);
        }
        public bool Equals(NodeAddressesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Values, target.Values)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<NodeAddressesMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in NodeAddressesMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Values.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Values.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Values.Count);
                    foreach (var n in value.Values)
                    {
                        OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public NodeAddressesMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniAddress[] p_values = global::System.Array.Empty<OmniAddress>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_values = new OmniAddress[length];
                                for (int i = 0; i < p_values.Length; i++)
                                {
                                    p_values[i] = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new NodeAddressesMessage(p_values);
            }
        }
    }

    internal sealed partial class WantBlocksMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<WantBlocksMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBlocksMessage> Formatter { get; }
        public static WantBlocksMessage Empty { get; }

        static WantBlocksMessage()
        {
            WantBlocksMessage.Formatter = new ___CustomFormatter();
            WantBlocksMessage.Empty = new WantBlocksMessage(global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantBlocksMessage(OmniHash[] parameters)
        {
            if (parameters is null) throw new global::System.ArgumentNullException("parameters");
            if (parameters.Length > 8192) throw new global::System.ArgumentOutOfRangeException("parameters");

            this.Parameters = new global::Omnix.Collections.ReadOnlyListSlim<OmniHash>(parameters);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in parameters)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.Collections.ReadOnlyListSlim<OmniHash> Parameters { get; }

        public static WantBlocksMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantBlocksMessage? left, WantBlocksMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantBlocksMessage? left, WantBlocksMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantBlocksMessage)) return false;
            return this.Equals((WantBlocksMessage)other);
        }
        public bool Equals(WantBlocksMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBlocksMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in WantBlocksMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Parameters.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Parameters.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Parameters.Count);
                    foreach (var n in value.Parameters)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public WantBlocksMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash[] p_parameters = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_parameters = new OmniHash[length];
                                for (int i = 0; i < p_parameters.Length; i++)
                                {
                                    p_parameters[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new WantBlocksMessage(p_parameters);
            }
        }
    }

    internal sealed partial class CancelBlocksMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<CancelBlocksMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<CancelBlocksMessage> Formatter { get; }
        public static CancelBlocksMessage Empty { get; }

        static CancelBlocksMessage()
        {
            CancelBlocksMessage.Formatter = new ___CustomFormatter();
            CancelBlocksMessage.Empty = new CancelBlocksMessage(global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxParametersCount = 8192;

        public CancelBlocksMessage(OmniHash[] parameters)
        {
            if (parameters is null) throw new global::System.ArgumentNullException("parameters");
            if (parameters.Length > 8192) throw new global::System.ArgumentOutOfRangeException("parameters");

            this.Parameters = new global::Omnix.Collections.ReadOnlyListSlim<OmniHash>(parameters);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in parameters)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.Collections.ReadOnlyListSlim<OmniHash> Parameters { get; }

        public static CancelBlocksMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(CancelBlocksMessage? left, CancelBlocksMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(CancelBlocksMessage? left, CancelBlocksMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is CancelBlocksMessage)) return false;
            return this.Equals((CancelBlocksMessage)other);
        }
        public bool Equals(CancelBlocksMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<CancelBlocksMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in CancelBlocksMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Parameters.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Parameters.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Parameters.Count);
                    foreach (var n in value.Parameters)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public CancelBlocksMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash[] p_parameters = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_parameters = new OmniHash[length];
                                for (int i = 0; i < p_parameters.Length; i++)
                                {
                                    p_parameters[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new CancelBlocksMessage(p_parameters);
            }
        }
    }

    internal sealed partial class BlocksMessages : global::Omnix.Serialization.RocketPack.IRocketPackMessage<BlocksMessages>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BlocksMessages> Formatter { get; }
        public static BlocksMessages Empty { get; }

        static BlocksMessages()
        {
            BlocksMessages.Formatter = new ___CustomFormatter();
            BlocksMessages.Empty = new BlocksMessages((OmniHashAlgorithmType)0, global::System.Array.Empty<global::System.Buffers.IMemoryOwner<byte>>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxBlocksCount = 32;

        public BlocksMessages(OmniHashAlgorithmType hashAlgorithm, global::System.Buffers.IMemoryOwner<byte>[] blocks)
        {
            if (blocks is null) throw new global::System.ArgumentNullException("blocks");
            if (blocks.Length > 32) throw new global::System.ArgumentOutOfRangeException("blocks");
            foreach (var n in blocks)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
                if (n.Memory.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("n");
            }

            this.HashAlgorithm = hashAlgorithm;
            this.Blocks = new global::Omnix.Collections.ReadOnlyListSlim<global::System.Buffers.IMemoryOwner<byte>>(blocks);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (hashAlgorithm != default) ___h.Add(hashAlgorithm.GetHashCode());
                foreach (var n in blocks)
                {
                    if (!n.Memory.IsEmpty) ___h.Add(global::Omnix.Base.Helpers.ObjectHelper.GetHashCode(n.Memory.Span));
                }
                return ___h.ToHashCode();
            });
        }

        public OmniHashAlgorithmType HashAlgorithm { get; }
        public global::Omnix.Collections.ReadOnlyListSlim<global::System.Buffers.IMemoryOwner<byte>> Blocks { get; }

        public static BlocksMessages Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BlocksMessages? left, BlocksMessages? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BlocksMessages? left, BlocksMessages? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BlocksMessages)) return false;
            return this.Equals((BlocksMessages)other);
        }
        public bool Equals(BlocksMessages? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.HashAlgorithm != target.HashAlgorithm) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Blocks, target.Blocks)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BlocksMessages>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in BlocksMessages value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.HashAlgorithm != (OmniHashAlgorithmType)0)
                    {
                        propertyCount++;
                    }
                    if (value.Blocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.HashAlgorithm != (OmniHashAlgorithmType)0)
                {
                    w.Write((uint)0);
                    w.Write((ulong)value.HashAlgorithm);
                }
                if (value.Blocks.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.Blocks.Count);
                    foreach (var n in value.Blocks)
                    {
                        w.Write(n.Span);
                    }
                }
            }

            public BlocksMessages Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHashAlgorithmType p_hashAlgorithm = (OmniHashAlgorithmType)0;
                global::System.Buffers.IMemoryOwner<byte>[] p_blocks = global::System.Array.Empty<global::System.Buffers.IMemoryOwner<byte>>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_hashAlgorithm = (OmniHashAlgorithmType)r.GetUInt64();
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_blocks = new global::System.Buffers.IMemoryOwner<byte>[length];
                                for (int i = 0; i < p_blocks.Length; i++)
                                {
                                    p_blocks[i] = r.GetRecyclableMemory(4194304);
                                }
                                break;
                            }
                    }
                }

                return new BlocksMessages(p_hashAlgorithm, p_blocks);
            }
        }
    }

}
