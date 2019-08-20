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

    internal sealed partial class BroadcastClue : global::Omnix.Serialization.RocketPack.IRocketPackMessage<BroadcastClue>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastClue> Formatter { get; }
        public static BroadcastClue Empty { get; }

        static BroadcastClue()
        {
            BroadcastClue.Formatter = new ___CustomFormatter();
            BroadcastClue.Empty = new BroadcastClue(string.Empty, global::Omnix.Serialization.RocketPack.Timestamp.Zero, XeusClue.Empty, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxTypeLength = 256;

        public BroadcastClue(string type, global::Omnix.Serialization.RocketPack.Timestamp creationTime, XeusClue clue, OmniCertificate? certificate)
        {
            if (type is null) throw new global::System.ArgumentNullException("type");
            if (type.Length > 256) throw new global::System.ArgumentOutOfRangeException("type");
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            this.Type = type;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Certificate = certificate;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (creationTime != default) ___h.Add(creationTime.GetHashCode());
                if (clue != default) ___h.Add(clue.GetHashCode());
                if (certificate != default) ___h.Add(certificate.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string Type { get; }
        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusClue Clue { get; }
        public OmniCertificate? Certificate { get; }

        public static BroadcastClue Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BroadcastClue? left, BroadcastClue? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BroadcastClue? left, BroadcastClue? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BroadcastClue)) return false;
            return this.Equals((BroadcastClue)other);
        }
        public bool Equals(BroadcastClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if ((this.Certificate is null) != (target.Certificate is null)) return false;
            if (!(this.Certificate is null) && !(target.Certificate is null) && this.Certificate != target.Certificate) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastClue>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in BroadcastClue value, in int rank)
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
                    if (value.Certificate != null)
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
                    XeusClue.Formatter.Serialize(ref w, value.Clue, rank + 1);
                }
                if (value.Certificate != null)
                {
                    w.Write((uint)3);
                    OmniCertificate.Formatter.Serialize(ref w, value.Certificate, rank + 1);
                }
            }

            public BroadcastClue Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                global::Omnix.Serialization.RocketPack.Timestamp p_creationTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusClue p_clue = XeusClue.Empty;
                OmniCertificate? p_certificate = null;

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
                                p_clue = XeusClue.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 3:
                            {
                                p_certificate = OmniCertificate.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new BroadcastClue(p_type, p_creationTime, p_clue, p_certificate);
            }
        }
    }

    internal sealed partial class UnicastClue : global::Omnix.Serialization.RocketPack.IRocketPackMessage<UnicastClue>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastClue> Formatter { get; }
        public static UnicastClue Empty { get; }

        static UnicastClue()
        {
            UnicastClue.Formatter = new ___CustomFormatter();
            UnicastClue.Empty = new UnicastClue(string.Empty, OmniSignature.Empty, global::Omnix.Serialization.RocketPack.Timestamp.Zero, XeusClue.Empty, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxTypeLength = 256;

        public UnicastClue(string type, OmniSignature signature, global::Omnix.Serialization.RocketPack.Timestamp creationTime, XeusClue clue, OmniCertificate? certificate)
        {
            if (type is null) throw new global::System.ArgumentNullException("type");
            if (type.Length > 256) throw new global::System.ArgumentOutOfRangeException("type");
            if (signature is null) throw new global::System.ArgumentNullException("signature");
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            this.Type = type;
            this.Signature = signature;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Certificate = certificate;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (signature != default) ___h.Add(signature.GetHashCode());
                if (creationTime != default) ___h.Add(creationTime.GetHashCode());
                if (clue != default) ___h.Add(clue.GetHashCode());
                if (certificate != default) ___h.Add(certificate.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string Type { get; }
        public OmniSignature Signature { get; }
        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusClue Clue { get; }
        public OmniCertificate? Certificate { get; }

        public static UnicastClue Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(UnicastClue? left, UnicastClue? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(UnicastClue? left, UnicastClue? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is UnicastClue)) return false;
            return this.Equals((UnicastClue)other);
        }
        public bool Equals(UnicastClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Signature != target.Signature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if ((this.Certificate is null) != (target.Certificate is null)) return false;
            if (!(this.Certificate is null) && !(target.Certificate is null) && this.Certificate != target.Certificate) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastClue>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in UnicastClue value, in int rank)
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
                    if (value.Certificate != null)
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
                    OmniSignature.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Clue != XeusClue.Empty)
                {
                    w.Write((uint)3);
                    XeusClue.Formatter.Serialize(ref w, value.Clue, rank + 1);
                }
                if (value.Certificate != null)
                {
                    w.Write((uint)4);
                    OmniCertificate.Formatter.Serialize(ref w, value.Certificate, rank + 1);
                }
            }

            public UnicastClue Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                OmniSignature p_signature = OmniSignature.Empty;
                global::Omnix.Serialization.RocketPack.Timestamp p_creationTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusClue p_clue = XeusClue.Empty;
                OmniCertificate? p_certificate = null;

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
                                p_signature = OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 4:
                            {
                                p_certificate = OmniCertificate.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new UnicastClue(p_type, p_signature, p_creationTime, p_clue, p_certificate);
            }
        }
    }

    internal sealed partial class MulticastClue : global::Omnix.Serialization.RocketPack.IRocketPackMessage<MulticastClue>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastClue> Formatter { get; }
        public static MulticastClue Empty { get; }

        static MulticastClue()
        {
            MulticastClue.Formatter = new ___CustomFormatter();
            MulticastClue.Empty = new MulticastClue(string.Empty, OmniSignature.Empty, global::Omnix.Serialization.RocketPack.Timestamp.Zero, XeusClue.Empty, null, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxTypeLength = 256;

        public MulticastClue(string type, OmniSignature signature, global::Omnix.Serialization.RocketPack.Timestamp creationTime, XeusClue clue, OmniHashcash? hashcash, OmniCertificate? certificate)
        {
            if (type is null) throw new global::System.ArgumentNullException("type");
            if (type.Length > 256) throw new global::System.ArgumentOutOfRangeException("type");
            if (signature is null) throw new global::System.ArgumentNullException("signature");
            if (clue is null) throw new global::System.ArgumentNullException("clue");
            this.Type = type;
            this.Signature = signature;
            this.CreationTime = creationTime;
            this.Clue = clue;
            this.Hashcash = hashcash;
            this.Certificate = certificate;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (signature != default) ___h.Add(signature.GetHashCode());
                if (creationTime != default) ___h.Add(creationTime.GetHashCode());
                if (clue != default) ___h.Add(clue.GetHashCode());
                if (hashcash != default) ___h.Add(hashcash.GetHashCode());
                if (certificate != default) ___h.Add(certificate.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string Type { get; }
        public OmniSignature Signature { get; }
        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusClue Clue { get; }
        public OmniHashcash? Hashcash { get; }
        public OmniCertificate? Certificate { get; }

        public static MulticastClue Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(MulticastClue? left, MulticastClue? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(MulticastClue? left, MulticastClue? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is MulticastClue)) return false;
            return this.Equals((MulticastClue)other);
        }
        public bool Equals(MulticastClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Signature != target.Signature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Clue != target.Clue) return false;
            if ((this.Hashcash is null) != (target.Hashcash is null)) return false;
            if (!(this.Hashcash is null) && !(target.Hashcash is null) && this.Hashcash != target.Hashcash) return false;
            if ((this.Certificate is null) != (target.Certificate is null)) return false;
            if (!(this.Certificate is null) && !(target.Certificate is null) && this.Certificate != target.Certificate) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastClue>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in MulticastClue value, in int rank)
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
                    if (value.Hashcash != null)
                    {
                        propertyCount++;
                    }
                    if (value.Certificate != null)
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
                    OmniSignature.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Clue != XeusClue.Empty)
                {
                    w.Write((uint)3);
                    XeusClue.Formatter.Serialize(ref w, value.Clue, rank + 1);
                }
                if (value.Hashcash != null)
                {
                    w.Write((uint)4);
                    OmniHashcash.Formatter.Serialize(ref w, value.Hashcash, rank + 1);
                }
                if (value.Certificate != null)
                {
                    w.Write((uint)5);
                    OmniCertificate.Formatter.Serialize(ref w, value.Certificate, rank + 1);
                }
            }

            public MulticastClue Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                OmniSignature p_signature = OmniSignature.Empty;
                global::Omnix.Serialization.RocketPack.Timestamp p_creationTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusClue p_clue = XeusClue.Empty;
                OmniHashcash? p_hashcash = null;
                OmniCertificate? p_certificate = null;

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
                                p_signature = OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3:
                            {
                                p_clue = XeusClue.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 4:
                            {
                                p_hashcash = OmniHashcash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 5:
                            {
                                p_certificate = OmniCertificate.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new MulticastClue(p_type, p_signature, p_creationTime, p_clue, p_hashcash, p_certificate);
            }
        }
    }

    internal sealed partial class HelloMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<HelloMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<HelloMessage> Formatter { get; }
        public static HelloMessage Empty { get; }

        static HelloMessage()
        {
            HelloMessage.Formatter = new ___CustomFormatter();
            HelloMessage.Empty = new HelloMessage(global::System.Array.Empty<ProtocolVersion>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxProtocolVersionsCount = 32;

        public HelloMessage(ProtocolVersion[] protocolVersions)
        {
            if (protocolVersions is null) throw new global::System.ArgumentNullException("protocolVersions");
            if (protocolVersions.Length > 32) throw new global::System.ArgumentOutOfRangeException("protocolVersions");

            this.ProtocolVersions = new global::Omnix.DataStructures.ReadOnlyListSlim<ProtocolVersion>(protocolVersions);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in protocolVersions)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<ProtocolVersion> ProtocolVersions { get; }

        public static HelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(HelloMessage? left, HelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(HelloMessage? left, HelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is HelloMessage)) return false;
            return this.Equals((HelloMessage)other);
        }
        public bool Equals(HelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ProtocolVersions, target.ProtocolVersions)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<HelloMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in HelloMessage value, in int rank)
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

            public HelloMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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

    internal sealed partial class ProfileMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<ProfileMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ProfileMessage> Formatter { get; }
        public static ProfileMessage Empty { get; }

        static ProfileMessage()
        {
            ProfileMessage.Formatter = new ___CustomFormatter();
            ProfileMessage.Empty = new ProfileMessage(global::System.ReadOnlyMemory<byte>.Empty, OmniAddress.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxIdLength = 32;

        public ProfileMessage(global::System.ReadOnlyMemory<byte> id, OmniAddress address)
        {
            if (id.Length > 32) throw new global::System.ArgumentOutOfRangeException("id");
            if (address is null) throw new global::System.ArgumentNullException("address");

            this.Id = id;
            this.Address = address;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (!id.IsEmpty) ___h.Add(global::Omnix.Base.Helpers.ObjectHelper.GetHashCode(id.Span));
                if (address != default) ___h.Add(address.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public global::System.ReadOnlyMemory<byte> Id { get; }
        public OmniAddress Address { get; }

        public static ProfileMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ProfileMessage? left, ProfileMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ProfileMessage? left, ProfileMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ProfileMessage)) return false;
            return this.Equals((ProfileMessage)other);
        }
        public bool Equals(ProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.BytesOperations.SequenceEqual(this.Id.Span, target.Id.Span)) return false;
            if (this.Address != target.Address) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ProfileMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in ProfileMessage value, in int rank)
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
                    OmniAddress.Formatter.Serialize(ref w, value.Address, rank + 1);
                }
            }

            public ProfileMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_address = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new ProfileMessage(p_id, p_address);
            }
        }
    }

    internal sealed partial class NodeAddressesMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<NodeAddressesMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<NodeAddressesMessage> Formatter { get; }
        public static NodeAddressesMessage Empty { get; }

        static NodeAddressesMessage()
        {
            NodeAddressesMessage.Formatter = new ___CustomFormatter();
            NodeAddressesMessage.Empty = new NodeAddressesMessage(global::System.Array.Empty<OmniAddress>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in addresses)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniAddress> Addresses { get; }

        public static NodeAddressesMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeAddressesMessage? left, NodeAddressesMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeAddressesMessage? left, NodeAddressesMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeAddressesMessage)) return false;
            return this.Equals((NodeAddressesMessage)other);
        }
        public bool Equals(NodeAddressesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<NodeAddressesMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in NodeAddressesMessage value, in int rank)
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
                        OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public NodeAddressesMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    p_addresses[i] = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new NodeAddressesMessage(p_addresses);
            }
        }
    }

    internal sealed partial class RelayOption : global::Omnix.Serialization.RocketPack.IRocketPackMessage<RelayOption>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<RelayOption> Formatter { get; }
        public static RelayOption Empty { get; }

        static RelayOption()
        {
            RelayOption.Formatter = new ___CustomFormatter();
            RelayOption.Empty = new RelayOption(0, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public RelayOption(byte hopLimit, byte priority)
        {
            this.HopLimit = hopLimit;
            this.Priority = priority;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (hopLimit != default) ___h.Add(hopLimit.GetHashCode());
                if (priority != default) ___h.Add(priority.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public byte HopLimit { get; }
        public byte Priority { get; }

        public static RelayOption Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(RelayOption? left, RelayOption? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(RelayOption? left, RelayOption? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is RelayOption)) return false;
            return this.Equals((RelayOption)other);
        }
        public bool Equals(RelayOption? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.HopLimit != target.HopLimit) return false;
            if (this.Priority != target.Priority) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<RelayOption>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in RelayOption value, in int rank)
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

            public RelayOption Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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

    internal sealed partial class WantBroadcastCluesMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<WantBroadcastCluesMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBroadcastCluesMessage> Formatter { get; }
        public static WantBroadcastCluesMessage Empty { get; }

        static WantBroadcastCluesMessage()
        {
            WantBroadcastCluesMessage.Formatter = new ___CustomFormatter();
            WantBroadcastCluesMessage.Empty = new WantBroadcastCluesMessage(new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in parameters)
                {
                    if (n.Key != default) ___h.Add(n.Key.GetHashCode());
                    if (n.Value != default) ___h.Add(n.Value.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption> Parameters { get; }

        public static WantBroadcastCluesMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantBroadcastCluesMessage? left, WantBroadcastCluesMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantBroadcastCluesMessage? left, WantBroadcastCluesMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantBroadcastCluesMessage)) return false;
            return this.Equals((WantBroadcastCluesMessage)other);
        }
        public bool Equals(WantBroadcastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBroadcastCluesMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in WantBroadcastCluesMessage value, in int rank)
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
                        OmniSignature.Formatter.Serialize(ref w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(ref w, n.Value, rank + 1);
                    }
                }
            }

            public WantBroadcastCluesMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    t_key = OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(ref r, rank + 1);
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

    internal sealed partial class WantUnicastCluesMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<WantUnicastCluesMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantUnicastCluesMessage> Formatter { get; }
        public static WantUnicastCluesMessage Empty { get; }

        static WantUnicastCluesMessage()
        {
            WantUnicastCluesMessage.Formatter = new ___CustomFormatter();
            WantUnicastCluesMessage.Empty = new WantUnicastCluesMessage(new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in parameters)
                {
                    if (n.Key != default) ___h.Add(n.Key.GetHashCode());
                    if (n.Value != default) ___h.Add(n.Value.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption> Parameters { get; }

        public static WantUnicastCluesMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantUnicastCluesMessage? left, WantUnicastCluesMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantUnicastCluesMessage? left, WantUnicastCluesMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantUnicastCluesMessage)) return false;
            return this.Equals((WantUnicastCluesMessage)other);
        }
        public bool Equals(WantUnicastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantUnicastCluesMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in WantUnicastCluesMessage value, in int rank)
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
                        OmniSignature.Formatter.Serialize(ref w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(ref w, n.Value, rank + 1);
                    }
                }
            }

            public WantUnicastCluesMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    t_key = OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(ref r, rank + 1);
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

    internal sealed partial class WantMulticastCluesMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<WantMulticastCluesMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantMulticastCluesMessage> Formatter { get; }
        public static WantMulticastCluesMessage Empty { get; }

        static WantMulticastCluesMessage()
        {
            WantMulticastCluesMessage.Formatter = new ___CustomFormatter();
            WantMulticastCluesMessage.Empty = new WantMulticastCluesMessage(new global::System.Collections.Generic.Dictionary<OmniSignature, RelayOption>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in parameters)
                {
                    if (n.Key != default) ___h.Add(n.Key.GetHashCode());
                    if (n.Value != default) ___h.Add(n.Value.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniSignature, RelayOption> Parameters { get; }

        public static WantMulticastCluesMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantMulticastCluesMessage? left, WantMulticastCluesMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantMulticastCluesMessage? left, WantMulticastCluesMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantMulticastCluesMessage)) return false;
            return this.Equals((WantMulticastCluesMessage)other);
        }
        public bool Equals(WantMulticastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantMulticastCluesMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in WantMulticastCluesMessage value, in int rank)
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
                        OmniSignature.Formatter.Serialize(ref w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(ref w, n.Value, rank + 1);
                    }
                }
            }

            public WantMulticastCluesMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    t_key = OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(ref r, rank + 1);
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

    internal sealed partial class BroadcastCluesMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<BroadcastCluesMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastCluesMessage> Formatter { get; }
        public static BroadcastCluesMessage Empty { get; }

        static BroadcastCluesMessage()
        {
            BroadcastCluesMessage.Formatter = new ___CustomFormatter();
            BroadcastCluesMessage.Empty = new BroadcastCluesMessage(global::System.Array.Empty<BroadcastClue>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in results)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<BroadcastClue> Results { get; }

        public static BroadcastCluesMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BroadcastCluesMessage? left, BroadcastCluesMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BroadcastCluesMessage? left, BroadcastCluesMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BroadcastCluesMessage)) return false;
            return this.Equals((BroadcastCluesMessage)other);
        }
        public bool Equals(BroadcastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BroadcastCluesMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in BroadcastCluesMessage value, in int rank)
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
                        BroadcastClue.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public BroadcastCluesMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    p_results[i] = BroadcastClue.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new BroadcastCluesMessage(p_results);
            }
        }
    }

    internal sealed partial class UnicastCluesMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<UnicastCluesMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastCluesMessage> Formatter { get; }
        public static UnicastCluesMessage Empty { get; }

        static UnicastCluesMessage()
        {
            UnicastCluesMessage.Formatter = new ___CustomFormatter();
            UnicastCluesMessage.Empty = new UnicastCluesMessage(global::System.Array.Empty<UnicastClue>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in results)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<UnicastClue> Results { get; }

        public static UnicastCluesMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(UnicastCluesMessage? left, UnicastCluesMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(UnicastCluesMessage? left, UnicastCluesMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is UnicastCluesMessage)) return false;
            return this.Equals((UnicastCluesMessage)other);
        }
        public bool Equals(UnicastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<UnicastCluesMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in UnicastCluesMessage value, in int rank)
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
                        UnicastClue.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public UnicastCluesMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    p_results[i] = UnicastClue.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new UnicastCluesMessage(p_results);
            }
        }
    }

    internal sealed partial class MulticastCluesMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<MulticastCluesMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastCluesMessage> Formatter { get; }
        public static MulticastCluesMessage Empty { get; }

        static MulticastCluesMessage()
        {
            MulticastCluesMessage.Formatter = new ___CustomFormatter();
            MulticastCluesMessage.Empty = new MulticastCluesMessage(global::System.Array.Empty<MulticastClue>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in results)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyListSlim<MulticastClue> Results { get; }

        public static MulticastCluesMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(MulticastCluesMessage? left, MulticastCluesMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(MulticastCluesMessage? left, MulticastCluesMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is MulticastCluesMessage)) return false;
            return this.Equals((MulticastCluesMessage)other);
        }
        public bool Equals(MulticastCluesMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Results, target.Results)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<MulticastCluesMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in MulticastCluesMessage value, in int rank)
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
                        MulticastClue.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public MulticastCluesMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    p_results[i] = MulticastClue.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new MulticastCluesMessage(p_results);
            }
        }
    }

    internal sealed partial class PublishBlocksMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<PublishBlocksMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<PublishBlocksMessage> Formatter { get; }
        public static PublishBlocksMessage Empty { get; }

        static PublishBlocksMessage()
        {
            PublishBlocksMessage.Formatter = new ___CustomFormatter();
            PublishBlocksMessage.Empty = new PublishBlocksMessage(new global::System.Collections.Generic.Dictionary<OmniHash, RelayOption>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in parameters)
                {
                    if (n.Key != default) ___h.Add(n.Key.GetHashCode());
                    if (n.Value != default) ___h.Add(n.Value.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniHash, RelayOption> Parameters { get; }

        public static PublishBlocksMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishBlocksMessage? left, PublishBlocksMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishBlocksMessage? left, PublishBlocksMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishBlocksMessage)) return false;
            return this.Equals((PublishBlocksMessage)other);
        }
        public bool Equals(PublishBlocksMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<PublishBlocksMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in PublishBlocksMessage value, in int rank)
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
                        OmniHash.Formatter.Serialize(ref w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(ref w, n.Value, rank + 1);
                    }
                }
            }

            public PublishBlocksMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    t_key = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(ref r, rank + 1);
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

    internal sealed partial class WantBlocksMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<WantBlocksMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBlocksMessage> Formatter { get; }
        public static WantBlocksMessage Empty { get; }

        static WantBlocksMessage()
        {
            WantBlocksMessage.Formatter = new ___CustomFormatter();
            WantBlocksMessage.Empty = new WantBlocksMessage(new global::System.Collections.Generic.Dictionary<OmniHash, RelayOption>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

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

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in parameters)
                {
                    if (n.Key != default) ___h.Add(n.Key.GetHashCode());
                    if (n.Value != default) ___h.Add(n.Value.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.DataStructures.ReadOnlyDictionarySlim<OmniHash, RelayOption> Parameters { get; }

        public static WantBlocksMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantBlocksMessage? left, WantBlocksMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantBlocksMessage? left, WantBlocksMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantBlocksMessage)) return false;
            return this.Equals((WantBlocksMessage)other);
        }
        public bool Equals(WantBlocksMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.Parameters, target.Parameters)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<WantBlocksMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in WantBlocksMessage value, in int rank)
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
                        OmniHash.Formatter.Serialize(ref w, n.Key, rank + 1);
                        RelayOption.Formatter.Serialize(ref w, n.Value, rank + 1);
                    }
                }
            }

            public WantBlocksMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    t_key = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                    t_value = RelayOption.Formatter.Deserialize(ref r, rank + 1);
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

    internal sealed partial class DiffuseBlockMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<DiffuseBlockMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DiffuseBlockMessage> Formatter { get; }
        public static DiffuseBlockMessage Empty { get; }

        static DiffuseBlockMessage()
        {
            DiffuseBlockMessage.Formatter = new ___CustomFormatter();
            DiffuseBlockMessage.Empty = new DiffuseBlockMessage(OmniHash.Empty, RelayOption.Empty, global::System.ReadOnlyMemory<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 4194304;

        public DiffuseBlockMessage(OmniHash hash, RelayOption relayOption, global::System.ReadOnlyMemory<byte> value)
        {
            if (relayOption is null) throw new global::System.ArgumentNullException("relayOption");
            if (value.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("value");

            this.Hash = hash;
            this.RelayOption = relayOption;
            this.Value = value;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (hash != default) ___h.Add(hash.GetHashCode());
                if (relayOption != default) ___h.Add(relayOption.GetHashCode());
                if (!value.IsEmpty) ___h.Add(global::Omnix.Base.Helpers.ObjectHelper.GetHashCode(value.Span));
                return ___h.ToHashCode();
            });
        }

        public OmniHash Hash { get; }
        public RelayOption RelayOption { get; }
        public global::System.ReadOnlyMemory<byte> Value { get; }

        public static DiffuseBlockMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(DiffuseBlockMessage? left, DiffuseBlockMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(DiffuseBlockMessage? left, DiffuseBlockMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is DiffuseBlockMessage)) return false;
            return this.Equals((DiffuseBlockMessage)other);
        }
        public bool Equals(DiffuseBlockMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.RelayOption != target.RelayOption) return false;
            if (!global::Omnix.Base.BytesOperations.SequenceEqual(this.Value.Span, target.Value.Span)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DiffuseBlockMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in DiffuseBlockMessage value, in int rank)
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
                    OmniHash.Formatter.Serialize(ref w, value.Hash, rank + 1);
                }
                if (value.RelayOption != RelayOption.Empty)
                {
                    w.Write((uint)1);
                    RelayOption.Formatter.Serialize(ref w, value.RelayOption, rank + 1);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)2);
                    w.Write(value.Value.Span);
                }
            }

            public DiffuseBlockMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                RelayOption p_relayOption = RelayOption.Empty;
                global::System.ReadOnlyMemory<byte> p_value = global::System.ReadOnlyMemory<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_hash = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_relayOption = RelayOption.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_value = r.GetMemory(4194304);
                                break;
                            }
                    }
                }

                return new DiffuseBlockMessage(p_hash, p_relayOption, p_value);
            }
        }
    }

    internal sealed partial class BlockMessage : global::Omnix.Serialization.RocketPack.IRocketPackMessage<BlockMessage>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BlockMessage> Formatter { get; }
        public static BlockMessage Empty { get; }

        static BlockMessage()
        {
            BlockMessage.Formatter = new ___CustomFormatter();
            BlockMessage.Empty = new BlockMessage(OmniHash.Empty, global::System.ReadOnlyMemory<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 4194304;

        public BlockMessage(OmniHash hash, global::System.ReadOnlyMemory<byte> value)
        {
            if (value.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("value");

            this.Hash = hash;
            this.Value = value;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (hash != default) ___h.Add(hash.GetHashCode());
                if (!value.IsEmpty) ___h.Add(global::Omnix.Base.Helpers.ObjectHelper.GetHashCode(value.Span));
                return ___h.ToHashCode();
            });
        }

        public OmniHash Hash { get; }
        public global::System.ReadOnlyMemory<byte> Value { get; }

        public static BlockMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(BlockMessage? left, BlockMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(BlockMessage? left, BlockMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is BlockMessage)) return false;
            return this.Equals((BlockMessage)other);
        }
        public bool Equals(BlockMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (!global::Omnix.Base.BytesOperations.SequenceEqual(this.Value.Span, target.Value.Span)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<BlockMessage>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in BlockMessage value, in int rank)
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
                    OmniHash.Formatter.Serialize(ref w, value.Hash, rank + 1);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.Value.Span);
                }
            }

            public BlockMessage Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                global::System.ReadOnlyMemory<byte> p_value = global::System.ReadOnlyMemory<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_hash = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_value = r.GetMemory(4194304);
                                break;
                            }
                    }
                }

                return new BlockMessage(p_hash, p_value);
            }
        }
    }

    internal sealed partial class DiffuseBlockInfo : global::Omnix.Serialization.RocketPack.IRocketPackMessage<DiffuseBlockInfo>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DiffuseBlockInfo> Formatter { get; }
        public static DiffuseBlockInfo Empty { get; }

        static DiffuseBlockInfo()
        {
            DiffuseBlockInfo.Formatter = new ___CustomFormatter();
            DiffuseBlockInfo.Empty = new DiffuseBlockInfo(global::Omnix.Serialization.RocketPack.Timestamp.Zero, OmniHash.Empty, RelayOption.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public DiffuseBlockInfo(global::Omnix.Serialization.RocketPack.Timestamp creationTime, OmniHash hash, RelayOption relayOption)
        {
            if (relayOption is null) throw new global::System.ArgumentNullException("relayOption");

            this.CreationTime = creationTime;
            this.Hash = hash;
            this.RelayOption = relayOption;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (creationTime != default) ___h.Add(creationTime.GetHashCode());
                if (hash != default) ___h.Add(hash.GetHashCode());
                if (relayOption != default) ___h.Add(relayOption.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public OmniHash Hash { get; }
        public RelayOption RelayOption { get; }

        public static DiffuseBlockInfo Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(DiffuseBlockInfo? left, DiffuseBlockInfo? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(DiffuseBlockInfo? left, DiffuseBlockInfo? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is DiffuseBlockInfo)) return false;
            return this.Equals((DiffuseBlockInfo)other);
        }
        public bool Equals(DiffuseBlockInfo? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Hash != target.Hash) return false;
            if (this.RelayOption != target.RelayOption) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<DiffuseBlockInfo>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in DiffuseBlockInfo value, in int rank)
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
                    OmniHash.Formatter.Serialize(ref w, value.Hash, rank + 1);
                }
                if (value.RelayOption != RelayOption.Empty)
                {
                    w.Write((uint)2);
                    RelayOption.Formatter.Serialize(ref w, value.RelayOption, rank + 1);
                }
            }

            public DiffuseBlockInfo Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_hash = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_relayOption = RelayOption.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new DiffuseBlockInfo(p_creationTime, p_hash, p_relayOption);
            }
        }
    }

    internal sealed partial class ExchangeManagerConfig : global::Omnix.Serialization.RocketPack.IRocketPackMessage<ExchangeManagerConfig>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ExchangeManagerConfig> Formatter { get; }
        public static ExchangeManagerConfig Empty { get; }

        static ExchangeManagerConfig()
        {
            ExchangeManagerConfig.Formatter = new ___CustomFormatter();
            ExchangeManagerConfig.Empty = new ExchangeManagerConfig(0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ExchangeManagerConfig(uint version)
        {
            this.Version = version;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (version != default) ___h.Add(version.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint Version { get; }

        public static ExchangeManagerConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ExchangeManagerConfig? left, ExchangeManagerConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ExchangeManagerConfig? left, ExchangeManagerConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ExchangeManagerConfig)) return false;
            return this.Equals((ExchangeManagerConfig)other);
        }
        public bool Equals(ExchangeManagerConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ExchangeManagerConfig>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in ExchangeManagerConfig value, in int rank)
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

            public ExchangeManagerConfig Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
