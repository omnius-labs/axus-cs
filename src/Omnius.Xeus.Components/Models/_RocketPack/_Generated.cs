using Omnius.Core.Cryptography;
using Omnius.Core.Network;

#nullable enable

namespace Omnius.Xeus.Components.Models
{
    public enum ConnectionType : byte
    {
        Unknown = 0,
        Connected = 1,
        Accepted = 2,
    }
    public enum TcpProxyType : byte
    {
        Unknown = 0,
        HttpProxy = 1,
        Socks5Proxy = 2,
    }

    public sealed partial class NodeProfile : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeProfile>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.NodeProfile> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeProfile>.Formatter;
        public static global::Omnius.Xeus.Components.Models.NodeProfile Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeProfile>.Empty;

        static NodeProfile()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeProfile>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeProfile>.Empty = new global::Omnius.Xeus.Components.Models.NodeProfile(global::System.Array.Empty<string>(), global::System.Array.Empty<OmniAddress>());
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxAvailableEngineNamesCount = 32;
        public static readonly int MaxAddressesCount = 32;

        public NodeProfile(string[] availableEngineNames, OmniAddress[] addresses)
        {
            if (availableEngineNames is null) throw new global::System.ArgumentNullException("availableEngineNames");
            if (availableEngineNames.Length > 32) throw new global::System.ArgumentOutOfRangeException("availableEngineNames");
            foreach (var n in availableEngineNames)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
                if (n.Length > 256) throw new global::System.ArgumentOutOfRangeException("n");
            }
            if (addresses is null) throw new global::System.ArgumentNullException("addresses");
            if (addresses.Length > 32) throw new global::System.ArgumentOutOfRangeException("addresses");
            foreach (var n in addresses)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.AvailableEngineNames = new global::Omnius.Core.Collections.ReadOnlyListSlim<string>(availableEngineNames);
            this.Addresses = new global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress>(addresses);

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                foreach (var n in availableEngineNames)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                foreach (var n in addresses)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<string> AvailableEngineNames { get; }
        public global::Omnius.Core.Collections.ReadOnlyListSlim<OmniAddress> Addresses { get; }

        public static global::Omnius.Xeus.Components.Models.NodeProfile Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.NodeProfile? left, global::Omnius.Xeus.Components.Models.NodeProfile? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.NodeProfile? left, global::Omnius.Xeus.Components.Models.NodeProfile? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.NodeProfile)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.NodeProfile)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.NodeProfile? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.AvailableEngineNames, target.AvailableEngineNames)) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.Addresses, target.Addresses)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.NodeProfile>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.NodeProfile value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.AvailableEngineNames.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.AvailableEngineNames.Count);
                    foreach (var n in value.AvailableEngineNames)
                    {
                        w.Write(n);
                    }
                }
                if (value.Addresses.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.Addresses.Count);
                    foreach (var n in value.Addresses)
                    {
                        global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.NodeProfile Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string[] p_availableEngineNames = global::System.Array.Empty<string>();
                OmniAddress[] p_addresses = global::System.Array.Empty<OmniAddress>();

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_availableEngineNames = new string[length];
                                for (int i = 0; i < p_availableEngineNames.Length; i++)
                                {
                                    p_availableEngineNames[i] = r.GetString(256);
                                }
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_addresses = new OmniAddress[length];
                                for (int i = 0; i < p_addresses.Length; i++)
                                {
                                    p_addresses[i] = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.NodeProfile(p_availableEngineNames, p_addresses);
            }
        }
    }
    public sealed partial class ResourceTag : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ResourceTag>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ResourceTag> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ResourceTag>.Formatter;
        public static global::Omnius.Xeus.Components.Models.ResourceTag Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ResourceTag>.Empty;

        static ResourceTag()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ResourceTag>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ResourceTag>.Empty = new global::Omnius.Xeus.Components.Models.ResourceTag(string.Empty, OmniHash.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxTypeLength = 256;

        public ResourceTag(string type, OmniHash hash)
        {
            if (type is null) throw new global::System.ArgumentNullException("type");
            if (type.Length > 256) throw new global::System.ArgumentOutOfRangeException("type");
            this.Type = type;
            this.Hash = hash;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (hash != default) ___h.Add(hash.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string Type { get; }
        public OmniHash Hash { get; }

        public static global::Omnius.Xeus.Components.Models.ResourceTag Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.ResourceTag? left, global::Omnius.Xeus.Components.Models.ResourceTag? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.ResourceTag? left, global::Omnius.Xeus.Components.Models.ResourceTag? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.ResourceTag)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.ResourceTag)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.ResourceTag? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.Hash != target.Hash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ResourceTag>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.ResourceTag value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Type != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Type);
                }
                if (value.Hash != OmniHash.Empty)
                {
                    w.Write((uint)2);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.Hash, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.ResourceTag Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_type = string.Empty;
                OmniHash p_hash = OmniHash.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_type = r.GetString(256);
                                break;
                            }
                        case 2:
                            {
                                p_hash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.ResourceTag(p_type, p_hash);
            }
        }
    }
    public sealed partial class DeclaredMessage : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessage>, global::System.IDisposable
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.DeclaredMessage> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessage>.Formatter;
        public static global::Omnius.Xeus.Components.Models.DeclaredMessage Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessage>.Empty;

        static DeclaredMessage()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessage>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessage>.Empty = new global::Omnius.Xeus.Components.Models.DeclaredMessage(global::Omnius.Core.RocketPack.Timestamp.Zero, global::Omnius.Core.MemoryOwner<byte>.Empty, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxValueLength = 33554432;

        public DeclaredMessage(global::Omnius.Core.RocketPack.Timestamp creationTime, global::System.Buffers.IMemoryOwner<byte> value, OmniCertificate? certificate)
        {
            if (value is null) throw new global::System.ArgumentNullException("value");
            if (value.Memory.Length > 33554432) throw new global::System.ArgumentOutOfRangeException("value");
            this.CreationTime = creationTime;
            _value = value;
            this.Certificate = certificate;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (creationTime != default) ___h.Add(creationTime.GetHashCode());
                if (!value.Memory.IsEmpty) ___h.Add(global::Omnius.Core.Helpers.ObjectHelper.GetHashCode(value.Memory.Span));
                if (certificate != default) ___h.Add(certificate.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public global::Omnius.Core.RocketPack.Timestamp CreationTime { get; }
        private readonly global::System.Buffers.IMemoryOwner<byte> _value;
        public global::System.ReadOnlyMemory<byte> Value => _value.Memory;
        public OmniCertificate? Certificate { get; }

        public static global::Omnius.Xeus.Components.Models.DeclaredMessage Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.DeclaredMessage? left, global::Omnius.Xeus.Components.Models.DeclaredMessage? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.DeclaredMessage? left, global::Omnius.Xeus.Components.Models.DeclaredMessage? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.DeclaredMessage)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.DeclaredMessage)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.DeclaredMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (!global::Omnius.Core.BytesOperations.Equals(this.Value.Span, target.Value.Span)) return false;
            if ((this.Certificate is null) != (target.Certificate is null)) return false;
            if (!(this.Certificate is null) && !(target.Certificate is null) && this.Certificate != target.Certificate) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        public void Dispose()
        {
            _value?.Dispose();
        }

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.DeclaredMessage>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.DeclaredMessage value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.CreationTime != global::Omnius.Core.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)1);
                    w.Write(value.CreationTime);
                }
                if (!value.Value.IsEmpty)
                {
                    w.Write((uint)2);
                    w.Write(value.Value.Span);
                }
                if (value.Certificate != null)
                {
                    w.Write((uint)3);
                    global::Omnius.Core.Cryptography.OmniCertificate.Formatter.Serialize(ref w, value.Certificate, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.DeclaredMessage Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                global::Omnius.Core.RocketPack.Timestamp p_creationTime = global::Omnius.Core.RocketPack.Timestamp.Zero;
                global::System.Buffers.IMemoryOwner<byte> p_value = global::Omnius.Core.MemoryOwner<byte>.Empty;
                OmniCertificate? p_certificate = null;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 2:
                            {
                                p_value = r.GetRecyclableMemory(33554432);
                                break;
                            }
                        case 3:
                            {
                                p_certificate = global::Omnius.Core.Cryptography.OmniCertificate.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.DeclaredMessage(p_creationTime, p_value, p_certificate);
            }
        }
    }
    public sealed partial class ConsistencyReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConsistencyReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ConsistencyReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConsistencyReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.ConsistencyReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConsistencyReport>.Empty;

        static ConsistencyReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConsistencyReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConsistencyReport>.Empty = new global::Omnius.Xeus.Components.Models.ConsistencyReport(0, 0, 0);
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

        public static global::Omnius.Xeus.Components.Models.ConsistencyReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.ConsistencyReport? left, global::Omnius.Xeus.Components.Models.ConsistencyReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.ConsistencyReport? left, global::Omnius.Xeus.Components.Models.ConsistencyReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.ConsistencyReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.ConsistencyReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.ConsistencyReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ConsistencyReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.ConsistencyReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.BadBlockCount != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.BadBlockCount);
                }
                if (value.CheckedBlockCount != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.CheckedBlockCount);
                }
                if (value.TotalBlockCount != 0)
                {
                    w.Write((uint)3);
                    w.Write(value.TotalBlockCount);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.ConsistencyReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint p_badBlockCount = 0;
                uint p_checkedBlockCount = 0;
                uint p_totalBlockCount = 0;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_badBlockCount = r.GetUInt32();
                                break;
                            }
                        case 2:
                            {
                                p_checkedBlockCount = r.GetUInt32();
                                break;
                            }
                        case 3:
                            {
                                p_totalBlockCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.ConsistencyReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }
    public sealed partial class ConnectionReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConnectionReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ConnectionReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConnectionReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.ConnectionReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConnectionReport>.Empty;

        static ConnectionReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConnectionReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ConnectionReport>.Empty = new global::Omnius.Xeus.Components.Models.ConnectionReport((ConnectionType)0, OmniAddress.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ConnectionReport(ConnectionType type, OmniAddress endPoint)
        {
            if (endPoint is null) throw new global::System.ArgumentNullException("endPoint");

            this.Type = type;
            this.EndPoint = endPoint;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (type != default) ___h.Add(type.GetHashCode());
                if (endPoint != default) ___h.Add(endPoint.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public ConnectionType Type { get; }
        public OmniAddress EndPoint { get; }

        public static global::Omnius.Xeus.Components.Models.ConnectionReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.ConnectionReport? left, global::Omnius.Xeus.Components.Models.ConnectionReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.ConnectionReport? left, global::Omnius.Xeus.Components.Models.ConnectionReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.ConnectionReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.ConnectionReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.ConnectionReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.EndPoint != target.EndPoint) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ConnectionReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.ConnectionReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Type != (ConnectionType)0)
                {
                    w.Write((uint)1);
                    w.Write((ulong)value.Type);
                }
                if (value.EndPoint != OmniAddress.Empty)
                {
                    w.Write((uint)2);
                    global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, value.EndPoint, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.ConnectionReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                ConnectionType p_type = (ConnectionType)0;
                OmniAddress p_endPoint = OmniAddress.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_type = (ConnectionType)r.GetUInt64();
                                break;
                            }
                        case 2:
                            {
                                p_endPoint = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.ConnectionReport(p_type, p_endPoint);
            }
        }
    }
    public sealed partial class TcpProxyOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpProxyOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpProxyOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpProxyOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.TcpProxyOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpProxyOptions>.Empty;

        static TcpProxyOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpProxyOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpProxyOptions>.Empty = new global::Omnius.Xeus.Components.Models.TcpProxyOptions((TcpProxyType)0, null);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpProxyOptions(TcpProxyType type, OmniAddress? address)
        {
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
        public OmniAddress? Address { get; }

        public static global::Omnius.Xeus.Components.Models.TcpProxyOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.TcpProxyOptions? left, global::Omnius.Xeus.Components.Models.TcpProxyOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.TcpProxyOptions? left, global::Omnius.Xeus.Components.Models.TcpProxyOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.TcpProxyOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.TcpProxyOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.TcpProxyOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if ((this.Address is null) != (target.Address is null)) return false;
            if (!(this.Address is null) && !(target.Address is null) && this.Address != target.Address) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpProxyOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.TcpProxyOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Type != (TcpProxyType)0)
                {
                    w.Write((uint)1);
                    w.Write((ulong)value.Type);
                }
                if (value.Address != null)
                {
                    w.Write((uint)2);
                    global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, value.Address, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.TcpProxyOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                TcpProxyType p_type = (TcpProxyType)0;
                OmniAddress? p_address = null;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_type = (TcpProxyType)r.GetUInt64();
                                break;
                            }
                        case 2:
                            {
                                p_address = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.TcpProxyOptions(p_type, p_address);
            }
        }
    }
    public sealed partial class TcpConnectingOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectingOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpConnectingOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectingOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.TcpConnectingOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectingOptions>.Empty;

        static TcpConnectingOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectingOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectingOptions>.Empty = new global::Omnius.Xeus.Components.Models.TcpConnectingOptions(false, null);
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

        public static global::Omnius.Xeus.Components.Models.TcpConnectingOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.TcpConnectingOptions? left, global::Omnius.Xeus.Components.Models.TcpConnectingOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.TcpConnectingOptions? left, global::Omnius.Xeus.Components.Models.TcpConnectingOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.TcpConnectingOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.TcpConnectingOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.TcpConnectingOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if ((this.ProxyOptions is null) != (target.ProxyOptions is null)) return false;
            if (!(this.ProxyOptions is null) && !(target.ProxyOptions is null) && this.ProxyOptions != target.ProxyOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpConnectingOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.TcpConnectingOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Enabled != false)
                {
                    w.Write((uint)1);
                    w.Write(value.Enabled);
                }
                if (value.ProxyOptions != null)
                {
                    w.Write((uint)2);
                    global::Omnius.Xeus.Components.Models.TcpProxyOptions.Formatter.Serialize(ref w, value.ProxyOptions, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.TcpConnectingOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                bool p_enabled = false;
                TcpProxyOptions? p_proxyOptions = null;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_enabled = r.GetBoolean();
                                break;
                            }
                        case 2:
                            {
                                p_proxyOptions = global::Omnius.Xeus.Components.Models.TcpProxyOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.TcpConnectingOptions(p_enabled, p_proxyOptions);
            }
        }
    }
    public sealed partial class TcpAcceptingOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpAcceptingOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpAcceptingOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpAcceptingOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.TcpAcceptingOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpAcceptingOptions>.Empty;

        static TcpAcceptingOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpAcceptingOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpAcceptingOptions>.Empty = new global::Omnius.Xeus.Components.Models.TcpAcceptingOptions(false, global::System.Array.Empty<OmniAddress>(), false);
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

        public static global::Omnius.Xeus.Components.Models.TcpAcceptingOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.TcpAcceptingOptions? left, global::Omnius.Xeus.Components.Models.TcpAcceptingOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.TcpAcceptingOptions? left, global::Omnius.Xeus.Components.Models.TcpAcceptingOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.TcpAcceptingOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.TcpAcceptingOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.TcpAcceptingOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Enabled != target.Enabled) return false;
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.ListenAddresses, target.ListenAddresses)) return false;
            if (this.UseUpnp != target.UseUpnp) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpAcceptingOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.TcpAcceptingOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Enabled != false)
                {
                    w.Write((uint)1);
                    w.Write(value.Enabled);
                }
                if (value.ListenAddresses.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.ListenAddresses.Count);
                    foreach (var n in value.ListenAddresses)
                    {
                        global::Omnius.Core.Network.OmniAddress.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                if (value.UseUpnp != false)
                {
                    w.Write((uint)3);
                    w.Write(value.UseUpnp);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.TcpAcceptingOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                bool p_enabled = false;
                OmniAddress[] p_listenAddresses = global::System.Array.Empty<OmniAddress>();
                bool p_useUpnp = false;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_enabled = r.GetBoolean();
                                break;
                            }
                        case 2:
                            {
                                var length = r.GetUInt32();
                                p_listenAddresses = new OmniAddress[length];
                                for (int i = 0; i < p_listenAddresses.Length; i++)
                                {
                                    p_listenAddresses[i] = global::Omnius.Core.Network.OmniAddress.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                        case 3:
                            {
                                p_useUpnp = r.GetBoolean();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.TcpAcceptingOptions(p_enabled, p_listenAddresses, p_useUpnp);
            }
        }
    }
    public sealed partial class BandwidthOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.BandwidthOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.BandwidthOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.BandwidthOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.BandwidthOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.BandwidthOptions>.Empty;

        static BandwidthOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.BandwidthOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.BandwidthOptions>.Empty = new global::Omnius.Xeus.Components.Models.BandwidthOptions(0, 0);
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

        public static global::Omnius.Xeus.Components.Models.BandwidthOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.BandwidthOptions? left, global::Omnius.Xeus.Components.Models.BandwidthOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.BandwidthOptions? left, global::Omnius.Xeus.Components.Models.BandwidthOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.BandwidthOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.BandwidthOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.BandwidthOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.MaxSendBytesPerSeconds != target.MaxSendBytesPerSeconds) return false;
            if (this.MaxReceiveBytesPerSeconds != target.MaxReceiveBytesPerSeconds) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.BandwidthOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.BandwidthOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.MaxSendBytesPerSeconds != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.MaxSendBytesPerSeconds);
                }
                if (value.MaxReceiveBytesPerSeconds != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.MaxReceiveBytesPerSeconds);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.BandwidthOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint p_maxSendBytesPerSeconds = 0;
                uint p_maxReceiveBytesPerSeconds = 0;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_maxSendBytesPerSeconds = r.GetUInt32();
                                break;
                            }
                        case 2:
                            {
                                p_maxReceiveBytesPerSeconds = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.BandwidthOptions(p_maxSendBytesPerSeconds, p_maxReceiveBytesPerSeconds);
            }
        }
    }
    public sealed partial class TcpConnectorOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpConnectorOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.TcpConnectorOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorOptions>.Empty;

        static TcpConnectorOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorOptions>.Empty = new global::Omnius.Xeus.Components.Models.TcpConnectorOptions(TcpConnectingOptions.Empty, TcpAcceptingOptions.Empty, BandwidthOptions.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpConnectorOptions(TcpConnectingOptions connectingOptions, TcpAcceptingOptions acceptingOptions, BandwidthOptions bandwidthOptions)
        {
            if (connectingOptions is null) throw new global::System.ArgumentNullException("connectingOptions");
            if (acceptingOptions is null) throw new global::System.ArgumentNullException("acceptingOptions");
            if (bandwidthOptions is null) throw new global::System.ArgumentNullException("bandwidthOptions");

            this.ConnectingOptions = connectingOptions;
            this.AcceptingOptions = acceptingOptions;
            this.BandwidthOptions = bandwidthOptions;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (connectingOptions != default) ___h.Add(connectingOptions.GetHashCode());
                if (acceptingOptions != default) ___h.Add(acceptingOptions.GetHashCode());
                if (bandwidthOptions != default) ___h.Add(bandwidthOptions.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public TcpConnectingOptions ConnectingOptions { get; }
        public TcpAcceptingOptions AcceptingOptions { get; }
        public BandwidthOptions BandwidthOptions { get; }

        public static global::Omnius.Xeus.Components.Models.TcpConnectorOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.TcpConnectorOptions? left, global::Omnius.Xeus.Components.Models.TcpConnectorOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.TcpConnectorOptions? left, global::Omnius.Xeus.Components.Models.TcpConnectorOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.TcpConnectorOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.TcpConnectorOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.TcpConnectorOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConnectingOptions != target.ConnectingOptions) return false;
            if (this.AcceptingOptions != target.AcceptingOptions) return false;
            if (this.BandwidthOptions != target.BandwidthOptions) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpConnectorOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.TcpConnectorOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.ConnectingOptions != TcpConnectingOptions.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Components.Models.TcpConnectingOptions.Formatter.Serialize(ref w, value.ConnectingOptions, rank + 1);
                }
                if (value.AcceptingOptions != TcpAcceptingOptions.Empty)
                {
                    w.Write((uint)2);
                    global::Omnius.Xeus.Components.Models.TcpAcceptingOptions.Formatter.Serialize(ref w, value.AcceptingOptions, rank + 1);
                }
                if (value.BandwidthOptions != BandwidthOptions.Empty)
                {
                    w.Write((uint)3);
                    global::Omnius.Xeus.Components.Models.BandwidthOptions.Formatter.Serialize(ref w, value.BandwidthOptions, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.TcpConnectorOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                TcpConnectingOptions p_connectingOptions = TcpConnectingOptions.Empty;
                TcpAcceptingOptions p_acceptingOptions = TcpAcceptingOptions.Empty;
                BandwidthOptions p_bandwidthOptions = BandwidthOptions.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_connectingOptions = global::Omnius.Xeus.Components.Models.TcpConnectingOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_acceptingOptions = global::Omnius.Xeus.Components.Models.TcpAcceptingOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 3:
                            {
                                p_bandwidthOptions = global::Omnius.Xeus.Components.Models.BandwidthOptions.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.TcpConnectorOptions(p_connectingOptions, p_acceptingOptions, p_bandwidthOptions);
            }
        }
    }
    public sealed partial class TcpConnectorReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpConnectorReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.TcpConnectorReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorReport>.Empty;

        static TcpConnectorReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.TcpConnectorReport>.Empty = new global::Omnius.Xeus.Components.Models.TcpConnectorReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public TcpConnectorReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Components.Models.TcpConnectorReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.TcpConnectorReport? left, global::Omnius.Xeus.Components.Models.TcpConnectorReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.TcpConnectorReport? left, global::Omnius.Xeus.Components.Models.TcpConnectorReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.TcpConnectorReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.TcpConnectorReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.TcpConnectorReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.TcpConnectorReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.TcpConnectorReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.TcpConnectorReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();


                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Components.Models.TcpConnectorReport();
            }
        }
    }
    public sealed partial class NodeFinderOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.NodeFinderOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.NodeFinderOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderOptions>.Empty;

        static NodeFinderOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderOptions>.Empty = new global::Omnius.Xeus.Components.Models.NodeFinderOptions(string.Empty, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public NodeFinderOptions(string configPath, uint maxConnectionCount)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");
            this.ConfigPath = configPath;
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }
        public uint MaxConnectionCount { get; }

        public static global::Omnius.Xeus.Components.Models.NodeFinderOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.NodeFinderOptions? left, global::Omnius.Xeus.Components.Models.NodeFinderOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.NodeFinderOptions? left, global::Omnius.Xeus.Components.Models.NodeFinderOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.NodeFinderOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.NodeFinderOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.NodeFinderOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.NodeFinderOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.NodeFinderOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.ConfigPath);
                }
                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.MaxConnectionCount);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.NodeFinderOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_configPath = string.Empty;
                uint p_maxConnectionCount = 0;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                        case 2:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.NodeFinderOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }
    public sealed partial class NodeFinderReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.NodeFinderReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.NodeFinderReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderReport>.Empty;

        static NodeFinderReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.NodeFinderReport>.Empty = new global::Omnius.Xeus.Components.Models.NodeFinderReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public NodeFinderReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Components.Models.NodeFinderReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.NodeFinderReport? left, global::Omnius.Xeus.Components.Models.NodeFinderReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.NodeFinderReport? left, global::Omnius.Xeus.Components.Models.NodeFinderReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.NodeFinderReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.NodeFinderReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.NodeFinderReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.NodeFinderReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.NodeFinderReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.NodeFinderReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();


                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Components.Models.NodeFinderReport();
            }
        }
    }
    public sealed partial class ContentExchangerOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ContentExchangerOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.ContentExchangerOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerOptions>.Empty;

        static ContentExchangerOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerOptions>.Empty = new global::Omnius.Xeus.Components.Models.ContentExchangerOptions(string.Empty, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public ContentExchangerOptions(string configPath, uint maxConnectionCount)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");
            this.ConfigPath = configPath;
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }
        public uint MaxConnectionCount { get; }

        public static global::Omnius.Xeus.Components.Models.ContentExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.ContentExchangerOptions? left, global::Omnius.Xeus.Components.Models.ContentExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.ContentExchangerOptions? left, global::Omnius.Xeus.Components.Models.ContentExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.ContentExchangerOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.ContentExchangerOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.ContentExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ContentExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.ContentExchangerOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.ConfigPath);
                }
                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.MaxConnectionCount);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.ContentExchangerOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_configPath = string.Empty;
                uint p_maxConnectionCount = 0;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                        case 2:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.ContentExchangerOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }
    public sealed partial class ContentExchangerReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ContentExchangerReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.ContentExchangerReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerReport>.Empty;

        static ContentExchangerReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.ContentExchangerReport>.Empty = new global::Omnius.Xeus.Components.Models.ContentExchangerReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public ContentExchangerReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Components.Models.ContentExchangerReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.ContentExchangerReport? left, global::Omnius.Xeus.Components.Models.ContentExchangerReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.ContentExchangerReport? left, global::Omnius.Xeus.Components.Models.ContentExchangerReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.ContentExchangerReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.ContentExchangerReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.ContentExchangerReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.ContentExchangerReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.ContentExchangerReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.ContentExchangerReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();


                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Components.Models.ContentExchangerReport();
            }
        }
    }
    public sealed partial class PushContentStorageOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.PushContentStorageOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.PushContentStorageOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageOptions>.Empty;

        static PushContentStorageOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageOptions>.Empty = new global::Omnius.Xeus.Components.Models.PushContentStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public PushContentStorageOptions(string configPath)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");

            this.ConfigPath = configPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }

        public static global::Omnius.Xeus.Components.Models.PushContentStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.PushContentStorageOptions? left, global::Omnius.Xeus.Components.Models.PushContentStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.PushContentStorageOptions? left, global::Omnius.Xeus.Components.Models.PushContentStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.PushContentStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.PushContentStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.PushContentStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.PushContentStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.PushContentStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.ConfigPath);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.PushContentStorageOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_configPath = string.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.PushContentStorageOptions(p_configPath);
            }
        }
    }
    public sealed partial class PushContentStorageReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.PushContentStorageReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.PushContentStorageReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageReport>.Empty;

        static PushContentStorageReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushContentStorageReport>.Empty = new global::Omnius.Xeus.Components.Models.PushContentStorageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PushContentStorageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Components.Models.PushContentStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.PushContentStorageReport? left, global::Omnius.Xeus.Components.Models.PushContentStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.PushContentStorageReport? left, global::Omnius.Xeus.Components.Models.PushContentStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.PushContentStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.PushContentStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.PushContentStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.PushContentStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.PushContentStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.PushContentStorageReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();


                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Components.Models.PushContentStorageReport();
            }
        }
    }
    public sealed partial class WantContentStorageOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.WantContentStorageOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.WantContentStorageOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageOptions>.Empty;

        static WantContentStorageOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageOptions>.Empty = new global::Omnius.Xeus.Components.Models.WantContentStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public WantContentStorageOptions(string configPath)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");

            this.ConfigPath = configPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }

        public static global::Omnius.Xeus.Components.Models.WantContentStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.WantContentStorageOptions? left, global::Omnius.Xeus.Components.Models.WantContentStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.WantContentStorageOptions? left, global::Omnius.Xeus.Components.Models.WantContentStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.WantContentStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.WantContentStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.WantContentStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.WantContentStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.WantContentStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.ConfigPath);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.WantContentStorageOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_configPath = string.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.WantContentStorageOptions(p_configPath);
            }
        }
    }
    public sealed partial class WantContentStorageReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.WantContentStorageReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.WantContentStorageReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageReport>.Empty;

        static WantContentStorageReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantContentStorageReport>.Empty = new global::Omnius.Xeus.Components.Models.WantContentStorageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantContentStorageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Components.Models.WantContentStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.WantContentStorageReport? left, global::Omnius.Xeus.Components.Models.WantContentStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.WantContentStorageReport? left, global::Omnius.Xeus.Components.Models.WantContentStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.WantContentStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.WantContentStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.WantContentStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.WantContentStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.WantContentStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.WantContentStorageReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();


                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Components.Models.WantContentStorageReport();
            }
        }
    }
    public sealed partial class DeclaredMessageExchangerOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions>.Empty;

        static DeclaredMessageExchangerOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions>.Empty = new global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions(string.Empty, 0);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public DeclaredMessageExchangerOptions(string configPath, uint maxConnectionCount)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");
            this.ConfigPath = configPath;
            this.MaxConnectionCount = maxConnectionCount;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                if (maxConnectionCount != default) ___h.Add(maxConnectionCount.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }
        public uint MaxConnectionCount { get; }

        public static global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions? left, global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions? left, global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;
            if (this.MaxConnectionCount != target.MaxConnectionCount) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.ConfigPath);
                }
                if (value.MaxConnectionCount != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.MaxConnectionCount);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_configPath = string.Empty;
                uint p_maxConnectionCount = 0;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                        case 2:
                            {
                                p_maxConnectionCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerOptions(p_configPath, p_maxConnectionCount);
            }
        }
    }
    public sealed partial class DeclaredMessageExchangerReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport>.Empty;

        static DeclaredMessageExchangerReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport>.Empty = new global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public DeclaredMessageExchangerReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport? left, global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport? left, global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();


                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Components.Models.DeclaredMessageExchangerReport();
            }
        }
    }
    public sealed partial class PushDeclaredMessageStorageOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions>.Empty;

        static PushDeclaredMessageStorageOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions>.Empty = new global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public PushDeclaredMessageStorageOptions(string configPath)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");

            this.ConfigPath = configPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }

        public static global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions? left, global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions? left, global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.ConfigPath);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_configPath = string.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageOptions(p_configPath);
            }
        }
    }
    public sealed partial class PushDeclaredMessageStorageReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport>.Empty;

        static PushDeclaredMessageStorageReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport>.Empty = new global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public PushDeclaredMessageStorageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport? left, global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport? left, global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();


                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Components.Models.PushDeclaredMessageStorageReport();
            }
        }
    }
    public sealed partial class WantDeclaredMessageStorageOptions : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions>.Formatter;
        public static global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions>.Empty;

        static WantDeclaredMessageStorageOptions()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions>.Empty = new global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public WantDeclaredMessageStorageOptions(string configPath)
        {
            if (configPath is null) throw new global::System.ArgumentNullException("configPath");
            if (configPath.Length > 1024) throw new global::System.ArgumentOutOfRangeException("configPath");

            this.ConfigPath = configPath;

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                if (configPath != default) ___h.Add(configPath.GetHashCode());
                return ___h.ToHashCode();
            });
        }

        public string ConfigPath { get; }

        public static global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions? left, global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions? left, global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.ConfigPath);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_configPath = string.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageOptions(p_configPath);
            }
        }
    }
    public sealed partial class WantDeclaredMessageStorageReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport>.Formatter;
        public static global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport>.Empty;

        static WantDeclaredMessageStorageReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport>.Empty = new global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport();
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public WantDeclaredMessageStorageReport()
        {

            ___hashCode = new global::System.Lazy<int>(() =>
            {
                var ___h = new global::System.HashCode();
                return ___h.ToHashCode();
            });
        }


        public static global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport? left, global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport? left, global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();


                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                    }
                }

                return new global::Omnius.Xeus.Components.Models.WantDeclaredMessageStorageReport();
            }
        }
    }


}
