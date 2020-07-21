using Omnius.Core.Cryptography;
using Omnius.Core.Network;

#nullable enable

namespace Omnius.Xeus.Service.Connectors
{
    public enum TcpProxyType : byte
    {
        Unknown = 0,
        HttpProxy = 1,
        Socks5Proxy = 2,
    }

    public sealed partial class TcpProxyOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpProxyOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.TcpProxyOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpProxyOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Connectors.TcpProxyOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpProxyOptions>.Empty;

        static TcpProxyOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpProxyOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpProxyOptions>.Empty = new global::Omnius.Xeus.Service.Connectors.TcpProxyOptions((TcpProxyType)0, OmniAddress.Empty);
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

        public static global::Omnius.Xeus.Service.Connectors.TcpProxyOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Connectors.TcpProxyOptions? left, global::Omnius.Xeus.Service.Connectors.TcpProxyOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Connectors.TcpProxyOptions? left, global::Omnius.Xeus.Service.Connectors.TcpProxyOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Connectors.TcpProxyOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Connectors.TcpProxyOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Connectors.TcpProxyOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Address != target.Address) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.TcpProxyOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Connectors.TcpProxyOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Connectors.TcpProxyOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Connectors.TcpProxyOptions(p_type, p_address);
            }
        }
    }

    public sealed partial class TcpConnectingOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions>.Empty;

        static TcpConnectingOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions>.Empty = new global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions(false, null);
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

        public static global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions? left, global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions? left, global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if ((this.ProxyOptions is null) != (target.ProxyOptions is null)) return false;
            if (!(this.ProxyOptions is null) && !(target.ProxyOptions is null) && this.ProxyOptions != target.ProxyOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions value, in int rank)
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
                    global::Omnius.Xeus.Service.Connectors.TcpProxyOptions.Formatter.Serialize(ref w, value.ProxyOptions, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_proxyOptions = global::Omnius.Xeus.Service.Connectors.TcpProxyOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions(p_enabled, p_proxyOptions);
            }
        }
    }

    public sealed partial class TcpAcceptingOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions>.Empty;

        static TcpAcceptingOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions>.Empty = new global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions(false, global::System.Array.Empty<OmniAddress>(), false);
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

        public static global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions? left, global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions? left, global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.ListenAddresses, target.ListenAddresses)) return false;
            if (this.UseUpnp != target.UseUpnp) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions(p_enabled, p_listenAddresses, p_useUpnp);
            }
        }
    }

    public sealed partial class BandwidthOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.BandwidthOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.BandwidthOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.BandwidthOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Connectors.BandwidthOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.BandwidthOptions>.Empty;

        static BandwidthOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.BandwidthOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.BandwidthOptions>.Empty = new global::Omnius.Xeus.Service.Connectors.BandwidthOptions(0, 0);
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

        public static global::Omnius.Xeus.Service.Connectors.BandwidthOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Connectors.BandwidthOptions? left, global::Omnius.Xeus.Service.Connectors.BandwidthOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Connectors.BandwidthOptions? left, global::Omnius.Xeus.Service.Connectors.BandwidthOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Connectors.BandwidthOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Connectors.BandwidthOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Connectors.BandwidthOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxSendBytesPerSeconds != target.MaxSendBytesPerSeconds) return false;
            if (this.MaxReceiveBytesPerSeconds != target.MaxReceiveBytesPerSeconds) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.BandwidthOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Connectors.BandwidthOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Connectors.BandwidthOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Connectors.BandwidthOptions(p_maxSendBytesPerSeconds, p_maxReceiveBytesPerSeconds);
            }
        }
    }

    public sealed partial class TcpConnectorOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions>.Empty;

        static TcpConnectorOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions>.Empty = new global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions(TcpConnectingOptions.Empty, TcpAcceptingOptions.Empty, BandwidthOptions.Empty);
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

        public static global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions? left, global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions? left, global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.TcpConnectingOptions != target.TcpConnectingOptions) return false;
            if (this.TcpAcceptingOptions != target.TcpAcceptingOptions) return false;
            if (this.BandwidthOptions != target.BandwidthOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions value, in int rank)
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
                    global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions.Formatter.Serialize(ref w, value.TcpConnectingOptions, rank + 1);
                }
                if (value.TcpAcceptingOptions != TcpAcceptingOptions.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions.Formatter.Serialize(ref w, value.TcpAcceptingOptions, rank + 1);
                }
                if (value.BandwidthOptions != BandwidthOptions.Empty)
                {
                    w.Write((uint)2);
                    global::Omnius.Xeus.Service.Connectors.BandwidthOptions.Formatter.Serialize(ref w, value.BandwidthOptions, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tcpConnectingOptions = global::Omnius.Xeus.Service.Connectors.TcpConnectingOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_tcpAcceptingOptions = global::Omnius.Xeus.Service.Connectors.TcpAcceptingOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_bandwidthOptions = global::Omnius.Xeus.Service.Connectors.BandwidthOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Connectors.TcpConnectorOptions(p_tcpConnectingOptions, p_tcpAcceptingOptions, p_bandwidthOptions);
            }
        }
    }

}
