using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Core;

#nullable enable

namespace Xeus.Core.Repositories.Internal
{
    internal sealed partial class MerkleTreeNode : global::Omnix.Serialization.OmniPack.IOmniPackMessage<MerkleTreeNode>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<MerkleTreeNode> Formatter { get; }
        public static MerkleTreeNode Empty { get; }

        static MerkleTreeNode()
        {
            MerkleTreeNode.Formatter = new ___CustomFormatter();
            MerkleTreeNode.Empty = new MerkleTreeNode(0, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxHashesCount = 1073741824;

        public MerkleTreeNode(ulong length, OmniHash[] hashes)
        {
            if (hashes is null) throw new global::System.ArgumentNullException("hashes");
            if (hashes.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("hashes");

            this.Length = length;
            this.Hashes = new global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash>(hashes);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (length != default) ___h.Add(length.GetHashCode());
                foreach (var n in hashes)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public ulong Length { get; }
        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash> Hashes { get; }

        public static MerkleTreeNode Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(MerkleTreeNode? left, MerkleTreeNode? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(MerkleTreeNode? left, MerkleTreeNode? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is MerkleTreeNode)) return false;
            return this.Equals((MerkleTreeNode)other);
        }
        public bool Equals(MerkleTreeNode? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Length != target.Length) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<MerkleTreeNode>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in MerkleTreeNode value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
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

                if (value.Length != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.Length);
                }
                if (value.Hashes.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.Hashes.Count);
                    foreach (var n in value.Hashes)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public MerkleTreeNode Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ulong p_length = 0;
                OmniHash[] p_hashes = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_length = r.GetUInt64();
                                break;
                            }
                        case 1:
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

                return new MerkleTreeNode(p_length, p_hashes);
            }
        }
    }

    internal sealed partial class SharedFileMetadata : global::Omnix.Serialization.OmniPack.IOmniPackMessage<SharedFileMetadata>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<SharedFileMetadata> Formatter { get; }
        public static SharedFileMetadata Empty { get; }

        static SharedFileMetadata()
        {
            SharedFileMetadata.Formatter = new ___CustomFormatter();
            SharedFileMetadata.Empty = new SharedFileMetadata(string.Empty, 0, 0, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPathLength = 1024;
        public static readonly int MaxHashesCount = 1073741824;

        public SharedFileMetadata(string path, ulong length, uint blockLength, OmniHash[] hashes)
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

        public static SharedFileMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(SharedFileMetadata? left, SharedFileMetadata? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(SharedFileMetadata? left, SharedFileMetadata? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is SharedFileMetadata)) return false;
            return this.Equals((SharedFileMetadata)other);
        }
        public bool Equals(SharedFileMetadata? target)
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

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<SharedFileMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in SharedFileMetadata value, in int rank)
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

            public SharedFileMetadata Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
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

                return new SharedFileMetadata(p_path, p_length, p_blockLength, p_hashes);
            }
        }
    }

    internal sealed partial class ContentMetadata : global::Omnix.Serialization.OmniPack.IOmniPackMessage<ContentMetadata>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<ContentMetadata> Formatter { get; }
        public static ContentMetadata Empty { get; }

        static ContentMetadata()
        {
            ContentMetadata.Formatter = new ___CustomFormatter();
            ContentMetadata.Empty = new ContentMetadata(Clue.Empty, global::System.Array.Empty<MerkleTreeNode>(), null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxMerkleTreeNodesCount = 32;

        public ContentMetadata(Clue clue, MerkleTreeNode[] merkleTreeNodes, SharedFileMetadata? sharedFileMetadata)
        {
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (merkleTreeNodes is null) throw new global::System.ArgumentNullException("merkleTreeNodes");
            if (merkleTreeNodes.Length > 32) throw new global::System.ArgumentOutOfRangeException("merkleTreeNodes");
            foreach (var n in merkleTreeNodes)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            this.Clue = clue;
            this.MerkleTreeNodes = new global::Omnix.DataStructures.ReadOnlyListSlim<MerkleTreeNode>(merkleTreeNodes);
            this.SharedFileMetadata = sharedFileMetadata;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (clue != default) ___h.Add(clue.GetHashCode());
                foreach (var n in merkleTreeNodes)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                if (sharedFileMetadata != default) ___h.Add(sharedFileMetadata.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public Clue Clue { get; }
        public global::Omnix.DataStructures.ReadOnlyListSlim<MerkleTreeNode> MerkleTreeNodes { get; }
        public SharedFileMetadata? SharedFileMetadata { get; }

        public static ContentMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
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
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.MerkleTreeNodes, target.MerkleTreeNodes)) return false;
            if ((this.SharedFileMetadata is null) != (target.SharedFileMetadata is null)) return false;
            if (!(this.SharedFileMetadata is null) && !(target.SharedFileMetadata is null) && this.SharedFileMetadata != target.SharedFileMetadata) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<ContentMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in ContentMetadata value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Clue != Clue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.MerkleTreeNodes.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.SharedFileMetadata != null)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Clue != Clue.Empty)
                {
                    w.Write((uint)0);
                    Clue.Formatter.Serialize(ref w, value.Clue, rank + 1);
                }
                if (value.MerkleTreeNodes.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.MerkleTreeNodes.Count);
                    foreach (var n in value.MerkleTreeNodes)
                    {
                        MerkleTreeNode.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.SharedFileMetadata != null)
                {
                    w.Write((uint)2);
                    SharedFileMetadata.Formatter.Serialize(ref w, value.SharedFileMetadata, rank + 1);
                }
            }

            public ContentMetadata Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                Clue p_clue = Clue.Empty;
                MerkleTreeNode[] p_merkleTreeNodes = global::System.Array.Empty<MerkleTreeNode>();
                SharedFileMetadata? p_sharedFileMetadata = null;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_clue = Clue.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_merkleTreeNodes = new MerkleTreeNode[length];
                                for (int i = 0; i < p_merkleTreeNodes.Length; i++)
                                {
                                    p_merkleTreeNodes[i] = MerkleTreeNode.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                p_sharedFileMetadata = SharedFileMetadata.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new ContentMetadata(p_clue, p_merkleTreeNodes, p_sharedFileMetadata);
            }
        }
    }

    internal sealed partial class DownloadingContentMetadata : global::Omnix.Serialization.OmniPack.IOmniPackMessage<DownloadingContentMetadata>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<DownloadingContentMetadata> Formatter { get; }
        public static DownloadingContentMetadata Empty { get; }

        static DownloadingContentMetadata()
        {
            DownloadingContentMetadata.Formatter = new ___CustomFormatter();
            DownloadingContentMetadata.Empty = new DownloadingContentMetadata(Clue.Empty, 0, 0, global::System.Array.Empty<MerkleTreeNode>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxDownloadingMerkleTreeNodesCount = 32;

        public DownloadingContentMetadata(Clue clue, ulong maxLength, uint downloadingDepth, MerkleTreeNode[] downloadingMerkleTreeNodes)
        {
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (downloadingMerkleTreeNodes is null) throw new global::System.ArgumentNullException("downloadingMerkleTreeNodes");
            if (downloadingMerkleTreeNodes.Length > 32) throw new global::System.ArgumentOutOfRangeException("downloadingMerkleTreeNodes");
            foreach (var n in downloadingMerkleTreeNodes)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Clue = clue;
            this.MaxLength = maxLength;
            this.DownloadingDepth = downloadingDepth;
            this.DownloadingMerkleTreeNodes = new global::Omnix.DataStructures.ReadOnlyListSlim<MerkleTreeNode>(downloadingMerkleTreeNodes);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (clue != default) ___h.Add(clue.GetHashCode());
                if (maxLength != default) ___h.Add(maxLength.GetHashCode());
                if (downloadingDepth != default) ___h.Add(downloadingDepth.GetHashCode());
                foreach (var n in downloadingMerkleTreeNodes)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public Clue Clue { get; }
        public ulong MaxLength { get; }
        public uint DownloadingDepth { get; }
        public global::Omnix.DataStructures.ReadOnlyListSlim<MerkleTreeNode> DownloadingMerkleTreeNodes { get; }

        public static DownloadingContentMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(DownloadingContentMetadata? left, DownloadingContentMetadata? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(DownloadingContentMetadata? left, DownloadingContentMetadata? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is DownloadingContentMetadata)) return false;
            return this.Equals((DownloadingContentMetadata)other);
        }
        public bool Equals(DownloadingContentMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.MaxLength != target.MaxLength) return false;
            if (this.DownloadingDepth != target.DownloadingDepth) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.DownloadingMerkleTreeNodes, target.DownloadingMerkleTreeNodes)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<DownloadingContentMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in DownloadingContentMetadata value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Clue != Clue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.MaxLength != 0)
                    {
                        propertyCount++;
                    }
                    if (value.DownloadingDepth != 0)
                    {
                        propertyCount++;
                    }
                    if (value.DownloadingMerkleTreeNodes.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Clue != Clue.Empty)
                {
                    w.Write((uint)0);
                    Clue.Formatter.Serialize(ref w, value.Clue, rank + 1);
                }
                if (value.MaxLength != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.MaxLength);
                }
                if (value.DownloadingDepth != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.DownloadingDepth);
                }
                if (value.DownloadingMerkleTreeNodes.Count != 0)
                {
                    w.Write((uint)3);
                    w.Write((uint)value.DownloadingMerkleTreeNodes.Count);
                    foreach (var n in value.DownloadingMerkleTreeNodes)
                    {
                        MerkleTreeNode.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public DownloadingContentMetadata Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                Clue p_clue = Clue.Empty;
                ulong p_maxLength = 0;
                uint p_downloadingDepth = 0;
                MerkleTreeNode[] p_downloadingMerkleTreeNodes = global::System.Array.Empty<MerkleTreeNode>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_clue = Clue.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_maxLength = r.GetUInt64();
                                break;
                            }
                        case 2:
                            {
                                p_downloadingDepth = r.GetUInt32();
                                break;
                            }
                        case 3:
                            {
                                var length = r.GetUInt32();
                                p_downloadingMerkleTreeNodes = new MerkleTreeNode[length];
                                for (int i = 0; i < p_downloadingMerkleTreeNodes.Length; i++)
                                {
                                    p_downloadingMerkleTreeNodes[i] = MerkleTreeNode.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new DownloadingContentMetadata(p_clue, p_maxLength, p_downloadingDepth, p_downloadingMerkleTreeNodes);
            }
        }
    }

    internal sealed partial class RepositoryConfig : global::Omnix.Serialization.OmniPack.IOmniPackMessage<RepositoryConfig>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<RepositoryConfig> Formatter { get; }
        public static RepositoryConfig Empty { get; }

        static RepositoryConfig()
        {
            RepositoryConfig.Formatter = new ___CustomFormatter();
            RepositoryConfig.Empty = new RepositoryConfig(0, global::System.Array.Empty<ContentMetadata>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxContentMetadatasCount = 1073741824;

        public RepositoryConfig(uint version, ContentMetadata[] contentMetadatas)
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

        public static RepositoryConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(RepositoryConfig? left, RepositoryConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(RepositoryConfig? left, RepositoryConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is RepositoryConfig)) return false;
            return this.Equals((RepositoryConfig)other);
        }
        public bool Equals(RepositoryConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ContentMetadatas, target.ContentMetadatas)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<RepositoryConfig>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in RepositoryConfig value, in int rank)
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

            public RepositoryConfig Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
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

                return new RepositoryConfig(p_version, p_contentMetadatas);
            }
        }
    }

}
