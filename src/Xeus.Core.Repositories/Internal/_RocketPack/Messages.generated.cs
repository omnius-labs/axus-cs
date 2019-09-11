using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Core;

#nullable enable

namespace Xeus.Core.Repositories.Internal
{
    internal sealed partial class MerkleTreeNode : global::Omnix.Serialization.RocketPack.IRocketPackMessage<MerkleTreeNode>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MerkleTreeNode> Formatter { get; }
        public static MerkleTreeNode Empty { get; }

        static MerkleTreeNode()
        {
            MerkleTreeNode.Formatter = new ___CustomFormatter();
            MerkleTreeNode.Empty = new MerkleTreeNode(0, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxHashesCount = 1048576;

        public MerkleTreeNode(ulong length, OmniHash[] hashes)
        {
            if (hashes is null) throw new global::System.ArgumentNullException("hashes");
            if (hashes.Length > 1048576) throw new global::System.ArgumentOutOfRangeException("hashes");

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

        public static MerkleTreeNode Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MerkleTreeNode>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in MerkleTreeNode value, in int rank)
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

            public MerkleTreeNode Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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

    internal sealed partial class SharedBlocksMetadata : global::Omnix.Serialization.RocketPack.IRocketPackMessage<SharedBlocksMetadata>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<SharedBlocksMetadata> Formatter { get; }
        public static SharedBlocksMetadata Empty { get; }

        static SharedBlocksMetadata()
        {
            SharedBlocksMetadata.Formatter = new ___CustomFormatter();
            SharedBlocksMetadata.Empty = new SharedBlocksMetadata(string.Empty, 0, 0, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPathLength = 1024;
        public static readonly int MaxHashesCount = 1073741824;

        public SharedBlocksMetadata(string path, ulong length, uint blockLength, OmniHash[] hashes)
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

        public static SharedBlocksMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(SharedBlocksMetadata? left, SharedBlocksMetadata? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(SharedBlocksMetadata? left, SharedBlocksMetadata? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is SharedBlocksMetadata)) return false;
            return this.Equals((SharedBlocksMetadata)other);
        }
        public bool Equals(SharedBlocksMetadata? target)
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

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<SharedBlocksMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in SharedBlocksMetadata value, in int rank)
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

            public SharedBlocksMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new SharedBlocksMetadata(p_path, p_length, p_blockLength, p_hashes);
            }
        }
    }

    internal sealed partial class ContentMetadata : global::Omnix.Serialization.RocketPack.IRocketPackMessage<ContentMetadata>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentMetadata> Formatter { get; }
        public static ContentMetadata Empty { get; }

        static ContentMetadata()
        {
            ContentMetadata.Formatter = new ___CustomFormatter();
            ContentMetadata.Empty = new ContentMetadata(XeusClue.Empty, global::System.Array.Empty<OmniHash>(), null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxLockedHashesCount = 1073741824;

        public ContentMetadata(XeusClue clue, OmniHash[] lockedHashes, SharedBlocksMetadata? sharedBlocksMetadata)
        {
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (lockedHashes is null) throw new global::System.ArgumentNullException("lockedHashes");
            if (lockedHashes.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("lockedHashes");
            this.Clue = clue;
            this.LockedHashes = new global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash>(lockedHashes);
            this.SharedBlocksMetadata = sharedBlocksMetadata;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (clue != default) ___h.Add(clue.GetHashCode());
                foreach (var n in lockedHashes)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                if (sharedBlocksMetadata != default) ___h.Add(sharedBlocksMetadata.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public XeusClue Clue { get; }
        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniHash> LockedHashes { get; }
        public SharedBlocksMetadata? SharedBlocksMetadata { get; }

        public static ContentMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.LockedHashes, target.LockedHashes)) return false;
            if ((this.SharedBlocksMetadata is null) != (target.SharedBlocksMetadata is null)) return false;
            if (!(this.SharedBlocksMetadata is null) && !(target.SharedBlocksMetadata is null) && this.SharedBlocksMetadata != target.SharedBlocksMetadata) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in ContentMetadata value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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
                    XeusClue.Formatter.Serialize(ref w, value.Clue, rank + 1);
                }
                if (value.LockedHashes.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.LockedHashes.Count);
                    foreach (var n in value.LockedHashes)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.SharedBlocksMetadata != null)
                {
                    w.Write((uint)2);
                    SharedBlocksMetadata.Formatter.Serialize(ref w, value.SharedBlocksMetadata, rank + 1);
                }
            }

            public ContentMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                XeusClue p_clue = XeusClue.Empty;
                OmniHash[] p_lockedHashes = global::System.Array.Empty<OmniHash>();
                SharedBlocksMetadata? p_sharedBlocksMetadata = null;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_lockedHashes = new OmniHash[length];
                                for (int i = 0; i < p_lockedHashes.Length; i++)
                                {
                                    p_lockedHashes[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                p_sharedBlocksMetadata = SharedBlocksMetadata.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new ContentMetadata(p_clue, p_lockedHashes, p_sharedBlocksMetadata);
            }
        }
    }

    internal sealed partial class ContentStorageConfig : global::Omnix.Serialization.RocketPack.IRocketPackMessage<ContentStorageConfig>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentStorageConfig> Formatter { get; }
        public static ContentStorageConfig Empty { get; }

        static ContentStorageConfig()
        {
            ContentStorageConfig.Formatter = new ___CustomFormatter();
            ContentStorageConfig.Empty = new ContentStorageConfig(0, global::System.Array.Empty<ContentMetadata>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxContentMetadatasCount = 1073741824;

        public ContentStorageConfig(uint version, ContentMetadata[] contentMetadatas)
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

        public static ContentStorageConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ContentStorageConfig? left, ContentStorageConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ContentStorageConfig? left, ContentStorageConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ContentStorageConfig)) return false;
            return this.Equals((ContentStorageConfig)other);
        }
        public bool Equals(ContentStorageConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ContentMetadatas, target.ContentMetadatas)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentStorageConfig>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in ContentStorageConfig value, in int rank)
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

            public ContentStorageConfig Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new ContentStorageConfig(p_version, p_contentMetadatas);
            }
        }
    }

    internal sealed partial class DownloadingContentMetadata : global::Omnix.Serialization.RocketPack.IRocketPackMessage<DownloadingContentMetadata>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DownloadingContentMetadata> Formatter { get; }
        public static DownloadingContentMetadata Empty { get; }

        static DownloadingContentMetadata()
        {
            DownloadingContentMetadata.Formatter = new ___CustomFormatter();
            DownloadingContentMetadata.Empty = new DownloadingContentMetadata(XeusClue.Empty, string.Empty, 0, 0, MerkleTreeNode.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPathLength = 1024;

        public DownloadingContentMetadata(XeusClue clue, string path, ulong maxLength, uint downloadingDepth, MerkleTreeNode downloadingMerkleTreeNode)
        {
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (path is null) throw new global::System.ArgumentNullException("path");
            if (path.Length > 1024) throw new global::System.ArgumentOutOfRangeException("path");
            if (downloadingMerkleTreeNode is null) throw new global::System.ArgumentNullException("downloadingMerkleTreeNode");

            this.Clue = clue;
            this.Path = path;
            this.MaxLength = maxLength;
            this.DownloadingDepth = downloadingDepth;
            this.DownloadingMerkleTreeNode = downloadingMerkleTreeNode;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (clue != default) ___h.Add(clue.GetHashCode());
                if (path != default) ___h.Add(path.GetHashCode());
                if (maxLength != default) ___h.Add(maxLength.GetHashCode());
                if (downloadingDepth != default) ___h.Add(downloadingDepth.GetHashCode());
                if (downloadingMerkleTreeNode != default) ___h.Add(downloadingMerkleTreeNode.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public XeusClue Clue { get; }
        public string Path { get; }
        public ulong MaxLength { get; }
        public uint DownloadingDepth { get; }
        public MerkleTreeNode DownloadingMerkleTreeNode { get; }

        public static DownloadingContentMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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
            if (this.Path != target.Path) return false;
            if (this.MaxLength != target.MaxLength) return false;
            if (this.DownloadingDepth != target.DownloadingDepth) return false;
            if (this.DownloadingMerkleTreeNode != target.DownloadingMerkleTreeNode) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DownloadingContentMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in DownloadingContentMetadata value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Clue != XeusClue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Path != string.Empty)
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
                    if (value.DownloadingMerkleTreeNode != MerkleTreeNode.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Clue != XeusClue.Empty)
                {
                    w.Write((uint)0);
                    XeusClue.Formatter.Serialize(ref w, value.Clue, rank + 1);
                }
                if (value.Path != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Path);
                }
                if (value.MaxLength != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.MaxLength);
                }
                if (value.DownloadingDepth != 0)
                {
                    w.Write((uint)3);
                    w.Write(value.DownloadingDepth);
                }
                if (value.DownloadingMerkleTreeNode != MerkleTreeNode.Empty)
                {
                    w.Write((uint)4);
                    MerkleTreeNode.Formatter.Serialize(ref w, value.DownloadingMerkleTreeNode, rank + 1);
                }
            }

            public DownloadingContentMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                XeusClue p_clue = XeusClue.Empty;
                string p_path = string.Empty;
                ulong p_maxLength = 0;
                uint p_downloadingDepth = 0;
                MerkleTreeNode p_downloadingMerkleTreeNode = MerkleTreeNode.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_path = r.GetString(1024);
                                break;
                            }
                        case 2:
                            {
                                p_maxLength = r.GetUInt64();
                                break;
                            }
                        case 3:
                            {
                                p_downloadingDepth = r.GetUInt32();
                                break;
                            }
                        case 4:
                            {
                                p_downloadingMerkleTreeNode = MerkleTreeNode.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new DownloadingContentMetadata(p_clue, p_path, p_maxLength, p_downloadingDepth, p_downloadingMerkleTreeNode);
            }
        }
    }

}
