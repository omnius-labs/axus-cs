using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Drivers;

#nullable enable

namespace Omnius.Xeus.Service.Engines
{
    public enum ConnectionType : byte
    {
        Unknown = 0,
        Connected = 1,
        Accepted = 2,
    }

    public sealed partial class NodeProfile : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeProfile>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.NodeProfile> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeProfile>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.NodeProfile Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeProfile>.Empty;

        static NodeProfile()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeProfile>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeProfile>.Empty = new global::Omnius.Xeus.Service.Engines.NodeProfile(global::System.Array.Empty<string>(), global::System.Array.Empty<OmniAddress>());
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

        public static global::Omnius.Xeus.Service.Engines.NodeProfile Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.NodeProfile? left, global::Omnius.Xeus.Service.Engines.NodeProfile? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.NodeProfile? left, global::Omnius.Xeus.Service.Engines.NodeProfile? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.NodeProfile)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.NodeProfile)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.NodeProfile? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Services, target.Services)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.NodeProfile>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.NodeProfile value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.NodeProfile Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.NodeProfile(p_services, p_addresses);
            }
        }
    }

    public sealed partial class Tag : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Tag>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Tag> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Tag>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.Tag Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Tag>.Empty;

        static Tag()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Tag>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.Tag>.Empty = new global::Omnius.Xeus.Service.Engines.Tag(string.Empty, OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxTypeLength = 256;

        public Tag(string type, OmniHash hash)
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

        public static global::Omnius.Xeus.Service.Engines.Tag Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.Tag? left, global::Omnius.Xeus.Service.Engines.Tag? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.Tag? left, global::Omnius.Xeus.Service.Engines.Tag? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.Tag)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.Tag)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.Tag? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Hash != target.Hash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.Tag>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.Tag value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.Tag Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.Tag(p_type, p_hash);
            }
        }
    }

    public sealed partial class DeclaredMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.DeclaredMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.DeclaredMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.DeclaredMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.DeclaredMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.DeclaredMessage>.Empty;

        static DeclaredMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.DeclaredMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.DeclaredMessage>.Empty = new global::Omnius.Xeus.Service.Engines.DeclaredMessage(global::System.ReadOnlyMemory<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 4194304;

        public DeclaredMessage(global::System.ReadOnlyMemory<byte> value)
        {
            if (value.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("value");

            this.Value = value;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (!value.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(value.Span));
                return ___h.ToHashCode();
            });
        }

        public global::System.ReadOnlyMemory<byte> Value { get; }

        public static global::Omnius.Xeus.Service.Engines.DeclaredMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.DeclaredMessage? left, global::Omnius.Xeus.Service.Engines.DeclaredMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.DeclaredMessage? left, global::Omnius.Xeus.Service.Engines.DeclaredMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.DeclaredMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.DeclaredMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.DeclaredMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Value.Span, target.Value.Span)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.DeclaredMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.DeclaredMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (!value.Value.IsEmpty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)0);
                    w.Write(value.Value.Span);
                }
            }

            public global::Omnius.Xeus.Service.Engines.DeclaredMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::System.ReadOnlyMemory<byte> p_value = global::System.ReadOnlyMemory<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_value = r.GetMemory(4194304);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.DeclaredMessage(p_value);
            }
        }
    }

    public sealed partial class OrientedMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.OrientedMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.OrientedMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.OrientedMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.OrientedMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.OrientedMessage>.Empty;

        static OrientedMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.OrientedMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.OrientedMessage>.Empty = new global::Omnius.Xeus.Service.Engines.OrientedMessage(Tag.Empty, global::System.ReadOnlyMemory<byte>.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 4194304;

        public OrientedMessage(Tag tag, global::System.ReadOnlyMemory<byte> value)
        {
            if (tag is null) throw new global::System.ArgumentNullException("tag");
            if (value.Length > 4194304) throw new global::System.ArgumentOutOfRangeException("value");

            this.Tag = tag;
            this.Value = value;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tag != default) ___h.Add(tag.GetHashCode());
                if (!value.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(value.Span));
                return ___h.ToHashCode();
            });
        }

        public Tag Tag { get; }
        public global::System.ReadOnlyMemory<byte> Value { get; }

        public static global::Omnius.Xeus.Service.Engines.OrientedMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.OrientedMessage? left, global::Omnius.Xeus.Service.Engines.OrientedMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.OrientedMessage? left, global::Omnius.Xeus.Service.Engines.OrientedMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.OrientedMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.OrientedMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.OrientedMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Value.Span, target.Value.Span)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.OrientedMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.OrientedMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != Tag.Empty)
                    {
                        propertyCount++;
                    }
                    if (!value.Value.IsEmpty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != Tag.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Xeus.Service.Engines.Tag.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)1);
                    w.Write(value.Value.Span);
                }
            }

            public global::Omnius.Xeus.Service.Engines.OrientedMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                Tag p_tag = Tag.Empty;
                global::System.ReadOnlyMemory<byte> p_value = global::System.ReadOnlyMemory<byte>.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = global::Omnius.Xeus.Service.Engines.Tag.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_value = r.GetMemory(4194304);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.OrientedMessage(p_tag, p_value);
            }
        }
    }

    public sealed partial class NodeFinderOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeFinderOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.NodeFinderOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeFinderOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.NodeFinderOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeFinderOptions>.Empty;

        static NodeFinderOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeFinderOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.NodeFinderOptions>.Empty = new global::Omnius.Xeus.Service.Engines.NodeFinderOptions(string.Empty, 0, global::System.Array.Empty<string>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;
        public static readonly int MaxEnabledServicesCount = 32;

        public NodeFinderOptions(string configPath, uint maxConnectionCount, string[] enabledServices)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");
            if (enabledServices is null) throw new global::System.ArgumentNullException("enabledServices");
            if (enabledServices.Length > 32) throw new global::System.ArgumentOutOfRangeException("enabledServices");
            foreach (var n in enabledServices)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
                if (n.Length > 256) throw new global::System.ArgumentOutOfRangeException("n");
            }

            this.ConfigPath = configPath;
            this.MaxConnectionCount = maxConnectionCount;
            this.EnabledServices = new global::Omnius.Core.Collections.ReadOnlyListSlim<string>(enabledServices);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                foreach (var n in enabledServices)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }
        public uint MaxConnectionCount { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<string> EnabledServices { get; }

        public static global::Omnius.Xeus.Service.Engines.NodeFinderOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.NodeFinderOptions? left, global::Omnius.Xeus.Service.Engines.NodeFinderOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.NodeFinderOptions? left, global::Omnius.Xeus.Service.Engines.NodeFinderOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.NodeFinderOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.NodeFinderOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.NodeFinderOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.EnabledServices, target.EnabledServices)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.NodeFinderOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.NodeFinderOptions value, in int rank)
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
                    if (value.EnabledServices.Count != 0)
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
                if (value.EnabledServices.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.EnabledServices.Count);
                    foreach (var n in value.EnabledServices)
                    {
                        w.Write(n);
                    }
                }
            }

            public global::Omnius.Xeus.Service.Engines.NodeFinderOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;
                uint p_maxConnectionCount = 0;
                string[] p_enabledServices = global::System.Array.Empty<string>();

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
                                p_enabledServices = new string[length];
                                for (int i = 0; i < p_enabledServices.Length; i++)
                                {
                                    p_enabledServices[i] = r.GetString(256);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.NodeFinderOptions(p_configPath, p_maxConnectionCount, p_enabledServices);
            }
        }
    }

    public sealed partial class PublishContentStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions>.Empty;

        static PublishContentStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions>.Empty = new global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public PublishContentStorageOptions(string configPath)
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

        public static global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions? left, global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions? left, global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.PublishContentStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class WantContentStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.WantContentStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.WantContentStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageOptions>.Empty;

        static WantContentStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageOptions>.Empty = new global::Omnius.Xeus.Service.Engines.WantContentStorageOptions(string.Empty);
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

        public static global::Omnius.Xeus.Service.Engines.WantContentStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.WantContentStorageOptions? left, global::Omnius.Xeus.Service.Engines.WantContentStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.WantContentStorageOptions? left, global::Omnius.Xeus.Service.Engines.WantContentStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.WantContentStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.WantContentStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.WantContentStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.WantContentStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.WantContentStorageOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.WantContentStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.WantContentStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class ContentExchangerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ContentExchangerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.ContentExchangerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ContentExchangerOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.ContentExchangerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ContentExchangerOptions>.Empty;

        static ContentExchangerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ContentExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ContentExchangerOptions>.Empty = new global::Omnius.Xeus.Service.Engines.ContentExchangerOptions(string.Empty, 0);
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

        public static global::Omnius.Xeus.Service.Engines.ContentExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.ContentExchangerOptions? left, global::Omnius.Xeus.Service.Engines.ContentExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.ContentExchangerOptions? left, global::Omnius.Xeus.Service.Engines.ContentExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.ContentExchangerOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.ContentExchangerOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.ContentExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.ContentExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.ContentExchangerOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.ContentExchangerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.ContentExchangerOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }

    public sealed partial class PublishMessageStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions>.Empty;

        static PublishMessageStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions>.Empty = new global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public PublishMessageStorageOptions(string configPath)
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

        public static global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions? left, global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions? left, global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.PublishMessageStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class WantMessageStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions>.Empty;

        static WantMessageStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions>.Empty = new global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public WantMessageStorageOptions(string configPath)
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

        public static global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions? left, global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions? left, global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.WantMessageStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class MessageExchangerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.MessageExchangerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.MessageExchangerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.MessageExchangerOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.MessageExchangerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.MessageExchangerOptions>.Empty;

        static MessageExchangerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.MessageExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.MessageExchangerOptions>.Empty = new global::Omnius.Xeus.Service.Engines.MessageExchangerOptions(string.Empty, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public MessageExchangerOptions(string configPath, uint maxConnectionCount)
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

        public static global::Omnius.Xeus.Service.Engines.MessageExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.MessageExchangerOptions? left, global::Omnius.Xeus.Service.Engines.MessageExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.MessageExchangerOptions? left, global::Omnius.Xeus.Service.Engines.MessageExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.MessageExchangerOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.MessageExchangerOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.MessageExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.MessageExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.MessageExchangerOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.MessageExchangerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.MessageExchangerOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }

    public sealed partial class ConsistencyReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConsistencyReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.ConsistencyReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConsistencyReport>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.ConsistencyReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConsistencyReport>.Empty;

        static ConsistencyReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConsistencyReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConsistencyReport>.Empty = new global::Omnius.Xeus.Service.Engines.ConsistencyReport(0, 0, 0);
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

        public static global::Omnius.Xeus.Service.Engines.ConsistencyReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.ConsistencyReport? left, global::Omnius.Xeus.Service.Engines.ConsistencyReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.ConsistencyReport? left, global::Omnius.Xeus.Service.Engines.ConsistencyReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.ConsistencyReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.ConsistencyReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.ConsistencyReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.ConsistencyReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.ConsistencyReport value, in int rank)
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

            public global::Omnius.Xeus.Service.Engines.ConsistencyReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Engines.ConsistencyReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }

    public sealed partial class ConnectionReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConnectionReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.ConnectionReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConnectionReport>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.ConnectionReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConnectionReport>.Empty;

        static ConnectionReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConnectionReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.ConnectionReport>.Empty = new global::Omnius.Xeus.Service.Engines.ConnectionReport((ConnectionType)0, OmniAddress.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ConnectionReport(ConnectionType type, OmniAddress endpoint)
        {
            if (endpoint is null) throw new global::System.ArgumentNullException("endpoint");

            this.Type = type;
            this.Endpoint = endpoint;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (endpoint != default) ___h.Add(endpoint.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public ConnectionType Type { get; }
        public OmniAddress Endpoint { get; }

        public static global::Omnius.Xeus.Service.Engines.ConnectionReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.ConnectionReport? left, global::Omnius.Xeus.Service.Engines.ConnectionReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.ConnectionReport? left, global::Omnius.Xeus.Service.Engines.ConnectionReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.ConnectionReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.ConnectionReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.ConnectionReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Endpoint != target.Endpoint) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.ConnectionReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.ConnectionReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Type != (ConnectionType)0)
                    {
                        propertyCount++;
                    }
                    if (value.Endpoint != OmniAddress.Empty)
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
                if (value.Endpoint != OmniAddress.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, value.Endpoint, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.ConnectionReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                ConnectionType p_type = (ConnectionType)0;
                OmniAddress p_endpoint = OmniAddress.Empty;

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
                                p_endpoint = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.ConnectionReport(p_type, p_endpoint);
            }
        }
    }

    public sealed partial class PublishContentStorageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.PublishContentStorageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageReport>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.PublishContentStorageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageReport>.Empty;

        static PublishContentStorageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishContentStorageReport>.Empty = new global::Omnius.Xeus.Service.Engines.PublishContentStorageReport(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PublishContentStorageReport(OmniHash tag)
        {
            this.Tag = tag;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tag != default) ___h.Add(tag.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public OmniHash Tag { get; }

        public static global::Omnius.Xeus.Service.Engines.PublishContentStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.PublishContentStorageReport? left, global::Omnius.Xeus.Service.Engines.PublishContentStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.PublishContentStorageReport? left, global::Omnius.Xeus.Service.Engines.PublishContentStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.PublishContentStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.PublishContentStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.PublishContentStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.PublishContentStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.PublishContentStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.PublishContentStorageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_tag = OmniHash.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.PublishContentStorageReport(p_tag);
            }
        }
    }

    public sealed partial class WantContentStorageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.WantContentStorageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageReport>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.WantContentStorageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageReport>.Empty;

        static WantContentStorageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantContentStorageReport>.Empty = new global::Omnius.Xeus.Service.Engines.WantContentStorageReport(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantContentStorageReport(OmniHash tag)
        {
            this.Tag = tag;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tag != default) ___h.Add(tag.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public OmniHash Tag { get; }

        public static global::Omnius.Xeus.Service.Engines.WantContentStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.WantContentStorageReport? left, global::Omnius.Xeus.Service.Engines.WantContentStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.WantContentStorageReport? left, global::Omnius.Xeus.Service.Engines.WantContentStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.WantContentStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.WantContentStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.WantContentStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.WantContentStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.WantContentStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.WantContentStorageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_tag = OmniHash.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.WantContentStorageReport(p_tag);
            }
        }
    }

    public sealed partial class PublishMessageStorageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport>.Empty;

        static PublishMessageStorageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport>.Empty = new global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PublishMessageStorageReport(OmniHash tag)
        {
            this.Tag = tag;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tag != default) ___h.Add(tag.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public OmniHash Tag { get; }

        public static global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport? left, global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport? left, global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_tag = OmniHash.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.PublishMessageStorageReport(p_tag);
            }
        }
    }

    public sealed partial class WantMessageStorageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.WantMessageStorageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageReport>.Formatter;
        public static global::Omnius.Xeus.Service.Engines.WantMessageStorageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageReport>.Empty;

        static WantMessageStorageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Engines.WantMessageStorageReport>.Empty = new global::Omnius.Xeus.Service.Engines.WantMessageStorageReport(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantMessageStorageReport(OmniHash tag)
        {
            this.Tag = tag;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tag != default) ___h.Add(tag.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public OmniHash Tag { get; }

        public static global::Omnius.Xeus.Service.Engines.WantMessageStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Engines.WantMessageStorageReport? left, global::Omnius.Xeus.Service.Engines.WantMessageStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Engines.WantMessageStorageReport? left, global::Omnius.Xeus.Service.Engines.WantMessageStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Engines.WantMessageStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Engines.WantMessageStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Engines.WantMessageStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Engines.WantMessageStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Engines.WantMessageStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Tag != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Tag != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Engines.WantMessageStorageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_tag = OmniHash.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tag = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Engines.WantMessageStorageReport(p_tag);
            }
        }
    }

}
