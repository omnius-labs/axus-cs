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

namespace Xeus.Core.Exchange.Internal
{
    internal enum ProtocolVersion : byte
    {
        Version1 = 1,
    }

    internal sealed partial class BroadcastMetadata : RocketPackMessageBase<BroadcastMetadata>
    {
        static BroadcastMetadata()
        {
            BroadcastMetadata.Formatter = new CustomFormatter();
        }

        public static readonly int MaxTypeLength = 256;

        public BroadcastMetadata(string type, Timestamp creationTime, Clue clue, OmniCertificate certificate)
        {
            if (type is null) throw new ArgumentNullException("type");
            if (type.Length > 256) throw new ArgumentOutOfRangeException("type");
            if (clue is null) throw new ArgumentNullException("clue");
            if (certificate is null) throw new ArgumentNullException("certificate");

            this.Type = type;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Certificate = certificate;

            {
                var hashCode = new HashCode();
                if (this.Type != default) hashCode.Add(this.Type.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.Clue != default) hashCode.Add(this.Clue.GetHashCode());
                if (this.Certificate != default) hashCode.Add(this.Certificate.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public string Type { get; }
        public Timestamp CreationTime { get; }
        public Clue Clue { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(BroadcastMetadata target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if (this.Certificate != target.Certificate) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<BroadcastMetadata>
        {
            public void Serialize(RocketPackWriter w, BroadcastMetadata value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Type != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Clue != default) propertyCount++;
                    if (value.Certificate != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Type
                if (value.Type != default)
                {
                    w.Write((ulong)0);
                    w.Write(value.Type);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.CreationTime);
                }
                // Clue
                if (value.Clue != default)
                {
                    w.Write((ulong)2);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                // Certificate
                if (value.Certificate != default)
                {
                    w.Write((ulong)3);
                    OmniCertificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }

            public BroadcastMetadata Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                string p_type = default;
                Timestamp p_creationTime = default;
                Clue p_clue = default;
                OmniCertificate p_certificate = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Type
                            {
                                p_type = r.GetString(256);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 2: // Clue
                            {
                                p_clue = Clue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 3: // Certificate
                            {
                                p_certificate = OmniCertificate.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new BroadcastMetadata(p_type, p_creationTime, p_clue, p_certificate);
            }
        }
    }

    internal sealed partial class UnicastMetadata : RocketPackMessageBase<UnicastMetadata>
    {
        static UnicastMetadata()
        {
            UnicastMetadata.Formatter = new CustomFormatter();
        }

        public static readonly int MaxTypeLength = 256;

        public UnicastMetadata(string type, OmniSignature signature, Timestamp creationTime, Clue clue, OmniCertificate certificate)
        {
            if (type is null) throw new ArgumentNullException("type");
            if (type.Length > 256) throw new ArgumentOutOfRangeException("type");
            if (signature is null) throw new ArgumentNullException("signature");
            if (clue is null) throw new ArgumentNullException("clue");
            if (certificate is null) throw new ArgumentNullException("certificate");

            this.Type = type;
            this.Signature = signature;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Certificate = certificate;

            {
                var hashCode = new HashCode();
                if (this.Type != default) hashCode.Add(this.Type.GetHashCode());
                if (this.Signature != default) hashCode.Add(this.Signature.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.Clue != default) hashCode.Add(this.Clue.GetHashCode());
                if (this.Certificate != default) hashCode.Add(this.Certificate.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public string Type { get; }
        public OmniSignature Signature { get; }
        public Timestamp CreationTime { get; }
        public Clue Clue { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(UnicastMetadata target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Signature != target.Signature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if (this.Certificate != target.Certificate) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<UnicastMetadata>
        {
            public void Serialize(RocketPackWriter w, UnicastMetadata value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Type != default) propertyCount++;
                    if (value.Signature != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Clue != default) propertyCount++;
                    if (value.Certificate != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Type
                if (value.Type != default)
                {
                    w.Write((ulong)0);
                    w.Write(value.Type);
                }
                // Signature
                if (value.Signature != default)
                {
                    w.Write((ulong)1);
                    OmniSignature.Formatter.Serialize(w, value.Signature, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Clue
                if (value.Clue != default)
                {
                    w.Write((ulong)3);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                // Certificate
                if (value.Certificate != default)
                {
                    w.Write((ulong)4);
                    OmniCertificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }

            public UnicastMetadata Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                string p_type = default;
                OmniSignature p_signature = default;
                Timestamp p_creationTime = default;
                Clue p_clue = default;
                OmniCertificate p_certificate = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Type
                            {
                                p_type = r.GetString(256);
                                break;
                            }
                        case 1: // Signature
                            {
                                p_signature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3: // Clue
                            {
                                p_clue = Clue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 4: // Certificate
                            {
                                p_certificate = OmniCertificate.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new UnicastMetadata(p_type, p_signature, p_creationTime, p_clue, p_certificate);
            }
        }
    }

    internal sealed partial class MulticastMetadata : RocketPackMessageBase<MulticastMetadata>
    {
        static MulticastMetadata()
        {
            MulticastMetadata.Formatter = new CustomFormatter();
        }

        public static readonly int MaxTypeLength = 256;

        public MulticastMetadata(string type, Channel channel, Timestamp creationTime, Clue clue, OmniHashcash hashcash, OmniCertificate certificate)
        {
            if (type is null) throw new ArgumentNullException("type");
            if (type.Length > 256) throw new ArgumentOutOfRangeException("type");
            if (channel is null) throw new ArgumentNullException("channel");
            if (clue is null) throw new ArgumentNullException("clue");
            if (hashcash is null) throw new ArgumentNullException("hashcash");
            if (certificate is null) throw new ArgumentNullException("certificate");

            this.Type = type;
            this.Channel = channel;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Hashcash = hashcash;
            this.Certificate = certificate;

            {
                var hashCode = new HashCode();
                if (this.Type != default) hashCode.Add(this.Type.GetHashCode());
                if (this.Channel != default) hashCode.Add(this.Channel.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.Clue != default) hashCode.Add(this.Clue.GetHashCode());
                if (this.Hashcash != default) hashCode.Add(this.Hashcash.GetHashCode());
                if (this.Certificate != default) hashCode.Add(this.Certificate.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public string Type { get; }
        public Channel Channel { get; }
        public Timestamp CreationTime { get; }
        public Clue Clue { get; }
        public OmniHashcash Hashcash { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(MulticastMetadata target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Channel != target.Channel) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if (this.Hashcash != target.Hashcash) return false;
            if (this.Certificate != target.Certificate) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<MulticastMetadata>
        {
            public void Serialize(RocketPackWriter w, MulticastMetadata value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Type != default) propertyCount++;
                    if (value.Channel != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Clue != default) propertyCount++;
                    if (value.Hashcash != default) propertyCount++;
                    if (value.Certificate != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Type
                if (value.Type != default)
                {
                    w.Write((ulong)0);
                    w.Write(value.Type);
                }
                // Channel
                if (value.Channel != default)
                {
                    w.Write((ulong)1);
                    Channel.Formatter.Serialize(w, value.Channel, rank + 1);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Clue
                if (value.Clue != default)
                {
                    w.Write((ulong)3);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                // Hashcash
                if (value.Hashcash != default)
                {
                    w.Write((ulong)4);
                    OmniHashcash.Formatter.Serialize(w, value.Hashcash, rank + 1);
                }
                // Certificate
                if (value.Certificate != default)
                {
                    w.Write((ulong)5);
                    OmniCertificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }

            public MulticastMetadata Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                string p_type = default;
                Channel p_channel = default;
                Timestamp p_creationTime = default;
                Clue p_clue = default;
                OmniHashcash p_hashcash = default;
                OmniCertificate p_certificate = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Type
                            {
                                p_type = r.GetString(256);
                                break;
                            }
                        case 1: // Channel
                            {
                                p_channel = Channel.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3: // Clue
                            {
                                p_clue = Clue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 4: // Hashcash
                            {
                                p_hashcash = OmniHashcash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 5: // Certificate
                            {
                                p_certificate = OmniCertificate.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new MulticastMetadata(p_type, p_channel, p_creationTime, p_clue, p_hashcash, p_certificate);
            }
        }
    }

    internal sealed partial class HelloPacket : RocketPackMessageBase<HelloPacket>
    {
        static HelloPacket()
        {
            HelloPacket.Formatter = new CustomFormatter();
        }

        public static readonly int MaxProtocolVersionsCount = 32;

        public HelloPacket(IList<ProtocolVersion> protocolVersions)
        {
            if (protocolVersions is null) throw new ArgumentNullException("protocolVersions");
            if (protocolVersions.Count > 32) throw new ArgumentOutOfRangeException("protocolVersions");

            this.ProtocolVersions = new ReadOnlyCollection<ProtocolVersion>(protocolVersions);

            {
                var hashCode = new HashCode();
                foreach (var n in this.ProtocolVersions)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public IReadOnlyList<ProtocolVersion> ProtocolVersions { get; }

        public override bool Equals(HelloPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if ((this.ProtocolVersions is null) != (target.ProtocolVersions is null)) return false;
            if (!(this.ProtocolVersions is null) && !(target.ProtocolVersions is null) && !CollectionHelper.Equals(this.ProtocolVersions, target.ProtocolVersions)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<HelloPacket>
        {
            public void Serialize(RocketPackWriter w, HelloPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.ProtocolVersions.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // ProtocolVersions
                if (value.ProtocolVersions.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.ProtocolVersions.Count);
                    foreach (var n in value.ProtocolVersions)
                    {
                        w.Write((ulong)n);
                    }
                }
            }

            public HelloPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                IList<ProtocolVersion> p_protocolVersions = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // ProtocolVersions
                            {
                                var length = (int)r.GetUInt64();
                                p_protocolVersions = new ProtocolVersion[length];
                                for (int i = 0; i < p_protocolVersions.Count; i++)
                                {
                                    p_protocolVersions[i] = (ProtocolVersion)r.GetUInt64();
                                }
                                break;
                            }
                    }
                }

                return new HelloPacket(p_protocolVersions);
            }
        }
    }

    internal sealed partial class ProfilePacket : RocketPackMessageBase<ProfilePacket>
    {
        static ProfilePacket()
        {
            ProfilePacket.Formatter = new CustomFormatter();
        }

        public static readonly int MaxIdLength = 32;

        public ProfilePacket(ReadOnlyMemory<byte> id, OmniAddress address)
        {
            if (id.Length > 32) throw new ArgumentOutOfRangeException("id");
            if (address is null) throw new ArgumentNullException("address");

            this.Id = id;
            this.Address = address;

            {
                var hashCode = new HashCode();
                if (!this.Id.IsEmpty) hashCode.Add(ObjectHelper.GetHashCode(this.Id.Span));
                if (this.Address != default) hashCode.Add(this.Address.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public ReadOnlyMemory<byte> Id { get; }
        public OmniAddress Address { get; }

        public override bool Equals(ProfilePacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!BytesOperations.SequenceEqual(this.Id.Span, target.Id.Span)) return false;
            if (this.Address != target.Address) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ProfilePacket>
        {
            public void Serialize(RocketPackWriter w, ProfilePacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (!value.Id.IsEmpty) propertyCount++;
                    if (value.Address != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Id
                if (!value.Id.IsEmpty)
                {
                    w.Write((ulong)0);
                    w.Write(value.Id.Span);
                }
                // Address
                if (value.Address != default)
                {
                    w.Write((ulong)1);
                    OmniAddress.Formatter.Serialize(w, value.Address, rank + 1);
                }
            }

            public ProfilePacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                ReadOnlyMemory<byte> p_id = default;
                OmniAddress p_address = default;

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
                        case 1: // Address
                            {
                                p_address = OmniAddress.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new ProfilePacket(p_id, p_address);
            }
        }
    }

    internal sealed partial class AddressesPublishPacket : RocketPackMessageBase<AddressesPublishPacket>
    {
        static AddressesPublishPacket()
        {
            AddressesPublishPacket.Formatter = new CustomFormatter();
        }

        public static readonly int MaxAddressesCount = 256;

        public AddressesPublishPacket(IList<OmniAddress> addresses)
        {
            if (addresses is null) throw new ArgumentNullException("addresses");
            if (addresses.Count > 256) throw new ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new ArgumentNullException("n");
            }

            this.Addresses = new ReadOnlyCollection<OmniAddress>(addresses);

            {
                var hashCode = new HashCode();
                foreach (var n in this.Addresses)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public IReadOnlyList<OmniAddress> Addresses { get; }

        public override bool Equals(AddressesPublishPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if ((this.Addresses is null) != (target.Addresses is null)) return false;
            if (!(this.Addresses is null) && !(target.Addresses is null) && !CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<AddressesPublishPacket>
        {
            public void Serialize(RocketPackWriter w, AddressesPublishPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Addresses.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Addresses
                if (value.Addresses.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Addresses.Count);
                    foreach (var n in value.Addresses)
                    {
                        OmniAddress.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public AddressesPublishPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                IList<OmniAddress> p_addresses = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Addresses
                            {
                                var length = (int)r.GetUInt64();
                                p_addresses = new OmniAddress[length];
                                for (int i = 0; i < p_addresses.Count; i++)
                                {
                                    p_addresses[i] = OmniAddress.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new AddressesPublishPacket(p_addresses);
            }
        }
    }

    internal sealed partial class AddressesRequestPacket : RocketPackMessageBase<AddressesRequestPacket>
    {
        static AddressesRequestPacket()
        {
            AddressesRequestPacket.Formatter = new CustomFormatter();
        }

        public static readonly int MaxIdsCount = 256;

        public AddressesRequestPacket(IList<ReadOnlyMemory<byte>> ids)
        {
            if (ids is null) throw new ArgumentNullException("ids");
            if (ids.Count > 256) throw new ArgumentOutOfRangeException("ids");
            foreach (var n in ids)
            {
                if (n.Length > 32) throw new ArgumentOutOfRangeException("n");
            }

            this.Ids = new ReadOnlyCollection<ReadOnlyMemory<byte>>(ids);

            {
                var hashCode = new HashCode();
                foreach (var n in this.Ids)
                {
                    if (!n.IsEmpty) hashCode.Add(ObjectHelper.GetHashCode(n.Span));
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public IReadOnlyList<ReadOnlyMemory<byte>> Ids { get; }

        public override bool Equals(AddressesRequestPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if ((this.Ids is null) != (target.Ids is null)) return false;
            if (!(this.Ids is null) && !(target.Ids is null) && !CollectionHelper.Equals(this.Ids, target.Ids)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<AddressesRequestPacket>
        {
            public void Serialize(RocketPackWriter w, AddressesRequestPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Ids.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Ids
                if (value.Ids.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Ids.Count);
                    foreach (var n in value.Ids)
                    {
                        w.Write(n.Span);
                    }
                }
            }

            public AddressesRequestPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                IList<ReadOnlyMemory<byte>> p_ids = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Ids
                            {
                                var length = (int)r.GetUInt64();
                                p_ids = new ReadOnlyMemory<byte>[length];
                                for (int i = 0; i < p_ids.Count; i++)
                                {
                                    p_ids[i] = r.GetMemory(32);
                                }
                                break;
                            }
                    }
                }

                return new AddressesRequestPacket(p_ids);
            }
        }
    }

    internal sealed partial class AddressesResultPacket : RocketPackMessageBase<AddressesResultPacket>
    {
        static AddressesResultPacket()
        {
            AddressesResultPacket.Formatter = new CustomFormatter();
        }

        public static readonly int MaxIdLength = 32;
        public static readonly int MaxAddressesCount = 256;

        public AddressesResultPacket(ReadOnlyMemory<byte> id, IList<OmniAddress> addresses)
        {
            if (id.Length > 32) throw new ArgumentOutOfRangeException("id");
            if (addresses is null) throw new ArgumentNullException("addresses");
            if (addresses.Count > 256) throw new ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new ArgumentNullException("n");
            }

            this.Id = id;
            this.Addresses = new ReadOnlyCollection<OmniAddress>(addresses);

            {
                var hashCode = new HashCode();
                if (!this.Id.IsEmpty) hashCode.Add(ObjectHelper.GetHashCode(this.Id.Span));
                foreach (var n in this.Addresses)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public ReadOnlyMemory<byte> Id { get; }
        public IReadOnlyList<OmniAddress> Addresses { get; }

        public override bool Equals(AddressesResultPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (!BytesOperations.SequenceEqual(this.Id.Span, target.Id.Span)) return false;
            if ((this.Addresses is null) != (target.Addresses is null)) return false;
            if (!(this.Addresses is null) && !(target.Addresses is null) && !CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<AddressesResultPacket>
        {
            public void Serialize(RocketPackWriter w, AddressesResultPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (!value.Id.IsEmpty) propertyCount++;
                    if (value.Addresses.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Id
                if (!value.Id.IsEmpty)
                {
                    w.Write((ulong)0);
                    w.Write(value.Id.Span);
                }
                // Addresses
                if (value.Addresses.Count != 0)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Addresses.Count);
                    foreach (var n in value.Addresses)
                    {
                        OmniAddress.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public AddressesResultPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                ReadOnlyMemory<byte> p_id = default;
                IList<OmniAddress> p_addresses = default;

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
                        case 1: // Addresses
                            {
                                var length = (int)r.GetUInt64();
                                p_addresses = new OmniAddress[length];
                                for (int i = 0; i < p_addresses.Count; i++)
                                {
                                    p_addresses[i] = OmniAddress.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new AddressesResultPacket(p_id, p_addresses);
            }
        }
    }

    internal sealed partial class BroadcastMetadataRequestPacket : RocketPackMessageBase<BroadcastMetadataRequestPacket>
    {
        static BroadcastMetadataRequestPacket()
        {
            BroadcastMetadataRequestPacket.Formatter = new CustomFormatter();
        }

        public BroadcastMetadataRequestPacket(byte hopLimit, OmniSignature signatures)
        {
            if (signatures is null) throw new ArgumentNullException("signatures");

            this.HopLimit = hopLimit;
            this.Signatures = signatures;

            {
                var hashCode = new HashCode();
                if (this.HopLimit != default) hashCode.Add(this.HopLimit.GetHashCode());
                if (this.Signatures != default) hashCode.Add(this.Signatures.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public byte HopLimit { get; }
        public OmniSignature Signatures { get; }

        public override bool Equals(BroadcastMetadataRequestPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Signatures != target.Signatures) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<BroadcastMetadataRequestPacket>
        {
            public void Serialize(RocketPackWriter w, BroadcastMetadataRequestPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.HopLimit != default) propertyCount++;
                    if (value.Signatures != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // HopLimit
                if (value.HopLimit != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.HopLimit);
                }
                // Signatures
                if (value.Signatures != default)
                {
                    w.Write((ulong)1);
                    OmniSignature.Formatter.Serialize(w, value.Signatures, rank + 1);
                }
            }

            public BroadcastMetadataRequestPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                byte p_hopLimit = default;
                OmniSignature p_signatures = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // HopLimit
                            {
                                p_hopLimit = (byte)r.GetUInt64();
                                break;
                            }
                        case 1: // Signatures
                            {
                                p_signatures = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new BroadcastMetadataRequestPacket(p_hopLimit, p_signatures);
            }
        }
    }

    internal sealed partial class UnicastMetadataRequestPacket : RocketPackMessageBase<UnicastMetadataRequestPacket>
    {
        static UnicastMetadataRequestPacket()
        {
            UnicastMetadataRequestPacket.Formatter = new CustomFormatter();
        }

        public UnicastMetadataRequestPacket(byte hopLimit, OmniSignature signatures)
        {
            if (signatures is null) throw new ArgumentNullException("signatures");

            this.HopLimit = hopLimit;
            this.Signatures = signatures;

            {
                var hashCode = new HashCode();
                if (this.HopLimit != default) hashCode.Add(this.HopLimit.GetHashCode());
                if (this.Signatures != default) hashCode.Add(this.Signatures.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public byte HopLimit { get; }
        public OmniSignature Signatures { get; }

        public override bool Equals(UnicastMetadataRequestPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Signatures != target.Signatures) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<UnicastMetadataRequestPacket>
        {
            public void Serialize(RocketPackWriter w, UnicastMetadataRequestPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.HopLimit != default) propertyCount++;
                    if (value.Signatures != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // HopLimit
                if (value.HopLimit != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.HopLimit);
                }
                // Signatures
                if (value.Signatures != default)
                {
                    w.Write((ulong)1);
                    OmniSignature.Formatter.Serialize(w, value.Signatures, rank + 1);
                }
            }

            public UnicastMetadataRequestPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                byte p_hopLimit = default;
                OmniSignature p_signatures = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // HopLimit
                            {
                                p_hopLimit = (byte)r.GetUInt64();
                                break;
                            }
                        case 1: // Signatures
                            {
                                p_signatures = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new UnicastMetadataRequestPacket(p_hopLimit, p_signatures);
            }
        }
    }

    internal sealed partial class MulticastMetadataRequestPacket : RocketPackMessageBase<MulticastMetadataRequestPacket>
    {
        static MulticastMetadataRequestPacket()
        {
            MulticastMetadataRequestPacket.Formatter = new CustomFormatter();
        }

        public MulticastMetadataRequestPacket(byte hopLimit, Channel channel)
        {
            if (channel is null) throw new ArgumentNullException("channel");

            this.HopLimit = hopLimit;
            this.Channel = channel;

            {
                var hashCode = new HashCode();
                if (this.HopLimit != default) hashCode.Add(this.HopLimit.GetHashCode());
                if (this.Channel != default) hashCode.Add(this.Channel.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public byte HopLimit { get; }
        public Channel Channel { get; }

        public override bool Equals(MulticastMetadataRequestPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Channel != target.Channel) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<MulticastMetadataRequestPacket>
        {
            public void Serialize(RocketPackWriter w, MulticastMetadataRequestPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.HopLimit != default) propertyCount++;
                    if (value.Channel != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // HopLimit
                if (value.HopLimit != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.HopLimit);
                }
                // Channel
                if (value.Channel != default)
                {
                    w.Write((ulong)1);
                    Channel.Formatter.Serialize(w, value.Channel, rank + 1);
                }
            }

            public MulticastMetadataRequestPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                byte p_hopLimit = default;
                Channel p_channel = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // HopLimit
                            {
                                p_hopLimit = (byte)r.GetUInt64();
                                break;
                            }
                        case 1: // Channel
                            {
                                p_channel = Channel.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new MulticastMetadataRequestPacket(p_hopLimit, p_channel);
            }
        }
    }

    internal sealed partial class MetadatasResultPacket : RocketPackMessageBase<MetadatasResultPacket>
    {
        static MetadatasResultPacket()
        {
            MetadatasResultPacket.Formatter = new CustomFormatter();
        }

        public static readonly int MaxBroadcastMetadatasCount = 8192;
        public static readonly int MaxUnicastMetadatasCount = 8192;
        public static readonly int MaxMulticastMetadatasCount = 8192;

        public MetadatasResultPacket(IList<BroadcastMetadata> broadcastMetadatas, IList<UnicastMetadata> unicastMetadatas, IList<MulticastMetadata> multicastMetadatas)
        {
            if (broadcastMetadatas is null) throw new ArgumentNullException("broadcastMetadatas");
            if (broadcastMetadatas.Count > 8192) throw new ArgumentOutOfRangeException("broadcastMetadatas");
            foreach (var n in broadcastMetadatas)
            {
                if (n is null) throw new ArgumentNullException("n");
            }
            if (unicastMetadatas is null) throw new ArgumentNullException("unicastMetadatas");
            if (unicastMetadatas.Count > 8192) throw new ArgumentOutOfRangeException("unicastMetadatas");
            foreach (var n in unicastMetadatas)
            {
                if (n is null) throw new ArgumentNullException("n");
            }
            if (multicastMetadatas is null) throw new ArgumentNullException("multicastMetadatas");
            if (multicastMetadatas.Count > 8192) throw new ArgumentOutOfRangeException("multicastMetadatas");
            foreach (var n in multicastMetadatas)
            {
                if (n is null) throw new ArgumentNullException("n");
            }

            this.BroadcastMetadatas = new ReadOnlyCollection<BroadcastMetadata>(broadcastMetadatas);
            this.UnicastMetadatas = new ReadOnlyCollection<UnicastMetadata>(unicastMetadatas);
            this.MulticastMetadatas = new ReadOnlyCollection<MulticastMetadata>(multicastMetadatas);

            {
                var hashCode = new HashCode();
                foreach (var n in this.BroadcastMetadatas)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                foreach (var n in this.UnicastMetadatas)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                foreach (var n in this.MulticastMetadatas)
                {
                    if (n != default) hashCode.Add(n.GetHashCode());
                }
                _hashCode = hashCode.ToHashCode();
            }
        }

        public IReadOnlyList<BroadcastMetadata> BroadcastMetadatas { get; }
        public IReadOnlyList<UnicastMetadata> UnicastMetadatas { get; }
        public IReadOnlyList<MulticastMetadata> MulticastMetadatas { get; }

        public override bool Equals(MetadatasResultPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if ((this.BroadcastMetadatas is null) != (target.BroadcastMetadatas is null)) return false;
            if (!(this.BroadcastMetadatas is null) && !(target.BroadcastMetadatas is null) && !CollectionHelper.Equals(this.BroadcastMetadatas, target.BroadcastMetadatas)) return false;
            if ((this.UnicastMetadatas is null) != (target.UnicastMetadatas is null)) return false;
            if (!(this.UnicastMetadatas is null) && !(target.UnicastMetadatas is null) && !CollectionHelper.Equals(this.UnicastMetadatas, target.UnicastMetadatas)) return false;
            if ((this.MulticastMetadatas is null) != (target.MulticastMetadatas is null)) return false;
            if (!(this.MulticastMetadatas is null) && !(target.MulticastMetadatas is null) && !CollectionHelper.Equals(this.MulticastMetadatas, target.MulticastMetadatas)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<MetadatasResultPacket>
        {
            public void Serialize(RocketPackWriter w, MetadatasResultPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.BroadcastMetadatas.Count != 0) propertyCount++;
                    if (value.UnicastMetadatas.Count != 0) propertyCount++;
                    if (value.MulticastMetadatas.Count != 0) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // BroadcastMetadatas
                if (value.BroadcastMetadatas.Count != 0)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.BroadcastMetadatas.Count);
                    foreach (var n in value.BroadcastMetadatas)
                    {
                        BroadcastMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                // UnicastMetadatas
                if (value.UnicastMetadatas.Count != 0)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.UnicastMetadatas.Count);
                    foreach (var n in value.UnicastMetadatas)
                    {
                        UnicastMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                // MulticastMetadatas
                if (value.MulticastMetadatas.Count != 0)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.MulticastMetadatas.Count);
                    foreach (var n in value.MulticastMetadatas)
                    {
                        MulticastMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public MetadatasResultPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                IList<BroadcastMetadata> p_broadcastMetadatas = default;
                IList<UnicastMetadata> p_unicastMetadatas = default;
                IList<MulticastMetadata> p_multicastMetadatas = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // BroadcastMetadatas
                            {
                                var length = (int)r.GetUInt64();
                                p_broadcastMetadatas = new BroadcastMetadata[length];
                                for (int i = 0; i < p_broadcastMetadatas.Count; i++)
                                {
                                    p_broadcastMetadatas[i] = BroadcastMetadata.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 1: // UnicastMetadatas
                            {
                                var length = (int)r.GetUInt64();
                                p_unicastMetadatas = new UnicastMetadata[length];
                                for (int i = 0; i < p_unicastMetadatas.Count; i++)
                                {
                                    p_unicastMetadatas[i] = UnicastMetadata.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 2: // MulticastMetadatas
                            {
                                var length = (int)r.GetUInt64();
                                p_multicastMetadatas = new MulticastMetadata[length];
                                for (int i = 0; i < p_multicastMetadatas.Count; i++)
                                {
                                    p_multicastMetadatas[i] = MulticastMetadata.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new MetadatasResultPacket(p_broadcastMetadatas, p_unicastMetadatas, p_multicastMetadatas);
            }
        }
    }

    internal sealed partial class BlockPublishPacket : RocketPackMessageBase<BlockPublishPacket>, IDisposable
    {
        static BlockPublishPacket()
        {
            BlockPublishPacket.Formatter = new CustomFormatter();
        }

        public static readonly int MaxValueLength = 4194304;

        public BlockPublishPacket(byte hopLimit, byte priority, OmniHash hash, IMemoryOwner<byte> value)
        {
            if (value is null) throw new ArgumentNullException("value");
            if (value.Memory.Length > 4194304) throw new ArgumentOutOfRangeException("value");

            this.HopLimit = hopLimit;
            this.Priority = priority;
            this.Hash = hash;
            _value = value;

            {
                var hashCode = new HashCode();
                if (this.HopLimit != default) hashCode.Add(this.HopLimit.GetHashCode());
                if (this.Priority != default) hashCode.Add(this.Priority.GetHashCode());
                if (this.Hash != default) hashCode.Add(this.Hash.GetHashCode());
                if (!this.Value.IsEmpty) hashCode.Add(ObjectHelper.GetHashCode(this.Value.Span));
                _hashCode = hashCode.ToHashCode();
            }
        }

        public byte HopLimit { get; }
        public byte Priority { get; }
        public OmniHash Hash { get; }
        private readonly IMemoryOwner<byte> _value;
        public ReadOnlyMemory<byte> Value => _value.Memory;

        public override bool Equals(BlockPublishPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Priority != target.Priority) return false;
            if (this.Hash != target.Hash) return false;
            if (!BytesOperations.SequenceEqual(this.Value.Span, target.Value.Span)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class CustomFormatter : IRocketPackFormatter<BlockPublishPacket>
        {
            public void Serialize(RocketPackWriter w, BlockPublishPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.HopLimit != default) propertyCount++;
                    if (value.Priority != default) propertyCount++;
                    if (value.Hash != default) propertyCount++;
                    if (!value.Value.IsEmpty) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // HopLimit
                if (value.HopLimit != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.HopLimit);
                }
                // Priority
                if (value.Priority != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Priority);
                }
                // Hash
                if (value.Hash != default)
                {
                    w.Write((ulong)2);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
                // Value
                if (!value.Value.IsEmpty)
                {
                    w.Write((ulong)3);
                    w.Write(value.Value.Span);
                }
            }

            public BlockPublishPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                byte p_hopLimit = default;
                byte p_priority = default;
                OmniHash p_hash = default;
                IMemoryOwner<byte> p_value = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // HopLimit
                            {
                                p_hopLimit = (byte)r.GetUInt64();
                                break;
                            }
                        case 1: // Priority
                            {
                                p_priority = (byte)r.GetUInt64();
                                break;
                            }
                        case 2: // Hash
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 3: // Value
                            {
                                p_value = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new BlockPublishPacket(p_hopLimit, p_priority, p_hash, p_value);
            }
        }
    }

    internal sealed partial class BlockRequestPacket : RocketPackMessageBase<BlockRequestPacket>
    {
        static BlockRequestPacket()
        {
            BlockRequestPacket.Formatter = new CustomFormatter();
        }

        public BlockRequestPacket(byte hopLimit, byte priority, OmniHash hash)
        {
            this.HopLimit = hopLimit;
            this.Priority = priority;
            this.Hash = hash;

            {
                var hashCode = new HashCode();
                if (this.HopLimit != default) hashCode.Add(this.HopLimit.GetHashCode());
                if (this.Priority != default) hashCode.Add(this.Priority.GetHashCode());
                if (this.Hash != default) hashCode.Add(this.Hash.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public byte HopLimit { get; }
        public byte Priority { get; }
        public OmniHash Hash { get; }

        public override bool Equals(BlockRequestPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Priority != target.Priority) return false;
            if (this.Hash != target.Hash) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<BlockRequestPacket>
        {
            public void Serialize(RocketPackWriter w, BlockRequestPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.HopLimit != default) propertyCount++;
                    if (value.Priority != default) propertyCount++;
                    if (value.Hash != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // HopLimit
                if (value.HopLimit != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.HopLimit);
                }
                // Priority
                if (value.Priority != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Priority);
                }
                // Hash
                if (value.Hash != default)
                {
                    w.Write((ulong)2);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
            }

            public BlockRequestPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                byte p_hopLimit = default;
                byte p_priority = default;
                OmniHash p_hash = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // HopLimit
                            {
                                p_hopLimit = (byte)r.GetUInt64();
                                break;
                            }
                        case 1: // Priority
                            {
                                p_priority = (byte)r.GetUInt64();
                                break;
                            }
                        case 2: // Hash
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new BlockRequestPacket(p_hopLimit, p_priority, p_hash);
            }
        }
    }

    internal sealed partial class BlockLinkPacket : RocketPackMessageBase<BlockLinkPacket>
    {
        static BlockLinkPacket()
        {
            BlockLinkPacket.Formatter = new CustomFormatter();
        }

        public BlockLinkPacket(byte hopLimit, byte priority, OmniHash hash)
        {
            this.HopLimit = hopLimit;
            this.Priority = priority;
            this.Hash = hash;

            {
                var hashCode = new HashCode();
                if (this.HopLimit != default) hashCode.Add(this.HopLimit.GetHashCode());
                if (this.Priority != default) hashCode.Add(this.Priority.GetHashCode());
                if (this.Hash != default) hashCode.Add(this.Hash.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public byte HopLimit { get; }
        public byte Priority { get; }
        public OmniHash Hash { get; }

        public override bool Equals(BlockLinkPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Priority != target.Priority) return false;
            if (this.Hash != target.Hash) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<BlockLinkPacket>
        {
            public void Serialize(RocketPackWriter w, BlockLinkPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.HopLimit != default) propertyCount++;
                    if (value.Priority != default) propertyCount++;
                    if (value.Hash != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // HopLimit
                if (value.HopLimit != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.HopLimit);
                }
                // Priority
                if (value.Priority != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Priority);
                }
                // Hash
                if (value.Hash != default)
                {
                    w.Write((ulong)2);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
            }

            public BlockLinkPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                byte p_hopLimit = default;
                byte p_priority = default;
                OmniHash p_hash = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // HopLimit
                            {
                                p_hopLimit = (byte)r.GetUInt64();
                                break;
                            }
                        case 1: // Priority
                            {
                                p_priority = (byte)r.GetUInt64();
                                break;
                            }
                        case 2: // Hash
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new BlockLinkPacket(p_hopLimit, p_priority, p_hash);
            }
        }
    }

    internal sealed partial class BlockResultPacket : RocketPackMessageBase<BlockResultPacket>, IDisposable
    {
        static BlockResultPacket()
        {
            BlockResultPacket.Formatter = new CustomFormatter();
        }

        public static readonly int MaxValueLength = 4194304;

        public BlockResultPacket(OmniHash hash, IMemoryOwner<byte> value)
        {
            if (value is null) throw new ArgumentNullException("value");
            if (value.Memory.Length > 4194304) throw new ArgumentOutOfRangeException("value");

            this.Hash = hash;
            _value = value;

            {
                var hashCode = new HashCode();
                if (this.Hash != default) hashCode.Add(this.Hash.GetHashCode());
                if (!this.Value.IsEmpty) hashCode.Add(ObjectHelper.GetHashCode(this.Value.Span));
                _hashCode = hashCode.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        private readonly IMemoryOwner<byte> _value;
        public ReadOnlyMemory<byte> Value => _value.Memory;

        public override bool Equals(BlockResultPacket target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (!BytesOperations.SequenceEqual(this.Value.Span, target.Value.Span)) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class CustomFormatter : IRocketPackFormatter<BlockResultPacket>
        {
            public void Serialize(RocketPackWriter w, BlockResultPacket value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Hash != default) propertyCount++;
                    if (!value.Value.IsEmpty) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Hash
                if (value.Hash != default)
                {
                    w.Write((ulong)0);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
                // Value
                if (!value.Value.IsEmpty)
                {
                    w.Write((ulong)1);
                    w.Write(value.Value.Span);
                }
            }

            public BlockResultPacket Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                OmniHash p_hash = default;
                IMemoryOwner<byte> p_value = default;

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
                        case 1: // Value
                            {
                                p_value = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new BlockResultPacket(p_hash, p_value);
            }
        }
    }

    internal sealed partial class UploadBlockInfo : RocketPackMessageBase<UploadBlockInfo>
    {
        static UploadBlockInfo()
        {
            UploadBlockInfo.Formatter = new CustomFormatter();
        }

        public UploadBlockInfo(Timestamp creationTime, uint hopLimit, byte priority, OmniHash hash)
        {
            this.CreationTime = creationTime;
            this.HopLimit = hopLimit;
            this.Priority = priority;
            this.Hash = hash;

            {
                var hashCode = new HashCode();
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.HopLimit != default) hashCode.Add(this.HopLimit.GetHashCode());
                if (this.Priority != default) hashCode.Add(this.Priority.GetHashCode());
                if (this.Hash != default) hashCode.Add(this.Hash.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public Timestamp CreationTime { get; }
        public uint HopLimit { get; }
        public byte Priority { get; }
        public OmniHash Hash { get; }

        public override bool Equals(UploadBlockInfo target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Priority != target.Priority) return false;
            if (this.Hash != target.Hash) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<UploadBlockInfo>
        {
            public void Serialize(RocketPackWriter w, UploadBlockInfo value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.HopLimit != default) propertyCount++;
                    if (value.Priority != default) propertyCount++;
                    if (value.Hash != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)0);
                    w.Write(value.CreationTime);
                }
                // HopLimit
                if (value.HopLimit != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.HopLimit);
                }
                // Priority
                if (value.Priority != default)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.Priority);
                }
                // Hash
                if (value.Hash != default)
                {
                    w.Write((ulong)3);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
            }

            public UploadBlockInfo Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                Timestamp p_creationTime = default;
                uint p_hopLimit = default;
                byte p_priority = default;
                OmniHash p_hash = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 1: // HopLimit
                            {
                                p_hopLimit = (uint)r.GetUInt64();
                                break;
                            }
                        case 2: // Priority
                            {
                                p_priority = (byte)r.GetUInt64();
                                break;
                            }
                        case 3: // Hash
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new UploadBlockInfo(p_creationTime, p_hopLimit, p_priority, p_hash);
            }
        }
    }

    internal sealed partial class ExchangeManagerConfig : RocketPackMessageBase<ExchangeManagerConfig>
    {
        static ExchangeManagerConfig()
        {
            ExchangeManagerConfig.Formatter = new CustomFormatter();
        }

        public ExchangeManagerConfig(uint version)
        {
            this.Version = version;

            {
                var hashCode = new HashCode();
                if (this.Version != default) hashCode.Add(this.Version.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public uint Version { get; }

        public override bool Equals(ExchangeManagerConfig target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ExchangeManagerConfig>
        {
            public void Serialize(RocketPackWriter w, ExchangeManagerConfig value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Version != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Version
                if (value.Version != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Version);
                }
            }

            public ExchangeManagerConfig Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                uint p_version = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Version
                            {
                                p_version = (uint)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new ExchangeManagerConfig(p_version);
            }
        }
    }

}
