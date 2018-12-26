using Omnix.Base;
using Omnix.Base.Helpers;
using Omnix.Cryptography;
using Omnix.Serialization;
using Omnix.Serialization.RocketPack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xeus.Messages;

namespace Xeus.Messages.Options
{
    public enum ConnectionType : byte
    {
        None = 0,
        Tcp = 1,
        Socks5Proxy = 2,
        HttpProxy = 3,
    }

    public sealed partial class ConnectionFilter : RocketPackMessageBase<ConnectionFilter>
    {
        static ConnectionFilter()
        {
            ConnectionFilter.Formatter = new CustomFormatter();
        }

        public static readonly int MaxSchemeLength = 256;
        public static readonly int MaxProxyUriLength = 1024;

        public ConnectionFilter(string scheme, ConnectionType connectionType, string proxyUri)
        {
            if (scheme is null) throw new ArgumentNullException("scheme");
            if (scheme.Length > 256) throw new ArgumentOutOfRangeException("scheme");
            if (proxyUri is null) throw new ArgumentNullException("proxyUri");
            if (proxyUri.Length > 1024) throw new ArgumentOutOfRangeException("proxyUri");

            this.Scheme = scheme;
            this.ConnectionType = connectionType;
            this.ProxyUri = proxyUri;

            {
                var hashCode = new HashCode();
                if (this.Scheme != default) hashCode.Add(this.Scheme.GetHashCode());
                if (this.ConnectionType != default) hashCode.Add(this.ConnectionType.GetHashCode());
                if (this.ProxyUri != default) hashCode.Add(this.ProxyUri.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public string Scheme { get; }
        public ConnectionType ConnectionType { get; }
        public string ProxyUri { get; }

        public override bool Equals(ConnectionFilter target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Scheme != target.Scheme) return false;
            if (this.ConnectionType != target.ConnectionType) return false;
            if (this.ProxyUri != target.ProxyUri) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ConnectionFilter>
        {
            public void Serialize(RocketPackWriter w, ConnectionFilter value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Scheme != default) propertyCount++;
                    if (value.ConnectionType != default) propertyCount++;
                    if (value.ProxyUri != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Scheme
                if (value.Scheme != default)
                {
                    w.Write((ulong)0);
                    w.Write(value.Scheme);
                }
                // ConnectionType
                if (value.ConnectionType != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.ConnectionType);
                }
                // ProxyUri
                if (value.ProxyUri != default)
                {
                    w.Write((ulong)2);
                    w.Write(value.ProxyUri);
                }
            }

            public ConnectionFilter Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                string p_scheme = default;
                ConnectionType p_connectionType = default;
                string p_proxyUri = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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

    public sealed partial class ContentsOptions : RocketPackMessageBase<ContentsOptions>
    {
        static ContentsOptions()
        {
            ContentsOptions.Formatter = new CustomFormatter();
        }

        public ContentsOptions()
        {

            {
                var hashCode = new HashCode();
                _hashCode = hashCode.ToHashCode();
            }
        }


        public override bool Equals(ContentsOptions target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ContentsOptions>
        {
            public void Serialize(RocketPackWriter w, ContentsOptions value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    w.Write((ulong)propertyCount);
                }

            }

            public ContentsOptions Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();


                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                    }
                }

                return new ContentsOptions();
            }
        }
    }

    public sealed partial class DownloadOptions : RocketPackMessageBase<DownloadOptions>
    {
        static DownloadOptions()
        {
            DownloadOptions.Formatter = new CustomFormatter();
        }

        public static readonly int MaxBasePathLength = 1024;

        public DownloadOptions(string basePath)
        {
            if (basePath is null) throw new ArgumentNullException("basePath");
            if (basePath.Length > 1024) throw new ArgumentOutOfRangeException("basePath");

            this.BasePath = basePath;

            {
                var hashCode = new HashCode();
                if (this.BasePath != default) hashCode.Add(this.BasePath.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public string BasePath { get; }

        public override bool Equals(DownloadOptions target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.BasePath != target.BasePath) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<DownloadOptions>
        {
            public void Serialize(RocketPackWriter w, DownloadOptions value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.BasePath != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // BasePath
                if (value.BasePath != default)
                {
                    w.Write((ulong)0);
                    w.Write(value.BasePath);
                }
            }

            public DownloadOptions Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                string p_basePath = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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
