using Omnix.Cryptography;
using Xeus.Messages;

#nullable enable

namespace Xeus.Messages.Reports
{
    public enum ErrorReportType : byte
    {
        SpaceNotFound = 0,
    }

    public enum DownloadStateType : byte
    {
        Downloading = 0,
        ParityDecoding = 1,
        Decoding = 2,
        Completed = 3,
        Error = 4,
    }

    public enum SessionType : byte
    {
        In = 0,
        Out = 1,
    }

    public sealed partial class ErrorReport : Omnix.Serialization.RocketPack.RocketPackMessageBase<ErrorReport>
    {
        static ErrorReport()
        {
            ErrorReport.Formatter = new CustomFormatter();
            ErrorReport.Empty = new ErrorReport(Omnix.Serialization.RocketPack.Timestamp.Zero, (ErrorReportType)0);
        }

        private readonly int __hashCode;

        public ErrorReport(Omnix.Serialization.RocketPack.Timestamp creationTime, ErrorReportType type)
        {
            this.CreationTime = creationTime;
            this.Type = type;

            {
                var __h = new System.HashCode();
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Type != default) __h.Add(this.Type.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public ErrorReportType Type { get; }

        public override bool Equals(ErrorReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Type != target.Type) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ErrorReport>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ErrorReport value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Type != (ErrorReportType)0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
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

            public ErrorReport Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                ErrorReportType p_type = (ErrorReportType)0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 1: // Type
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

    public sealed partial class CheckBlocksProgressReport : Omnix.Serialization.RocketPack.RocketPackMessageBase<CheckBlocksProgressReport>
    {
        static CheckBlocksProgressReport()
        {
            CheckBlocksProgressReport.Formatter = new CustomFormatter();
            CheckBlocksProgressReport.Empty = new CheckBlocksProgressReport(0, 0, 0);
        }

        private readonly int __hashCode;

        public CheckBlocksProgressReport(uint badBlockCount, uint checkedBlockCount, uint totalBlockCount)
        {
            this.BadBlockCount = badBlockCount;
            this.CheckedBlockCount = checkedBlockCount;
            this.TotalBlockCount = totalBlockCount;

            {
                var __h = new System.HashCode();
                if (this.BadBlockCount != default) __h.Add(this.BadBlockCount.GetHashCode());
                if (this.CheckedBlockCount != default) __h.Add(this.CheckedBlockCount.GetHashCode());
                if (this.TotalBlockCount != default) __h.Add(this.TotalBlockCount.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public uint BadBlockCount { get; }
        public uint CheckedBlockCount { get; }
        public uint TotalBlockCount { get; }

        public override bool Equals(CheckBlocksProgressReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<CheckBlocksProgressReport>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, CheckBlocksProgressReport value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

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

            public CheckBlocksProgressReport Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                uint p_badBlockCount = 0;
                uint p_checkedBlockCount = 0;
                uint p_totalBlockCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // BadBlockCount
                            {
                                p_badBlockCount = r.GetUInt32();
                                break;
                            }
                        case 1: // CheckedBlockCount
                            {
                                p_checkedBlockCount = r.GetUInt32();
                                break;
                            }
                        case 2: // TotalBlockCount
                            {
                                p_totalBlockCount = r.GetUInt32();
                                break;
                            }
                    }
                }

                return new CheckBlocksProgressReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }

    public sealed partial class CacheContentReport : Omnix.Serialization.RocketPack.RocketPackMessageBase<CacheContentReport>
    {
        static CacheContentReport()
        {
            CacheContentReport.Formatter = new CustomFormatter();
            CacheContentReport.Empty = new CacheContentReport(Clue.Empty, 0, Omnix.Serialization.RocketPack.Timestamp.Zero, string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxPathLength = 1024;

        public CacheContentReport(Clue clue, ulong length, Omnix.Serialization.RocketPack.Timestamp creationTime, string path)
        {
            if (clue is null) throw new System.ArgumentNullException("clue");
            if (path is null) throw new System.ArgumentNullException("path");
            if (path.Length > 1024) throw new System.ArgumentOutOfRangeException("path");

            this.Clue = clue;
            this.Length = length;
            this.CreationTime = creationTime;
            this.Path = path;

            {
                var __h = new System.HashCode();
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Length != default) __h.Add(this.Length.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Path != default) __h.Add(this.Path.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public Clue Clue { get; }
        public ulong Length { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public string Path { get; }

        public override bool Equals(CacheContentReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.Length != target.Length) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Path != target.Path) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<CacheContentReport>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, CacheContentReport value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Clue != Clue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Length != 0)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Path != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Clue != Clue.Empty)
                {
                    w.Write((uint)0);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Length != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Length);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Path != string.Empty)
                {
                    w.Write((uint)3);
                    w.Write(value.Path);
                }
            }

            public CacheContentReport Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                Clue p_clue = Clue.Empty;
                ulong p_length = 0;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                string p_path = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Clue
                            {
                                p_clue = Clue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // Length
                            {
                                p_length = r.GetUInt64();
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3: // Path
                            {
                                p_path = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new CacheContentReport(p_clue, p_length, p_creationTime, p_path);
            }
        }
    }

    public sealed partial class CacheReport : Omnix.Serialization.RocketPack.RocketPackMessageBase<CacheReport>
    {
        static CacheReport()
        {
            CacheReport.Formatter = new CustomFormatter();
            CacheReport.Empty = new CacheReport(0, 0, 0, 0);
        }

        private readonly int __hashCode;

        public CacheReport(uint blockCount, ulong usingArea, ulong protectionArea, ulong freeArea)
        {
            this.BlockCount = blockCount;
            this.UsingArea = usingArea;
            this.ProtectionArea = protectionArea;
            this.FreeArea = freeArea;

            {
                var __h = new System.HashCode();
                if (this.BlockCount != default) __h.Add(this.BlockCount.GetHashCode());
                if (this.UsingArea != default) __h.Add(this.UsingArea.GetHashCode());
                if (this.ProtectionArea != default) __h.Add(this.ProtectionArea.GetHashCode());
                if (this.FreeArea != default) __h.Add(this.FreeArea.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public uint BlockCount { get; }
        public ulong UsingArea { get; }
        public ulong ProtectionArea { get; }
        public ulong FreeArea { get; }

        public override bool Equals(CacheReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.BlockCount != target.BlockCount) return false;
            if (this.UsingArea != target.UsingArea) return false;
            if (this.ProtectionArea != target.ProtectionArea) return false;
            if (this.FreeArea != target.FreeArea) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<CacheReport>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, CacheReport value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.BlockCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.UsingArea != 0)
                    {
                        propertyCount++;
                    }
                    if (value.ProtectionArea != 0)
                    {
                        propertyCount++;
                    }
                    if (value.FreeArea != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.BlockCount != 0)
                {
                    w.Write((uint)0);
                    w.Write(value.BlockCount);
                }
                if (value.UsingArea != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.UsingArea);
                }
                if (value.ProtectionArea != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.ProtectionArea);
                }
                if (value.FreeArea != 0)
                {
                    w.Write((uint)3);
                    w.Write(value.FreeArea);
                }
            }

            public CacheReport Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                uint p_blockCount = 0;
                ulong p_usingArea = 0;
                ulong p_protectionArea = 0;
                ulong p_freeArea = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // BlockCount
                            {
                                p_blockCount = r.GetUInt32();
                                break;
                            }
                        case 1: // UsingArea
                            {
                                p_usingArea = r.GetUInt64();
                                break;
                            }
                        case 2: // ProtectionArea
                            {
                                p_protectionArea = r.GetUInt64();
                                break;
                            }
                        case 3: // FreeArea
                            {
                                p_freeArea = r.GetUInt64();
                                break;
                            }
                    }
                }

                return new CacheReport(p_blockCount, p_usingArea, p_protectionArea, p_freeArea);
            }
        }
    }

    public sealed partial class DownloadContentReport : Omnix.Serialization.RocketPack.RocketPackMessageBase<DownloadContentReport>
    {
        static DownloadContentReport()
        {
            DownloadContentReport.Formatter = new CustomFormatter();
            DownloadContentReport.Empty = new DownloadContentReport(Clue.Empty, string.Empty, (DownloadStateType)0, 0, 0, 0, 0);
        }

        private readonly int __hashCode;

        public static readonly int MaxPathLength = 1024;

        public DownloadContentReport(Clue clue, string path, DownloadStateType downloadStateType, byte downloadingDepth, ulong totalBlockCount, ulong downloadedBlockCount, ulong parityBlockCount)
        {
            if (clue is null) throw new System.ArgumentNullException("clue");
            if (path is null) throw new System.ArgumentNullException("path");
            if (path.Length > 1024) throw new System.ArgumentOutOfRangeException("path");
            this.Clue = clue;
            this.Path = path;
            this.DownloadStateType = downloadStateType;
            this.DownloadingDepth = downloadingDepth;
            this.TotalBlockCount = totalBlockCount;
            this.DownloadedBlockCount = downloadedBlockCount;
            this.ParityBlockCount = parityBlockCount;

            {
                var __h = new System.HashCode();
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Path != default) __h.Add(this.Path.GetHashCode());
                if (this.DownloadStateType != default) __h.Add(this.DownloadStateType.GetHashCode());
                if (this.DownloadingDepth != default) __h.Add(this.DownloadingDepth.GetHashCode());
                if (this.TotalBlockCount != default) __h.Add(this.TotalBlockCount.GetHashCode());
                if (this.DownloadedBlockCount != default) __h.Add(this.DownloadedBlockCount.GetHashCode());
                if (this.ParityBlockCount != default) __h.Add(this.ParityBlockCount.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public Clue Clue { get; }
        public string Path { get; }
        public DownloadStateType DownloadStateType { get; }
        public byte DownloadingDepth { get; }
        public ulong TotalBlockCount { get; }
        public ulong DownloadedBlockCount { get; }
        public ulong ParityBlockCount { get; }

        public override bool Equals(DownloadContentReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.Path != target.Path) return false;
            if (this.DownloadStateType != target.DownloadStateType) return false;
            if (this.DownloadingDepth != target.DownloadingDepth) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;
            if (this.DownloadedBlockCount != target.DownloadedBlockCount) return false;
            if (this.ParityBlockCount != target.ParityBlockCount) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<DownloadContentReport>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, DownloadContentReport value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Clue != Clue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Path != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.DownloadStateType != (DownloadStateType)0)
                    {
                        propertyCount++;
                    }
                    if (value.DownloadingDepth != 0)
                    {
                        propertyCount++;
                    }
                    if (value.TotalBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.DownloadedBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    if (value.ParityBlockCount != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Clue != Clue.Empty)
                {
                    w.Write((uint)0);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Path != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Path);
                }
                if (value.DownloadStateType != (DownloadStateType)0)
                {
                    w.Write((uint)2);
                    w.Write((ulong)value.DownloadStateType);
                }
                if (value.DownloadingDepth != 0)
                {
                    w.Write((uint)3);
                    w.Write(value.DownloadingDepth);
                }
                if (value.TotalBlockCount != 0)
                {
                    w.Write((uint)4);
                    w.Write(value.TotalBlockCount);
                }
                if (value.DownloadedBlockCount != 0)
                {
                    w.Write((uint)5);
                    w.Write(value.DownloadedBlockCount);
                }
                if (value.ParityBlockCount != 0)
                {
                    w.Write((uint)6);
                    w.Write(value.ParityBlockCount);
                }
            }

            public DownloadContentReport Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                Clue p_clue = Clue.Empty;
                string p_path = string.Empty;
                DownloadStateType p_downloadStateType = (DownloadStateType)0;
                byte p_downloadingDepth = 0;
                ulong p_totalBlockCount = 0;
                ulong p_downloadedBlockCount = 0;
                ulong p_parityBlockCount = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Clue
                            {
                                p_clue = Clue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // Path
                            {
                                p_path = r.GetString(1024);
                                break;
                            }
                        case 2: // DownloadStateType
                            {
                                p_downloadStateType = (DownloadStateType)r.GetUInt64();
                                break;
                            }
                        case 3: // DownloadingDepth
                            {
                                p_downloadingDepth = r.GetUInt8();
                                break;
                            }
                        case 4: // TotalBlockCount
                            {
                                p_totalBlockCount = r.GetUInt64();
                                break;
                            }
                        case 5: // DownloadedBlockCount
                            {
                                p_downloadedBlockCount = r.GetUInt64();
                                break;
                            }
                        case 6: // ParityBlockCount
                            {
                                p_parityBlockCount = r.GetUInt64();
                                break;
                            }
                    }
                }

                return new DownloadContentReport(p_clue, p_path, p_downloadStateType, p_downloadingDepth, p_totalBlockCount, p_downloadedBlockCount, p_parityBlockCount);
            }
        }
    }

    public sealed partial class DownloadReport : Omnix.Serialization.RocketPack.RocketPackMessageBase<DownloadReport>
    {
        static DownloadReport()
        {
            DownloadReport.Formatter = new CustomFormatter();
            DownloadReport.Empty = new DownloadReport();
        }

        private readonly int __hashCode;

        public DownloadReport()
        {

            {
                var __h = new System.HashCode();
                __hashCode = __h.ToHashCode();
            }
        }


        public override bool Equals(DownloadReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<DownloadReport>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, DownloadReport value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public DownloadReport Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new DownloadReport();
            }
        }
    }

    public sealed partial class NetworkReport : Omnix.Serialization.RocketPack.RocketPackMessageBase<NetworkReport>
    {
        static NetworkReport()
        {
            NetworkReport.Formatter = new CustomFormatter();
            NetworkReport.Empty = new NetworkReport();
        }

        private readonly int __hashCode;

        public NetworkReport()
        {

            {
                var __h = new System.HashCode();
                __hashCode = __h.ToHashCode();
            }
        }


        public override bool Equals(NetworkReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<NetworkReport>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, NetworkReport value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public NetworkReport Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new NetworkReport();
            }
        }
    }

    public sealed partial class ConnectionsReport : Omnix.Serialization.RocketPack.RocketPackMessageBase<ConnectionsReport>
    {
        static ConnectionsReport()
        {
            ConnectionsReport.Formatter = new CustomFormatter();
            ConnectionsReport.Empty = new ConnectionsReport();
        }

        private readonly int __hashCode;

        public ConnectionsReport()
        {

            {
                var __h = new System.HashCode();
                __hashCode = __h.ToHashCode();
            }
        }


        public override bool Equals(ConnectionsReport? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<ConnectionsReport>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, ConnectionsReport value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    w.Write(propertyCount);
                }

            }

            public ConnectionsReport Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();


                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                    }
                }

                return new ConnectionsReport();
            }
        }
    }

}
