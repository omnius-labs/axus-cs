using System.Diagnostics;
using System.Net;
using Omnius.Core.Net;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axis.Engines;

public class FileExchangerTest
{
    [Fact]
    public async Task NormalTest()
    {
        var listenAddress1 = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50001);
        var listenAddress2 = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50002);

        var fileExchanger1 = await FileExchangerNode.CreateAsync(listenAddress1);
        var fileExchanger2 = await FileExchangerNode.CreateAsync(listenAddress2);

        var nodeFinder1 = fileExchanger1.GetNodeFinder();
        var nodeFinder2 = fileExchanger2.GetNodeFinder();

        await nodeFinder1.AddCloudNodeLocationsAsync(new[] { await nodeFinder2.GetMyNodeLocationAsync() });
        await nodeFinder2.AddCloudNodeLocationsAsync(new[] { await nodeFinder1.GetMyNodeLocationAsync() });

        var publishedFileStorage = fileExchanger1.GetPublishedFileStorage();
        var subscribedFileStorage = fileExchanger2.GetSubscribedFileStorage();

        using var publishDirectoryDeleter = FixtureFactory.GenTempDirectory(out var publishDirectoryPath);
        var publishedFilePath = FixtureFactory.GenRandomFile(publishDirectoryPath, 8192);

        var rootHash = await publishedFileStorage.PublishFileAsync(publishedFilePath, "test");
        await subscribedFileStorage.SubscribeFileAsync(rootHash, "test");

        using var subscribDirectoryDeleter = FixtureFactory.GenTempDirectory(out var subscribDirectoryPath);
        var subscribedFilePath = Path.Combine(subscribDirectoryPath, "subscribedFile");

        var sw = Stopwatch.StartNew();

        for (; ; )
        {
            if (sw.Elapsed > TimeSpan.FromMinutes(10)) throw new TimeoutException();

            await Task.Delay(TimeSpan.FromSeconds(1));

            var result = await subscribedFileStorage.TryExportFileAsync(rootHash, subscribedFilePath);
            if (result) break;
        }
    }
}
