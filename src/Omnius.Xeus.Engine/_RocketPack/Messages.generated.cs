using Omnius.Core.Cryptography;
using Omnius.Core.Network;

#nullable enable

namespace Omnius.Xeus.Engine
{
    public enum ErrorReportType : byte
    {
        SpaceNotFound = 0,
    }

    public enum TcpProxyType : byte
    {
        HttpProxy = 0,
        Socks5Proxy = 1,
    }

    public sealed partial class ErrorReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<ErrorReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ErrorReport> Formatter { get; }
        public static ErrorReport Empty { get; }

        static ErrorReport()
        {
            ErrorReport.Formatter = new ___CustomFormatter();
            ErrorReport.Empty = new ErrorReport(global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero, (ErrorReportType)0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ErrorReport(global::Omnius.Core.Serialization.RocketPack.Timestamp creationTime, ErrorReportType type)
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
        public ErrorReportType Type { get; }

        public static ErrorReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ErrorReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ErrorReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CreationTime != global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Type != (ErrorReportType)0)
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
                if (value.Type != (ErrorReportType)0)
                {
                    w.Write((uint)1);
                    w.Write((ulong)value.Type);
                }
            }

            public ErrorReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                global::Omnius.Core.Serialization.RocketPack.Timestamp p_creationTime = global::Omnius.Core.Serialization.RocketPack.Timestamp.Zero;
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

    public sealed partial class TcpProxyOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<TcpProxyOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpProxyOptions> Formatter { get; }
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

        public static TcpProxyOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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

    public sealed partial class TcpConnectOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<TcpConnectOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpConnectOptions> Formatter { get; }
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

        public static TcpConnectOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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

    public sealed partial class TcpAcceptOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<TcpAcceptOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<TcpAcceptOptions> Formatter { get; }
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

        public static TcpAcceptOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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

    public sealed partial class CheckConsistencyReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<CheckConsistencyReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<CheckConsistencyReport> Formatter { get; }
        public static CheckConsistencyReport Empty { get; }

        static CheckConsistencyReport()
        {
            CheckConsistencyReport.Formatter = new ___CustomFormatter();
            CheckConsistencyReport.Empty = new CheckConsistencyReport(0, 0, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public CheckConsistencyReport(uint badBlockCount, uint checkedBlockCount, uint totalBlockCount)
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

        public static CheckConsistencyReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(CheckConsistencyReport? left, CheckConsistencyReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(CheckConsistencyReport? left, CheckConsistencyReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is CheckConsistencyReport)) return false;
            return this.Equals((CheckConsistencyReport)other);
        }
        public bool Equals(CheckConsistencyReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<CheckConsistencyReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in CheckConsistencyReport value, in int rank)
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

            public CheckConsistencyReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
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

                return new CheckConsistencyReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }

    public sealed partial class PublishMessageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<PublishMessageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishMessageReport> Formatter { get; }
        public static PublishMessageReport Empty { get; }

        static PublishMessageReport()
        {
            PublishMessageReport.Formatter = new ___CustomFormatter();
            PublishMessageReport.Empty = new PublishMessageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PublishMessageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static PublishMessageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishMessageReport? left, PublishMessageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishMessageReport? left, PublishMessageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishMessageReport)) return false;
            return this.Equals((PublishMessageReport)other);
        }
        public bool Equals(PublishMessageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishMessageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishMessageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public PublishMessageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new PublishMessageReport();
            }
        }
    }

    public sealed partial class WantMessageReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<WantMessageReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantMessageReport> Formatter { get; }
        public static WantMessageReport Empty { get; }

        static WantMessageReport()
        {
            WantMessageReport.Formatter = new ___CustomFormatter();
            WantMessageReport.Empty = new WantMessageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantMessageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static WantMessageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantMessageReport? left, WantMessageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantMessageReport? left, WantMessageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantMessageReport)) return false;
            return this.Equals((WantMessageReport)other);
        }
        public bool Equals(WantMessageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantMessageReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantMessageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public WantMessageReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new WantMessageReport();
            }
        }
    }

    public sealed partial class PublishFileReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<PublishFileReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishFileReport> Formatter { get; }
        public static PublishFileReport Empty { get; }

        static PublishFileReport()
        {
            PublishFileReport.Formatter = new ___CustomFormatter();
            PublishFileReport.Empty = new PublishFileReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PublishFileReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static PublishFileReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(PublishFileReport? left, PublishFileReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(PublishFileReport? left, PublishFileReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is PublishFileReport)) return false;
            return this.Equals((PublishFileReport)other);
        }
        public bool Equals(PublishFileReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<PublishFileReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in PublishFileReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public PublishFileReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new PublishFileReport();
            }
        }
    }

    public sealed partial class WantFileReport : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<WantFileReport>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantFileReport> Formatter { get; }
        public static WantFileReport Empty { get; }

        static WantFileReport()
        {
            WantFileReport.Formatter = new ___CustomFormatter();
            WantFileReport.Empty = new WantFileReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantFileReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static WantFileReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(WantFileReport? left, WantFileReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(WantFileReport? left, WantFileReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is WantFileReport)) return false;
            return this.Equals((WantFileReport)other);
        }
        public bool Equals(WantFileReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<WantFileReport>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in WantFileReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public WantFileReport Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new WantFileReport();
            }
        }
    }

    public sealed partial class NodeProfile : global::Omnius.Core.Serialization.RocketPack.IRocketPackMessage<NodeProfile>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<NodeProfile> Formatter { get; }
        public static NodeProfile Empty { get; }

        static NodeProfile()
        {
            NodeProfile.Formatter = new ___CustomFormatter();
            NodeProfile.Empty = new NodeProfile(global::System.ReadOnlyMemory<byte>.Empty, global::System.Array.Empty<OmniAddress>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxIdLength = 256;
        public static readonly int MaxAddressesCount = 32;

        public NodeProfile(global::System.ReadOnlyMemory<byte> id, OmniAddress[] addresses)
        {
            if (id.Length > 256) throw new global::System.ArgumentOutOfRangeException("id");
            if (addresses is null) throw new global::System.ArgumentNullException("addresses");
            if (addresses.Length > 32) throw new global::System.ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.Id = id;
            this.Addresses = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress>(addresses);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (!id.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(id.Span));
                foreach (var n in addresses)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::System.ReadOnlyMemory<byte> Id { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress> Addresses { get; }

        public static NodeProfile Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBufferPool<byte> bufferPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
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
            if (!global::Omnius.Core.BytesOperations.Equals(this.Id.Span, target.Id.Span)) return false;
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
                    if (!value.Id.IsEmpty)
                    {
                        propertyCount++;
                    }
                    if (value.Addresses.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (!value.Id.IsEmpty)
                {
                    w.Write((uint)0);
                    w.Write(value.Id.Span);
                }
                if (value.Addresses.Count != 0)
                {
                    w.Write((uint)1);
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

                global::System.ReadOnlyMemory<byte> p_id = global::System.ReadOnlyMemory<byte>.Empty;
                OmniAddress[] p_addresses = global::System.Array.Empty<OmniAddress>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_id = r.GetMemory(256);
                                break;
                            }
                        case 1:
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

                return new NodeProfile(p_id, p_addresses);
            }
        }
    }

}
