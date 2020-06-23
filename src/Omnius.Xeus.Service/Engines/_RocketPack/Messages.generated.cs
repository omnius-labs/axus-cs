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

    public sealed partial class NodeProfile : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeProfile> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Formatter;
        public static NodeProfile Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Empty;

        static NodeProfile()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Empty = new NodeProfile(global::System.Array.Empty<string>(), global::System.Array.Empty<OmniAddress>());
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

        public static NodeProfile Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeProfile? left, NodeProfile? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeProfile? left, NodeProfile? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeProfile)) return false;
            return this.Equals((NodeProfile)other);
        }
        public bool Equals(NodeProfile? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Services, target.Services)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeProfile>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NodeProfile value, in int rank)
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
                        OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public NodeProfile Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                    p_addresses[i] = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new NodeProfile(p_services, p_addresses);
            }
        }
    }

    public sealed partial class Tag : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Tag>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<Tag> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Tag>.Formatter;
        public static Tag Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Tag>.Empty;

        static Tag()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Tag>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<Tag>.Empty = new Tag(string.Empty, OmniHash.Empty);
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

        public static Tag Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(Tag? left, Tag? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(Tag? left, Tag? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is Tag)) return false;
            return this.Equals((Tag)other);
        }
        public bool Equals(Tag? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Hash != target.Hash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<Tag>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in Tag value, in int rank)
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
                    OmniHash.Formatter.Serialize(ref w, value.Hash, rank + 1);
                }
            }

            public Tag Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_hash = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new Tag(p_type, p_hash);
            }
        }
    }

    public sealed partial class NodeFinderOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeFinderOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeFinderOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeFinderOptions>.Formatter;
        public static NodeFinderOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeFinderOptions>.Empty;

        static NodeFinderOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeFinderOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeFinderOptions>.Empty = new NodeFinderOptions(string.Empty, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public NodeFinderOptions(string configPath, uint maxConnectionCount)
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

        public static NodeFinderOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NodeFinderOptions? left, NodeFinderOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NodeFinderOptions? left, NodeFinderOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NodeFinderOptions)) return false;
            return this.Equals((NodeFinderOptions)other);
        }
        public bool Equals(NodeFinderOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeFinderOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NodeFinderOptions value, in int rank)
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

            public NodeFinderOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new NodeFinderOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }

    public sealed partial class PublishContentStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishContentStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentStorageOptions>.Formatter;
        public static PublishContentStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentStorageOptions>.Empty;

        static PublishContentStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentStorageOptions>.Empty = new PublishContentStorageOptions(string.Empty);
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

        public static PublishContentStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishContentStorageOptions? left, PublishContentStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishContentStorageOptions? left, PublishContentStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishContentStorageOptions)) return false;
            return this.Equals((PublishContentStorageOptions)other);
        }
        public bool Equals(PublishContentStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishContentStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishContentStorageOptions value, in int rank)
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

            public PublishContentStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new PublishContentStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class WantContentStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantContentStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentStorageOptions>.Formatter;
        public static WantContentStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentStorageOptions>.Empty;

        static WantContentStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentStorageOptions>.Empty = new WantContentStorageOptions(string.Empty);
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

        public static WantContentStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantContentStorageOptions? left, WantContentStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantContentStorageOptions? left, WantContentStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantContentStorageOptions)) return false;
            return this.Equals((WantContentStorageOptions)other);
        }
        public bool Equals(WantContentStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantContentStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantContentStorageOptions value, in int rank)
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

            public WantContentStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new WantContentStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class ContentExchangerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentExchangerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ContentExchangerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentExchangerOptions>.Formatter;
        public static ContentExchangerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentExchangerOptions>.Empty;

        static ContentExchangerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ContentExchangerOptions>.Empty = new ContentExchangerOptions(string.Empty, 0);
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

        public static ContentExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ContentExchangerOptions? left, ContentExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ContentExchangerOptions? left, ContentExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ContentExchangerOptions)) return false;
            return this.Equals((ContentExchangerOptions)other);
        }
        public bool Equals(ContentExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ContentExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ContentExchangerOptions value, in int rank)
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
                    w.Write((uint)0);
                    w.Write(value.MaxConnectionCount);
                }
            }

            public ContentExchangerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                        case 0:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new ContentExchangerOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }

    public sealed partial class PublishMessageStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishMessageStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageStorageOptions>.Formatter;
        public static PublishMessageStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageStorageOptions>.Empty;

        static PublishMessageStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageStorageOptions>.Empty = new PublishMessageStorageOptions(string.Empty);
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

        public static PublishMessageStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishMessageStorageOptions? left, PublishMessageStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishMessageStorageOptions? left, PublishMessageStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishMessageStorageOptions)) return false;
            return this.Equals((PublishMessageStorageOptions)other);
        }
        public bool Equals(PublishMessageStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishMessageStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishMessageStorageOptions value, in int rank)
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

            public PublishMessageStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new PublishMessageStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class WantMessageStorageOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageStorageOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantMessageStorageOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageStorageOptions>.Formatter;
        public static WantMessageStorageOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageStorageOptions>.Empty;

        static WantMessageStorageOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageStorageOptions>.Empty = new WantMessageStorageOptions(string.Empty);
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

        public static WantMessageStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantMessageStorageOptions? left, WantMessageStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantMessageStorageOptions? left, WantMessageStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantMessageStorageOptions)) return false;
            return this.Equals((WantMessageStorageOptions)other);
        }
        public bool Equals(WantMessageStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantMessageStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantMessageStorageOptions value, in int rank)
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

            public WantMessageStorageOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new WantMessageStorageOptions(p_configPath);
            }
        }
    }

    public sealed partial class MessageExchangerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MessageExchangerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<MessageExchangerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MessageExchangerOptions>.Formatter;
        public static MessageExchangerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MessageExchangerOptions>.Empty;

        static MessageExchangerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MessageExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<MessageExchangerOptions>.Empty = new MessageExchangerOptions(string.Empty, 0);
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

        public static MessageExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(MessageExchangerOptions? left, MessageExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(MessageExchangerOptions? left, MessageExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is MessageExchangerOptions)) return false;
            return this.Equals((MessageExchangerOptions)other);
        }
        public bool Equals(MessageExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<MessageExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in MessageExchangerOptions value, in int rank)
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
                    w.Write((uint)0);
                    w.Write(value.MaxConnectionCount);
                }
            }

            public MessageExchangerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                        case 0:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new MessageExchangerOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }

    public sealed partial class ConsistencyReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConsistencyReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>.Formatter;
        public static ConsistencyReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>.Empty;

        static ConsistencyReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConsistencyReport>.Empty = new ConsistencyReport(0, 0, 0);
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

        public static ConsistencyReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ConsistencyReport? left, ConsistencyReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ConsistencyReport? left, ConsistencyReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ConsistencyReport)) return false;
            return this.Equals((ConsistencyReport)other);
        }
        public bool Equals(ConsistencyReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConsistencyReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ConsistencyReport value, in int rank)
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

            public ConsistencyReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new ConsistencyReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }

    public sealed partial class ConnectionReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConnectionReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>.Formatter;
        public static ConnectionReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>.Empty;

        static ConnectionReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionReport>.Empty = new ConnectionReport((ConnectionType)0, OmniAddress.Empty);
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

        public static ConnectionReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ConnectionReport? left, ConnectionReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ConnectionReport? left, ConnectionReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ConnectionReport)) return false;
            return this.Equals((ConnectionReport)other);
        }
        public bool Equals(ConnectionReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Endpoint != target.Endpoint) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConnectionReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ConnectionReport value, in int rank)
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
                    OmniAddress.Formatter.Serialize(ref w, value.Endpoint, rank + 1);
                }
            }

            public ConnectionReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_endpoint = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new ConnectionReport(p_type, p_endpoint);
            }
        }
    }

    public sealed partial class PublishContentReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishContentReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentReport>.Formatter;
        public static PublishContentReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentReport>.Empty;

        static PublishContentReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishContentReport>.Empty = new PublishContentReport(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PublishContentReport(OmniHash tag)
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

        public static PublishContentReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishContentReport? left, PublishContentReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishContentReport? left, PublishContentReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishContentReport)) return false;
            return this.Equals((PublishContentReport)other);
        }
        public bool Equals(PublishContentReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishContentReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishContentReport value, in int rank)
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
                    OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
            }

            public PublishContentReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tag = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new PublishContentReport(p_tag);
            }
        }
    }

    public sealed partial class WantContentReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantContentReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentReport>.Formatter;
        public static WantContentReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentReport>.Empty;

        static WantContentReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantContentReport>.Empty = new WantContentReport(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantContentReport(OmniHash tag)
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

        public static WantContentReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantContentReport? left, WantContentReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantContentReport? left, WantContentReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantContentReport)) return false;
            return this.Equals((WantContentReport)other);
        }
        public bool Equals(WantContentReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantContentReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantContentReport value, in int rank)
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
                    OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
            }

            public WantContentReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tag = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new WantContentReport(p_tag);
            }
        }
    }

    public sealed partial class PublishMessageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishMessageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageReport>.Formatter;
        public static PublishMessageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageReport>.Empty;

        static PublishMessageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishMessageReport>.Empty = new PublishMessageReport(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PublishMessageReport(OmniHash tag)
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

        public static PublishMessageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishMessageReport? left, PublishMessageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishMessageReport? left, PublishMessageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishMessageReport)) return false;
            return this.Equals((PublishMessageReport)other);
        }
        public bool Equals(PublishMessageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishMessageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishMessageReport value, in int rank)
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
                    OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
            }

            public PublishMessageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tag = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new PublishMessageReport(p_tag);
            }
        }
    }

    public sealed partial class WantMessageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantMessageReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageReport>.Formatter;
        public static WantMessageReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageReport>.Empty;

        static WantMessageReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantMessageReport>.Empty = new WantMessageReport(OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantMessageReport(OmniHash tag)
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

        public static WantMessageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantMessageReport? left, WantMessageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantMessageReport? left, WantMessageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantMessageReport)) return false;
            return this.Equals((WantMessageReport)other);
        }
        public bool Equals(WantMessageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Tag != target.Tag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantMessageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantMessageReport value, in int rank)
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
                    OmniHash.Formatter.Serialize(ref w, value.Tag, rank + 1);
                }
            }

            public WantMessageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tag = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new WantMessageReport(p_tag);
            }
        }
    }

}
