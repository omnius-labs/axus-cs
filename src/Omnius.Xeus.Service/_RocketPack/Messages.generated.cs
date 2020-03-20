using Omnius.Core.Cryptography;
using Omnius.Core.Network;

#nullable enable

namespace Omnius.Xeus.Service
{
    public enum TcpProxyType : byte
    {
        HttpProxy = 0,
        Socks5Proxy = 1,
    }

    public enum EventReportType : byte
    {
        SpaceNotFound = 0,
    }

    public sealed partial class TcpProxyOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpProxyOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpProxyOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpProxyOptions>.Formatter;
        public static TcpProxyOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpProxyOptions>.Empty;

        static TcpProxyOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpProxyOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpProxyOptions>.Empty = new TcpProxyOptions((TcpProxyType)0, OmniAddress.Empty);
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

        public static TcpProxyOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
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

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpProxyOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in TcpProxyOptions value, in int rank)
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

            public TcpProxyOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

    public sealed partial class TcpConnectOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpConnectOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectOptions>.Formatter;
        public static TcpConnectOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectOptions>.Empty;

        static TcpConnectOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectOptions>.Empty = new TcpConnectOptions(false, null);
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

        public static TcpConnectOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
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

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpConnectOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in TcpConnectOptions value, in int rank)
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

            public TcpConnectOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

    public sealed partial class TcpAcceptOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpAcceptOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpAcceptOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpAcceptOptions>.Formatter;
        public static TcpAcceptOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpAcceptOptions>.Empty;

        static TcpAcceptOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpAcceptOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpAcceptOptions>.Empty = new TcpAcceptOptions(false, global::System.Array.Empty<OmniAddress>(), false);
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

        public static TcpAcceptOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
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
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.ListenAddresses, target.ListenAddresses)) return false;
            if (this.UseUpnp != target.UseUpnp) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpAcceptOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in TcpAcceptOptions value, in int rank)
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

            public TcpAcceptOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

    public sealed partial class TcpConnectorOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectorOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpConnectorOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectorOptions>.Formatter;
        public static TcpConnectorOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectorOptions>.Empty;

        static TcpConnectorOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectorOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<TcpConnectorOptions>.Empty = new TcpConnectorOptions(TcpConnectOptions.Empty, TcpAcceptOptions.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpConnectorOptions(TcpConnectOptions connectOptions, TcpAcceptOptions acceptOptions)
        {
            if (connectOptions is null) throw new global::System.ArgumentNullException("connectOptions");
            if (acceptOptions is null) throw new global::System.ArgumentNullException("acceptOptions");

            this.ConnectOptions = connectOptions;
            this.AcceptOptions = acceptOptions;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (connectOptions != default) ___h.Add(connectOptions.GetHashCode());
                if (acceptOptions != default) ___h.Add(acceptOptions.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public TcpConnectOptions ConnectOptions { get; }
        public TcpAcceptOptions AcceptOptions { get; }

        public static TcpConnectorOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(TcpConnectorOptions? left, TcpConnectorOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(TcpConnectorOptions? left, TcpConnectorOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is TcpConnectorOptions)) return false;
            return this.Equals((TcpConnectorOptions)other);
        }
        public bool Equals(TcpConnectorOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConnectOptions != target.ConnectOptions) return false;
            if (this.AcceptOptions != target.AcceptOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpConnectorOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in TcpConnectorOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConnectOptions != TcpConnectOptions.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.AcceptOptions != TcpAcceptOptions.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConnectOptions != TcpConnectOptions.Empty)
                {
                    w.Write((uint)0);
                    TcpConnectOptions.Formatter.Serialize(ref w, value.ConnectOptions, rank + 1);
                }
                if (value.AcceptOptions != TcpAcceptOptions.Empty)
                {
                    w.Write((uint)1);
                    TcpAcceptOptions.Formatter.Serialize(ref w, value.AcceptOptions, rank + 1);
                }
            }

            public TcpConnectorOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                TcpConnectOptions p_connectOptions = TcpConnectOptions.Empty;
                TcpAcceptOptions p_acceptOptions = TcpAcceptOptions.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_connectOptions = TcpConnectOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                p_acceptOptions = TcpAcceptOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpConnectorOptions(p_connectOptions, p_acceptOptions);
            }
        }
    }

    public sealed partial class ExplorerOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ExplorerOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ExplorerOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ExplorerOptions>.Formatter;
        public static ExplorerOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ExplorerOptions>.Empty;

        static ExplorerOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ExplorerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ExplorerOptions>.Empty = new ExplorerOptions(0, 0, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ExplorerOptions(uint maxConnectionCount, uint maxBytesSendPerSecond, uint maxBytesReceivePerSecond)
        {
            this.MaxConnectionCount = maxConnectionCount;
            this.MaxBytesSendPerSecond = maxBytesSendPerSecond;
            this.MaxBytesReceivePerSecond = maxBytesReceivePerSecond;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                if (maxBytesSendPerSecond != default) ___h.Add(maxBytesSendPerSecond.GetHashCode());
                if (maxBytesReceivePerSecond != default) ___h.Add(maxBytesReceivePerSecond.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint MaxConnectionCount { get; }
        public uint MaxBytesSendPerSecond { get; }
        public uint MaxBytesReceivePerSecond { get; }

        public static ExplorerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ExplorerOptions? left, ExplorerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ExplorerOptions? left, ExplorerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ExplorerOptions)) return false;
            return this.Equals((ExplorerOptions)other);
        }
        public bool Equals(ExplorerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;
            if (this.MaxBytesSendPerSecond != target.MaxBytesSendPerSecond) return false;
            if (this.MaxBytesReceivePerSecond != target.MaxBytesReceivePerSecond) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ExplorerOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ExplorerOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.MaxBytesSendPerSecond != 0)
                    {
                        propertyCount++;
                    }
                    if (value.MaxBytesReceivePerSecond != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.MaxConnectionCount);
                }
                if (value.MaxBytesSendPerSecond != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.MaxBytesSendPerSecond);
                }
                if (value.MaxBytesReceivePerSecond != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.MaxBytesReceivePerSecond);
                }
            }

            public ExplorerOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_maxConnectionCount = 0;
                uint p_maxBytesSendPerSecond = 0;
                uint p_maxBytesReceivePerSecond = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                        case 1:
                            {
                                p_maxBytesSendPerSecond = r.GetUInt32();
                                break;
                            }
                        case 2:
                            {
                                p_maxBytesReceivePerSecond = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new ExplorerOptions(p_maxConnectionCount, p_maxBytesSendPerSecond, p_maxBytesReceivePerSecond);
            }
        }
    }

    public sealed partial class NegotiatorOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NegotiatorOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NegotiatorOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NegotiatorOptions>.Formatter;
        public static NegotiatorOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NegotiatorOptions>.Empty;

        static NegotiatorOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NegotiatorOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NegotiatorOptions>.Empty = new NegotiatorOptions(0, 0, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public NegotiatorOptions(uint maxConnectionCount, uint maxBytesSendPerSecond, uint maxBytesReceivePerSecond)
        {
            this.MaxConnectionCount = maxConnectionCount;
            this.MaxBytesSendPerSecond = maxBytesSendPerSecond;
            this.MaxBytesReceivePerSecond = maxBytesReceivePerSecond;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                if (maxBytesSendPerSecond != default) ___h.Add(maxBytesSendPerSecond.GetHashCode());
                if (maxBytesReceivePerSecond != default) ___h.Add(maxBytesReceivePerSecond.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint MaxConnectionCount { get; }
        public uint MaxBytesSendPerSecond { get; }
        public uint MaxBytesReceivePerSecond { get; }

        public static NegotiatorOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(NegotiatorOptions? left, NegotiatorOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(NegotiatorOptions? left, NegotiatorOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is NegotiatorOptions)) return false;
            return this.Equals((NegotiatorOptions)other);
        }
        public bool Equals(NegotiatorOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;
            if (this.MaxBytesSendPerSecond != target.MaxBytesSendPerSecond) return false;
            if (this.MaxBytesReceivePerSecond != target.MaxBytesReceivePerSecond) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NegotiatorOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in NegotiatorOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.MaxConnectionCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.MaxBytesSendPerSecond != 0)
                    {
                        propertyCount++;
                    }
                    if (value.MaxBytesReceivePerSecond != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.MaxConnectionCount);
                }
                if (value.MaxBytesSendPerSecond != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.MaxBytesSendPerSecond);
                }
                if (value.MaxBytesReceivePerSecond != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.MaxBytesReceivePerSecond);
                }
            }

            public NegotiatorOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                uint p_maxConnectionCount = 0;
                uint p_maxBytesSendPerSecond = 0;
                uint p_maxBytesReceivePerSecond = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                        case 1:
                            {
                                p_maxBytesSendPerSecond = r.GetUInt32();
                                break;
                            }
                        case 2:
                            {
                                p_maxBytesReceivePerSecond = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new NegotiatorOptions(p_maxConnectionCount, p_maxBytesSendPerSecond, p_maxBytesReceivePerSecond);
            }
        }
    }

    public sealed partial class EventReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<EventReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<EventReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<EventReport>.Formatter;
        public static EventReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<EventReport>.Empty;

        static EventReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<EventReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<EventReport>.Empty = new EventReport(global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero, (EventReportType)0);
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

        public static EventReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(EventReport? left, EventReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(EventReport? left, EventReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is EventReport)) return false;
            return this.Equals((EventReport)other);
        }
        public bool Equals(EventReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Type != target.Type) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<EventReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in EventReport value, in int rank)
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

            public EventReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new EventReport(p_creationTime, p_type);
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

    public sealed partial class PublishReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>.Formatter;
        public static PublishReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>.Empty;

        static PublishReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<PublishReport>.Empty = new PublishReport(OmniHash.Empty, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxPublishBlocksCount = 1073741824;

        public PublishReport(OmniHash rootHash, OmniHash[] publishBlocks)
        {
            if (publishBlocks is null) throw new global::System.ArgumentNullException("publishBlocks");
            if (publishBlocks.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("publishBlocks");

            this.RootHash = rootHash;
            this.PublishBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(publishBlocks);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                foreach (var n in publishBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public OmniHash RootHash { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> PublishBlocks { get; }

        public static PublishReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishReport? left, PublishReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishReport? left, PublishReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishReport)) return false;
            return this.Equals((PublishReport)other);
        }
        public bool Equals(PublishReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.RootHash != target.RootHash) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PublishBlocks, target.PublishBlocks)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.RootHash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.PublishBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                if (value.PublishBlocks.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.PublishBlocks.Count);
                    foreach (var n in value.PublishBlocks)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public PublishReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_rootHash = OmniHash.Empty;
                OmniHash[] p_publishBlocks = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_rootHash = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_publishBlocks = new OmniHash[length];
                                for (int i = 0; i < p_publishBlocks.Length; i++)
                                {
                                    p_publishBlocks[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new PublishReport(p_rootHash, p_publishBlocks);
            }
        }
    }

    public sealed partial class WantReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantReport> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>.Formatter;
        public static WantReport Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>.Empty;

        static WantReport()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<WantReport>.Empty = new WantReport(OmniHash.Empty, global::System.Array.Empty<OmniHash>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxWantBlocksCount = 1073741824;

        public WantReport(OmniHash rootHash, OmniHash[] wantBlocks)
        {
            if (wantBlocks is null) throw new global::System.ArgumentNullException("wantBlocks");
            if (wantBlocks.Length > 1073741824) throw new global::System.ArgumentOutOfRangeException("wantBlocks");

            this.RootHash = rootHash;
            this.WantBlocks = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash>(wantBlocks);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                foreach (var n in wantBlocks)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public OmniHash RootHash { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniHash> WantBlocks { get; }

        public static WantReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantReport? left, WantReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantReport? left, WantReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantReport)) return false;
            return this.Equals((WantReport)other);
        }
        public bool Equals(WantReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.RootHash != target.RootHash) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantBlocks, target.WantBlocks)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.RootHash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.WantBlocks.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                if (value.WantBlocks.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.WantBlocks.Count);
                    foreach (var n in value.WantBlocks)
                    {
                        OmniHash.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
            }

            public WantReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                OmniHash p_rootHash = OmniHash.Empty;
                OmniHash[] p_wantBlocks = global::System.Array.Empty<OmniHash>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_rootHash = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_wantBlocks = new OmniHash[length];
                                for (int i = 0; i < p_wantBlocks.Length; i++)
                                {
                                    p_wantBlocks[i] = OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new WantReport(p_rootHash, p_wantBlocks);
            }
        }
    }

    public sealed partial class NodeProfile : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeProfile> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Formatter;
        public static NodeProfile Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Empty;

        static NodeProfile()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<NodeProfile>.Empty = new NodeProfile(global::System.Array.Empty<OmniAddress>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxAddressesCount = 32;

        public NodeProfile(OmniAddress[] addresses)
        {
            if (addresses is null) throw new global::System.ArgumentNullException("addresses");
            if (addresses.Length > 32) throw new global::System.ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Addresses = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress>(addresses);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in addresses)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

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
                    if (value.Addresses.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Addresses.Count != 0)
                {
                    w.Write((uint)0);
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

                OmniAddress[] p_addresses = global::System.Array.Empty<OmniAddress>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
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

                return new NodeProfile(p_addresses);
            }
        }
    }

}
