using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Drivers;

#nullable enable

namespace Omnius.Xeus.Service.Drivers.Internal
{
    internal sealed partial class ConnectionHelloMessage : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage>.Formatter;
        public static global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage>.Empty;

        static ConnectionHelloMessage()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage>.Empty = new global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxServiceLength = 256;

        public ConnectionHelloMessage(string service)
        {
            if (service is null) throw new global::System.ArgumentNullException("service");
            if (service.Length > 256) throw new global::System.ArgumentOutOfRangeException("service");

            this.Service = service;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (service != default) ___h.Add(service.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string Service { get; }

        public static global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage? left, global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage? left, global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Service != target.Service) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Service != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Service != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Service);
                }
            }

            public global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_service = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_service = r.GetString(256);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Drivers.Internal.ConnectionHelloMessage(p_service);
            }
        }
    }

}
