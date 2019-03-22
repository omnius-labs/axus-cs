using Omnix.Base;
using Omnix.Base.Helpers;
using Omnix.Cryptography;
using Omnix.Network;
using Omnix.Serialization;
using Omnix.Serialization.RocketPack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xeus.Messages;
using Xeus.Messages.Options;
using Xeus.Messages.Reports;

namespace Xeus.Core.Contents.Internal
{
    internal sealed partial class ClusterInfo : RocketPackMessageBase<ClusterInfo>
    {
        static ClusterInfo()
        {
            ClusterInfo.Formatter = new CustomFormatter();
        }

        public static readonly int MaxSectorsCount = 256;

        public ClusterInfo(IList<ulong> sectors, uint length, Timestamp lastAccessTime)
        {
            if (sectors is null) throw new ArgumentNullException("sectors");
            if (sectors.Count > 256) throw new ArgumentOutOfRangeException("sectors");
            this.Sectors = new ReadOnlyCollection<ulong>(sectors);
            this.Length = length;
            this.LastAccessTime = lastAccessTime;

            {
                var hashCode = new HashCode();
                foreach (var n in this.Sectors)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                if (this.Length != default) hashCode.Add(this.Length.GetHashCode());
                if (this.LastAccessTime != default) hashCode.Add(this.LastAccessTime.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public IReadOnlyList<ulong> Sectors { get; }
        public uint Length { get; }
        public Timestamp LastAccessTime { get; }

        public override bool Equals(ClusterInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if ((this.Sectors is null) != (target.Sectors is null)) return false;
            if (!(this.Sectors is null) && !(target.Sectors is null) && !CollectionHelper.Equals(this.Sectors, target.Sectors)) return false;
            if (this.Length != target.Length) return false;
            if (this.LastAccessTime != target.LastAccessTime) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ClusterInfo>
        {
            public void Serialize(RocketPackWriter w, ClusterInfo value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Sectors.Count != 0) propertyCount++;
                    if (value.Length != default) propertyCount++;
                    if (value.LastAccessTime != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Sectors
                if (value.Sectors.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Sectors.Count);
                    foreach (var n in value.Sectors)
                    {
                        w.Write((ulong)n);
                    }
                }
                // Length
                if (value.Length != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Length);
                }
                // LastAccessTime
                if (value.LastAccessTime != default)
                {
                    w.Write((ulong)2);
                    w.Write(value.LastAccessTime);
                }
            }

            public ClusterInfo Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                IList<ulong> p_sectors = default;
                uint p_length = default;
                Timestamp p_lastAccessTime = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Sectors
                            {
                                var length = (int)r.GetUInt64();
                                p_sectors = new ulong[length];
                                for (int i = 0; i < p_sectors.Count; i++)
                                {
                                    p_sectors[i] = (ulong)r.GetUInt64();
                                }
                                break;
                            }
                        case 1: // Length
                            {
                                p_length = (uint)r.GetUInt64();
                                break;
                            }
                        case 2: // LastAccessTime
                            {
                                p_lastAccessTime = r.GetTimestamp();
                                break;
                            }
                    }
                }

                return new ClusterInfo(p_sectors, p_length, p_lastAccessTime);
            }
        }
    }

    internal sealed partial class ContentInfo : RocketPackMessageBase<ContentInfo>
    {
        static ContentInfo()
        {
            ContentInfo.Formatter = new CustomFormatter();
        }

        public static readonly int MaxLockedHashesCount = 1073741824;

        public ContentInfo(Clue clue, Timestamp creationTime, IList<OmniHash> lockedHashes, SharedBlocksInfo sharedBlocksInfo)
        {
            if (clue is null) throw new ArgumentNullException("clue");
            if (lockedHashes is null) throw new ArgumentNullException("lockedHashes");
            if (lockedHashes.Count > 1073741824) throw new ArgumentOutOfRangeException("lockedHashes");
            if (sharedBlocksInfo is null) throw new ArgumentNullException("sharedBlocksInfo");

            this.Clue = clue;
            this.CreationTime = creationTime;
            this.LockedHashes = new ReadOnlyCollection<OmniHash>(lockedHashes);
            this.SharedBlocksInfo = sharedBlocksInfo;

            {
                var hashCode = new HashCode();
                if (this.Clue != default) hashCode.Add(this.Clue.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                foreach (var n in this.LockedHashes)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                if (this.SharedBlocksInfo != default) hashCode.Add(this.SharedBlocksInfo.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public Clue Clue { get; }
        public Timestamp CreationTime { get; }
        public IReadOnlyList<OmniHash> LockedHashes { get; }
        public SharedBlocksInfo SharedBlocksInfo { get; }

        public override bool Equals(ContentInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if ((this.LockedHashes is null) != (target.LockedHashes is null)) return false;
            if (!(this.LockedHashes is null) && !(target.LockedHashes is null) && !CollectionHelper.Equals(this.LockedHashes, target.LockedHashes)) return false;
            if (this.SharedBlocksInfo != target.SharedBlocksInfo) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ContentInfo>
        {
            public void Serialize(RocketPackWriter w, ContentInfo value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Clue != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.LockedHashes.Count != 0) propertyCount++;
                    if (value.SharedBlocksInfo != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Clue
                if (value.Clue != default)
                {
                    w.Write((ulong)0);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.CreationTime);
                }
                // LockedHashes
                if (value.LockedHashes.Count != 0)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.LockedHashes.Count);
                    foreach (var n in value.LockedHashes)
                    {
                        OmniHash.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                // SharedBlocksInfo
                if (value.SharedBlocksInfo != default)
                {
                    w.Write((ulong)3);
                    SharedBlocksInfo.Formatter.Serialize(w, value.SharedBlocksInfo, rank + 1);
                }
            }

            public ContentInfo Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                Clue p_clue = default;
                Timestamp p_creationTime = default;
                IList<OmniHash> p_lockedHashes = default;
                SharedBlocksInfo p_sharedBlocksInfo = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Clue
                            {
                                p_clue = Clue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 2: // LockedHashes
                            {
                                var length = (int)r.GetUInt64();
                                p_lockedHashes = new OmniHash[length];
                                for (int i = 0; i < p_lockedHashes.Count; i++)
                                {
                                    p_lockedHashes[i] = OmniHash.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 3: // SharedBlocksInfo
                            {
                                p_sharedBlocksInfo = SharedBlocksInfo.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new ContentInfo(p_clue, p_creationTime, p_lockedHashes, p_sharedBlocksInfo);
            }
        }
    }

    internal sealed partial class SharedBlocksInfo : RocketPackMessageBase<SharedBlocksInfo>
    {
        static SharedBlocksInfo()
        {
            SharedBlocksInfo.Formatter = new CustomFormatter();
        }

        public static readonly int MaxPathLength = 1024;
        public static readonly int MaxHashesCount = 1073741824;

        public SharedBlocksInfo(string path, ulong length, uint blockLength, IList<OmniHash> hashes)
        {
            if (path is null) throw new ArgumentNullException("path");
            if (path.Length > 1024) throw new ArgumentOutOfRangeException("path");
            if (hashes is null) throw new ArgumentNullException("hashes");
            if (hashes.Count > 1073741824) throw new ArgumentOutOfRangeException("hashes");

            this.Path = path;
            this.Length = length;
            this.BlockLength = blockLength;
            this.Hashes = new ReadOnlyCollection<OmniHash>(hashes);

            {
                var hashCode = new HashCode();
                if (this.Path != default) hashCode.Add(this.Path.GetHashCode());
                if (this.Length != default) hashCode.Add(this.Length.GetHashCode());
                if (this.BlockLength != default) hashCode.Add(this.BlockLength.GetHashCode());
                foreach (var n in this.Hashes)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public string Path { get; }
        public ulong Length { get; }
        public uint BlockLength { get; }
        public IReadOnlyList<OmniHash> Hashes { get; }

        public override bool Equals(SharedBlocksInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Path != target.Path) return false;
            if (this.Length != target.Length) return false;
            if (this.BlockLength != target.BlockLength) return false;
            if ((this.Hashes is null) != (target.Hashes is null)) return false;
            if (!(this.Hashes is null) && !(target.Hashes is null) && !CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<SharedBlocksInfo>
        {
            public void Serialize(RocketPackWriter w, SharedBlocksInfo value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Path != default) propertyCount++;
                    if (value.Length != default) propertyCount++;
                    if (value.BlockLength != default) propertyCount++;
                    if (value.Hashes.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Path
                if (value.Path != default)
                {
                    w.Write((ulong)0);
                    w.Write(value.Path);
                }
                // Length
                if (value.Length != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Length);
                }
                // BlockLength
                if (value.BlockLength != default)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.BlockLength);
                }
                // Hashes
                if (value.Hashes.Count != 0)
                {
                    w.Write((ulong)3);
                    w.Write((ulong)value.Hashes.Count);
                    foreach (var n in value.Hashes)
                    {
                        OmniHash.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public SharedBlocksInfo Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                string p_path = default;
                ulong p_length = default;
                uint p_blockLength = default;
                IList<OmniHash> p_hashes = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Path
                            {
                                p_path = r.GetString(1024);
                                break;
                            }
                        case 1: // Length
                            {
                                p_length = (ulong)r.GetUInt64();
                                break;
                            }
                        case 2: // BlockLength
                            {
                                p_blockLength = (uint)r.GetUInt64();
                                break;
                            }
                        case 3: // Hashes
                            {
                                var length = (int)r.GetUInt64();
                                p_hashes = new OmniHash[length];
                                for (int i = 0; i < p_hashes.Count; i++)
                                {
                                    p_hashes[i] = OmniHash.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new SharedBlocksInfo(p_path, p_length, p_blockLength, p_hashes);
            }
        }
    }

    internal sealed partial class BlocksStorageConfig : RocketPackMessageBase<BlocksStorageConfig>
    {
        static BlocksStorageConfig()
        {
            BlocksStorageConfig.Formatter = new CustomFormatter();
        }

        public static readonly int MaxClusterInfoMapCount = 1073741824;

        public BlocksStorageConfig(uint version, IDictionary<OmniHash, ClusterInfo> clusterInfoMap, ulong size)
        {
            if (clusterInfoMap is null) throw new ArgumentNullException("clusterInfoMap");
            if (clusterInfoMap.Count > 1073741824) throw new ArgumentOutOfRangeException("clusterInfoMap");
            foreach (var n in clusterInfoMap)
            {
                if (n.Value is null) throw new ArgumentNullException("n.Value");
            }
            this.Version = version;
            this.ClusterInfoMap = new ReadOnlyDictionary<OmniHash, ClusterInfo>(clusterInfoMap);
            this.Size = size;

            {
                var hashCode = new HashCode();
                if (this.Version != default) hashCode.Add(this.Version.GetHashCode());
                foreach (var n in this.ClusterInfoMap)
                {
                    if (n.Key != default) hashCode.Add(n.Key.GetHashCode());
                    if (n.Value != default) hashCode.Add(n.Value.GetHashCode());
                }
                if (this.Size != default) hashCode.Add(this.Size.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public uint Version { get; }
        public IReadOnlyDictionary<OmniHash, ClusterInfo> ClusterInfoMap { get; }
        public ulong Size { get; }

        public override bool Equals(BlocksStorageConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if ((this.ClusterInfoMap is null) != (target.ClusterInfoMap is null)) return false;
            if (!(this.ClusterInfoMap is null) && !(target.ClusterInfoMap is null) && !CollectionHelper.Equals(this.ClusterInfoMap, target.ClusterInfoMap)) return false;
            if (this.Size != target.Size) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<BlocksStorageConfig>
        {
            public void Serialize(RocketPackWriter w, BlocksStorageConfig value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Version != default) propertyCount++;
                    if (value.ClusterInfoMap.Count != 0) propertyCount++;
                    if (value.Size != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Version
                if (value.Version != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Version);
                }
                // ClusterInfoMap
                if (value.ClusterInfoMap.Count != 0)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.ClusterInfoMap.Count);
                    foreach (var n in value.ClusterInfoMap)
                    {
                        OmniHash.Formatter.Serialize(w, n.Key, rank + 1);
                        ClusterInfo.Formatter.Serialize(w, n.Value, rank + 1);
                    }
                }
                // Size
                if (value.Size != default)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.Size);
                }
            }

            public BlocksStorageConfig Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                uint p_version = default;
                IDictionary<OmniHash, ClusterInfo> p_clusterInfoMap = default;
                ulong p_size = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Version
                            {
                                p_version = (uint)r.GetUInt64();
                                break;
                            }
                        case 1: // ClusterInfoMap
                            {
                                var length = (int)r.GetUInt64();
                                p_clusterInfoMap = new Dictionary<OmniHash, ClusterInfo>();
                                OmniHash t_key = default;
                                ClusterInfo t_value = default;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = OmniHash.Formatter.Deserialize(r, rank + 1);
                                    t_value = ClusterInfo.Formatter.Deserialize(r, rank + 1);
                                    p_clusterInfoMap[t_key] = t_value;
                                }
                                break;
                            }
                        case 2: // Size
                            {
                                p_size = (ulong)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new BlocksStorageConfig(p_version, p_clusterInfoMap, p_size);
            }
        }
    }

    internal sealed partial class ContentsManagerConfig : RocketPackMessageBase<ContentsManagerConfig>
    {
        static ContentsManagerConfig()
        {
            ContentsManagerConfig.Formatter = new CustomFormatter();
        }

        public static readonly int MaxContentInfosCount = 1073741824;

        public ContentsManagerConfig(uint version, IList<ContentInfo> contentInfos)
        {
            if (contentInfos is null) throw new ArgumentNullException("contentInfos");
            if (contentInfos.Count > 1073741824) throw new ArgumentOutOfRangeException("contentInfos");
            foreach (var n in contentInfos)
            {
                if (n is null) throw new ArgumentNullException("n");
            }

            this.Version = version;
            this.ContentInfos = new ReadOnlyCollection<ContentInfo>(contentInfos);

            {
                var hashCode = new HashCode();
                if (this.Version != default) hashCode.Add(this.Version.GetHashCode());
                foreach (var n in this.ContentInfos)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public uint Version { get; }
        public IReadOnlyList<ContentInfo> ContentInfos { get; }

        public override bool Equals(ContentsManagerConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if ((this.ContentInfos is null) != (target.ContentInfos is null)) return false;
            if (!(this.ContentInfos is null) && !(target.ContentInfos is null) && !CollectionHelper.Equals(this.ContentInfos, target.ContentInfos)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ContentsManagerConfig>
        {
            public void Serialize(RocketPackWriter w, ContentsManagerConfig value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Version != default) propertyCount++;
                    if (value.ContentInfos.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Version
                if (value.Version != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Version);
                }
                // ContentInfos
                if (value.ContentInfos.Count != 0)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.ContentInfos.Count);
                    foreach (var n in value.ContentInfos)
                    {
                        ContentInfo.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public ContentsManagerConfig Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                uint p_version = default;
                IList<ContentInfo> p_contentInfos = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Version
                            {
                                p_version = (uint)r.GetUInt64();
                                break;
                            }
                        case 1: // ContentInfos
                            {
                                var length = (int)r.GetUInt64();
                                p_contentInfos = new ContentInfo[length];
                                for (int i = 0; i < p_contentInfos.Count; i++)
                                {
                                    p_contentInfos[i] = ContentInfo.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new ContentsManagerConfig(p_version, p_contentInfos);
            }
        }
    }

}
