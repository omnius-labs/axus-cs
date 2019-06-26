using Omnix.Cryptography;
using Omnix.Network;
using Xeus.Messages;

#nullable enable

namespace Xeus.Core.Connections.Internal
{
    internal sealed partial class TcpConnectionCreatorConfig : Omnix.Serialization.RocketPack.RocketPackMessageBase<TcpConnectionCreatorConfig>
    {
        static TcpConnectionCreatorConfig()
        {
            TcpConnectionCreatorConfig.Formatter = new CustomFormatter();
            TcpConnectionCreatorConfig.Empty = new TcpConnectionCreatorConfig(0, null, null, System.Array.Empty<ushort>());
        }

        private readonly int __hashCode;

        public static readonly int MaxOpenedPortsByUpnpCount = 32;

        public TcpConnectionCreatorConfig(uint version, TcpConnectConfig? tcpConnectConfig, TcpAcceptConfig? tcpAcceptConfig, ushort[] openedPortsByUpnp)
        {
            if (openedPortsByUpnp is null) throw new System.ArgumentNullException("openedPortsByUpnp");
            if (openedPortsByUpnp.Length > 32) throw new System.ArgumentOutOfRangeException("openedPortsByUpnp");

            this.Version = version;
            this.TcpConnectConfig = tcpConnectConfig;
            this.TcpAcceptConfig = tcpAcceptConfig;
            this.OpenedPortsByUpnp = new Omnix.Collections.ReadOnlyListSlim<ushort>(openedPortsByUpnp);

            {
                var __h = new System.HashCode();
                if (this.Version != default) __h.Add(this.Version.GetHashCode());
                if (this.TcpConnectConfig != default) __h.Add(this.TcpConnectConfig.GetHashCode());
                if (this.TcpAcceptConfig != default) __h.Add(this.TcpAcceptConfig.GetHashCode());
                foreach (var n in this.OpenedPortsByUpnp)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public uint Version { get; }
        public TcpConnectConfig? TcpConnectConfig { get; }
        public TcpAcceptConfig? TcpAcceptConfig { get; }
        public Omnix.Collections.ReadOnlyListSlim<ushort> OpenedPortsByUpnp { get; }

        public override bool Equals(TcpConnectionCreatorConfig? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Version != target.Version) return false;
            if ((this.TcpConnectConfig is null) != (target.TcpConnectConfig is null)) return false;
            if (!(this.TcpConnectConfig is null) && !(target.TcpConnectConfig is null) && this.TcpConnectConfig != target.TcpConnectConfig) return false;
            if ((this.TcpAcceptConfig is null) != (target.TcpAcceptConfig is null)) return false;
            if (!(this.TcpAcceptConfig is null) && !(target.TcpAcceptConfig is null) && this.TcpAcceptConfig != target.TcpAcceptConfig) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.OpenedPortsByUpnp, target.OpenedPortsByUpnp)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpConnectionCreatorConfig>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, TcpConnectionCreatorConfig value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Version != 0)
                    {
                        propertyCount++;
                    }
                    if (value.TcpConnectConfig != null)
                    {
                        propertyCount++;
                    }
                    if (value.TcpAcceptConfig != null)
                    {
                        propertyCount++;
                    }
                    if (value.OpenedPortsByUpnp.Count != 0)
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
                if (value.TcpConnectConfig != null)
                {
                    w.Write((uint)1);
                    TcpConnectConfig.Formatter.Serialize(w, value.TcpConnectConfig, rank + 1);
                }
                if (value.TcpAcceptConfig != null)
                {
                    w.Write((uint)2);
                    TcpAcceptConfig.Formatter.Serialize(w, value.TcpAcceptConfig, rank + 1);
                }
                if (value.OpenedPortsByUpnp.Count != 0)
                {
                    w.Write((uint)3);
                    w.Write((uint)value.OpenedPortsByUpnp.Count);
                    foreach (var n in value.OpenedPortsByUpnp)
                    {
                        w.Write(n);
                    }
                }
            }

            public TcpConnectionCreatorConfig Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_version = 0;
                TcpConnectConfig? p_tcpConnectConfig = null;
                TcpAcceptConfig? p_tcpAcceptConfig = null;
                ushort[] p_openedPortsByUpnp = System.Array.Empty<ushort>();

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
                                p_tcpConnectConfig = TcpConnectConfig.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_tcpAcceptConfig = TcpAcceptConfig.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 3:
                            {
                                var length = r.GetUInt32();
                                p_openedPortsByUpnp = new ushort[length];
                                for (int i = 0; i < p_openedPortsByUpnp.Length; i++)
                                {
                                    p_openedPortsByUpnp[i] = r.GetUInt16();
                                }
                                break;
                            }
                    }
                }

                return new TcpConnectionCreatorConfig(p_version, p_tcpConnectConfig, p_tcpAcceptConfig, p_openedPortsByUpnp);
            }
        }
    }

}
