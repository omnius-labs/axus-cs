using Omnix.Cryptography;

#nullable enable

namespace Xeus.Messages
{
    public sealed partial class XeusOptions : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusOptions>
    {
        static XeusOptions()
        {
            XeusOptions.Formatter = new CustomFormatter();
            XeusOptions.Empty = new XeusOptions(string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxConfigDirectoryPathLength = 1024;

        public XeusOptions(string configDirectoryPath)
        {
            if (configDirectoryPath is null) throw new System.ArgumentNullException("configDirectoryPath");
            if (configDirectoryPath.Length > 1024) throw new System.ArgumentOutOfRangeException("configDirectoryPath");

            this.ConfigDirectoryPath = configDirectoryPath;

            {
                var __h = new System.HashCode();
                if (this.ConfigDirectoryPath != default) __h.Add(this.ConfigDirectoryPath.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string ConfigDirectoryPath { get; }

        public override bool Equals(XeusOptions? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ConfigDirectoryPath != target.ConfigDirectoryPath) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusOptions>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusOptions value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ConfigDirectoryPath != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ConfigDirectoryPath != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.ConfigDirectoryPath);
                }
            }

            public XeusOptions Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_configDirectoryPath = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // ConfigDirectoryPath
                            {
                                p_configDirectoryPath = r.GetString(1024);
                                break;
                            }
                    }
                }

                return new XeusOptions(p_configDirectoryPath);
            }
        }
    }

    public sealed partial class XeusClue : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusClue>
    {
        static XeusClue()
        {
            XeusClue.Formatter = new CustomFormatter();
            XeusClue.Empty = new XeusClue(OmniHash.Empty, 0);
        }

        private readonly int __hashCode;

        public XeusClue(OmniHash hash, byte depth)
        {
            this.Hash = hash;
            this.Depth = depth;

            {
                var __h = new System.HashCode();
                if (this.Hash != default) __h.Add(this.Hash.GetHashCode());
                if (this.Depth != default) __h.Add(this.Depth.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniHash Hash { get; }
        public byte Depth { get; }

        public override bool Equals(XeusClue? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Hash != target.Hash) return false;
            if (this.Depth != target.Depth) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusClue>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusClue value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Hash != OmniHash.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Depth != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Hash != OmniHash.Empty)
                {
                    w.Write((uint)0);
                    OmniHash.Formatter.Serialize(w, value.Hash, rank + 1);
                }
                if (value.Depth != 0)
                {
                    w.Write((uint)1);
                    w.Write(value.Depth);
                }
            }

            public XeusClue Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniHash p_hash = OmniHash.Empty;
                byte p_depth = 0;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Hash
                            {
                                p_hash = OmniHash.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // Depth
                            {
                                p_depth = r.GetUInt8();
                                break;
                            }
                    }
                }

                return new XeusClue(p_hash, p_depth);
            }
        }
    }

    public sealed partial class XeusFileMetadata : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusFileMetadata>
    {
        static XeusFileMetadata()
        {
            XeusFileMetadata.Formatter = new CustomFormatter();
            XeusFileMetadata.Empty = new XeusFileMetadata(XeusClue.Empty, string.Empty, 0, Omnix.Serialization.RocketPack.Timestamp.Zero, System.Array.Empty<string>());
        }

        private readonly int __hashCode;

        public static readonly int MaxNameLength = 1024;
        public static readonly int MaxTagsCount = 6;

        public XeusFileMetadata(XeusClue clue, string name, ulong length, Omnix.Serialization.RocketPack.Timestamp creationTime, string[] tags)
        {
            if (clue is null) throw new System.ArgumentNullException("clue");
            if (name is null) throw new System.ArgumentNullException("name");
            if (name.Length > 1024) throw new System.ArgumentOutOfRangeException("name");
            if (tags is null) throw new System.ArgumentNullException("tags");
            if (tags.Length > 6) throw new System.ArgumentOutOfRangeException("tags");
            foreach (var n in tags)
            {
                if (n is null) throw new System.ArgumentNullException("n");
                if (n.Length > 32) throw new System.ArgumentOutOfRangeException("n");
            }

            this.Clue = clue;
            this.Name = name;
            this.Length = length;
            this.CreationTime = creationTime;
            this.Tags = new Omnix.Collections.ReadOnlyListSlim<string>(tags);

            {
                var __h = new System.HashCode();
                if (this.Clue != default) __h.Add(this.Clue.GetHashCode());
                if (this.Name != default) __h.Add(this.Name.GetHashCode());
                if (this.Length != default) __h.Add(this.Length.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                foreach (var n in this.Tags)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public XeusClue Clue { get; }
        public string Name { get; }
        public ulong Length { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public Omnix.Collections.ReadOnlyListSlim<string> Tags { get; }

        public override bool Equals(XeusFileMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Clue != target.Clue) return false;
            if (this.Name != target.Name) return false;
            if (this.Length != target.Length) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Tags, target.Tags)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusFileMetadata>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusFileMetadata value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Clue != XeusClue.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Name != string.Empty)
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
                    if (value.Tags.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Clue != XeusClue.Empty)
                {
                    w.Write((uint)0);
                    XeusClue.Formatter.Serialize(w, value.Clue, rank + 1);
                }
                if (value.Name != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Name);
                }
                if (value.Length != 0)
                {
                    w.Write((uint)2);
                    w.Write(value.Length);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)3);
                    w.Write(value.CreationTime);
                }
                if (value.Tags.Count != 0)
                {
                    w.Write((uint)4);
                    w.Write((uint)value.Tags.Count);
                    foreach (var n in value.Tags)
                    {
                        w.Write(n);
                    }
                }
            }

            public XeusFileMetadata Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                XeusClue p_clue = XeusClue.Empty;
                string p_name = string.Empty;
                ulong p_length = 0;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                string[] p_tags = System.Array.Empty<string>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Clue
                            {
                                p_clue = XeusClue.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // Name
                            {
                                p_name = r.GetString(1024);
                                break;
                            }
                        case 2: // Length
                            {
                                p_length = r.GetUInt64();
                                break;
                            }
                        case 3: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 4: // Tags
                            {
                                var length = r.GetUInt32();
                                p_tags = new string[length];
                                for (int i = 0; i < p_tags.Length; i++)
                                {
                                    p_tags[i] = r.GetString(32);
                                }
                                break;
                            }
                    }
                }

                return new XeusFileMetadata(p_clue, p_name, p_length, p_creationTime, p_tags);
            }
        }
    }

    public sealed partial class XeusDirectoryMetadata : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusDirectoryMetadata>
    {
        static XeusDirectoryMetadata()
        {
            XeusDirectoryMetadata.Formatter = new CustomFormatter();
            XeusDirectoryMetadata.Empty = new XeusDirectoryMetadata(string.Empty, System.Array.Empty<XeusFileMetadata>(), System.Array.Empty<XeusDirectoryMetadata>());
        }

        private readonly int __hashCode;

        public static readonly int MaxNameLength = 1024;
        public static readonly int MaxFilesCount = 32768;
        public static readonly int MaxDirectoriesCount = 32768;

        public XeusDirectoryMetadata(string name, XeusFileMetadata[] files, XeusDirectoryMetadata[] directories)
        {
            if (name is null) throw new System.ArgumentNullException("name");
            if (name.Length > 1024) throw new System.ArgumentOutOfRangeException("name");
            if (files is null) throw new System.ArgumentNullException("files");
            if (files.Length > 32768) throw new System.ArgumentOutOfRangeException("files");
            foreach (var n in files)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }
            if (directories is null) throw new System.ArgumentNullException("directories");
            if (directories.Length > 32768) throw new System.ArgumentOutOfRangeException("directories");
            foreach (var n in directories)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.Name = name;
            this.Files = new Omnix.Collections.ReadOnlyListSlim<XeusFileMetadata>(files);
            this.Directories = new Omnix.Collections.ReadOnlyListSlim<XeusDirectoryMetadata>(directories);

            {
                var __h = new System.HashCode();
                if (this.Name != default) __h.Add(this.Name.GetHashCode());
                foreach (var n in this.Files)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                foreach (var n in this.Directories)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public string Name { get; }
        public Omnix.Collections.ReadOnlyListSlim<XeusFileMetadata> Files { get; }
        public Omnix.Collections.ReadOnlyListSlim<XeusDirectoryMetadata> Directories { get; }

        public override bool Equals(XeusDirectoryMetadata? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Name != target.Name) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Files, target.Files)) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Directories, target.Directories)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusDirectoryMetadata>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusDirectoryMetadata value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Name != string.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.Files.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Directories.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Name != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Name);
                }
                if (value.Files.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.Files.Count);
                    foreach (var n in value.Files)
                    {
                        XeusFileMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                if (value.Directories.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.Directories.Count);
                    foreach (var n in value.Directories)
                    {
                        XeusDirectoryMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public XeusDirectoryMetadata Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_name = string.Empty;
                XeusFileMetadata[] p_files = System.Array.Empty<XeusFileMetadata>();
                XeusDirectoryMetadata[] p_directories = System.Array.Empty<XeusDirectoryMetadata>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Name
                            {
                                p_name = r.GetString(1024);
                                break;
                            }
                        case 1: // Files
                            {
                                var length = r.GetUInt32();
                                p_files = new XeusFileMetadata[length];
                                for (int i = 0; i < p_files.Length; i++)
                                {
                                    p_files[i] = XeusFileMetadata.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 2: // Directories
                            {
                                var length = r.GetUInt32();
                                p_directories = new XeusDirectoryMetadata[length];
                                for (int i = 0; i < p_directories.Length; i++)
                                {
                                    p_directories[i] = XeusDirectoryMetadata.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new XeusDirectoryMetadata(p_name, p_files, p_directories);
            }
        }
    }

    public sealed partial class XeusProfileContent : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusProfileContent>
    {
        static XeusProfileContent()
        {
            XeusProfileContent.Formatter = new CustomFormatter();
            XeusProfileContent.Empty = new XeusProfileContent(OmniAgreementPublicKey.Empty, System.Array.Empty<OmniSignature>(), System.Array.Empty<OmniSignature>());
        }

        private readonly int __hashCode;

        public static readonly int MaxTrustedSignaturesCount = 256;
        public static readonly int MaxInvalidSignaturesCount = 256;

        public XeusProfileContent(OmniAgreementPublicKey agreementPublicKey, OmniSignature[] trustedSignatures, OmniSignature[] invalidSignatures)
        {
            if (agreementPublicKey is null) throw new System.ArgumentNullException("agreementPublicKey");
            if (trustedSignatures is null) throw new System.ArgumentNullException("trustedSignatures");
            if (trustedSignatures.Length > 256) throw new System.ArgumentOutOfRangeException("trustedSignatures");
            foreach (var n in trustedSignatures)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }
            if (invalidSignatures is null) throw new System.ArgumentNullException("invalidSignatures");
            if (invalidSignatures.Length > 256) throw new System.ArgumentOutOfRangeException("invalidSignatures");
            foreach (var n in invalidSignatures)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.AgreementPublicKey = agreementPublicKey;
            this.TrustedSignatures = new Omnix.Collections.ReadOnlyListSlim<OmniSignature>(trustedSignatures);
            this.InvalidSignatures = new Omnix.Collections.ReadOnlyListSlim<OmniSignature>(invalidSignatures);

            {
                var __h = new System.HashCode();
                if (this.AgreementPublicKey != default) __h.Add(this.AgreementPublicKey.GetHashCode());
                foreach (var n in this.TrustedSignatures)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                foreach (var n in this.InvalidSignatures)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniAgreementPublicKey AgreementPublicKey { get; }
        public Omnix.Collections.ReadOnlyListSlim<OmniSignature> TrustedSignatures { get; }
        public Omnix.Collections.ReadOnlyListSlim<OmniSignature> InvalidSignatures { get; }

        public override bool Equals(XeusProfileContent? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.AgreementPublicKey != target.AgreementPublicKey) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.TrustedSignatures, target.TrustedSignatures)) return false;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.InvalidSignatures, target.InvalidSignatures)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusProfileContent>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusProfileContent value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.AgreementPublicKey != OmniAgreementPublicKey.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.TrustedSignatures.Count != 0)
                    {
                        propertyCount++;
                    }
                    if (value.InvalidSignatures.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.AgreementPublicKey != OmniAgreementPublicKey.Empty)
                {
                    w.Write((uint)0);
                    OmniAgreementPublicKey.Formatter.Serialize(w, value.AgreementPublicKey, rank + 1);
                }
                if (value.TrustedSignatures.Count != 0)
                {
                    w.Write((uint)1);
                    w.Write((uint)value.TrustedSignatures.Count);
                    foreach (var n in value.TrustedSignatures)
                    {
                        OmniSignature.Formatter.Serialize(w, n, rank + 1);
                    }
                }
                if (value.InvalidSignatures.Count != 0)
                {
                    w.Write((uint)2);
                    w.Write((uint)value.InvalidSignatures.Count);
                    foreach (var n in value.InvalidSignatures)
                    {
                        OmniSignature.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public XeusProfileContent Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniAgreementPublicKey p_agreementPublicKey = OmniAgreementPublicKey.Empty;
                OmniSignature[] p_trustedSignatures = System.Array.Empty<OmniSignature>();
                OmniSignature[] p_invalidSignatures = System.Array.Empty<OmniSignature>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // AgreementPublicKey
                            {
                                p_agreementPublicKey = OmniAgreementPublicKey.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // TrustedSignatures
                            {
                                var length = r.GetUInt32();
                                p_trustedSignatures = new OmniSignature[length];
                                for (int i = 0; i < p_trustedSignatures.Length; i++)
                                {
                                    p_trustedSignatures[i] = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                        case 2: // InvalidSignatures
                            {
                                var length = r.GetUInt32();
                                p_invalidSignatures = new OmniSignature[length];
                                for (int i = 0; i < p_invalidSignatures.Length; i++)
                                {
                                    p_invalidSignatures[i] = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new XeusProfileContent(p_agreementPublicKey, p_trustedSignatures, p_invalidSignatures);
            }
        }
    }

    public sealed partial class XeusStorageContent : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusStorageContent>
    {
        static XeusStorageContent()
        {
            XeusStorageContent.Formatter = new CustomFormatter();
            XeusStorageContent.Empty = new XeusStorageContent(System.Array.Empty<XeusDirectoryMetadata>());
        }

        private readonly int __hashCode;

        public static readonly int MaxDirectoriesCount = 32768;

        public XeusStorageContent(XeusDirectoryMetadata[] directories)
        {
            if (directories is null) throw new System.ArgumentNullException("directories");
            if (directories.Length > 32768) throw new System.ArgumentOutOfRangeException("directories");
            foreach (var n in directories)
            {
                if (n is null) throw new System.ArgumentNullException("n");
            }

            this.Directories = new Omnix.Collections.ReadOnlyListSlim<XeusDirectoryMetadata>(directories);

            {
                var __h = new System.HashCode();
                foreach (var n in this.Directories)
                {
                    if (n != default) __h.Add(n.GetHashCode());
                }
                __hashCode = __h.ToHashCode();
            }
        }

        public Omnix.Collections.ReadOnlyListSlim<XeusDirectoryMetadata> Directories { get; }

        public override bool Equals(XeusStorageContent? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.Helpers.CollectionHelper.Equals(this.Directories, target.Directories)) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusStorageContent>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusStorageContent value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Directories.Count != 0)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Directories.Count != 0)
                {
                    w.Write((uint)0);
                    w.Write((uint)value.Directories.Count);
                    foreach (var n in value.Directories)
                    {
                        XeusDirectoryMetadata.Formatter.Serialize(w, n, rank + 1);
                    }
                }
            }

            public XeusStorageContent Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                XeusDirectoryMetadata[] p_directories = System.Array.Empty<XeusDirectoryMetadata>();

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Directories
                            {
                                var length = r.GetUInt32();
                                p_directories = new XeusDirectoryMetadata[length];
                                for (int i = 0; i < p_directories.Length; i++)
                                {
                                    p_directories[i] = XeusDirectoryMetadata.Formatter.Deserialize(r, rank + 1);
                                }
                                break;
                            }
                    }
                }

                return new XeusStorageContent(p_directories);
            }
        }
    }

    public sealed partial class XeusCommentContent : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusCommentContent>
    {
        static XeusCommentContent()
        {
            XeusCommentContent.Formatter = new CustomFormatter();
            XeusCommentContent.Empty = new XeusCommentContent(string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxCommentLength = 8192;

        public XeusCommentContent(string comment)
        {
            if (comment is null) throw new System.ArgumentNullException("comment");
            if (comment.Length > 8192) throw new System.ArgumentOutOfRangeException("comment");

            this.Comment = comment;

            {
                var __h = new System.HashCode();
                if (this.Comment != default) __h.Add(this.Comment.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public string Comment { get; }

        public override bool Equals(XeusCommentContent? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.Comment != target.Comment) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusCommentContent>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusCommentContent value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.Comment != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.Comment != string.Empty)
                {
                    w.Write((uint)0);
                    w.Write(value.Comment);
                }
            }

            public XeusCommentContent Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                string p_comment = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Comment
                            {
                                p_comment = r.GetString(8192);
                                break;
                            }
                    }
                }

                return new XeusCommentContent(p_comment);
            }
        }
    }

    public sealed partial class XeusBroadcastProfileMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusBroadcastProfileMessage>
    {
        static XeusBroadcastProfileMessage()
        {
            XeusBroadcastProfileMessage.Formatter = new CustomFormatter();
            XeusBroadcastProfileMessage.Empty = new XeusBroadcastProfileMessage(OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, XeusProfileContent.Empty);
        }

        private readonly int __hashCode;

        public XeusBroadcastProfileMessage(OmniSignature authorSignature, Omnix.Serialization.RocketPack.Timestamp creationTime, XeusProfileContent value)
        {
            if (authorSignature is null) throw new System.ArgumentNullException("authorSignature");
            if (value is null) throw new System.ArgumentNullException("value");

            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var __h = new System.HashCode();
                if (this.AuthorSignature != default) __h.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) __h.Add(this.Value.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniSignature AuthorSignature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusProfileContent Value { get; }

        public override bool Equals(XeusBroadcastProfileMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusBroadcastProfileMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusBroadcastProfileMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.AuthorSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Value != XeusProfileContent.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.AuthorSignature != OmniSignature.Empty)
                {
                    w.Write((uint)0);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)1);
                    w.Write(value.CreationTime);
                }
                if (value.Value != XeusProfileContent.Empty)
                {
                    w.Write((uint)2);
                    XeusProfileContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public XeusBroadcastProfileMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniSignature p_authorSignature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusProfileContent p_value = XeusProfileContent.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // AuthorSignature
                            {
                                p_authorSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 2: // Value
                            {
                                p_value = XeusProfileContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new XeusBroadcastProfileMessage(p_authorSignature, p_creationTime, p_value);
            }
        }
    }

    public sealed partial class XeusBroadcastStoreMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusBroadcastStoreMessage>
    {
        static XeusBroadcastStoreMessage()
        {
            XeusBroadcastStoreMessage.Formatter = new CustomFormatter();
            XeusBroadcastStoreMessage.Empty = new XeusBroadcastStoreMessage(OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, XeusStorageContent.Empty);
        }

        private readonly int __hashCode;

        public XeusBroadcastStoreMessage(OmniSignature authorSignature, Omnix.Serialization.RocketPack.Timestamp creationTime, XeusStorageContent value)
        {
            if (authorSignature is null) throw new System.ArgumentNullException("authorSignature");
            if (value is null) throw new System.ArgumentNullException("value");

            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var __h = new System.HashCode();
                if (this.AuthorSignature != default) __h.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) __h.Add(this.Value.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniSignature AuthorSignature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusStorageContent Value { get; }

        public override bool Equals(XeusBroadcastStoreMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusBroadcastStoreMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusBroadcastStoreMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.AuthorSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Value != XeusStorageContent.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.AuthorSignature != OmniSignature.Empty)
                {
                    w.Write((uint)0);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)1);
                    w.Write(value.CreationTime);
                }
                if (value.Value != XeusStorageContent.Empty)
                {
                    w.Write((uint)2);
                    XeusStorageContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public XeusBroadcastStoreMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniSignature p_authorSignature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusStorageContent p_value = XeusStorageContent.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // AuthorSignature
                            {
                                p_authorSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 2: // Value
                            {
                                p_value = XeusStorageContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new XeusBroadcastStoreMessage(p_authorSignature, p_creationTime, p_value);
            }
        }
    }

    public sealed partial class XeusChannelId : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusChannelId>
    {
        static XeusChannelId()
        {
            XeusChannelId.Formatter = new CustomFormatter();
            XeusChannelId.Empty = new XeusChannelId(System.ReadOnlyMemory<byte>.Empty, string.Empty);
        }

        private readonly int __hashCode;

        public static readonly int MaxKeyLength = 32;
        public static readonly int MaxNameLength = 256;

        public XeusChannelId(System.ReadOnlyMemory<byte> key, string name)
        {
            if (key.Length > 32) throw new System.ArgumentOutOfRangeException("key");
            if (name is null) throw new System.ArgumentNullException("name");
            if (name.Length > 256) throw new System.ArgumentOutOfRangeException("name");

            this.Key = key;
            this.Name = name;

            {
                var __h = new System.HashCode();
                if (!this.Key.IsEmpty) __h.Add(Omnix.Base.Helpers.ObjectHelper.GetHashCode(this.Key.Span));
                if (this.Name != default) __h.Add(this.Name.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public System.ReadOnlyMemory<byte> Key { get; }
        public string Name { get; }

        public override bool Equals(XeusChannelId? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (!Omnix.Base.BytesOperations.SequenceEqual(this.Key.Span, target.Key.Span)) return false;
            if (this.Name != target.Name) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusChannelId>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusChannelId value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (!value.Key.IsEmpty)
                    {
                        propertyCount++;
                    }
                    if (value.Name != string.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (!value.Key.IsEmpty)
                {
                    w.Write((uint)0);
                    w.Write(value.Key.Span);
                }
                if (value.Name != string.Empty)
                {
                    w.Write((uint)1);
                    w.Write(value.Name);
                }
            }

            public XeusChannelId Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                System.ReadOnlyMemory<byte> p_key = System.ReadOnlyMemory<byte>.Empty;
                string p_name = string.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // Key
                            {
                                p_key = r.GetMemory(32);
                                break;
                            }
                        case 1: // Name
                            {
                                p_name = r.GetString(256);
                                break;
                            }
                    }
                }

                return new XeusChannelId(p_key, p_name);
            }
        }
    }

    public sealed partial class XeusMulticastCommentMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusMulticastCommentMessage>
    {
        static XeusMulticastCommentMessage()
        {
            XeusMulticastCommentMessage.Formatter = new CustomFormatter();
            XeusMulticastCommentMessage.Empty = new XeusMulticastCommentMessage(XeusChannelId.Empty, OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, 0, XeusCommentContent.Empty);
        }

        private readonly int __hashCode;

        public XeusMulticastCommentMessage(XeusChannelId channelId, OmniSignature authorSignature, Omnix.Serialization.RocketPack.Timestamp creationTime, uint cost, XeusCommentContent value)
        {
            if (channelId is null) throw new System.ArgumentNullException("channelId");
            if (authorSignature is null) throw new System.ArgumentNullException("authorSignature");
            if (value is null) throw new System.ArgumentNullException("value");

            this.ChannelId = channelId;
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Cost = cost;
            this.Value = value;

            {
                var __h = new System.HashCode();
                if (this.ChannelId != default) __h.Add(this.ChannelId.GetHashCode());
                if (this.AuthorSignature != default) __h.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Cost != default) __h.Add(this.Cost.GetHashCode());
                if (this.Value != default) __h.Add(this.Value.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public XeusChannelId ChannelId { get; }
        public OmniSignature AuthorSignature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public uint Cost { get; }
        public XeusCommentContent Value { get; }

        public override bool Equals(XeusMulticastCommentMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.ChannelId != target.ChannelId) return false;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Cost != target.Cost) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusMulticastCommentMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusMulticastCommentMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.ChannelId != XeusChannelId.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.AuthorSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Cost != 0)
                    {
                        propertyCount++;
                    }
                    if (value.Value != XeusCommentContent.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.ChannelId != XeusChannelId.Empty)
                {
                    w.Write((uint)0);
                    XeusChannelId.Formatter.Serialize(w, value.ChannelId, rank + 1);
                }
                if (value.AuthorSignature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Cost != 0)
                {
                    w.Write((uint)3);
                    w.Write(value.Cost);
                }
                if (value.Value != XeusCommentContent.Empty)
                {
                    w.Write((uint)4);
                    XeusCommentContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public XeusMulticastCommentMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                XeusChannelId p_channelId = XeusChannelId.Empty;
                OmniSignature p_authorSignature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                uint p_cost = 0;
                XeusCommentContent p_value = XeusCommentContent.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // ChannelId
                            {
                                p_channelId = XeusChannelId.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // AuthorSignature
                            {
                                p_authorSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3: // Cost
                            {
                                p_cost = r.GetUInt32();
                                break;
                            }
                        case 4: // Value
                            {
                                p_value = XeusCommentContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new XeusMulticastCommentMessage(p_channelId, p_authorSignature, p_creationTime, p_cost, p_value);
            }
        }
    }

    public sealed partial class XeusUnicastCommentMessage : Omnix.Serialization.RocketPack.RocketPackMessageBase<XeusUnicastCommentMessage>
    {
        static XeusUnicastCommentMessage()
        {
            XeusUnicastCommentMessage.Formatter = new CustomFormatter();
            XeusUnicastCommentMessage.Empty = new XeusUnicastCommentMessage(OmniSignature.Empty, OmniSignature.Empty, Omnix.Serialization.RocketPack.Timestamp.Zero, XeusCommentContent.Empty);
        }

        private readonly int __hashCode;

        public XeusUnicastCommentMessage(OmniSignature targetSignature, OmniSignature authorSignature, Omnix.Serialization.RocketPack.Timestamp creationTime, XeusCommentContent value)
        {
            if (targetSignature is null) throw new System.ArgumentNullException("targetSignature");
            if (authorSignature is null) throw new System.ArgumentNullException("authorSignature");
            if (value is null) throw new System.ArgumentNullException("value");

            this.TargetSignature = targetSignature;
            this.AuthorSignature = authorSignature;
            this.CreationTime = creationTime;
            this.Value = value;

            {
                var __h = new System.HashCode();
                if (this.TargetSignature != default) __h.Add(this.TargetSignature.GetHashCode());
                if (this.AuthorSignature != default) __h.Add(this.AuthorSignature.GetHashCode());
                if (this.CreationTime != default) __h.Add(this.CreationTime.GetHashCode());
                if (this.Value != default) __h.Add(this.Value.GetHashCode());
                __hashCode = __h.ToHashCode();
            }
        }

        public OmniSignature TargetSignature { get; }
        public OmniSignature AuthorSignature { get; }
        public Omnix.Serialization.RocketPack.Timestamp CreationTime { get; }
        public XeusCommentContent Value { get; }

        public override bool Equals(XeusUnicastCommentMessage? target)
        {
            if (target is null) return false;
            if (object.ReferenceEquals(this, target)) return true;
            if (this.TargetSignature != target.TargetSignature) return false;
            if (this.AuthorSignature != target.AuthorSignature) return false;
            if (this.CreationTime != target.CreationTime) return false;
            if (this.Value != target.Value) return false;

            return true;
        }

        public override int GetHashCode() => __hashCode;

        private sealed class CustomFormatter : Omnix.Serialization.RocketPack.IRocketPackFormatter<XeusUnicastCommentMessage>
        {
            public void Serialize(Omnix.Serialization.RocketPack.RocketPackWriter w, XeusUnicastCommentMessage value, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                {
                    uint propertyCount = 0;
                    if (value.TargetSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.AuthorSignature != OmniSignature.Empty)
                    {
                        propertyCount++;
                    }
                    if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                    {
                        propertyCount++;
                    }
                    if (value.Value != XeusCommentContent.Empty)
                    {
                        propertyCount++;
                    }
                    w.Write(propertyCount);
                }

                if (value.TargetSignature != OmniSignature.Empty)
                {
                    w.Write((uint)0);
                    OmniSignature.Formatter.Serialize(w, value.TargetSignature, rank + 1);
                }
                if (value.AuthorSignature != OmniSignature.Empty)
                {
                    w.Write((uint)1);
                    OmniSignature.Formatter.Serialize(w, value.AuthorSignature, rank + 1);
                }
                if (value.CreationTime != Omnix.Serialization.RocketPack.Timestamp.Zero)
                {
                    w.Write((uint)2);
                    w.Write(value.CreationTime);
                }
                if (value.Value != XeusCommentContent.Empty)
                {
                    w.Write((uint)3);
                    XeusCommentContent.Formatter.Serialize(w, value.Value, rank + 1);
                }
            }

            public XeusUnicastCommentMessage Deserialize(Omnix.Serialization.RocketPack.RocketPackReader r, int rank)
            {
                if (rank > 256) throw new System.FormatException();

                // Read property count
                uint propertyCount = r.GetUInt32();

                OmniSignature p_targetSignature = OmniSignature.Empty;
                OmniSignature p_authorSignature = OmniSignature.Empty;
                Omnix.Serialization.RocketPack.Timestamp p_creationTime = Omnix.Serialization.RocketPack.Timestamp.Zero;
                XeusCommentContent p_value = XeusCommentContent.Empty;

                for (; propertyCount > 0; propertyCount--)
                {
                    uint id = r.GetUInt32();
                    switch (id)
                    {
                        case 0: // TargetSignature
                            {
                                p_targetSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 1: // AuthorSignature
                            {
                                p_authorSignature = OmniSignature.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                        case 2: // CreationTime
                            {
                                p_creationTime = r.GetTimestamp();
                                break;
                            }
                        case 3: // Value
                            {
                                p_value = XeusCommentContent.Formatter.Deserialize(r, rank + 1);
                                break;
                            }
                    }
                }

                return new XeusUnicastCommentMessage(p_targetSignature, p_authorSignature, p_creationTime, p_value);
            }
        }
    }

}
