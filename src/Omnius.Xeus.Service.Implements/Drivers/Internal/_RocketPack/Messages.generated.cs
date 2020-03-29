using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Drivers;

#nullable enable

namespace Omnius.Xeus.Service.Drivers.Internal
{
    internal sealed partial class ConnectionHelloMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionHelloMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConnectionHelloMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionHelloMessage>.Formatter;
        public static ConnectionHelloMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionHelloMessage>.Empty;

        static ConnectionHelloMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionHelloMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<ConnectionHelloMessage>.Empty = new ConnectionHelloMessage(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxServiceTypeLength = 256;

        public ConnectionHelloMessage(string serviceType)
        {
            if (serviceType is null) throw new global::System.ArgumentNullException("serviceType");
            if (serviceType.Length > 256) throw new global::System.ArgumentOutOfRangeException("serviceType");

            this.ServiceType = serviceType;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (serviceType != default) ___h.Add(serviceType.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ServiceType { get; }

        public static ConnectionHelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(ConnectionHelloMessage? left, ConnectionHelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(ConnectionHelloMessage? left, ConnectionHelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is ConnectionHelloMessage)) return false;
            return this.Equals((ConnectionHelloMessage)other);
        }
        public bool Equals(ConnectionHelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ServiceType != target.ServiceType) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<ConnectionHelloMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in ConnectionHelloMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ServiceType != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ServiceType != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ServiceType);
                }
            }

            public ConnectionHelloMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_serviceType = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_serviceType = r.GetString(256);
                                break;
                            }
                    }
                }

                return new ConnectionHelloMessage(p_serviceType);
            }
        }
    }

}
