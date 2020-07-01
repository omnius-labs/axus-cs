using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Drivers;
using Omnius.Xeus.Service.Engines;

#nullable enable

namespace Omnius.Xeus.Service.Engines.Internal
{
    internal enum NodeFinderVersion : sbyte
    {
        Unknown = 0,
        Version1 = 1,
    }

    internal enum ContentExchangerVersion : byte
    {
        Unknown = 0,
        Version1 = 1,
    }

    internal sealed partial class MerkleTreeSection : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection>.Empty;

        static MerkleTreeSection()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection(0, 0, global::System.Array.Empty<OmniHash>());
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

        public static global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection? left, global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection? left, global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Depth != target.Depth) return false;
            if (this.Length != target.Length) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection value, in int rank)
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
                        global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    p_hashes[i] = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.MerkleTreeSection(p_depth, p_length, p_hashes);
            }
        }
    }

    internal sealed partial class Location : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.Location>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.Location> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.Location>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.Location Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.Location>.Empty;

        static Location()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.Location>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.Location>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.Location(Tag.Empty, global::System.Array.Empty<NodeProfile>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxNodeProfilesCount = 8192;

        public Location(Tag tag, NodeProfile[] nodeProfiles)
        {
            if (tag is null) throw new global::System.ArgumentNullException("tag");
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

        public Tag Tag { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> NodeProfiles { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.Location Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.Location? left, global::Omnius.Xeus.Service.Engines.Internal.Location? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.Location? left, global::Omnius.Xeus.Service.Engines.Internal.Location? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.Location)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.Location)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.Location? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfiles, target.NodeProfiles)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.Location>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.Location value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != Tag.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.NodeProfiles.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != Tag.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Xeus.Service.Engines.Tag.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
                if (value.NodeProfiles.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.NodeProfiles.Count);
                    foreach (var n in value.NodeProfiles)
                    {
                        global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.Location Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                Tag p_tag = Tag.Empty;
                NodeProfile[] p_nodeProfiles = global::System.Array.Empty<NodeProfile>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = global::Omnius.Xeus.Service.Engines.Tag.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_nodeProfiles = new NodeProfile[length];
                                for (int i = 0; i < p_nodeProfiles.Length; i++)
                                {
                                    p_nodeProfiles[i] = global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.Location(p_tag, p_nodeProfiles);
            }
        }
    }

    internal sealed partial class ContentBlock : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlock>, global::System.IDisposable
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentBlock> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlock>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentBlock Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlock>.Empty;

        static ContentBlock()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlock>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlock>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentBlock(OmniHash.Empty, global::Omnius.Core.SimpleMemoryOwner<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 4194304;

        public ContentBlock(OmniHash tag, global::System.Buffers.IMemoryOwner<byte> value)
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

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentBlock Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentBlock? left, global::Omnius.Xeus.Service.Engines.Internal.ContentBlock? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentBlock? left, global::Omnius.Xeus.Service.Engines.Internal.ContentBlock? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentBlock)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentBlock)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentBlock? target)
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

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentBlock>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentBlock value, in int rank)
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
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.Value.Span);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ContentBlock Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tag = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_value = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentBlock(p_tag, p_value);
            }
        }
    }

    internal sealed partial class ContentBlockFlags : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags>, global::System.IDisposable
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags>.Empty;

        static ContentBlockFlags()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags(0, global::Omnius.Core.SimpleMemoryOwner<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxFlagsLength = 4194304;

        public ContentBlockFlags(int depth, global::System.Buffers.IMemoryOwner<byte> flags)
        {
            if (flags is null) throw new global::System.ArgumentNullException("flags");
            if (flags.Memory.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("flags");

            this.Depth = depth;
            _flags = flags;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (depth != default) ___h.Add(depth.GetHashCode());
                if (!flags.Memory.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(flags.Memory.Span));
                return ___h.ToHashCode();
            });
        }

        public int Depth { get; }
        private readonly global::System.Buffers.IMemoryOwner<byte> _flags;
        public global::System.ReadOnlyMemory<byte> Flags => _flags.Memory;

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags? left, global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags? left, global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Depth != target.Depth) return false;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Flags.Span, target.Flags.Span)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        public void Dispose()
        {
            _flags?.Dispose();
        }

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Depth != 0)
                    {
                        propertyCount++;
                    }
                    if (!value.Flags.IsEmpty)
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
                if (!value.Flags.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.Flags.Span);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                int p_depth = 0;
                global::System.Buffers.IMemoryOwner<byte> p_flags = global::Omnius.Core.SimpleMemoryOwner<byte>.Empty;

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
                                p_flags = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags(p_depth, p_flags);
            }
        }
    }

    internal sealed partial class NodeFinderHelloMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage>.Empty;

        static NodeFinderHelloMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage(global::System.Array.Empty<NodeFinderVersion>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxVersionsCount = 32;

        public NodeFinderHelloMessage(NodeFinderVersion[] versions)
        {
            if (versions is null) throw new global::System.ArgumentNullException("versions");
            if (versions.Length > 32) throw new global::System.ArgumentOutOfRangeException("versions");

            this.Versions = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeFinderVersion>(versions);

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

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeFinderVersion> Versions { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage? left, global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage? left, global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Versions, target.Versions)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                NodeFinderVersion[] p_versions = global::System.Array.Empty<NodeFinderVersion>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_versions = new NodeFinderVersion[length];
                                for (int i = 0; i < p_versions.Length; i++)
                                {
                                    p_versions[i] = (NodeFinderVersion)r.GetInt64();
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderHelloMessage(p_versions);
            }
        }
    }

    internal sealed partial class NodeFinderProfileMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage>.Empty;

        static NodeFinderProfileMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage(global::System.ReadOnlyMemory<byte>.Empty, NodeProfile.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxIdLength = 32;

        public NodeFinderProfileMessage(global::System.ReadOnlyMemory<byte> id, NodeProfile nodeProfile)
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

        public static global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage? left, global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage? left, global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Id.Span, target.Id.Span)) return false;
            if (this.NodeProfile != target.NodeProfile) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage value, in int rank)
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
                    global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_nodeProfile = global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderProfileMessage(p_id, p_nodeProfile);
            }
        }
    }

    internal sealed partial class NodeFinderDataMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage>.Empty;

        static NodeFinderDataMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage(global::System.Array.Empty<NodeProfile>(), global::System.Array.Empty<Location>(), global::System.Array.Empty<Tag>(), global::System.Array.Empty<Location>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPushNodeProfilesCount = 256;
        public static readonly int MaxPushLocationsCount = 256;
        public static readonly int MaxWantLocationsCount = 256;
        public static readonly int MaxGiveLocationsCount = 256;

        public NodeFinderDataMessage(NodeProfile[] pushNodeProfiles, Location[] pushLocations, Tag[] wantLocations, Location[] giveLocations)
        {
            if (pushNodeProfiles is null) throw new global::System.ArgumentNullException("pushNodeProfiles");
            if (pushNodeProfiles.Length > 256) throw new global::System.ArgumentOutOfRangeException("pushNodeProfiles");
            foreach (var n in pushNodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (pushLocations is null) throw new global::System.ArgumentNullException("pushLocations");
            if (pushLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("pushLocations");
            foreach (var n in pushLocations)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (wantLocations is null) throw new global::System.ArgumentNullException("wantLocations");
            if (wantLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("wantLocations");
            foreach (var n in wantLocations)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (giveLocations is null) throw new global::System.ArgumentNullException("giveLocations");
            if (giveLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("giveLocations");
            foreach (var n in giveLocations)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.PushNodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(pushNodeProfiles);
            this.PushLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<Location>(pushLocations);
            this.WantLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<Tag>(wantLocations);
            this.GiveLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<Location>(giveLocations);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in pushNodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in pushLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in wantLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in giveLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> PushNodeProfiles { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<Location> PushLocations { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<Tag> WantLocations { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<Location> GiveLocations { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage? left, global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage? left, global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushNodeProfiles, target.PushNodeProfiles)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushLocations, target.PushLocations)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantLocations, target.WantLocations)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.GiveLocations, target.GiveLocations)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.PushNodeProfiles.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.PushLocations.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.WantLocations.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.GiveLocations.Count != 0)
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
                        global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.PushLocations.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.PushLocations.Count);
                    foreach (var n in value.PushLocations)
                    {
                        global::Omnius.Xeus.Service.Engines.Internal.Location.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.WantLocations.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.WantLocations.Count);
                    foreach (var n in value.WantLocations)
                    {
                        global::Omnius.Xeus.Service.Engines.Tag.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.GiveLocations.Count != 0)
                {
                    w.Write((uint)3);
                    w.Write((uint)value.GiveLocations.Count);
                    foreach (var n in value.GiveLocations)
                    {
                        global::Omnius.Xeus.Service.Engines.Internal.Location.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                NodeProfile[] p_pushNodeProfiles = global::System.Array.Empty<NodeProfile>();
                Location[] p_pushLocations = global::System.Array.Empty<Location>();
                Tag[] p_wantLocations = global::System.Array.Empty<Tag>();
                Location[] p_giveLocations = global::System.Array.Empty<Location>();

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
                                    p_pushNodeProfiles[i] = global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_pushLocations = new Location[length];
                                for (int i = 0; i < p_pushLocations.Length; i++)
                                {
                                    p_pushLocations[i] = global::Omnius.Xeus.Service.Engines.Internal.Location.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_wantLocations = new Tag[length];
                                for (int i = 0; i < p_wantLocations.Length; i++)
                                {
                                    p_wantLocations[i] = global::Omnius.Xeus.Service.Engines.Tag.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 3:
                            {
                                var length = r.GetUInt32();
                                p_giveLocations = new Location[length];
                                for (int i = 0; i < p_giveLocations.Length; i++)
                                {
                                    p_giveLocations[i] = global::Omnius.Xeus.Service.Engines.Internal.Location.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage(p_pushNodeProfiles, p_pushLocations, p_wantLocations, p_giveLocations);
            }
        }
    }

    internal sealed partial class ContentExchangerHelloMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage>.Empty;

        static ContentExchangerHelloMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage(global::System.Array.Empty<ContentExchangerVersion>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxVersionsCount = 32;

        public ContentExchangerHelloMessage(ContentExchangerVersion[] versions)
        {
            if (versions is null) throw new global::System.ArgumentNullException("versions");
            if (versions.Length > 32) throw new global::System.ArgumentOutOfRangeException("versions");

            this.Versions = new global::Omnius.Core.Collections.ReadOnlyListSlim<ContentExchangerVersion>(versions);

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

        public global::Omnius.Core.Collections.ReadOnlyListSlim<ContentExchangerVersion> Versions { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Versions, target.Versions)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ContentExchangerVersion[] p_versions = global::System.Array.Empty<ContentExchangerVersion>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_versions = new ContentExchangerVersion[length];
                                for (int i = 0; i < p_versions.Length; i++)
                                {
                                    p_versions[i] = (ContentExchangerVersion)r.GetUInt64();
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerHelloMessage(p_versions);
            }
        }
    }

    internal sealed partial class ContentExchangerRequestMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage>.Empty;

        static ContentExchangerRequestMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage(OmniHash.Empty, NodeProfile.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ContentExchangerRequestMessage(OmniHash tag, NodeProfile nodeProfile)
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

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;
            if (this.NodeProfile != target.NodeProfile) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage value, in int rank)
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
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
                if (value.NodeProfile != NodeProfile.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tag = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_nodeProfile = global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestMessage(p_tag, p_nodeProfile);
            }
        }
    }

    internal sealed partial class ContentExchangerResponseMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage>.Empty;

        static ContentExchangerResponseMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage(OmniHash.Empty, NodeProfile.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ContentExchangerResponseMessage(OmniHash tag, NodeProfile nodeProfile)
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

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;
            if (this.NodeProfile != target.NodeProfile) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage value, in int rank)
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
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
                if (value.NodeProfile != NodeProfile.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tag = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_nodeProfile = global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerResponseMessage(p_tag, p_nodeProfile);
            }
        }
    }

    internal sealed partial class ContentExchangerDataMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage>.Empty;

        static ContentExchangerDataMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage(global::System.Array.Empty<NodeProfile>(), global::System.Array.Empty<ContentBlockFlags>(), global::System.Array.Empty<OmniHash>(), global::System.Array.Empty<OmniHash>(), global::System.Array.Empty<ContentBlock>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPushNodeProfilesCount = 256;
        public static readonly int MaxContentBlockFlagsCount = 32;
        public static readonly int MaxWantContentBlocksCount = 256;
        public static readonly int MaxCancelContentBlocksCount = 256;
        public static readonly int MaxGiveContentBlocksCount = 8;

        public ContentExchangerDataMessage(NodeProfile[] pushNodeProfiles, ContentBlockFlags[] contentBlockFlags, OmniHash[] wantContentBlocks, OmniHash[] cancelContentBlocks, ContentBlock[] giveContentBlocks)
        {
            if (pushNodeProfiles is null) throw new global::System.ArgumentNullException("pushNodeProfiles");
            if (pushNodeProfiles.Length > 256) throw new global::System.ArgumentOutOfRangeException("pushNodeProfiles");
            foreach (var n in pushNodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (contentBlockFlags is null) throw new global::System.ArgumentNullException("contentBlockFlags");
            if (contentBlockFlags.Length > 32) throw new global::System.ArgumentOutOfRangeException("contentBlockFlags");
            foreach (var n in contentBlockFlags)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (wantContentBlocks is null) throw new global::System.ArgumentNullException("wantContentBlocks");
            if (wantContentBlocks.Length > 256) throw new global::System.ArgumentOutOfRangeException("wantContentBlocks");
            if (cancelContentBlocks is null) throw new global::System.ArgumentNullException("cancelContentBlocks");
            if (cancelContentBlocks.Length > 256) throw new global::System.ArgumentOutOfRangeException("cancelContentBlocks");
            if (giveContentBlocks is null) throw new global::System.ArgumentNullException("giveContentBlocks");
            if (giveContentBlocks.Length > 8) throw new global::System.ArgumentOutOfRangeException("giveContentBlocks");
            foreach (var n in giveContentBlocks)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.PushNodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(pushNodeProfiles);
            this.ContentBlockFlags = new global::Omnius.Core.Collections.ReadOnlyListSlim<ContentBlockFlags>(contentBlockFlags);
            this.WantContentBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(wantContentBlocks);
            this.CancelContentBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(cancelContentBlocks);
            this.GiveContentBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<ContentBlock>(giveContentBlocks);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in pushNodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in contentBlockFlags)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in wantContentBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in cancelContentBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in giveContentBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> PushNodeProfiles { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<ContentBlockFlags> ContentBlockFlags { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> WantContentBlocks { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> CancelContentBlocks { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<ContentBlock> GiveContentBlocks { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushNodeProfiles, target.PushNodeProfiles)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.ContentBlockFlags, target.ContentBlockFlags)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantContentBlocks, target.WantContentBlocks)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.CancelContentBlocks, target.CancelContentBlocks)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.GiveContentBlocks, target.GiveContentBlocks)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.PushNodeProfiles.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.ContentBlockFlags.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.WantContentBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.CancelContentBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.GiveContentBlocks.Count != 0)
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
                        global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.ContentBlockFlags.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.ContentBlockFlags.Count);
                    foreach (var n in value.ContentBlockFlags)
                    {
                        global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.WantContentBlocks.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.WantContentBlocks.Count);
                    foreach (var n in value.WantContentBlocks)
                    {
                        global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.CancelContentBlocks.Count != 0)
                {
                    w.Write((uint)3);
                    w.Write((uint)value.CancelContentBlocks.Count);
                    foreach (var n in value.CancelContentBlocks)
                    {
                        global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.GiveContentBlocks.Count != 0)
                {
                    w.Write((uint)4);
                    w.Write((uint)value.GiveContentBlocks.Count);
                    foreach (var n in value.GiveContentBlocks)
                    {
                        global::Omnius.Xeus.Service.Engines.Internal.ContentBlock.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                NodeProfile[] p_pushNodeProfiles = global::System.Array.Empty<NodeProfile>();
                ContentBlockFlags[] p_contentBlockFlags = global::System.Array.Empty<ContentBlockFlags>();
                OmniHash[] p_wantContentBlocks = global::System.Array.Empty<OmniHash>();
                OmniHash[] p_cancelContentBlocks = global::System.Array.Empty<OmniHash>();
                ContentBlock[] p_giveContentBlocks = global::System.Array.Empty<ContentBlock>();

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
                                    p_pushNodeProfiles[i] = global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_contentBlockFlags = new ContentBlockFlags[length];
                                for (int i = 0; i < p_contentBlockFlags.Length; i++)
                                {
                                    p_contentBlockFlags[i] = global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_wantContentBlocks = new OmniHash[length];
                                for (int i = 0; i < p_wantContentBlocks.Length; i++)
                                {
                                    p_wantContentBlocks[i] = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 3:
                            {
                                var length = r.GetUInt32();
                                p_cancelContentBlocks = new OmniHash[length];
                                for (int i = 0; i < p_cancelContentBlocks.Length; i++)
                                {
                                    p_cancelContentBlocks[i] = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 4:
                            {
                                var length = r.GetUInt32();
                                p_giveContentBlocks = new ContentBlock[length];
                                for (int i = 0; i < p_giveContentBlocks.Length; i++)
                                {
                                    p_giveContentBlocks[i] = global::Omnius.Xeus.Service.Engines.Internal.ContentBlock.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage(p_pushNodeProfiles, p_contentBlockFlags, p_wantContentBlocks, p_cancelContentBlocks, p_giveContentBlocks);
            }
        }
    }

    internal sealed partial class NodeFinderConfig : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig>.Empty;

        static NodeFinderConfig()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig(new global::System.Collections.Generic.Dictionary<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxNodeProfileMapCount = 1073741824;

        public NodeFinderConfig(global::System.Collections.Generic.Dictionary<NodeProfile, global::Omnius.Core.Serialization.RocketPack.Timestamp> nodeProfileMap)
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

        public static global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig? left, global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig? left, global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfileMap, target.NodeProfileMap)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig value, in int rank)
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
                        global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Serialize(ref w, n.Key, rank + 1);
                        w.Write(n.Value);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    t_key = global::Omnius.Xeus.Service.Engines.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                    t_value = r.GetTimestamp();
                                    p_nodeProfileMap[t_key] = t_value;
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderConfig(p_nodeProfileMap);
            }
        }
    }

}
