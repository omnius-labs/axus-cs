using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Core.Internal;
using Xeus.Messages;

#nullable enable

namespace Xeus.Core.Internal.Exchange
{
    internal enum ProtocolVersion : byte
    {
        Version1 = 1,
    }

    internal sealed partial class BroadcastClue : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<BroadcastClue>
    {
        static BroadcastClue()
        {
            BroadcastClue.Formatter = new CustomFormatter();
            BroadcastClue.Empty = new BroadcastClue(string.Empty, global::Omnix.Serialization.RocketPack.Timestamp.Zero, XeusClue.Empty, OmniCertificate.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxTypeLength = 256;

        public BroadcastClue(string type, global::Omnix.Serialization.RocketPack.Timestamp creationTime, XeusClue clue, OmniCertificate certificate)
        {
            if (type is null) throw new global::System.ArgumentNullException("type");
            if (type.Length > 256) throw new global::System.ArgumentOutOfRangeException("type");
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (certificate is null) throw new global::System.ArgumentNullException("certificate");

            this.Type = type;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Certificate = certificate;

            {
                var __h = new global::System.HashCode();
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Certificate != default) __h.Add(this.Certificate.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string Type { get; }
        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusClue Clue { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(BroadcastClue target)
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

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastClue>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, BroadcastClue value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Clue != XeusClue.Empty)
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
                if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)1);
                    w.Write(value.CreationTime);
                }
                if (value.Clue != XeusClue.Empty)
                {
                    w.Write((uint)2);
                    XeusClue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Certificate != OmniCertificate.Empty)
                {
                    w.Write((uint)3);
                    OmniCertificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }

            public BroadcastClue Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                global::Omnix.Serialization.RocketPack.Timestamp p_creationTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusClue p_clue = XeusClue.Empty;
                OmniCertificate p_certificate = OmniCertificate.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_type = r.GetString(256);
                                break;
                            }
                        case 1:
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 2:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 3:
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

    internal sealed partial class UnicastClue : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<UnicastClue>
    {
        static UnicastClue()
        {
            UnicastClue.Formatter = new CustomFormatter();
            UnicastClue.Empty = new UnicastClue(string.Empty, OmniSignature.Empty, global::Omnix.Serialization.RocketPack.Timestamp.Zero, XeusClue.Empty, OmniCertificate.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxTypeLength = 256;

        public UnicastClue(string type, OmniSignature signature, global::Omnix.Serialization.RocketPack.Timestamp creationTime, XeusClue clue, OmniCertificate certificate)
        {
            if (type is null) throw new global::System.ArgumentNullException("type");
            if (type.Length > 256) throw new global::System.ArgumentOutOfRangeException("type");
            if (signature is null) throw new global::System.ArgumentNullException("signature");
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (certificate is null) throw new global::System.ArgumentNullException("certificate");

            this.Type = type;
            this.Signature = signature;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Certificate = certificate;

            {
                var __h = new global::System.HashCode();
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
        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusClue Clue { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(UnicastClue target)
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

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastClue>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, UnicastClue value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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
                    if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Clue != XeusClue.Empty)
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
                if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Clue != XeusClue.Empty)
                {
                    w.Write((uint)3);
                    XeusClue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Certificate != OmniCertificate.Empty)
                {
                    w.Write((uint)4);
                    OmniCertificate.Formatter.Serialize(w, value.Certificate, rank + 1);
                }
            }

            public UnicastClue Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                OmniSignature p_signature = OmniSignature.Empty;
                global::Omnix.Serialization.RocketPack.Timestamp p_creationTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusClue p_clue = XeusClue.Empty;
                OmniCertificate p_certificate = OmniCertificate.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_type = r.GetString(256);
                                break;
                            }
                        case 1:
                            {
                                p_signature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 4:
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

    internal sealed partial class MulticastClue : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<MulticastClue>
    {
        static MulticastClue()
        {
            MulticastClue.Formatter = new CustomFormatter();
            MulticastClue.Empty = new MulticastClue(string.Empty, OmniSignature.Empty, global::Omnix.Serialization.RocketPack.Timestamp.Zero, XeusClue.Empty, OmniHashcash.Empty, OmniCertificate.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxTypeLength = 256;

        public MulticastClue(string type, OmniSignature signature, global::Omnix.Serialization.RocketPack.Timestamp creationTime, XeusClue clue, OmniHashcash hashcash, OmniCertificate certificate)
        {
            if (type is null) throw new global::System.ArgumentNullException("type");
            if (type.Length > 256) throw new global::System.ArgumentOutOfRangeException("type");
            if (signature is null) throw new global::System.ArgumentNullException("signature");
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            if (hashcash is null) throw new global::System.ArgumentNullException("hashcash");
            if (certificate is null) throw new global::System.ArgumentNullException("certificate");

            this.Type = type;
            this.Signature = signature;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Hashcash = hashcash;
            this.Certificate = certificate;

            {
                var __h = new global::System.HashCode();
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                if (this.Signature != default) __h.Add(this.Signature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Hashcash != default) __h.Add(this.Hashcash.GetHashCode());
                if (this.Certificate != default) __h.Add(this.Certificate.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string Type { get; }
        public OmniSignature Signature { get; }
        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusClue Clue { get; }
        public OmniHashcash Hashcash { get; }
        public OmniCertificate Certificate { get; }

        public override bool Equals(MulticastClue target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Signature != target.Signature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if (this.Hashcash != target.Hashcash) return false;
            if (this.Certificate != target.Certificate) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastClue>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, MulticastClue value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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
                    if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Clue != XeusClue.Empty)
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
                if (value.Signature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    OmniSignature.Formatter.Serialize(w, value.Signature, rank + 1);
                }
                if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Clue != XeusClue.Empty)
                {
                    w.Write((uint)3);
                    XeusClue.Formatter.Serialize(w, value.Clue, rank + 1);
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

            public MulticastClue Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                OmniSignature p_signature = OmniSignature.Empty;
                global::Omnix.Serialization.RocketPack.Timestamp p_creationTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusClue p_clue = XeusClue.Empty;
                OmniHashcash p_hashcash = OmniHashcash.Empty;
                OmniCertificate p_certificate = OmniCertificate.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_type = r.GetString(256);
                                break;
                            }
                        case 1:
                            {
                                p_signature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 4:
                            {
                                p_hashcash = OmniHashcash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 5:
                            {
                                p_certificate = OmniCertificate.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new MulticastClue(p_type, p_signature, p_creationTime, p_clue, p_hashcash, p_certificate);
            }
        }
    }

    internal sealed partial class HelloMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<HelloMessage>
    {
        static HelloMessage()
        {
            HelloMessage.Formatter = new CustomFormatter();
            HelloMessage.Empty = new HelloMessage(global::System.Array.Empty<ProtocolVersion>());
        }

        private readonly int __hashCode;

        public static readonly int MaxProtocolVersionsCount = 32;

        public HelloMessage(ProtocolVersion[] protocolVersions)
        {
            if (protocolVersions is null) throw new global::System.ArgumentNullException("protocolVersions");
            if (protocolVersions.Length > 32) throw new global::System.ArgumentOutOfRangeException("protocolVersions");

            this.ProtocolVersions = new global::Omnix.DataStructures.ReadOnlyListSlim<ProtocolVersion>(protocolVersions);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.ProtocolVersions)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<ProtocolVersion> ProtocolVersions { get; }

        public override bool Equals(HelloMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ProtocolVersions, target.ProtocolVersions)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<HelloMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, HelloMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public HelloMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ProtocolVersion[] p_protocolVersions = global::System.Array.Empty<ProtocolVersion>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
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

    internal sealed partial class ProfileMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<ProfileMessage>
    {
        static ProfileMessage()
        {
            ProfileMessage.Formatter = new CustomFormatter();
            ProfileMessage.Empty = new ProfileMessage(global::System.ReadOnlyMemory<byte>.Empty, OmniAddress.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxIdLength = 32;

        public ProfileMessage(global::System.ReadOnlyMemory<byte> id, OmniAddress address)
        {
            if (id.Length > 32) throw new global::System.ArgumentOutOfRangeException("id");
            if (address is null) throw new global::System.ArgumentNullException("address");

            this.Id = id;
            this.Address = address;

            {
                var __h = new global::System.HashCode();
                if (!this.Id.IsEmpty) __h.Add(global::Omnix.Base.Helpers.ObjectHelper.GetHashCode(this.Id.Span));
                if (this.Address != default) __h.Add(this.Address.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public global::System.ReadOnlyMemory<byte> Id { get; }
        public OmniAddress Address { get; }

        public override bool Equals(ProfileMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.BytesOperations.SequenceEqual(this.Id.Span, target.Id.Span)) return false;
            if (this.Address != target.Address) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ProfileMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, ProfileMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public ProfileMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.ReadOnlyMemory<byte> p_id = global::System.ReadOnlyMemory<byte>.Empty;
                OmniAddress p_address = OmniAddress.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_id = r.GetMemory(32);
                                break;
                            }
                        case 1:
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

    internal sealed partial class NodeAddressesMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<NodeAddressesMessage>
    {
        static NodeAddressesMessage()
        {
            NodeAddressesMessage.Formatter = new CustomFormatter();
            NodeAddressesMessage.Empty = new NodeAddressesMessage(global::System.Array.Empty<OmniAddress>());
        }

        private readonly int __hashCode;

        public static readonly int MaxAddressesCount = 256;

        public NodeAddressesMessage(OmniAddress[] addresses)
        {
            if (addresses is null) throw new global::System.ArgumentNullException("addresses");
            if (addresses.Length > 256) throw new global::System.ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Addresses = new global::Omnix.DataStructures.ReadOnlyListSlim<OmniAddress>(addresses);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Addresses)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniAddress> Addresses { get; }

        public override bool Equals(NodeAddressesMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<NodeAddressesMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, NodeAddressesMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public NodeAddressesMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniAddress[] p_addresses = global::System.Array.Empty<OmniAddress>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
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

    internal sealed partial class RelayOption : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<RelayOption>
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
                var __h = new global::System.HashCode();
                if (this.HopLimit != default) __h.Add(this.HopLimit.GetHashCode());
                if (this.Priority != default) __h.Add(this.Priority.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public byte HopLimit { get; }
        public byte Priority { get; }

        public override bool Equals(RelayOption target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Priority != target.Priority) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<RelayOption>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, RelayOption value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public RelayOption Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                byte p_hopLimit = 0;
                byte p_priority = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_hopLimit = r.GetUInt8();
                                break;
                            }
                        case 1:
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

    internal sealed partial class WantBroadcastCluesMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<WantBroadcastCluesMessage>
    {
        static WantBroadcastCluesMessage()
        {
            WantBroadcastCluesMessage.Formatter = new CustomFormatter();
            WantBroadcastCluesMessage.Empty = new WantBroadcastCluesMessage(new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantBroadcastCluesMessage(global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption> parameters)
        {
            if (parameters is null) throw new global::System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new global::System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Key is null) throw new global::System.ArgumentNullException("n.Key");
                if (n.Value is null) throw new global::System.ArgumentNullException("n.Value");
            }

            this.Parameters = new global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption>(parameters);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption> Parameters { get; }

        public override bool Equals(WantBroadcastCluesMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBroadcastCluesMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, WantBroadcastCluesMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public WantBroadcastCluesMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption> p_parameters = new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_parameters = new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();
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

    internal sealed partial class WantUnicastCluesMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<WantUnicastCluesMessage>
    {
        static WantUnicastCluesMessage()
        {
            WantUnicastCluesMessage.Formatter = new CustomFormatter();
            WantUnicastCluesMessage.Empty = new WantUnicastCluesMessage(new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantUnicastCluesMessage(global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption> parameters)
        {
            if (parameters is null) throw new global::System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new global::System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Key is null) throw new global::System.ArgumentNullException("n.Key");
                if (n.Value is null) throw new global::System.ArgumentNullException("n.Value");
            }

            this.Parameters = new global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption>(parameters);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption> Parameters { get; }

        public override bool Equals(WantUnicastCluesMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantUnicastCluesMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, WantUnicastCluesMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public WantUnicastCluesMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption> p_parameters = new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_parameters = new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();
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

    internal sealed partial class WantMulticastCluesMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<WantMulticastCluesMessage>
    {
        static WantMulticastCluesMessage()
        {
            WantMulticastCluesMessage.Formatter = new CustomFormatter();
            WantMulticastCluesMessage.Empty = new WantMulticastCluesMessage(new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantMulticastCluesMessage(global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption> parameters)
        {
            if (parameters is null) throw new global::System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new global::System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Key is null) throw new global::System.ArgumentNullException("n.Key");
                if (n.Value is null) throw new global::System.ArgumentNullException("n.Value");
            }

            this.Parameters = new global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption>(parameters);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption> Parameters { get; }

        public override bool Equals(WantMulticastCluesMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantMulticastCluesMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, WantMulticastCluesMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public WantMulticastCluesMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption> p_parameters = new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_parameters = new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>();
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

                return new WantMulticastCluesMessage(p_parameters);
            }
        }
    }

    internal sealed partial class BroadcastCluesMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<BroadcastCluesMessage>
    {
        static BroadcastCluesMessage()
        {
            BroadcastCluesMessage.Formatter = new CustomFormatter();
            BroadcastCluesMessage.Empty = new BroadcastCluesMessage(global::System.Array.Empty<BroadcastClue>());
        }

        private readonly int __hashCode;

        public static readonly int MaxResultsCount = 8192;

        public BroadcastCluesMessage(BroadcastClue[] results)
        {
            if (results is null) throw new global::System.ArgumentNullException("results");
            if (results.Length > 8192) throw new global::System.ArgumentOutOfRangeException("results");
            foreach (var n in results)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Results = new global::Omnix.DataStructures.ReadOnlyListSlim<BroadcastClue>(results);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Results)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<BroadcastClue> Results { get; }

        public override bool Equals(BroadcastCluesMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastCluesMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, BroadcastCluesMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public BroadcastCluesMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                BroadcastClue[] p_results = global::System.Array.Empty<BroadcastClue>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
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

    internal sealed partial class UnicastCluesMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<UnicastCluesMessage>
    {
        static UnicastCluesMessage()
        {
            UnicastCluesMessage.Formatter = new CustomFormatter();
            UnicastCluesMessage.Empty = new UnicastCluesMessage(global::System.Array.Empty<UnicastClue>());
        }

        private readonly int __hashCode;

        public static readonly int MaxResultsCount = 8192;

        public UnicastCluesMessage(UnicastClue[] results)
        {
            if (results is null) throw new global::System.ArgumentNullException("results");
            if (results.Length > 8192) throw new global::System.ArgumentOutOfRangeException("results");
            foreach (var n in results)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Results = new global::Omnix.DataStructures.ReadOnlyListSlim<UnicastClue>(results);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Results)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<UnicastClue> Results { get; }

        public override bool Equals(UnicastCluesMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastCluesMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, UnicastCluesMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public UnicastCluesMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                UnicastClue[] p_results = global::System.Array.Empty<UnicastClue>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
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

    internal sealed partial class MulticastCluesMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<MulticastCluesMessage>
    {
        static MulticastCluesMessage()
        {
            MulticastCluesMessage.Formatter = new CustomFormatter();
            MulticastCluesMessage.Empty = new MulticastCluesMessage(global::System.Array.Empty<MulticastClue>());
        }

        private readonly int __hashCode;

        public static readonly int MaxResultsCount = 8192;

        public MulticastCluesMessage(MulticastClue[] results)
        {
            if (results is null) throw new global::System.ArgumentNullException("results");
            if (results.Length > 8192) throw new global::System.ArgumentOutOfRangeException("results");
            foreach (var n in results)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Results = new global::Omnix.DataStructures.ReadOnlyListSlim<MulticastClue>(results);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Results)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<MulticastClue> Results { get; }

        public override bool Equals(MulticastCluesMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastCluesMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, MulticastCluesMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public MulticastCluesMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                MulticastClue[] p_results = global::System.Array.Empty<MulticastClue>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
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

    internal sealed partial class PublishBlocksMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<PublishBlocksMessage>
    {
        static PublishBlocksMessage()
        {
            PublishBlocksMessage.Formatter = new CustomFormatter();
            PublishBlocksMessage.Empty = new PublishBlocksMessage(new global::System.Collections.Generic.Dictionary<OmniHash, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public PublishBlocksMessage(global::System.Collections.Generic.Dictionary<OmniHash, RelayOption> parameters)
        {
            if (parameters is null) throw new global::System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new global::System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Value is null) throw new global::System.ArgumentNullException("n.Value");
            }

            this.Parameters = new global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniHash, RelayOption>(parameters);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniHash, RelayOption> Parameters { get; }

        public override bool Equals(PublishBlocksMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<PublishBlocksMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, PublishBlocksMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public PublishBlocksMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.Collections.Generic.Dictionary<OmniHash, RelayOption> p_parameters = new global::System.Collections.Generic.Dictionary<OmniHash, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_parameters = new global::System.Collections.Generic.Dictionary<OmniHash, RelayOption>();
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

    internal sealed partial class WantBlocksMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<WantBlocksMessage>
    {
        static WantBlocksMessage()
        {
            WantBlocksMessage.Formatter = new CustomFormatter();
            WantBlocksMessage.Empty = new WantBlocksMessage(new global::System.Collections.Generic.Dictionary<OmniHash, RelayOption>());
        }

        private readonly int __hashCode;

        public static readonly int MaxParametersCount = 8192;

        public WantBlocksMessage(global::System.Collections.Generic.Dictionary<OmniHash, RelayOption> parameters)
        {
            if (parameters is null) throw new global::System.ArgumentNullException("parameters");
            if (parameters.Count > 8192) throw new global::System.ArgumentOutOfRangeException("parameters");
            foreach (var n in parameters)
            {
                if (n.Value is null) throw new global::System.ArgumentNullException("n.Value");
            }

            this.Parameters = new global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniHash, RelayOption>(parameters);

            {
                var __h = new global::System.HashCode();
                foreach (var n in this.Parameters)
                {
                    if (n.Key != default) __h.Add(n.Key.GetHashCode());
                    if (n.Value != default) __h.Add(n.Value.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniHash, RelayOption> Parameters { get; }

        public override bool Equals(WantBlocksMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBlocksMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, WantBlocksMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public WantBlocksMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.Collections.Generic.Dictionary<OmniHash, RelayOption> p_parameters = new global::System.Collections.Generic.Dictionary<OmniHash, RelayOption>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_parameters = new global::System.Collections.Generic.Dictionary<OmniHash, RelayOption>();
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

    internal sealed partial class DiffuseBlockMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<DiffuseBlockMessage>, global::System.IDisposable
    {
        static DiffuseBlockMessage()
        {
            DiffuseBlockMessage.Formatter = new CustomFormatter();
            DiffuseBlockMessage.Empty = new DiffuseBlockMessage(OmniHash.Empty, RelayOption.Empty, global::Omnix.Base.SimpleMemoryOwner<byte>.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxValueLength = 4194304;

        public DiffuseBlockMessage(OmniHash hash, RelayOption relayOption, global::System.Buffers.IMemoryOwner<byte> value)
        {
            if (relayOption is null) throw new global::System.ArgumentNullException("relayOption");
            if (value is null) throw new global::System.ArgumentNullException("value");
            if (value.Memory.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("value");

            this.Hash = hash;
            this.RelayOption = relayOption;
            _value = value;

            {
                var __h = new global::System.HashCode();
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (this.RelayOption != default) __h.Add(this.RelayOption.GetHashCode());
                if (!this.Value.IsEmpty) __h.Add(global::Omnix.Base.Helpers.ObjectHelper.GetHashCode(this.Value.Span));
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        public RelayOption RelayOption { get; }
        private readonly global::System.Buffers.IMemoryOwner<byte> _value;
        public global::System.ReadOnlyMemory<byte> Value => _value.Memory;

        public override bool Equals(DiffuseBlockMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.RelayOption != target.RelayOption) return false;
            if (!global::Omnix.Base.BytesOperations.SequenceEqual(this.Value.Span, target.Value.Span)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DiffuseBlockMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, DiffuseBlockMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public DiffuseBlockMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                RelayOption p_relayOption = RelayOption.Empty;
                global::System.Buffers.IMemoryOwner<byte> p_value = global::Omnix.Base.SimpleMemoryOwner<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_relayOption = RelayOption.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2:
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

    internal sealed partial class BlockMessage : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<BlockMessage>, global::System.IDisposable
    {
        static BlockMessage()
        {
            BlockMessage.Formatter = new CustomFormatter();
            BlockMessage.Empty = new BlockMessage(OmniHash.Empty, global::Omnix.Base.SimpleMemoryOwner<byte>.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxValueLength = 4194304;

        public BlockMessage(OmniHash hash, global::System.Buffers.IMemoryOwner<byte> value)
        {
            if (value is null) throw new global::System.ArgumentNullException("value");
            if (value.Memory.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("value");

            this.Hash = hash;
            _value = value;

            {
                var __h = new global::System.HashCode();
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (!this.Value.IsEmpty) __h.Add(global::Omnix.Base.Helpers.ObjectHelper.GetHashCode(this.Value.Span));
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        private readonly global::System.Buffers.IMemoryOwner<byte> _value;
        public global::System.ReadOnlyMemory<byte> Value => _value.Memory;

        public override bool Equals(BlockMessage target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (!global::Omnix.Base.BytesOperations.SequenceEqual(this.Value.Span, target.Value.Span)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BlockMessage>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, BlockMessage value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public BlockMessage Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                global::System.Buffers.IMemoryOwner<byte> p_value = global::Omnix.Base.SimpleMemoryOwner<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1:
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

    internal sealed partial class DiffuseBlockInfo : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<DiffuseBlockInfo>
    {
        static DiffuseBlockInfo()
        {
            DiffuseBlockInfo.Formatter = new CustomFormatter();
            DiffuseBlockInfo.Empty = new DiffuseBlockInfo(global::Omnix.Serialization.RocketPack.Timestamp.Zero, OmniHash.Empty, RelayOption.Empty);
        }

        private readonly int __hashCode;

        public DiffuseBlockInfo(global::Omnix.Serialization.RocketPack.Timestamp creationTime, OmniHash hash, RelayOption relayOption)
        {
            if (relayOption is null) throw new global::System.ArgumentNullException("relayOption");

            this.CreationTime = creationTime;
            this.Hash = hash;
            this.RelayOption = relayOption;

            {
                var __h = new global::System.HashCode();
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (this.RelayOption != default) __h.Add(this.RelayOption.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public OmniHash Hash { get; }
        public RelayOption RelayOption { get; }

        public override bool Equals(DiffuseBlockInfo target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Hash != target.Hash) return false;
            if (this.RelayOption != target.RelayOption) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DiffuseBlockInfo>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, DiffuseBlockInfo value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
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

                if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
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

            public DiffuseBlockInfo Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::Omnix.Serialization.RocketPack.Timestamp p_creationTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;
                OmniHash p_hash = OmniHash.Empty;
                RelayOption p_relayOption = RelayOption.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 1:
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2:
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

    internal sealed partial class ExchangeManagerConfig : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<ExchangeManagerConfig>
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
                var __h = new global::System.HashCode();
                if (this.Version != default) __h.Add(this.Version.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public uint Version { get; }

        public override bool Equals(ExchangeManagerConfig target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ExchangeManagerConfig>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, ExchangeManagerConfig value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

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

            public ExchangeManagerConfig Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_version = 0;

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
                    }
                }

                return new ExchangeManagerConfig(p_version);
            }
        }
    }

}
