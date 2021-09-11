using System.Collections.Generic;
using Omnius.Core;
using Omnius.Xeus.Service.Engines.Primitives;

namespace Omnius.Xeus.Service.Engines
{
    public record NodeFinderOptions
    {
        public NodeFinderOptions(string configDirectoryPath, uint maxSessionCount)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
            this.MaxSessionCount = maxSessionCount;
        }

        public string ConfigDirectoryPath { get; }

        public uint MaxSessionCount { get; }
    }
}
