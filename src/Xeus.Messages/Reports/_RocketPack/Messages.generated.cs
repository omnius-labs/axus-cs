using Omnix.Base;
using Omnix.Base.Helpers;
using Omnix.Cryptography;
using Omnix.Serialization;
using Omnix.Serialization.RocketPack;
using System;
using System.Buffers;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Xeus.Messages;

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

    public sealed partial class ErrorReport : RocketPackMessageBase<ErrorReport>
    {
        static ErrorReport()
        {
            ErrorReport.Formatter = new CustomFormatter();
        }

        public ErrorReport(ErrorReportType type, Timestamp creationTime)
        {
            this.Type = type;
            this.CreationTime = creationTime;

            {
                var hashCode = new HashCode();
                if (this.Type != default) hashCode.Add(this.Type.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public ErrorReportType Type { get; }
        public Timestamp CreationTime { get; }

        public override bool Equals(ErrorReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Type != target.Type) return false;
            if (this.CreationTime != target.CreationTime) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ErrorReport>
        {
            public void Serialize(RocketPackWriter w, ErrorReport value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Type != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Type
                if (value.Type != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.Type);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.CreationTime);
                }
            }

            public ErrorReport Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                ErrorReportType p_type = default;
                Timestamp p_creationTime = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Type
                            {
                                p_type = (ErrorReportType)r.GetUInt64();
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                    }
                }

                return new ErrorReport(p_type, p_creationTime);
            }
        }
    }

    public sealed partial class CheckBlocksProgressReport : RocketPackMessageBase<CheckBlocksProgressReport>
    {
        static CheckBlocksProgressReport()
        {
            CheckBlocksProgressReport.Formatter = new CustomFormatter();
        }

        public CheckBlocksProgressReport(uint badBlockCount, uint checkedBlockCount, uint totalBlockCount)
        {
            this.BadBlockCount = badBlockCount;
            this.CheckedBlockCount = checkedBlockCount;
            this.TotalBlockCount = totalBlockCount;

            {
                var hashCode = new HashCode();
                if (this.BadBlockCount != default) hashCode.Add(this.BadBlockCount.GetHashCode());
                if (this.CheckedBlockCount != default) hashCode.Add(this.CheckedBlockCount.GetHashCode());
                if (this.TotalBlockCount != default) hashCode.Add(this.TotalBlockCount.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public uint BadBlockCount { get; }
        public uint CheckedBlockCount { get; }
        public uint TotalBlockCount { get; }

        public override bool Equals(CheckBlocksProgressReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.BadBlockCount != target.BadBlockCount) return false;
            if (this.CheckedBlockCount != target.CheckedBlockCount) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<CheckBlocksProgressReport>
        {
            public void Serialize(RocketPackWriter w, CheckBlocksProgressReport value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.BadBlockCount != default) propertyCount++;
                    if (value.CheckedBlockCount != default) propertyCount++;
                    if (value.TotalBlockCount != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // BadBlockCount
                if (value.BadBlockCount != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.BadBlockCount);
                }
                // CheckedBlockCount
                if (value.CheckedBlockCount != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.CheckedBlockCount);
                }
                // TotalBlockCount
                if (value.TotalBlockCount != default)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.TotalBlockCount);
                }
            }

            public CheckBlocksProgressReport Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                uint p_badBlockCount = default;
                uint p_checkedBlockCount = default;
                uint p_totalBlockCount = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // BadBlockCount
                            {
                                p_badBlockCount = (uint)r.GetUInt64();
                                break;
                            }
                        case 1: // CheckedBlockCount
                            {
                                p_checkedBlockCount = (uint)r.GetUInt64();
                                break;
                            }
                        case 2: // TotalBlockCount
                            {
                                p_totalBlockCount = (uint)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new CheckBlocksProgressReport(p_badBlockCount, p_checkedBlockCount, p_totalBlockCount);
            }
        }
    }

    public sealed partial class ContentReport : RocketPackMessageBase<ContentReport>
    {
        static ContentReport()
        {
            ContentReport.Formatter = new CustomFormatter();
        }

        public static readonly int MaxPathLength = 1024;

        public ContentReport(Clue clue, ulong length, Timestamp creationTime, string path)
        {
            if (clue is null) throw new ArgumentNullException("clue");
            if (path is null) throw new ArgumentNullException("path");
            if (path.Length > 1024) throw new ArgumentOutOfRangeException("path");

            this.Clue = clue;
            this.Length = length;
            this.CreationTime = creationTime;
            this.Path = path;

            {
                var hashCode = new HashCode();
                if (this.Clue != default) hashCode.Add(this.Clue.GetHashCode());
                if (this.Length != default) hashCode.Add(this.Length.GetHashCode());
                if (this.CreationTime != default) hashCode.Add(this.CreationTime.GetHashCode());
                if (this.Path != default) hashCode.Add(this.Path.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public Clue Clue { get; }
        public ulong Length { get; }
        public Timestamp CreationTime { get; }
        public string Path { get; }

        public override bool Equals(ContentReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.Length != target.Length) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Path != target.Path) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ContentReport>
        {
            public void Serialize(RocketPackWriter w, ContentReport value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Clue != default) propertyCount++;
                    if (value.Length != default) propertyCount++;
                    if (value.CreationTime != default) propertyCount++;
                    if (value.Path != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Clue
                if (value.Clue != default)
                {
                    w.Write((ulong)0);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                // Length
                if (value.Length != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.Length);
                }
                // CreationTime
                if (value.CreationTime != default)
                {
                    w.Write((ulong)2);
                    w.Write(value.CreationTime);
                }
                // Path
                if (value.Path != default)
                {
                    w.Write((ulong)3);
                    w.Write(value.Path);
                }
            }

            public ContentReport Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                Clue p_clue = default;
                ulong p_length = default;
                Timestamp p_creationTime = default;
                string p_path = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // Clue
                            {
                                p_clue = Clue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // Length
                            {
                                p_length = (ulong)r.GetUInt64();
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

                return new ContentReport(p_clue, p_length, p_creationTime, p_path);
            }
        }
    }

    public sealed partial class ContentsReport : RocketPackMessageBase<ContentsReport>
    {
        static ContentsReport()
        {
            ContentsReport.Formatter = new CustomFormatter();
        }

        public ContentsReport(uint blockCount, ulong usingAreaSize, ulong protectionAreaSize)
        {
            this.BlockCount = blockCount;
            this.UsingAreaSize = usingAreaSize;
            this.ProtectionAreaSize = protectionAreaSize;

            {
                var hashCode = new HashCode();
                if (this.BlockCount != default) hashCode.Add(this.BlockCount.GetHashCode());
                if (this.UsingAreaSize != default) hashCode.Add(this.UsingAreaSize.GetHashCode());
                if (this.ProtectionAreaSize != default) hashCode.Add(this.ProtectionAreaSize.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public uint BlockCount { get; }
        public ulong UsingAreaSize { get; }
        public ulong ProtectionAreaSize { get; }

        public override bool Equals(ContentsReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.BlockCount != target.BlockCount) return false;
            if (this.UsingAreaSize != target.UsingAreaSize) return false;
            if (this.ProtectionAreaSize != target.ProtectionAreaSize) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ContentsReport>
        {
            public void Serialize(RocketPackWriter w, ContentsReport value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.BlockCount != default) propertyCount++;
                    if (value.UsingAreaSize != default) propertyCount++;
                    if (value.ProtectionAreaSize != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // BlockCount
                if (value.BlockCount != default)
                {
                    w.Write((ulong)0);
                    w.Write((ulong)value.BlockCount);
                }
                // UsingAreaSize
                if (value.UsingAreaSize != default)
                {
                    w.Write((ulong)1);
                    w.Write((ulong)value.UsingAreaSize);
                }
                // ProtectionAreaSize
                if (value.ProtectionAreaSize != default)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.ProtectionAreaSize);
                }
            }

            public ContentsReport Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                uint p_blockCount = default;
                ulong p_usingAreaSize = default;
                ulong p_protectionAreaSize = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                        case 0: // BlockCount
                            {
                                p_blockCount = (uint)r.GetUInt64();
                                break;
                            }
                        case 1: // UsingAreaSize
                            {
                                p_usingAreaSize = (ulong)r.GetUInt64();
                                break;
                            }
                        case 2: // ProtectionAreaSize
                            {
                                p_protectionAreaSize = (ulong)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new ContentsReport(p_blockCount, p_usingAreaSize, p_protectionAreaSize);
            }
        }
    }

    public sealed partial class DownloadContentReport : RocketPackMessageBase<DownloadContentReport>
    {
        static DownloadContentReport()
        {
            DownloadContentReport.Formatter = new CustomFormatter();
        }

        public static readonly int MaxPathLength = 1024;

        public DownloadContentReport(Clue clue, string path, DownloadStateType downloadStateType, byte downloadingDepth, ulong totalBlockCount, ulong downloadedBlockCount, ulong parityBlockCount)
        {
            if (clue is null) throw new ArgumentNullException("clue");
            if (path is null) throw new ArgumentNullException("path");
            if (path.Length > 1024) throw new ArgumentOutOfRangeException("path");
            this.Clue = clue;
            this.Path = path;
            this.DownloadStateType = downloadStateType;
            this.DownloadingDepth = downloadingDepth;
            this.TotalBlockCount = totalBlockCount;
            this.DownloadedBlockCount = downloadedBlockCount;
            this.ParityBlockCount = parityBlockCount;

            {
                var hashCode = new HashCode();
                if (this.Clue != default) hashCode.Add(this.Clue.GetHashCode());
                if (this.Path != default) hashCode.Add(this.Path.GetHashCode());
                if (this.DownloadStateType != default) hashCode.Add(this.DownloadStateType.GetHashCode());
                if (this.DownloadingDepth != default) hashCode.Add(this.DownloadingDepth.GetHashCode());
                if (this.TotalBlockCount != default) hashCode.Add(this.TotalBlockCount.GetHashCode());
                if (this.DownloadedBlockCount != default) hashCode.Add(this.DownloadedBlockCount.GetHashCode());
                if (this.ParityBlockCount != default) hashCode.Add(this.ParityBlockCount.GetHashCode());
                _hashCode = hashCode.ToHashCode();
            }
        }

        public Clue Clue { get; }
        public string Path { get; }
        public DownloadStateType DownloadStateType { get; }
        public byte DownloadingDepth { get; }
        public ulong TotalBlockCount { get; }
        public ulong DownloadedBlockCount { get; }
        public ulong ParityBlockCount { get; }

        public override bool Equals(DownloadContentReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.Path != target.Path) return false;
            if (this.DownloadStateType != target.DownloadStateType) return false;
            if (this.DownloadingDepth != target.DownloadingDepth) return false;
            if (this.TotalBlockCount != target.TotalBlockCount) return false;
            if (this.DownloadedBlockCount != target.DownloadedBlockCount) return false;
            if (this.ParityBlockCount != target.ParityBlockCount) return false;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<DownloadContentReport>
        {
            public void Serialize(RocketPackWriter w, DownloadContentReport value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    if (value.Clue != default) propertyCount++;
                    if (value.Path != default) propertyCount++;
                    if (value.DownloadStateType != default) propertyCount++;
                    if (value.DownloadingDepth != default) propertyCount++;
                    if (value.TotalBlockCount != default) propertyCount++;
                    if (value.DownloadedBlockCount != default) propertyCount++;
                    if (value.ParityBlockCount != default) propertyCount++;
                    w.Write((ulong)propertyCount);
                }

                // Clue
                if (value.Clue != default)
                {
                    w.Write((ulong)0);
                    Clue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                // Path
                if (value.Path != default)
                {
                    w.Write((ulong)1);
                    w.Write(value.Path);
                }
                // DownloadStateType
                if (value.DownloadStateType != default)
                {
                    w.Write((ulong)2);
                    w.Write((ulong)value.DownloadStateType);
                }
                // DownloadingDepth
                if (value.DownloadingDepth != default)
                {
                    w.Write((ulong)3);
                    w.Write((ulong)value.DownloadingDepth);
                }
                // TotalBlockCount
                if (value.TotalBlockCount != default)
                {
                    w.Write((ulong)4);
                    w.Write((ulong)value.TotalBlockCount);
                }
                // DownloadedBlockCount
                if (value.DownloadedBlockCount != default)
                {
                    w.Write((ulong)5);
                    w.Write((ulong)value.DownloadedBlockCount);
                }
                // ParityBlockCount
                if (value.ParityBlockCount != default)
                {
                    w.Write((ulong)6);
                    w.Write((ulong)value.ParityBlockCount);
                }
            }

            public DownloadContentReport Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();

                Clue p_clue = default;
                string p_path = default;
                DownloadStateType p_downloadStateType = default;
                byte p_downloadingDepth = default;
                ulong p_totalBlockCount = default;
                ulong p_downloadedBlockCount = default;
                ulong p_parityBlockCount = default;

                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
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
                                p_downloadingDepth = (byte)r.GetUInt64();
                                break;
                            }
                        case 4: // TotalBlockCount
                            {
                                p_totalBlockCount = (ulong)r.GetUInt64();
                                break;
                            }
                        case 5: // DownloadedBlockCount
                            {
                                p_downloadedBlockCount = (ulong)r.GetUInt64();
                                break;
                            }
                        case 6: // ParityBlockCount
                            {
                                p_parityBlockCount = (ulong)r.GetUInt64();
                                break;
                            }
                    }
                }

                return new DownloadContentReport(p_clue, p_path, p_downloadStateType, p_downloadingDepth, p_totalBlockCount, p_downloadedBlockCount, p_parityBlockCount);
            }
        }
    }

    public sealed partial class DownloadReport : RocketPackMessageBase<DownloadReport>
    {
        static DownloadReport()
        {
            DownloadReport.Formatter = new CustomFormatter();
        }

        public DownloadReport()
        {

            {
                var hashCode = new HashCode();
                _hashCode = hashCode.ToHashCode();
            }
        }


        public override bool Equals(DownloadReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<DownloadReport>
        {
            public void Serialize(RocketPackWriter w, DownloadReport value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    w.Write((ulong)propertyCount);
                }

            }

            public DownloadReport Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();


                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                    }
                }

                return new DownloadReport();
            }
        }
    }

    public sealed partial class NetworkReport : RocketPackMessageBase<NetworkReport>
    {
        static NetworkReport()
        {
            NetworkReport.Formatter = new CustomFormatter();
        }

        public NetworkReport()
        {

            {
                var hashCode = new HashCode();
                _hashCode = hashCode.ToHashCode();
            }
        }


        public override bool Equals(NetworkReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<NetworkReport>
        {
            public void Serialize(RocketPackWriter w, NetworkReport value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    w.Write((ulong)propertyCount);
                }

            }

            public NetworkReport Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();


                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                    }
                }

                return new NetworkReport();
            }
        }
    }

    public sealed partial class ConnectionsReport : RocketPackMessageBase<ConnectionsReport>
    {
        static ConnectionsReport()
        {
            ConnectionsReport.Formatter = new CustomFormatter();
        }

        public ConnectionsReport()
        {

            {
                var hashCode = new HashCode();
                _hashCode = hashCode.ToHashCode();
            }
        }


        public override bool Equals(ConnectionsReport target)
        {
            if ((object)target == null) return false;
            if (Object.ReferenceEquals(this, target)) return true;

            return true;
        }

        private readonly int _hashCode;
        public override int GetHashCode() => _hashCode;

        private sealed class CustomFormatter : IRocketPackFormatter<ConnectionsReport>
        {
            public void Serialize(RocketPackWriter w, ConnectionsReport value, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Write property count
                {
                    int propertyCount = 0;
                    w.Write((ulong)propertyCount);
                }

            }

            public ConnectionsReport Deserialize(RocketPackReader r, int rank)
            {
                if (rank > 256) throw new FormatException();

                // Read property count
                int propertyCount = (int)r.GetUInt64();


                for (; propertyCount > 0; propertyCount--)
                {
                    int id = (int)r.GetUInt64();
                    switch (id)
                    {
                    }
                }

                return new ConnectionsReport();
            }
        }
    }

}
