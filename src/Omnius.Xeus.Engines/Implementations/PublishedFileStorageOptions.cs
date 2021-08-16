using Omnius.Core;
using Omnius.Core.Storages;

namespace Omnius.Xeus.Engines
{
    public record PublishedFileStorageOptions
    {
        public string? ConfigDirectoryPath { get; init; }
        public IBytesStorageFactory? BytesStorageFactory { get; init; }
        public IBytesPool? BytesPool { get; init; }
    }
}
