using Omnius.Core.Cryptography;
using Omnius.Core.Network;

#nullable enable

namespace Omnius.Xeus.Service.Repositories
{
    public sealed partial class WantContentRepositoryOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions>.Empty;

        static WantContentRepositoryOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions>.Empty = new global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public WantContentRepositoryOptions(string configPath)
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

        public static global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions? left, global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions? left, global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
            }

            public global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Repositories.WantContentRepositoryOptions(p_configPath);
            }
        }
    }

    public sealed partial class WantDeclaredMessageRepositoryOptions : global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions>
    {
        public static global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions> Formatter => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions>.Formatter;
        public static global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions Empty => global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions>.Empty;

        static WantDeclaredMessageRepositoryOptions()
        {
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.Serialization.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions>.Empty = new global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions(string.Empty);
        }

        private readonly global::System.Lazy<int> ___hashCode;

        public static readonly int MaxConfigPathLength = 1024;

        public WantDeclaredMessageRepositoryOptions(string configPath)
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

        public static global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.Serialization.RocketPack.RocketPackReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.Serialization.RocketPack.RocketPackWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions? left, global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions? right)
        {
            return (right is null) ? (left is null) : right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions? left, global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions? right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigPath != target.ConfigPath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode.Value;

        private sealed class ___CustomFormatter : global::Omnius.Core.Serialization.RocketPack.IRocketPackFormatter<global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions>
        {
            public void Serialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackWriter w, in global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigPath);
                }
            }

            public global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions Deserialize(ref global::Omnius.Core.Serialization.RocketPack.RocketPackReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                uint propertyCount = r.GetUInt32();

                string p_configPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0:
                            {
                                p_configPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Service.Repositories.WantDeclaredMessageRepositoryOptions(p_configPath);
            }
        }
    }

}
