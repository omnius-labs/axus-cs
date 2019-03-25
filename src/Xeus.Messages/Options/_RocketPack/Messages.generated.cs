using Omnix.Cryptography;
using Xeus.Messages;

#nullable enable

namespace Xeus.Messages.Options
{
    public enum ConnectionType : byte
    {
        None = 0,
        Tcp = 1,
        Socks5Proxy = 2,
        HttpProxy = 3,
    }

    public sealed partial class ConnectionFilter : Omnix.Serialization.RocketPack.RocketPackMessageBase<ConnectionFilter>
    {
        static ConnectionFilter()
        {
            ConnectionFilter.Formatter = new CustomFormatter();
            ConnectionFilter.Empty = new ConnectionFilter(string.Empty, (ConnectionType)0, string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxSchemeLength = 256;
        public static readonly int MaxProxyUriLength = 1024;

        public ConnectionFilter(string scheme, ConnectionType connectionType, string proxyUri)
        {
            if (scheme is null) throw new System.ArgumentNullException("scheme");
            if (scheme.Length > 256) throw new System.ArgumentOutOfRangeException("scheme");
            if (proxyUri is null) throw new System.ArgumentNullException("proxyUri");
            if (proxyUri.Length > 1024) throw new System.ArgumentOutOfRangeException("proxyUri");

            this.Scheme = scheme;
            this.ConnectionType = connectionType;
            this.ProxyUri = proxyUri;

            {
                var __h = new System.HashCode();
                if (this.Scheme != default) __h.Add(this.Scheme.GetHashCode());
                if (this.ConnectionType != default) __h.Add(this.ConnectionType.GetHashCode());
                if (this.ProxyUri != default) __h.Add(this.ProxyUri.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string Scheme { get; }
        public ConnectionType ConnectionType { get; }
        public string ProxyUri { get; }

        public override bool Equals(ConnectionFilter? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Scheme != target.Scheme) return false;
            if (this.ConnectionType != target.ConnectionType) return false;
            if (this.ProxyUri != target.ProxyUri) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ConnectionFilter>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ConnectionFilter value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Scheme != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.ConnectionType != (ConnectionType)0)
                    {
                        propertyCount++;
                    }
                    if (value.ProxyUri != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Scheme != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Scheme);
                }
                if (value.ConnectionType != (ConnectionType)0)
                {
                    w.Write((uint)1);
                    w.Write((ulong)value.ConnectionType);
                }
                if (value.ProxyUri != string.Empty)
                {
                    w.Write((uint)2);
                    w.Write(value.ProxyUri);
                }
            }

            public ConnectionFilter Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_scheme = string.Empty;
                ConnectionType p_connectionType = (ConnectionType)0;
                string p_proxyUri = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Scheme
                            {
                                p_scheme = r.GetString(256);
                                break;
                            }
                        case 1: // ConnectionType
                            {
                                p_connectionType = (ConnectionType)r.GetUInt64();
                                break;
                            }
                        case 2: // ProxyUri
                            {
                                p_proxyUri = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new ConnectionFilter(p_scheme, p_connectionType, p_proxyUri);
            }
        }
    }

    public sealed partial class ContentsOptions : Omnix.Serialization.RocketPack.RocketPackMessageBase<ContentsOptions>
    {
        static ContentsOptions()
        {
            ContentsOptions.Formatter = new CustomFormatter();
            ContentsOptions.Empty = new ContentsOptions();
        }

        private readonly int __hashCode;

        public ContentsOptions()
        {

            {
                var __h = new System.HashCode();
                __hashCode = __h.ToHashCode();
            }
        }


        public override bool Equals(ContentsOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ContentsOptions>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ContentsOptions value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public ContentsOptions Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new ContentsOptions();
            }
        }
    }

    public sealed partial class DownloadOptions : Omnix.Serialization.RocketPack.RocketPackMessageBase<DownloadOptions>
    {
        static DownloadOptions()
        {
            DownloadOptions.Formatter = new CustomFormatter();
            DownloadOptions.Empty = new DownloadOptions(string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxBasePathLength = 1024;

        public DownloadOptions(string basePath)
        {
            if (basePath is null) throw new System.ArgumentNullException("basePath");
            if (basePath.Length > 1024) throw new System.ArgumentOutOfRangeException("basePath");

            this.BasePath = basePath;

            {
                var __h = new System.HashCode();
                if (this.BasePath != default) __h.Add(this.BasePath.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string BasePath { get; }

        public override bool Equals(DownloadOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BasePath != target.BasePath) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<DownloadOptions>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, DownloadOptions value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.BasePath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.BasePath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.BasePath);
                }
            }

            public DownloadOptions Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_basePath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // BasePath
                            {
                                p_basePath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new DownloadOptions(p_basePath);
            }
        }
    }

}
