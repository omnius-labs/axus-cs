using Omnius.Core;
using Omnius.Core.Storages;

namespace Omnius.Xeus.Service.Engines
{
    public record PublishedFileStorageOptions
    {
        public PublishedFileStorageOptions(string configDirectoryPath)
        {
            this.ConfigDirectoryPath = configDirectoryPath;
        }

        public string ConfigDirectoryPath { get; }
    }
}
