using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Storages;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axis.Engines;

public class PublishedFileStorageTest
{
    [Fact]
    public async Task SuccessTest()
    {
        using var deleter = FixtureFactory.GenTempDirectory(out var workingPath);
        var configDirectoryPath = Path.Combine(workingPath, "config");
        var tempDirectoryPath = Path.Combine(workingPath, "temp");

        DirectoryHelper.CreateDirectory(configDirectoryPath);
        DirectoryHelper.CreateDirectory(tempDirectoryPath);

        var bytesPool = BytesPool.Shared;
        var options = new PublishedFileStorageOptions(configDirectoryPath);
        await using var publishedFileStorage = await PublishedFileStorage.CreateAsync(KeyValueLiteDatabaseStorage.Factory, bytesPool, options);

        string filepath;

        using (var tempFileStream = FixtureFactory.GenTempFileStream(tempDirectoryPath))
        {
            tempFileStream.Write(FixtureFactory.GetRandomBytes(1024));
            filepath = tempFileStream.Name;
        }

        var contentHash = await publishedFileStorage.PublishFileAsync(filepath, "aaa");
    }
}
