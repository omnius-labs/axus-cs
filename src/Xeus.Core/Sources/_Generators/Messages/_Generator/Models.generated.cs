using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Amoeba.Messages;
using Newtonsoft.Json;
using Omnius.Base;
using Omnius.Security;
using Omnius.Serialization;
using Omnius.Utils;

namespace Amoeba.Service
{
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class Index : MessageBase<Index>
    {
        static Index()
        {
            Index.Formatter = new CustomFormatter();
        }
        public static readonly int MaxHashesCount = 1024 * 1024 * 1024;
        [JsonConstructor]
        public Index(IList<Hash> hashes)
        {
            if (hashes == null) throw new ArgumentNullException("hashes");
            if (hashes.Count > MaxHashesCount) throw new ArgumentOutOfRangeException("hashes");
            for (int i = 0; i < hashes.Count; i++)
            {
                if (hashes[i] == null) throw new ArgumentNullException("hashes[i]");
            }
            this.Hashes = new ReadOnlyCollection<Hash>(hashes);
            {
                int h = 0;
                for (int i = 0; i < Hashes.Count; i++)
                {
                    h ^= this.Hashes[i].GetHashCode();
                }
                _hashCode = h;
            }
        }
        [JsonProperty]
        public IReadOnlyList<Hash> Hashes { get; }
        public override bool Equals(Index target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!CollectionUtils.Equals(this.Hashes, target.Hashes)) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<Index>
        {
            private int GetPropertyCount(Index value)
            {
                int c = 0;
                if (value.Hashes.Count != 0) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, Index value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Hashes
                if (value.Hashes.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Hashes.Count);
                    for (int i = 0; i < value.Hashes.Count; i++)
                    {
                        Hash.Formatter.Serialize(w, value.Hashes[i], rank + 1);
                    }
                }
            }
            public Index Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                Hash[] p_hashes = Array.Empty<Hash>();
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Hashes
                            {
                                var length = (int)r.GetUInt64();
                                Array.Resize(ref p_hashes, Math.Min(length, MaxHashesCount));
                                for (int index = 0; index < p_hashes.Length; index++)
                                {
                                    p_hashes[index] = Hash.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }
                return new Index(p_hashes);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class BroadcastMetadata : MessageBase<BroadcastMetadata>
    {
        static BroadcastMetadata()
        {
            BroadcastMetadata.Formatter = new CustomFormatter();
        }
        public static readonly int MaxTypeLength = 256;
        [JsonConstructor]
        public BroadcastMetadata(string type, DateTime creationTime, Metadata metadata, Certificate certificate)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (metadata == null) throw new ArgumentNullException("metadata");
            if (type.Length > MaxTypeLength) throw new ArgumentOutOfRangeException("type");
            this.Type = type;
            this.CreationTime = creationTime.Normalize();
            this.Metadata = metadata;
            this.Certificate = certificate;
            {
                int h = 0;
                if (this.Type != default(string)) h ^= this.Type.GetHashCode();
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                if (this.Certificate != default(Certificate)) h ^= this.Certificate.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public string Type { get; }
        [JsonProperty]
        public DateTime CreationTime { get; }
        [JsonProperty]
        public Metadata Metadata { get; }
        [JsonProperty]
        public Certificate Certificate { get; }
        public override bool Equals(BroadcastMetadata target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Metadata != target.Metadata) return false;
            if (this.Certificate != target.Certificate) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<BroadcastMetadata>
        {
            private int GetPropertyCount(BroadcastMetadata value)
            {
                int c = 0;
                if (value.Type != default(string)) c++;
                if (value.CreationTime != default(DateTime)) c++;
                if (value.Metadata != default(Metadata)) c++;
                if (value.Certificate != default(Certificate)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, BroadcastMetadata value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Type
                if (value.Type != default(string))
                {
                    w.Write((ulong)0);
                    w.Write(value.Type);
                }
                // CreationTime
                if (value.CreationTime != default(DateTime))
                {
                    w.Write((ulong)1);
                    w.Write(value.CreationTime);
                }
                // Metadata
                if (value.Metadata != default(Metadata))
                {
                    w.Write((ulong)2);
                    Metadata.Formatter.Serialize(w, value.Metadata, rank + 1);
                }
                // Certificate
                if (value.Certificate != default(Certificate))
                {
                    w.Write((ulong)3);
                    Certificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }
            public BroadcastMetadata Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                string p_type = default(string);
                DateTime p_creationTime = default(DateTime);
                Metadata p_metadata = default(Metadata);
                Certificate p_certificate = default(Certificate);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Type
                            {
                                p_type = r.GetString(MaxTypeLength);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetDateTime();
                                break;
                            }
                        case 2: // Metadata
                            {
                                p_metadata = Metadata.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 3: // Certificate
                            {
                                p_certificate = Certificate.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }
                return new BroadcastMetadata(p_type, p_creationTime, p_metadata, p_certificate);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class UnicastMetadata : MessageBase<UnicastMetadata>
    {
        static UnicastMetadata()
        {
            UnicastMetadata.Formatter = new CustomFormatter();
        }
        public static readonly int MaxTypeLength = 256;
        [JsonConstructor]
        public UnicastMetadata(string type, Signature signature, DateTime creationTime, Metadata metadata, Certificate certificate)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (signature == null) throw new ArgumentNullException("signature");
            if (metadata == null) throw new ArgumentNullException("metadata");
            if (type.Length > MaxTypeLength) throw new ArgumentOutOfRangeException("type");
            this.Type = type;
            this.Signature = signature;
            this.CreationTime = creationTime.Normalize();
            this.Metadata = metadata;
            this.Certificate = certificate;
            {
                int h = 0;
                if (this.Type != default(string)) h ^= this.Type.GetHashCode();
                if (this.Signature != default(Signature)) h ^= this.Signature.GetHashCode();
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                if (this.Certificate != default(Certificate)) h ^= this.Certificate.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public string Type { get; }
        [JsonProperty]
        public Signature Signature { get; }
        [JsonProperty]
        public DateTime CreationTime { get; }
        [JsonProperty]
        public Metadata Metadata { get; }
        [JsonProperty]
        public Certificate Certificate { get; }
        public override bool Equals(UnicastMetadata target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Signature != target.Signature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Metadata != target.Metadata) return false;
            if (this.Certificate != target.Certificate) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<UnicastMetadata>
        {
            private int GetPropertyCount(UnicastMetadata value)
            {
                int c = 0;
                if (value.Type != default(string)) c++;
                if (value.Signature != default(Signature)) c++;
                if (value.CreationTime != default(DateTime)) c++;
                if (value.Metadata != default(Metadata)) c++;
                if (value.Certificate != default(Certificate)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, UnicastMetadata value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Type
                if (value.Type != default(string))
                {
                    w.Write((ulong)0);
                    w.Write(value.Type);
                }
                // Signature
                if (value.Signature != default(Signature))
                {
                    w.Write((ulong)1);
                    Signature.Formatter.Serialize(w, value.Signature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default(DateTime))
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Metadata
                if (value.Metadata != default(Metadata))
                {
                    w.Write((ulong)3);
                    Metadata.Formatter.Serialize(w, value.Metadata, rank + 1);
                }
                // Certificate
                if (value.Certificate != default(Certificate))
                {
                    w.Write((ulong)4);
                    Certificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }
            public UnicastMetadata Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                string p_type = default(string);
                Signature p_signature = default(Signature);
                DateTime p_creationTime = default(DateTime);
                Metadata p_metadata = default(Metadata);
                Certificate p_certificate = default(Certificate);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Type
                            {
                                p_type = r.GetString(MaxTypeLength);
                                break;
                            }
                        case 1: // Signature
                            {
                                p_signature = Signature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetDateTime();
                                break;
                            }
                        case 3: // Metadata
                            {
                                p_metadata = Metadata.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 4: // Certificate
                            {
                                p_certificate = Certificate.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }
                return new UnicastMetadata(p_type, p_signature, p_creationTime, p_metadata, p_certificate);
            }
        }
    }
    [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
    internal sealed partial class MulticastMetadata : MessageBase<MulticastMetadata>
    {
        static MulticastMetadata()
        {
            MulticastMetadata.Formatter = new CustomFormatter();
        }
        public static readonly int MaxTypeLength = 256;
        [JsonConstructor]
        public MulticastMetadata(string type, Tag tag, DateTime creationTime, Metadata metadata, Cash cash, Certificate certificate)
        {
            if (type == null) throw new ArgumentNullException("type");
            if (tag == null) throw new ArgumentNullException("tag");
            if (metadata == null) throw new ArgumentNullException("metadata");
            if (type.Length > MaxTypeLength) throw new ArgumentOutOfRangeException("type");
            this.Type = type;
            this.Tag = tag;
            this.CreationTime = creationTime.Normalize();
            this.Metadata = metadata;
            this.Cash = cash;
            this.Certificate = certificate;
            {
                int h = 0;
                if (this.Type != default(string)) h ^= this.Type.GetHashCode();
                if (this.Tag != default(Tag)) h ^= this.Tag.GetHashCode();
                if (this.CreationTime != default(DateTime)) h ^= this.CreationTime.GetHashCode();
                if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                if (this.Cash != default(Cash)) h ^= this.Cash.GetHashCode();
                if (this.Certificate != default(Certificate)) h ^= this.Certificate.GetHashCode();
                _hashCode = h;
            }
        }
        [JsonProperty]
        public string Type { get; }
        [JsonProperty]
        public Tag Tag { get; }
        [JsonProperty]
        public DateTime CreationTime { get; }
        [JsonProperty]
        public Metadata Metadata { get; }
        [JsonProperty]
        public Cash Cash { get; }
        [JsonProperty]
        public Certificate Certificate { get; }
        public override bool Equals(MulticastMetadata target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Tag != target.Tag) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Metadata != target.Metadata) return false;
            if (this.Cash != target.Cash) return false;
            if (this.Certificate != target.Certificate) return false;
            return true;
        }
        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;
        private sealed class CustomFormatter : IMessageFormatter<MulticastMetadata>
        {
            private int GetPropertyCount(MulticastMetadata value)
            {
                int c = 0;
                if (value.Type != default(string)) c++;
                if (value.Tag != default(Tag)) c++;
                if (value.CreationTime != default(DateTime)) c++;
                if (value.Metadata != default(Metadata)) c++;
                if (value.Cash != default(Cash)) c++;
                if (value.Certificate != default(Certificate)) c++;
                return c;
            }
            public void Serialize(MessageStreamWriter w, MulticastMetadata value, int rank)
            {
                if (rank > 256) throw new FormatException();
                w.Write((ulong)this.GetPropertyCount(value));
                // Type
                if (value.Type != default(string))
                {
                    w.Write((ulong)0);
                    w.Write(value.Type);
                }
                // Tag
                if (value.Tag != default(Tag))
                {
                    w.Write((ulong)1);
                    Tag.Formatter.Serialize(w, value.Tag, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default(DateTime))
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Metadata
                if (value.Metadata != default(Metadata))
                {
                    w.Write((ulong)3);
                    Metadata.Formatter.Serialize(w, value.Metadata, rank + 1);
                }
                // Cash
                if (value.Cash != default(Cash))
                {
                    w.Write((ulong)4);
                    Cash.Formatter.Serialize(w, value.Cash, rank + 1);
                }
                // Certificate
                if (value.Certificate != default(Certificate))
                {
                    w.Write((ulong)5);
                    Certificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }
            public MulticastMetadata Deserialize(MessageStreamReader r, int rank)
            {
                if (rank > 256) throw new FormatException();
                string p_type = default(string);
                Tag p_tag = default(Tag);
                DateTime p_creationTime = default(DateTime);
                Metadata p_metadata = default(Metadata);
                Cash p_cash = default(Cash);
                Certificate p_certificate = default(Certificate);
                for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Type
                            {
                                p_type = r.GetString(MaxTypeLength);
                                break;
                            }
                        case 1: // Tag
                            {
                                p_tag = Tag.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetDateTime();
                                break;
                            }
                        case 3: // Metadata
                            {
                                p_metadata = Metadata.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 4: // Cash
                            {
                                p_cash = Cash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 5: // Certificate
                            {
                                p_certificate = Certificate.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }
                return new MulticastMetadata(p_type, p_tag, p_creationTime, p_metadata, p_cash, p_certificate);
            }
        }
    }

    sealed partial class NetworkManager
    {
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class SearchHandshakePacket : MessageBase<SearchHandshakePacket>
        {
            static SearchHandshakePacket()
            {
                SearchHandshakePacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxSearchProtocolVersionsCount = 32;
            [JsonConstructor]
            public SearchHandshakePacket(IList<SearchProtocolVersion> searchProtocolVersions)
            {
                if (searchProtocolVersions == null) throw new ArgumentNullException("searchProtocolVersions");
                if (searchProtocolVersions.Count > MaxSearchProtocolVersionsCount) throw new ArgumentOutOfRangeException("searchProtocolVersions");
                this.SearchProtocolVersions = new ReadOnlyCollection<SearchProtocolVersion>(searchProtocolVersions);
                {
                    int h = 0;
                    for (int i = 0; i < SearchProtocolVersions.Count; i++)
                    {
                        h ^= this.SearchProtocolVersions[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<SearchProtocolVersion> SearchProtocolVersions { get; }
            public override bool Equals(SearchHandshakePacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.SearchProtocolVersions, target.SearchProtocolVersions)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<SearchHandshakePacket>
            {
                private int GetPropertyCount(SearchHandshakePacket value)
                {
                    int c = 0;
                    if (value.SearchProtocolVersions.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, SearchHandshakePacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // SearchProtocolVersions
                    if (value.SearchProtocolVersions.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.SearchProtocolVersions.Count);
                        for (int i = 0; i < value.SearchProtocolVersions.Count; i++)
                        {
                            w.Write((ulong)value.SearchProtocolVersions[i]);
                        }
                    }
                }
                public SearchHandshakePacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    SearchProtocolVersion[] p_searchProtocolVersions = Array.Empty<SearchProtocolVersion>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // SearchProtocolVersions
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_searchProtocolVersions, Math.Min(length, MaxSearchProtocolVersionsCount));
                                    for (int index = 0; index < p_searchProtocolVersions.Length; index++)
                                    {
                                        p_searchProtocolVersions[index] = (SearchProtocolVersion)r.GetUInt64();
                                    }
                                    break;
                                }
                        }
                    }
                    return new SearchHandshakePacket(p_searchProtocolVersions);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class ExchangeHandshakePacket : MessageBase<ExchangeHandshakePacket>
        {
            static ExchangeHandshakePacket()
            {
                ExchangeHandshakePacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxExchangeProtocolVersionsCount = 32;
            [JsonConstructor]
            public ExchangeHandshakePacket(IList<ExchangeProtocolVersion> exchangeProtocolVersions)
            {
                if (exchangeProtocolVersions == null) throw new ArgumentNullException("exchangeProtocolVersions");
                if (exchangeProtocolVersions.Count > MaxExchangeProtocolVersionsCount) throw new ArgumentOutOfRangeException("exchangeProtocolVersions");
                this.ExchangeProtocolVersions = new ReadOnlyCollection<ExchangeProtocolVersion>(exchangeProtocolVersions);
                {
                    int h = 0;
                    for (int i = 0; i < ExchangeProtocolVersions.Count; i++)
                    {
                        h ^= this.ExchangeProtocolVersions[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<ExchangeProtocolVersion> ExchangeProtocolVersions { get; }
            public override bool Equals(ExchangeHandshakePacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.ExchangeProtocolVersions, target.ExchangeProtocolVersions)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<ExchangeHandshakePacket>
            {
                private int GetPropertyCount(ExchangeHandshakePacket value)
                {
                    int c = 0;
                    if (value.ExchangeProtocolVersions.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, ExchangeHandshakePacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // ExchangeProtocolVersions
                    if (value.ExchangeProtocolVersions.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.ExchangeProtocolVersions.Count);
                        for (int i = 0; i < value.ExchangeProtocolVersions.Count; i++)
                        {
                            w.Write((ulong)value.ExchangeProtocolVersions[i]);
                        }
                    }
                }
                public ExchangeHandshakePacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    ExchangeProtocolVersion[] p_exchangeProtocolVersions = Array.Empty<ExchangeProtocolVersion>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // ExchangeProtocolVersions
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_exchangeProtocolVersions, Math.Min(length, MaxExchangeProtocolVersionsCount));
                                    for (int index = 0; index < p_exchangeProtocolVersions.Length; index++)
                                    {
                                        p_exchangeProtocolVersions[index] = (ExchangeProtocolVersion)r.GetUInt64();
                                    }
                                    break;
                                }
                        }
                    }
                    return new ExchangeHandshakePacket(p_exchangeProtocolVersions);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class SearchProfilePacket : MessageBase<SearchProfilePacket>
        {
            static SearchProfilePacket()
            {
                SearchProfilePacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxIdLength = 32;
            [JsonConstructor]
            public SearchProfilePacket(byte[] id, Location location)
            {
                if (id == null) throw new ArgumentNullException("id");
                if (location == null) throw new ArgumentNullException("location");
                if (id.Length > MaxIdLength) throw new ArgumentOutOfRangeException("id");
                this.Id = id;
                this.Location = location;
                {
                    int h = 0;
                    if (this.Id != default(byte[])) h ^= ItemUtils.GetHashCode(this.Id);
                    if (this.Location != default(Location)) h ^= this.Location.GetHashCode();
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public byte[] Id { get; }
            [JsonProperty]
            public Location Location { get; }
            public override bool Equals(SearchProfilePacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if ((this.Id == null) != (target.Id == null)) return false;
                if ((this.Id != null && target.Id != null)
                    && !Unsafe.Equals(this.Id, target.Id)) return false;
                if (this.Location != target.Location) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<SearchProfilePacket>
            {
                private int GetPropertyCount(SearchProfilePacket value)
                {
                    int c = 0;
                    if (value.Id != default(byte[])) c++;
                    if (value.Location != default(Location)) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, SearchProfilePacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Id
                    if (value.Id != default(byte[]))
                    {
                        w.Write((ulong)0);
                        w.Write(value.Id);
                    }
                    // Location
                    if (value.Location != default(Location))
                    {
                        w.Write((ulong)1);
                        Location.Formatter.Serialize(w, value.Location, rank + 1);
                    }
                }
                public SearchProfilePacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    byte[] p_id = default(byte[]);
                    Location p_location = default(Location);
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Id
                                {
                                    p_id = r.GetBytes(MaxIdLength);
                                    break;
                                }
                            case 1: // Location
                                {
                                    p_location = Location.Formatter.Deserialize(r, rank + 1);
                                    break;
                                }
                        }
                    }
                    return new SearchProfilePacket(p_id, p_location);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class ExchangeProfilePacket : MessageBase<ExchangeProfilePacket>
        {
            static ExchangeProfilePacket()
            {
                ExchangeProfilePacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxIdLength = 32;
            [JsonConstructor]
            public ExchangeProfilePacket(byte[] id, Location location)
            {
                if (id == null) throw new ArgumentNullException("id");
                if (location == null) throw new ArgumentNullException("location");
                if (id.Length > MaxIdLength) throw new ArgumentOutOfRangeException("id");
                this.Id = id;
                this.Location = location;
                {
                    int h = 0;
                    if (this.Id != default(byte[])) h ^= ItemUtils.GetHashCode(this.Id);
                    if (this.Location != default(Location)) h ^= this.Location.GetHashCode();
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public byte[] Id { get; }
            [JsonProperty]
            public Location Location { get; }
            public override bool Equals(ExchangeProfilePacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if ((this.Id == null) != (target.Id == null)) return false;
                if ((this.Id != null && target.Id != null)
                    && !Unsafe.Equals(this.Id, target.Id)) return false;
                if (this.Location != target.Location) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<ExchangeProfilePacket>
            {
                private int GetPropertyCount(ExchangeProfilePacket value)
                {
                    int c = 0;
                    if (value.Id != default(byte[])) c++;
                    if (value.Location != default(Location)) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, ExchangeProfilePacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Id
                    if (value.Id != default(byte[]))
                    {
                        w.Write((ulong)0);
                        w.Write(value.Id);
                    }
                    // Location
                    if (value.Location != default(Location))
                    {
                        w.Write((ulong)1);
                        Location.Formatter.Serialize(w, value.Location, rank + 1);
                    }
                }
                public ExchangeProfilePacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    byte[] p_id = default(byte[]);
                    Location p_location = default(Location);
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Id
                                {
                                    p_id = r.GetBytes(MaxIdLength);
                                    break;
                                }
                            case 1: // Location
                                {
                                    p_location = Location.Formatter.Deserialize(r, rank + 1);
                                    break;
                                }
                        }
                    }
                    return new ExchangeProfilePacket(p_id, p_location);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class LocationsPublishPacket : MessageBase<LocationsPublishPacket>
        {
            static LocationsPublishPacket()
            {
                LocationsPublishPacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxLocationsCount = 256;
            [JsonConstructor]
            public LocationsPublishPacket(IList<Location> locations)
            {
                if (locations == null) throw new ArgumentNullException("locations");
                if (locations.Count > MaxLocationsCount) throw new ArgumentOutOfRangeException("locations");
                for (int i = 0; i < locations.Count; i++)
                {
                    if (locations[i] == null) throw new ArgumentNullException("locations[i]");
                }
                this.Locations = new ReadOnlyCollection<Location>(locations);
                {
                    int h = 0;
                    for (int i = 0; i < Locations.Count; i++)
                    {
                        h ^= this.Locations[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<Location> Locations { get; }
            public override bool Equals(LocationsPublishPacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.Locations, target.Locations)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<LocationsPublishPacket>
            {
                private int GetPropertyCount(LocationsPublishPacket value)
                {
                    int c = 0;
                    if (value.Locations.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, LocationsPublishPacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Locations
                    if (value.Locations.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.Locations.Count);
                        for (int i = 0; i < value.Locations.Count; i++)
                        {
                            Location.Formatter.Serialize(w, value.Locations[i], rank + 1);
                        }
                    }
                }
                public LocationsPublishPacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    Location[] p_locations = Array.Empty<Location>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Locations
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_locations, Math.Min(length, MaxLocationsCount));
                                    for (int index = 0; index < p_locations.Length; index++)
                                    {
                                        p_locations[index] = Location.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                        }
                    }
                    return new LocationsPublishPacket(p_locations);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class LocationsRequestPacket : MessageBase<LocationsRequestPacket>
        {
            static LocationsRequestPacket()
            {
                LocationsRequestPacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxMetadatasCount = 256;
            [JsonConstructor]
            public LocationsRequestPacket(IList<Metadata> metadatas)
            {
                if (metadatas == null) throw new ArgumentNullException("metadatas");
                if (metadatas.Count > MaxMetadatasCount) throw new ArgumentOutOfRangeException("metadatas");
                for (int i = 0; i < metadatas.Count; i++)
                {
                    if (metadatas[i] == null) throw new ArgumentNullException("metadatas[i]");
                }
                this.Metadatas = new ReadOnlyCollection<Metadata>(metadatas);
                {
                    int h = 0;
                    for (int i = 0; i < Metadatas.Count; i++)
                    {
                        h ^= this.Metadatas[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<Metadata> Metadatas { get; }
            public override bool Equals(LocationsRequestPacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.Metadatas, target.Metadatas)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<LocationsRequestPacket>
            {
                private int GetPropertyCount(LocationsRequestPacket value)
                {
                    int c = 0;
                    if (value.Metadatas.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, LocationsRequestPacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Metadatas
                    if (value.Metadatas.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.Metadatas.Count);
                        for (int i = 0; i < value.Metadatas.Count; i++)
                        {
                            Metadata.Formatter.Serialize(w, value.Metadatas[i], rank + 1);
                        }
                    }
                }
                public LocationsRequestPacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    Metadata[] p_metadatas = Array.Empty<Metadata>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Metadatas
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_metadatas, Math.Min(length, MaxMetadatasCount));
                                    for (int index = 0; index < p_metadatas.Length; index++)
                                    {
                                        p_metadatas[index] = Metadata.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                        }
                    }
                    return new LocationsRequestPacket(p_metadatas);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class LocationsResultPacket : MessageBase<LocationsResultPacket>
        {
            static LocationsResultPacket()
            {
                LocationsResultPacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxLocationsCount = 256;
            [JsonConstructor]
            public LocationsResultPacket(Metadata metadata, IList<Location> locations)
            {
                if (metadata == null) throw new ArgumentNullException("metadata");
                if (locations == null) throw new ArgumentNullException("locations");
                if (locations.Count > MaxLocationsCount) throw new ArgumentOutOfRangeException("locations");
                for (int i = 0; i < locations.Count; i++)
                {
                    if (locations[i] == null) throw new ArgumentNullException("locations[i]");
                }
                this.Metadata = metadata;
                this.Locations = new ReadOnlyCollection<Location>(locations);
                {
                    int h = 0;
                    if (this.Metadata != default(Metadata)) h ^= this.Metadata.GetHashCode();
                    for (int i = 0; i < Locations.Count; i++)
                    {
                        h ^= this.Locations[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public Metadata Metadata { get; }
            [JsonProperty]
            public IReadOnlyList<Location> Locations { get; }
            public override bool Equals(LocationsResultPacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (this.Metadata != target.Metadata) return false;
                if (!CollectionUtils.Equals(this.Locations, target.Locations)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<LocationsResultPacket>
            {
                private int GetPropertyCount(LocationsResultPacket value)
                {
                    int c = 0;
                    if (value.Metadata != default(Metadata)) c++;
                    if (value.Locations.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, LocationsResultPacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Metadata
                    if (value.Metadata != default(Metadata))
                    {
                        w.Write((ulong)0);
                        Metadata.Formatter.Serialize(w, value.Metadata, rank + 1);
                    }
                    // Locations
                    if (value.Locations.Count != 0)
                    {
                        w.Write((ulong)1);
                        w.Write((ulong)value.Locations.Count);
                        for (int i = 0; i < value.Locations.Count; i++)
                        {
                            Location.Formatter.Serialize(w, value.Locations[i], rank + 1);
                        }
                    }
                }
                public LocationsResultPacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    Metadata p_metadata = default(Metadata);
                    Location[] p_locations = Array.Empty<Location>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Metadata
                                {
                                    p_metadata = Metadata.Formatter.Deserialize(r, rank + 1);
                                    break;
                                }
                            case 1: // Locations
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_locations, Math.Min(length, MaxLocationsCount));
                                    for (int index = 0; index < p_locations.Length; index++)
                                    {
                                        p_locations[index] = Location.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                        }
                    }
                    return new LocationsResultPacket(p_metadata, p_locations);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class MetadatasRequestPacket : MessageBase<MetadatasRequestPacket>
        {
            static MetadatasRequestPacket()
            {
                MetadatasRequestPacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxBroadcastMetadataSignaturesCount = 1024 * 8;
            public static readonly int MaxUnicastMetadataSignaturesCount = 1024 * 8;
            public static readonly int MaxMulticastMetadataTagsCount = 1024 * 8;
            [JsonConstructor]
            public MetadatasRequestPacket(IList<Signature> broadcastMetadataSignatures, IList<Signature> unicastMetadataSignatures, IList<Tag> multicastMetadataTags)
            {
                if (broadcastMetadataSignatures == null) throw new ArgumentNullException("broadcastMetadataSignatures");
                if (unicastMetadataSignatures == null) throw new ArgumentNullException("unicastMetadataSignatures");
                if (multicastMetadataTags == null) throw new ArgumentNullException("multicastMetadataTags");
                if (broadcastMetadataSignatures.Count > MaxBroadcastMetadataSignaturesCount) throw new ArgumentOutOfRangeException("broadcastMetadataSignatures");
                if (unicastMetadataSignatures.Count > MaxUnicastMetadataSignaturesCount) throw new ArgumentOutOfRangeException("unicastMetadataSignatures");
                if (multicastMetadataTags.Count > MaxMulticastMetadataTagsCount) throw new ArgumentOutOfRangeException("multicastMetadataTags");
                for (int i = 0; i < broadcastMetadataSignatures.Count; i++)
                {
                    if (broadcastMetadataSignatures[i] == null) throw new ArgumentNullException("broadcastMetadataSignatures[i]");
                }
                for (int i = 0; i < unicastMetadataSignatures.Count; i++)
                {
                    if (unicastMetadataSignatures[i] == null) throw new ArgumentNullException("unicastMetadataSignatures[i]");
                }
                for (int i = 0; i < multicastMetadataTags.Count; i++)
                {
                    if (multicastMetadataTags[i] == null) throw new ArgumentNullException("multicastMetadataTags[i]");
                }
                this.BroadcastMetadataSignatures = new ReadOnlyCollection<Signature>(broadcastMetadataSignatures);
                this.UnicastMetadataSignatures = new ReadOnlyCollection<Signature>(unicastMetadataSignatures);
                this.MulticastMetadataTags = new ReadOnlyCollection<Tag>(multicastMetadataTags);
                {
                    int h = 0;
                    for (int i = 0; i < BroadcastMetadataSignatures.Count; i++)
                    {
                        h ^= this.BroadcastMetadataSignatures[i].GetHashCode();
                    }
                    for (int i = 0; i < UnicastMetadataSignatures.Count; i++)
                    {
                        h ^= this.UnicastMetadataSignatures[i].GetHashCode();
                    }
                    for (int i = 0; i < MulticastMetadataTags.Count; i++)
                    {
                        h ^= this.MulticastMetadataTags[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<Signature> BroadcastMetadataSignatures { get; }
            [JsonProperty]
            public IReadOnlyList<Signature> UnicastMetadataSignatures { get; }
            [JsonProperty]
            public IReadOnlyList<Tag> MulticastMetadataTags { get; }
            public override bool Equals(MetadatasRequestPacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.BroadcastMetadataSignatures, target.BroadcastMetadataSignatures)) return false;
                if (!CollectionUtils.Equals(this.UnicastMetadataSignatures, target.UnicastMetadataSignatures)) return false;
                if (!CollectionUtils.Equals(this.MulticastMetadataTags, target.MulticastMetadataTags)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<MetadatasRequestPacket>
            {
                private int GetPropertyCount(MetadatasRequestPacket value)
                {
                    int c = 0;
                    if (value.BroadcastMetadataSignatures.Count != 0) c++;
                    if (value.UnicastMetadataSignatures.Count != 0) c++;
                    if (value.MulticastMetadataTags.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, MetadatasRequestPacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // BroadcastMetadataSignatures
                    if (value.BroadcastMetadataSignatures.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.BroadcastMetadataSignatures.Count);
                        for (int i = 0; i < value.BroadcastMetadataSignatures.Count; i++)
                        {
                            Signature.Formatter.Serialize(w, value.BroadcastMetadataSignatures[i], rank + 1);
                        }
                    }
                    // UnicastMetadataSignatures
                    if (value.UnicastMetadataSignatures.Count != 0)
                    {
                        w.Write((ulong)1);
                        w.Write((ulong)value.UnicastMetadataSignatures.Count);
                        for (int i = 0; i < value.UnicastMetadataSignatures.Count; i++)
                        {
                            Signature.Formatter.Serialize(w, value.UnicastMetadataSignatures[i], rank + 1);
                        }
                    }
                    // MulticastMetadataTags
                    if (value.MulticastMetadataTags.Count != 0)
                    {
                        w.Write((ulong)2);
                        w.Write((ulong)value.MulticastMetadataTags.Count);
                        for (int i = 0; i < value.MulticastMetadataTags.Count; i++)
                        {
                            Tag.Formatter.Serialize(w, value.MulticastMetadataTags[i], rank + 1);
                        }
                    }
                }
                public MetadatasRequestPacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    Signature[] p_broadcastMetadataSignatures = Array.Empty<Signature>();
                    Signature[] p_unicastMetadataSignatures = Array.Empty<Signature>();
                    Tag[] p_multicastMetadataTags = Array.Empty<Tag>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // BroadcastMetadataSignatures
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_broadcastMetadataSignatures, Math.Min(length, MaxBroadcastMetadataSignaturesCount));
                                    for (int index = 0; index < p_broadcastMetadataSignatures.Length; index++)
                                    {
                                        p_broadcastMetadataSignatures[index] = Signature.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                            case 1: // UnicastMetadataSignatures
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_unicastMetadataSignatures, Math.Min(length, MaxUnicastMetadataSignaturesCount));
                                    for (int index = 0; index < p_unicastMetadataSignatures.Length; index++)
                                    {
                                        p_unicastMetadataSignatures[index] = Signature.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                            case 2: // MulticastMetadataTags
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_multicastMetadataTags, Math.Min(length, MaxMulticastMetadataTagsCount));
                                    for (int index = 0; index < p_multicastMetadataTags.Length; index++)
                                    {
                                        p_multicastMetadataTags[index] = Tag.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                        }
                    }
                    return new MetadatasRequestPacket(p_broadcastMetadataSignatures, p_unicastMetadataSignatures, p_multicastMetadataTags);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class MetadatasResultPacket : MessageBase<MetadatasResultPacket>
        {
            static MetadatasResultPacket()
            {
                MetadatasResultPacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxBroadcastMetadatasCount = 1024 * 8;
            public static readonly int MaxUnicastMetadatasCount = 1024 * 8;
            public static readonly int MaxMulticastMetadatasCount = 1024 * 8;
            [JsonConstructor]
            public MetadatasResultPacket(IList<BroadcastMetadata> broadcastMetadatas, IList<UnicastMetadata> unicastMetadatas, IList<MulticastMetadata> multicastMetadatas)
            {
                if (broadcastMetadatas == null) throw new ArgumentNullException("broadcastMetadatas");
                if (unicastMetadatas == null) throw new ArgumentNullException("unicastMetadatas");
                if (multicastMetadatas == null) throw new ArgumentNullException("multicastMetadatas");
                if (broadcastMetadatas.Count > MaxBroadcastMetadatasCount) throw new ArgumentOutOfRangeException("broadcastMetadatas");
                if (unicastMetadatas.Count > MaxUnicastMetadatasCount) throw new ArgumentOutOfRangeException("unicastMetadatas");
                if (multicastMetadatas.Count > MaxMulticastMetadatasCount) throw new ArgumentOutOfRangeException("multicastMetadatas");
                for (int i = 0; i < broadcastMetadatas.Count; i++)
                {
                    if (broadcastMetadatas[i] == null) throw new ArgumentNullException("broadcastMetadatas[i]");
                }
                for (int i = 0; i < unicastMetadatas.Count; i++)
                {
                    if (unicastMetadatas[i] == null) throw new ArgumentNullException("unicastMetadatas[i]");
                }
                for (int i = 0; i < multicastMetadatas.Count; i++)
                {
                    if (multicastMetadatas[i] == null) throw new ArgumentNullException("multicastMetadatas[i]");
                }
                this.BroadcastMetadatas = new ReadOnlyCollection<BroadcastMetadata>(broadcastMetadatas);
                this.UnicastMetadatas = new ReadOnlyCollection<UnicastMetadata>(unicastMetadatas);
                this.MulticastMetadatas = new ReadOnlyCollection<MulticastMetadata>(multicastMetadatas);
                {
                    int h = 0;
                    for (int i = 0; i < BroadcastMetadatas.Count; i++)
                    {
                        h ^= this.BroadcastMetadatas[i].GetHashCode();
                    }
                    for (int i = 0; i < UnicastMetadatas.Count; i++)
                    {
                        h ^= this.UnicastMetadatas[i].GetHashCode();
                    }
                    for (int i = 0; i < MulticastMetadatas.Count; i++)
                    {
                        h ^= this.MulticastMetadatas[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<BroadcastMetadata> BroadcastMetadatas { get; }
            [JsonProperty]
            public IReadOnlyList<UnicastMetadata> UnicastMetadatas { get; }
            [JsonProperty]
            public IReadOnlyList<MulticastMetadata> MulticastMetadatas { get; }
            public override bool Equals(MetadatasResultPacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.BroadcastMetadatas, target.BroadcastMetadatas)) return false;
                if (!CollectionUtils.Equals(this.UnicastMetadatas, target.UnicastMetadatas)) return false;
                if (!CollectionUtils.Equals(this.MulticastMetadatas, target.MulticastMetadatas)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<MetadatasResultPacket>
            {
                private int GetPropertyCount(MetadatasResultPacket value)
                {
                    int c = 0;
                    if (value.BroadcastMetadatas.Count != 0) c++;
                    if (value.UnicastMetadatas.Count != 0) c++;
                    if (value.MulticastMetadatas.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, MetadatasResultPacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // BroadcastMetadatas
                    if (value.BroadcastMetadatas.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.BroadcastMetadatas.Count);
                        for (int i = 0; i < value.BroadcastMetadatas.Count; i++)
                        {
                            BroadcastMetadata.Formatter.Serialize(w, value.BroadcastMetadatas[i], rank + 1);
                        }
                    }
                    // UnicastMetadatas
                    if (value.UnicastMetadatas.Count != 0)
                    {
                        w.Write((ulong)1);
                        w.Write((ulong)value.UnicastMetadatas.Count);
                        for (int i = 0; i < value.UnicastMetadatas.Count; i++)
                        {
                            UnicastMetadata.Formatter.Serialize(w, value.UnicastMetadatas[i], rank + 1);
                        }
                    }
                    // MulticastMetadatas
                    if (value.MulticastMetadatas.Count != 0)
                    {
                        w.Write((ulong)2);
                        w.Write((ulong)value.MulticastMetadatas.Count);
                        for (int i = 0; i < value.MulticastMetadatas.Count; i++)
                        {
                            MulticastMetadata.Formatter.Serialize(w, value.MulticastMetadatas[i], rank + 1);
                        }
                    }
                }
                public MetadatasResultPacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    BroadcastMetadata[] p_broadcastMetadatas = Array.Empty<BroadcastMetadata>();
                    UnicastMetadata[] p_unicastMetadatas = Array.Empty<UnicastMetadata>();
                    MulticastMetadata[] p_multicastMetadatas = Array.Empty<MulticastMetadata>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // BroadcastMetadatas
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_broadcastMetadatas, Math.Min(length, MaxBroadcastMetadatasCount));
                                    for (int index = 0; index < p_broadcastMetadatas.Length; index++)
                                    {
                                        p_broadcastMetadatas[index] = BroadcastMetadata.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                            case 1: // UnicastMetadatas
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_unicastMetadatas, Math.Min(length, MaxUnicastMetadatasCount));
                                    for (int index = 0; index < p_unicastMetadatas.Length; index++)
                                    {
                                        p_unicastMetadatas[index] = UnicastMetadata.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                            case 2: // MulticastMetadatas
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_multicastMetadatas, Math.Min(length, MaxMulticastMetadatasCount));
                                    for (int index = 0; index < p_multicastMetadatas.Length; index++)
                                    {
                                        p_multicastMetadatas[index] = MulticastMetadata.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                        }
                    }
                    return new MetadatasResultPacket(p_broadcastMetadatas, p_unicastMetadatas, p_multicastMetadatas);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class MetadatasLookupRequest : MessageBase<MetadatasLookupRequest>
        {
            static MetadatasLookupRequest()
            {
                MetadatasLookupRequest.Formatter = new CustomFormatter();
            }
            public static readonly int MaxMetadatasCount = 1024 * 8;
            [JsonConstructor]
            public MetadatasLookupRequest(IList<Metadata> metadatas)
            {
                if (metadatas == null) throw new ArgumentNullException("metadatas");
                if (metadatas.Count > MaxMetadatasCount) throw new ArgumentOutOfRangeException("metadatas");
                for (int i = 0; i < metadatas.Count; i++)
                {
                    if (metadatas[i] == null) throw new ArgumentNullException("metadatas[i]");
                }
                this.Metadatas = new ReadOnlyCollection<Metadata>(metadatas);
                {
                    int h = 0;
                    for (int i = 0; i < Metadatas.Count; i++)
                    {
                        h ^= this.Metadatas[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<Metadata> Metadatas { get; }
            public override bool Equals(MetadatasLookupRequest target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.Metadatas, target.Metadatas)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<MetadatasLookupRequest>
            {
                private int GetPropertyCount(MetadatasLookupRequest value)
                {
                    int c = 0;
                    if (value.Metadatas.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, MetadatasLookupRequest value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Metadatas
                    if (value.Metadatas.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.Metadatas.Count);
                        for (int i = 0; i < value.Metadatas.Count; i++)
                        {
                            Metadata.Formatter.Serialize(w, value.Metadatas[i], rank + 1);
                        }
                    }
                }
                public MetadatasLookupRequest Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    Metadata[] p_metadatas = Array.Empty<Metadata>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Metadatas
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_metadatas, Math.Min(length, MaxMetadatasCount));
                                    for (int index = 0; index < p_metadatas.Length; index++)
                                    {
                                        p_metadatas[index] = Metadata.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                        }
                    }
                    return new MetadatasLookupRequest(p_metadatas);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class MetadatasLookupResult : MessageBase<MetadatasLookupResult>
        {
            static MetadatasLookupResult()
            {
                MetadatasLookupResult.Formatter = new CustomFormatter();
            }
            public static readonly int MaxMetadatasCount = 1024 * 8;
            [JsonConstructor]
            public MetadatasLookupResult(IList<Metadata> metadatas)
            {
                if (metadatas == null) throw new ArgumentNullException("metadatas");
                if (metadatas.Count > MaxMetadatasCount) throw new ArgumentOutOfRangeException("metadatas");
                for (int i = 0; i < metadatas.Count; i++)
                {
                    if (metadatas[i] == null) throw new ArgumentNullException("metadatas[i]");
                }
                this.Metadatas = new ReadOnlyCollection<Metadata>(metadatas);
                {
                    int h = 0;
                    for (int i = 0; i < Metadatas.Count; i++)
                    {
                        h ^= this.Metadatas[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<Metadata> Metadatas { get; }
            public override bool Equals(MetadatasLookupResult target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.Metadatas, target.Metadatas)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<MetadatasLookupResult>
            {
                private int GetPropertyCount(MetadatasLookupResult value)
                {
                    int c = 0;
                    if (value.Metadatas.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, MetadatasLookupResult value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Metadatas
                    if (value.Metadatas.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.Metadatas.Count);
                        for (int i = 0; i < value.Metadatas.Count; i++)
                        {
                            Metadata.Formatter.Serialize(w, value.Metadatas[i], rank + 1);
                        }
                    }
                }
                public MetadatasLookupResult Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    Metadata[] p_metadatas = Array.Empty<Metadata>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Metadatas
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_metadatas, Math.Min(length, MaxMetadatasCount));
                                    for (int index = 0; index < p_metadatas.Length; index++)
                                    {
                                        p_metadatas[index] = Metadata.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                        }
                    }
                    return new MetadatasLookupResult(p_metadatas);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class BlocksRequestPacket : MessageBase<BlocksRequestPacket>
        {
            static BlocksRequestPacket()
            {
                BlocksRequestPacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxHashesCount = 1024 * 8;
            [JsonConstructor]
            public BlocksRequestPacket(IList<Hash> hashes)
            {
                if (hashes == null) throw new ArgumentNullException("hashes");
                if (hashes.Count > MaxHashesCount) throw new ArgumentOutOfRangeException("hashes");
                for (int i = 0; i < hashes.Count; i++)
                {
                    if (hashes[i] == null) throw new ArgumentNullException("hashes[i]");
                }
                this.Hashes = new ReadOnlyCollection<Hash>(hashes);
                {
                    int h = 0;
                    for (int i = 0; i < Hashes.Count; i++)
                    {
                        h ^= this.Hashes[i].GetHashCode();
                    }
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public IReadOnlyList<Hash> Hashes { get; }
            public override bool Equals(BlocksRequestPacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (!CollectionUtils.Equals(this.Hashes, target.Hashes)) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<BlocksRequestPacket>
            {
                private int GetPropertyCount(BlocksRequestPacket value)
                {
                    int c = 0;
                    if (value.Hashes.Count != 0) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, BlocksRequestPacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Hashes
                    if (value.Hashes.Count != 0)
                    {
                        w.Write((ulong)0);
                        w.Write((ulong)value.Hashes.Count);
                        for (int i = 0; i < value.Hashes.Count; i++)
                        {
                            Hash.Formatter.Serialize(w, value.Hashes[i], rank + 1);
                        }
                    }
                }
                public BlocksRequestPacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    Hash[] p_hashes = Array.Empty<Hash>();
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Hashes
                                {
                                    var length = (int)r.GetUInt64();
                                    Array.Resize(ref p_hashes, Math.Min(length, MaxHashesCount));
                                    for (int index = 0; index < p_hashes.Length; index++)
                                    {
                                        p_hashes[index] = Hash.Formatter.Deserialize(r, rank + 1);
                                    }
                                    break;
                                }
                        }
                    }
                    return new BlocksRequestPacket(p_hashes);
                }
            }
        }
        [JsonObject(MemberSerialization = MemberSerialization.OptIn)]
        internal sealed partial class BlockResultPacket : MessageBase<BlockResultPacket>
        {
            static BlockResultPacket()
            {
                BlockResultPacket.Formatter = new CustomFormatter();
            }
            public static readonly int MaxValueLength = 1024 * 1024 * 4;
            [JsonConstructor]
            public BlockResultPacket(Hash hash, ArraySegment<byte> value)
            {
                if (hash == null) throw new ArgumentNullException("hash");
                if (value == null) throw new ArgumentNullException("value");
                if (value.Count > MaxValueLength) throw new ArgumentOutOfRangeException("value");
                this.Hash = hash;
                this.Value = value;
                {
                    int h = 0;
                    if (this.Hash != default(Hash)) h ^= this.Hash.GetHashCode();
                    if (this.Value != default(ArraySegment<byte>)) h ^= ItemUtils.GetHashCode(this.Value);
                    _hashCode = h;
                }
            }
            [JsonProperty]
            public Hash Hash { get; }
            [JsonProperty]
            public ArraySegment<byte> Value { get; }
            public override bool Equals(BlockResultPacket target)
            {
                if ((object)target == null) return false;
                if (Object.ReferenceEquals(this, target)) return true;
                if (this.Hash != target.Hash) return false;
                if ((this.Value.Array == null) != (target.Value.Array == null)) return false;
                if ((this.Value.Array != null && target.Value.Array != null)
                    && (this.Value.Count != target.Value.Count
                    && !Unsafe.Equals(this.Value.Array, this.Value.Offset, target.Value.Array, target.Value.Offset, this.Value.Count))) return false;
                return true;
            }
            private readonly int _hashCode;
            public override int GetHashCode() => _hashCode;
            private sealed class CustomFormatter : IMessageFormatter<BlockResultPacket>
            {
                private int GetPropertyCount(BlockResultPacket value)
                {
                    int c = 0;
                    if (value.Hash != default(Hash)) c++;
                    if (value.Value != default(ArraySegment<byte>)) c++;
                    return c;
                }
                public void Serialize(MessageStreamWriter w, BlockResultPacket value, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    w.Write((ulong)this.GetPropertyCount(value));
                    // Hash
                    if (value.Hash != default(Hash))
                    {
                        w.Write((ulong)0);
                        Hash.Formatter.Serialize(w, value.Hash, rank + 1);
                    }
                    // Value
                    if (value.Value != default(ArraySegment<byte>))
                    {
                        w.Write((ulong)1);
                        w.Write(value.Value.Array, value.Value.Offset, value.Value.Count);
                    }
                }
                public BlockResultPacket Deserialize(MessageStreamReader r, int rank)
                {
                    if (rank > 256) throw new FormatException();
                    Hash p_hash = default(Hash);
                    ArraySegment<byte> p_value = default(ArraySegment<byte>);
                    for (int count = (int)r.GetUInt64() - 1; count >= 0; count--)
                    {
                        int id = (int)r.GetUInt64();
                        switch (id)
                        {
                            case 0: // Hash
                                {
                                    p_hash = Hash.Formatter.Deserialize(r, rank + 1);
                                    break;
                                }
                            case 1: // Value
                                {
                                    p_value = r.GetRecycleBytesSegment(MaxValueLength);
                                    break;
                                }
                        }
                    }
                    return new BlockResultPacket(p_hash, p_value);
                }
            }
        }
    }
}

