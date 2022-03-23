using System.Diagnostics;
using System.Net;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Net;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axis.Engines;

public class FileExchangerTest
{
    public static IEnumerable<object[]> GetPublishAndSubscribeCases()
    {
        var results = new List<(OmniAddress, OmniAddress, long, TimeSpan)>{
            (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50011), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50012), 1, TimeSpan.FromMinutes(3)),
            (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50031), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50032), 8192, TimeSpan.FromMinutes(3)),
            (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50051), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50052), 1024 * 1024 * 32, TimeSpan.FromMinutes(3)),
        };
        return results.Select(n => new object[] { n.Item1, n.Item2, n.Item3, n.Item4 });
    }

    [Theory]
    [MemberData(nameof(GetPublishAndSubscribeCases))]
    public async Task PublishAndSubscribeTest(OmniAddress publisherListenPort, OmniAddress subscriberListenPort, long fileSize, TimeSpan timeout)
    {
        await using var fileExchanger1 = await FileExchangerNode.CreateAsync(publisherListenPort);
        await using var fileExchanger2 = await FileExchangerNode.CreateAsync(subscriberListenPort);

        var nodeFinder1 = fileExchanger1.GetNodeFinder();
        var nodeFinder2 = fileExchanger2.GetNodeFinder();

        await nodeFinder1.AddCloudNodeLocationsAsync(new[] { await nodeFinder2.GetMyNodeLocationAsync() });
        await nodeFinder2.AddCloudNodeLocationsAsync(new[] { await nodeFinder1.GetMyNodeLocationAsync() });

        var publishedFileStorage = fileExchanger1.GetPublishedFileStorage();
        var subscribedFileStorage = fileExchanger2.GetSubscribedFileStorage();

        using var publishDirectoryDeleter = FixtureFactory.GenTempDirectory(out var publishDirectoryPath);
        var publishedFilePath = FixtureFactory.GenRandomFile(publishDirectoryPath, fileSize);

        var rootHash = await publishedFileStorage.PublishFileAsync(publishedFilePath, "test");
        await subscribedFileStorage.SubscribeFileAsync(rootHash, "test");

        using var subscribDirectoryDeleter = FixtureFactory.GenTempDirectory(out var subscribDirectoryPath);
        var subscribedFilePath = Path.Combine(subscribDirectoryPath, "subscribed_test_file");

        var sw = Stopwatch.StartNew();

        bool succeed = false;

        for (; ; )
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            succeed = await subscribedFileStorage.TryExportFileAsync(rootHash, subscribedFilePath);
            if (succeed) break;

            if (sw.Elapsed > timeout) throw new TimeoutException();
        }

        using var publishedFileStream = new FileStream(publishedFilePath, FileMode.Open);
        using var subscribedFileStream = new FileStream(subscribedFilePath, FileMode.Open);

        Assert.Equal(await Sha2_256.ComputeHashAsync(publishedFileStream), await Sha2_256.ComputeHashAsync(subscribedFileStream));
    }
}
