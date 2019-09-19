using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Core;

#nullable enable

namespace Xeus.Core.Storage.Internal
{
    internal sealed partial class ClusterMetadata : global::Omnix.Serialization.OmniPack.IOmniPackMessage<ClusterMetadata>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<ClusterMetadata> Formatter { get; }
        public static ClusterMetadata Empty { get; }

        static ClusterMetadata()
        {
            ClusterMetadata.Formatter = new ___CustomFormatter();
            ClusterMetadata.Empty = new ClusterMetadata(global::System.Array.Empty<ulong>(), 0, global::Omnix.Serialization.OmniPack.Timestamp.Zero);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxSectorsCount = 256;

        public ClusterMetadata(ulong[] sectors, uint length, global::Omnix.Serialization.OmniPack.Timestamp lastAccessTime)
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
        public global::Omnix.Serialization.OmniPack.Timestamp LastAccessTime { get; }

        public static ClusterMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
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

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<ClusterMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in ClusterMetadata value, in int rank)
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
                    if (value.LastAccessTime != global::Omnix.Serialization.OmniPack.Timestamp.Zero)
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
                if (value.LastAccessTime != global::Omnix.Serialization.OmniPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.LastAccessTime);
                }
            }

            public ClusterMetadata Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ulong[] p_sectors = global::System.Array.Empty<ulong>();
                uint p_length = 0;
                global::Omnix.Serialization.OmniPack.Timestamp p_lastAccessTime = global::Omnix.Serialization.OmniPack.Timestamp.Zero;

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

    internal sealed partial class BlockStorageConfig : global::Omnix.Serialization.OmniPack.IOmniPackMessage<BlockStorageConfig>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<BlockStorageConfig> Formatter { get; }
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
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
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

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<BlockStorageConfig>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in BlockStorageConfig value, in int rank)
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

            public BlockStorageConfig Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
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

}
