using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Messages;

#nullable enable

namespace Xeus.Core.Internal.Storage
{
    internal sealed partial class ClusterMetadata : global::Omnix.Serialization.RocketPack.IRocketPackMessage<ClusterMetadata>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ClusterMetadata> Formatter { get; }
        public static ClusterMetadata Empty { get; }

        static ClusterMetadata()
        {
            ClusterMetadata.Formatter = new ___CustomFormatter();
            ClusterMetadata.Empty = new ClusterMetadata(global::System.Array.Empty<ulong>(), 0, global::Omnix.Serialization.RocketPack.Timestamp.Zero);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxSectorsCount = 256;

        public ClusterMetadata(ulong[] sectors, uint length, global::Omnix.Serialization.RocketPack.Timestamp lastAccessTime)
        {
            if (sectors is null) throw new global::System.ArgumentNullException("sectors");
            if (sectors.Length > 256) throw new global::System.ArgumentOutOfRangeException("sectors");
            this.Sectors = new global::Omnix.DataStructures.ReadOnlyListSlim<ulong>(sectors);
            this.Length = length;
            this.LastAccessTime = lastAccessTime;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in sectors)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                if (length != default) ___h.Add(length.GetHashCode());
                if (lastAccessTime != default) ___h.Add(lastAccessTime.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<ulong> Sectors { get; }
        public uint Length { get; }
        public global::Omnix.Serialization.RocketPack.Timestamp LastAccessTime { get; }

        public static ClusterMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ClusterMetadata? left, ClusterMetadata? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ClusterMetadata? left, ClusterMetadata? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ClusterMetadata)) return false;
            return this.Equals((ClusterMetadata)other);
        }
        public bool Equals(ClusterMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Sectors, target.Sectors)) return false;
            if (this.Length != target.Length) return false;
            if (this.LastAccessTime != target.LastAccessTime) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ClusterMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in ClusterMetadata value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Sectors.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Length != 0)
                    {
                        propertyCount++;
                    }
                    if (value.LastAccessTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Sectors.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Sectors.Count);
                    foreach (var n in value.Sectors)
                    {
                        w.Write(n);
                    }
                }
                if (value.Length != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Length);
                }
                if (value.LastAccessTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.LastAccessTime);
                }
            }

            public ClusterMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ulong[] p_sectors = global::System.Array.Empty<ulong>();
                uint p_length = 0;
                global::Omnix.Serialization.RocketPack.Timestamp p_lastAccessTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_sectors = new ulong[length];
                                for (int i = 0; i < p_sectors.Length; i++)
                                {
                                    p_sectors[i] = r.GetUInt64();
                                }
                                break;
                            }
                        case 1:
                            {
                                p_length = r.GetUInt32();
                                break;
                            }
                        case 2:
                            {
                                p_lastAccessTime = r.GetTimestamp();
                                break;
                            }
                    }
                }

                return new ClusterMetadata(p_sectors, p_length, p_lastAccessTime);
            }
        }
    }

    internal sealed partial class BlockStorageConfig : global::Omnix.Serialization.RocketPack.IRocketPackMessage<BlockStorageConfig>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BlockStorageConfig> Formatter { get; }
        public static BlockStorageConfig Empty { get; }

        static BlockStorageConfig()
        {
            BlockStorageConfig.Formatter = new ___CustomFormatter();
            BlockStorageConfig.Empty = new BlockStorageConfig(0, 0, new global::System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxClusterMetadataMapCount = 1073741824;

        public BlockStorageConfig(uint version, ulong size, global::System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata> clusterMetadataMap)
        {
            if (clusterMetadataMap is null) throw new global::System.ArgumentNullException("clusterMetadataMap");
            if (clusterMetadataMap.Count > 1073741824) throw new global::System.ArgumentOutOfRangeException("clusterMetadataMap");
            foreach (var n in clusterMetadataMap)
            {
                if (n.Value is null) throw new global::System.ArgumentNullException("n.Value");
            }

            this.Version = version;
            this.Size = size;
            this.ClusterMetadataMap = new global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniHash, ClusterMetadata>(clusterMetadataMap);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (version != default) ___h.Add(version.GetHashCode());
                if (size != default) ___h.Add(size.GetHashCode());
                foreach (var n in clusterMetadataMap)
                {
                    if (n.Key != default) ___h.Add(n.Key.GetHashCode());
                    if (n.Value != default) ___h.Add(n.Value.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public uint Version { get; }
        public ulong Size { get; }
        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniHash, ClusterMetadata> ClusterMetadataMap { get; }

        public static BlockStorageConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BlockStorageConfig? left, BlockStorageConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BlockStorageConfig? left, BlockStorageConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BlockStorageConfig)) return false;
            return this.Equals((BlockStorageConfig)other);
        }
        public bool Equals(BlockStorageConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if (this.Size != target.Size) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ClusterMetadataMap, target.ClusterMetadataMap)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BlockStorageConfig>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in BlockStorageConfig value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Version != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Size != 0)
                    {
                        propertyCount++;
                    }
                    if (value.ClusterMetadataMap.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Version != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.Version);
                }
                if (value.Size != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Size);
                }
                if (value.ClusterMetadataMap.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.ClusterMetadataMap.Count);
                    foreach (var n in value.ClusterMetadataMap)
                    {
                        OmniHash.Formatter.Serialize(ref w, n.Key, rank + 1);
                        ClusterMetadata.Formatter.Serialize(ref w, n.Value, rank + 1);
                    }
                }
            }

            public BlockStorageConfig Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_version = 0;
                ulong p_size = 0;
                global::System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata> p_clusterMetadataMap = new global::System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_version = r.GetUInt32();
                                break;
                            }
                        case 1:
                            {
                                p_size = r.GetUInt64();
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_clusterMetadataMap = new global::System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata>();
                                OmniHash t_key = OmniHash.Empty;
                                ClusterMetadata t_value = ClusterMetadata.Empty;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                    t_value = ClusterMetadata.Formatter.Deserialize(ref r, rank + 1);
                                    p_clusterMetadataMap[t_key] = t_value;
                                }
                                break;
                            }
                    }
                }

                return new BlockStorageConfig(p_version, p_size, p_clusterMetadataMap);
            }
        }
    }

    internal sealed partial class SharedBlocksMetadata : global::Omnix.Serialization.RocketPack.IRocketPackMessage<SharedBlocksMetadata>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<SharedBlocksMetadata> Formatter { get; }
        public static SharedBlocksMetadata Empty { get; }

        static SharedBlocksMetadata()
        {
            SharedBlocksMetadata.Formatter = new ___CustomFormatter();
            SharedBlocksMetadata.Empty = new SharedBlocksMetadata(string.Empty, 0, 0, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPathLength = 1024;
        public static readonly int MaxHashesCount = 1073741824;

        public SharedBlocksMetadata(string path, ulong length, uint blockLength, OmniHash[] hashes)
        {
            if (path is null) throw new global::System.ArgumentNullException("path");
            if (path.Length > 1024) throw new global::System.ArgumentOutOfRangeException("path");
            if (hashes is null) throw new global::System.ArgumentNullException("hashes");
            if (hashes.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("hashes");

            this.Path = path;
            this.Length = length;
            this.BlockLength = blockLength;
            this.Hashes = new global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash>(hashes);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (path != default) ___h.Add(path.GetHashCode());
                if (length != default) ___h.Add(length.GetHashCode());
                if (blockLength != default) ___h.Add(blockLength.GetHashCode());
                foreach (var n in hashes)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public string Path { get; }
        public ulong Length { get; }
        public uint BlockLength { get; }
        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash> Hashes { get; }

        public static SharedBlocksMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(SharedBlocksMetadata? left, SharedBlocksMetadata? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(SharedBlocksMetadata? left, SharedBlocksMetadata? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is SharedBlocksMetadata)) return false;
            return this.Equals((SharedBlocksMetadata)other);
        }
        public bool Equals(SharedBlocksMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Path != target.Path) return false;
            if (this.Length != target.Length) return false;
            if (this.BlockLength != target.BlockLength) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<SharedBlocksMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in SharedBlocksMetadata value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Path != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Length != 0)
                    {
                        propertyCount++;
                    }
                    if (value.BlockLength != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Hashes.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Path != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Path);
                }
                if (value.Length != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Length);
                }
                if (value.BlockLength != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.BlockLength);
                }
                if (value.Hashes.Count != 0)
                {
                    w.Write((uint)3);
                    w.Write((uint)value.Hashes.Count);
                    foreach (var n in value.Hashes)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public SharedBlocksMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_path = string.Empty;
                ulong p_length = 0;
                uint p_blockLength = 0;
                OmniHash[] p_hashes = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_path = r.GetString(1024);
                                break;
                            }
                        case 1:
                            {
                                p_length = r.GetUInt64();
                                break;
                            }
                        case 2:
                            {
                                p_blockLength = r.GetUInt32();
                                break;
                            }
                        case 3:
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

                return new SharedBlocksMetadata(p_path, p_length, p_blockLength, p_hashes);
            }
        }
    }

    internal sealed partial class ContentMetadata : global::Omnix.Serialization.RocketPack.IRocketPackMessage<ContentMetadata>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentMetadata> Formatter { get; }
        public static ContentMetadata Empty { get; }

        static ContentMetadata()
        {
            ContentMetadata.Formatter = new ___CustomFormatter();
            ContentMetadata.Empty = new ContentMetadata(XeusClue.Empty, global::System.Array.Empty<OmniHash>(), null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxLockedHashesCount = 1073741824;

        public ContentMetadata(XeusClue clue, OmniHash[] lockedHashes, SharedBlocksMetadata? sharedBlocksMetadata)
        {
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (lockedHashes is null) throw new global::System.ArgumentNullException("lockedHashes");
            if (lockedHashes.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("lockedHashes");
            this.Clue = clue;
            this.LockedHashes = new global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash>(lockedHashes);
            this.SharedBlocksMetadata = sharedBlocksMetadata;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (clue != default) ___h.Add(clue.GetHashCode());
                foreach (var n in lockedHashes)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                if (sharedBlocksMetadata != default) ___h.Add(sharedBlocksMetadata.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public XeusClue Clue { get; }
        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash> LockedHashes { get; }
        public SharedBlocksMetadata? SharedBlocksMetadata { get; }

        public static ContentMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ContentMetadata? left, ContentMetadata? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ContentMetadata? left, ContentMetadata? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ContentMetadata)) return false;
            return this.Equals((ContentMetadata)other);
        }
        public bool Equals(ContentMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.LockedHashes, target.LockedHashes)) return false;
            if ((this.SharedBlocksMetadata is null) != (target.SharedBlocksMetadata is null)) return false;
            if (!(this.SharedBlocksMetadata is null) && !(target.SharedBlocksMetadata is null) && this.SharedBlocksMetadata != target.SharedBlocksMetadata) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in ContentMetadata value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Clue != XeusClue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.LockedHashes.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.SharedBlocksMetadata != null)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Clue != XeusClue.Empty)
                {
                    w.Write((uint)0);
                    XeusClue.Formatter.Serialize(ref w, value.Clue, rank + 1);
                }
                if (value.LockedHashes.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.LockedHashes.Count);
                    foreach (var n in value.LockedHashes)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.SharedBlocksMetadata != null)
                {
                    w.Write((uint)2);
                    SharedBlocksMetadata.Formatter.Serialize(ref w, value.SharedBlocksMetadata, rank + 1);
                }
            }

            public ContentMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                XeusClue p_clue = XeusClue.Empty;
                OmniHash[] p_lockedHashes = global::System.Array.Empty<OmniHash>();
                SharedBlocksMetadata? p_sharedBlocksMetadata = null;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_lockedHashes = new OmniHash[length];
                                for (int i = 0; i < p_lockedHashes.Length; i++)
                                {
                                    p_lockedHashes[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                p_sharedBlocksMetadata = SharedBlocksMetadata.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new ContentMetadata(p_clue, p_lockedHashes, p_sharedBlocksMetadata);
            }
        }
    }

    internal sealed partial class ContentStorageConfig : global::Omnix.Serialization.RocketPack.IRocketPackMessage<ContentStorageConfig>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentStorageConfig> Formatter { get; }
        public static ContentStorageConfig Empty { get; }

        static ContentStorageConfig()
        {
            ContentStorageConfig.Formatter = new ___CustomFormatter();
            ContentStorageConfig.Empty = new ContentStorageConfig(0, global::System.Array.Empty<ContentMetadata>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxContentMetadatasCount = 1073741824;

        public ContentStorageConfig(uint version, ContentMetadata[] contentMetadatas)
        {
            if (contentMetadatas is null) throw new global::System.ArgumentNullException("contentMetadatas");
            if (contentMetadatas.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("contentMetadatas");
            foreach (var n in contentMetadatas)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Version = version;
            this.ContentMetadatas = new global::Omnix.DataStructures.ReadOnlyListSlim<ContentMetadata>(contentMetadatas);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (version != default) ___h.Add(version.GetHashCode());
                foreach (var n in contentMetadatas)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public uint Version { get; }
        public global::Omnix.DataStructures.ReadOnlyListSlim<ContentMetadata> ContentMetadatas { get; }

        public static ContentStorageConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ContentStorageConfig? left, ContentStorageConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ContentStorageConfig? left, ContentStorageConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ContentStorageConfig)) return false;
            return this.Equals((ContentStorageConfig)other);
        }
        public bool Equals(ContentStorageConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ContentMetadatas, target.ContentMetadatas)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentStorageConfig>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in ContentStorageConfig value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Version != 0)
                    {
                        propertyCount++;
                    }
                    if (value.ContentMetadatas.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Version != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.Version);
                }
                if (value.ContentMetadatas.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.ContentMetadatas.Count);
                    foreach (var n in value.ContentMetadatas)
                    {
                        ContentMetadata.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public ContentStorageConfig Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_version = 0;
                ContentMetadata[] p_contentMetadatas = global::System.Array.Empty<ContentMetadata>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_version = r.GetUInt32();
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_contentMetadatas = new ContentMetadata[length];
                                for (int i = 0; i < p_contentMetadatas.Length; i++)
                                {
                                    p_contentMetadatas[i] = ContentMetadata.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new ContentStorageConfig(p_version, p_contentMetadatas);
            }
        }
    }

}
