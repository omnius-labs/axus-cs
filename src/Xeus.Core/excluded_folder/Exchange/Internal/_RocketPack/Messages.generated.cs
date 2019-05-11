using Omnix.Cryptography;
using Omnix.Network;
using Xeus.Messages;
using Xeus.Messages.Options;
using Xeus.Messages.Reports;

#nullable enable

namespace Xeus.Core.Exchange.Internal
{
    internal enum ProtocolVersion : byte
    {
        Version1 = 1,
    }

    internal sealed partial class BroadcastClue : Omnix.Serialization.RocketPack.RocketPackMessageBase<BroadcastClue>
    {
        static BroadcastClue()
        {
            BroadcastClue.Formatter = new CustomFormatter();
            BroadcastClue.Empty = new BroadcastClue(string.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, Clue.Empty, OmniCertificate.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxTypeLength = 256;

        public BroadcastClue(string type, Omnix.Serialization.RocketPack.Timestamp creationTime, Clue clue, OmniCertificate certificate)
        {
            if (type is null) throw new System.ArgumentNullException("type");
            if (type.Length > 256) throw new System.ArgumentOutOfRangeException("type");
            if (clue is null) throw new System.ArgumentNullException("clue");
            if (certificate is null) throw new System.ArgumentNullException("certificate");

            this.Type = type;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Certificate = certificate;

            {
                var __h = new System.HashCode();
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Certificate != default) __h.Add(this.Certificate.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string Type { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public Clue Clue { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(BroadcastClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if (this.Certificate != target.Certificate) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastClue>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, BroadcastClue value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Clue != Clue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Certificate != OmniCertificate.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Type != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Type);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)1);
                    w.Write(value.CreationTime);
                }
                if (value.Clue != Clue.Empty)
                {
                    w.Write((uint)2);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Certificate != OmniCertificate.Empty)
                {
                    w.Write((uint)3);
                    OmniCertificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }

            public BroadcastClue Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                Clue p_clue = Clue.Empty;
                OmniCertificate p_certificate = OmniCertificate.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
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

                return new BroadcastClue(p_type, p_creationTime, p_clue, p_certificate);
            }
        }
    }

    internal sealed partial class UnicastClue : Omnix.Serialization.RocketPack.RocketPackMessageBase<UnicastClue>
    {
        static UnicastClue()
        {
            UnicastClue.Formatter = new CustomFormatter();
            UnicastClue.Empty = new UnicastClue(string.Empty, OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, Clue.Empty, OmniCertificate.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxTypeLength = 256;

        public UnicastClue(string type, OmniSignature signature, Omnix.Serialization.RocketPack.Timestamp creationTime, Clue clue, OmniCertificate certificate)
        {
            if (type is null) throw new System.ArgumentNullException("type");
            if (type.Length > 256) throw new System.ArgumentOutOfRangeException("type");
            if (signature is null) throw new System.ArgumentNullException("signature");
            if (clue is null) throw new System.ArgumentNullException("clue");
            if (certificate is null) throw new System.ArgumentNullException("certificate");

            this.Type = type;
            this.Signature = signature;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Certificate = certificate;

            {
                var __h = new System.HashCode();
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                if (this.Signature != default) __h.Add(this.Signature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Certificate != default) __h.Add(this.Certificate.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string Type { get; }
        public OmniSignature Signature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public Clue Clue { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(UnicastClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Signature != target.Signature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if (this.Certificate != target.Certificate) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastClue>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, UnicastClue value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Signature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Clue != Clue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Certificate != OmniCertificate.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Type != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Type);
                }
                if (value.Signature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    OmniSignature.Formatter.Serialize(w, value.Signature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Clue != Clue.Empty)
                {
                    w.Write((uint)3);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Certificate != OmniCertificate.Empty)
                {
                    w.Write((uint)4);
                    OmniCertificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }

            public UnicastClue Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                OmniSignature p_signature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                Clue p_clue = Clue.Empty;
                OmniCertificate p_certificate = OmniCertificate.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
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

                return new UnicastClue(p_type, p_signature, p_creationTime, p_clue, p_certificate);
            }
        }
    }

    internal sealed partial class MulticastClue : Omnix.Serialization.RocketPack.RocketPackMessageBase<MulticastClue>
    {
        static MulticastClue()
        {
            MulticastClue.Formatter = new CustomFormatter();
            MulticastClue.Empty = new MulticastClue(string.Empty, Channel.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, Clue.Empty, OmniHashcash.Empty, OmniCertificate.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxTypeLength = 256;

        public MulticastClue(string type, Channel channel, Omnix.Serialization.RocketPack.Timestamp creationTime, Clue clue, OmniHashcash hashcash, OmniCertificate certificate)
        {
            if (type is null) throw new System.ArgumentNullException("type");
            if (type.Length > 256) throw new System.ArgumentOutOfRangeException("type");
            if (channel is null) throw new System.ArgumentNullException("channel");
            if (clue is null) throw new System.ArgumentNullException("clue");
            if (hashcash is null) throw new System.ArgumentNullException("hashcash");
            if (certificate is null) throw new System.ArgumentNullException("certificate");

            this.Type = type;
            this.Channel = channel;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Hashcash = hashcash;
            this.Certificate = certificate;

            {
                var __h = new System.HashCode();
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                if (this.Channel != default) __h.Add(this.Channel.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Hashcash != default) __h.Add(this.Hashcash.GetHashCode());
                if (this.Certificate != default) __h.Add(this.Certificate.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string Type { get; }
        public Channel Channel { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public Clue Clue { get; }
        public OmniHashcash Hashcash { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(MulticastClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Channel != target.Channel) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if (this.Hashcash != target.Hashcash) return false;
            if (this.Certificate != target.Certificate) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastClue>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, MulticastClue value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Channel != Channel.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Clue != Clue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Hashcash != OmniHashcash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Certificate != OmniCertificate.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Type != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Type);
                }
                if (value.Channel != Channel.Empty)
                {
                    w.Write((uint)1);
                    Channel.Formatter.Serialize(w, value.Channel, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Clue != Clue.Empty)
                {
                    w.Write((uint)3);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Hashcash != OmniHashcash.Empty)
                {
                    w.Write((uint)4);
                    OmniHashcash.Formatter.Serialize(w, value.Hashcash, rank + 1);
                }
                if (value.Certificate != OmniCertificate.Empty)
                {
                    w.Write((uint)5);
                    OmniCertificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }

            public MulticastClue Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                Channel p_channel = Channel.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                Clue p_clue = Clue.Empty;
                OmniHashcash p_hashcash = OmniHashcash.Empty;
                OmniCertificate p_certificate = OmniCertificate.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
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

                return new MulticastClue(p_type, p_channel, p_creationTime, p_clue, p_hashcash, p_certificate);
            }
        }
    }

    internal sealed partial class HelloMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<HelloMessage>
    {
        static HelloMessage()
        {
            HelloMessage.Formatter = new CustomFormatter();
            HelloMessage.Empty = new HelloMessage(System.Array.Empty<ProtocolVersion>());
        }

        private readonly int __hashCode;

        public static readonly int MaxProtocolVersionsCount = 32;

        public HelloMessage(ProtocolVersion[] protocolVersions)
        {
            if (protocolVersions is null) throw new System.ArgumentNullException("protocolVersions");
            if (protocolVersions.Length > 32) throw new System.ArgumentOutOfRangeException("protocolVersions");

            this.ProtocolVersions = new Omnix.Collections.ReadOnlyListSlim<ProtocolVersion>(protocolVersions);

            {
                var __h = new System.HashCode();
                foreach (var n in this.ProtocolVersions)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyListSlim<ProtocolVersion> ProtocolVersions { get; }

        public override bool Equals(HelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.ProtocolVersions, target.ProtocolVersions)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<HelloMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, HelloMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ProtocolVersions.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ProtocolVersions.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.ProtocolVersions.Count);
                    foreach (var n in value.ProtocolVersions)
                    {
                        w.Write((ulong)n);
                    }
                }
            }

            public HelloMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                ProtocolVersion[] p_protocolVersions = System.Array.Empty<ProtocolVersion>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // ProtocolVersions
                            {
                                var length = r.GetUInt32();
                                p_protocolVersions = new ProtocolVersion[length];
                                for (int i = 0; i < p_protocolVersions.Length; i++)
                                {
                                    p_protocolVersions[i] = (ProtocolVersion)r.GetUInt64();
                                }
                                break;
                            }
                    }
                }

                return new HelloMessage(p_protocolVersions);
            }
        }
    }

    internal sealed partial class ProfileMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<ProfileMessage>
    {
        static ProfileMessage()
        {
            ProfileMessage.Formatter = new CustomFormatter();
            ProfileMessage.Empty = new ProfileMessage(System.ReadOnlyMemory<byte>.Empty, OmniAddress.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxIdLength = 32;

        public ProfileMessage(System.ReadOnlyMemory<byte> id, OmniAddress address)
        {
            if (id.Length > 32) throw new System.ArgumentOutOfRangeException("id");
            if (address is null) throw new System.ArgumentNullException("address");

            this.Id = id;
            this.Address = address;

            {
                var __h = new System.HashCode();
                if (!this.Id.IsEmpty) __h.Add(Omnix.Base.Helpers.ObjectHelper.GetHashCode(this.Id.Span));
                if (this.Address != default) __h.Add(this.Address.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public System.ReadOnlyMemory<byte> Id { get; }
        public OmniAddress Address { get; }

        public override bool Equals(ProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.BytesOperations.SequenceEqual(this.Id.Span, target.Id.Span)) return false;
            if (this.Address != target.Address) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ProfileMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ProfileMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (!value.Id.IsEmpty)
                    {
                        propertyCount++;
                    }
                    if (value.Address != OmniAddress.Empty)
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
                if (value.Address != OmniAddress.Empty)
                {
                    w.Write((uint)1);
                    OmniAddress.Formatter.Serialize(w, value.Address, rank + 1);
                }
            }

            public ProfileMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                System.ReadOnlyMemory<byte> p_id = System.ReadOnlyMemory<byte>.Empty;
                OmniAddress p_address = OmniAddress.Empty;

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
                        case 1: // Address
                            {
                                p_address = OmniAddress.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new ProfileMessage(p_id, p_address);
            }
        }
    }

    internal sealed partial class NodeAddressesMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<NodeAddressesMessage>
    {
        static NodeAddressesMessage()
        {
            NodeAddressesMessage.Formatter = new CustomFormatter();
            NodeAddressesMessage.Empty = new NodeAddressesMessage(System.Array.Empty<OmniAddress>());
        }

        private readonly int __hashCode;

        public static readonly int MaxAddressesCount = 256;

        public NodeAddressesMessage(OmniAddress[] addresses)
        {
            if (addresses is null) throw new System.ArgumentNullException("addresses");
            if (addresses.Length > 256) throw new System.ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.Addresses = new Omnix.Collections.ReadOnlyListSlim<OmniAddress>(addresses);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Addresses)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyListSlim<OmniAddress> Addresses { get; }

        public override bool Equals(NodeAddressesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<NodeAddressesMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, NodeAddressesMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Addresses.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Addresses.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Addresses.Count);
                    foreach (var n in value.Addresses)
                    {
                        OmniAddress.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public NodeAddressesMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniAddress[] p_addresses = System.Array.Empty<OmniAddress>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Addresses
                            {
                                var length = r.GetUInt32();
                                p_addresses = new OmniAddress[length];
                                for (int i = 0; i < p_addresses.Length; i++)
                                {
                                    p_addresses[i] = OmniAddress.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new NodeAddressesMessage(p_addresses);
            }
        }
    }

    internal sealed partial class RelayOption : Omnix.Serialization.RocketPack.RocketPackMessageBase<RelayOption>
    {
        static RelayOption()
        {
            RelayOption.Formatter = new CustomFormatter();
            RelayOption.Empty = new RelayOption(0, 0);
        }

        private readonly int __hashCode;

        public RelayOption(byte hopLimit, byte priority)
        {
            this.HopLimit = hopLimit;
            this.Priority = priority;

            {
                var __h = new System.HashCode();
                if (this.HopLimit != default) __h.Add(this.HopLimit.GetHashCode());
                if (this.Priority != default) __h.Add(this.Priority.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public byte HopLimit { get; }
        public byte Priority { get; }

        public override bool Equals(RelayOption? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Priority != target.Priority) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<RelayOption>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, RelayOption value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.HopLimit != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Priority != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.HopLimit != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.HopLimit);
                }
                if (value.Priority != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Priority);
                }
            }

            public RelayOption Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                byte p_hopLimit = 0;
                byte p_priority = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // HopLimit
                            {
                                p_hopLimit = r.GetUInt8();
                                break;
                            }
                        case 1: // Priority
                            {
                                p_priority = r.GetUInt8();
                                break;
                            }
                    }
                }

                return new RelayOption(p_hopLimit, p_priority);
            }
        }
    }

    internal sealed partial class WantBroadcastCluesMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<WantBroadcastCluesMessage>
    {
        static WantBroadcastCluesMessage()
        {
            WantBroadcastCluesMessage.Formatter = new CustomFormatter();
            WantBroadcastCluesMessage.Empty = new WantBroadcastCluesMessage(new System.Collections.Generic.Dictionary<OmniSignature, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantBroadcastCluesMessage(System.Collections.Generic.Dictionary<OmniSignature, RelayOption> parameters)
        {
            if (parameters is null) throw new System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Key is null) throw new System.ArgumentNullException("n.Key");
                if (n.Value is null) throw new System.ArgumentNullException("n.Value");
            }

            this.Parameters = new Omnix.Collections.ReadOnlyDictionarySlim<OmniSignature, RelayOption>(parameters);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyDictionarySlim<OmniSignature, RelayOption> Parameters { get; }

        public override bool Equals(WantBroadcastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBroadcastCluesMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, WantBroadcastCluesMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Parameters.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Parameters.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Parameters.Count);
                    foreach (var n in value.Parameters)
                    {
                        OmniSignature.Formatter.Serialize(w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(w, n.Value, rank + 1);
                    }
                }
            }

            public WantBroadcastCluesMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                System.Collections.Generic.Dictionary<OmniSignature, RelayOption> p_parameters = new System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Parameters
                            {
                                var length = r.GetUInt32();
                                p_parameters = new System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();
                                OmniSignature t_key = OmniSignature.Empty;
                                RelayOption t_value = RelayOption.Empty;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(r, rank + 1);
                                    p_parameters[t_key] = t_value;
                                }
                                break;
                            }
                    }
                }

                return new WantBroadcastCluesMessage(p_parameters);
            }
        }
    }

    internal sealed partial class WantUnicastCluesMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<WantUnicastCluesMessage>
    {
        static WantUnicastCluesMessage()
        {
            WantUnicastCluesMessage.Formatter = new CustomFormatter();
            WantUnicastCluesMessage.Empty = new WantUnicastCluesMessage(new System.Collections.Generic.Dictionary<OmniSignature, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantUnicastCluesMessage(System.Collections.Generic.Dictionary<OmniSignature, RelayOption> parameters)
        {
            if (parameters is null) throw new System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Key is null) throw new System.ArgumentNullException("n.Key");
                if (n.Value is null) throw new System.ArgumentNullException("n.Value");
            }

            this.Parameters = new Omnix.Collections.ReadOnlyDictionarySlim<OmniSignature, RelayOption>(parameters);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyDictionarySlim<OmniSignature, RelayOption> Parameters { get; }

        public override bool Equals(WantUnicastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<WantUnicastCluesMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, WantUnicastCluesMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Parameters.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Parameters.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Parameters.Count);
                    foreach (var n in value.Parameters)
                    {
                        OmniSignature.Formatter.Serialize(w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(w, n.Value, rank + 1);
                    }
                }
            }

            public WantUnicastCluesMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                System.Collections.Generic.Dictionary<OmniSignature, RelayOption> p_parameters = new System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Parameters
                            {
                                var length = r.GetUInt32();
                                p_parameters = new System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();
                                OmniSignature t_key = OmniSignature.Empty;
                                RelayOption t_value = RelayOption.Empty;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(r, rank + 1);
                                    p_parameters[t_key] = t_value;
                                }
                                break;
                            }
                    }
                }

                return new WantUnicastCluesMessage(p_parameters);
            }
        }
    }

    internal sealed partial class WantMulticastCluesMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<WantMulticastCluesMessage>
    {
        static WantMulticastCluesMessage()
        {
            WantMulticastCluesMessage.Formatter = new CustomFormatter();
            WantMulticastCluesMessage.Empty = new WantMulticastCluesMessage(new System.Collections.Generic.Dictionary<Channel, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantMulticastCluesMessage(System.Collections.Generic.Dictionary<Channel, RelayOption> parameters)
        {
            if (parameters is null) throw new System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Key is null) throw new System.ArgumentNullException("n.Key");
                if (n.Value is null) throw new System.ArgumentNullException("n.Value");
            }

            this.Parameters = new Omnix.Collections.ReadOnlyDictionarySlim<Channel, RelayOption>(parameters);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyDictionarySlim<Channel, RelayOption> Parameters { get; }

        public override bool Equals(WantMulticastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<WantMulticastCluesMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, WantMulticastCluesMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Parameters.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Parameters.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Parameters.Count);
                    foreach (var n in value.Parameters)
                    {
                        Channel.Formatter.Serialize(w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(w, n.Value, rank + 1);
                    }
                }
            }

            public WantMulticastCluesMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                System.Collections.Generic.Dictionary<Channel, RelayOption> p_parameters = new System.Collections.Generic.Dictionary<Channel, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Parameters
                            {
                                var length = r.GetUInt32();
                                p_parameters = new System.Collections.Generic.Dictionary<Channel, RelayOption>();
                                Channel t_key = Channel.Empty;
                                RelayOption t_value = RelayOption.Empty;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = Channel.Formatter.Deserialize(r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(r, rank + 1);
                                    p_parameters[t_key] = t_value;
                                }
                                break;
                            }
                    }
                }

                return new WantMulticastCluesMessage(p_parameters);
            }
        }
    }

    internal sealed partial class BroadcastCluesMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<BroadcastCluesMessage>
    {
        static BroadcastCluesMessage()
        {
            BroadcastCluesMessage.Formatter = new CustomFormatter();
            BroadcastCluesMessage.Empty = new BroadcastCluesMessage(System.Array.Empty<BroadcastClue>());
        }

        private readonly int __hashCode;

        public static readonly int MaxResultsCount = 8192;

        public BroadcastCluesMessage(BroadcastClue[] results)
        {
            if (results is null) throw new System.ArgumentNullException("results");
            if (results.Length > 8192) throw new System.ArgumentOutOfRangeException("results");
            foreach (var n in results)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.Results = new Omnix.Collections.ReadOnlyListSlim<BroadcastClue>(results);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Results)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyListSlim<BroadcastClue> Results { get; }

        public override bool Equals(BroadcastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastCluesMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, BroadcastCluesMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Results.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Results.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Results.Count);
                    foreach (var n in value.Results)
                    {
                        BroadcastClue.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public BroadcastCluesMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                BroadcastClue[] p_results = System.Array.Empty<BroadcastClue>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Results
                            {
                                var length = r.GetUInt32();
                                p_results = new BroadcastClue[length];
                                for (int i = 0; i < p_results.Length; i++)
                                {
                                    p_results[i] = BroadcastClue.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new BroadcastCluesMessage(p_results);
            }
        }
    }

    internal sealed partial class UnicastCluesMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<UnicastCluesMessage>
    {
        static UnicastCluesMessage()
        {
            UnicastCluesMessage.Formatter = new CustomFormatter();
            UnicastCluesMessage.Empty = new UnicastCluesMessage(System.Array.Empty<UnicastClue>());
        }

        private readonly int __hashCode;

        public static readonly int MaxResultsCount = 8192;

        public UnicastCluesMessage(UnicastClue[] results)
        {
            if (results is null) throw new System.ArgumentNullException("results");
            if (results.Length > 8192) throw new System.ArgumentOutOfRangeException("results");
            foreach (var n in results)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.Results = new Omnix.Collections.ReadOnlyListSlim<UnicastClue>(results);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Results)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyListSlim<UnicastClue> Results { get; }

        public override bool Equals(UnicastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastCluesMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, UnicastCluesMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Results.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Results.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Results.Count);
                    foreach (var n in value.Results)
                    {
                        UnicastClue.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public UnicastCluesMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                UnicastClue[] p_results = System.Array.Empty<UnicastClue>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Results
                            {
                                var length = r.GetUInt32();
                                p_results = new UnicastClue[length];
                                for (int i = 0; i < p_results.Length; i++)
                                {
                                    p_results[i] = UnicastClue.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new UnicastCluesMessage(p_results);
            }
        }
    }

    internal sealed partial class MulticastCluesMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<MulticastCluesMessage>
    {
        static MulticastCluesMessage()
        {
            MulticastCluesMessage.Formatter = new CustomFormatter();
            MulticastCluesMessage.Empty = new MulticastCluesMessage(System.Array.Empty<MulticastClue>());
        }

        private readonly int __hashCode;

        public static readonly int MaxResultsCount = 8192;

        public MulticastCluesMessage(MulticastClue[] results)
        {
            if (results is null) throw new System.ArgumentNullException("results");
            if (results.Length > 8192) throw new System.ArgumentOutOfRangeException("results");
            foreach (var n in results)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.Results = new Omnix.Collections.ReadOnlyListSlim<MulticastClue>(results);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Results)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyListSlim<MulticastClue> Results { get; }

        public override bool Equals(MulticastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastCluesMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, MulticastCluesMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Results.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Results.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Results.Count);
                    foreach (var n in value.Results)
                    {
                        MulticastClue.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public MulticastCluesMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                MulticastClue[] p_results = System.Array.Empty<MulticastClue>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Results
                            {
                                var length = r.GetUInt32();
                                p_results = new MulticastClue[length];
                                for (int i = 0; i < p_results.Length; i++)
                                {
                                    p_results[i] = MulticastClue.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new MulticastCluesMessage(p_results);
            }
        }
    }

    internal sealed partial class PublishBlocksMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<PublishBlocksMessage>
    {
        static PublishBlocksMessage()
        {
            PublishBlocksMessage.Formatter = new CustomFormatter();
            PublishBlocksMessage.Empty = new PublishBlocksMessage(new System.Collections.Generic.Dictionary<OmniHash, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public PublishBlocksMessage(System.Collections.Generic.Dictionary<OmniHash, RelayOption> parameters)
        {
            if (parameters is null) throw new System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Value is null) throw new System.ArgumentNullException("n.Value");
            }

            this.Parameters = new Omnix.Collections.ReadOnlyDictionarySlim<OmniHash, RelayOption>(parameters);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyDictionarySlim<OmniHash, RelayOption> Parameters { get; }

        public override bool Equals(PublishBlocksMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<PublishBlocksMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, PublishBlocksMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Parameters.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Parameters.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Parameters.Count);
                    foreach (var n in value.Parameters)
                    {
                        OmniHash.Formatter.Serialize(w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(w, n.Value, rank + 1);
                    }
                }
            }

            public PublishBlocksMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                System.Collections.Generic.Dictionary<OmniHash, RelayOption> p_parameters = new System.Collections.Generic.Dictionary<OmniHash, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Parameters
                            {
                                var length = r.GetUInt32();
                                p_parameters = new System.Collections.Generic.Dictionary<OmniHash, RelayOption>();
                                OmniHash t_key = OmniHash.Empty;
                                RelayOption t_value = RelayOption.Empty;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = OmniHash.Formatter.Deserialize(r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(r, rank + 1);
                                    p_parameters[t_key] = t_value;
                                }
                                break;
                            }
                    }
                }

                return new PublishBlocksMessage(p_parameters);
            }
        }
    }

    internal sealed partial class WantBlocksMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<WantBlocksMessage>
    {
        static WantBlocksMessage()
        {
            WantBlocksMessage.Formatter = new CustomFormatter();
            WantBlocksMessage.Empty = new WantBlocksMessage(new System.Collections.Generic.Dictionary<OmniHash, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantBlocksMessage(System.Collections.Generic.Dictionary<OmniHash, RelayOption> parameters)
        {
            if (parameters is null) throw new System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Value is null) throw new System.ArgumentNullException("n.Value");
            }

            this.Parameters = new Omnix.Collections.ReadOnlyDictionarySlim<OmniHash, RelayOption>(parameters);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyDictionarySlim<OmniHash, RelayOption> Parameters { get; }

        public override bool Equals(WantBlocksMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBlocksMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, WantBlocksMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Parameters.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Parameters.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Parameters.Count);
                    foreach (var n in value.Parameters)
                    {
                        OmniHash.Formatter.Serialize(w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(w, n.Value, rank + 1);
                    }
                }
            }

            public WantBlocksMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                System.Collections.Generic.Dictionary<OmniHash, RelayOption> p_parameters = new System.Collections.Generic.Dictionary<OmniHash, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Parameters
                            {
                                var length = r.GetUInt32();
                                p_parameters = new System.Collections.Generic.Dictionary<OmniHash, RelayOption>();
                                OmniHash t_key = OmniHash.Empty;
                                RelayOption t_value = RelayOption.Empty;
                                for (int i = 0; i < length; i++)
                                {
                                    t_key = OmniHash.Formatter.Deserialize(r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(r, rank + 1);
                                    p_parameters[t_key] = t_value;
                                }
                                break;
                            }
                    }
                }

                return new WantBlocksMessage(p_parameters);
            }
        }
    }

    internal sealed partial class DiffuseBlockMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<DiffuseBlockMessage>, System.IDisposable
    {
        static DiffuseBlockMessage()
        {
            DiffuseBlockMessage.Formatter = new CustomFormatter();
            DiffuseBlockMessage.Empty = new DiffuseBlockMessage(OmniHash.Empty, RelayOption.Empty, Omnix.Base.SimpleMemoryOwner<byte>.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxValueLength = 4194304;

        public DiffuseBlockMessage(OmniHash hash, RelayOption relayOption, System.Buffers.IMemoryOwner<byte> value)
        {
            if (relayOption is null) throw new System.ArgumentNullException("relayOption");
            if (value is null) throw new System.ArgumentNullException("value");
            if (value.Memory.Length > 4194304) throw new System.ArgumentOutOfRangeException("value");

            this.Hash = hash;
            this.RelayOption = relayOption;
            _value = value;

            {
                var __h = new System.HashCode();
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (this.RelayOption != default) __h.Add(this.RelayOption.GetHashCode());
                if (!this.Value.IsEmpty) __h.Add(Omnix.Base.Helpers.ObjectHelper.GetHashCode(this.Value.Span));
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        public RelayOption RelayOption { get; }
        private readonly System.Buffers.IMemoryOwner<byte> _value;
        public System.ReadOnlyMemory<byte> Value => _value.Memory;

        public override bool Equals(DiffuseBlockMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.RelayOption != target.RelayOption) return false;
            if (!Omnix.Base.BytesOperations.SequenceEqual(this.Value.Span, target.Value.Span)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<DiffuseBlockMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, DiffuseBlockMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Hash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.RelayOption != RelayOption.Empty)
                    {
                        propertyCount++;
                    }
                    if (!value.Value.IsEmpty)
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
                if (value.RelayOption != RelayOption.Empty)
                {
                    w.Write((uint)1);
                    RelayOption.Formatter.Serialize(w, value.RelayOption, rank + 1);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)2);
                    w.Write(value.Value.Span);
                }
            }

            public DiffuseBlockMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                RelayOption p_relayOption = RelayOption.Empty;
                System.Buffers.IMemoryOwner<byte> p_value = Omnix.Base.SimpleMemoryOwner<byte>.Empty;

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
                        case 1: // RelayOption
                            {
                                p_relayOption = RelayOption.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // Value
                            {
                                p_value = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new DiffuseBlockMessage(p_hash, p_relayOption, p_value);
            }
        }
    }

    internal sealed partial class BlockMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<BlockMessage>, System.IDisposable
    {
        static BlockMessage()
        {
            BlockMessage.Formatter = new CustomFormatter();
            BlockMessage.Empty = new BlockMessage(OmniHash.Empty, Omnix.Base.SimpleMemoryOwner<byte>.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxValueLength = 4194304;

        public BlockMessage(OmniHash hash, System.Buffers.IMemoryOwner<byte> value)
        {
            if (value is null) throw new System.ArgumentNullException("value");
            if (value.Memory.Length > 4194304) throw new System.ArgumentOutOfRangeException("value");

            this.Hash = hash;
            _value = value;

            {
                var __h = new System.HashCode();
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (!this.Value.IsEmpty) __h.Add(Omnix.Base.Helpers.ObjectHelper.GetHashCode(this.Value.Span));
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        private readonly System.Buffers.IMemoryOwner<byte> _value;
        public System.ReadOnlyMemory<byte> Value => _value.Memory;

        public override bool Equals(BlockMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (!Omnix.Base.BytesOperations.SequenceEqual(this.Value.Span, target.Value.Span)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<BlockMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, BlockMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Hash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (!value.Value.IsEmpty)
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
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.Value.Span);
                }
            }

            public BlockMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                System.Buffers.IMemoryOwner<byte> p_value = Omnix.Base.SimpleMemoryOwner<byte>.Empty;

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
                        case 1: // Value
                            {
                                p_value = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new BlockMessage(p_hash, p_value);
            }
        }
    }

    internal sealed partial class DiffuseBlockInfo : Omnix.Serialization.RocketPack.RocketPackMessageBase<DiffuseBlockInfo>
    {
        static DiffuseBlockInfo()
        {
            DiffuseBlockInfo.Formatter = new CustomFormatter();
            DiffuseBlockInfo.Empty = new DiffuseBlockInfo(Omnix.Serialization.RocketPack.Timestamp.Zero, OmniHash.Empty, RelayOption.Empty);
        }

        private readonly int __hashCode;

        public DiffuseBlockInfo(Omnix.Serialization.RocketPack.Timestamp creationTime, OmniHash hash, RelayOption relayOption)
        {
            if (relayOption is null) throw new System.ArgumentNullException("relayOption");

            this.CreationTime = creationTime;
            this.Hash = hash;
            this.RelayOption = relayOption;

            {
                var __h = new System.HashCode();
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (this.RelayOption != default) __h.Add(this.RelayOption.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public OmniHash Hash { get; }
        public RelayOption RelayOption { get; }

        public override bool Equals(DiffuseBlockInfo? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Hash != target.Hash) return false;
            if (this.RelayOption != target.RelayOption) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<DiffuseBlockInfo>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, DiffuseBlockInfo value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Hash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.RelayOption != RelayOption.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)0);
                    w.Write(value.CreationTime);
                }
                if (value.Hash != OmniHash.Empty)
                {
                    w.Write((uint)1);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
                if (value.RelayOption != RelayOption.Empty)
                {
                    w.Write((uint)2);
                    RelayOption.Formatter.Serialize(w, value.RelayOption, rank + 1);
                }
            }

            public DiffuseBlockInfo Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                OmniHash p_hash = OmniHash.Empty;
                RelayOption p_relayOption = RelayOption.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 1: // Hash
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // RelayOption
                            {
                                p_relayOption = RelayOption.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new DiffuseBlockInfo(p_creationTime, p_hash, p_relayOption);
            }
        }
    }

    internal sealed partial class ExchangeManagerConfig : Omnix.Serialization.RocketPack.RocketPackMessageBase<ExchangeManagerConfig>
    {
        static ExchangeManagerConfig()
        {
            ExchangeManagerConfig.Formatter = new CustomFormatter();
            ExchangeManagerConfig.Empty = new ExchangeManagerConfig(0);
        }

        private readonly int __hashCode;

        public ExchangeManagerConfig(uint version)
        {
            this.Version = version;

            {
                var __h = new System.HashCode();
                if (this.Version != default) __h.Add(this.Version.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public uint Version { get; }

        public override bool Equals(ExchangeManagerConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ExchangeManagerConfig>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ExchangeManagerConfig value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Version != 0)
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
            }

            public ExchangeManagerConfig Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                uint p_version = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Version
                            {
                                p_version = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new ExchangeManagerConfig(p_version);
            }
        }
    }

}
