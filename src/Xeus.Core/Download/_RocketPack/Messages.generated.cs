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
using Xeus.Core.Contents;
using Xeus.Messages;
using Xeus.Messages.Options;
using Xeus.Messages.Reports;

namespace Xeus.Core.Download
{
    public sealed partial class DownloadItemInfo : RocketPackMessageBase<DownloadItemInfo>
    {
        static DownloadItemInfo()
        {
            DownloadItemInfo.Formatter = new CustomFormatter();
        }

        public static readonly int MaxPathLength = 1024;

        public DownloadItemInfo(Clue clue, string path, ulong maxLength, uint downloadingDepth, MerkleTreeNode downloadingMerkleTreeNode, DownloadStateType stateType)
        {
            if (clue is null) throw new ArgumentNullException("clue");
            if (path is null) throw new ArgumentNullException("path");
            if (path.Length > 1024) throw new ArgumentOutOfRangeException("path");
            if (downloadingMerkleTreeNode is null) throw new ArgumentNullException("downloadingMerkleTreeNode");
            this.Clue = clue;
            this.Path = path;
            this.MaxLength = maxLength;
            this.DownloadingDepth = downloadingDepth;
            this.DownloadingMerkleTreeNode = downloadingMerkleTreeNode;
            this.StateType = stateType;

            {
                var hashCode = new HashCode();
                if (this.Clue != default) hashCode.Add(this.Clue.GetHashCode());
                if (this.Path != default) hashCode.Add(this.Path.GetHashCode());
                if (this.MaxLength != default) hashCode.Add(this.MaxLength.GetHashCode());
                if (this.DownloadingDepth != default) hashCode.Add(this.DownloadingDepth.GetHashCode());
                if (this.DownloadingMerkleTreeNode != default) hashCode.Add(this.DownloadingMerkleTreeNode.GetHashCode());
                if (this.StateType != default) hashCode.Add(this.StateType.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public Clue Clue { get; }
        public string Path { get; }
        public ulong MaxLength { get; }
        public uint DownloadingDepth { get; }
        public MerkleTreeNode DownloadingMerkleTreeNode { get; }
        public DownloadStateType StateType { get; }

        public override bool Equals(DownloadItemInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.Path != target.Path) return false;
            if (this.MaxLength != target.MaxLength) return false;
            if (this.DownloadingDepth != target.DownloadingDepth) return false;
            if (this.DownloadingMerkleTreeNode != target.DownloadingMerkleTreeNode) return false;
            if (this.StateType != target.StateType) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<DownloadItemInfo>
        {
            public void Serialize(RocketPackWriter w, DownloadItemInfo value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Clue != default) propertyCount++;
                    if (value.Path != default) propertyCount++;
                    if (value.MaxLength != default) propertyCount++;
                    if (value.DownloadingDepth != default) propertyCount++;
                    if (value.DownloadingMerkleTreeNode != default) propertyCount++;
                    if (value.StateType != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Clue
                if (value.Clue != default)
                {
                    w.Write((ulong)0);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                // Path
                if (value.Path != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.Path);
                }
                // MaxLength
                if (value.MaxLength != default)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.MaxLength);
                }
                // DownloadingDepth
                if (value.DownloadingDepth != default)
                {
                    w.Write((ulong)3);
                    w.Write((ulong)value.DownloadingDepth);
                }
                // DownloadingMerkleTreeNode
                if (value.DownloadingMerkleTreeNode != default)
                {
                    w.Write((ulong)4);
                    MerkleTreeNode.Formatter.Serialize(w, value.DownloadingMerkleTreeNode, rank + 1);
                }
                // StateType
                if (value.StateType != default)
                {
                    w.Write((ulong)5);
                    w.Write((ulong)value.StateType);
                }
            }

            public DownloadItemInfo Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                Clue p_clue = default;
                string p_path = default;
                ulong p_maxLength = default;
                uint p_downloadingDepth = default;
                MerkleTreeNode p_downloadingMerkleTreeNode = default;
                DownloadStateType p_stateType = default;

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
                        case 1: // Path
                            {
                                p_path = r.GetString(1024);
                                break;
                            }
                        case 2: // MaxLength
                            {
                                p_maxLength = (ulong)r.GetUInt64();
                                break;
                            }
                        case 3: // DownloadingDepth
                            {
                                p_downloadingDepth = (uint)r.GetUInt64();
                                break;
                            }
                        case 4: // DownloadingMerkleTreeNode
                            {
                                p_downloadingMerkleTreeNode = MerkleTreeNode.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 5: // StateType
                            {
                                p_stateType = (DownloadStateType)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new DownloadItemInfo(p_clue, p_path, p_maxLength, p_downloadingDepth, p_downloadingMerkleTreeNode, p_stateType);
            }
        }
    }

}
