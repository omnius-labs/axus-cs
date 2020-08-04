using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Models;

#nullable enable

namespace Omnius.Xeus.Service.Storages.Internal
{
    internal sealed partial class MerkleTreeSection : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection>.Formatter;
        public static global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection>.Empty;

        static MerkleTreeSection()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection>.Empty = new global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection(0, 0, global::System.Array.Empty<OmniHash>());
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

        public static global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection? left, global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection? left, global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Depth != target.Depth) return false;
            if (this.Length != target.Length) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection value, in int rank)
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

            public global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Storages.Internal.MerkleTreeSection(p_depth, p_length, p_hashes);
            }
        }
    }

}
