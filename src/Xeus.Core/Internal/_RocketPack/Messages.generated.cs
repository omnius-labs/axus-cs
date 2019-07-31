using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Messages;

#nullable enable

namespace Xeus.Core.Internal
{
    internal enum CorrectionAlgorithmType : byte
    {
        ReedSolomon8 = 0,
    }

    internal sealed partial class MerkleTreeSection : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<MerkleTreeSection>
    {
        static MerkleTreeSection()
        {
            MerkleTreeSection.Formatter = new CustomFormatter();
            MerkleTreeSection.Empty = new MerkleTreeSection((CorrectionAlgorithmType)0, 0, global::System.Array.Empty<OmniHash>());
        }

        private readonly int __hashCode;

        public static readonly int MaxHashesCount = 1048576;

        public MerkleTreeSection(CorrectionAlgorithmType correctionAlgorithmType, ulong length, OmniHash[] hashes)
        {
            if (hashes is null) throw new global::System.ArgumentNullException("hashes");
            if (hashes.Length > 1048576) throw new global::System.ArgumentOutOfRangeException("hashes");

            this.CorrectionAlgorithmType = correctionAlgorithmType;
            this.Length = length;
            this.Hashes = new global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash>(hashes);

            {
                var __h = new global::System.HashCode();
                if (this.CorrectionAlgorithmType != default) __h.Add(this.CorrectionAlgorithmType.GetHashCode());
                if (this.Length != default) __h.Add(this.Length.GetHashCode());
                foreach (var n in this.Hashes)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public CorrectionAlgorithmType CorrectionAlgorithmType { get; }
        public ulong Length { get; }
        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash> Hashes { get; }

        public override bool Equals(MerkleTreeSection target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CorrectionAlgorithmType != target.CorrectionAlgorithmType) return false;
            if (this.Length != target.Length) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MerkleTreeSection>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, MerkleTreeSection value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CorrectionAlgorithmType != (CorrectionAlgorithmType)0)
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

                if (value.CorrectionAlgorithmType != (CorrectionAlgorithmType)0)
                {
                    w.Write((uint)0);
                    w.Write((ulong)value.CorrectionAlgorithmType);
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
                        OmniHash.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public MerkleTreeSection Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                CorrectionAlgorithmType p_correctionAlgorithmType = (CorrectionAlgorithmType)0;
                ulong p_length = 0;
                OmniHash[] p_hashes = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_correctionAlgorithmType = (CorrectionAlgorithmType)r.GetUInt64();
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
                                    p_hashes[i] = OmniHash.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new MerkleTreeSection(p_correctionAlgorithmType, p_length, p_hashes);
            }
        }
    }

    internal sealed partial class MerkleTreeNode : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<MerkleTreeNode>
    {
        static MerkleTreeNode()
        {
            MerkleTreeNode.Formatter = new CustomFormatter();
            MerkleTreeNode.Empty = new MerkleTreeNode(global::System.Array.Empty<MerkleTreeSection>());
        }

        private readonly int __hashCode;

        public static readonly int MaxSectionsCount = 1048576;

        public MerkleTreeNode(MerkleTreeSection[] sections)
        {
            if (sections is null) throw new global::System.ArgumentNullException("sections");
            if (sections.Length > 1048576) throw new global::System.ArgumentOutOfRangeException("sections");
            foreach (var n in sections)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Sections = new global::Omnix.DataStructures.ReadOnlyListSlim<MerkleTreeSection>(sections);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Sections)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<MerkleTreeSection> Sections { get; }

        public override bool Equals(MerkleTreeNode target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Sections, target.Sections)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MerkleTreeNode>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, MerkleTreeNode value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Sections.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Sections.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Sections.Count);
                    foreach (var n in value.Sections)
                    {
                        MerkleTreeSection.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public MerkleTreeNode Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                MerkleTreeSection[] p_sections = global::System.Array.Empty<MerkleTreeSection>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_sections = new MerkleTreeSection[length];
                                for (int i = 0; i < p_sections.Length; i++)
                                {
                                    p_sections[i] = MerkleTreeSection.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new MerkleTreeNode(p_sections);
            }
        }
    }

}
