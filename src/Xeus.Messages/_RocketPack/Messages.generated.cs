using Omnix.Algorithms.Cryptography;
using Omnix.Network;

#nullable enable

namespace Xeus.Messages
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

    public sealed partial class XeusClue : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusClue>
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
                var __h = new global::System.HashCode();
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

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusClue>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, XeusClue value, int rank)
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
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
                if (value.Depth != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Depth);
                }
            }

            public XeusClue Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
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

    public sealed partial class TcpProxyOptions : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<TcpProxyOptions>
    {
        static TcpProxyOptions()
        {
            TcpProxyOptions.Formatter = new CustomFormatter();
            TcpProxyOptions.Empty = new TcpProxyOptions((TcpProxyType)0, OmniAddress.Empty);
        }

        private readonly int __hashCode;

        public TcpProxyOptions(TcpProxyType type, OmniAddress address)
        {
            if (address is null) throw new global::System.ArgumentNullException("address");

            this.Type = type;
            this.Address = address;

            {
                var __h = new global::System.HashCode();
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                if (this.Address != default) __h.Add(this.Address.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public TcpProxyType Type { get; }
        public OmniAddress Address { get; }

        public override bool Equals(TcpProxyOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Address != target.Address) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpProxyOptions>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, TcpProxyOptions value, int rank)
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
                    OmniAddress.Formatter.Serialize(w, value.Address, rank + 1);
                }
            }

            public TcpProxyOptions Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
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
                                p_address = OmniAddress.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpProxyOptions(p_type, p_address);
            }
        }
    }

    public sealed partial class TcpConnectOptions : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<TcpConnectOptions>
    {
        static TcpConnectOptions()
        {
            TcpConnectOptions.Formatter = new CustomFormatter();
            TcpConnectOptions.Empty = new TcpConnectOptions(false, null);
        }

        private readonly int __hashCode;

        public TcpConnectOptions(bool enabled, TcpProxyOptions? proxyOptions)
        {
            this.Enabled = enabled;
            this.ProxyOptions = proxyOptions;

            {
                var __h = new global::System.HashCode();
                if (this.Enabled != default) __h.Add(this.Enabled.GetHashCode());
                if (this.ProxyOptions != default) __h.Add(this.ProxyOptions.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public bool Enabled { get; }
        public TcpProxyOptions? ProxyOptions { get; }

        public override bool Equals(TcpConnectOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if ((this.ProxyOptions is null) != (target.ProxyOptions is null)) return false;
            if (!(this.ProxyOptions is null) && !(target.ProxyOptions is null) && this.ProxyOptions != target.ProxyOptions) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpConnectOptions>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, TcpConnectOptions value, int rank)
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
                    TcpProxyOptions.Formatter.Serialize(w, value.ProxyOptions, rank + 1);
                }
            }

            public TcpConnectOptions Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
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
                                p_proxyOptions = TcpProxyOptions.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpConnectOptions(p_enabled, p_proxyOptions);
            }
        }
    }

    public sealed partial class TcpAcceptOptions : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<TcpAcceptOptions>
    {
        static TcpAcceptOptions()
        {
            TcpAcceptOptions.Formatter = new CustomFormatter();
            TcpAcceptOptions.Empty = new TcpAcceptOptions(false, global::System.Array.Empty<OmniAddress>(), false);
        }

        private readonly int __hashCode;

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

            {
                var __h = new global::System.HashCode();
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
        public global::Omnix.DataStructures.ReadOnlyListSlim<OmniAddress> ListenAddresses { get; }
        public bool UseUpnp { get; }

        public override bool Equals(TcpAcceptOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if (!global::Omnix.Base.Helpers.CollectionHelper.Equals(this.ListenAddresses, target.ListenAddresses)) return false;
            if (this.UseUpnp != target.UseUpnp) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpAcceptOptions>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, TcpAcceptOptions value, int rank)
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
                        OmniAddress.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                if (value.UseUpnp != false)
                {
                    w.Write((uint)2);
                    w.Write(value.UseUpnp);
                }
            }

            public TcpAcceptOptions Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
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

                return new TcpAcceptOptions(p_enabled, p_listenAddresses, p_useUpnp);
            }
        }
    }

    public sealed partial class ConnectionCreatorOptions : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<ConnectionCreatorOptions>
    {
        static ConnectionCreatorOptions()
        {
            ConnectionCreatorOptions.Formatter = new CustomFormatter();
            ConnectionCreatorOptions.Empty = new ConnectionCreatorOptions(TcpConnectOptions.Empty, TcpAcceptOptions.Empty);
        }

        private readonly int __hashCode;

        public ConnectionCreatorOptions(TcpConnectOptions tcpConnectOptions, TcpAcceptOptions tcpAcceptOptions)
        {
            if (tcpConnectOptions is null) throw new global::System.ArgumentNullException("tcpConnectOptions");
            if (tcpAcceptOptions is null) throw new global::System.ArgumentNullException("tcpAcceptOptions");

            this.TcpConnectOptions = tcpConnectOptions;
            this.TcpAcceptOptions = tcpAcceptOptions;

            {
                var __h = new global::System.HashCode();
                if (this.TcpConnectOptions != default) __h.Add(this.TcpConnectOptions.GetHashCode());
                if (this.TcpAcceptOptions != default) __h.Add(this.TcpAcceptOptions.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public TcpConnectOptions TcpConnectOptions { get; }
        public TcpAcceptOptions TcpAcceptOptions { get; }

        public override bool Equals(ConnectionCreatorOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.TcpConnectOptions != target.TcpConnectOptions) return false;
            if (this.TcpAcceptOptions != target.TcpAcceptOptions) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ConnectionCreatorOptions>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, ConnectionCreatorOptions value, int rank)
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
                    TcpConnectOptions.Formatter.Serialize(w, value.TcpConnectOptions, rank + 1);
                }
                if (value.TcpAcceptOptions != TcpAcceptOptions.Empty)
                {
                    w.Write((uint)1);
                    TcpAcceptOptions.Formatter.Serialize(w, value.TcpAcceptOptions, rank + 1);
                }
            }

            public ConnectionCreatorOptions Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
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
                                p_tcpConnectOptions = TcpConnectOptions.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_tcpAcceptOptions = TcpAcceptOptions.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new ConnectionCreatorOptions(p_tcpConnectOptions, p_tcpAcceptOptions);
            }
        }
    }

    public sealed partial class XeusOptions : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusOptions>
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
            if (configDirectoryPath is null) throw new global::System.ArgumentNullException("configDirectoryPath");
            if (configDirectoryPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configDirectoryPath");

            this.ConfigDirectoryPath = configDirectoryPath;

            {
                var __h = new global::System.HashCode();
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

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusOptions>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, XeusOptions value, int rank)
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

            public XeusOptions Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
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

    public sealed partial class ErrorReport : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<ErrorReport>
    {
        static ErrorReport()
        {
            ErrorReport.Formatter = new CustomFormatter();
            ErrorReport.Empty = new ErrorReport(global::Omnix.Serialization.RocketPack.Timestamp.Zero, (ErrorReportType)0);
        }

        private readonly int __hashCode;

        public ErrorReport(global::Omnix.Serialization.RocketPack.Timestamp creationTime, ErrorReportType type)
        {
            this.CreationTime = creationTime;
            this.Type = type;

            {
                var __h = new global::System.HashCode();
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public global::Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public ErrorReportType Type { get; }

        public override bool Equals(ErrorReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Type != target.Type) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<ErrorReport>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, ErrorReport value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CreationTime != global::Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Type != (ErrorReportType)0)
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
                if (value.Type != (ErrorReportType)0)
                {
                    w.Write((uint)1);
                    w.Write((ulong)value.Type);
                }
            }

            public ErrorReport Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::Omnix.Serialization.RocketPack.Timestamp p_creationTime = global::Omnix.Serialization.RocketPack.Timestamp.Zero;
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

    public sealed partial class CheckBlocksProgressReport : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<CheckBlocksProgressReport>
    {
        static CheckBlocksProgressReport()
        {
            CheckBlocksProgressReport.Formatter = new CustomFormatter();
            CheckBlocksProgressReport.Empty = new CheckBlocksProgressReport(0, 0, 0);
        }

        private readonly int __hashCode;

        public CheckBlocksProgressReport(uint badBlockCount, uint checkedBlockCount, uint totalBlockCount)
        {
            this.BadBlockCount = badBlockCount;
            this.CheckedBlockCount = checkedBlockCount;
            this.TotalBlockCount = totalBlockCount;

            {
                var __h = new global::System.HashCode();
                if (this.BadBlockCount != default) __h.Add(this.BadBlockCount.GetHashCode());
                if (this.CheckedBlockCount != default) __h.Add(this.CheckedBlockCount.GetHashCode());
                if (this.TotalBlockCount != default) __h.Add(this.TotalBlockCount.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public uint BadBlockCount { get; }
        public uint CheckedBlockCount { get; }
        public uint TotalBlockCount { get; }

        public override bool Equals(CheckBlocksProgressReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<CheckBlocksProgressReport>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, CheckBlocksProgressReport value, int rank)
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

            public CheckBlocksProgressReport Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
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
