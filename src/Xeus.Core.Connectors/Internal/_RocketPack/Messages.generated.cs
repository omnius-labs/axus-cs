using Omnix.Algorithms.Cryptography;
using Omnix.Network;
using Xeus.Core;

#nullable enable

namespace Xeus.Core.Connectors.Internal
{
    internal sealed partial class TcpConnectorConfig : global::Omnix.Serialization.RocketPack.IRocketPackMessage<TcpConnectorConfig>
    {
        public static global::Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpConnectorConfig> Formatter { get; }
        public static TcpConnectorConfig Empty { get; }

        static TcpConnectorConfig()
        {
            TcpConnectorConfig.Formatter = new ___CustomFormatter();
            TcpConnectorConfig.Empty = new TcpConnectorConfig(0, null, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpConnectorConfig(uint version, TcpConnectOptions? tcpConnectOptions, TcpAcceptOptions? tcpAcceptOptions)
        {
            this.Version = version;
            this.TcpConnectOptions = tcpConnectOptions;
            this.TcpAcceptOptions = tcpAcceptOptions;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (version != default) ___h.Add(version.GetHashCode());
                if (tcpConnectOptions != default) ___h.Add(tcpConnectOptions.GetHashCode());
                if (tcpAcceptOptions != default) ___h.Add(tcpAcceptOptions.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public uint Version { get; }
        public TcpConnectOptions? TcpConnectOptions { get; }
        public TcpAcceptOptions? TcpAcceptOptions { get; }

        public static TcpConnectorConfig Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnix.Base.BufferPool bufferPool)
        {
            var reader = new global::Omnix.Serialization.RocketPack.RocketPackReader(sequence, bufferPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnix.Base.BufferPool bufferPool)
        {
            var writer = new global::Omnix.Serialization.RocketPack.RocketPackWriter(bufferWriter, bufferPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(TcpConnectorConfig? left, TcpConnectorConfig? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(TcpConnectorConfig? left, TcpConnectorConfig? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is TcpConnectorConfig)) return false;
            return this.Equals((TcpConnectorConfig)other);
        }
        public bool Equals(TcpConnectorConfig? target)
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
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnix.Serialization.RocketPack.IRocketPackFormatter<TcpConnectorConfig>
        {
            public void Serialize(ref global::Omnix.Serialization.RocketPack.RocketPackWriter w, in TcpConnectorConfig value, in int rank)
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
                    TcpConnectOptions.Formatter.Serialize(ref w, value.TcpConnectOptions, rank + 1);
                }
                if (value.TcpAcceptOptions != null)
                {
                    w.Write((uint)2);
                    TcpAcceptOptions.Formatter.Serialize(ref w, value.TcpAcceptOptions, rank + 1);
                }
            }

            public TcpConnectorConfig Deserialize(ref global::Omnix.Serialization.RocketPack.RocketPackReader r, in int rank)
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
                                p_tcpConnectOptions = TcpConnectOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_tcpAcceptOptions = TcpAcceptOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new TcpConnectorConfig(p_version, p_tcpConnectOptions, p_tcpAcceptOptions);
            }
        }
    }

}
