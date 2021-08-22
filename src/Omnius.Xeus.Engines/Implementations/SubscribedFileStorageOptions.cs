using Omnius.Core;
using Omnius.Core.Storages;

namespace Omnius.Xeus.Engines
{
    public record SubscribedFileStorageOptions
    {
        public SubscribedFileStorageOptions(string configDirectoryPath)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
        }

        public string ConfigDirectoryPath { get; }
    }
}
