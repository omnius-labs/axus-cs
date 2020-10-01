using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Components.Models;

#nullable enable

namespace Omnius.Xeus.Service.Models
{
    public readonly partial struct GetMyNodeProfileResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult>.Formatter;
        public static global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult>.Empty;

        static GetMyNodeProfileResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult>.Empty = new global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult(NodeProfile.Empty);
        }

        private readonly int ___hashCode;

        public GetMyNodeProfileResult(NodeProfile nodeProfile)
        {
            if (nodeProfile is null) throw new global::System.ArgumentNullException("nodeProfile");

            this.NodeProfile = nodeProfile;

            {
                var ___h = new global::System.HashCode();
                if (nodeProfile != default) ___h.Add(nodeProfile.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public NodeProfile NodeProfile { get; }

        public static global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult left, global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult left, global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult target)
        {
            if (this.NodeProfile != target.NodeProfile) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
            }

            public global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                NodeProfile p_nodeProfile = NodeProfile.Empty;

                {
                    p_nodeProfile = global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                }
                return new global::Omnius.Xeus.Service.Models.GetMyNodeProfileResult(p_nodeProfile);
            }
        }
    }
    public readonly partial struct AddCloudNodeProfilesParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam>.Formatter;
        public static global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam>.Empty;

        static AddCloudNodeProfilesParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam>.Empty = new global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam(global::System.Array.Empty<NodeProfile>());
        }

        private readonly int ___hashCode;

        public static readonly int MaxNodeProfilesCount = 2147483647;

        public AddCloudNodeProfilesParam(NodeProfile[] nodeProfiles)
        {
            if (nodeProfiles is null) throw new global::System.ArgumentNullException("nodeProfiles");
            if (nodeProfiles.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("nodeProfiles");
            foreach (var n in nodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.NodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(nodeProfiles);

            {
                var ___h = new global::System.HashCode();
                foreach (var n in nodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                ___hashCode = ___h.ToHashCode();
            }
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> NodeProfiles { get; }

        public static global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam left, global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam left, global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfiles, target.NodeProfiles)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)value.NodeProfiles.Count);
                foreach (var n in value.NodeProfiles)
                {
                    global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                NodeProfile[] p_nodeProfiles = global::System.Array.Empty<NodeProfile>();

                {
                    var length = r.GetUInt32();
                    p_nodeProfiles = new NodeProfile[length];
                    for (int i = 0; i < p_nodeProfiles.Length; i++)
                    {
                        p_nodeProfiles[i] = global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                    }
                }
                return new global::Omnius.Xeus.Service.Models.AddCloudNodeProfilesParam(p_nodeProfiles);
            }
        }
    }
    public readonly partial struct FindNodeProfilesParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam>.Formatter;
        public static global::Omnius.Xeus.Service.Models.FindNodeProfilesParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam>.Empty;

        static FindNodeProfilesParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam>.Empty = new global::Omnius.Xeus.Service.Models.FindNodeProfilesParam(ResourceTag.Empty);
        }

        private readonly int ___hashCode;

        public FindNodeProfilesParam(ResourceTag resourceTag)
        {
            if (resourceTag is null) throw new global::System.ArgumentNullException("resourceTag");

            this.ResourceTag = resourceTag;

            {
                var ___h = new global::System.HashCode();
                if (resourceTag != default) ___h.Add(resourceTag.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public ResourceTag ResourceTag { get; }

        public static global::Omnius.Xeus.Service.Models.FindNodeProfilesParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.FindNodeProfilesParam left, global::Omnius.Xeus.Service.Models.FindNodeProfilesParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.FindNodeProfilesParam left, global::Omnius.Xeus.Service.Models.FindNodeProfilesParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.FindNodeProfilesParam)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.FindNodeProfilesParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.FindNodeProfilesParam target)
        {
            if (this.ResourceTag != target.ResourceTag) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Service.Models.FindNodeProfilesParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Service.Models.FindNodeProfilesParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                global::Omnius.Xeus.Components.Models.ResourceTag.Formatter.Serialize(ref w, value.ResourceTag, rank + 1);
            }

            public global::Omnius.Xeus.Service.Models.FindNodeProfilesParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                ResourceTag p_resourceTag = ResourceTag.Empty;

                {
                    p_resourceTag = global::Omnius.Xeus.Components.Models.ResourceTag.Formatter.Deserialize(ref r, rank + 1);
                }
                return new global::Omnius.Xeus.Service.Models.FindNodeProfilesParam(p_resourceTag);
            }
        }
    }
    public readonly partial struct FindNodeProfilesResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult>.Formatter;
        public static global::Omnius.Xeus.Service.Models.FindNodeProfilesResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult>.Empty;

        static FindNodeProfilesResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult>.Empty = new global::Omnius.Xeus.Service.Models.FindNodeProfilesResult(global::System.Array.Empty<NodeProfile>());
        }

        private readonly int ___hashCode;

        public static readonly int MaxNodeProfilesCount = 2147483647;

        public FindNodeProfilesResult(NodeProfile[] nodeProfiles)
        {
            if (nodeProfiles is null) throw new global::System.ArgumentNullException("nodeProfiles");
            if (nodeProfiles.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("nodeProfiles");
            foreach (var n in nodeProfiles)
            {
                if (n is null) throw new global::System.ArgumentNullException("n");
            }

            this.NodeProfiles = new global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile>(nodeProfiles);

            {
                var ___h = new global::System.HashCode();
                foreach (var n in nodeProfiles)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                ___hashCode = ___h.ToHashCode();
            }
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<NodeProfile> NodeProfiles { get; }

        public static global::Omnius.Xeus.Service.Models.FindNodeProfilesResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Service.Models.FindNodeProfilesResult left, global::Omnius.Xeus.Service.Models.FindNodeProfilesResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Service.Models.FindNodeProfilesResult left, global::Omnius.Xeus.Service.Models.FindNodeProfilesResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Service.Models.FindNodeProfilesResult)) return false;
            return this.Equals((global::Omnius.Xeus.Service.Models.FindNodeProfilesResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Service.Models.FindNodeProfilesResult target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfiles, target.NodeProfiles)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Service.Models.FindNodeProfilesResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Service.Models.FindNodeProfilesResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                w.Write((uint)value.NodeProfiles.Count);
                foreach (var n in value.NodeProfiles)
                {
                    global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                }
            }

            public global::Omnius.Xeus.Service.Models.FindNodeProfilesResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                NodeProfile[] p_nodeProfiles = global::System.Array.Empty<NodeProfile>();

                {
                    var length = r.GetUInt32();
                    p_nodeProfiles = new NodeProfile[length];
                    for (int i = 0; i < p_nodeProfiles.Length; i++)
                    {
                        p_nodeProfiles[i] = global::Omnius.Xeus.Components.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                    }
                }
                return new global::Omnius.Xeus.Service.Models.FindNodeProfilesResult(p_nodeProfiles);
            }
        }
    }
}
