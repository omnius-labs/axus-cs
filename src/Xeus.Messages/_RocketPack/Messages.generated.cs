using Omnix.Base;
using Omnix.Base.Helpers;
using Omnix.Cryptography;
using Omnix.Serialization;
using Omnix.Serialization.RocketPack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace Xeus.Messages
{
    public sealed partial class Clue : RocketPackMessageBase<Clue>
    {
        static Clue()
        {
            Clue.Formatter = new CustomFormatter();
        }

        public Clue(OmniHash hash, byte depth)
        {
            this.Hash = hash;
            this.Depth = depth;

            {
                var hashCode = new HashCode();
                if (this.Hash != default) hashCode.Add(this.Hash.GetHashCode());
                if (this.Depth != default) hashCode.Add(this.Depth.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        public byte Depth { get; }

        public override bool Equals(Clue target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.Depth != target.Depth) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<Clue>
        {
            public void Serialize(RocketPackWriter w, Clue value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Hash != default) propertyCount++;
                    if (value.Depth != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Hash
                if (value.Hash != default)
                {
                    w.Write((ulong)0);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
                // Depth
                if (value.Depth != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Depth);
                }
            }

            public Clue Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                OmniHash p_hash = default;
                byte p_depth = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Hash
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // Depth
                            {
                                p_depth = (byte)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new Clue(p_hash, p_depth);
            }
        }
    }

    public sealed partial class ArchiveMetadata : RocketPackMessageBase<ArchiveMetadata>
    {
        static ArchiveMetadata()
        {
            ArchiveMetadata.Formatter = new CustomFormatter();
        }

        public static readonly int MaxNameLength = 1024;
        public static readonly int MaxTagsCount = 6;

        public ArchiveMetadata(Clue clue, string name, ulong length, Timestamp creationTime, IList<string> tags)
        {
            if (clue is null) throw new ArgumentNullException("clue");
            if (name is null) throw new ArgumentNullException("name");
            if (name.Length > 1024) throw new ArgumentOutOfRangeException("name");
            if (tags is null) throw new ArgumentNullException("tags");
            if (tags.Count > 6) throw new ArgumentOutOfRangeException("tags");
            foreach (var n in tags)
            {
                if (n is null) throw new ArgumentNullException("n");
                if (n.Length > 32) throw new ArgumentOutOfRangeException("n");
            }

            this.Clue = clue;
            this.Name = name;
            this.Length = length;
            this.CreationTime = creationTime;
            this.Tags = new ReadOnlyCollection<string>(tags);

            {
                var hashCode = new HashCode();
                if (this.Clue != default) hashCode.Add(this.Clue.GetHashCode());
                if (this.Name != default) hashCode.Add(this.Name.GetHashCode());
                if (this.Length != default) hashCode.Add(this.Length.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                foreach (var n in this.Tags)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public Clue Clue { get; }
        public string Name { get; }
        public ulong Length { get; }
        public Timestamp CreationTime { get; }
        public IReadOnlyList<string> Tags { get; }

        public override bool Equals(ArchiveMetadata target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.Name != target.Name) return false;
            if (this.Length != target.Length) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if ((this.Tags is null) != (target.Tags is null)) return false;
            if (!(this.Tags is null) && !(target.Tags is null) && !CollectionHelper.Equals(this.Tags, target.Tags)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ArchiveMetadata>
        {
            public void Serialize(RocketPackWriter w, ArchiveMetadata value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Clue != default) propertyCount++;
                    if (value.Name != default) propertyCount++;
                    if (value.Length != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Tags.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Clue
                if (value.Clue != default)
                {
                    w.Write((ulong)0);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                // Name
                if (value.Name != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.Name);
                }
                // Length
                if (value.Length != default)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.Length);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)3);
                    w.Write(value.CreationTime);
                }
                // Tags
                if (value.Tags.Count != 0)
                {
                    w.Write((ulong)4);
                    w.Write((ulong)value.Tags.Count);
                    foreach (var n in value.Tags)
                    {
                        w.Write(n);
                    }
                }
            }

            public ArchiveMetadata Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                Clue p_clue = default;
                string p_name = default;
                ulong p_length = default;
                Timestamp p_creationTime = default;
                IList<string> p_tags = default;

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
                        case 1: // Name
                            {
                                p_name = r.GetString(1024);
                                break;
                            }
                        case 2: // Length
                            {
                                p_length = (ulong)r.GetUInt64();
                                break;
                            }
                        case 3: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 4: // Tags
                            {
                                var length = (int)r.GetUInt64();
                                p_tags = new string[length];
                                for (int i = 0; i < p_tags.Count; i++)
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

    public sealed partial class ProfileContent : RocketPackMessageBase<ProfileContent>
    {
        static ProfileContent()
        {
            ProfileContent.Formatter = new CustomFormatter();
        }

        public static readonly int MaxTrustedSignaturesCount = 256;
        public static readonly int MaxInvalidSignaturesCount = 256;

        public ProfileContent(OmniAgreementPublicKey agreementPublicKey, IList<OmniSignature> trustedSignatures, IList<OmniSignature> invalidSignatures)
        {
            if (agreementPublicKey is null) throw new ArgumentNullException("agreementPublicKey");
            if (trustedSignatures is null) throw new ArgumentNullException("trustedSignatures");
            if (trustedSignatures.Count > 256) throw new ArgumentOutOfRangeException("trustedSignatures");
            foreach (var n in trustedSignatures)
            {
                if (n is null) throw new ArgumentNullException("n");
            }
            if (invalidSignatures is null) throw new ArgumentNullException("invalidSignatures");
            if (invalidSignatures.Count > 256) throw new ArgumentOutOfRangeException("invalidSignatures");
            foreach (var n in invalidSignatures)
            {
                if (n is null) throw new ArgumentNullException("n");
            }

            this.AgreementPublicKey = agreementPublicKey;
            this.TrustedSignatures = new ReadOnlyCollection<OmniSignature>(trustedSignatures);
            this.InvalidSignatures = new ReadOnlyCollection<OmniSignature>(invalidSignatures);

            {
                var hashCode = new HashCode();
                if (this.AgreementPublicKey != default) hashCode.Add(this.AgreementPublicKey.GetHashCode());
                foreach (var n in this.TrustedSignatures)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                foreach (var n in this.InvalidSignatures)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public OmniAgreementPublicKey AgreementPublicKey { get; }
        public IReadOnlyList<OmniSignature> TrustedSignatures { get; }
        public IReadOnlyList<OmniSignature> InvalidSignatures { get; }

        public override bool Equals(ProfileContent target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.AgreementPublicKey != target.AgreementPublicKey) return false;
            if ((this.TrustedSignatures is null) != (target.TrustedSignatures is null)) return false;
            if (!(this.TrustedSignatures is null) && !(target.TrustedSignatures is null) && !CollectionHelper.Equals(this.TrustedSignatures, target.TrustedSignatures)) return false;
            if ((this.InvalidSignatures is null) != (target.InvalidSignatures is null)) return false;
            if (!(this.InvalidSignatures is null) && !(target.InvalidSignatures is null) && !CollectionHelper.Equals(this.InvalidSignatures, target.InvalidSignatures)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ProfileContent>
        {
            public void Serialize(RocketPackWriter w, ProfileContent value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.AgreementPublicKey != default) propertyCount++;
                    if (value.TrustedSignatures.Count != 0) propertyCount++;
                    if (value.InvalidSignatures.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // AgreementPublicKey
                if (value.AgreementPublicKey != default)
                {
                    w.Write((ulong)0);
                    OmniAgreementPublicKey.Formatter.Serialize(w, value.AgreementPublicKey, rank + 1);
                }
                // TrustedSignatures
                if (value.TrustedSignatures.Count != 0)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.TrustedSignatures.Count);
                    foreach (var n in value.TrustedSignatures)
                    {
                        OmniSignature.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                // InvalidSignatures
                if (value.InvalidSignatures.Count != 0)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.InvalidSignatures.Count);
                    foreach (var n in value.InvalidSignatures)
                    {
                        OmniSignature.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public ProfileContent Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                OmniAgreementPublicKey p_agreementPublicKey = default;
                IList<OmniSignature> p_trustedSignatures = default;
                IList<OmniSignature> p_invalidSignatures = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // AgreementPublicKey
                            {
                                p_agreementPublicKey = OmniAgreementPublicKey.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // TrustedSignatures
                            {
                                var length = (int)r.GetUInt64();
                                p_trustedSignatures = new OmniSignature[length];
                                for (int i = 0; i < p_trustedSignatures.Count; i++)
                                {
                                    p_trustedSignatures[i] = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 2: // InvalidSignatures
                            {
                                var length = (int)r.GetUInt64();
                                p_invalidSignatures = new OmniSignature[length];
                                for (int i = 0; i < p_invalidSignatures.Count; i++)
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

    public sealed partial class StoreContent : RocketPackMessageBase<StoreContent>
    {
        static StoreContent()
        {
            StoreContent.Formatter = new CustomFormatter();
        }

        public static readonly int MaxArchiveMetadatasCount = 32768;

        public StoreContent(IList<ArchiveMetadata> archiveMetadatas)
        {
            if (archiveMetadatas is null) throw new ArgumentNullException("archiveMetadatas");
            if (archiveMetadatas.Count > 32768) throw new ArgumentOutOfRangeException("archiveMetadatas");
            foreach (var n in archiveMetadatas)
            {
                if (n is null) throw new ArgumentNullException("n");
            }

            this.ArchiveMetadatas = new ReadOnlyCollection<ArchiveMetadata>(archiveMetadatas);

            {
                var hashCode = new HashCode();
                foreach (var n in this.ArchiveMetadatas)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public IReadOnlyList<ArchiveMetadata> ArchiveMetadatas { get; }

        public override bool Equals(StoreContent target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if ((this.ArchiveMetadatas is null) != (target.ArchiveMetadatas is null)) return false;
            if (!(this.ArchiveMetadatas is null) && !(target.ArchiveMetadatas is null) && !CollectionHelper.Equals(this.ArchiveMetadatas, target.ArchiveMetadatas)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<StoreContent>
        {
            public void Serialize(RocketPackWriter w, StoreContent value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.ArchiveMetadatas.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // ArchiveMetadatas
                if (value.ArchiveMetadatas.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.ArchiveMetadatas.Count);
                    foreach (var n in value.ArchiveMetadatas)
                    {
                        ArchiveMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public StoreContent Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                IList<ArchiveMetadata> p_archiveMetadatas = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // ArchiveMetadatas
                            {
                                var length = (int)r.GetUInt64();
                                p_archiveMetadatas = new ArchiveMetadata[length];
                                for (int i = 0; i < p_archiveMetadatas.Count; i++)
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

    public sealed partial class CommentContent : RocketPackMessageBase<CommentContent>
    {
        static CommentContent()
        {
            CommentContent.Formatter = new CustomFormatter();
        }

        public static readonly int MaxCommentLength = 8192;

        public CommentContent(string comment)
        {
            if (comment is null) throw new ArgumentNullException("comment");
            if (comment.Length > 8192) throw new ArgumentOutOfRangeException("comment");

            this.Comment = comment;

            {
                var hashCode = new HashCode();
                if (this.Comment != default) hashCode.Add(this.Comment.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public string Comment { get; }

        public override bool Equals(CommentContent target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Comment != target.Comment) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<CommentContent>
        {
            public void Serialize(RocketPackWriter w, CommentContent value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Comment != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Comment
                if (value.Comment != default)
                {
                    w.Write((ulong)0);
                    w.Write(value.Comment);
                }
            }

            public CommentContent Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                string p_comment = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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

    public sealed partial class BroadcastProfileMessage : RocketPackMessageBase<BroadcastProfileMessage>
    {
        static BroadcastProfileMessage()
        {
            BroadcastProfileMessage.Formatter = new CustomFormatter();
        }

        public BroadcastProfileMessage(OmniSignature authorSignature, Timestamp creationTime, ProfileContent value)
        {
            if (authorSignature is null) throw new ArgumentNullException("authorSignature");
            if (value is null) throw new ArgumentNullException("value");

            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var hashCode = new HashCode();
                if (this.AuthorSignature != default) hashCode.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) hashCode.Add(this.Value.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public OmniSignature AuthorSignature { get; }
        public Timestamp CreationTime { get; }
        public ProfileContent Value { get; }

        public override bool Equals(BroadcastProfileMessage target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<BroadcastProfileMessage>
        {
            public void Serialize(RocketPackWriter w, BroadcastProfileMessage value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.AuthorSignature != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Value != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // AuthorSignature
                if (value.AuthorSignature != default)
                {
                    w.Write((ulong)0);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.CreationTime);
                }
                // Value
                if (value.Value != default)
                {
                    w.Write((ulong)2);
                    ProfileContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public BroadcastProfileMessage Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                OmniSignature p_authorSignature = default;
                Timestamp p_creationTime = default;
                ProfileContent p_value = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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

    public sealed partial class BroadcastStoreMessage : RocketPackMessageBase<BroadcastStoreMessage>
    {
        static BroadcastStoreMessage()
        {
            BroadcastStoreMessage.Formatter = new CustomFormatter();
        }

        public BroadcastStoreMessage(OmniSignature authorSignature, Timestamp creationTime, StoreContent value)
        {
            if (authorSignature is null) throw new ArgumentNullException("authorSignature");
            if (value is null) throw new ArgumentNullException("value");

            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var hashCode = new HashCode();
                if (this.AuthorSignature != default) hashCode.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) hashCode.Add(this.Value.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public OmniSignature AuthorSignature { get; }
        public Timestamp CreationTime { get; }
        public StoreContent Value { get; }

        public override bool Equals(BroadcastStoreMessage target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<BroadcastStoreMessage>
        {
            public void Serialize(RocketPackWriter w, BroadcastStoreMessage value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.AuthorSignature != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Value != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // AuthorSignature
                if (value.AuthorSignature != default)
                {
                    w.Write((ulong)0);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.CreationTime);
                }
                // Value
                if (value.Value != default)
                {
                    w.Write((ulong)2);
                    StoreContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public BroadcastStoreMessage Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                OmniSignature p_authorSignature = default;
                Timestamp p_creationTime = default;
                StoreContent p_value = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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

    public sealed partial class Channel : RocketPackMessageBase<Channel>
    {
        static Channel()
        {
            Channel.Formatter = new CustomFormatter();
        }

        public static readonly int MaxIdLength = 32;
        public static readonly int MaxNameLength = 256;

        public Channel(ReadOnlyMemory<byte> id, string name)
        {
            if (id.Length > 32) throw new ArgumentOutOfRangeException("id");
            if (name is null) throw new ArgumentNullException("name");
            if (name.Length > 256) throw new ArgumentOutOfRangeException("name");

            this.Id = id;
            this.Name = name;

            {
                var hashCode = new HashCode();
                if (!this.Id.IsEmpty) hashCode.Add(ObjectHelper.GetHashCode(this.Id.Span));
                if (this.Name != default) hashCode.Add(this.Name.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public ReadOnlyMemory<byte> Id { get; }
        public string Name { get; }

        public override bool Equals(Channel target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!BytesOperations.SequenceEqual(this.Id.Span, target.Id.Span)) return false;
            if (this.Name != target.Name) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<Channel>
        {
            public void Serialize(RocketPackWriter w, Channel value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (!value.Id.IsEmpty) propertyCount++;
                    if (value.Name != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Id
                if (!value.Id.IsEmpty)
                {
                    w.Write((ulong)0);
                    w.Write(value.Id.Span);
                }
                // Name
                if (value.Name != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.Name);
                }
            }

            public Channel Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                ReadOnlyMemory<byte> p_id = default;
                string p_name = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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

    public sealed partial class MulticastCommentMessage : RocketPackMessageBase<MulticastCommentMessage>
    {
        static MulticastCommentMessage()
        {
            MulticastCommentMessage.Formatter = new CustomFormatter();
        }

        public MulticastCommentMessage(Channel channel, OmniSignature authorSignature, Timestamp creationTime, uint cost, CommentContent value)
        {
            if (channel is null) throw new ArgumentNullException("channel");
            if (authorSignature is null) throw new ArgumentNullException("authorSignature");
            if (value is null) throw new ArgumentNullException("value");

            this.Channel = channel;
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Cost = cost;
            this.Value = value;

            {
                var hashCode = new HashCode();
                if (this.Channel != default) hashCode.Add(this.Channel.GetHashCode());
                if (this.AuthorSignature != default) hashCode.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.Cost != default) hashCode.Add(this.Cost.GetHashCode());
                if (this.Value != default) hashCode.Add(this.Value.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public Channel Channel { get; }
        public OmniSignature AuthorSignature { get; }
        public Timestamp CreationTime { get; }
        public uint Cost { get; }
        public CommentContent Value { get; }

        public override bool Equals(MulticastCommentMessage target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Channel != target.Channel) return false;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Cost != target.Cost) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<MulticastCommentMessage>
        {
            public void Serialize(RocketPackWriter w, MulticastCommentMessage value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Channel != default) propertyCount++;
                    if (value.AuthorSignature != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Cost != default) propertyCount++;
                    if (value.Value != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Channel
                if (value.Channel != default)
                {
                    w.Write((ulong)0);
                    Channel.Formatter.Serialize(w, value.Channel, rank + 1);
                }
                // AuthorSignature
                if (value.AuthorSignature != default)
                {
                    w.Write((ulong)1);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Cost
                if (value.Cost != default)
                {
                    w.Write((ulong)3);
                    w.Write((ulong)value.Cost);
                }
                // Value
                if (value.Value != default)
                {
                    w.Write((ulong)4);
                    CommentContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public MulticastCommentMessage Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                Channel p_channel = default;
                OmniSignature p_authorSignature = default;
                Timestamp p_creationTime = default;
                uint p_cost = default;
                CommentContent p_value = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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
                                p_cost = (uint)r.GetUInt64();
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

    public sealed partial class UnicastCommentMessage : RocketPackMessageBase<UnicastCommentMessage>
    {
        static UnicastCommentMessage()
        {
            UnicastCommentMessage.Formatter = new CustomFormatter();
        }

        public UnicastCommentMessage(OmniSignature targetSignature, OmniSignature authorSignature, Timestamp creationTime, CommentContent value)
        {
            if (targetSignature is null) throw new ArgumentNullException("targetSignature");
            if (authorSignature is null) throw new ArgumentNullException("authorSignature");
            if (value is null) throw new ArgumentNullException("value");

            this.TargetSignature = targetSignature;
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var hashCode = new HashCode();
                if (this.TargetSignature != default) hashCode.Add(this.TargetSignature.GetHashCode());
                if (this.AuthorSignature != default) hashCode.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) hashCode.Add(this.Value.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public OmniSignature TargetSignature { get; }
        public OmniSignature AuthorSignature { get; }
        public Timestamp CreationTime { get; }
        public CommentContent Value { get; }

        public override bool Equals(UnicastCommentMessage target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.TargetSignature != target.TargetSignature) return false;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<UnicastCommentMessage>
        {
            public void Serialize(RocketPackWriter w, UnicastCommentMessage value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.TargetSignature != default) propertyCount++;
                    if (value.AuthorSignature != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Value != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // TargetSignature
                if (value.TargetSignature != default)
                {
                    w.Write((ulong)0);
                    OmniSignature.Formatter.Serialize(w, value.TargetSignature, rank + 1);
                }
                // AuthorSignature
                if (value.AuthorSignature != default)
                {
                    w.Write((ulong)1);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Value
                if (value.Value != default)
                {
                    w.Write((ulong)3);
                    CommentContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public UnicastCommentMessage Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                OmniSignature p_targetSignature = default;
                OmniSignature p_authorSignature = default;
                Timestamp p_creationTime = default;
                CommentContent p_value = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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
