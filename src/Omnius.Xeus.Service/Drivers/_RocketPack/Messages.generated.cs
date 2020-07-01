using Omnius.Core.Cryptography;
using Omnius.Core.Network;

#nullable enable

namespace Omnius.Xeus.Service.Drivers
{
    public enum TcpProxyType : byte
    {
        Unknown = 0,
        HttpProxy = 1,
        Socks5Proxy = 2,
    }

    public enum EventReportType : byte
    {
        Unknown = 0,
        SpaceNotFound = 1,
    }

    public sealed partial class TcpProxyOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpProxyOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.TcpProxyOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpProxyOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Drivers.TcpProxyOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpProxyOptions>.Empty;

        static TcpProxyOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpProxyOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpProxyOptions>.Empty = new global::Omnius.Xeus.Service.Drivers.TcpProxyOptions((TcpProxyType)0, OmniAddress.Empty);
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

        public static global::Omnius.Xeus.Service.Drivers.TcpProxyOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Drivers.TcpProxyOptions? left, global::Omnius.Xeus.Service.Drivers.TcpProxyOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Drivers.TcpProxyOptions? left, global::Omnius.Xeus.Service.Drivers.TcpProxyOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Drivers.TcpProxyOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Drivers.TcpProxyOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Drivers.TcpProxyOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Address != target.Address) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.TcpProxyOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Drivers.TcpProxyOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Drivers.TcpProxyOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Drivers.TcpProxyOptions(p_type, p_address);
            }
        }
    }

    public sealed partial class TcpConnectOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpConnectOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.TcpConnectOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpConnectOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Drivers.TcpConnectOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpConnectOptions>.Empty;

        static TcpConnectOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpConnectOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpConnectOptions>.Empty = new global::Omnius.Xeus.Service.Drivers.TcpConnectOptions(false, null);
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

        public static global::Omnius.Xeus.Service.Drivers.TcpConnectOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Drivers.TcpConnectOptions? left, global::Omnius.Xeus.Service.Drivers.TcpConnectOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Drivers.TcpConnectOptions? left, global::Omnius.Xeus.Service.Drivers.TcpConnectOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Drivers.TcpConnectOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Drivers.TcpConnectOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Drivers.TcpConnectOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if ((this.ProxyOptions is null) != (target.ProxyOptions is null)) return false;
            if (!(this.ProxyOptions is null) && !(target.ProxyOptions is null) && this.ProxyOptions != target.ProxyOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.TcpConnectOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Drivers.TcpConnectOptions value, in int rank)
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
                    global::Omnius.Xeus.Service.Drivers.TcpProxyOptions.Formatter.Serialize(ref w, value.ProxyOptions, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Drivers.TcpConnectOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_proxyOptions = global::Omnius.Xeus.Service.Drivers.TcpProxyOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Drivers.TcpConnectOptions(p_enabled, p_proxyOptions);
            }
        }
    }

    public sealed partial class TcpAcceptOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions>.Empty;

        static TcpAcceptOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions>.Empty = new global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions(false, global::System.Array.Empty<OmniAddress>(), false);
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

        public static global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions? left, global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions? left, global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.ListenAddresses, target.ListenAddresses)) return false;
            if (this.UseUpnp != target.UseUpnp) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions(p_enabled, p_listenAddresses, p_useUpnp);
            }
        }
    }

    public sealed partial class BandwidthOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.BandwidthOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.BandwidthOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.BandwidthOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Drivers.BandwidthOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.BandwidthOptions>.Empty;

        static BandwidthOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.BandwidthOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.BandwidthOptions>.Empty = new global::Omnius.Xeus.Service.Drivers.BandwidthOptions(0, 0);
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

        public static global::Omnius.Xeus.Service.Drivers.BandwidthOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Drivers.BandwidthOptions? left, global::Omnius.Xeus.Service.Drivers.BandwidthOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Drivers.BandwidthOptions? left, global::Omnius.Xeus.Service.Drivers.BandwidthOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Drivers.BandwidthOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Drivers.BandwidthOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Drivers.BandwidthOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxSendBytesPerSeconds != target.MaxSendBytesPerSeconds) return false;
            if (this.MaxReceiveBytesPerSeconds != target.MaxReceiveBytesPerSeconds) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.BandwidthOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Drivers.BandwidthOptions value, in int rank)
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

            public global::Omnius.Xeus.Service.Drivers.BandwidthOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new global::Omnius.Xeus.Service.Drivers.BandwidthOptions(p_maxSendBytesPerSeconds, p_maxReceiveBytesPerSeconds);
            }
        }
    }

    public sealed partial class ConnectionControllerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions>.Empty;

        static ConnectionControllerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions>.Empty = new global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions(TcpConnectOptions.Empty, TcpAcceptOptions.Empty, BandwidthOptions.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ConnectionControllerOptions(TcpConnectOptions tcpConnectOptions, TcpAcceptOptions tcpAcceptOptions, BandwidthOptions bandwidthOptions)
        {
            if (tcpConnectOptions is null) throw new global::System.ArgumentNullException("tcpConnectOptions");
            if (tcpAcceptOptions is null) throw new global::System.ArgumentNullException("tcpAcceptOptions");
            if (bandwidthOptions is null) throw new global::System.ArgumentNullException("bandwidthOptions");

            this.TcpConnectOptions = tcpConnectOptions;
            this.TcpAcceptOptions = tcpAcceptOptions;
            this.BandwidthOptions = bandwidthOptions;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (tcpConnectOptions != default) ___h.Add(tcpConnectOptions.GetHashCode());
                if (tcpAcceptOptions != default) ___h.Add(tcpAcceptOptions.GetHashCode());
                if (bandwidthOptions != default) ___h.Add(bandwidthOptions.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public TcpConnectOptions TcpConnectOptions { get; }
        public TcpAcceptOptions TcpAcceptOptions { get; }
        public BandwidthOptions BandwidthOptions { get; }

        public static global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions? left, global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions? left, global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.TcpConnectOptions != target.TcpConnectOptions) return false;
            if (this.TcpAcceptOptions != target.TcpAcceptOptions) return false;
            if (this.BandwidthOptions != target.BandwidthOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions value, in int rank)
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
                    if (value.BandwidthOptions != BandwidthOptions.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.TcpConnectOptions != TcpConnectOptions.Empty)
                {
                    w.Write((uint)0);
                    global::Omnius.Xeus.Service.Drivers.TcpConnectOptions.Formatter.Serialize(ref w, value.TcpConnectOptions, rank + 1);
                }
                if (value.TcpAcceptOptions != TcpAcceptOptions.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions.Formatter.Serialize(ref w, value.TcpAcceptOptions, rank + 1);
                }
                if (value.BandwidthOptions != BandwidthOptions.Empty)
                {
                    w.Write((uint)2);
                    global::Omnius.Xeus.Service.Drivers.BandwidthOptions.Formatter.Serialize(ref w, value.BandwidthOptions, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                TcpConnectOptions p_tcpConnectOptions = TcpConnectOptions.Empty;
                TcpAcceptOptions p_tcpAcceptOptions = TcpAcceptOptions.Empty;
                BandwidthOptions p_bandwidthOptions = BandwidthOptions.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_tcpConnectOptions = global::Omnius.Xeus.Service.Drivers.TcpConnectOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_tcpAcceptOptions = global::Omnius.Xeus.Service.Drivers.TcpAcceptOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_bandwidthOptions = global::Omnius.Xeus.Service.Drivers.BandwidthOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Drivers.ConnectionControllerOptions(p_tcpConnectOptions, p_tcpAcceptOptions, p_bandwidthOptions);
            }
        }
    }

    public sealed partial class EventReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.EventReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.EventReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.EventReport>.Formatter;
        public static global::Omnius.Xeus.Service.Drivers.EventReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.EventReport>.Empty;

        static EventReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.EventReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.EventReport>.Empty = new global::Omnius.Xeus.Service.Drivers.EventReport(global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero, (EventReportType)0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public EventReport(global::Omnius.Core.Serialization.RocketPack.Timestamp creationTime, EventReportType type)
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

        public global::Omnius.Core.Serialization.RocketPack.Timestamp CreationTime { get; }
        public EventReportType Type { get; }

        public static global::Omnius.Xeus.Service.Drivers.EventReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Drivers.EventReport? left, global::Omnius.Xeus.Service.Drivers.EventReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Drivers.EventReport? left, global::Omnius.Xeus.Service.Drivers.EventReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Drivers.EventReport)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Drivers.EventReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Drivers.EventReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Type != target.Type) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.EventReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Drivers.EventReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CreationTime != global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Type != (EventReportType)0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.CreationTime != global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)0);
                    w.Write(value.CreationTime);
                }
                if (value.Type != (EventReportType)0)
                {
                    w.Write((uint)1);
                    w.Write((ulong)value.Type);
                }
            }

            public global::Omnius.Xeus.Service.Drivers.EventReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::Omnius.Core.Serialization.RocketPack.Timestamp p_creationTime = global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero;
                EventReportType p_type = (EventReportType)0;

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
                                p_type = (EventReportType)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Drivers.EventReport(p_creationTime, p_type);
            }
        }
    }

}
