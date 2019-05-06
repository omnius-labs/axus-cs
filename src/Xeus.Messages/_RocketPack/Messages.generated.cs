using Omnix.Cryptography;

#nullable enable

namespace Xeus.Messages
{
    public sealed partial class XeusOptions : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusOptions>
    {
        static XeusOptions()
        {
            XeusOptions.Formatter = new CustomFormatter();
            XeusOptions.Empty = new XeusOptions(string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxConfigDirectoryPathLength = 1024;

        public XeusOptions(string configDirectoryPath)
        {
            if (configDirectoryPath is null) throw new System.ArgumentNullException("configDirectoryPath");
            if (configDirectoryPath.Length > 1024) throw new System.ArgumentOutOfRangeException("configDirectoryPath");

            this.ConfigDirectoryPath = configDirectoryPath;

            {
                var __h = new System.HashCode();
                if (this.ConfigDirectoryPath != default) __h.Add(this.ConfigDirectoryPath.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string ConfigDirectoryPath { get; }

        public override bool Equals(XeusOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigDirectoryPath != target.ConfigDirectoryPath) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusOptions>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusOptions value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigDirectoryPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigDirectoryPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigDirectoryPath);
                }
            }

            public XeusOptions Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_configDirectoryPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // ConfigDirectoryPath
                            {
                                p_configDirectoryPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new XeusOptions(p_configDirectoryPath);
            }
        }
    }

    public sealed partial class Clue : Omnix.Serialization.RocketPack.RocketPackMessageBase<Clue>
    {
        static Clue()
        {
            Clue.Formatter = new CustomFormatter();
            Clue.Empty = new Clue(OmniHash.Empty, 0);
        }

        private readonly int __hashCode;

        public Clue(OmniHash hash, byte depth)
        {
            this.Hash = hash;
            this.Depth = depth;

            {
                var __h = new System.HashCode();
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (this.Depth != default) __h.Add(this.Depth.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        public byte Depth { get; }

        public override bool Equals(Clue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.Depth != target.Depth) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<Clue>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, Clue value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Hash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Depth != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Hash != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
                if (value.Depth != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Depth);
                }
            }

            public Clue Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                byte p_depth = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Hash
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // Depth
                            {
                                p_depth = r.GetUInt8();
                                break;
                            }
                    }
                }

                return new Clue(p_hash, p_depth);
            }
        }
    }

    public sealed partial class ArchiveMetadata : Omnix.Serialization.RocketPack.RocketPackMessageBase<ArchiveMetadata>
    {
        static ArchiveMetadata()
        {
            ArchiveMetadata.Formatter = new CustomFormatter();
            ArchiveMetadata.Empty = new ArchiveMetadata(Clue.Empty, string.Empty, 0, Omnix.Serialization.RocketPack.Timestamp.Zero, System.Array.Empty<string>());
        }

        private readonly int __hashCode;

        public static readonly int MaxNameLength = 1024;
        public static readonly int MaxTagsCount = 6;

        public ArchiveMetadata(Clue clue, string name, ulong length, Omnix.Serialization.RocketPack.Timestamp creationTime, string[] tags)
        {
            if (clue is null) throw new System.ArgumentNullException("clue");
            if (name is null) throw new System.ArgumentNullException("name");
            if (name.Length > 1024) throw new System.ArgumentOutOfRangeException("name");
            if (tags is null) throw new System.ArgumentNullException("tags");
            if (tags.Length > 6) throw new System.ArgumentOutOfRangeException("tags");
            foreach (var n in tags)
            {
                if (n is null) throw new System.ArgumentNullException("n");
                if (n.Length > 32) throw new System.ArgumentOutOfRangeException("n");
            }

            this.Clue = clue;
            this.Name = name;
            this.Length = length;
            this.CreationTime = creationTime;
            this.Tags = new Omnix.Collections.ReadOnlyListSlim<string>(tags);

            {
                var __h = new System.HashCode();
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Name != default) __h.Add(this.Name.GetHashCode());
                if (this.Length != default) __h.Add(this.Length.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                foreach (var n in this.Tags)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Clue Clue { get; }
        public string Name { get; }
        public ulong Length { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public Omnix.Collections.ReadOnlyListSlim<string> Tags { get; }

        public override bool Equals(ArchiveMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.Name != target.Name) return false;
            if (this.Length != target.Length) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Tags, target.Tags)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ArchiveMetadata>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ArchiveMetadata value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Clue != Clue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Name != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Length != 0)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Tags.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Clue != Clue.Empty)
                {
                    w.Write((uint)0);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Name != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Name);
                }
                if (value.Length != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.Length);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)3);
                    w.Write(value.CreationTime);
                }
                if (value.Tags.Count != 0)
                {
                    w.Write((uint)4);
                    w.Write((uint)value.Tags.Count);
                    foreach (var n in value.Tags)
                    {
                        w.Write(n);
                    }
                }
            }

            public ArchiveMetadata Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                Clue p_clue = Clue.Empty;
                string p_name = string.Empty;
                ulong p_length = 0;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                string[] p_tags = System.Array.Empty<string>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Clue
                            {
                                p_clue = Clue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // Name
                            {
                                p_name = r.GetString(1024);
                                break;
                            }
                        case 2: // Length
                            {
                                p_length = r.GetUInt64();
                                break;
                            }
                        case 3: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 4: // Tags
                            {
                                var length = r.GetUInt32();
                                p_tags = new string[length];
                                for (int i = 0; i < p_tags.Length; i++)
                                {
                                    p_tags[i] = r.GetString(32);
                                }
                                break;
                            }
                    }
                }

                return new ArchiveMetadata(p_clue, p_name, p_length, p_creationTime, p_tags);
            }
        }
    }

    public sealed partial class ProfileContent : Omnix.Serialization.RocketPack.RocketPackMessageBase<ProfileContent>
    {
        static ProfileContent()
        {
            ProfileContent.Formatter = new CustomFormatter();
            ProfileContent.Empty = new ProfileContent(OmniAgreementPublicKey.Empty, System.Array.Empty<OmniSignature>(), System.Array.Empty<OmniSignature>());
        }

        private readonly int __hashCode;

        public static readonly int MaxTrustedSignaturesCount = 256;
        public static readonly int MaxInvalidSignaturesCount = 256;

        public ProfileContent(OmniAgreementPublicKey agreementPublicKey, OmniSignature[] trustedSignatures, OmniSignature[] invalidSignatures)
        {
            if (agreementPublicKey is null) throw new System.ArgumentNullException("agreementPublicKey");
            if (trustedSignatures is null) throw new System.ArgumentNullException("trustedSignatures");
            if (trustedSignatures.Length > 256) throw new System.ArgumentOutOfRangeException("trustedSignatures");
            foreach (var n in trustedSignatures)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }
            if (invalidSignatures is null) throw new System.ArgumentNullException("invalidSignatures");
            if (invalidSignatures.Length > 256) throw new System.ArgumentOutOfRangeException("invalidSignatures");
            foreach (var n in invalidSignatures)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.AgreementPublicKey = agreementPublicKey;
            this.TrustedSignatures = new Omnix.Collections.ReadOnlyListSlim<OmniSignature>(trustedSignatures);
            this.InvalidSignatures = new Omnix.Collections.ReadOnlyListSlim<OmniSignature>(invalidSignatures);

            {
                var __h = new System.HashCode();
                if (this.AgreementPublicKey != default) __h.Add(this.AgreementPublicKey.GetHashCode());
                foreach (var n in this.TrustedSignatures)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                foreach (var n in this.InvalidSignatures)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniAgreementPublicKey AgreementPublicKey { get; }
        public Omnix.Collections.ReadOnlyListSlim<OmniSignature> TrustedSignatures { get; }
        public Omnix.Collections.ReadOnlyListSlim<OmniSignature> InvalidSignatures { get; }

        public override bool Equals(ProfileContent? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.AgreementPublicKey != target.AgreementPublicKey) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.TrustedSignatures, target.TrustedSignatures)) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.InvalidSignatures, target.InvalidSignatures)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ProfileContent>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ProfileContent value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.AgreementPublicKey != OmniAgreementPublicKey.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.TrustedSignatures.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.InvalidSignatures.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.AgreementPublicKey != OmniAgreementPublicKey.Empty)
                {
                    w.Write((uint)0);
                    OmniAgreementPublicKey.Formatter.Serialize(w, value.AgreementPublicKey, rank + 1);
                }
                if (value.TrustedSignatures.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.TrustedSignatures.Count);
                    foreach (var n in value.TrustedSignatures)
                    {
                        OmniSignature.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                if (value.InvalidSignatures.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.InvalidSignatures.Count);
                    foreach (var n in value.InvalidSignatures)
                    {
                        OmniSignature.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public ProfileContent Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniAgreementPublicKey p_agreementPublicKey = OmniAgreementPublicKey.Empty;
                OmniSignature[] p_trustedSignatures = System.Array.Empty<OmniSignature>();
                OmniSignature[] p_invalidSignatures = System.Array.Empty<OmniSignature>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // AgreementPublicKey
                            {
                                p_agreementPublicKey = OmniAgreementPublicKey.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // TrustedSignatures
                            {
                                var length = r.GetUInt32();
                                p_trustedSignatures = new OmniSignature[length];
                                for (int i = 0; i < p_trustedSignatures.Length; i++)
                                {
                                    p_trustedSignatures[i] = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 2: // InvalidSignatures
                            {
                                var length = r.GetUInt32();
                                p_invalidSignatures = new OmniSignature[length];
                                for (int i = 0; i < p_invalidSignatures.Length; i++)
                                {
                                    p_invalidSignatures[i] = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new ProfileContent(p_agreementPublicKey, p_trustedSignatures, p_invalidSignatures);
            }
        }
    }

    public sealed partial class StoreContent : Omnix.Serialization.RocketPack.RocketPackMessageBase<StoreContent>
    {
        static StoreContent()
        {
            StoreContent.Formatter = new CustomFormatter();
            StoreContent.Empty = new StoreContent(System.Array.Empty<ArchiveMetadata>());
        }

        private readonly int __hashCode;

        public static readonly int MaxArchiveMetadatasCount = 32768;

        public StoreContent(ArchiveMetadata[] archiveMetadatas)
        {
            if (archiveMetadatas is null) throw new System.ArgumentNullException("archiveMetadatas");
            if (archiveMetadatas.Length > 32768) throw new System.ArgumentOutOfRangeException("archiveMetadatas");
            foreach (var n in archiveMetadatas)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.ArchiveMetadatas = new Omnix.Collections.ReadOnlyListSlim<ArchiveMetadata>(archiveMetadatas);

            {
                var __h = new System.HashCode();
                foreach (var n in this.ArchiveMetadatas)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyListSlim<ArchiveMetadata> ArchiveMetadatas { get; }

        public override bool Equals(StoreContent? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.ArchiveMetadatas, target.ArchiveMetadatas)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<StoreContent>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, StoreContent value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ArchiveMetadatas.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ArchiveMetadatas.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.ArchiveMetadatas.Count);
                    foreach (var n in value.ArchiveMetadatas)
                    {
                        ArchiveMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public StoreContent Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                ArchiveMetadata[] p_archiveMetadatas = System.Array.Empty<ArchiveMetadata>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // ArchiveMetadatas
                            {
                                var length = r.GetUInt32();
                                p_archiveMetadatas = new ArchiveMetadata[length];
                                for (int i = 0; i < p_archiveMetadatas.Length; i++)
                                {
                                    p_archiveMetadatas[i] = ArchiveMetadata.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new StoreContent(p_archiveMetadatas);
            }
        }
    }

    public sealed partial class CommentContent : Omnix.Serialization.RocketPack.RocketPackMessageBase<CommentContent>
    {
        static CommentContent()
        {
            CommentContent.Formatter = new CustomFormatter();
            CommentContent.Empty = new CommentContent(string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxCommentLength = 8192;

        public CommentContent(string comment)
        {
            if (comment is null) throw new System.ArgumentNullException("comment");
            if (comment.Length > 8192) throw new System.ArgumentOutOfRangeException("comment");

            this.Comment = comment;

            {
                var __h = new System.HashCode();
                if (this.Comment != default) __h.Add(this.Comment.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string Comment { get; }

        public override bool Equals(CommentContent? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Comment != target.Comment) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<CommentContent>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, CommentContent value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Comment != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Comment != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Comment);
                }
            }

            public CommentContent Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_comment = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Comment
                            {
                                p_comment = r.GetString(8192);
                                break;
                            }
                    }
                }

                return new CommentContent(p_comment);
            }
        }
    }

    public sealed partial class BroadcastProfileMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<BroadcastProfileMessage>
    {
        static BroadcastProfileMessage()
        {
            BroadcastProfileMessage.Formatter = new CustomFormatter();
            BroadcastProfileMessage.Empty = new BroadcastProfileMessage(OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, ProfileContent.Empty);
        }

        private readonly int __hashCode;

        public BroadcastProfileMessage(OmniSignature authorSignature, Omnix.Serialization.RocketPack.Timestamp creationTime, ProfileContent value)
        {
            if (authorSignature is null) throw new System.ArgumentNullException("authorSignature");
            if (value is null) throw new System.ArgumentNullException("value");

            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var __h = new System.HashCode();
                if (this.AuthorSignature != default) __h.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) __h.Add(this.Value.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniSignature AuthorSignature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public ProfileContent Value { get; }

        public override bool Equals(BroadcastProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastProfileMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, BroadcastProfileMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.AuthorSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Value != ProfileContent.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.AuthorSignature != OmniSignature.Empty)
                {
                    w.Write((uint)0);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)1);
                    w.Write(value.CreationTime);
                }
                if (value.Value != ProfileContent.Empty)
                {
                    w.Write((uint)2);
                    ProfileContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public BroadcastProfileMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniSignature p_authorSignature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                ProfileContent p_value = ProfileContent.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // AuthorSignature
                            {
                                p_authorSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 2: // Value
                            {
                                p_value = ProfileContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new BroadcastProfileMessage(p_authorSignature, p_creationTime, p_value);
            }
        }
    }

    public sealed partial class BroadcastStoreMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<BroadcastStoreMessage>
    {
        static BroadcastStoreMessage()
        {
            BroadcastStoreMessage.Formatter = new CustomFormatter();
            BroadcastStoreMessage.Empty = new BroadcastStoreMessage(OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, StoreContent.Empty);
        }

        private readonly int __hashCode;

        public BroadcastStoreMessage(OmniSignature authorSignature, Omnix.Serialization.RocketPack.Timestamp creationTime, StoreContent value)
        {
            if (authorSignature is null) throw new System.ArgumentNullException("authorSignature");
            if (value is null) throw new System.ArgumentNullException("value");

            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var __h = new System.HashCode();
                if (this.AuthorSignature != default) __h.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) __h.Add(this.Value.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniSignature AuthorSignature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public StoreContent Value { get; }

        public override bool Equals(BroadcastStoreMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastStoreMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, BroadcastStoreMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.AuthorSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Value != StoreContent.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.AuthorSignature != OmniSignature.Empty)
                {
                    w.Write((uint)0);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)1);
                    w.Write(value.CreationTime);
                }
                if (value.Value != StoreContent.Empty)
                {
                    w.Write((uint)2);
                    StoreContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public BroadcastStoreMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniSignature p_authorSignature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                StoreContent p_value = StoreContent.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // AuthorSignature
                            {
                                p_authorSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 2: // Value
                            {
                                p_value = StoreContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new BroadcastStoreMessage(p_authorSignature, p_creationTime, p_value);
            }
        }
    }

    public sealed partial class Channel : Omnix.Serialization.RocketPack.RocketPackMessageBase<Channel>
    {
        static Channel()
        {
            Channel.Formatter = new CustomFormatter();
            Channel.Empty = new Channel(System.ReadOnlyMemory<byte>.Empty, string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxIdLength = 32;
        public static readonly int MaxNameLength = 256;

        public Channel(System.ReadOnlyMemory<byte> id, string name)
        {
            if (id.Length > 32) throw new System.ArgumentOutOfRangeException("id");
            if (name is null) throw new System.ArgumentNullException("name");
            if (name.Length > 256) throw new System.ArgumentOutOfRangeException("name");

            this.Id = id;
            this.Name = name;

            {
                var __h = new System.HashCode();
                if (!this.Id.IsEmpty) __h.Add(Omnix.Base.Helpers.ObjectHelper.GetHashCode(this.Id.Span));
                if (this.Name != default) __h.Add(this.Name.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public System.ReadOnlyMemory<byte> Id { get; }
        public string Name { get; }

        public override bool Equals(Channel? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.BytesOperations.SequenceEqual(this.Id.Span, target.Id.Span)) return false;
            if (this.Name != target.Name) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<Channel>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, Channel value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (!value.Id.IsEmpty)
                    {
                        propertyCount++;
                    }
                    if (value.Name != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (!value.Id.IsEmpty)
                {
                    w.Write((uint)0);
                    w.Write(value.Id.Span);
                }
                if (value.Name != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Name);
                }
            }

            public Channel Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                System.ReadOnlyMemory<byte> p_id = System.ReadOnlyMemory<byte>.Empty;
                string p_name = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Id
                            {
                                p_id = r.GetMemory(32);
                                break;
                            }
                        case 1: // Name
                            {
                                p_name = r.GetString(256);
                                break;
                            }
                    }
                }

                return new Channel(p_id, p_name);
            }
        }
    }

    public sealed partial class MulticastCommentMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<MulticastCommentMessage>
    {
        static MulticastCommentMessage()
        {
            MulticastCommentMessage.Formatter = new CustomFormatter();
            MulticastCommentMessage.Empty = new MulticastCommentMessage(Channel.Empty, OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, 0, CommentContent.Empty);
        }

        private readonly int __hashCode;

        public MulticastCommentMessage(Channel channel, OmniSignature authorSignature, Omnix.Serialization.RocketPack.Timestamp creationTime, uint cost, CommentContent value)
        {
            if (channel is null) throw new System.ArgumentNullException("channel");
            if (authorSignature is null) throw new System.ArgumentNullException("authorSignature");
            if (value is null) throw new System.ArgumentNullException("value");

            this.Channel = channel;
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Cost = cost;
            this.Value = value;

            {
                var __h = new System.HashCode();
                if (this.Channel != default) __h.Add(this.Channel.GetHashCode());
                if (this.AuthorSignature != default) __h.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Cost != default) __h.Add(this.Cost.GetHashCode());
                if (this.Value != default) __h.Add(this.Value.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public Channel Channel { get; }
        public OmniSignature AuthorSignature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public uint Cost { get; }
        public CommentContent Value { get; }

        public override bool Equals(MulticastCommentMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Channel != target.Channel) return false;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Cost != target.Cost) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastCommentMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, MulticastCommentMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Channel != Channel.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.AuthorSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Cost != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Value != CommentContent.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Channel != Channel.Empty)
                {
                    w.Write((uint)0);
                    Channel.Formatter.Serialize(w, value.Channel, rank + 1);
                }
                if (value.AuthorSignature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Cost != 0)
                {
                    w.Write((uint)3);
                    w.Write(value.Cost);
                }
                if (value.Value != CommentContent.Empty)
                {
                    w.Write((uint)4);
                    CommentContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public MulticastCommentMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                Channel p_channel = Channel.Empty;
                OmniSignature p_authorSignature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                uint p_cost = 0;
                CommentContent p_value = CommentContent.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Channel
                            {
                                p_channel = Channel.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // AuthorSignature
                            {
                                p_authorSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3: // Cost
                            {
                                p_cost = r.GetUInt32();
                                break;
                            }
                        case 4: // Value
                            {
                                p_value = CommentContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new MulticastCommentMessage(p_channel, p_authorSignature, p_creationTime, p_cost, p_value);
            }
        }
    }

    public sealed partial class UnicastCommentMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<UnicastCommentMessage>
    {
        static UnicastCommentMessage()
        {
            UnicastCommentMessage.Formatter = new CustomFormatter();
            UnicastCommentMessage.Empty = new UnicastCommentMessage(OmniSignature.Empty, OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, CommentContent.Empty);
        }

        private readonly int __hashCode;

        public UnicastCommentMessage(OmniSignature targetSignature, OmniSignature authorSignature, Omnix.Serialization.RocketPack.Timestamp creationTime, CommentContent value)
        {
            if (targetSignature is null) throw new System.ArgumentNullException("targetSignature");
            if (authorSignature is null) throw new System.ArgumentNullException("authorSignature");
            if (value is null) throw new System.ArgumentNullException("value");

            this.TargetSignature = targetSignature;
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var __h = new System.HashCode();
                if (this.TargetSignature != default) __h.Add(this.TargetSignature.GetHashCode());
                if (this.AuthorSignature != default) __h.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) __h.Add(this.Value.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniSignature TargetSignature { get; }
        public OmniSignature AuthorSignature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public CommentContent Value { get; }

        public override bool Equals(UnicastCommentMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.TargetSignature != target.TargetSignature) return false;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastCommentMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, UnicastCommentMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.TargetSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.AuthorSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Value != CommentContent.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.TargetSignature != OmniSignature.Empty)
                {
                    w.Write((uint)0);
                    OmniSignature.Formatter.Serialize(w, value.TargetSignature, rank + 1);
                }
                if (value.AuthorSignature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Value != CommentContent.Empty)
                {
                    w.Write((uint)3);
                    CommentContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public UnicastCommentMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniSignature p_targetSignature = OmniSignature.Empty;
                OmniSignature p_authorSignature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                CommentContent p_value = CommentContent.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // TargetSignature
                            {
                                p_targetSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // AuthorSignature
                            {
                                p_authorSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3: // Value
                            {
                                p_value = CommentContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new UnicastCommentMessage(p_targetSignature, p_authorSignature, p_creationTime, p_value);
            }
        }
    }

}
