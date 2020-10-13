using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Xeus.Engines.Models;

#nullable enable

namespace Omnius.Xeus.Api.Models
{
    public readonly partial struct GetMyNodeProfileResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult>.Formatter;
        public static global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult>.Empty;

        static GetMyNodeProfileResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult>.Empty = new global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult(NodeProfile.Empty);
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

        public static global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult left, global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult left, global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult target)
        {
            if (this.NodeProfile != target.NodeProfile) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.NodeProfile != NodeProfile.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Engines.Models.NodeProfile.Formatter.Serialize(ref w, value.NodeProfile, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                NodeProfile p_nodeProfile = NodeProfile.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_nodeProfile = global::Omnius.Xeus.Engines.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.GetMyNodeProfileResult(p_nodeProfile);
            }
        }
    }
    public readonly partial struct AddCloudNodeProfilesParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam>.Empty;

        static AddCloudNodeProfilesParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam>.Empty = new global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam(global::System.Array.Empty<NodeProfile>());
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

        public static global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam left, global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam left, global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.NodeProfiles, target.NodeProfiles)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.NodeProfiles.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.NodeProfiles.Count);
                    foreach (var n in value.NodeProfiles)
                    {
                        global::Omnius.Xeus.Engines.Models.NodeProfile.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                NodeProfile[] p_nodeProfiles = global::System.Array.Empty<NodeProfile>();

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_nodeProfiles = new NodeProfile[length];
                                for (int i = 0; i < p_nodeProfiles.Length; i++)
                                {
                                    p_nodeProfiles[i] = global::Omnius.Xeus.Engines.Models.NodeProfile.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.AddCloudNodeProfilesParam(p_nodeProfiles);
            }
        }
    }
    public readonly partial struct PushContentReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushContentReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.PushContentReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushContentReport>.Formatter;
        public static global::Omnius.Xeus.Api.Models.PushContentReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushContentReport>.Empty;

        static PushContentReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushContentReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushContentReport>.Empty = new global::Omnius.Xeus.Api.Models.PushContentReport(string.Empty, OmniHash.Empty);
        }

        private readonly int ___hashCode;

        public static readonly int MaxFilePathLength = 2147483647;

        public PushContentReport(string filePath, OmniHash rootHash)
        {
            if (filePath is null) throw new global::System.ArgumentNullException("filePath");
            if (filePath.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("filePath");
            this.FilePath = filePath;
            this.RootHash = rootHash;

            {
                var ___h = new global::System.HashCode();
                if (filePath != default) ___h.Add(filePath.GetHashCode());
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public string FilePath { get; }
        public OmniHash RootHash { get; }

        public static global::Omnius.Xeus.Api.Models.PushContentReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.PushContentReport left, global::Omnius.Xeus.Api.Models.PushContentReport right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.PushContentReport left, global::Omnius.Xeus.Api.Models.PushContentReport right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.PushContentReport)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.PushContentReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.PushContentReport target)
        {
            if (this.FilePath != target.FilePath) return false;
            if (this.RootHash != target.RootHash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.PushContentReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.PushContentReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.FilePath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.FilePath);
                }
                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)2);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.PushContentReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_filePath = string.Empty;
                OmniHash p_rootHash = OmniHash.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_filePath = r.GetString(2147483647);
                                break;
                            }
                        case 2:
                            {
                                p_rootHash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.PushContentReport(p_filePath, p_rootHash);
            }
        }
    }
    public readonly partial struct GetPushContentsReportResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult>.Formatter;
        public static global::Omnius.Xeus.Api.Models.GetPushContentsReportResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult>.Empty;

        static GetPushContentsReportResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult>.Empty = new global::Omnius.Xeus.Api.Models.GetPushContentsReportResult(global::System.Array.Empty<PushContentReport>());
        }

        private readonly int ___hashCode;

        public static readonly int MaxPushContentsCount = 2147483647;

        public GetPushContentsReportResult(PushContentReport[] pushContents)
        {
            if (pushContents is null) throw new global::System.ArgumentNullException("pushContents");
            if (pushContents.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("pushContents");

            this.PushContents = new global::Omnius.Core.Collections.ReadOnlyListSlim<PushContentReport>(pushContents);

            {
                var ___h = new global::System.HashCode();
                foreach (var n in pushContents)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                ___hashCode = ___h.ToHashCode();
            }
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<PushContentReport> PushContents { get; }

        public static global::Omnius.Xeus.Api.Models.GetPushContentsReportResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.GetPushContentsReportResult left, global::Omnius.Xeus.Api.Models.GetPushContentsReportResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.GetPushContentsReportResult left, global::Omnius.Xeus.Api.Models.GetPushContentsReportResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.GetPushContentsReportResult)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.GetPushContentsReportResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.GetPushContentsReportResult target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushContents, target.PushContents)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetPushContentsReportResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.GetPushContentsReportResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.PushContents.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.PushContents.Count);
                    foreach (var n in value.PushContents)
                    {
                        global::Omnius.Xeus.Api.Models.PushContentReport.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.GetPushContentsReportResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                PushContentReport[] p_pushContents = global::System.Array.Empty<PushContentReport>();

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_pushContents = new PushContentReport[length];
                                for (int i = 0; i < p_pushContents.Length; i++)
                                {
                                    p_pushContents[i] = global::Omnius.Xeus.Api.Models.PushContentReport.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.GetPushContentsReportResult(p_pushContents);
            }
        }
    }
    public readonly partial struct RegisterPushContentParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterPushContentParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.RegisterPushContentParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentParam>.Empty;

        static RegisterPushContentParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentParam>.Empty = new global::Omnius.Xeus.Api.Models.RegisterPushContentParam(string.Empty);
        }

        private readonly int ___hashCode;

        public static readonly int MaxFilePathLength = 2147483647;

        public RegisterPushContentParam(string filePath)
        {
            if (filePath is null) throw new global::System.ArgumentNullException("filePath");
            if (filePath.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("filePath");

            this.FilePath = filePath;

            {
                var ___h = new global::System.HashCode();
                if (filePath != default) ___h.Add(filePath.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public string FilePath { get; }

        public static global::Omnius.Xeus.Api.Models.RegisterPushContentParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.RegisterPushContentParam left, global::Omnius.Xeus.Api.Models.RegisterPushContentParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.RegisterPushContentParam left, global::Omnius.Xeus.Api.Models.RegisterPushContentParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.RegisterPushContentParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.RegisterPushContentParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.RegisterPushContentParam target)
        {
            if (this.FilePath != target.FilePath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterPushContentParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.RegisterPushContentParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.FilePath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.FilePath);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.RegisterPushContentParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_filePath = string.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_filePath = r.GetString(2147483647);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.RegisterPushContentParam(p_filePath);
            }
        }
    }
    public readonly partial struct RegisterPushContentResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterPushContentResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentResult>.Formatter;
        public static global::Omnius.Xeus.Api.Models.RegisterPushContentResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentResult>.Empty;

        static RegisterPushContentResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushContentResult>.Empty = new global::Omnius.Xeus.Api.Models.RegisterPushContentResult(OmniHash.Empty);
        }

        private readonly int ___hashCode;

        public RegisterPushContentResult(OmniHash rootHash)
        {
            this.RootHash = rootHash;

            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniHash RootHash { get; }

        public static global::Omnius.Xeus.Api.Models.RegisterPushContentResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.RegisterPushContentResult left, global::Omnius.Xeus.Api.Models.RegisterPushContentResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.RegisterPushContentResult left, global::Omnius.Xeus.Api.Models.RegisterPushContentResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.RegisterPushContentResult)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.RegisterPushContentResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.RegisterPushContentResult target)
        {
            if (this.RootHash != target.RootHash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterPushContentResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.RegisterPushContentResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.RegisterPushContentResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniHash p_rootHash = OmniHash.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_rootHash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.RegisterPushContentResult(p_rootHash);
            }
        }
    }
    public readonly partial struct UnregisterPushContentParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.UnregisterPushContentParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam>.Empty;

        static UnregisterPushContentParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam>.Empty = new global::Omnius.Xeus.Api.Models.UnregisterPushContentParam(string.Empty);
        }

        private readonly int ___hashCode;

        public static readonly int MaxFilePathLength = 2147483647;

        public UnregisterPushContentParam(string filePath)
        {
            if (filePath is null) throw new global::System.ArgumentNullException("filePath");
            if (filePath.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("filePath");

            this.FilePath = filePath;

            {
                var ___h = new global::System.HashCode();
                if (filePath != default) ___h.Add(filePath.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public string FilePath { get; }

        public static global::Omnius.Xeus.Api.Models.UnregisterPushContentParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.UnregisterPushContentParam left, global::Omnius.Xeus.Api.Models.UnregisterPushContentParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.UnregisterPushContentParam left, global::Omnius.Xeus.Api.Models.UnregisterPushContentParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.UnregisterPushContentParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.UnregisterPushContentParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.UnregisterPushContentParam target)
        {
            if (this.FilePath != target.FilePath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.UnregisterPushContentParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.UnregisterPushContentParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.FilePath != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.FilePath);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.UnregisterPushContentParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                string p_filePath = string.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_filePath = r.GetString(2147483647);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.UnregisterPushContentParam(p_filePath);
            }
        }
    }
    public readonly partial struct WantContentReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantContentReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.WantContentReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantContentReport>.Formatter;
        public static global::Omnius.Xeus.Api.Models.WantContentReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantContentReport>.Empty;

        static WantContentReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantContentReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantContentReport>.Empty = new global::Omnius.Xeus.Api.Models.WantContentReport(OmniHash.Empty);
        }

        private readonly int ___hashCode;

        public WantContentReport(OmniHash rootHash)
        {
            this.RootHash = rootHash;

            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniHash RootHash { get; }

        public static global::Omnius.Xeus.Api.Models.WantContentReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.WantContentReport left, global::Omnius.Xeus.Api.Models.WantContentReport right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.WantContentReport left, global::Omnius.Xeus.Api.Models.WantContentReport right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.WantContentReport)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.WantContentReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.WantContentReport target)
        {
            if (this.RootHash != target.RootHash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.WantContentReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.WantContentReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.WantContentReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniHash p_rootHash = OmniHash.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_rootHash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.WantContentReport(p_rootHash);
            }
        }
    }
    public readonly partial struct GetWantContentsReportResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult>.Formatter;
        public static global::Omnius.Xeus.Api.Models.GetWantContentsReportResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult>.Empty;

        static GetWantContentsReportResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult>.Empty = new global::Omnius.Xeus.Api.Models.GetWantContentsReportResult(global::System.Array.Empty<WantContentReport>());
        }

        private readonly int ___hashCode;

        public static readonly int MaxWantContentsCount = 2147483647;

        public GetWantContentsReportResult(WantContentReport[] wantContents)
        {
            if (wantContents is null) throw new global::System.ArgumentNullException("wantContents");
            if (wantContents.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("wantContents");

            this.WantContents = new global::Omnius.Core.Collections.ReadOnlyListSlim<WantContentReport>(wantContents);

            {
                var ___h = new global::System.HashCode();
                foreach (var n in wantContents)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                ___hashCode = ___h.ToHashCode();
            }
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<WantContentReport> WantContents { get; }

        public static global::Omnius.Xeus.Api.Models.GetWantContentsReportResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.GetWantContentsReportResult left, global::Omnius.Xeus.Api.Models.GetWantContentsReportResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.GetWantContentsReportResult left, global::Omnius.Xeus.Api.Models.GetWantContentsReportResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.GetWantContentsReportResult)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.GetWantContentsReportResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.GetWantContentsReportResult target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantContents, target.WantContents)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetWantContentsReportResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.GetWantContentsReportResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.WantContents.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.WantContents.Count);
                    foreach (var n in value.WantContents)
                    {
                        global::Omnius.Xeus.Api.Models.WantContentReport.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.GetWantContentsReportResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                WantContentReport[] p_wantContents = global::System.Array.Empty<WantContentReport>();

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_wantContents = new WantContentReport[length];
                                for (int i = 0; i < p_wantContents.Length; i++)
                                {
                                    p_wantContents[i] = global::Omnius.Xeus.Api.Models.WantContentReport.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.GetWantContentsReportResult(p_wantContents);
            }
        }
    }
    public readonly partial struct RegisterWantContentParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantContentParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterWantContentParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantContentParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.RegisterWantContentParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantContentParam>.Empty;

        static RegisterWantContentParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantContentParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantContentParam>.Empty = new global::Omnius.Xeus.Api.Models.RegisterWantContentParam(OmniHash.Empty);
        }

        private readonly int ___hashCode;

        public RegisterWantContentParam(OmniHash rootHash)
        {
            this.RootHash = rootHash;

            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniHash RootHash { get; }

        public static global::Omnius.Xeus.Api.Models.RegisterWantContentParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.RegisterWantContentParam left, global::Omnius.Xeus.Api.Models.RegisterWantContentParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.RegisterWantContentParam left, global::Omnius.Xeus.Api.Models.RegisterWantContentParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.RegisterWantContentParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.RegisterWantContentParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.RegisterWantContentParam target)
        {
            if (this.RootHash != target.RootHash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterWantContentParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.RegisterWantContentParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.RegisterWantContentParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniHash p_rootHash = OmniHash.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_rootHash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.RegisterWantContentParam(p_rootHash);
            }
        }
    }
    public readonly partial struct UnregisterWantContentParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.UnregisterWantContentParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam>.Empty;

        static UnregisterWantContentParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam>.Empty = new global::Omnius.Xeus.Api.Models.UnregisterWantContentParam(OmniHash.Empty);
        }

        private readonly int ___hashCode;

        public UnregisterWantContentParam(OmniHash rootHash)
        {
            this.RootHash = rootHash;

            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniHash RootHash { get; }

        public static global::Omnius.Xeus.Api.Models.UnregisterWantContentParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.UnregisterWantContentParam left, global::Omnius.Xeus.Api.Models.UnregisterWantContentParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.UnregisterWantContentParam left, global::Omnius.Xeus.Api.Models.UnregisterWantContentParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.UnregisterWantContentParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.UnregisterWantContentParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.UnregisterWantContentParam target)
        {
            if (this.RootHash != target.RootHash) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.UnregisterWantContentParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.UnregisterWantContentParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.UnregisterWantContentParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniHash p_rootHash = OmniHash.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_rootHash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.UnregisterWantContentParam(p_rootHash);
            }
        }
    }
    public readonly partial struct ExportWantContentParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ExportWantContentParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.ExportWantContentParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ExportWantContentParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.ExportWantContentParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ExportWantContentParam>.Empty;

        static ExportWantContentParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ExportWantContentParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ExportWantContentParam>.Empty = new global::Omnius.Xeus.Api.Models.ExportWantContentParam(OmniHash.Empty, string.Empty);
        }

        private readonly int ___hashCode;

        public static readonly int MaxFilePathLength = 2147483647;

        public ExportWantContentParam(OmniHash rootHash, string filePath)
        {
            if (filePath is null) throw new global::System.ArgumentNullException("filePath");
            if (filePath.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("filePath");

            this.RootHash = rootHash;
            this.FilePath = filePath;

            {
                var ___h = new global::System.HashCode();
                if (rootHash != default) ___h.Add(rootHash.GetHashCode());
                if (filePath != default) ___h.Add(filePath.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniHash RootHash { get; }
        public string FilePath { get; }

        public static global::Omnius.Xeus.Api.Models.ExportWantContentParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.ExportWantContentParam left, global::Omnius.Xeus.Api.Models.ExportWantContentParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.ExportWantContentParam left, global::Omnius.Xeus.Api.Models.ExportWantContentParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.ExportWantContentParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.ExportWantContentParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.ExportWantContentParam target)
        {
            if (this.RootHash != target.RootHash) return false;
            if (this.FilePath != target.FilePath) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.ExportWantContentParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.ExportWantContentParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.RootHash != OmniHash.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniHash.Formatter.Serialize(ref w, value.RootHash, rank + 1);
                }
                if (value.FilePath != string.Empty)
                {
                    w.Write((uint)2);
                    w.Write(value.FilePath);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.ExportWantContentParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniHash p_rootHash = OmniHash.Empty;
                string p_filePath = string.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_rootHash = global::Omnius.Core.Cryptography.OmniHash.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                        case 2:
                            {
                                p_filePath = r.GetString(2147483647);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.ExportWantContentParam(p_rootHash, p_filePath);
            }
        }
    }
    public readonly partial struct PushDeclaredMessageReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport>.Formatter;
        public static global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport>.Empty;

        static PushDeclaredMessageReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport>.Empty = new global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport(OmniSignature.Empty);
        }

        private readonly int ___hashCode;

        public PushDeclaredMessageReport(OmniSignature signature)
        {
            if (signature is null) throw new global::System.ArgumentNullException("signature");

            this.Signature = signature;

            {
                var ___h = new global::System.HashCode();
                if (signature != default) ___h.Add(signature.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniSignature Signature { get; }

        public static global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport left, global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport left, global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport target)
        {
            if (this.Signature != target.Signature) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Signature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniSignature.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniSignature p_signature = OmniSignature.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_signature = global::Omnius.Core.Cryptography.OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport(p_signature);
            }
        }
    }
    public readonly partial struct GetPushDeclaredMessagesReportResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult>.Formatter;
        public static global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult>.Empty;

        static GetPushDeclaredMessagesReportResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult>.Empty = new global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult(global::System.Array.Empty<PushDeclaredMessageReport>());
        }

        private readonly int ___hashCode;

        public static readonly int MaxPushDeclaredMessageCount = 2147483647;

        public GetPushDeclaredMessagesReportResult(PushDeclaredMessageReport[] pushDeclaredMessage)
        {
            if (pushDeclaredMessage is null) throw new global::System.ArgumentNullException("pushDeclaredMessage");
            if (pushDeclaredMessage.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("pushDeclaredMessage");

            this.PushDeclaredMessage = new global::Omnius.Core.Collections.ReadOnlyListSlim<PushDeclaredMessageReport>(pushDeclaredMessage);

            {
                var ___h = new global::System.HashCode();
                foreach (var n in pushDeclaredMessage)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                ___hashCode = ___h.ToHashCode();
            }
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<PushDeclaredMessageReport> PushDeclaredMessage { get; }

        public static global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult left, global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult left, global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.PushDeclaredMessage, target.PushDeclaredMessage)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.PushDeclaredMessage.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.PushDeclaredMessage.Count);
                    foreach (var n in value.PushDeclaredMessage)
                    {
                        global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                PushDeclaredMessageReport[] p_pushDeclaredMessage = global::System.Array.Empty<PushDeclaredMessageReport>();

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_pushDeclaredMessage = new PushDeclaredMessageReport[length];
                                for (int i = 0; i < p_pushDeclaredMessage.Length; i++)
                                {
                                    p_pushDeclaredMessage[i] = global::Omnius.Xeus.Api.Models.PushDeclaredMessageReport.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.GetPushDeclaredMessagesReportResult(p_pushDeclaredMessage);
            }
        }
    }
    public readonly partial struct RegisterPushDeclaredMessageParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam>.Empty;

        static RegisterPushDeclaredMessageParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam>.Empty = new global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam(DeclaredMessage.Empty);
        }

        private readonly int ___hashCode;

        public RegisterPushDeclaredMessageParam(DeclaredMessage message)
        {
            if (message is null) throw new global::System.ArgumentNullException("message");

            this.Message = message;

            {
                var ___h = new global::System.HashCode();
                if (message != default) ___h.Add(message.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public DeclaredMessage Message { get; }

        public static global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam target)
        {
            if (this.Message != target.Message) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Message != DeclaredMessage.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Engines.Models.DeclaredMessage.Formatter.Serialize(ref w, value.Message, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                DeclaredMessage p_message = DeclaredMessage.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_message = global::Omnius.Xeus.Engines.Models.DeclaredMessage.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.RegisterPushDeclaredMessageParam(p_message);
            }
        }
    }
    public readonly partial struct UnregisterPushDeclaredMessageParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam>.Empty;

        static UnregisterPushDeclaredMessageParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam>.Empty = new global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam(OmniSignature.Empty);
        }

        private readonly int ___hashCode;

        public UnregisterPushDeclaredMessageParam(OmniSignature signature)
        {
            if (signature is null) throw new global::System.ArgumentNullException("signature");

            this.Signature = signature;

            {
                var ___h = new global::System.HashCode();
                if (signature != default) ___h.Add(signature.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniSignature Signature { get; }

        public static global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam target)
        {
            if (this.Signature != target.Signature) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Signature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniSignature.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniSignature p_signature = OmniSignature.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_signature = global::Omnius.Core.Cryptography.OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.UnregisterPushDeclaredMessageParam(p_signature);
            }
        }
    }
    public readonly partial struct ReadWantDeclaredMessageParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam>.Empty;

        static ReadWantDeclaredMessageParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam>.Empty = new global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam(OmniSignature.Empty);
        }

        private readonly int ___hashCode;

        public ReadWantDeclaredMessageParam(OmniSignature signature)
        {
            if (signature is null) throw new global::System.ArgumentNullException("signature");

            this.Signature = signature;

            {
                var ___h = new global::System.HashCode();
                if (signature != default) ___h.Add(signature.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniSignature Signature { get; }

        public static global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam target)
        {
            if (this.Signature != target.Signature) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Signature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniSignature.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniSignature p_signature = OmniSignature.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_signature = global::Omnius.Core.Cryptography.OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageParam(p_signature);
            }
        }
    }
    public readonly partial struct ReadWantDeclaredMessageResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult>.Formatter;
        public static global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult>.Empty;

        static ReadWantDeclaredMessageResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult>.Empty = new global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult(null);
        }

        private readonly int ___hashCode;

        public ReadWantDeclaredMessageResult(DeclaredMessage? signature)
        {
            this.Signature = signature;

            {
                var ___h = new global::System.HashCode();
                if (signature != default) ___h.Add(signature.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public DeclaredMessage? Signature { get; }

        public static global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult left, global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult left, global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult target)
        {
            if ((this.Signature is null) != (target.Signature is null)) return false;
            if (!(this.Signature is null) && !(target.Signature is null) && this.Signature != target.Signature) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Signature != null)
                {
                    w.Write((uint)1);
                    global::Omnius.Xeus.Engines.Models.DeclaredMessage.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                DeclaredMessage? p_signature = null;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_signature = global::Omnius.Xeus.Engines.Models.DeclaredMessage.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.ReadWantDeclaredMessageResult(p_signature);
            }
        }
    }
    public readonly partial struct WantDeclaredMessageReport : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport>.Formatter;
        public static global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport>.Empty;

        static WantDeclaredMessageReport()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport>.Empty = new global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport(OmniSignature.Empty);
        }

        private readonly int ___hashCode;

        public WantDeclaredMessageReport(OmniSignature signature)
        {
            if (signature is null) throw new global::System.ArgumentNullException("signature");

            this.Signature = signature;

            {
                var ___h = new global::System.HashCode();
                if (signature != default) ___h.Add(signature.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniSignature Signature { get; }

        public static global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport left, global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport left, global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport target)
        {
            if (this.Signature != target.Signature) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Signature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniSignature.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniSignature p_signature = OmniSignature.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_signature = global::Omnius.Core.Cryptography.OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport(p_signature);
            }
        }
    }
    public readonly partial struct GetWantDeclaredMessagesReportResult : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult>.Formatter;
        public static global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult>.Empty;

        static GetWantDeclaredMessagesReportResult()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult>.Empty = new global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult(global::System.Array.Empty<WantDeclaredMessageReport>());
        }

        private readonly int ___hashCode;

        public static readonly int MaxWantDeclaredMessageCount = 2147483647;

        public GetWantDeclaredMessagesReportResult(WantDeclaredMessageReport[] wantDeclaredMessage)
        {
            if (wantDeclaredMessage is null) throw new global::System.ArgumentNullException("wantDeclaredMessage");
            if (wantDeclaredMessage.Length > 2147483647) throw new global::System.ArgumentOutOfRangeException("wantDeclaredMessage");

            this.WantDeclaredMessage = new global::Omnius.Core.Collections.ReadOnlyListSlim<WantDeclaredMessageReport>(wantDeclaredMessage);

            {
                var ___h = new global::System.HashCode();
                foreach (var n in wantDeclaredMessage)
                {
                    if (n != default) ___h.Add(n.GetHashCode());
                }
                ___hashCode = ___h.ToHashCode();
            }
        }

        public global::Omnius.Core.Collections.ReadOnlyListSlim<WantDeclaredMessageReport> WantDeclaredMessage { get; }

        public static global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult left, global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult left, global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult target)
        {
            if (!global::Omnius.Core.Helpers.CollectionHelper.Equals(this.WantDeclaredMessage, target.WantDeclaredMessage)) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.WantDeclaredMessage.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.WantDeclaredMessage.Count);
                    foreach (var n in value.WantDeclaredMessage)
                    {
                        global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport.Formatter.Serialize(ref w, n, rank + 1);
                    }
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                WantDeclaredMessageReport[] p_wantDeclaredMessage = global::System.Array.Empty<WantDeclaredMessageReport>();

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                var length = r.GetUInt32();
                                p_wantDeclaredMessage = new WantDeclaredMessageReport[length];
                                for (int i = 0; i < p_wantDeclaredMessage.Length; i++)
                                {
                                    p_wantDeclaredMessage[i] = global::Omnius.Xeus.Api.Models.WantDeclaredMessageReport.Formatter.Deserialize(ref r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.GetWantDeclaredMessagesReportResult(p_wantDeclaredMessage);
            }
        }
    }
    public readonly partial struct RegisterWantDeclaredMessageParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam>.Empty;

        static RegisterWantDeclaredMessageParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam>.Empty = new global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam(OmniSignature.Empty);
        }

        private readonly int ___hashCode;

        public RegisterWantDeclaredMessageParam(OmniSignature signature)
        {
            if (signature is null) throw new global::System.ArgumentNullException("signature");

            this.Signature = signature;

            {
                var ___h = new global::System.HashCode();
                if (signature != default) ___h.Add(signature.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniSignature Signature { get; }

        public static global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam target)
        {
            if (this.Signature != target.Signature) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Signature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniSignature.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniSignature p_signature = OmniSignature.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_signature = global::Omnius.Core.Cryptography.OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.RegisterWantDeclaredMessageParam(p_signature);
            }
        }
    }
    public readonly partial struct UnregisterWantDeclaredMessageParam : global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam>
    {
        public static global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam> Formatter => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam>.Formatter;
        public static global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam Empty => global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam>.Empty;

        static UnregisterWantDeclaredMessageParam()
        {
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam>.Formatter = new ___CustomFormatter();
            global::Omnius.Core.RocketPack.IRocketPackObject<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam>.Empty = new global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam(OmniSignature.Empty);
        }

        private readonly int ___hashCode;

        public UnregisterWantDeclaredMessageParam(OmniSignature signature)
        {
            if (signature is null) throw new global::System.ArgumentNullException("signature");

            this.Signature = signature;

            {
                var ___h = new global::System.HashCode();
                if (signature != default) ___h.Add(signature.GetHashCode());
                ___hashCode = ___h.ToHashCode();
            }
        }

        public OmniSignature Signature { get; }

        public static global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam Import(global::System.Buffers.ReadOnlySequence<byte> sequence, global::Omnius.Core.IBytesPool bytesPool)
        {
            var reader = new global::Omnius.Core.RocketPack.RocketPackObjectReader(sequence, bytesPool);
            return Formatter.Deserialize(ref reader, 0);
        }
        public void Export(global::System.Buffers.IBufferWriter<byte> bufferWriter, global::Omnius.Core.IBytesPool bytesPool)
        {
            var writer = new global::Omnius.Core.RocketPack.RocketPackObjectWriter(bufferWriter, bytesPool);
            Formatter.Serialize(ref writer, this, 0);
        }

        public static bool operator ==(global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam right)
        {
            return right.Equals(left);
        }
        public static bool operator !=(global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam left, global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam right)
        {
            return !(left == right);
        }
        public override bool Equals(object? other)
        {
            if (!(other is global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam)) return false;
            return this.Equals((global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam)other);
        }
        public bool Equals(global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam target)
        {
            if (this.Signature != target.Signature) return false;

            return true;
        }
        public override int GetHashCode() => ___hashCode;

        private sealed class ___CustomFormatter : global::Omnius.Core.RocketPack.IRocketPackObjectFormatter<global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam>
        {
            public void Serialize(ref global::Omnius.Core.RocketPack.RocketPackObjectWriter w, in global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam value, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                if (value.Signature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    global::Omnius.Core.Cryptography.OmniSignature.Formatter.Serialize(ref w, value.Signature, rank + 1);
                }
                w.Write((uint)0);
            }

            public global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam Deserialize(ref global::Omnius.Core.RocketPack.RocketPackObjectReader r, in int rank)
            {
                if (rank > 256) throw new global::System.FormatException();

                OmniSignature p_signature = OmniSignature.Empty;

                for (; ; )
                {
                    uint id = r.GetUInt32();
                    if (id == 0) break;
                    switch (id)
                    {
                        case 1:
                            {
                                p_signature = global::Omnius.Core.Cryptography.OmniSignature.Formatter.Deserialize(ref r, rank + 1);
                                break;
                            }
                    }
                }

                return new global::Omnius.Xeus.Api.Models.UnregisterWantDeclaredMessageParam(p_signature);
            }
        }
    }
}
