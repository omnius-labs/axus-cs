using Omnix.Cryptography;
using Omnix.Network;
using Xeus.Messages;

#nullable enable

namespace Xeus.Core.Contents.Internal
{
    internal sealed partial class ClusterMetadata : Omnix.Serialization.RocketPack.RocketPackMessageBase<ClusterMetadata>
    {
        static ClusterMetadata()
        {
            ClusterMetadata.Formatter = new CustomFormatter();
            ClusterMetadata.Empty = new ClusterMetadata(System.Array.Empty<ulong>(), 0, Omnix.Serialization.RocketPack.Timestamp.Zero);
        }

        private readonly int __hashCode;

        public static readonly int MaxSectorsCount = 256;

        public ClusterMetadata(ulong[] sectors, uint length, Omnix.Serialization.RocketPack.Timestamp lastAccessTime)
        {
            if (sectors is null) throw new System.ArgumentNullException("sectors");
            if (sectors.Length > 256) throw new System.ArgumentOutOfRangeException("sectors");
            this.Sectors = new Omnix.Collections.ReadOnlyListSlim<ulong>(sectors);
            this.Length = length;
            this.LastAccessTime = lastAccessTime;

            {
                var __h = new System.HashCode();
                foreach (var n in this.Sectors)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                if (this.Length != default) __h.Add(this.Length.GetHashCode());
                if (this.LastAccessTime != default) __h.Add(this.LastAccessTime.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyListSlim<ulong> Sectors { get; }
        public uint Length { get; }
        public Omnix.Serialization.RocketPack.Timestamp LastAccessTime { get; }

        public override bool Equals(ClusterMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Sectors, target.Sectors)) return false;
            if (this.Length != target.Length) return false;
            if (this.LastAccessTime != target.LastAccessTime) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ClusterMetadata>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ClusterMetadata value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                    if (value.LastAccessTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
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
                if (value.LastAccessTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.LastAccessTime);
                }
            }

            public ClusterMetadata Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                ulong[] p_sectors = System.Array.Empty<ulong>();
                uint p_length = 0;
                Omnix.Serialization.RocketPack.Timestamp p_lastAccessTime = Omnix.Serialization.RocketPack.Timestamp.Zero;

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

    internal sealed partial class ContentMetadata : Omnix.Serialization.RocketPack.RocketPackMessageBase<ContentMetadata>
    {
        static ContentMetadata()
        {
            ContentMetadata.Formatter = new CustomFormatter();
            ContentMetadata.Empty = new ContentMetadata(XeusClue.Empty, System.Array.Empty<OmniHash>(), null);
        }

        private readonly int __hashCode;

        public static readonly int MaxLockedHashesCount = 1073741824;

        public ContentMetadata(XeusClue clue, OmniHash[] lockedHashes, SharedBlocksMetadata? sharedBlocksMetadata)
        {
            if (clue is null) throw new System.ArgumentNullException("clue");
            if (lockedHashes is null) throw new System.ArgumentNullException("lockedHashes");
            if (lockedHashes.Length > 1073741824) throw new System.ArgumentOutOfRangeException("lockedHashes");
            this.Clue = clue;
            this.LockedHashes = new Omnix.Collections.ReadOnlyListSlim<OmniHash>(lockedHashes);
            this.SharedBlocksMetadata = sharedBlocksMetadata;

            {
                var __h = new System.HashCode();
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                foreach (var n in this.LockedHashes)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                if (this.SharedBlocksMetadata != default) __h.Add(this.SharedBlocksMetadata.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public XeusClue Clue { get; }
        public Omnix.Collections.ReadOnlyListSlim<OmniHash> LockedHashes { get; }
        public SharedBlocksMetadata? SharedBlocksMetadata { get; }

        public override bool Equals(ContentMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.LockedHashes, target.LockedHashes)) return false;
            if ((this.SharedBlocksMetadata is null) != (target.SharedBlocksMetadata is null)) return false;
            if (!(this.SharedBlocksMetadata is null) && !(target.SharedBlocksMetadata is null) && this.SharedBlocksMetadata != target.SharedBlocksMetadata) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentMetadata>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ContentMetadata value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                    XeusClue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.LockedHashes.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.LockedHashes.Count);
                    foreach (var n in value.LockedHashes)
                    {
                        OmniHash.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                if (value.SharedBlocksMetadata != null)
                {
                    w.Write((uint)2);
                    SharedBlocksMetadata.Formatter.Serialize(w, value.SharedBlocksMetadata, rank + 1);
                }
            }

            public ContentMetadata Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                XeusClue p_clue = XeusClue.Empty;
                OmniHash[] p_lockedHashes = System.Array.Empty<OmniHash>();
                SharedBlocksMetadata? p_sharedBlocksMetadata = null;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_lockedHashes = new OmniHash[length];
                                for (int i = 0; i < p_lockedHashes.Length; i++)
                                {
                                    p_lockedHashes[i] = OmniHash.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                p_sharedBlocksMetadata = SharedBlocksMetadata.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new ContentMetadata(p_clue, p_lockedHashes, p_sharedBlocksMetadata);
            }
        }
    }

    internal sealed partial class SharedBlocksMetadata : Omnix.Serialization.RocketPack.RocketPackMessageBase<SharedBlocksMetadata>
    {
        static SharedBlocksMetadata()
        {
            SharedBlocksMetadata.Formatter = new CustomFormatter();
            SharedBlocksMetadata.Empty = new SharedBlocksMetadata(string.Empty, 0, 0, System.Array.Empty<OmniHash>());
        }

        private readonly int __hashCode;

        public static readonly int MaxPathLength = 1024;
        public static readonly int MaxHashesCount = 1073741824;

        public SharedBlocksMetadata(string path, ulong length, uint blockLength, OmniHash[] hashes)
        {
            if (path is null) throw new System.ArgumentNullException("path");
            if (path.Length > 1024) throw new System.ArgumentOutOfRangeException("path");
            if (hashes is null) throw new System.ArgumentNullException("hashes");
            if (hashes.Length > 1073741824) throw new System.ArgumentOutOfRangeException("hashes");

            this.Path = path;
            this.Length = length;
            this.BlockLength = blockLength;
            this.Hashes = new Omnix.Collections.ReadOnlyListSlim<OmniHash>(hashes);

            {
                var __h = new System.HashCode();
                if (this.Path != default) __h.Add(this.Path.GetHashCode());
                if (this.Length != default) __h.Add(this.Length.GetHashCode());
                if (this.BlockLength != default) __h.Add(this.BlockLength.GetHashCode());
                foreach (var n in this.Hashes)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public string Path { get; }
        public ulong Length { get; }
        public uint BlockLength { get; }
        public Omnix.Collections.ReadOnlyListSlim<OmniHash> Hashes { get; }

        public override bool Equals(SharedBlocksMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Path != target.Path) return false;
            if (this.Length != target.Length) return false;
            if (this.BlockLength != target.BlockLength) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<SharedBlocksMetadata>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, SharedBlocksMetadata value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                        OmniHash.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public SharedBlocksMetadata Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_path = string.Empty;
                ulong p_length = 0;
                uint p_blockLength = 0;
                OmniHash[] p_hashes = System.Array.Empty<OmniHash>();

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
                                    p_hashes[i] = OmniHash.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new SharedBlocksMetadata(p_path, p_length, p_blockLength, p_hashes);
            }
        }
    }

    internal sealed partial class BlockStorageConfig : Omnix.Serialization.RocketPack.RocketPackMessageBase<BlockStorageConfig>
    {
        static BlockStorageConfig()
        {
            BlockStorageConfig.Formatter = new CustomFormatter();
            BlockStorageConfig.Empty = new BlockStorageConfig(0, new System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata>(), 0);
        }

        private readonly int __hashCode;

        public static readonly int MaxClusterMetadataMapCount = 1073741824;

        public BlockStorageConfig(uint version, System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata> clusterMetadataMap, ulong size)
        {
            if (clusterMetadataMap is null) throw new System.ArgumentNullException("clusterMetadataMap");
            if (clusterMetadataMap.Count > 1073741824) throw new System.ArgumentOutOfRangeException("clusterMetadataMap");
            foreach (var n in clusterMetadataMap)
            {
                if (n.Value is null) throw new System.ArgumentNullException("n.Value");
            }
            this.Version = version;
            this.ClusterMetadataMap = new Omnix.Collections.ReadOnlyDictionarySlim<OmniHash, ClusterMetadata>(clusterMetadataMap);
            this.Size = size;

            {
                var __h = new System.HashCode();
                if (this.Version != default) __h.Add(this.Version.GetHashCode());
                foreach (var n in this.ClusterMetadataMap)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                if (this.Size != default) __h.Add(this.Size.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public uint Version { get; }
        public Omnix.Collections.ReadOnlyDictionarySlim<OmniHash, ClusterMetadata> ClusterMetadataMap { get; }
        public ulong Size { get; }

        public override bool Equals(BlockStorageConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.ClusterMetadataMap, target.ClusterMetadataMap)) return false;
            if (this.Size != target.Size) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<BlockStorageConfig>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, BlockStorageConfig value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Version != 0)
                    {
                        propertyCount++;
                    }
                    if (value.ClusterMetadataMap.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Size != 0)
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
                if (value.ClusterMetadataMap.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.ClusterMetadataMap.Count);
                    foreach (var n in value.ClusterMetadataMap)
                    {
                        OmniHash.Formatter.Serialize(w, n.Key, rank + 1);
                        ClusterMetadata.Formatter.Serialize(w, n.Value, rank + 1);
                    }
                }
                if (value.Size != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.Size);
                }
            }

            public BlockStorageConfig Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_version = 0;
                System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata> p_clusterMetadataMap = new System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata>();
                ulong p_size = 0;

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
                                p_clusterMetadataMap = new System.Collections.Generic.Dictionary<OmniHash, ClusterMetadata>();
                                OmniHash t_key = OmniHash.Empty;
                                ClusterMetadata t_value = ClusterMetadata.Empty;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = OmniHash.Formatter.Deserialize(r, rank + 1);
                                    t_value = ClusterMetadata.Formatter.Deserialize(r, rank + 1);
                                    p_clusterMetadataMap[t_key] = t_value;
                                }
                                break;
                            }
                        case 2:
                            {
                                p_size = r.GetUInt64();
                                break;
                            }
                    }
                }

                return new BlockStorageConfig(p_version, p_clusterMetadataMap, p_size);
            }
        }
    }

    internal sealed partial class ContentStorageConfig : Omnix.Serialization.RocketPack.RocketPackMessageBase<ContentStorageConfig>
    {
        static ContentStorageConfig()
        {
            ContentStorageConfig.Formatter = new CustomFormatter();
            ContentStorageConfig.Empty = new ContentStorageConfig(0, System.Array.Empty<ContentMetadata>());
        }

        private readonly int __hashCode;

        public static readonly int MaxContentMetadatasCount = 1073741824;

        public ContentStorageConfig(uint version, ContentMetadata[] contentMetadatas)
        {
            if (contentMetadatas is null) throw new System.ArgumentNullException("contentMetadatas");
            if (contentMetadatas.Length > 1073741824) throw new System.ArgumentOutOfRangeException("contentMetadatas");
            foreach (var n in contentMetadatas)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.Version = version;
            this.ContentMetadatas = new Omnix.Collections.ReadOnlyListSlim<ContentMetadata>(contentMetadatas);

            {
                var __h = new System.HashCode();
                if (this.Version != default) __h.Add(this.Version.GetHashCode());
                foreach (var n in this.ContentMetadatas)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public uint Version { get; }
        public Omnix.Collections.ReadOnlyListSlim<ContentMetadata> ContentMetadatas { get; }

        public override bool Equals(ContentStorageConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.ContentMetadatas, target.ContentMetadatas)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentStorageConfig>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ContentStorageConfig value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                        ContentMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public ContentStorageConfig Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_version = 0;
                ContentMetadata[] p_contentMetadatas = System.Array.Empty<ContentMetadata>();

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
                                    p_contentMetadatas[i] = ContentMetadata.Formatter.Deserialize(r, rank + 1);
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
