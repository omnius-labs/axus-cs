using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Runtime.InteropServices;
using Newtonsoft.Json;
using Omnius.Base;
using Omnius.Net;
using Omnius.Net.Secure;
using Omnius.Security;
using Omnius.Serialization;
using Omnius.Utils;

namespace Amoeba.Messages
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class Location : MessageBase<Location>
    {
        static Location()
        {
            Location.Formatter = new CustomFormatter();
        }
        public static readonly int MaxUrisCount = 32;
        [JsonConstructor]
        public Location(IList<string> uris)
        {
            if (uris == null) throw new ArgumentNullException("uris");
            if (uris.Count > MaxUrisCount) throw new ArgumentOutOfRangeException("uris");
            for (int i = 0; i < uris.Count; i++)
            {
                if (uris[i] == null) throw new ArgumentNullException("uris[i]");
                if (uris[i].Length > 256) throw new ArgumentOutOfRangeException("uris[i]");
            }
            this.Uris = new ReadOnlyCollection<string>(uris);
            {
                int h = 0;
                for (int i = 0; i < Uris.Count; i++)
                {
                    h ^= this.Uris[i].GetHashCode();
                }
                _hashCode = h;
            }
        }
        [JsonProperty]
        public IReadOnlyList<string> Uris { get; }
        public override bool Equals(Location target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!CollectionUtils.Equals(this.Uris, target.Uris)) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<Location>
        {
            private int GetPropertyCount(Location value)
            {
                int c = 0;
                if (value.Uris.Count != 0) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, Location value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Uris
                if (value.Uris.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Uris.Count);
                    for (int i = 0; i < value.Uris.Count; i++)
                    {
                        w.Write(value.Uris[i]);
                    }
                }
            }
            public Location Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                string[] p_uris = Array.Empty<string>();
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Uris
                            {
                                var length = (int)r.GetUInt64();
                                Array.Resize(ref p_uris, Math.Min(length, MaxUrisCount));
                                for (int index = 0; index < p_uris.Length; index++)
                                {
                                    p_uris[index] = r.GetString(256);
                                }
                                break;
                            }
                    }
                }
                return new Location(p_uris);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class Metadata : MessageBase<Metadata>
    {
        static Metadata()
        {
            Metadata.Formatter = new CustomFormatter();
        }
        [JsonConstructor]
        public Metadata(int depth, Hash hash)
        {
            if (hash == null) throw new ArgumentNullException("hash");
            this.Depth = depth;
            this.Hash = hash;
            {
                int h = 0;
                if (this.Depth != default(int)) h ^= this.Depth.GetHashCode();
                if (this.Hash != default(Hash)) h ^= this.Hash.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public int Depth { get; }
        [JsonProperty]
        public Hash Hash { get; }
        public override bool Equals(Metadata target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Depth != target.Depth) return false;
            if (this.Hash != target.Hash) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<Metadata>
        {
            private int GetPropertyCount(Metadata value)
            {
                int c = 0;
                if (value.Depth != default(int)) c++;
                if (value.Hash != default(Hash)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, Metadata value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Depth
                if (value.Depth != default(int))
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Depth);
                }
                // Hash
                if (value.Hash != default(Hash))
                {
                    w.Write((ulong)1);
                    Hash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
            }
            public Metadata Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                int p_depth = default(int);
                Hash p_hash = default(Hash);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Depth
                            {
                                p_depth = (int)r.GetUInt64();
                                break;
                            }
                        case 1: // Hash
                            {
                                p_hash = Hash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }
                return new Metadata(p_depth, p_hash);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class Tag : MessageBase<Tag>
    {
        static Tag()
        {
            Tag.Formatter = new CustomFormatter();
        }
        public static readonly int MaxNameLength = 256;
        public static readonly int MaxIdLength = 32;
        [JsonConstructor]
        public Tag(string name, byte[] id)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (id == null) throw new ArgumentNullException("id");
            if (name.Length > MaxNameLength) throw new ArgumentOutOfRangeException("name");
            if (id.Length > MaxIdLength) throw new ArgumentOutOfRangeException("id");
            this.Name = name;
            this.Id = id;
            {
                int h = 0;
                if (this.Name != default(string)) h ^= this.Name.GetHashCode();
                if (this.Id != default(byte[])) h ^= ItemUtils.GetHashCode(this.Id);
                _hashCode = h;
            }
        }
        [JsonProperty]
        public string Name { get; }
        [JsonProperty]
        public byte[] Id { get; }
        public override bool Equals(Tag target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Name != target.Name) return false;
            if ((this.Id == null) != (target.Id == null)) return false;
            if ((this.Id != null && target.Id != null)
                && !Unsafe.Equals(this.Id, target.Id)) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<Tag>
        {
            private int GetPropertyCount(Tag value)
            {
                int c = 0;
                if (value.Name != default(string)) c++;
                if (value.Id != default(byte[])) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, Tag value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Name
                if (value.Name != default(string))
                {
                    w.Write((ulong)0);
                    w.Write(value.Name);
                }
                // Id
                if (value.Id != default(byte[]))
                {
                    w.Write((ulong)1);
                    w.Write(value.Id);
                }
            }
            public Tag Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                string p_name = default(string);
                byte[] p_id = default(byte[]);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Name
                            {
                                p_name = r.GetString(MaxNameLength);
                                break;
                            }
                        case 1: // Id
                            {
                                p_id = r.GetBytes(MaxIdLength);
                                break;
                            }
                    }
                }
                return new Tag(p_name, p_id);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class ProfileContent : MessageBase<ProfileContent>
    {
        static ProfileContent()
        {
            ProfileContent.Formatter = new CustomFormatter();
        }
        public static readonly int MaxTrustSignaturesCount = 1024;
        public static readonly int MaxUntrustSignaturesCount = 1024;
        public static readonly int MaxSubscriptionTagsCount = 1024;
        public static readonly int MaxCommentLength = 8192;
        [JsonConstructor]
        public ProfileContent(AgreementPublicKey agreementPublicKey, IList<Signature> trustSignatures, IList<Signature> untrustSignatures, IList<Tag> subscriptionTags, string comment)
        {
            if (agreementPublicKey == null) throw new ArgumentNullException("agreementPublicKey");
            if (trustSignatures == null) throw new ArgumentNullException("trustSignatures");
            if (untrustSignatures == null) throw new ArgumentNullException("untrustSignatures");
            if (subscriptionTags == null) throw new ArgumentNullException("subscriptionTags");
            if (trustSignatures.Count > MaxTrustSignaturesCount) throw new ArgumentOutOfRangeException("trustSignatures");
            if (untrustSignatures.Count > MaxUntrustSignaturesCount) throw new ArgumentOutOfRangeException("untrustSignatures");
            if (subscriptionTags.Count > MaxSubscriptionTagsCount) throw new ArgumentOutOfRangeException("subscriptionTags");
            if (comment != null && comment.Length > MaxCommentLength) throw new ArgumentOutOfRangeException("comment");
            for (int i = 0; i < trustSignatures.Count; i++)
            {
                if (trustSignatures[i] == null) throw new ArgumentNullException("trustSignatures[i]");
            }
            for (int i = 0; i < untrustSignatures.Count; i++)
            {
                if (untrustSignatures[i] == null) throw new ArgumentNullException("untrustSignatures[i]");
            }
            for (int i = 0; i < subscriptionTags.Count; i++)
            {
                if (subscriptionTags[i] == null) throw new ArgumentNullException("subscriptionTags[i]");
            }
            this.AgreementPublicKey = agreementPublicKey;
            this.TrustSignatures = new ReadOnlyCollection<Signature>(trustSignatures);
            this.UntrustSignatures = new ReadOnlyCollection<Signature>(untrustSignatures);
            this.SubscriptionTags = new ReadOnlyCollection<Tag>(subscriptionTags);
            this.Comment = comment;
            {
                int h = 0;
                if (this.AgreementPublicKey != default(AgreementPublicKey)) h ^= this.AgreementPublicKey.GetHashCode();
                for (int i = 0; i < TrustSignatures.Count; i++)
                {
                    h ^= this.TrustSignatures[i].GetHashCode();
                }
                for (int i = 0; i < UntrustSignatures.Count; i++)
                {
                    h ^= this.UntrustSignatures[i].GetHashCode();
                }
                for (int i = 0; i < SubscriptionTags.Count; i++)
                {
                    h ^= this.SubscriptionTags[i].GetHashCode();
                }
                if (this.Comment != default(string)) h ^= this.Comment.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public AgreementPublicKey AgreementPublicKey { get; }
        [JsonProperty]
        public IReadOnlyList<Signature> TrustSignatures { get; }
        [JsonProperty]
        public IReadOnlyList<Signature> UntrustSignatures { get; }
        [JsonProperty]
        public IReadOnlyList<Tag> SubscriptionTags { get; }
        [JsonProperty]
        public string Comment { get; }
        public override bool Equals(ProfileContent target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.AgreementPublicKey != target.AgreementPublicKey) return false;
            if (!CollectionUtils.Equals(this.TrustSignatures, target.TrustSignatures)) return false;
            if (!CollectionUtils.Equals(this.UntrustSignatures, target.UntrustSignatures)) return false;
            if (!CollectionUtils.Equals(this.SubscriptionTags, target.SubscriptionTags)) return false;
            if (this.Comment != target.Comment) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<ProfileContent>
        {
            private int GetPropertyCount(ProfileContent value)
            {
                int c = 0;
                if (value.AgreementPublicKey != default(AgreementPublicKey)) c++;
                if (value.TrustSignatures.Count != 0) c++;
                if (value.UntrustSignatures.Count != 0) c++;
                if (value.SubscriptionTags.Count != 0) c++;
                if (value.Comment != default(string)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, ProfileContent value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // AgreementPublicKey
                if (value.AgreementPublicKey != default(AgreementPublicKey))
                {
                    w.Write((ulong)0);
                    AgreementPublicKey.Formatter.Serialize(w, value.AgreementPublicKey, rank + 1);
                }
                // TrustSignatures
                if (value.TrustSignatures.Count != 0)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.TrustSignatures.Count);
                    for (int i = 0; i < value.TrustSignatures.Count; i++)
                    {
                        Signature.Formatter.Serialize(w, value.TrustSignatures[i], rank + 1);
                    }
                }
                // UntrustSignatures
                if (value.UntrustSignatures.Count != 0)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.UntrustSignatures.Count);
                    for (int i = 0; i < value.UntrustSignatures.Count; i++)
                    {
                        Signature.Formatter.Serialize(w, value.UntrustSignatures[i], rank + 1);
                    }
                }
                // SubscriptionTags
                if (value.SubscriptionTags.Count != 0)
                {
                    w.Write((ulong)3);
                    w.Write((ulong)value.SubscriptionTags.Count);
                    for (int i = 0; i < value.SubscriptionTags.Count; i++)
                    {
                        Tag.Formatter.Serialize(w, value.SubscriptionTags[i], rank + 1);
                    }
                }
                // Comment
                if (value.Comment != default(string))
                {
                    w.Write((ulong)4);
                    w.Write(value.Comment);
                }
            }
            public ProfileContent Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                AgreementPublicKey p_agreementPublicKey = default(AgreementPublicKey);
                Signature[] p_trustSignatures = Array.Empty<Signature>();
                Signature[] p_untrustSignatures = Array.Empty<Signature>();
                Tag[] p_subscriptionTags = Array.Empty<Tag>();
                string p_comment = default(string);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // AgreementPublicKey
                            {
                                p_agreementPublicKey = AgreementPublicKey.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // TrustSignatures
                            {
                                var length = (int)r.GetUInt64();
                                Array.Resize(ref p_trustSignatures, Math.Min(length, MaxTrustSignaturesCount));
                                for (int index = 0; index < p_trustSignatures.Length; index++)
                                {
                                    p_trustSignatures[index] = Signature.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 2: // UntrustSignatures
                            {
                                var length = (int)r.GetUInt64();
                                Array.Resize(ref p_untrustSignatures, Math.Min(length, MaxUntrustSignaturesCount));
                                for (int index = 0; index < p_untrustSignatures.Length; index++)
                                {
                                    p_untrustSignatures[index] = Signature.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 3: // SubscriptionTags
                            {
                                var length = (int)r.GetUInt64();
                                Array.Resize(ref p_subscriptionTags, Math.Min(length, MaxSubscriptionTagsCount));
                                for (int index = 0; index < p_subscriptionTags.Length; index++)
                                {
                                    p_subscriptionTags[index] = Tag.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 4: // Comment
                            {
                                p_comment = r.GetString(MaxCommentLength);
                                break;
                            }
                    }
                }
                return new ProfileContent(p_agreementPublicKey, p_trustSignatures, p_untrustSignatures, p_subscriptionTags, p_comment);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class StoreContent : MessageBase<StoreContent>
    {
        static StoreContent()
        {
            StoreContent.Formatter = new CustomFormatter();
        }
        public static readonly int MaxBoxesCount = 1024 * 8;
        [JsonConstructor]
        public StoreContent(IList<Box> boxes)
        {
            if (boxes == null) throw new ArgumentNullException("boxes");
            if (boxes.Count > MaxBoxesCount) throw new ArgumentOutOfRangeException("boxes");
            for (int i = 0; i < boxes.Count; i++)
            {
                if (boxes[i] == null) throw new ArgumentNullException("boxes[i]");
            }
            this.Boxes = new ReadOnlyCollection<Box>(boxes);
            {
                int h = 0;
                for (int i = 0; i < Boxes.Count; i++)
                {
                    h ^= this.Boxes[i].GetHashCode();
                }
                _hashCode = h;
            }
        }
        [JsonProperty]
        public IReadOnlyList<Box> Boxes { get; }
        public override bool Equals(StoreContent target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!CollectionUtils.Equals(this.Boxes, target.Boxes)) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<StoreContent>
        {
            private int GetPropertyCount(StoreContent value)
            {
                int c = 0;
                if (value.Boxes.Count != 0) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, StoreContent value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Boxes
                if (value.Boxes.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Boxes.Count);
                    for (int i = 0; i < value.Boxes.Count; i++)
                    {
                        Box.Formatter.Serialize(w, value.Boxes[i], rank + 1);
                    }
                }
            }
            public StoreContent Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                Box[] p_boxes = Array.Empty<Box>();
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Boxes
                            {
                                var length = (int)r.GetUInt64();
                                Array.Resize(ref p_boxes, Math.Min(length, MaxBoxesCount));
                                for (int index = 0; index < p_boxes.Length; index++)
                                {
                                    p_boxes[index] = Box.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }
                return new StoreContent(p_boxes);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class Box : MessageBase<Box>
    {
        static Box()
        {
            Box.Formatter = new CustomFormatter();
        }
        public static readonly int MaxNameLength = 256;
        public static readonly int MaxSeedsCount = 1024 * 64;
        public static readonly int MaxBoxesCount = 1024 * 8;
        [JsonConstructor]
        public Box(string name, IList<Seed> seeds, IList<Box> boxes)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (seeds == null) throw new ArgumentNullException("seeds");
            if (boxes == null) throw new ArgumentNullException("boxes");
            if (name.Length > MaxNameLength) throw new ArgumentOutOfRangeException("name");
            if (seeds.Count > MaxSeedsCount) throw new ArgumentOutOfRangeException("seeds");
            if (boxes.Count > MaxBoxesCount) throw new ArgumentOutOfRangeException("boxes");
            for (int i = 0; i < seeds.Count; i++)
            {
                if (seeds[i] == null) throw new ArgumentNullException("seeds[i]");
            }
            for (int i = 0; i < boxes.Count; i++)
            {
                if (boxes[i] == null) throw new ArgumentNullException("boxes[i]");
            }
            this.Name = name;
            this.Seeds = new ReadOnlyCollection<Seed>(seeds);
            this.Boxes = new ReadOnlyCollection<Box>(boxes);
            {
                int h = 0;
                if (this.Name != default(string)) h ^= this.Name.GetHashCode();
                for (int i = 0; i < Seeds.Count; i++)
                {
                    h ^= this.Seeds[i].GetHashCode();
                }
                for (int i = 0; i < Boxes.Count; i++)
                {
                    h ^= this.Boxes[i].GetHashCode();
                }
                _hashCode = h;
            }
        }
        [JsonProperty]
        public string Name { get; }
        [JsonProperty]
        public IReadOnlyList<Seed> Seeds { get; }
        [JsonProperty]
        public IReadOnlyList<Box> Boxes { get; }
        public override bool Equals(Box target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Name != target.Name) return false;
            if (!CollectionUtils.Equals(this.Seeds, target.Seeds)) return false;
            if (!CollectionUtils.Equals(this.Boxes, target.Boxes)) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<Box>
        {
            private int GetPropertyCount(Box value)
            {
                int c = 0;
                if (value.Name != default(string)) c++;
                if (value.Seeds.Count != 0) c++;
                if (value.Boxes.Count != 0) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, Box value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Name
                if (value.Name != default(string))
                {
                    w.Write((ulong)0);
                    w.Write(value.Name);
                }
                // Seeds
                if (value.Seeds.Count != 0)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Seeds.Count);
                    for (int i = 0; i < value.Seeds.Count; i++)
                    {
                        Seed.Formatter.Serialize(w, value.Seeds[i], rank + 1);
                    }
                }
                // Boxes
                if (value.Boxes.Count != 0)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.Boxes.Count);
                    for (int i = 0; i < value.Boxes.Count; i++)
                    {
                        Box.Formatter.Serialize(w, value.Boxes[i], rank + 1);
                    }
                }
            }
            public Box Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                string p_name = default(string);
                Seed[] p_seeds = Array.Empty<Seed>();
                Box[] p_boxes = Array.Empty<Box>();
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Name
                            {
                                p_name = r.GetString(MaxNameLength);
                                break;
                            }
                        case 1: // Seeds
                            {
                                var length = (int)r.GetUInt64();
                                Array.Resize(ref p_seeds, Math.Min(length, MaxSeedsCount));
                                for (int index = 0; index < p_seeds.Length; index++)
                                {
                                    p_seeds[index] = Seed.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 2: // Boxes
                            {
                                var length = (int)r.GetUInt64();
                                Array.Resize(ref p_boxes, Math.Min(length, MaxBoxesCount));
                                for (int index = 0; index < p_boxes.Length; index++)
                                {
                                    p_boxes[index] = Box.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }
                return new Box(p_name, p_seeds, p_boxes);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class Seed : MessageBase<Seed>
    {
        static Seed()
        {
            Seed.Formatter = new CustomFormatter();
        }
        public static readonly int MaxNameLength = 256;
        [JsonConstructor]
        public Seed(string name, long length, Metadata metadata)
        {
            if (name == null) throw new ArgumentNullException("name");
            if (metadata == null) throw new ArgumentNullException("metadata");
            if (name.Length > MaxNameLength) throw new ArgumentOutOfRangeException("name");
            this.Name = name;
            this.Length = length;
            this.Metadata = metadata;
            {
                int h = 0;
                if (this.Name != default(string)) h ^= this.Name.GetHashCode();
                if (this.Length != default(long)) h ^= this.Length.GetHashCode();
                if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public string Name { get; }
        [JsonProperty]
        public long Length { get; }
        [JsonProperty]
        public Metadata Metadata { get; }
        public override bool Equals(Seed target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Name != target.Name) return false;
            if (this.Length != target.Length) return false;
            if (this.Metadata != target.Metadata) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<Seed>
        {
            private int GetPropertyCount(Seed value)
            {
                int c = 0;
                if (value.Name != default(string)) c++;
                if (value.Length != default(long)) c++;
                if (value.Metadata != default(Metadata)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, Seed value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Name
                if (value.Name != default(string))
                {
                    w.Write((ulong)0);
                    w.Write(value.Name);
                }
                // Length
                if (value.Length != default(long))
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Length);
                }
                // Metadata
                if (value.Metadata != default(Metadata))
                {
                    w.Write((ulong)2);
                    Metadata.Formatter.Serialize(w, value.Metadata, rank + 1);
                }
            }
            public Seed Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                string p_name = default(string);
                long p_length = default(long);
                Metadata p_metadata = default(Metadata);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Name
                            {
                                p_name = r.GetString(MaxNameLength);
                                break;
                            }
                        case 1: // Length
                            {
                                p_length = (long)r.GetUInt64();
                                break;
                            }
                        case 2: // Metadata
                            {
                                p_metadata = Metadata.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }
                return new Seed(p_name, p_length, p_metadata);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class CommentContent : MessageBase<CommentContent>
    {
        static CommentContent()
        {
            CommentContent.Formatter = new CustomFormatter();
        }
        public static readonly int MaxCommentLength = 8192;
        [JsonConstructor]
        public CommentContent(string comment)
        {
            if (comment == null) throw new ArgumentNullException("comment");
            if (comment.Length > MaxCommentLength) throw new ArgumentOutOfRangeException("comment");
            this.Comment = comment;
            {
                int h = 0;
                if (this.Comment != default(string)) h ^= this.Comment.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
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
        private sealed class CustomFormatter : IMessageFormatter<CommentContent>
        {
            private int GetPropertyCount(CommentContent value)
            {
                int c = 0;
                if (value.Comment != default(string)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, CommentContent value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Comment
                if (value.Comment != default(string))
                {
                    w.Write((ulong)0);
                    w.Write(value.Comment);
                }
            }
            public CommentContent Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                string p_comment = default(string);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Comment
                            {
                                p_comment = r.GetString(MaxCommentLength);
                                break;
                            }
                    }
                }
                return new CommentContent(p_comment);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class BroadcastProfileMessage : MessageBase<BroadcastProfileMessage>
    {
        static BroadcastProfileMessage()
        {
            BroadcastProfileMessage.Formatter = new CustomFormatter();
        }
        [JsonConstructor]
        public BroadcastProfileMessage(Signature authorSignature, DateTime creationTime, ProfileContent value)
        {
            if (authorSignature == null) throw new ArgumentNullException("authorSignature");
            if (value == null) throw new ArgumentNullException("value");
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime.Normalize();
            this.Value = value;
            {
                int h = 0;
                if (this.AuthorSignature != default(Signature)) h ^= this.AuthorSignature.GetHashCode();
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.Value != default(ProfileContent)) h ^= this.Value.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public Signature AuthorSignature { get; }
        [JsonProperty]
        public DateTime CreationTime { get; }
        [JsonProperty]
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
        private sealed class CustomFormatter : IMessageFormatter<BroadcastProfileMessage>
        {
            private int GetPropertyCount(BroadcastProfileMessage value)
            {
                int c = 0;
                if (value.AuthorSignature != default(Signature)) c++;
                if (value.CreationTime != default(DateTime)) c++;
                if (value.Value != default(ProfileContent)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, BroadcastProfileMessage value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // AuthorSignature
                if (value.AuthorSignature != default(Signature))
                {
                    w.Write((ulong)0);
                    Signature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default(DateTime))
                {
                    w.Write((ulong)1);
                    w.Write(value.CreationTime);
                }
                // Value
                if (value.Value != default(ProfileContent))
                {
                    w.Write((ulong)2);
                    ProfileContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }
            public BroadcastProfileMessage Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                Signature p_authorSignature = default(Signature);
                DateTime p_creationTime = default(DateTime);
                ProfileContent p_value = default(ProfileContent);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // AuthorSignature
                            {
                                p_authorSignature = Signature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetDateTime();
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
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class BroadcastStoreMessage : MessageBase<BroadcastStoreMessage>
    {
        static BroadcastStoreMessage()
        {
            BroadcastStoreMessage.Formatter = new CustomFormatter();
        }
        [JsonConstructor]
        public BroadcastStoreMessage(Signature authorSignature, DateTime creationTime, StoreContent value)
        {
            if (authorSignature == null) throw new ArgumentNullException("authorSignature");
            if (value == null) throw new ArgumentNullException("value");
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime.Normalize();
            this.Value = value;
            {
                int h = 0;
                if (this.AuthorSignature != default(Signature)) h ^= this.AuthorSignature.GetHashCode();
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.Value != default(StoreContent)) h ^= this.Value.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public Signature AuthorSignature { get; }
        [JsonProperty]
        public DateTime CreationTime { get; }
        [JsonProperty]
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
        private sealed class CustomFormatter : IMessageFormatter<BroadcastStoreMessage>
        {
            private int GetPropertyCount(BroadcastStoreMessage value)
            {
                int c = 0;
                if (value.AuthorSignature != default(Signature)) c++;
                if (value.CreationTime != default(DateTime)) c++;
                if (value.Value != default(StoreContent)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, BroadcastStoreMessage value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // AuthorSignature
                if (value.AuthorSignature != default(Signature))
                {
                    w.Write((ulong)0);
                    Signature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default(DateTime))
                {
                    w.Write((ulong)1);
                    w.Write(value.CreationTime);
                }
                // Value
                if (value.Value != default(StoreContent))
                {
                    w.Write((ulong)2);
                    StoreContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }
            public BroadcastStoreMessage Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                Signature p_authorSignature = default(Signature);
                DateTime p_creationTime = default(DateTime);
                StoreContent p_value = default(StoreContent);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // AuthorSignature
                            {
                                p_authorSignature = Signature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetDateTime();
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
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class MulticastCommentMessage : MessageBase<MulticastCommentMessage>
    {
        static MulticastCommentMessage()
        {
            MulticastCommentMessage.Formatter = new CustomFormatter();
        }
        [JsonConstructor]
        public MulticastCommentMessage(Tag tag, Signature authorSignature, DateTime creationTime, Cost cost, CommentContent value)
        {
            if (tag == null) throw new ArgumentNullException("tag");
            if (authorSignature == null) throw new ArgumentNullException("authorSignature");
            if (cost == null) throw new ArgumentNullException("cost");
            if (value == null) throw new ArgumentNullException("value");
            this.Tag = tag;
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime.Normalize();
            this.Cost = cost;
            this.Value = value;
            {
                int h = 0;
                if (this.Tag != default(Tag)) h ^= this.Tag.GetHashCode();
                if (this.AuthorSignature != default(Signature)) h ^= this.AuthorSignature.GetHashCode();
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.Cost != default(Cost)) h ^= this.Cost.GetHashCode();
                if (this.Value != default(CommentContent)) h ^= this.Value.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public Tag Tag { get; }
        [JsonProperty]
        public Signature AuthorSignature { get; }
        [JsonProperty]
        public DateTime CreationTime { get; }
        [JsonProperty]
        public Cost Cost { get; }
        [JsonProperty]
        public CommentContent Value { get; }
        public override bool Equals(MulticastCommentMessage target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Cost != target.Cost) return false;
            if (this.Value != target.Value) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<MulticastCommentMessage>
        {
            private int GetPropertyCount(MulticastCommentMessage value)
            {
                int c = 0;
                if (value.Tag != default(Tag)) c++;
                if (value.AuthorSignature != default(Signature)) c++;
                if (value.CreationTime != default(DateTime)) c++;
                if (value.Cost != default(Cost)) c++;
                if (value.Value != default(CommentContent)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, MulticastCommentMessage value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Tag
                if (value.Tag != default(Tag))
                {
                    w.Write((ulong)0);
                    Tag.Formatter.Serialize(w, value.Tag, rank + 1);
                }
                // AuthorSignature
                if (value.AuthorSignature != default(Signature))
                {
                    w.Write((ulong)1);
                    Signature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default(DateTime))
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Cost
                if (value.Cost != default(Cost))
                {
                    w.Write((ulong)3);
                    Cost.Formatter.Serialize(w, value.Cost, rank + 1);
                }
                // Value
                if (value.Value != default(CommentContent))
                {
                    w.Write((ulong)4);
                    CommentContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }
            public MulticastCommentMessage Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                Tag p_tag = default(Tag);
                Signature p_authorSignature = default(Signature);
                DateTime p_creationTime = default(DateTime);
                Cost p_cost = default(Cost);
                CommentContent p_value = default(CommentContent);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Tag
                            {
                                p_tag = Tag.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // AuthorSignature
                            {
                                p_authorSignature = Signature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetDateTime();
                                break;
                            }
                        case 3: // Cost
                            {
                                p_cost = Cost.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 4: // Value
                            {
                                p_value = CommentContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }
                return new MulticastCommentMessage(p_tag, p_authorSignature, p_creationTime, p_cost, p_value);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public sealed partial class UnicastCommentMessage : MessageBase<UnicastCommentMessage>
    {
        static UnicastCommentMessage()
        {
            UnicastCommentMessage.Formatter = new CustomFormatter();
        }
        [JsonConstructor]
        public UnicastCommentMessage(Signature targetSignature, Signature authorSignature, DateTime creationTime, CommentContent value)
        {
            if (targetSignature == null) throw new ArgumentNullException("targetSignature");
            if (authorSignature == null) throw new ArgumentNullException("authorSignature");
            if (value == null) throw new ArgumentNullException("value");
            this.TargetSignature = targetSignature;
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime.Normalize();
            this.Value = value;
            {
                int h = 0;
                if (this.TargetSignature != default(Signature)) h ^= this.TargetSignature.GetHashCode();
                if (this.AuthorSignature != default(Signature)) h ^= this.AuthorSignature.GetHashCode();
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.Value != default(CommentContent)) h ^= this.Value.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public Signature TargetSignature { get; }
        [JsonProperty]
        public Signature AuthorSignature { get; }
        [JsonProperty]
        public DateTime CreationTime { get; }
        [JsonProperty]
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
        private sealed class CustomFormatter : IMessageFormatter<UnicastCommentMessage>
        {
            private int GetPropertyCount(UnicastCommentMessage value)
            {
                int c = 0;
                if (value.TargetSignature != default(Signature)) c++;
                if (value.AuthorSignature != default(Signature)) c++;
                if (value.CreationTime != default(DateTime)) c++;
                if (value.Value != default(CommentContent)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, UnicastCommentMessage value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // TargetSignature
                if (value.TargetSignature != default(Signature))
                {
                    w.Write((ulong)0);
                    Signature.Formatter.Serialize(w, value.TargetSignature, rank + 1);
                }
                // AuthorSignature
                if (value.AuthorSignature != default(Signature))
                {
                    w.Write((ulong)1);
                    Signature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default(DateTime))
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Value
                if (value.Value != default(CommentContent))
                {
                    w.Write((ulong)3);
                    CommentContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }
            public UnicastCommentMessage Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                Signature p_targetSignature = default(Signature);
                Signature p_authorSignature = default(Signature);
                DateTime p_creationTime = default(DateTime);
                CommentContent p_value = default(CommentContent);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // TargetSignature
                            {
                                p_targetSignature = Signature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // AuthorSignature
                            {
                                p_authorSignature = Signature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetDateTime();
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
    [StructLayout(LayoutKind.Sequential, Pack = 1)]
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    public readonly struct Hash : IEquatable<Hash>
    {
        public static IMessageFormatter<Hash> Formatter { get; }
        static Hash()
        {
            Hash.Formatter = new CustomFormatter();
        }
        public static readonly int MaxValueLength = 32;
        [JsonConstructor]
        public Hash(HashAlgorithm algorithm, byte[] value)
        {
            if (value == null) throw new ArgumentNullException("value");
            if (value.Length > MaxValueLength) throw new ArgumentOutOfRangeException("value");
            this.Algorithm = algorithm;
            this.Value = value;
            {
                int h = 0;
                if (this.Algorithm != default(HashAlgorithm)) h ^= this.Algorithm.GetHashCode();
                if (this.Value != default(byte[])) h ^= ItemUtils.GetHashCode(this.Value);
                _hashCode = h;
            }
        }
        [JsonProperty]
        public HashAlgorithm Algorithm { get; }
        [JsonProperty]
        public byte[] Value { get; }
        public static bool operator ==(Hash x, Hash y) => x.Equals(y);
        public static bool operator !=(Hash x, Hash y) => !x.Equals(y);
        public override bool Equals(object other)
        {
            if (!(other is Hash)) return false;
            return this.Equals((Hash)other);
        }
        public bool Equals(Hash target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Algorithm != target.Algorithm) return false;
            if ((this.Value == null) != (target.Value == null)) return false;
            if ((this.Value != null && target.Value != null)
                && !Unsafe.Equals(this.Value, target.Value)) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<Hash>
        {
            public void Serialize(MessageStreamWriter w, Hash value, int rank)
            {
                if (rank > 256) throw new FormatException();
                // Algorithm
                w.Write((ulong)value.Algorithm);
                // Value
                w.Write(value.Value);
            }
            public Hash Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                // Algorithm
                var p_algorithm = (HashAlgorithm)r.GetUInt64();
                // Value
                var p_value = r.GetBytes(MaxValueLength);
                return new Hash(p_algorithm, p_value);
            }
        }
    }
}

