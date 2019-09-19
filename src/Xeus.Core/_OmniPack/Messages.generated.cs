using Omnix.Algorithms.Cryptography;
using Omnix.Network;

#nullable enable

namespace Xeus.Core
{
    public enum TcpProxyType : byte
    {
        HttpProxy = 0,
        Socks5Proxy = 1,
    }

    public enum ErrorReportType : byte
    {
        SpaceNotFound = 0,
    }

    public sealed partial class XeusClue : global::Omnix.Serialization.OmniPack.IOmniPackMessage<XeusClue>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<XeusClue> Formatter { get; }
        public static XeusClue Empty { get; }

        static XeusClue()
        {
            XeusClue.Formatter = new ___CustomFormatter();
            XeusClue.Empty = new XeusClue(OmniHash.Empty, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public XeusClue(OmniHash hash, byte depth)
        {
            this.Hash = hash;
            this.Depth = depth;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (hash != default) ___h.Add(hash.GetHashCode());
                if (depth != default) ___h.Add(depth.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public OmniHash Hash { get; }
        public byte Depth { get; }

        public static XeusClue Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(XeusClue? left, XeusClue? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(XeusClue? left, XeusClue? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is XeusClue)) return false;
            return this.Equals((XeusClue)other);
        }
        public bool Equals(XeusClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.Depth != target.Depth) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<XeusClue>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in XeusClue value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Hash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Depth != 0)
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
                if (value.Depth != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Depth);
                }
            }

            public XeusClue Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                byte p_depth = 0;

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
                                p_depth = r.GetUInt8();
                                break;
                            }
                    }
                }

                return new XeusClue(p_hash, p_depth);
            }
        }
    }

    public sealed partial class TcpProxyOptions : global::Omnix.Serialization.OmniPack.IOmniPackMessage<TcpProxyOptions>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<TcpProxyOptions> Formatter { get; }
        public static TcpProxyOptions Empty { get; }

        static TcpProxyOptions()
        {
            TcpProxyOptions.Formatter = new ___CustomFormatter();
            TcpProxyOptions.Empty = new TcpProxyOptions((TcpProxyType)0, OmniAddress.Empty);
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

        public static TcpProxyOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(TcpProxyOptions? left, TcpProxyOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(TcpProxyOptions? left, TcpProxyOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is TcpProxyOptions)) return false;
            return this.Equals((TcpProxyOptions)other);
        }
        public bool Equals(TcpProxyOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Address != target.Address) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<TcpProxyOptions>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in TcpProxyOptions value, in int rank)
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
                    OmniAddress.Formatter.Serialize(ref w, value.Address, rank + 1);
                }
            }

            public TcpProxyOptions Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
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
                                p_address = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpProxyOptions(p_type, p_address);
            }
        }
    }

    public sealed partial class TcpConnectOptions : global::Omnix.Serialization.OmniPack.IOmniPackMessage<TcpConnectOptions>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<TcpConnectOptions> Formatter { get; }
        public static TcpConnectOptions Empty { get; }

        static TcpConnectOptions()
        {
            TcpConnectOptions.Formatter = new ___CustomFormatter();
            TcpConnectOptions.Empty = new TcpConnectOptions(false, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpConnectOptions(bool enabled, TcpProxyOptions? proxyOptions)
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

        public static TcpConnectOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(TcpConnectOptions? left, TcpConnectOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(TcpConnectOptions? left, TcpConnectOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is TcpConnectOptions)) return false;
            return this.Equals((TcpConnectOptions)other);
        }
        public bool Equals(TcpConnectOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if ((this.ProxyOptions is null) != (target.ProxyOptions is null)) return false;
            if (!(this.ProxyOptions is null) && !(target.ProxyOptions is null) && this.ProxyOptions != target.ProxyOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<TcpConnectOptions>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in TcpConnectOptions value, in int rank)
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
                    TcpProxyOptions.Formatter.Serialize(ref w, value.ProxyOptions, rank + 1);
                }
            }

            public TcpConnectOptions Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
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
                                p_proxyOptions = TcpProxyOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpConnectOptions(p_enabled, p_proxyOptions);
            }
        }
    }

    public sealed partial class TcpAcceptOptions : global::Omnix.Serialization.OmniPack.IOmniPackMessage<TcpAcceptOptions>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<TcpAcceptOptions> Formatter { get; }
        public static TcpAcceptOptions Empty { get; }

        static TcpAcceptOptions()
        {
            TcpAcceptOptions.Formatter = new ___CustomFormatter();
            TcpAcceptOptions.Empty = new TcpAcceptOptions(false, global::System.Array.Empty<OmniAddress>(), false);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxListenAddressesCount = 32;

        public TcpAcceptOptions(bool enabled, OmniAddress[] listenAddresses, bool useUpnp)
        {
            if (listenAddresses is null) throw new global::System.ArgumentNullException("listenAddresses");
            if (listenAddresses.Length > 32) throw new global::System.ArgumentOutOfRangeException("listenAddresses");
            foreach (var n in listenAddresses)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }
            this.Enabled = enabled;
            this.ListenAddresses = new global::Omnix.DataStructures.ReadOnlyListSlim<OmniAddress>(listenAddresses);
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
        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniAddress> ListenAddresses { get; }
        public bool UseUpnp { get; }

        public static TcpAcceptOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(TcpAcceptOptions? left, TcpAcceptOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(TcpAcceptOptions? left, TcpAcceptOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is TcpAcceptOptions)) return false;
            return this.Equals((TcpAcceptOptions)other);
        }
        public bool Equals(TcpAcceptOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ListenAddresses, target.ListenAddresses)) return false;
            if (this.UseUpnp != target.UseUpnp) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<TcpAcceptOptions>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in TcpAcceptOptions value, in int rank)
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
                        OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.UseUpnp != false)
                {
                    w.Write((uint)2);
                    w.Write(value.UseUpnp);
                }
            }

            public TcpAcceptOptions Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
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
                                    p_listenAddresses[i] = OmniAddress.Formatter.Deserialize(ref r, rank + 1);
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

                return new TcpAcceptOptions(p_enabled, p_listenAddresses, p_useUpnp);
            }
        }
    }

    public sealed partial class ConnectorOptions : global::Omnix.Serialization.OmniPack.IOmniPackMessage<ConnectorOptions>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<ConnectorOptions> Formatter { get; }
        public static ConnectorOptions Empty { get; }

        static ConnectorOptions()
        {
            ConnectorOptions.Formatter = new ___CustomFormatter();
            ConnectorOptions.Empty = new ConnectorOptions(TcpConnectOptions.Empty, TcpAcceptOptions.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ConnectorOptions(TcpConnectOptions tcpConnectOptions, TcpAcceptOptions tcpAcceptOptions)
        {
            if (tcpConnectOptions is null) throw new global::System.ArgumentNullException("tcpConnectOptions");
            if (tcpAcceptOptions is null) throw new global::System.ArgumentNullException("tcpAcceptOptions");

            this.TcpConnectOptions = tcpConnectOptions;
            this.TcpAcceptOptions = tcpAcceptOptions;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tcpConnectOptions != default) ___h.Add(tcpConnectOptions.GetHashCode());
                if (tcpAcceptOptions != default) ___h.Add(tcpAcceptOptions.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public TcpConnectOptions TcpConnectOptions { get; }
        public TcpAcceptOptions TcpAcceptOptions { get; }

        public static ConnectorOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ConnectorOptions? left, ConnectorOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ConnectorOptions? left, ConnectorOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ConnectorOptions)) return false;
            return this.Equals((ConnectorOptions)other);
        }
        public bool Equals(ConnectorOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.TcpConnectOptions != target.TcpConnectOptions) return false;
            if (this.TcpAcceptOptions != target.TcpAcceptOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<ConnectorOptions>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in ConnectorOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.TcpConnectOptions != TcpConnectOptions.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.TcpAcceptOptions != TcpAcceptOptions.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.TcpConnectOptions != TcpConnectOptions.Empty)
                {
                    w.Write((uint)0);
                    TcpConnectOptions.Formatter.Serialize(ref w, value.TcpConnectOptions, rank + 1);
                }
                if (value.TcpAcceptOptions != TcpAcceptOptions.Empty)
                {
                    w.Write((uint)1);
                    TcpAcceptOptions.Formatter.Serialize(ref w, value.TcpAcceptOptions, rank + 1);
                }
            }

            public ConnectorOptions Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                TcpConnectOptions p_tcpConnectOptions = TcpConnectOptions.Empty;
                TcpAcceptOptions p_tcpAcceptOptions = TcpAcceptOptions.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tcpConnectOptions = TcpConnectOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_tcpAcceptOptions = TcpAcceptOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new ConnectorOptions(p_tcpConnectOptions, p_tcpAcceptOptions);
            }
        }
    }

    public sealed partial class XeusOptions : global::Omnix.Serialization.OmniPack.IOmniPackMessage<XeusOptions>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<XeusOptions> Formatter { get; }
        public static XeusOptions Empty { get; }

        static XeusOptions()
        {
            XeusOptions.Formatter = new ___CustomFormatter();
            XeusOptions.Empty = new XeusOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigDirectoryPathLength = 1024;

        public XeusOptions(string configDirectoryPath)
        {
            if (configDirectoryPath is null) throw new global::System.ArgumentNullException("configDirectoryPath");
            if (configDirectoryPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configDirectoryPath");

            this.ConfigDirectoryPath = configDirectoryPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configDirectoryPath != default) ___h.Add(configDirectoryPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigDirectoryPath { get; }

        public static XeusOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(XeusOptions? left, XeusOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(XeusOptions? left, XeusOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is XeusOptions)) return false;
            return this.Equals((XeusOptions)other);
        }
        public bool Equals(XeusOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigDirectoryPath != target.ConfigDirectoryPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<XeusOptions>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in XeusOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigDirectoryPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigDirectoryPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigDirectoryPath);
                }
            }

            public XeusOptions Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configDirectoryPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configDirectoryPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new XeusOptions(p_configDirectoryPath);
            }
        }
    }

    public sealed partial class ErrorReport : global::Omnix.Serialization.OmniPack.IOmniPackMessage<ErrorReport>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<ErrorReport> Formatter { get; }
        public static ErrorReport Empty { get; }

        static ErrorReport()
        {
            ErrorReport.Formatter = new ___CustomFormatter();
            ErrorReport.Empty = new ErrorReport(global::Omnix.Serialization.OmniPack.Timestamp.Zero, (ErrorReportType)0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ErrorReport(global::Omnix.Serialization.OmniPack.Timestamp creationTime, ErrorReportType type)
        {
            this.CreationTime = creationTime;
            this.Type = type;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (creationTime != default) ___h.Add(creationTime.GetHashCode());
                if (type != default) ___h.Add(type.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public global::Omnix.Serialization.OmniPack.Timestamp CreationTime { get; }
        public ErrorReportType Type { get; }

        public static ErrorReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ErrorReport? left, ErrorReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ErrorReport? left, ErrorReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ErrorReport)) return false;
            return this.Equals((ErrorReport)other);
        }
        public bool Equals(ErrorReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Type != target.Type) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<ErrorReport>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in ErrorReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CreationTime != global::Omnix.Serialization.OmniPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Type != (ErrorReportType)0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.CreationTime != global::Omnix.Serialization.OmniPack.Timestamp.Zero)
                {
                    w.Write((uint)0);
                    w.Write(value.CreationTime);
                }
                if (value.Type != (ErrorReportType)0)
                {
                    w.Write((uint)1);
                    w.Write((ulong)value.Type);
                }
            }

            public ErrorReport Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::Omnix.Serialization.OmniPack.Timestamp p_creationTime = global::Omnix.Serialization.OmniPack.Timestamp.Zero;
                ErrorReportType p_type = (ErrorReportType)0;

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
                                p_type = (ErrorReportType)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new ErrorReport(p_creationTime, p_type);
            }
        }
    }

    public sealed partial class CheckBlocksProgressReport : global::Omnix.Serialization.OmniPack.IOmniPackMessage<CheckBlocksProgressReport>
    {
        public static global::Omnix.Serialization.OmniPack.IOmniPackFormatter<CheckBlocksProgressReport> Formatter { get; }
        public static CheckBlocksProgressReport Empty { get; }

        static CheckBlocksProgressReport()
        {
            CheckBlocksProgressReport.Formatter = new ___CustomFormatter();
            CheckBlocksProgressReport.Empty = new CheckBlocksProgressReport(0, 0, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public CheckBlocksProgressReport(uint badBlockCount, uint checkedBlockCount, uint totalBlockCount)
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

        public static CheckBlocksProgressReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.OmniPack.OmniPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.OmniPack.OmniPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(CheckBlocksProgressReport? left, CheckBlocksProgressReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(CheckBlocksProgressReport? left, CheckBlocksProgressReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is CheckBlocksProgressReport)) return false;
            return this.Equals((CheckBlocksProgressReport)other);
        }
        public bool Equals(CheckBlocksProgressReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.OmniPack.IOmniPackFormatter<CheckBlocksProgressReport>
        {
            public void Serialize(ref global::Omnix.Serialization.OmniPack.OmniPackWriter w, in CheckBlocksProgressReport value, in int rank)
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

            public CheckBlocksProgressReport Deserialize(ref global::Omnix.Serialization.OmniPack.OmniPackReader r, in int rank)
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

                return new CheckBlocksProgressReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }

}
