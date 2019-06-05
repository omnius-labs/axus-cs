using Omnix.Cryptography;
using Omnix.Network;

#nullable enable

namespace Xeus.Messages
{
    public enum TcpProxyType : byte
    {
        HttpProxy = 0,
        Socks5Proxy = 1,
    }

    public sealed partial class XeusClue : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusClue>
    {
        static XeusClue()
        {
            XeusClue.Formatter = new CustomFormatter();
            XeusClue.Empty = new XeusClue(OmniHash.Empty, 0);
        }

        private readonly int __hashCode;

        public XeusClue(OmniHash hash, byte depth)
        {
            this.Hash = hash;
            this.Depth = depth;

            {
                var __h = new System.HashCode();
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (this.Depth != default) __h.Add(this.Depth.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        public byte Depth { get; }

        public override bool Equals(XeusClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.Depth != target.Depth) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusClue>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusClue value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
                if (value.Depth != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Depth);
                }
            }

            public XeusClue Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
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

    public sealed partial class TcpProxyConfig : Omnix.Serialization.RocketPack.RocketPackMessageBase<TcpProxyConfig>
    {
        static TcpProxyConfig()
        {
            TcpProxyConfig.Formatter = new CustomFormatter();
            TcpProxyConfig.Empty = new TcpProxyConfig((TcpProxyType)0, OmniAddress.Empty);
        }

        private readonly int __hashCode;

        public TcpProxyConfig(TcpProxyType type, OmniAddress address)
        {
            if (address is null) throw new System.ArgumentNullException("address");

            this.Type = type;
            this.Address = address;

            {
                var __h = new System.HashCode();
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                if (this.Address != default) __h.Add(this.Address.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public TcpProxyType Type { get; }
        public OmniAddress Address { get; }

        public override bool Equals(TcpProxyConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Address != target.Address) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpProxyConfig>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, TcpProxyConfig value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                    OmniAddress.Formatter.Serialize(w, value.Address, rank + 1);
                }
            }

            public TcpProxyConfig Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                                p_address = OmniAddress.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpProxyConfig(p_type, p_address);
            }
        }
    }

    public sealed partial class TcpConnectConfig : Omnix.Serialization.RocketPack.RocketPackMessageBase<TcpConnectConfig>
    {
        static TcpConnectConfig()
        {
            TcpConnectConfig.Formatter = new CustomFormatter();
            TcpConnectConfig.Empty = new TcpConnectConfig(false, null);
        }

        private readonly int __hashCode;

        public TcpConnectConfig(bool enabled, TcpProxyConfig? proxyConfig)
        {
            this.Enabled = enabled;
            this.ProxyConfig = proxyConfig;

            {
                var __h = new System.HashCode();
                if (this.Enabled != default) __h.Add(this.Enabled.GetHashCode());
                if (this.ProxyConfig != default) __h.Add(this.ProxyConfig.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public bool Enabled { get; }
        public TcpProxyConfig? ProxyConfig { get; }

        public override bool Equals(TcpConnectConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if ((this.ProxyConfig is null) != (target.ProxyConfig is null)) return false;
            if (!(this.ProxyConfig is null) && !(target.ProxyConfig is null) && this.ProxyConfig != target.ProxyConfig) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpConnectConfig>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, TcpConnectConfig value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Enabled != false)
                    {
                        propertyCount++;
                    }
                    if (value.ProxyConfig != null)
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
                if (value.ProxyConfig != null)
                {
                    w.Write((uint)1);
                    TcpProxyConfig.Formatter.Serialize(w, value.ProxyConfig, rank + 1);
                }
            }

            public TcpConnectConfig Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                bool p_enabled = false;
                TcpProxyConfig? p_proxyConfig = null;

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
                                p_proxyConfig = TcpProxyConfig.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpConnectConfig(p_enabled, p_proxyConfig);
            }
        }
    }

    public sealed partial class TcpAcceptConfig : Omnix.Serialization.RocketPack.RocketPackMessageBase<TcpAcceptConfig>
    {
        static TcpAcceptConfig()
        {
            TcpAcceptConfig.Formatter = new CustomFormatter();
            TcpAcceptConfig.Empty = new TcpAcceptConfig(false, System.Array.Empty<OmniAddress>(), false);
        }

        private readonly int __hashCode;

        public static readonly int MaxListenAddressesCount = 32;

        public TcpAcceptConfig(bool enabled, OmniAddress[] listenAddresses, bool useUpnp)
        {
            if (listenAddresses is null) throw new System.ArgumentNullException("listenAddresses");
            if (listenAddresses.Length > 32) throw new System.ArgumentOutOfRangeException("listenAddresses");
            foreach (var n in listenAddresses)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }
            this.Enabled = enabled;
            this.ListenAddresses = new Omnix.Collections.ReadOnlyListSlim<OmniAddress>(listenAddresses);
            this.UseUpnp = useUpnp;

            {
                var __h = new System.HashCode();
                if (this.Enabled != default) __h.Add(this.Enabled.GetHashCode());
                foreach (var n in this.ListenAddresses)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                if (this.UseUpnp != default) __h.Add(this.UseUpnp.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public bool Enabled { get; }
        public Omnix.Collections.ReadOnlyListSlim<OmniAddress> ListenAddresses { get; }
        public bool UseUpnp { get; }

        public override bool Equals(TcpAcceptConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.ListenAddresses, target.ListenAddresses)) return false;
            if (this.UseUpnp != target.UseUpnp) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpAcceptConfig>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, TcpAcceptConfig value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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
                        OmniAddress.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                if (value.UseUpnp != false)
                {
                    w.Write((uint)2);
                    w.Write(value.UseUpnp);
                }
            }

            public TcpAcceptConfig Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                bool p_enabled = false;
                OmniAddress[] p_listenAddresses = System.Array.Empty<OmniAddress>();
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
                                    p_listenAddresses[i] = OmniAddress.Formatter.Deserialize(r, rank + 1);
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

                return new TcpAcceptConfig(p_enabled, p_listenAddresses, p_useUpnp);
            }
        }
    }

    public sealed partial class ConnectionCreatorConfig : Omnix.Serialization.RocketPack.RocketPackMessageBase<ConnectionCreatorConfig>
    {
        static ConnectionCreatorConfig()
        {
            ConnectionCreatorConfig.Formatter = new CustomFormatter();
            ConnectionCreatorConfig.Empty = new ConnectionCreatorConfig(TcpConnectConfig.Empty, TcpAcceptConfig.Empty);
        }

        private readonly int __hashCode;

        public ConnectionCreatorConfig(TcpConnectConfig tcpConnectConfig, TcpAcceptConfig tcpAcceptConfig)
        {
            if (tcpConnectConfig is null) throw new System.ArgumentNullException("tcpConnectConfig");
            if (tcpAcceptConfig is null) throw new System.ArgumentNullException("tcpAcceptConfig");

            this.TcpConnectConfig = tcpConnectConfig;
            this.TcpAcceptConfig = tcpAcceptConfig;

            {
                var __h = new System.HashCode();
                if (this.TcpConnectConfig != default) __h.Add(this.TcpConnectConfig.GetHashCode());
                if (this.TcpAcceptConfig != default) __h.Add(this.TcpAcceptConfig.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public TcpConnectConfig TcpConnectConfig { get; }
        public TcpAcceptConfig TcpAcceptConfig { get; }

        public override bool Equals(ConnectionCreatorConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.TcpConnectConfig != target.TcpConnectConfig) return false;
            if (this.TcpAcceptConfig != target.TcpAcceptConfig) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ConnectionCreatorConfig>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ConnectionCreatorConfig value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.TcpConnectConfig != TcpConnectConfig.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.TcpAcceptConfig != TcpAcceptConfig.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.TcpConnectConfig != TcpConnectConfig.Empty)
                {
                    w.Write((uint)0);
                    TcpConnectConfig.Formatter.Serialize(w, value.TcpConnectConfig, rank + 1);
                }
                if (value.TcpAcceptConfig != TcpAcceptConfig.Empty)
                {
                    w.Write((uint)1);
                    TcpAcceptConfig.Formatter.Serialize(w, value.TcpAcceptConfig, rank + 1);
                }
            }

            public ConnectionCreatorConfig Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                TcpConnectConfig p_tcpConnectConfig = TcpConnectConfig.Empty;
                TcpAcceptConfig p_tcpAcceptConfig = TcpAcceptConfig.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tcpConnectConfig = TcpConnectConfig.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_tcpAcceptConfig = TcpAcceptConfig.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new ConnectionCreatorConfig(p_tcpConnectConfig, p_tcpAcceptConfig);
            }
        }
    }

    public sealed partial class XeusOptions : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusOptions>
    {
        static XeusOptions()
        {
            XeusOptions.Formatter = new CustomFormatter();
            XeusOptions.Empty = new XeusOptions(string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxConfigDirectoryPathLength = 1024;

        public XeusOptions(string configDirectoryPath)
        {
            if (configDirectoryPath is null) throw new System.ArgumentNullException("configDirectoryPath");
            if (configDirectoryPath.Length > 1024) throw new System.ArgumentOutOfRangeException("configDirectoryPath");

            this.ConfigDirectoryPath = configDirectoryPath;

            {
                var __h = new System.HashCode();
                if (this.ConfigDirectoryPath != default) __h.Add(this.ConfigDirectoryPath.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string ConfigDirectoryPath { get; }

        public override bool Equals(XeusOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigDirectoryPath != target.ConfigDirectoryPath) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusOptions>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusOptions value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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

            public XeusOptions Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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

}
