using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Core.Internal;
using Xeus.Messages;

#nullable enable

namespace Xeus.Core.Internal.Connection
{
    internal sealed partial class TcpConnectionCreatorConfig : global::Omnix.Serialization.RocketPack.RocketPackMessageBase<TcpConnectionCreatorConfig>
    {
        static TcpConnectionCreatorConfig()
        {
            TcpConnectionCreatorConfig.Formatter = new CustomFormatter();
            TcpConnectionCreatorConfig.Empty = new TcpConnectionCreatorConfig(0, null, null);
        }

        private readonly int __hashCode;

        public TcpConnectionCreatorConfig(uint version, TcpConnectOptions? tcpConnectOptions, TcpAcceptOptions? tcpAcceptOptions)
        {
            this.Version = version;
            this.TcpConnectOptions = tcpConnectOptions;
            this.TcpAcceptOptions = tcpAcceptOptions;

            {
                var __h = new global::System.HashCode();
                if (this.Version != default) __h.Add(this.Version.GetHashCode());
                if (this.TcpConnectOptions != default) __h.Add(this.TcpConnectOptions.GetHashCode());
                if (this.TcpAcceptOptions != default) __h.Add(this.TcpAcceptOptions.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public uint Version { get; }
        public TcpConnectOptions? TcpConnectOptions { get; }
        public TcpAcceptOptions? TcpAcceptOptions { get; }

        public override bool Equals(TcpConnectionCreatorConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if ((this.TcpConnectOptions is null) != (target.TcpConnectOptions is null)) return false;
            if (!(this.TcpConnectOptions is null) && !(target.TcpConnectOptions is null) && this.TcpConnectOptions != target.TcpConnectOptions) return false;
            if ((this.TcpAcceptOptions is null) != (target.TcpAcceptOptions is null)) return false;
            if (!(this.TcpAcceptOptions is null) && !(target.TcpAcceptOptions is null) && this.TcpAcceptOptions != target.TcpAcceptOptions) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpConnectionCreatorConfig>
        {
            public void Serialize(global::Omnix.Serialization.RocketPack.RocketPackWriter w, TcpConnectionCreatorConfig value, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Version != 0)
                    {
                        propertyCount++;
                    }
                    if (value.TcpConnectOptions != null)
                    {
                        propertyCount++;
                    }
                    if (value.TcpAcceptOptions != null)
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
                if (value.TcpConnectOptions != null)
                {
                    w.Write((uint)1);
                    TcpConnectOptions.Formatter.Serialize(w, value.TcpConnectOptions, rank + 1);
                }
                if (value.TcpAcceptOptions != null)
                {
                    w.Write((uint)2);
                    TcpAcceptOptions.Formatter.Serialize(w, value.TcpAcceptOptions, rank + 1);
                }
            }

            public TcpConnectionCreatorConfig Deserialize(global::Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_version = 0;
                TcpConnectOptions? p_tcpConnectOptions = null;
                TcpAcceptOptions? p_tcpAcceptOptions = null;

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
                        case 1:
                            {
                                p_tcpConnectOptions = TcpConnectOptions.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_tcpAcceptOptions = TcpAcceptOptions.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpConnectionCreatorConfig(p_version, p_tcpConnectOptions, p_tcpAcceptOptions);
            }
        }
    }

}
