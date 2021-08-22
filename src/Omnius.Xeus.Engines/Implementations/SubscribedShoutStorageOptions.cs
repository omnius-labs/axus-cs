using Omnius.Core;
using Omnius.Core.Storages;

namespace Omnius.Xeus.Engines
{
    public record SubscribedShoutStorageOptions
    {
        public SubscribedShoutStorageOptions(string configDirectoryPath)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
        }

        public string ConfigDirectoryPath { get; }
    }
}
