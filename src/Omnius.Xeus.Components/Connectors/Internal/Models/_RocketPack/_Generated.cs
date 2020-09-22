#nullable enable

namespace Omnius.Xeus.Components.Connectors.Internal.Models
{

    internal sealed partial class ConnectionHelloMessage : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage>.Formatter;
        public static global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage>.Empty;

        static ConnectionHelloMessage()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage>.Empty = new global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage(string.Empty);
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

        public static global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage? left, global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage? left, global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Service != target.Service) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Service != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Service);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_service = string.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_service = r.GetString(256);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Connectors.Internal.Models.ConnectionHelloMessage(p_service);
            }
        }
    }


}
