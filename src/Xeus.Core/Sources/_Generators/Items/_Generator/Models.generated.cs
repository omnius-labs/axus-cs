using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Amoeba.Messages;
using Newtonsoft.Json;
using Omnius.Base;
using Omnius.Serialization;
using Omnius.Utils;

namespace Amoeba.Service
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class ProtectedCacheInfo : ItemBase<ProtectedCacheInfo>
    {
        [JsonConstructor]
        public ProtectedCacheInfo(DateTime creationTime, IEnumerable<Hash> hashes)
        {
            this.CreationTime = creationTime;
            this.Hashes = new ReadOnlyCollection<Hash>(hashes.ToArray());
        }
        private DateTime _creationTime;
        [JsonProperty]
        public DateTime CreationTime
        {
            get => _creationTime;
            private set => _creationTime = value.Normalize();
        }
        [JsonProperty]
        public IReadOnlyList<Hash> Hashes { get; }
        public override bool Equals(ProtectedCacheInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (!CollectionUtils.Equals(this.Hashes, target.Hashes)) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                for (int i = 0; i < Hashes.Count; i++)
                {
                    h ^= this.Hashes[i].GetHashCode();
                }
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class ClusterInfo : ItemBase<ClusterInfo>
    {
        [JsonConstructor]
        public ClusterInfo(long[] indexes, int length, DateTime updateTime)
        {
            this.Indexes = indexes;
            this.Length = length;
            this.UpdateTime = updateTime;
        }
        [JsonProperty]
        public long[] Indexes { get; }
        [JsonProperty]
        public int Length { get; }
        private DateTime _updateTime;
        [JsonProperty]
        public DateTime UpdateTime
        {
            get => _updateTime;
            set => _updateTime = value.Normalize();
        }
        public override bool Equals(ClusterInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Indexes != target.Indexes) return false;
            if (this.Length != target.Length) return false;
            if (this.UpdateTime != target.UpdateTime) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Indexes != default(long[])) h ^= this.Indexes.GetHashCode();
                if (this.Length != default(int)) h ^= this.Length.GetHashCode();
                if (this.UpdateTime != default(DateTime)) h ^= this.UpdateTime.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class DownloadItemInfo : ItemBase<DownloadItemInfo>
    {
        [JsonConstructor]
        public DownloadItemInfo(Metadata metadata, string path, long maxLength, int depth, Index index, DownloadState state, IEnumerable<Hash> resultHashes)
        {
            this.Metadata = metadata;
            this.Path = path;
            this.MaxLength = maxLength;
            this.Depth = depth;
            this.Index = index;
            this.State = state;
            this.ResultHashes = new List<Hash>(resultHashes.ToArray());
        }
        [JsonProperty]
        public Metadata Metadata { get; }
        [JsonProperty]
        public string Path { get; }
        [JsonProperty]
        public long MaxLength { get; set; }
        [JsonProperty]
        public int Depth { get; set; }
        [JsonProperty]
        public Index Index { get; set; }
        [JsonProperty]
        public DownloadState State { get; set; }
        [JsonProperty]
        public IList<Hash> ResultHashes { get; }
        public override bool Equals(DownloadItemInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Metadata != target.Metadata) return false;
            if (this.Path != target.Path) return false;
            if (this.MaxLength != target.MaxLength) return false;
            if (this.Depth != target.Depth) return false;
            if (this.Index != target.Index) return false;
            if (this.State != target.State) return false;
            if (!CollectionUtils.Equals(this.ResultHashes, target.ResultHashes)) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                if (this.Path != default(string)) h ^= this.Path.GetHashCode();
                if (this.MaxLength != default(long)) h ^= this.MaxLength.GetHashCode();
                if (this.Depth != default(int)) h ^= this.Depth.GetHashCode();
                if (this.Index != default(Index)) h ^= this.Index.GetHashCode();
                if (this.State != default(DownloadState)) h ^= this.State.GetHashCode();
                for (int i = 0; i < ResultHashes.Count; i++)
                {
                    h ^= this.ResultHashes[i].GetHashCode();
                }
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class ContentInfo : ItemBase<ContentInfo>
    {
        [JsonConstructor]
        public ContentInfo(DateTime creationTime, TimeSpan lifeSpan, Metadata metadata, IEnumerable<Hash> lockedHashes, ShareInfo shareInfo)
        {
            this.CreationTime = creationTime;
            this.LifeSpan = lifeSpan;
            this.Metadata = metadata;
            this.LockedHashes = new ReadOnlyCollection<Hash>(lockedHashes.ToArray());
            this.ShareInfo = shareInfo;
        }
        private DateTime _creationTime;
        [JsonProperty]
        public DateTime CreationTime
        {
            get => _creationTime;
            private set => _creationTime = value.Normalize();
        }
        [JsonProperty]
        public TimeSpan LifeSpan { get; }
        [JsonProperty]
        public Metadata Metadata { get; }
        [JsonProperty]
        public IReadOnlyList<Hash> LockedHashes { get; }
        [JsonProperty]
        public ShareInfo ShareInfo { get; }
        public override bool Equals(ContentInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.LifeSpan != target.LifeSpan) return false;
            if (this.Metadata != target.Metadata) return false;
            if (!CollectionUtils.Equals(this.LockedHashes, target.LockedHashes)) return false;
            if (this.ShareInfo != target.ShareInfo) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.LifeSpan != default(TimeSpan)) h ^= this.LifeSpan.GetHashCode();
                if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                for (int i = 0; i < LockedHashes.Count; i++)
                {
                    h ^= this.LockedHashes[i].GetHashCode();
                }
                if (this.ShareInfo != default(ShareInfo)) h ^= this.ShareInfo.GetHashCode();
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class ShareInfo : ItemBase<ShareInfo>
    {
        [JsonConstructor]
        public ShareInfo(string path, long fileLength, int blockLength, IEnumerable<Hash> hashes)
        {
            this.Path = path;
            this.FileLength = fileLength;
            this.BlockLength = blockLength;
            this.Hashes = new ReadOnlyCollection<Hash>(hashes.ToArray());
        }
        [JsonProperty]
        public string Path { get; }
        [JsonProperty]
        public long FileLength { get; }
        [JsonProperty]
        public int BlockLength { get; }
        [JsonProperty]
        public IReadOnlyList<Hash> Hashes { get; }
        public override bool Equals(ShareInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Path != target.Path) return false;
            if (this.FileLength != target.FileLength) return false;
            if (this.BlockLength != target.BlockLength) return false;
            if (!CollectionUtils.Equals(this.Hashes, target.Hashes)) return false;
            return true;
        }
        private int? _hashCode;
        public override int GetHashCode()
        {
            if (!_hashCode.HasValue)
            {
                int h = 0;
                if (this.Path != default(string)) h ^= this.Path.GetHashCode();
                if (this.FileLength != default(long)) h ^= this.FileLength.GetHashCode();
                if (this.BlockLength != default(int)) h ^= this.BlockLength.GetHashCode();
                for (int i = 0; i < Hashes.Count; i++)
                {
                    h ^= this.Hashes[i].GetHashCode();
                }
                _hashCode = h;
            }
            return _hashCode.Value;
        }
    }
}

