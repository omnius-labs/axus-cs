using System;
using System.Collections.Generic;
using System.Runtime.Serialization;
using System.Text;

namespace Amoeba.Rpc
{
    public sealed class DaemonConfig
    {
        public DaemonConfig() { }

        public DaemonConfig(Version version, CacheConfig cache, PathsConfig paths)
        {
            this.Version = version;
            this.Cache = cache;
            this.Paths = paths;
        }

        public Version Version { get; private set; }
        public CacheConfig Cache { get; private set; }
        public PathsConfig Paths { get; private set; }

        public sealed class CacheConfig
        {
            public CacheConfig() { }

            public CacheConfig(string blocksFilePath)
            {
                this.BlocksFilePath = blocksFilePath;
            }

            public string BlocksFilePath { get; private set; }
        }

        public sealed class PathsConfig
        {
            public PathsConfig() { }

            public PathsConfig(string tempDirectoryPath, string configDirectoryPath, string logDirectoryPath)
            {
                this.TempDirectoryPath = tempDirectoryPath;
                this.ConfigDirectoryPath = configDirectoryPath;
                this.LogDirectoryPath = logDirectoryPath;
            }

            public string TempDirectoryPath { get; private set; }
            public string ConfigDirectoryPath { get; private set; }
            public string LogDirectoryPath { get; private set; }
        }
    }
}
