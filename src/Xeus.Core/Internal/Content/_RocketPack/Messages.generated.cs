using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Core.Internal;
using Xeus.Messages;

#nullable enable

namespace Xeus.Core.Internal.Content
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

}
