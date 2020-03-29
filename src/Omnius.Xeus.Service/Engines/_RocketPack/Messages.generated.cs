using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Drivers;

#nullable enable

namespace Omnius.Xeus.Service.Engines
{
    public enum ConnectionType : byte
    {
        Connected = 0,
        Accepted = 1,
    }

    public sealed partial class NodeProfile : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeProfile> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Formatter;
        public static NodeProfile Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Empty;

        static NodeProfile()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Empty = new NodeProfile(global::System.Array.Empty<OmniAddress>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxAddressesCount = 32;

        public NodeProfile(OmniAddress[] addresses)
        {
            if (addresses is null) throw new global::System.ArgumentNullException("addresses");
            if (addresses.Length > 32) throw new global::System.ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Addresses = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress>(addresses);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in addresses)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress> Addresses { get; }

        public static NodeProfile Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeProfile? left, NodeProfile? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeProfile? left, NodeProfile? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeProfile)) return false;
            return this.Equals((NodeProfile)other);
        }
        public bool Equals(NodeProfile? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeProfile>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NodeProfile value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Addresses.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Addresses.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Addresses.Count);
                    foreach (var n in value.Addresses)
                    {
                        OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public NodeProfile Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniAddress[] p_addresses = global::System.Array.Empty<OmniAddress>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_addresses = new OmniAddress[length];
                                for (int i = 0; i < p_addresses.Length; i++)
                                {
                                    p_addresses[i] = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new NodeProfile(p_addresses);
            }
        }
    }

    public sealed partial class PublishStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishStorageOptions>.Formatter;
        public static PublishStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishStorageOptions>.Empty;

        static PublishStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishStorageOptions>.Empty = new PublishStorageOptions(0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PublishStorageOptions(uint maxConnectionCount)
        {
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint MaxConnectionCount { get; }

        public static PublishStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishStorageOptions? left, PublishStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishStorageOptions? left, PublishStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishStorageOptions)) return false;
            return this.Equals((PublishStorageOptions)other);
        }
        public bool Equals(PublishStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.MaxConnectionCount);
                }
            }

            public PublishStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_maxConnectionCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new PublishStorageOptions(p_maxConnectionCount);
            }
        }
    }

    public sealed partial class WantStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantStorageOptions>.Formatter;
        public static WantStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantStorageOptions>.Empty;

        static WantStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantStorageOptions>.Empty = new WantStorageOptions(0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantStorageOptions(uint maxConnectionCount)
        {
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint MaxConnectionCount { get; }

        public static WantStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantStorageOptions? left, WantStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantStorageOptions? left, WantStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantStorageOptions)) return false;
            return this.Equals((WantStorageOptions)other);
        }
        public bool Equals(WantStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.MaxConnectionCount);
                }
            }

            public WantStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_maxConnectionCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new WantStorageOptions(p_maxConnectionCount);
            }
        }
    }

    public sealed partial class NodeExplorerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerOptions>.Formatter;
        public static NodeExplorerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerOptions>.Empty;

        static NodeExplorerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerOptions>.Empty = new NodeExplorerOptions(0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public NodeExplorerOptions(uint maxConnectionCount)
        {
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint MaxConnectionCount { get; }

        public static NodeExplorerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeExplorerOptions? left, NodeExplorerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeExplorerOptions? left, NodeExplorerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeExplorerOptions)) return false;
            return this.Equals((NodeExplorerOptions)other);
        }
        public bool Equals(NodeExplorerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NodeExplorerOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.MaxConnectionCount);
                }
            }

            public NodeExplorerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_maxConnectionCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new NodeExplorerOptions(p_maxConnectionCount);
            }
        }
    }

    public sealed partial class BlockExchangerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlockExchangerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerOptions>.Formatter;
        public static BlockExchangerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerOptions>.Empty;

        static BlockExchangerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerOptions>.Empty = new BlockExchangerOptions(0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public BlockExchangerOptions(uint maxConnectionCount)
        {
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint MaxConnectionCount { get; }

        public static BlockExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BlockExchangerOptions? left, BlockExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BlockExchangerOptions? left, BlockExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BlockExchangerOptions)) return false;
            return this.Equals((BlockExchangerOptions)other);
        }
        public bool Equals(BlockExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlockExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in BlockExchangerOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.MaxConnectionCount);
                }
            }

            public BlockExchangerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_maxConnectionCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new BlockExchangerOptions(p_maxConnectionCount);
            }
        }
    }

    public sealed partial class ConsistencyReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConsistencyReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>.Formatter;
        public static ConsistencyReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>.Empty;

        static ConsistencyReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>.Empty = new ConsistencyReport(0, 0, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ConsistencyReport(uint badBlockCount, uint checkedBlockCount, uint totalBlockCount)
        {
            this.BadBlockCount = badBlockCount;
            this.CheckedBlockCount = checkedBlockCount;
            this.TotalBlockCount = totalBlockCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (badBlockCount != default) ___h.Add(badBlockCount.GetHashCode());
                if (checkedBlockCount != default) ___h.Add(checkedBlockCount.GetHashCode());
                if (totalBlockCount != default) ___h.Add(totalBlockCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint BadBlockCount { get; }
        public uint CheckedBlockCount { get; }
        public uint TotalBlockCount { get; }

        public static ConsistencyReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ConsistencyReport? left, ConsistencyReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ConsistencyReport? left, ConsistencyReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ConsistencyReport)) return false;
            return this.Equals((ConsistencyReport)other);
        }
        public bool Equals(ConsistencyReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConsistencyReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ConsistencyReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.BadBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.CheckedBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.TotalBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.BadBlockCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.BadBlockCount);
                }
                if (value.CheckedBlockCount != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.CheckedBlockCount);
                }
                if (value.TotalBlockCount != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.TotalBlockCount);
                }
            }

            public ConsistencyReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_badBlockCount = 0;
                uint p_checkedBlockCount = 0;
                uint p_totalBlockCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_badBlockCount = r.GetUInt32();
                                break;
                            }
                        case 1:
                            {
                                p_checkedBlockCount = r.GetUInt32();
                                break;
                            }
                        case 2:
                            {
                                p_totalBlockCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new ConsistencyReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }

    public sealed partial class ConnectionReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConnectionReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>.Formatter;
        public static ConnectionReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>.Empty;

        static ConnectionReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>.Empty = new ConnectionReport((ConnectionType)0, OmniAddress.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ConnectionReport(ConnectionType type, OmniAddress endpoint)
        {
            if (endpoint is null) throw new global::System.ArgumentNullException("endpoint");

            this.Type = type;
            this.Endpoint = endpoint;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (endpoint != default) ___h.Add(endpoint.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public ConnectionType Type { get; }
        public OmniAddress Endpoint { get; }

        public static ConnectionReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ConnectionReport? left, ConnectionReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ConnectionReport? left, ConnectionReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ConnectionReport)) return false;
            return this.Equals((ConnectionReport)other);
        }
        public bool Equals(ConnectionReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Endpoint != target.Endpoint) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConnectionReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ConnectionReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != (ConnectionType)0)
                    {
                        propertyCount++;
                    }
                    if (value.Endpoint != OmniAddress.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Type != (ConnectionType)0)
                {
                    w.Write((uint)0);
                    w.Write((ulong)value.Type);
                }
                if (value.Endpoint != OmniAddress.Empty)
                {
                    w.Write((uint)1);
                    OmniAddress.Formatter.Serialize(ref w, value.Endpoint, rank + 1);
                }
            }

            public ConnectionReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ConnectionType p_type = (ConnectionType)0;
                OmniAddress p_endpoint = OmniAddress.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_type = (ConnectionType)r.GetUInt64();
                                break;
                            }
                        case 1:
                            {
                                p_endpoint = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new ConnectionReport(p_type, p_endpoint);
            }
        }
    }

    public sealed partial class PublishReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>.Formatter;
        public static PublishReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>.Empty;

        static PublishReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>.Empty = new PublishReport(OmniHash.Empty, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPublishBlocksCount = 1073741824;

        public PublishReport(OmniHash rootHash, OmniHash[] publishBlocks)
        {
            if (publishBlocks is null) throw new global::System.ArgumentNullException("publishBlocks");
            if (publishBlocks.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("publishBlocks");

            this.RootHash = rootHash;
            this.PublishBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(publishBlocks);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                foreach (var n in publishBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public OmniHash RootHash { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> PublishBlocks { get; }

        public static PublishReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishReport? left, PublishReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishReport? left, PublishReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishReport)) return false;
            return this.Equals((PublishReport)other);
        }
        public bool Equals(PublishReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.RootHash != target.RootHash) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PublishBlocks, target.PublishBlocks)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.RootHash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.PublishBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                if (value.PublishBlocks.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.PublishBlocks.Count);
                    foreach (var n in value.PublishBlocks)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public PublishReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_rootHash = OmniHash.Empty;
                OmniHash[] p_publishBlocks = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_rootHash = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_publishBlocks = new OmniHash[length];
                                for (int i = 0; i < p_publishBlocks.Length; i++)
                                {
                                    p_publishBlocks[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new PublishReport(p_rootHash, p_publishBlocks);
            }
        }
    }

    public sealed partial class WantReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>.Formatter;
        public static WantReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>.Empty;

        static WantReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>.Empty = new WantReport(OmniHash.Empty, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxWantBlocksCount = 1073741824;

        public WantReport(OmniHash rootHash, OmniHash[] wantBlocks)
        {
            if (wantBlocks is null) throw new global::System.ArgumentNullException("wantBlocks");
            if (wantBlocks.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("wantBlocks");

            this.RootHash = rootHash;
            this.WantBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(wantBlocks);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                foreach (var n in wantBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public OmniHash RootHash { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> WantBlocks { get; }

        public static WantReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantReport? left, WantReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantReport? left, WantReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantReport)) return false;
            return this.Equals((WantReport)other);
        }
        public bool Equals(WantReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.RootHash != target.RootHash) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantBlocks, target.WantBlocks)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.RootHash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.WantBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                if (value.WantBlocks.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.WantBlocks.Count);
                    foreach (var n in value.WantBlocks)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public WantReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_rootHash = OmniHash.Empty;
                OmniHash[] p_wantBlocks = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_rootHash = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_wantBlocks = new OmniHash[length];
                                for (int i = 0; i < p_wantBlocks.Length; i++)
                                {
                                    p_wantBlocks[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new WantReport(p_rootHash, p_wantBlocks);
            }
        }
    }

}
