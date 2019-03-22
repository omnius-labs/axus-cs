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

namespace Xeus.Core.Internal
{
    internal enum CorrectionAlgorithmType : byte
    {
        ReedSolomon8 = 0,
    }

    internal sealed partial class MerkleTreeSection : RocketPackMessageBase<MerkleTreeSection>
    {
        static MerkleTreeSection()
        {
            MerkleTreeSection.Formatter = new CustomFormatter();
        }

        public static readonly int MaxHashesCount = 1048576;

        public MerkleTreeSection(CorrectionAlgorithmType correctionAlgorithmType, ulong length, IList<OmniHash> hashes)
        {
            if (hashes is null) throw new ArgumentNullException("hashes");
            if (hashes.Count > 1048576) throw new ArgumentOutOfRangeException("hashes");

            this.CorrectionAlgorithmType = correctionAlgorithmType;
            this.Length = length;
            this.Hashes = new ReadOnlyCollection<OmniHash>(hashes);

            {
                var hashCode = new HashCode();
                if (this.CorrectionAlgorithmType != default) hashCode.Add(this.CorrectionAlgorithmType.GetHashCode());
                if (this.Length != default) hashCode.Add(this.Length.GetHashCode());
                foreach (var n in this.Hashes)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public CorrectionAlgorithmType CorrectionAlgorithmType { get; }
        public ulong Length { get; }
        public IReadOnlyList<OmniHash> Hashes { get; }

        public override bool Equals(MerkleTreeSection target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.CorrectionAlgorithmType != target.CorrectionAlgorithmType) return false;
            if (this.Length != target.Length) return false;
            if ((this.Hashes is null) != (target.Hashes is null)) return false;
            if (!(this.Hashes is null) && !(target.Hashes is null) && !CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<MerkleTreeSection>
        {
            public void Serialize(RocketPackWriter w, MerkleTreeSection value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.CorrectionAlgorithmType != default) propertyCount++;
                    if (value.Length != default) propertyCount++;
                    if (value.Hashes.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // CorrectionAlgorithmType
                if (value.CorrectionAlgorithmType != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.CorrectionAlgorithmType);
                }
                // Length
                if (value.Length != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Length);
                }
                // Hashes
                if (value.Hashes.Count != 0)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.Hashes.Count);
                    foreach (var n in value.Hashes)
                    {
                        OmniHash.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public MerkleTreeSection Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                CorrectionAlgorithmType p_correctionAlgorithmType = default;
                ulong p_length = default;
                IList<OmniHash> p_hashes = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // CorrectionAlgorithmType
                            {
                                p_correctionAlgorithmType = (CorrectionAlgorithmType)r.GetUInt64();
                                break;
                            }
                        case 1: // Length
                            {
                                p_length = (ulong)r.GetUInt64();
                                break;
                            }
                        case 2: // Hashes
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

                return new MerkleTreeSection(p_correctionAlgorithmType, p_length, p_hashes);
            }
        }
    }

    internal sealed partial class MerkleTreeNode : RocketPackMessageBase<MerkleTreeNode>
    {
        static MerkleTreeNode()
        {
            MerkleTreeNode.Formatter = new CustomFormatter();
        }

        public static readonly int MaxSectionsCount = 1048576;

        public MerkleTreeNode(IList<MerkleTreeSection> sections)
        {
            if (sections is null) throw new ArgumentNullException("sections");
            if (sections.Count > 1048576) throw new ArgumentOutOfRangeException("sections");
            foreach (var n in sections)
            {
                if (n is null) throw new ArgumentNullException("n");
            }

            this.Sections = new ReadOnlyCollection<MerkleTreeSection>(sections);

            {
                var hashCode = new HashCode();
                foreach (var n in this.Sections)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public IReadOnlyList<MerkleTreeSection> Sections { get; }

        public override bool Equals(MerkleTreeNode target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if ((this.Sections is null) != (target.Sections is null)) return false;
            if (!(this.Sections is null) && !(target.Sections is null) && !CollectionHelper.Equals(this.Sections, target.Sections)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<MerkleTreeNode>
        {
            public void Serialize(RocketPackWriter w, MerkleTreeNode value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Sections.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Sections
                if (value.Sections.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Sections.Count);
                    foreach (var n in value.Sections)
                    {
                        MerkleTreeSection.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public MerkleTreeNode Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                IList<MerkleTreeSection> p_sections = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Sections
                            {
                                var length = (int)r.GetUInt64();
                                p_sections = new MerkleTreeSection[length];
                                for (int i = 0; i < p_sections.Count; i++)
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
