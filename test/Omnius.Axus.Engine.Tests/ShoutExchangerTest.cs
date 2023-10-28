using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Omnius.Axus.Engine.Internal;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Net;
using Omnius.Core.RocketPack;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axus.Engine.Internal.Services;

public class ShoutExchangerTest
{
    public static IEnumerable<object[]> GetPublishAndSubscribeTestCases()
    {
        var results = new List<(OmniAddress, OmniAddress, int, TimeSpan)>{
            (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40011), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40012), 1, TimeSpan.FromMinutes(3)),
            // (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40021), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40022), 8192, TimeSpan.FromMinutes(3)),
            // (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40031), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40032), 1024 * 1024 * 32, TimeSpan.FromMinutes(10)),
        };
        return results.Select(n => new object[] { n.Item1, n.Item2, n.Item3, n.Item4 });
    }

    [Theory(Skip = "TODO")]
    [MemberData(nameof(GetPublishAndSubscribeTestCases))]
    public async Task PublishAndSubscribeTest(OmniAddress publisherListenAddress, OmniAddress subscriberListenAddress, int fileSize, TimeSpan timeout)
    {
        await using var shoutExchanger1 = await ShoutExchangerNode.CreateAsync(publisherListenAddress);
        await using var shoutExchanger2 = await ShoutExchangerNode.CreateAsync(subscriberListenAddress);

        var nodeFinder1 = shoutExchanger1.GetNodeFinder();
        var nodeFinder2 = shoutExchanger2.GetNodeFinder();

        await nodeFinder1.AddCloudNodeLocationsAsync(new[] { await nodeFinder2.GetMyNodeLocationAsync() });
        await nodeFinder2.AddCloudNodeLocationsAsync(new[] { await nodeFinder1.GetMyNodeLocationAsync() });

        var publishedShoutStorage = shoutExchanger1.GetPublishedShoutStorage();
        var subscribedShoutStorage = shoutExchanger2.GetSubscribedShoutStorage();

        var body = FixtureFactory.GetRandomBytes(fileSize);
        var digitalSignature = OmniDigitalSignature.Create(FixtureFactory.GetRandomString(10), OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);
        var publishedShout = Shout.Create("test_channel", Timestamp64.FromDateTime(DateTime.UtcNow), new MemoryOwner<byte>(body), digitalSignature);

        await publishedShoutStorage.PublishShoutAsync(publishedShout, null);

        var publishedShout2 = await publishedShoutStorage.TryReadShoutAsync(digitalSignature.GetOmniSignature(), "test_channel", DateTime.MinValue.ToUniversalTime());
        publishedShout2.Should().Be(publishedShout);

        var publishedShoutCreatedTime = await publishedShoutStorage.ReadShoutCreatedTimeAsync(digitalSignature.GetOmniSignature(), "test_channel");
        var truncateTimeSpan = TimeSpan.FromTicks(TimeSpan.TicksPerSecond);
        publishedShoutCreatedTime.Truncate(truncateTimeSpan).Should().Be(publishedShout.CreatedTime.ToDateTime().Truncate(truncateTimeSpan));

        await subscribedShoutStorage.SubscribeShoutAsync(digitalSignature.GetOmniSignature(), null, "test_channel");

        var sw = Stopwatch.StartNew();

        for (; ; )
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            var createdTime = await subscribedShoutStorage.ReadShoutCreatedTimeAsync(digitalSignature.GetOmniSignature(), "test_channel");
            if (createdTime == publishedShoutCreatedTime) break;

            if (sw.Elapsed > timeout) throw new TimeoutException();
        }

        var subscribedShout = await subscribedShoutStorage.TryReadShoutAsync(digitalSignature.GetOmniSignature(), "test_channel", DateTime.MinValue.ToUniversalTime());

        publishedShout.Should().Be(subscribedShout);
    }
}
