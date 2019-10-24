using Omnix.Cryptography;
using Omnix.Network;
using Xeus.Core;

#nullable enable

namespace Xeus.Core.Repositories.Internal
{
    internal sealed partial class MerkleTreeSection : global::Omnix.Serialization.RocketPack.IRocketPackMessage<MerkleTreeSection>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MerkleTreeSection> Formatter { get; }
        public static MerkleTreeSection Empty { get; }

        static MerkleTreeSection()
        {
            MerkleTreeSection.Formatter = new ___CustomFormatter();
            MerkleTreeSection.Empty = new MerkleTreeSection(0, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxHashesCount = 1073741824;

        public MerkleTreeSection(ulong length, OmniHash[] hashes)
        {
            if (hashes is null) throw new global::System.ArgumentNullException("hashes");
            if (hashes.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("hashes");

            this.Length = length;
            this.Hashes = new global::Omnix.Collections.ReadOnlyListSlim<OmniHash>(hashes);

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
        public global::Omnix.Collections.ReadOnlyListSlim<OmniHash> Hashes { get; }

        public static MerkleTreeSection Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(MerkleTreeSection? left, MerkleTreeSection? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(MerkleTreeSection? left, MerkleTreeSection? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is MerkleTreeSection)) return false;
            return this.Equals((MerkleTreeSection)other);
        }
        public bool Equals(MerkleTreeSection? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Length != target.Length) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Hashes, target.Hashes)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MerkleTreeSection>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in MerkleTreeSection value, in int rank)
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

            public MerkleTreeSection Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new MerkleTreeSection(p_length, p_hashes);
            }
        }
    }

    internal sealed partial class SharedFileMetadata : global::Omnix.Serialization.RocketPack.IRocketPackMessage<SharedFileMetadata>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<SharedFileMetadata> Formatter { get; }
        public static SharedFileMetadata Empty { get; }

        static SharedFileMetadata()
        {
            SharedFileMetadata.Formatter = new ___CustomFormatter();
            SharedFileMetadata.Empty = new SharedFileMetadata(0, string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPathLength = 1024;

        public SharedFileMetadata(uint blockLength, string path)
        {
            if (path is null) throw new global::System.ArgumentNullException("path");
            if (path.Length > 1024) throw new global::System.ArgumentOutOfRangeException("path");

            this.BlockLength = blockLength;
            this.Path = path;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (blockLength != default) ___h.Add(blockLength.GetHashCode());
                if (path != default) ___h.Add(path.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint BlockLength { get; }
        public string Path { get; }

        public static SharedFileMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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
            if (this.BlockLength != target.BlockLength) return false;
            if (this.Path != target.Path) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<SharedFileMetadata>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in SharedFileMetadata value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.BlockLength != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Path != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.BlockLength != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.BlockLength);
                }
                if (value.Path != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Path);
                }
            }

            public SharedFileMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_blockLength = 0;
                string p_path = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_blockLength = r.GetUInt32();
                                break;
                            }
                        case 1:
                            {
                                p_path = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new SharedFileMetadata(p_blockLength, p_path);
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
            ContentMetadata.Empty = new ContentMetadata(Clue.Empty, global::System.Array.Empty<MerkleTreeSection>(), null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxMerkleTreeNodesCount = 32;

        public ContentMetadata(Clue clue, MerkleTreeSection[] merkleTreeNodes, SharedFileMetadata? sharedFileMetadata)
        {
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (merkleTreeNodes is null) throw new global::System.ArgumentNullException("merkleTreeNodes");
            if (merkleTreeNodes.Length > 32) throw new global::System.ArgumentOutOfRangeException("merkleTreeNodes");
            foreach (var n in merkleTreeNodes)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            this.Clue = clue;
            this.MerkleTreeNodes = new global::Omnix.Collections.ReadOnlyListSlim<MerkleTreeSection>(merkleTreeNodes);
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
        public global::Omnix.Collections.ReadOnlyListSlim<MerkleTreeSection> MerkleTreeNodes { get; }
        public SharedFileMetadata? SharedFileMetadata { get; }

        public static ContentMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
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
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.MerkleTreeNodes, target.MerkleTreeNodes)) return false;
            if ((this.SharedFileMetadata is null) != (target.SharedFileMetadata is null)) return false;
            if (!(this.SharedFileMetadata is null) && !(target.SharedFileMetadata is null) && this.SharedFileMetadata != target.SharedFileMetadata) return false;

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
                        MerkleTreeSection.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.SharedFileMetadata != null)
                {
                    w.Write((uint)2);
                    SharedFileMetadata.Formatter.Serialize(ref w, value.SharedFileMetadata, rank + 1);
                }
            }

            public ContentMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                Clue p_clue = Clue.Empty;
                MerkleTreeSection[] p_merkleTreeNodes = global::System.Array.Empty<MerkleTreeSection>();
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
                                p_merkleTreeNodes = new MerkleTreeSection[length];
                                for (int i = 0; i < p_merkleTreeNodes.Length; i++)
                                {
                                    p_merkleTreeNodes[i] = MerkleTreeSection.Formatter.Deserialize(ref r, rank + 1);
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

    internal sealed partial class DownloadingContentMetadata : global::Omnix.Serialization.RocketPack.IRocketPackMessage<DownloadingContentMetadata>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DownloadingContentMetadata> Formatter { get; }
        public static DownloadingContentMetadata Empty { get; }

        static DownloadingContentMetadata()
        {
            DownloadingContentMetadata.Formatter = new ___CustomFormatter();
            DownloadingContentMetadata.Empty = new DownloadingContentMetadata(Clue.Empty, 0, 0, global::System.Array.Empty<MerkleTreeSection>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxDownloadingMerkleTreeNodesCount = 32;

        public DownloadingContentMetadata(Clue clue, ulong maxLength, uint downloadingDepth, MerkleTreeSection[] downloadingMerkleTreeNodes)
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
            this.DownloadingMerkleTreeNodes = new global::Omnix.Collections.ReadOnlyListSlim<MerkleTreeSection>(downloadingMerkleTreeNodes);

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
        public global::Omnix.Collections.ReadOnlyListSlim<MerkleTreeSection> DownloadingMerkleTreeNodes { get; }

        public static DownloadingContentMetadata Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
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
            if (this.MaxLength != target.MaxLength) return false;
            if (this.DownloadingDepth != target.DownloadingDepth) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.DownloadingMerkleTreeNodes, target.DownloadingMerkleTreeNodes)) return false;

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
                        MerkleTreeSection.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public DownloadingContentMetadata Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                Clue p_clue = Clue.Empty;
                ulong p_maxLength = 0;
                uint p_downloadingDepth = 0;
                MerkleTreeSection[] p_downloadingMerkleTreeNodes = global::System.Array.Empty<MerkleTreeSection>();

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
                                p_downloadingMerkleTreeNodes = new MerkleTreeSection[length];
                                for (int i = 0; i < p_downloadingMerkleTreeNodes.Length; i++)
                                {
                                    p_downloadingMerkleTreeNodes[i] = MerkleTreeSection.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new DownloadingContentMetadata(p_clue, p_maxLength, p_downloadingDepth, p_downloadingMerkleTreeNodes);
            }
        }
    }

    internal sealed partial class XeusRepositoryConfig : global::Omnix.Serialization.RocketPack.IRocketPackMessage<XeusRepositoryConfig>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusRepositoryConfig> Formatter { get; }
        public static XeusRepositoryConfig Empty { get; }

        static XeusRepositoryConfig()
        {
            XeusRepositoryConfig.Formatter = new ___CustomFormatter();
            XeusRepositoryConfig.Empty = new XeusRepositoryConfig(0, global::System.Array.Empty<ContentMetadata>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxContentMetadatasCount = 1073741824;

        public XeusRepositoryConfig(uint version, ContentMetadata[] contentMetadatas)
        {
            if (contentMetadatas is null) throw new global::System.ArgumentNullException("contentMetadatas");
            if (contentMetadatas.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("contentMetadatas");
            foreach (var n in contentMetadatas)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Version = version;
            this.ContentMetadatas = new global::Omnix.Collections.ReadOnlyListSlim<ContentMetadata>(contentMetadatas);

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
        public global::Omnix.Collections.ReadOnlyListSlim<ContentMetadata> ContentMetadatas { get; }

        public static XeusRepositoryConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(XeusRepositoryConfig? left, XeusRepositoryConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(XeusRepositoryConfig? left, XeusRepositoryConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is XeusRepositoryConfig)) return false;
            return this.Equals((XeusRepositoryConfig)other);
        }
        public bool Equals(XeusRepositoryConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ContentMetadatas, target.ContentMetadatas)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusRepositoryConfig>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in XeusRepositoryConfig value, in int rank)
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

            public XeusRepositoryConfig Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new XeusRepositoryConfig(p_version, p_contentMetadatas);
            }
        }
    }

}
