using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines;

#nullable enable

namespace Omnius.Xeus.Service.Engines.Internal
{
    internal enum NodeExplorerVersion : sbyte
    {
        Unknown = 0,
        Version1 = 1,
    }

    internal enum BlockExchangerVersion : byte
    {
        Unknown = 0,
        Version1 = 1,
    }

    internal sealed partial class MerkleTreeSection : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MerkleTreeSection>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<MerkleTreeSection> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MerkleTreeSection>.Formatter;
        public static MerkleTreeSection Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MerkleTreeSection>.Empty;

        static MerkleTreeSection()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MerkleTreeSection>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MerkleTreeSection>.Empty = new MerkleTreeSection(0, 0, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxHashesCount = 1073741824;

        public MerkleTreeSection(int depth, ulong length, OmniHash[] hashes)
        {
            if (hashes is null) throw new global::System.ArgumentNullException("hashes");
            if (hashes.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("hashes");

            this.Depth = depth;
            this.Length = length;
            this.Hashes = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(hashes);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (depth != default) ___h.Add(depth.GetHashCode());
                if (length != default) ___h.Add(length.GetHashCode());
                foreach (var n in hashes)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public int Depth { get; }
        public ulong Length { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> Hashes { get; }

        public static MerkleTreeSection Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(MerkleTreeSection? left, MerkleTreeSection? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(MerkleTreeSection? left, MerkleTreeSection? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is MerkleTreeSection)) return false;
            return this.Equals((MerkleTreeSection)other);
        }
        public bool Equals(MerkleTreeSection? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Depth != target.Depth) return false;
            if (this.Length != target.Length) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<MerkleTreeSection>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in MerkleTreeSection value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Depth != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Length != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Hashes.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Depth != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.Depth);
                }
                if (value.Length != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Length);
                }
                if (value.Hashes.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.Hashes.Count);
                    foreach (var n in value.Hashes)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public MerkleTreeSection Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                int p_depth = 0;
                ulong p_length = 0;
                OmniHash[] p_hashes = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_depth = r.GetInt32();
                                break;
                            }
                        case 1:
                            {
                                p_length = r.GetUInt64();
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_hashes = new OmniHash[length];
                                for (int i = 0; i < p_hashes.Length; i++)
                                {
                                    p_hashes[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new MerkleTreeSection(p_depth, p_length, p_hashes);
            }
        }
    }

    internal sealed partial class ContentLocation : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentLocation>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ContentLocation> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentLocation>.Formatter;
        public static ContentLocation Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentLocation>.Empty;

        static ContentLocation()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentLocation>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentLocation>.Empty = new ContentLocation(OmniHash.Empty, global::System.Array.Empty<NodeProfile>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxNodeProfilesCount = 8192;

        public ContentLocation(OmniHash tag, NodeProfile[] nodeProfiles)
        {
            if (nodeProfiles is null) throw new global::System.ArgumentNullException("nodeProfiles");
            if (nodeProfiles.Length > 8192) throw new global::System.ArgumentOutOfRangeException("nodeProfiles");
            foreach (var n in nodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Tag = tag;
            this.NodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(nodeProfiles);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tag != default) ___h.Add(tag.GetHashCode());
                foreach (var n in nodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public OmniHash Tag { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> NodeProfiles { get; }

        public static ContentLocation Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ContentLocation? left, ContentLocation? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ContentLocation? left, ContentLocation? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ContentLocation)) return false;
            return this.Equals((ContentLocation)other);
        }
        public bool Equals(ContentLocation? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfiles, target.NodeProfiles)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ContentLocation>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ContentLocation value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.NodeProfiles.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
                if (value.NodeProfiles.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.NodeProfiles.Count);
                    foreach (var n in value.NodeProfiles)
                    {
                        NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public ContentLocation Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_tag = OmniHash.Empty;
                NodeProfile[] p_nodeProfiles = global::System.Array.Empty<NodeProfile>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_nodeProfiles = new NodeProfile[length];
                                for (int i = 0; i < p_nodeProfiles.Length; i++)
                                {
                                    p_nodeProfiles[i] = NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new ContentLocation(p_tag, p_nodeProfiles);
            }
        }
    }

    internal sealed partial class Block : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Block>, global::System.IDisposable
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<Block> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Block>.Formatter;
        public static Block Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Block>.Empty;

        static Block()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Block>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Block>.Empty = new Block(OmniHash.Empty, global::Omnius.Core.SimpleMemoryOwner<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 4194304;

        public Block(OmniHash tag, global::System.Buffers.IMemoryOwner<byte> value)
        {
            if (value is null) throw new global::System.ArgumentNullException("value");
            if (value.Memory.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("value");

            this.Tag = tag;
            _value = value;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tag != default) ___h.Add(tag.GetHashCode());
                if (!value.Memory.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(value.Memory.Span));
                return ___h.ToHashCode();
            });
        }

        public OmniHash Tag { get; }
        private readonly global::System.Buffers.IMemoryOwner<byte> _value;
        public global::System.ReadOnlyMemory<byte> Value => _value.Memory;

        public static Block Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(Block? left, Block? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(Block? left, Block? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is Block)) return false;
            return this.Equals((Block)other);
        }
        public bool Equals(Block? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Value.Span, target.Value.Span)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<Block>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in Block value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (!value.Value.IsEmpty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.Value.Span);
                }
            }

            public Block Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_tag = OmniHash.Empty;
                global::System.Buffers.IMemoryOwner<byte> p_value = global::Omnius.Core.SimpleMemoryOwner<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_value = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new Block(p_tag, p_value);
            }
        }
    }

    internal sealed partial class BlocksFilter : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlocksFilter>, global::System.IDisposable
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlocksFilter> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlocksFilter>.Formatter;
        public static BlocksFilter Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlocksFilter>.Empty;

        static BlocksFilter()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlocksFilter>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlocksFilter>.Empty = new BlocksFilter(0, global::Omnius.Core.SimpleMemoryOwner<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxGolombCodedSetLength = 4194304;

        public BlocksFilter(int falsePositiveRate, global::System.Buffers.IMemoryOwner<byte> golombCodedSet)
        {
            if (golombCodedSet is null) throw new global::System.ArgumentNullException("golombCodedSet");
            if (golombCodedSet.Memory.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("golombCodedSet");

            this.FalsePositiveRate = falsePositiveRate;
            _golombCodedSet = golombCodedSet;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (falsePositiveRate != default) ___h.Add(falsePositiveRate.GetHashCode());
                if (!golombCodedSet.Memory.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(golombCodedSet.Memory.Span));
                return ___h.ToHashCode();
            });
        }

        public int FalsePositiveRate { get; }
        private readonly global::System.Buffers.IMemoryOwner<byte> _golombCodedSet;
        public global::System.ReadOnlyMemory<byte> GolombCodedSet => _golombCodedSet.Memory;

        public static BlocksFilter Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BlocksFilter? left, BlocksFilter? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BlocksFilter? left, BlocksFilter? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BlocksFilter)) return false;
            return this.Equals((BlocksFilter)other);
        }
        public bool Equals(BlocksFilter? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.FalsePositiveRate != target.FalsePositiveRate) return false;
            if (!global::Omnius.Core.BytesOperations.Equals(this.GolombCodedSet.Span, target.GolombCodedSet.Span)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        public void Dispose()
        {
            _golombCodedSet?.Dispose();
        }

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlocksFilter>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in BlocksFilter value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.FalsePositiveRate != 0)
                    {
                        propertyCount++;
                    }
                    if (!value.GolombCodedSet.IsEmpty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.FalsePositiveRate != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.FalsePositiveRate);
                }
                if (!value.GolombCodedSet.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.GolombCodedSet.Span);
                }
            }

            public BlocksFilter Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                int p_falsePositiveRate = 0;
                global::System.Buffers.IMemoryOwner<byte> p_golombCodedSet = global::Omnius.Core.SimpleMemoryOwner<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_falsePositiveRate = r.GetInt32();
                                break;
                            }
                        case 1:
                            {
                                p_golombCodedSet = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new BlocksFilter(p_falsePositiveRate, p_golombCodedSet);
            }
        }
    }

    internal sealed partial class NodeExplorerHelloMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerHelloMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerHelloMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerHelloMessage>.Formatter;
        public static NodeExplorerHelloMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerHelloMessage>.Empty;

        static NodeExplorerHelloMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerHelloMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerHelloMessage>.Empty = new NodeExplorerHelloMessage(global::System.Array.Empty<NodeExplorerVersion>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxVersionsCount = 32;

        public NodeExplorerHelloMessage(NodeExplorerVersion[] versions)
        {
            if (versions is null) throw new global::System.ArgumentNullException("versions");
            if (versions.Length > 32) throw new global::System.ArgumentOutOfRangeException("versions");

            this.Versions = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeExplorerVersion>(versions);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in versions)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeExplorerVersion> Versions { get; }

        public static NodeExplorerHelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeExplorerHelloMessage? left, NodeExplorerHelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeExplorerHelloMessage? left, NodeExplorerHelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeExplorerHelloMessage)) return false;
            return this.Equals((NodeExplorerHelloMessage)other);
        }
        public bool Equals(NodeExplorerHelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Versions, target.Versions)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerHelloMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NodeExplorerHelloMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Versions.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Versions.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Versions.Count);
                    foreach (var n in value.Versions)
                    {
                        w.Write((long)n);
                    }
                }
            }

            public NodeExplorerHelloMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                NodeExplorerVersion[] p_versions = global::System.Array.Empty<NodeExplorerVersion>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_versions = new NodeExplorerVersion[length];
                                for (int i = 0; i < p_versions.Length; i++)
                                {
                                    p_versions[i] = (NodeExplorerVersion)r.GetInt64();
                                }
                                break;
                            }
                    }
                }

                return new NodeExplorerHelloMessage(p_versions);
            }
        }
    }

    internal sealed partial class NodeExplorerProfileMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerProfileMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerProfileMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerProfileMessage>.Formatter;
        public static NodeExplorerProfileMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerProfileMessage>.Empty;

        static NodeExplorerProfileMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerProfileMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerProfileMessage>.Empty = new NodeExplorerProfileMessage(global::System.ReadOnlyMemory<byte>.Empty, NodeProfile.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxIdLength = 32;

        public NodeExplorerProfileMessage(global::System.ReadOnlyMemory<byte> id, NodeProfile nodeProfile)
        {
            if (id.Length > 32) throw new global::System.ArgumentOutOfRangeException("id");
            if (nodeProfile is null) throw new global::System.ArgumentNullException("nodeProfile");

            this.Id = id;
            this.NodeProfile = nodeProfile;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (!id.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(id.Span));
                if (nodeProfile != default) ___h.Add(nodeProfile.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public global::System.ReadOnlyMemory<byte> Id { get; }
        public NodeProfile NodeProfile { get; }

        public static NodeExplorerProfileMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeExplorerProfileMessage? left, NodeExplorerProfileMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeExplorerProfileMessage? left, NodeExplorerProfileMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeExplorerProfileMessage)) return false;
            return this.Equals((NodeExplorerProfileMessage)other);
        }
        public bool Equals(NodeExplorerProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Id.Span, target.Id.Span)) return false;
            if (this.NodeProfile != target.NodeProfile) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerProfileMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NodeExplorerProfileMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (!value.Id.IsEmpty)
                    {
                        propertyCount++;
                    }
                    if (value.NodeProfile != NodeProfile.Empty)
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
                if (value.NodeProfile != NodeProfile.Empty)
                {
                    w.Write((uint)1);
                    NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
                }
            }

            public NodeExplorerProfileMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.ReadOnlyMemory<byte> p_id = global::System.ReadOnlyMemory<byte>.Empty;
                NodeProfile p_nodeProfile = NodeProfile.Empty;

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
                                p_nodeProfile = NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new NodeExplorerProfileMessage(p_id, p_nodeProfile);
            }
        }
    }

    internal sealed partial class NodeExplorerDataMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerDataMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerDataMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerDataMessage>.Formatter;
        public static NodeExplorerDataMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerDataMessage>.Empty;

        static NodeExplorerDataMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerDataMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerDataMessage>.Empty = new NodeExplorerDataMessage(global::System.Array.Empty<NodeProfile>(), global::System.Array.Empty<ContentLocation>(), global::System.Array.Empty<OmniHash>(), global::System.Array.Empty<ContentLocation>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPushNodeProfilesCount = 256;
        public static readonly int MaxPushContentLocationsCount = 256;
        public static readonly int MaxWantContentLocationsCount = 256;
        public static readonly int MaxGiveContentLocationsCount = 256;

        public NodeExplorerDataMessage(NodeProfile[] pushNodeProfiles, ContentLocation[] pushContentLocations, OmniHash[] wantContentLocations, ContentLocation[] giveContentLocations)
        {
            if (pushNodeProfiles is null) throw new global::System.ArgumentNullException("pushNodeProfiles");
            if (pushNodeProfiles.Length > 256) throw new global::System.ArgumentOutOfRangeException("pushNodeProfiles");
            foreach (var n in pushNodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (pushContentLocations is null) throw new global::System.ArgumentNullException("pushContentLocations");
            if (pushContentLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("pushContentLocations");
            foreach (var n in pushContentLocations)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (wantContentLocations is null) throw new global::System.ArgumentNullException("wantContentLocations");
            if (wantContentLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("wantContentLocations");
            if (giveContentLocations is null) throw new global::System.ArgumentNullException("giveContentLocations");
            if (giveContentLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("giveContentLocations");
            foreach (var n in giveContentLocations)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.PushNodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(pushNodeProfiles);
            this.PushContentLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<ContentLocation>(pushContentLocations);
            this.WantContentLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(wantContentLocations);
            this.GiveContentLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<ContentLocation>(giveContentLocations);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in pushNodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in pushContentLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in wantContentLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in giveContentLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> PushNodeProfiles { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<ContentLocation> PushContentLocations { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> WantContentLocations { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<ContentLocation> GiveContentLocations { get; }

        public static NodeExplorerDataMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeExplorerDataMessage? left, NodeExplorerDataMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeExplorerDataMessage? left, NodeExplorerDataMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeExplorerDataMessage)) return false;
            return this.Equals((NodeExplorerDataMessage)other);
        }
        public bool Equals(NodeExplorerDataMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushNodeProfiles, target.PushNodeProfiles)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushContentLocations, target.PushContentLocations)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantContentLocations, target.WantContentLocations)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.GiveContentLocations, target.GiveContentLocations)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerDataMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NodeExplorerDataMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.PushNodeProfiles.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.PushContentLocations.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.WantContentLocations.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.GiveContentLocations.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.PushNodeProfiles.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.PushNodeProfiles.Count);
                    foreach (var n in value.PushNodeProfiles)
                    {
                        NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.PushContentLocations.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.PushContentLocations.Count);
                    foreach (var n in value.PushContentLocations)
                    {
                        ContentLocation.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.WantContentLocations.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.WantContentLocations.Count);
                    foreach (var n in value.WantContentLocations)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.GiveContentLocations.Count != 0)
                {
                    w.Write((uint)3);
                    w.Write((uint)value.GiveContentLocations.Count);
                    foreach (var n in value.GiveContentLocations)
                    {
                        ContentLocation.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public NodeExplorerDataMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                NodeProfile[] p_pushNodeProfiles = global::System.Array.Empty<NodeProfile>();
                ContentLocation[] p_pushContentLocations = global::System.Array.Empty<ContentLocation>();
                OmniHash[] p_wantContentLocations = global::System.Array.Empty<OmniHash>();
                ContentLocation[] p_giveContentLocations = global::System.Array.Empty<ContentLocation>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_pushNodeProfiles = new NodeProfile[length];
                                for (int i = 0; i < p_pushNodeProfiles.Length; i++)
                                {
                                    p_pushNodeProfiles[i] = NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_pushContentLocations = new ContentLocation[length];
                                for (int i = 0; i < p_pushContentLocations.Length; i++)
                                {
                                    p_pushContentLocations[i] = ContentLocation.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_wantContentLocations = new OmniHash[length];
                                for (int i = 0; i < p_wantContentLocations.Length; i++)
                                {
                                    p_wantContentLocations[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 3:
                            {
                                var length = r.GetUInt32();
                                p_giveContentLocations = new ContentLocation[length];
                                for (int i = 0; i < p_giveContentLocations.Length; i++)
                                {
                                    p_giveContentLocations[i] = ContentLocation.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new NodeExplorerDataMessage(p_pushNodeProfiles, p_pushContentLocations, p_wantContentLocations, p_giveContentLocations);
            }
        }
    }

    internal sealed partial class BlockExchangerHelloMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerHelloMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlockExchangerHelloMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerHelloMessage>.Formatter;
        public static BlockExchangerHelloMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerHelloMessage>.Empty;

        static BlockExchangerHelloMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerHelloMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerHelloMessage>.Empty = new BlockExchangerHelloMessage(global::System.Array.Empty<BlockExchangerVersion>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxVersionsCount = 32;

        public BlockExchangerHelloMessage(BlockExchangerVersion[] versions)
        {
            if (versions is null) throw new global::System.ArgumentNullException("versions");
            if (versions.Length > 32) throw new global::System.ArgumentOutOfRangeException("versions");

            this.Versions = new global::Omnius.Core.Collections.ReadOnlyListSlim<BlockExchangerVersion>(versions);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in versions)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<BlockExchangerVersion> Versions { get; }

        public static BlockExchangerHelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BlockExchangerHelloMessage? left, BlockExchangerHelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BlockExchangerHelloMessage? left, BlockExchangerHelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BlockExchangerHelloMessage)) return false;
            return this.Equals((BlockExchangerHelloMessage)other);
        }
        public bool Equals(BlockExchangerHelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Versions, target.Versions)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlockExchangerHelloMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in BlockExchangerHelloMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Versions.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Versions.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Versions.Count);
                    foreach (var n in value.Versions)
                    {
                        w.Write((ulong)n);
                    }
                }
            }

            public BlockExchangerHelloMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                BlockExchangerVersion[] p_versions = global::System.Array.Empty<BlockExchangerVersion>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_versions = new BlockExchangerVersion[length];
                                for (int i = 0; i < p_versions.Length; i++)
                                {
                                    p_versions[i] = (BlockExchangerVersion)r.GetUInt64();
                                }
                                break;
                            }
                    }
                }

                return new BlockExchangerHelloMessage(p_versions);
            }
        }
    }

    internal sealed partial class BlockExchangerProfileMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerProfileMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlockExchangerProfileMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerProfileMessage>.Formatter;
        public static BlockExchangerProfileMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerProfileMessage>.Empty;

        static BlockExchangerProfileMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerProfileMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerProfileMessage>.Empty = new BlockExchangerProfileMessage(OmniHash.Empty, NodeProfile.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public BlockExchangerProfileMessage(OmniHash tag, NodeProfile nodeProfile)
        {
            if (nodeProfile is null) throw new global::System.ArgumentNullException("nodeProfile");

            this.Tag = tag;
            this.NodeProfile = nodeProfile;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tag != default) ___h.Add(tag.GetHashCode());
                if (nodeProfile != default) ___h.Add(nodeProfile.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public OmniHash Tag { get; }
        public NodeProfile NodeProfile { get; }

        public static BlockExchangerProfileMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BlockExchangerProfileMessage? left, BlockExchangerProfileMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BlockExchangerProfileMessage? left, BlockExchangerProfileMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BlockExchangerProfileMessage)) return false;
            return this.Equals((BlockExchangerProfileMessage)other);
        }
        public bool Equals(BlockExchangerProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;
            if (this.NodeProfile != target.NodeProfile) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlockExchangerProfileMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in BlockExchangerProfileMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.NodeProfile != NodeProfile.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
                if (value.NodeProfile != NodeProfile.Empty)
                {
                    w.Write((uint)1);
                    NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
                }
            }

            public BlockExchangerProfileMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_tag = OmniHash.Empty;
                NodeProfile p_nodeProfile = NodeProfile.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_nodeProfile = NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new BlockExchangerProfileMessage(p_tag, p_nodeProfile);
            }
        }
    }

    internal sealed partial class BlockExchangerDataMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerDataMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlockExchangerDataMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerDataMessage>.Formatter;
        public static BlockExchangerDataMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerDataMessage>.Empty;

        static BlockExchangerDataMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerDataMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<BlockExchangerDataMessage>.Empty = new BlockExchangerDataMessage(global::System.Array.Empty<NodeProfile>(), global::System.Array.Empty<OmniHash>(), global::System.Array.Empty<OmniHash>(), BlocksFilter.Empty, global::System.Array.Empty<Block>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPushNodeProfilesCount = 256;
        public static readonly int MaxWantBlocksCount = 256;
        public static readonly int MaxCancelBlocksCount = 256;
        public static readonly int MaxGiveBlocksCount = 8;

        public BlockExchangerDataMessage(NodeProfile[] pushNodeProfiles, OmniHash[] wantBlocks, OmniHash[] cancelBlocks, BlocksFilter ownedBlocksFilter, Block[] giveBlocks)
        {
            if (pushNodeProfiles is null) throw new global::System.ArgumentNullException("pushNodeProfiles");
            if (pushNodeProfiles.Length > 256) throw new global::System.ArgumentOutOfRangeException("pushNodeProfiles");
            foreach (var n in pushNodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (wantBlocks is null) throw new global::System.ArgumentNullException("wantBlocks");
            if (wantBlocks.Length > 256) throw new global::System.ArgumentOutOfRangeException("wantBlocks");
            if (cancelBlocks is null) throw new global::System.ArgumentNullException("cancelBlocks");
            if (cancelBlocks.Length > 256) throw new global::System.ArgumentOutOfRangeException("cancelBlocks");
            if (ownedBlocksFilter is null) throw new global::System.ArgumentNullException("ownedBlocksFilter");
            if (giveBlocks is null) throw new global::System.ArgumentNullException("giveBlocks");
            if (giveBlocks.Length > 8) throw new global::System.ArgumentOutOfRangeException("giveBlocks");
            foreach (var n in giveBlocks)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.PushNodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(pushNodeProfiles);
            this.WantBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(wantBlocks);
            this.CancelBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(cancelBlocks);
            this.OwnedBlocksFilter = ownedBlocksFilter;
            this.GiveBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<Block>(giveBlocks);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in pushNodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in wantBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in cancelBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                if (ownedBlocksFilter != default) ___h.Add(ownedBlocksFilter.GetHashCode());
                foreach (var n in giveBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> PushNodeProfiles { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> WantBlocks { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> CancelBlocks { get; }
        public BlocksFilter OwnedBlocksFilter { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<Block> GiveBlocks { get; }

        public static BlockExchangerDataMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BlockExchangerDataMessage? left, BlockExchangerDataMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BlockExchangerDataMessage? left, BlockExchangerDataMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BlockExchangerDataMessage)) return false;
            return this.Equals((BlockExchangerDataMessage)other);
        }
        public bool Equals(BlockExchangerDataMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushNodeProfiles, target.PushNodeProfiles)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantBlocks, target.WantBlocks)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.CancelBlocks, target.CancelBlocks)) return false;
            if (this.OwnedBlocksFilter != target.OwnedBlocksFilter) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.GiveBlocks, target.GiveBlocks)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<BlockExchangerDataMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in BlockExchangerDataMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.PushNodeProfiles.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.WantBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.CancelBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.OwnedBlocksFilter != BlocksFilter.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.GiveBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.PushNodeProfiles.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.PushNodeProfiles.Count);
                    foreach (var n in value.PushNodeProfiles)
                    {
                        NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
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
                if (value.CancelBlocks.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.CancelBlocks.Count);
                    foreach (var n in value.CancelBlocks)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.OwnedBlocksFilter != BlocksFilter.Empty)
                {
                    w.Write((uint)3);
                    BlocksFilter.Formatter.Serialize(ref w, value.OwnedBlocksFilter, rank + 1);
                }
                if (value.GiveBlocks.Count != 0)
                {
                    w.Write((uint)4);
                    w.Write((uint)value.GiveBlocks.Count);
                    foreach (var n in value.GiveBlocks)
                    {
                        Block.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public BlockExchangerDataMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                NodeProfile[] p_pushNodeProfiles = global::System.Array.Empty<NodeProfile>();
                OmniHash[] p_wantBlocks = global::System.Array.Empty<OmniHash>();
                OmniHash[] p_cancelBlocks = global::System.Array.Empty<OmniHash>();
                BlocksFilter p_ownedBlocksFilter = BlocksFilter.Empty;
                Block[] p_giveBlocks = global::System.Array.Empty<Block>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_pushNodeProfiles = new NodeProfile[length];
                                for (int i = 0; i < p_pushNodeProfiles.Length; i++)
                                {
                                    p_pushNodeProfiles[i] = NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
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
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_cancelBlocks = new OmniHash[length];
                                for (int i = 0; i < p_cancelBlocks.Length; i++)
                                {
                                    p_cancelBlocks[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 3:
                            {
                                p_ownedBlocksFilter = BlocksFilter.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 4:
                            {
                                var length = r.GetUInt32();
                                p_giveBlocks = new Block[length];
                                for (int i = 0; i < p_giveBlocks.Length; i++)
                                {
                                    p_giveBlocks[i] = Block.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new BlockExchangerDataMessage(p_pushNodeProfiles, p_wantBlocks, p_cancelBlocks, p_ownedBlocksFilter, p_giveBlocks);
            }
        }
    }

    internal sealed partial class NodeExplorerConfig : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerConfig>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerConfig> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerConfig>.Formatter;
        public static NodeExplorerConfig Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerConfig>.Empty;

        static NodeExplorerConfig()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerConfig>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeExplorerConfig>.Empty = new NodeExplorerConfig(new global::System.Collections.Generic.Dictionary<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxNodeProfileMapCount = 1073741824;

        public NodeExplorerConfig(global::System.Collections.Generic.Dictionary<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp> nodeProfileMap)
        {
            if (nodeProfileMap is null) throw new global::System.ArgumentNullException("nodeProfileMap");
            if (nodeProfileMap.Count > 1073741824) throw new global::System.ArgumentOutOfRangeException("nodeProfileMap");
            foreach (var n in nodeProfileMap)
            {
                if (n.Key is null) throw new global::System.ArgumentNullException("n.Key");
            }

            this.NodeProfileMap = new global::Omnius.Core.Collections.ReadOnlyDictionarySlim<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp>(nodeProfileMap);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in nodeProfileMap)
                {
                    if (n.Key != default) ___h.Add(n.Key.GetHashCode());
                    if (n.Value != default) ___h.Add(n.Value.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyDictionarySlim<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp> NodeProfileMap { get; }

        public static NodeExplorerConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeExplorerConfig? left, NodeExplorerConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeExplorerConfig? left, NodeExplorerConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeExplorerConfig)) return false;
            return this.Equals((NodeExplorerConfig)other);
        }
        public bool Equals(NodeExplorerConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfileMap, target.NodeProfileMap)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeExplorerConfig>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NodeExplorerConfig value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.NodeProfileMap.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.NodeProfileMap.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.NodeProfileMap.Count);
                    foreach (var n in value.NodeProfileMap)
                    {
                        NodeProfile.Formatter.Serialize(ref w, n.Key, rank + 1);
                        w.Write(n.Value);
                    }
                }
            }

            public NodeExplorerConfig Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.Collections.Generic.Dictionary<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp> p_nodeProfileMap = new global::System.Collections.Generic.Dictionary<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_nodeProfileMap = new global::System.Collections.Generic.Dictionary<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp>();
                                NodeProfile t_key = NodeProfile.Empty;
                                global::Omnius.Core.Serialization.RocketPack.Timestamp t_value = global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                    t_value = r.GetTimestamp();
                                    p_nodeProfileMap[t_key] = t_value;
                                }
                                break;
                            }
                    }
                }

                return new NodeExplorerConfig(p_nodeProfileMap);
            }
        }
    }

}
