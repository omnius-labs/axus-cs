using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Models;

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

    internal enum ContentExchangerRequestExchangeResultType : byte
    {
        Unknown = 0,
        Rejected = 1,
        Accepted = 2,
    }

    internal enum DeclaredMessageExchangerVersion : byte
    {
        Unknown = 0,
        Version1 = 1,
    }

    internal enum DeclaredMessageExchangerFetchMessageResultType : byte
    {
        Unknown = 0,
        Rejected = 1,
        NotFound = 2,
        SameFound = 3,
        UpdatedFound = 4,
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

    internal sealed partial class ResourceLocation : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation>.Empty;

        static ResourceLocation()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation(ResourceTag.Empty, global::System.Array.Empty<NodeProfile>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxNodeProfilesCount = 8192;

        public ResourceLocation(ResourceTag resourceTag, NodeProfile[] nodeProfiles)
        {
            if (resourceTag is null) throw new global::System.ArgumentNullException("resourceTag");
            if (nodeProfiles is null) throw new global::System.ArgumentNullException("nodeProfiles");
            if (nodeProfiles.Length > 8192) throw new global::System.ArgumentOutOfRangeException("nodeProfiles");
            foreach (var n in nodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.ResourceTag = resourceTag;
            this.NodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(nodeProfiles);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (resourceTag != default) ___h.Add(resourceTag.GetHashCode());
                foreach (var n in nodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public ResourceTag ResourceTag { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> NodeProfiles { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation? left, global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation? left, global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ResourceTag != target.ResourceTag) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfiles, target.NodeProfiles)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ResourceTag != ResourceTag.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.NodeProfiles.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ResourceTag != ResourceTag.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Xeus.Service.Models.ResourceTag.Formatter.Serialize(ref w, value.ResourceTag, rank + 1);
                }
                if (value.NodeProfiles.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.NodeProfiles.Count);
                    foreach (var n in value.NodeProfiles)
                    {
                        global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ResourceTag p_resourceTag = ResourceTag.Empty;
                NodeProfile[] p_nodeProfiles = global::System.Array.Empty<NodeProfile>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_resourceTag = global::Omnius.Xeus.Service.Models.ResourceTag.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_nodeProfiles = new NodeProfile[length];
                                for (int i = 0; i < p_nodeProfiles.Length; i++)
                                {
                                    p_nodeProfiles[i] = global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation(p_resourceTag, p_nodeProfiles);
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
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentBlockFlags(0, global::Omnius.Core.MemoryOwner<byte>.Empty);
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
                global::System.Buffers.IMemoryOwner<byte> p_flags = global::Omnius.Core.MemoryOwner<byte>.Empty;

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
                        global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Serialize(ref w, n.Key, rank + 1);
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
                                    t_key = global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
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
                    global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
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
                                p_nodeProfile = global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
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
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage(global::System.Array.Empty<NodeProfile>(), global::System.Array.Empty<ResourceLocation>(), global::System.Array.Empty<ResourceTag>(), global::System.Array.Empty<ResourceLocation>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPushNodeProfilesCount = 256;
        public static readonly int MaxPushResourceLocationsCount = 256;
        public static readonly int MaxWantResourceLocationsCount = 256;
        public static readonly int MaxGiveResourceLocationsCount = 256;

        public NodeFinderDataMessage(NodeProfile[] pushNodeProfiles, ResourceLocation[] pushResourceLocations, ResourceTag[] wantResourceLocations, ResourceLocation[] giveResourceLocations)
        {
            if (pushNodeProfiles is null) throw new global::System.ArgumentNullException("pushNodeProfiles");
            if (pushNodeProfiles.Length > 256) throw new global::System.ArgumentOutOfRangeException("pushNodeProfiles");
            foreach (var n in pushNodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (pushResourceLocations is null) throw new global::System.ArgumentNullException("pushResourceLocations");
            if (pushResourceLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("pushResourceLocations");
            foreach (var n in pushResourceLocations)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (wantResourceLocations is null) throw new global::System.ArgumentNullException("wantResourceLocations");
            if (wantResourceLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("wantResourceLocations");
            foreach (var n in wantResourceLocations)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            if (giveResourceLocations is null) throw new global::System.ArgumentNullException("giveResourceLocations");
            if (giveResourceLocations.Length > 256) throw new global::System.ArgumentOutOfRangeException("giveResourceLocations");
            foreach (var n in giveResourceLocations)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.PushNodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(pushNodeProfiles);
            this.PushResourceLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<ResourceLocation>(pushResourceLocations);
            this.WantResourceLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<ResourceTag>(wantResourceLocations);
            this.GiveResourceLocations = new global::Omnius.Core.Collections.ReadOnlyListSlim<ResourceLocation>(giveResourceLocations);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in pushNodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in pushResourceLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in wantResourceLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in giveResourceLocations)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> PushNodeProfiles { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<ResourceLocation> PushResourceLocations { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<ResourceTag> WantResourceLocations { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<ResourceLocation> GiveResourceLocations { get; }

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
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushResourceLocations, target.PushResourceLocations)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantResourceLocations, target.WantResourceLocations)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.GiveResourceLocations, target.GiveResourceLocations)) return false;

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
                    if (value.PushResourceLocations.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.WantResourceLocations.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.GiveResourceLocations.Count != 0)
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
                        global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.PushResourceLocations.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.PushResourceLocations.Count);
                    foreach (var n in value.PushResourceLocations)
                    {
                        global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.WantResourceLocations.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.WantResourceLocations.Count);
                    foreach (var n in value.WantResourceLocations)
                    {
                        global::Omnius.Xeus.Service.Models.ResourceTag.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.GiveResourceLocations.Count != 0)
                {
                    w.Write((uint)3);
                    w.Write((uint)value.GiveResourceLocations.Count);
                    foreach (var n in value.GiveResourceLocations)
                    {
                        global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                NodeProfile[] p_pushNodeProfiles = global::System.Array.Empty<NodeProfile>();
                ResourceLocation[] p_pushResourceLocations = global::System.Array.Empty<ResourceLocation>();
                ResourceTag[] p_wantResourceLocations = global::System.Array.Empty<ResourceTag>();
                ResourceLocation[] p_giveResourceLocations = global::System.Array.Empty<ResourceLocation>();

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
                                    p_pushNodeProfiles[i] = global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_pushResourceLocations = new ResourceLocation[length];
                                for (int i = 0; i < p_pushResourceLocations.Length; i++)
                                {
                                    p_pushResourceLocations[i] = global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_wantResourceLocations = new ResourceTag[length];
                                for (int i = 0; i < p_wantResourceLocations.Length; i++)
                                {
                                    p_wantResourceLocations[i] = global::Omnius.Xeus.Service.Models.ResourceTag.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 3:
                            {
                                var length = r.GetUInt32();
                                p_giveResourceLocations = new ResourceLocation[length];
                                for (int i = 0; i < p_giveResourceLocations.Length; i++)
                                {
                                    p_giveResourceLocations[i] = global::Omnius.Xeus.Service.Engines.Internal.ResourceLocation.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.NodeFinderDataMessage(p_pushNodeProfiles, p_pushResourceLocations, p_wantResourceLocations, p_giveResourceLocations);
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

    internal sealed partial class ContentExchangerProfileMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage>.Empty;

        static ContentExchangerProfileMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage(NodeProfile.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ContentExchangerProfileMessage(NodeProfile nodeProfile)
        {
            if (nodeProfile is null) throw new global::System.ArgumentNullException("nodeProfile");

            this.NodeProfile = nodeProfile;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (nodeProfile != default) ___h.Add(nodeProfile.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public NodeProfile NodeProfile { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.NodeProfile != target.NodeProfile) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.NodeProfile != NodeProfile.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.NodeProfile != NodeProfile.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                NodeProfile p_nodeProfile = NodeProfile.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_nodeProfile = global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerProfileMessage(p_nodeProfile);
            }
        }
    }

    internal sealed partial class ContentExchangerRequestExchangeMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage>.Empty;

        static ContentExchangerRequestExchangeMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ContentExchangerRequestExchangeMessage(OmniHash hash)
        {
            this.Hash = hash;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (hash != default) ___h.Add(hash.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public OmniHash Hash { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Hash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Hash != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Hash, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_hash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeMessage(p_hash);
            }
        }
    }

    internal sealed partial class ContentExchangerRequestExchangeResultMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage>.Empty;

        static ContentExchangerRequestExchangeResultMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage((ContentExchangerRequestExchangeResultType)0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ContentExchangerRequestExchangeResultMessage(ContentExchangerRequestExchangeResultType type)
        {
            this.Type = type;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public ContentExchangerRequestExchangeResultType Type { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage? left, global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != (ContentExchangerRequestExchangeResultType)0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Type != (ContentExchangerRequestExchangeResultType)0)
                {
                    w.Write((uint)0);
                    w.Write((ulong)value.Type);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ContentExchangerRequestExchangeResultType p_type = (ContentExchangerRequestExchangeResultType)0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_type = (ContentExchangerRequestExchangeResultType)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerRequestExchangeResultMessage(p_type);
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
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage(global::System.Array.Empty<NodeProfile>(), global::System.Array.Empty<ContentBlockFlags>(), global::System.Array.Empty<OmniHash>(), global::System.Array.Empty<ContentBlock>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPushNodeProfilesCount = 256;
        public static readonly int MaxContentBlockFlagsCount = 32;
        public static readonly int MaxWantContentBlocksCount = 256;
        public static readonly int MaxGiveContentBlocksCount = 8;

        public ContentExchangerDataMessage(NodeProfile[] pushNodeProfiles, ContentBlockFlags[] contentBlockFlags, OmniHash[] wantContentBlocks, ContentBlock[] giveContentBlocks)
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
            if (giveContentBlocks is null) throw new global::System.ArgumentNullException("giveContentBlocks");
            if (giveContentBlocks.Length > 8) throw new global::System.ArgumentOutOfRangeException("giveContentBlocks");
            foreach (var n in giveContentBlocks)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.PushNodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(pushNodeProfiles);
            this.ContentBlockFlags = new global::Omnius.Core.Collections.ReadOnlyListSlim<ContentBlockFlags>(contentBlockFlags);
            this.WantContentBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(wantContentBlocks);
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
                        global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
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
                if (value.GiveContentBlocks.Count != 0)
                {
                    w.Write((uint)3);
                    w.Write((uint)value.GiveContentBlocks.Count);
                    foreach (var n in value.GiveContentBlocks)
                    {
                        global::Omnius.Xeus.Service.Models.ContentBlock.Formatter.Serialize(ref w, n, rank + 1);
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
                                    p_pushNodeProfiles[i] = global::Omnius.Xeus.Service.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
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
                                p_giveContentBlocks = new ContentBlock[length];
                                for (int i = 0; i < p_giveContentBlocks.Length; i++)
                                {
                                    p_giveContentBlocks[i] = global::Omnius.Xeus.Service.Models.ContentBlock.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.ContentExchangerDataMessage(p_pushNodeProfiles, p_contentBlockFlags, p_wantContentBlocks, p_giveContentBlocks);
            }
        }
    }

    internal sealed partial class DeclaredMessageExchangerHelloMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage>.Empty;

        static DeclaredMessageExchangerHelloMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage(global::System.Array.Empty<DeclaredMessageExchangerVersion>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxVersionsCount = 32;

        public DeclaredMessageExchangerHelloMessage(DeclaredMessageExchangerVersion[] versions)
        {
            if (versions is null) throw new global::System.ArgumentNullException("versions");
            if (versions.Length > 32) throw new global::System.ArgumentOutOfRangeException("versions");

            this.Versions = new global::Omnius.Core.Collections.ReadOnlyListSlim<DeclaredMessageExchangerVersion>(versions);

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

        public global::Omnius.Core.Collections.ReadOnlyListSlim<DeclaredMessageExchangerVersion> Versions { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage? left, global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage? left, global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Versions, target.Versions)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                DeclaredMessageExchangerVersion[] p_versions = global::System.Array.Empty<DeclaredMessageExchangerVersion>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_versions = new DeclaredMessageExchangerVersion[length];
                                for (int i = 0; i < p_versions.Length; i++)
                                {
                                    p_versions[i] = (DeclaredMessageExchangerVersion)r.GetUInt64();
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerHelloMessage(p_versions);
            }
        }
    }

    internal sealed partial class DeclaredMessageExchangerFetchMessageMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage>.Empty;

        static DeclaredMessageExchangerFetchMessageMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage(OmniHash.Empty, global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public DeclaredMessageExchangerFetchMessageMessage(OmniHash hash, global::Omnius.Core.Serialization.RocketPack.Timestamp updatedTime)
        {
            this.Hash = hash;
            this.UpdatedTime = updatedTime;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (hash != default) ___h.Add(hash.GetHashCode());
                if (updatedTime != default) ___h.Add(updatedTime.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public OmniHash Hash { get; }
        public global::Omnius.Core.Serialization.RocketPack.Timestamp UpdatedTime { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage? left, global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage? left, global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.UpdatedTime != target.UpdatedTime) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Hash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.UpdatedTime != global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Hash != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Hash, rank + 1);
                }
                if (value.UpdatedTime != global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)1);
                    w.Write(value.UpdatedTime);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                global::Omnius.Core.Serialization.RocketPack.Timestamp p_updatedTime = global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_hash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_updatedTime = r.GetTimestamp();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageMessage(p_hash, p_updatedTime);
            }
        }
    }

    internal sealed partial class DeclaredMessageExchangerFetchMessageResultMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage>.Empty;

        static DeclaredMessageExchangerFetchMessageResultMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage((DeclaredMessageExchangerFetchMessageResultType)0, DeclaredMessage.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public DeclaredMessageExchangerFetchMessageResultMessage(DeclaredMessageExchangerFetchMessageResultType type, DeclaredMessage declaredMessage)
        {
            if (declaredMessage is null) throw new global::System.ArgumentNullException("declaredMessage");

            this.Type = type;
            this.DeclaredMessage = declaredMessage;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (declaredMessage != default) ___h.Add(declaredMessage.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public DeclaredMessageExchangerFetchMessageResultType Type { get; }
        public DeclaredMessage DeclaredMessage { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage? left, global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage? left, global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.DeclaredMessage != target.DeclaredMessage) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != (DeclaredMessageExchangerFetchMessageResultType)0)
                    {
                        propertyCount++;
                    }
                    if (value.DeclaredMessage != DeclaredMessage.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Type != (DeclaredMessageExchangerFetchMessageResultType)0)
                {
                    w.Write((uint)0);
                    w.Write((ulong)value.Type);
                }
                if (value.DeclaredMessage != DeclaredMessage.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Service.Models.DeclaredMessage.Formatter.Serialize(ref w, value.DeclaredMessage, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                DeclaredMessageExchangerFetchMessageResultType p_type = (DeclaredMessageExchangerFetchMessageResultType)0;
                DeclaredMessage p_declaredMessage = DeclaredMessage.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_type = (DeclaredMessageExchangerFetchMessageResultType)r.GetUInt64();
                                break;
                            }
                        case 1:
                            {
                                p_declaredMessage = global::Omnius.Xeus.Service.Models.DeclaredMessage.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerFetchMessageResultMessage(p_type, p_declaredMessage);
            }
        }
    }

    internal sealed partial class DeclaredMessageExchangerPostMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage>.Empty;

        static DeclaredMessageExchangerPostMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage>.Empty = new global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage(DeclaredMessage.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public DeclaredMessageExchangerPostMessage(DeclaredMessage declaredMessage)
        {
            if (declaredMessage is null) throw new global::System.ArgumentNullException("declaredMessage");

            this.DeclaredMessage = declaredMessage;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (declaredMessage != default) ___h.Add(declaredMessage.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public DeclaredMessage DeclaredMessage { get; }

        public static global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage? left, global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage? left, global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.DeclaredMessage != target.DeclaredMessage) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.DeclaredMessage != DeclaredMessage.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.DeclaredMessage != DeclaredMessage.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Xeus.Service.Models.DeclaredMessage.Formatter.Serialize(ref w, value.DeclaredMessage, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                DeclaredMessage p_declaredMessage = DeclaredMessage.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_declaredMessage = global::Omnius.Xeus.Service.Models.DeclaredMessage.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.Internal.DeclaredMessageExchangerPostMessage(p_declaredMessage);
            }
        }
    }

}
