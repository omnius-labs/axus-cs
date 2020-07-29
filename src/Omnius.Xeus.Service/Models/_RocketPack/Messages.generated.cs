using Omnius.Core.Cryptography;
using Omnius.Core.Network;

#nullable enable

namespace Omnius.Xeus.Service.Models
{
    public enum ConnectionType : byte
    {
        Unknown = 0,
        Connected = 1,
        Accepted = 2,
    }

    public enum TcpProxyType : byte
    {
        Unknown = 0,
        HttpProxy = 1,
        Socks5Proxy = 2,
    }

    public sealed partial class NodeProfile : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeProfile>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.NodeProfile> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeProfile>.Formatter;
        public static global::Omnius.Xeus.Service.Models.NodeProfile Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeProfile>.Empty;

        static NodeProfile()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeProfile>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeProfile>.Empty = new global::Omnius.Xeus.Service.Models.NodeProfile(global::System.Array.Empty<string>(), global::System.Array.Empty<OmniAddress>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxServicesCount = 32;
        public static readonly int MaxAddressesCount = 32;

        public NodeProfile(string[] services, OmniAddress[] addresses)
        {
            if (services is null) throw new global::System.ArgumentNullException("services");
            if (services.Length > 32) throw new global::System.ArgumentOutOfRangeException("services");
            foreach (var n in services)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
                if (n.Length > 256) throw new global::System.ArgumentOutOfRangeException("n");
            }
            if (addresses is null) throw new global::System.ArgumentNullException("addresses");
            if (addresses.Length > 32) throw new global::System.ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Services = new global::Omnius.Core.Collections.ReadOnlyListSlim<string>(services);
            this.Addresses = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress>(addresses);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in services)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in addresses)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<string> Services { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress> Addresses { get; }

        public static global::Omnius.Xeus.Service.Models.NodeProfile Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.NodeProfile? left, global::Omnius.Xeus.Service.Models.NodeProfile? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.NodeProfile? left, global::Omnius.Xeus.Service.Models.NodeProfile? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.NodeProfile)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.NodeProfile)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.NodeProfile? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Services, target.Services)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.NodeProfile>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.NodeProfile value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Services.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Addresses.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Services.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Services.Count);
                    foreach (var n in value.Services)
                    {
                        w.Write(n);
                    }
                }
                if (value.Addresses.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.Addresses.Count);
                    foreach (var n in value.Addresses)
                    {
                        global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Models.NodeProfile Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string[] p_services = global::System.Array.Empty<string>();
                OmniAddress[] p_addresses = global::System.Array.Empty<OmniAddress>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                var length = r.GetUInt32();
                                p_services = new string[length];
                                for (int i = 0; i < p_services.Length; i++)
                                {
                                    p_services[i] = r.GetString(256);
                                }
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_addresses = new OmniAddress[length];
                                for (int i = 0; i < p_addresses.Length; i++)
                                {
                                    p_addresses[i] = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.NodeProfile(p_services, p_addresses);
            }
        }
    }

    public sealed partial class ResourceTag : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ResourceTag>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ResourceTag> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ResourceTag>.Formatter;
        public static global::Omnius.Xeus.Service.Models.ResourceTag Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ResourceTag>.Empty;

        static ResourceTag()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ResourceTag>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ResourceTag>.Empty = new global::Omnius.Xeus.Service.Models.ResourceTag(string.Empty, OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxTypeLength = 256;

        public ResourceTag(string type, OmniHash hash)
        {
            if (type is null) throw new global::System.ArgumentNullException("type");
            if (type.Length > 256) throw new global::System.ArgumentOutOfRangeException("type");
            this.Type = type;
            this.Hash = hash;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (hash != default) ___h.Add(hash.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string Type { get; }
        public OmniHash Hash { get; }

        public static global::Omnius.Xeus.Service.Models.ResourceTag Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.ResourceTag? left, global::Omnius.Xeus.Service.Models.ResourceTag? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.ResourceTag? left, global::Omnius.Xeus.Service.Models.ResourceTag? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.ResourceTag)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.ResourceTag)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.ResourceTag? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Hash != target.Hash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ResourceTag>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.ResourceTag value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Hash != OmniHash.Empty)
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
                if (value.Hash != OmniHash.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Hash, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Models.ResourceTag Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_type = string.Empty;
                OmniHash p_hash = OmniHash.Empty;

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
                                p_hash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.ResourceTag(p_type, p_hash);
            }
        }
    }

    public sealed partial class ContentBlock : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentBlock>, global::System.IDisposable
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ContentBlock> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentBlock>.Formatter;
        public static global::Omnius.Xeus.Service.Models.ContentBlock Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentBlock>.Empty;

        static ContentBlock()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentBlock>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentBlock>.Empty = new global::Omnius.Xeus.Service.Models.ContentBlock(OmniHash.Empty, global::Omnius.Core.MemoryOwner<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 4194304;

        public ContentBlock(OmniHash resourceTag, global::System.Buffers.IMemoryOwner<byte> value)
        {
            if (value is null) throw new global::System.ArgumentNullException("value");
            if (value.Memory.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("value");

            this.ResourceTag = resourceTag;
            _value = value;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (resourceTag != default) ___h.Add(resourceTag.GetHashCode());
                if (!value.Memory.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(value.Memory.Span));
                return ___h.ToHashCode();
            });
        }

        public OmniHash ResourceTag { get; }
        private readonly global::System.Buffers.IMemoryOwner<byte> _value;
        public global::System.ReadOnlyMemory<byte> Value => _value.Memory;

        public static global::Omnius.Xeus.Service.Models.ContentBlock Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.ContentBlock? left, global::Omnius.Xeus.Service.Models.ContentBlock? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.ContentBlock? left, global::Omnius.Xeus.Service.Models.ContentBlock? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.ContentBlock)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.ContentBlock)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.ContentBlock? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ResourceTag != target.ResourceTag) return false;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Value.Span, target.Value.Span)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ContentBlock>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.ContentBlock value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ResourceTag != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (!value.Value.IsEmpty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ResourceTag != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.ResourceTag, rank + 1);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.Value.Span);
                }
            }

            public global::Omnius.Xeus.Service.Models.ContentBlock Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_resourceTag = OmniHash.Empty;
                global::System.Buffers.IMemoryOwner<byte> p_value = global::Omnius.Core.MemoryOwner<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_resourceTag = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_value = r.GetRecyclableMemory(4194304);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.ContentBlock(p_resourceTag, p_value);
            }
        }
    }

    public sealed partial class DeclaredMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessage>, global::System.IDisposable
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.DeclaredMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Models.DeclaredMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessage>.Empty;

        static DeclaredMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessage>.Empty = new global::Omnius.Xeus.Service.Models.DeclaredMessage(global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero, global::Omnius.Core.MemoryOwner<byte>.Empty, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 33554432;

        public DeclaredMessage(global::Omnius.Core.Serialization.RocketPack.Timestamp creationTime, global::System.Buffers.IMemoryOwner<byte> value, OmniCertificate? certificate)
        {
            if (value is null) throw new global::System.ArgumentNullException("value");
            if (value.Memory.Length > 33554432) throw new global::System.ArgumentOutOfRangeException("value");
            this.CreationTime = creationTime;
            _value = value;
            this.Certificate = certificate;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (creationTime != default) ___h.Add(creationTime.GetHashCode());
                if (!value.Memory.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(value.Memory.Span));
                if (certificate != default) ___h.Add(certificate.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Serialization.RocketPack.Timestamp CreationTime { get; }
        private readonly global::System.Buffers.IMemoryOwner<byte> _value;
        public global::System.ReadOnlyMemory<byte> Value => _value.Memory;
        public OmniCertificate? Certificate { get; }

        public static global::Omnius.Xeus.Service.Models.DeclaredMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.DeclaredMessage? left, global::Omnius.Xeus.Service.Models.DeclaredMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.DeclaredMessage? left, global::Omnius.Xeus.Service.Models.DeclaredMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.DeclaredMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.DeclaredMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.DeclaredMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Value.Span, target.Value.Span)) return false;
            if ((this.Certificate is null) != (target.Certificate is null)) return false;
            if (!(this.Certificate is null) && !(target.Certificate is null) && this.Certificate != target.Certificate) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.DeclaredMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.DeclaredMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CreationTime != global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (!value.Value.IsEmpty)
                    {
                        propertyCount++;
                    }
                    if (value.Certificate != null)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.CreationTime != global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)0);
                    w.Write(value.CreationTime);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.Value.Span);
                }
                if (value.Certificate != null)
                {
                    w.Write((uint)2);
                    global::Omnius.Core.Cryptography.OmniCertificate.Formatter.Serialize(ref w, value.Certificate, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Models.DeclaredMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::Omnius.Core.Serialization.RocketPack.Timestamp p_creationTime = global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero;
                global::System.Buffers.IMemoryOwner<byte> p_value = global::Omnius.Core.MemoryOwner<byte>.Empty;
                OmniCertificate? p_certificate = null;

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
                                p_value = r.GetRecyclableMemory(33554432);
                                break;
                            }
                        case 2:
                            {
                                p_certificate = global::Omnius.Core.Cryptography.OmniCertificate.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.DeclaredMessage(p_creationTime, p_value, p_certificate);
            }
        }
    }

    public sealed partial class ConsistencyReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConsistencyReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ConsistencyReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConsistencyReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.ConsistencyReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConsistencyReport>.Empty;

        static ConsistencyReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConsistencyReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConsistencyReport>.Empty = new global::Omnius.Xeus.Service.Models.ConsistencyReport(0, 0, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ConsistencyReport(uint badBlockCount, uint checkedBlockCount, uint totalBlockCount)
        {
            this.BadBlockCount = badBlockCount;
            this.CheckedBlockCount = checkedBlockCount;
            this.TotalBlockCount = totalBlockCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (badBlockCount != default) ___h.Add(badBlockCount.GetHashCode());
                if (checkedBlockCount != default) ___h.Add(checkedBlockCount.GetHashCode());
                if (totalBlockCount != default) ___h.Add(totalBlockCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint BadBlockCount { get; }
        public uint CheckedBlockCount { get; }
        public uint TotalBlockCount { get; }

        public static global::Omnius.Xeus.Service.Models.ConsistencyReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.ConsistencyReport? left, global::Omnius.Xeus.Service.Models.ConsistencyReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.ConsistencyReport? left, global::Omnius.Xeus.Service.Models.ConsistencyReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.ConsistencyReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.ConsistencyReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.ConsistencyReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ConsistencyReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.ConsistencyReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.BadBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.CheckedBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.TotalBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.BadBlockCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.BadBlockCount);
                }
                if (value.CheckedBlockCount != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.CheckedBlockCount);
                }
                if (value.TotalBlockCount != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.TotalBlockCount);
                }
            }

            public global::Omnius.Xeus.Service.Models.ConsistencyReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_badBlockCount = 0;
                uint p_checkedBlockCount = 0;
                uint p_totalBlockCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_badBlockCount = r.GetUInt32();
                                break;
                            }
                        case 1:
                            {
                                p_checkedBlockCount = r.GetUInt32();
                                break;
                            }
                        case 2:
                            {
                                p_totalBlockCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.ConsistencyReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }

    public sealed partial class ConnectionReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConnectionReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ConnectionReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConnectionReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.ConnectionReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConnectionReport>.Empty;

        static ConnectionReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConnectionReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ConnectionReport>.Empty = new global::Omnius.Xeus.Service.Models.ConnectionReport((ConnectionType)0, OmniAddress.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ConnectionReport(ConnectionType type, OmniAddress endPoint)
        {
            if (endPoint is null) throw new global::System.ArgumentNullException("endPoint");

            this.Type = type;
            this.EndPoint = endPoint;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (endPoint != default) ___h.Add(endPoint.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public ConnectionType Type { get; }
        public OmniAddress EndPoint { get; }

        public static global::Omnius.Xeus.Service.Models.ConnectionReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.ConnectionReport? left, global::Omnius.Xeus.Service.Models.ConnectionReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.ConnectionReport? left, global::Omnius.Xeus.Service.Models.ConnectionReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.ConnectionReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.ConnectionReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.ConnectionReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.EndPoint != target.EndPoint) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ConnectionReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.ConnectionReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != (ConnectionType)0)
                    {
                        propertyCount++;
                    }
                    if (value.EndPoint != OmniAddress.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Type != (ConnectionType)0)
                {
                    w.Write((uint)0);
                    w.Write((ulong)value.Type);
                }
                if (value.EndPoint != OmniAddress.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, value.EndPoint, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Models.ConnectionReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ConnectionType p_type = (ConnectionType)0;
                OmniAddress p_endPoint = OmniAddress.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_type = (ConnectionType)r.GetUInt64();
                                break;
                            }
                        case 1:
                            {
                                p_endPoint = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.ConnectionReport(p_type, p_endPoint);
            }
        }
    }

    public sealed partial class TcpProxyOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpProxyOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpProxyOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpProxyOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.TcpProxyOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpProxyOptions>.Empty;

        static TcpProxyOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpProxyOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpProxyOptions>.Empty = new global::Omnius.Xeus.Service.Models.TcpProxyOptions((TcpProxyType)0, OmniAddress.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpProxyOptions(TcpProxyType type, OmniAddress address)
        {
            if (address is null) throw new global::System.ArgumentNullException("address");

            this.Type = type;
            this.Address = address;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (address != default) ___h.Add(address.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public TcpProxyType Type { get; }
        public OmniAddress Address { get; }

        public static global::Omnius.Xeus.Service.Models.TcpProxyOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.TcpProxyOptions? left, global::Omnius.Xeus.Service.Models.TcpProxyOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.TcpProxyOptions? left, global::Omnius.Xeus.Service.Models.TcpProxyOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.TcpProxyOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.TcpProxyOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.TcpProxyOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Address != target.Address) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpProxyOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.TcpProxyOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != (TcpProxyType)0)
                    {
                        propertyCount++;
                    }
                    if (value.Address != OmniAddress.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Type != (TcpProxyType)0)
                {
                    w.Write((uint)0);
                    w.Write((ulong)value.Type);
                }
                if (value.Address != OmniAddress.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, value.Address, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Models.TcpProxyOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                TcpProxyType p_type = (TcpProxyType)0;
                OmniAddress p_address = OmniAddress.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_type = (TcpProxyType)r.GetUInt64();
                                break;
                            }
                        case 1:
                            {
                                p_address = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.TcpProxyOptions(p_type, p_address);
            }
        }
    }

    public sealed partial class TcpConnectingOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectingOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpConnectingOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectingOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.TcpConnectingOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectingOptions>.Empty;

        static TcpConnectingOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectingOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectingOptions>.Empty = new global::Omnius.Xeus.Service.Models.TcpConnectingOptions(false, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpConnectingOptions(bool enabled, TcpProxyOptions? proxyOptions)
        {
            this.Enabled = enabled;
            this.ProxyOptions = proxyOptions;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (enabled != default) ___h.Add(enabled.GetHashCode());
                if (proxyOptions != default) ___h.Add(proxyOptions.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public bool Enabled { get; }
        public TcpProxyOptions? ProxyOptions { get; }

        public static global::Omnius.Xeus.Service.Models.TcpConnectingOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.TcpConnectingOptions? left, global::Omnius.Xeus.Service.Models.TcpConnectingOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.TcpConnectingOptions? left, global::Omnius.Xeus.Service.Models.TcpConnectingOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.TcpConnectingOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.TcpConnectingOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.TcpConnectingOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if ((this.ProxyOptions is null) != (target.ProxyOptions is null)) return false;
            if (!(this.ProxyOptions is null) && !(target.ProxyOptions is null) && this.ProxyOptions != target.ProxyOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpConnectingOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.TcpConnectingOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Enabled != false)
                    {
                        propertyCount++;
                    }
                    if (value.ProxyOptions != null)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Enabled != false)
                {
                    w.Write((uint)0);
                    w.Write(value.Enabled);
                }
                if (value.ProxyOptions != null)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Service.Models.TcpProxyOptions.Formatter.Serialize(ref w, value.ProxyOptions, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Models.TcpConnectingOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                bool p_enabled = false;
                TcpProxyOptions? p_proxyOptions = null;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_enabled = r.GetBoolean();
                                break;
                            }
                        case 1:
                            {
                                p_proxyOptions = global::Omnius.Xeus.Service.Models.TcpProxyOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.TcpConnectingOptions(p_enabled, p_proxyOptions);
            }
        }
    }

    public sealed partial class TcpAcceptingOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpAcceptingOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpAcceptingOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpAcceptingOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.TcpAcceptingOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpAcceptingOptions>.Empty;

        static TcpAcceptingOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpAcceptingOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpAcceptingOptions>.Empty = new global::Omnius.Xeus.Service.Models.TcpAcceptingOptions(false, global::System.Array.Empty<OmniAddress>(), false);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxListenAddressesCount = 32;

        public TcpAcceptingOptions(bool enabled, OmniAddress[] listenAddresses, bool useUpnp)
        {
            if (listenAddresses is null) throw new global::System.ArgumentNullException("listenAddresses");
            if (listenAddresses.Length > 32) throw new global::System.ArgumentOutOfRangeException("listenAddresses");
            foreach (var n in listenAddresses)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            this.Enabled = enabled;
            this.ListenAddresses = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress>(listenAddresses);
            this.UseUpnp = useUpnp;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (enabled != default) ___h.Add(enabled.GetHashCode());
                foreach (var n in listenAddresses)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                if (useUpnp != default) ___h.Add(useUpnp.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public bool Enabled { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress> ListenAddresses { get; }
        public bool UseUpnp { get; }

        public static global::Omnius.Xeus.Service.Models.TcpAcceptingOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.TcpAcceptingOptions? left, global::Omnius.Xeus.Service.Models.TcpAcceptingOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.TcpAcceptingOptions? left, global::Omnius.Xeus.Service.Models.TcpAcceptingOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.TcpAcceptingOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.TcpAcceptingOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.TcpAcceptingOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.ListenAddresses, target.ListenAddresses)) return false;
            if (this.UseUpnp != target.UseUpnp) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpAcceptingOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.TcpAcceptingOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Enabled != false)
                    {
                        propertyCount++;
                    }
                    if (value.ListenAddresses.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.UseUpnp != false)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Enabled != false)
                {
                    w.Write((uint)0);
                    w.Write(value.Enabled);
                }
                if (value.ListenAddresses.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.ListenAddresses.Count);
                    foreach (var n in value.ListenAddresses)
                    {
                        global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.UseUpnp != false)
                {
                    w.Write((uint)2);
                    w.Write(value.UseUpnp);
                }
            }

            public global::Omnius.Xeus.Service.Models.TcpAcceptingOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                bool p_enabled = false;
                OmniAddress[] p_listenAddresses = global::System.Array.Empty<OmniAddress>();
                bool p_useUpnp = false;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_enabled = r.GetBoolean();
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_listenAddresses = new OmniAddress[length];
                                for (int i = 0; i < p_listenAddresses.Length; i++)
                                {
                                    p_listenAddresses[i] = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 2:
                            {
                                p_useUpnp = r.GetBoolean();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.TcpAcceptingOptions(p_enabled, p_listenAddresses, p_useUpnp);
            }
        }
    }

    public sealed partial class BandwidthOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.BandwidthOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.BandwidthOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.BandwidthOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.BandwidthOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.BandwidthOptions>.Empty;

        static BandwidthOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.BandwidthOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.BandwidthOptions>.Empty = new global::Omnius.Xeus.Service.Models.BandwidthOptions(0, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public BandwidthOptions(uint maxSendBytesPerSeconds, uint maxReceiveBytesPerSeconds)
        {
            this.MaxSendBytesPerSeconds = maxSendBytesPerSeconds;
            this.MaxReceiveBytesPerSeconds = maxReceiveBytesPerSeconds;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (maxSendBytesPerSeconds != default) ___h.Add(maxSendBytesPerSeconds.GetHashCode());
                if (maxReceiveBytesPerSeconds != default) ___h.Add(maxReceiveBytesPerSeconds.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint MaxSendBytesPerSeconds { get; }
        public uint MaxReceiveBytesPerSeconds { get; }

        public static global::Omnius.Xeus.Service.Models.BandwidthOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.BandwidthOptions? left, global::Omnius.Xeus.Service.Models.BandwidthOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.BandwidthOptions? left, global::Omnius.Xeus.Service.Models.BandwidthOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.BandwidthOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.BandwidthOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.BandwidthOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxSendBytesPerSeconds != target.MaxSendBytesPerSeconds) return false;
            if (this.MaxReceiveBytesPerSeconds != target.MaxReceiveBytesPerSeconds) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.BandwidthOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.BandwidthOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.MaxSendBytesPerSeconds != 0)
                    {
                        propertyCount++;
                    }
                    if (value.MaxReceiveBytesPerSeconds != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.MaxSendBytesPerSeconds != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.MaxSendBytesPerSeconds);
                }
                if (value.MaxReceiveBytesPerSeconds != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.MaxReceiveBytesPerSeconds);
                }
            }

            public global::Omnius.Xeus.Service.Models.BandwidthOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_maxSendBytesPerSeconds = 0;
                uint p_maxReceiveBytesPerSeconds = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_maxSendBytesPerSeconds = r.GetUInt32();
                                break;
                            }
                        case 1:
                            {
                                p_maxReceiveBytesPerSeconds = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.BandwidthOptions(p_maxSendBytesPerSeconds, p_maxReceiveBytesPerSeconds);
            }
        }
    }

    public sealed partial class TcpConnectorOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpConnectorOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.TcpConnectorOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorOptions>.Empty;

        static TcpConnectorOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorOptions>.Empty = new global::Omnius.Xeus.Service.Models.TcpConnectorOptions(TcpConnectingOptions.Empty, TcpAcceptingOptions.Empty, BandwidthOptions.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpConnectorOptions(TcpConnectingOptions tcpConnectingOptions, TcpAcceptingOptions tcpAcceptingOptions, BandwidthOptions bandwidthOptions)
        {
            if (tcpConnectingOptions is null) throw new global::System.ArgumentNullException("tcpConnectingOptions");
            if (tcpAcceptingOptions is null) throw new global::System.ArgumentNullException("tcpAcceptingOptions");
            if (bandwidthOptions is null) throw new global::System.ArgumentNullException("bandwidthOptions");

            this.TcpConnectingOptions = tcpConnectingOptions;
            this.TcpAcceptingOptions = tcpAcceptingOptions;
            this.BandwidthOptions = bandwidthOptions;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tcpConnectingOptions != default) ___h.Add(tcpConnectingOptions.GetHashCode());
                if (tcpAcceptingOptions != default) ___h.Add(tcpAcceptingOptions.GetHashCode());
                if (bandwidthOptions != default) ___h.Add(bandwidthOptions.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public TcpConnectingOptions TcpConnectingOptions { get; }
        public TcpAcceptingOptions TcpAcceptingOptions { get; }
        public BandwidthOptions BandwidthOptions { get; }

        public static global::Omnius.Xeus.Service.Models.TcpConnectorOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.TcpConnectorOptions? left, global::Omnius.Xeus.Service.Models.TcpConnectorOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.TcpConnectorOptions? left, global::Omnius.Xeus.Service.Models.TcpConnectorOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.TcpConnectorOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.TcpConnectorOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.TcpConnectorOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.TcpConnectingOptions != target.TcpConnectingOptions) return false;
            if (this.TcpAcceptingOptions != target.TcpAcceptingOptions) return false;
            if (this.BandwidthOptions != target.BandwidthOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpConnectorOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.TcpConnectorOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.TcpConnectingOptions != TcpConnectingOptions.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.TcpAcceptingOptions != TcpAcceptingOptions.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.BandwidthOptions != BandwidthOptions.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.TcpConnectingOptions != TcpConnectingOptions.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Xeus.Service.Models.TcpConnectingOptions.Formatter.Serialize(ref w, value.TcpConnectingOptions, rank + 1);
                }
                if (value.TcpAcceptingOptions != TcpAcceptingOptions.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Service.Models.TcpAcceptingOptions.Formatter.Serialize(ref w, value.TcpAcceptingOptions, rank + 1);
                }
                if (value.BandwidthOptions != BandwidthOptions.Empty)
                {
                    w.Write((uint)2);
                    global::Omnius.Xeus.Service.Models.BandwidthOptions.Formatter.Serialize(ref w, value.BandwidthOptions, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Models.TcpConnectorOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                TcpConnectingOptions p_tcpConnectingOptions = TcpConnectingOptions.Empty;
                TcpAcceptingOptions p_tcpAcceptingOptions = TcpAcceptingOptions.Empty;
                BandwidthOptions p_bandwidthOptions = BandwidthOptions.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tcpConnectingOptions = global::Omnius.Xeus.Service.Models.TcpConnectingOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_tcpAcceptingOptions = global::Omnius.Xeus.Service.Models.TcpAcceptingOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_bandwidthOptions = global::Omnius.Xeus.Service.Models.BandwidthOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.TcpConnectorOptions(p_tcpConnectingOptions, p_tcpAcceptingOptions, p_bandwidthOptions);
            }
        }
    }

    public sealed partial class TcpConnectorReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpConnectorReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.TcpConnectorReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorReport>.Empty;

        static TcpConnectorReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.TcpConnectorReport>.Empty = new global::Omnius.Xeus.Service.Models.TcpConnectorReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpConnectorReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Service.Models.TcpConnectorReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.TcpConnectorReport? left, global::Omnius.Xeus.Service.Models.TcpConnectorReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.TcpConnectorReport? left, global::Omnius.Xeus.Service.Models.TcpConnectorReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.TcpConnectorReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.TcpConnectorReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.TcpConnectorReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.TcpConnectorReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.TcpConnectorReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public global::Omnius.Xeus.Service.Models.TcpConnectorReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Service.Models.TcpConnectorReport();
            }
        }
    }

    public sealed partial class NodeFinderOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.NodeFinderOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.NodeFinderOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderOptions>.Empty;

        static NodeFinderOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderOptions>.Empty = new global::Omnius.Xeus.Service.Models.NodeFinderOptions(string.Empty, 0, global::System.Array.Empty<string>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;
        public static readonly int MaxServicesCount = 32;

        public NodeFinderOptions(string configPath, uint maxConnectionCount, string[] services)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");
            if (services is null) throw new global::System.ArgumentNullException("services");
            if (services.Length > 32) throw new global::System.ArgumentOutOfRangeException("services");
            foreach (var n in services)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
                if (n.Length > 256) throw new global::System.ArgumentOutOfRangeException("n");
            }

            this.ConfigPath = configPath;
            this.MaxConnectionCount = maxConnectionCount;
            this.Services = new global::Omnius.Core.Collections.ReadOnlyListSlim<string>(services);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                foreach (var n in services)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }
        public uint MaxConnectionCount { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<string> Services { get; }

        public static global::Omnius.Xeus.Service.Models.NodeFinderOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.NodeFinderOptions? left, global::Omnius.Xeus.Service.Models.NodeFinderOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.NodeFinderOptions? left, global::Omnius.Xeus.Service.Models.NodeFinderOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.NodeFinderOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.NodeFinderOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.NodeFinderOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Services, target.Services)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.NodeFinderOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.NodeFinderOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Services.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.MaxConnectionCount);
                }
                if (value.Services.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.Services.Count);
                    foreach (var n in value.Services)
                    {
                        w.Write(n);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Models.NodeFinderOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;
                uint p_maxConnectionCount = 0;
                string[] p_services = global::System.Array.Empty<string>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                        case 1:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_services = new string[length];
                                for (int i = 0; i < p_services.Length; i++)
                                {
                                    p_services[i] = r.GetString(256);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.NodeFinderOptions(p_configPath, p_maxConnectionCount, p_services);
            }
        }
    }

    public sealed partial class NodeFinderReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.NodeFinderReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.NodeFinderReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderReport>.Empty;

        static NodeFinderReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.NodeFinderReport>.Empty = new global::Omnius.Xeus.Service.Models.NodeFinderReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public NodeFinderReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Service.Models.NodeFinderReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.NodeFinderReport? left, global::Omnius.Xeus.Service.Models.NodeFinderReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.NodeFinderReport? left, global::Omnius.Xeus.Service.Models.NodeFinderReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.NodeFinderReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.NodeFinderReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.NodeFinderReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.NodeFinderReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.NodeFinderReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public global::Omnius.Xeus.Service.Models.NodeFinderReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Service.Models.NodeFinderReport();
            }
        }
    }

    public sealed partial class ContentExchangerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ContentExchangerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.ContentExchangerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerOptions>.Empty;

        static ContentExchangerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerOptions>.Empty = new global::Omnius.Xeus.Service.Models.ContentExchangerOptions(string.Empty, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public ContentExchangerOptions(string configPath, uint maxConnectionCount)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");
            this.ConfigPath = configPath;
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }
        public uint MaxConnectionCount { get; }

        public static global::Omnius.Xeus.Service.Models.ContentExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.ContentExchangerOptions? left, global::Omnius.Xeus.Service.Models.ContentExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.ContentExchangerOptions? left, global::Omnius.Xeus.Service.Models.ContentExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.ContentExchangerOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.ContentExchangerOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.ContentExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ContentExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.ContentExchangerOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.MaxConnectionCount);
                }
            }

            public global::Omnius.Xeus.Service.Models.ContentExchangerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;
                uint p_maxConnectionCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                        case 1:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.ContentExchangerOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }

    public sealed partial class ContentExchangerReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ContentExchangerReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.ContentExchangerReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerReport>.Empty;

        static ContentExchangerReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.ContentExchangerReport>.Empty = new global::Omnius.Xeus.Service.Models.ContentExchangerReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ContentExchangerReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Service.Models.ContentExchangerReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.ContentExchangerReport? left, global::Omnius.Xeus.Service.Models.ContentExchangerReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.ContentExchangerReport? left, global::Omnius.Xeus.Service.Models.ContentExchangerReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.ContentExchangerReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.ContentExchangerReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.ContentExchangerReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.ContentExchangerReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.ContentExchangerReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public global::Omnius.Xeus.Service.Models.ContentExchangerReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Service.Models.ContentExchangerReport();
            }
        }
    }

    public sealed partial class PushContentStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.PushContentStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.PushContentStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageOptions>.Empty;

        static PushContentStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageOptions>.Empty = new global::Omnius.Xeus.Service.Models.PushContentStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public PushContentStorageOptions(string configPath)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");

            this.ConfigPath = configPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }

        public static global::Omnius.Xeus.Service.Models.PushContentStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.PushContentStorageOptions? left, global::Omnius.Xeus.Service.Models.PushContentStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.PushContentStorageOptions? left, global::Omnius.Xeus.Service.Models.PushContentStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.PushContentStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.PushContentStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.PushContentStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.PushContentStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.PushContentStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
            }

            public global::Omnius.Xeus.Service.Models.PushContentStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.PushContentStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class PushContentStorageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.PushContentStorageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.PushContentStorageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageReport>.Empty;

        static PushContentStorageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushContentStorageReport>.Empty = new global::Omnius.Xeus.Service.Models.PushContentStorageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PushContentStorageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Service.Models.PushContentStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.PushContentStorageReport? left, global::Omnius.Xeus.Service.Models.PushContentStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.PushContentStorageReport? left, global::Omnius.Xeus.Service.Models.PushContentStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.PushContentStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.PushContentStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.PushContentStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.PushContentStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.PushContentStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public global::Omnius.Xeus.Service.Models.PushContentStorageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Service.Models.PushContentStorageReport();
            }
        }
    }

    public sealed partial class WantContentStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.WantContentStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.WantContentStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageOptions>.Empty;

        static WantContentStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageOptions>.Empty = new global::Omnius.Xeus.Service.Models.WantContentStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public WantContentStorageOptions(string configPath)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");

            this.ConfigPath = configPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }

        public static global::Omnius.Xeus.Service.Models.WantContentStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.WantContentStorageOptions? left, global::Omnius.Xeus.Service.Models.WantContentStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.WantContentStorageOptions? left, global::Omnius.Xeus.Service.Models.WantContentStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.WantContentStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.WantContentStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.WantContentStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.WantContentStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.WantContentStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
            }

            public global::Omnius.Xeus.Service.Models.WantContentStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.WantContentStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class WantContentStorageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.WantContentStorageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.WantContentStorageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageReport>.Empty;

        static WantContentStorageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantContentStorageReport>.Empty = new global::Omnius.Xeus.Service.Models.WantContentStorageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantContentStorageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Service.Models.WantContentStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.WantContentStorageReport? left, global::Omnius.Xeus.Service.Models.WantContentStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.WantContentStorageReport? left, global::Omnius.Xeus.Service.Models.WantContentStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.WantContentStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.WantContentStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.WantContentStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.WantContentStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.WantContentStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public global::Omnius.Xeus.Service.Models.WantContentStorageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Service.Models.WantContentStorageReport();
            }
        }
    }

    public sealed partial class DeclaredMessageExchangerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions>.Empty;

        static DeclaredMessageExchangerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions>.Empty = new global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions(string.Empty, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public DeclaredMessageExchangerOptions(string configPath, uint maxConnectionCount)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");
            this.ConfigPath = configPath;
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }
        public uint MaxConnectionCount { get; }

        public static global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions? left, global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions? left, global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.MaxConnectionCount);
                }
            }

            public global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;
                uint p_maxConnectionCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                        case 1:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }

    public sealed partial class DeclaredMessageExchangerReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport>.Empty;

        static DeclaredMessageExchangerReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport>.Empty = new global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public DeclaredMessageExchangerReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport? left, global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport? left, global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Service.Models.DeclaredMessageExchangerReport();
            }
        }
    }

    public sealed partial class PushDeclaredMessageStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions>.Empty;

        static PushDeclaredMessageStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions>.Empty = new global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public PushDeclaredMessageStorageOptions(string configPath)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");

            this.ConfigPath = configPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }

        public static global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions? left, global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions? left, global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
            }

            public global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class PushDeclaredMessageStorageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport>.Empty;

        static PushDeclaredMessageStorageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport>.Empty = new global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PushDeclaredMessageStorageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport? left, global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport? left, global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Service.Models.PushDeclaredMessageStorageReport();
            }
        }
    }

    public sealed partial class WantDeclaredMessageStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions>.Empty;

        static WantDeclaredMessageStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions>.Empty = new global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public WantDeclaredMessageStorageOptions(string configPath)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");

            this.ConfigPath = configPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }

        public static global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions? left, global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions? left, global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
            }

            public global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class WantDeclaredMessageStorageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport>.Formatter;
        public static global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport>.Empty;

        static WantDeclaredMessageStorageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport>.Empty = new global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantDeclaredMessageStorageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport? left, global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport? left, global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Service.Models.WantDeclaredMessageStorageReport();
            }
        }
    }

}
