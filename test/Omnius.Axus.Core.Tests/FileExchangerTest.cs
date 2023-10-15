using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Omnius.Axus.Core.Engine;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Net;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axus.Core.Engine.Services;

public class FileExchangerTest
{
    public static IEnumerable<object[]> GetPublishAndSubscribeTestCases()
    {
        var results = new List<(OmniAddress, OmniAddress, long, TimeSpan)>{
            (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50011), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50012), 1, TimeSpan.FromMinutes(3)),
            // (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50021), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50022), 8192, TimeSpan.FromMinutes(3)),
            // (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50031), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50032), 1024 * 1024 * 32, TimeSpan.FromMinutes(10)),
        };
        return results.Select(n => new object[] { n.Item1, n.Item2, n.Item3, n.Item4 });
    }

    [Theory]
    [MemberData(nameof(GetPublishAndSubscribeTestCases))]
    public async Task PublishAndSubscribeTest(OmniAddress publisherListenAddress, OmniAddress subscriberListenAddress, long fileSize, TimeSpan timeout)
    {
        await using var fileExchanger1 = await FileExchangerNode.CreateAsync(publisherListenAddress);
        await using var fileExchanger2 = await FileExchangerNode.CreateAsync(subscriberListenAddress);

        var nodeFinder1 = fileExchanger1.GetNodeFinder();
        var nodeFinder2 = fileExchanger2.GetNodeFinder();

        await nodeFinder1.AddCloudNodeLocationsAsync(new[] { await nodeFinder2.GetMyNodeLocationAsync() });
        await nodeFinder2.AddCloudNodeLocationsAsync(new[] { await nodeFinder1.GetMyNodeLocationAsync() });

        var publishedFileStorage = fileExchanger1.GetPublishedFileStorage();
        var subscribedFileStorage = fileExchanger2.GetSubscribedFileStorage();

        using var publishDirectoryRemover = FixtureFactory.GenTempDirectory(out var publishDirectoryPath);
        var publishedFilePath = FixtureFactory.GenRandomFile(publishDirectoryPath, fileSize);

        var rootHash = await publishedFileStorage.PublishFileAsync(publishedFilePath, 1024 * 1024, null);
        await subscribedFileStorage.SubscribeFileAsync(rootHash, null);

        using var subscriberDirectoryRemover = FixtureFactory.GenTempDirectory(out var subscriberDirectoryPath);
        var subscribedFilePath = Path.Combine(subscriberDirectoryPath, "subscribed_test_file");

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

        var publishedFileBytes = await Sha2_256.ComputeHashAsync(publishedFileStream);
        var subscribedFileBytes = await Sha2_256.ComputeHashAsync(subscribedFileStream);

        publishedFileBytes.Should().BeEquivalentTo(subscribedFileBytes);
    }
}
