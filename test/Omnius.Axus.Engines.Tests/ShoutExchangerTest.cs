using System.Diagnostics;
using System.Net;
using FluentAssertions;
using Omnius.Axus.Models;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Net;
using Omnius.Core.RocketPack;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axus.Engines;

public class ShoutExchangerTest
{
    public static IEnumerable<object[]> GetPublishAndSubscribeTestCases()
    {
        var results = new List<(OmniAddress, OmniAddress, TimeSpan)>{
            (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback,40011), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 40012), TimeSpan.FromMinutes(3)),
            // (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50031), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50032), TimeSpan.FromMinutes(3)),
            // (OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50051), OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 50052), TimeSpan.FromMinutes(3)),
        };
        return results.Select(n => new object[] { n.Item1, n.Item2, n.Item3 });
    }

    [Theory]
    [MemberData(nameof(GetPublishAndSubscribeTestCases))]
    public async Task PublishAndSubscribeTest(OmniAddress publisherListenPort, OmniAddress subscriberListenPort, TimeSpan timeout)
    {
        await using var shoutExchanger1 = await ShoutExchangerNode.CreateAsync(publisherListenPort);
        await using var shoutExchanger2 = await ShoutExchangerNode.CreateAsync(subscriberListenPort);

        var nodeFinder1 = shoutExchanger1.GetNodeFinder();
        var nodeFinder2 = shoutExchanger2.GetNodeFinder();

        await nodeFinder1.AddCloudNodeLocationsAsync(new[] { await nodeFinder2.GetMyNodeLocationAsync() });
        await nodeFinder2.AddCloudNodeLocationsAsync(new[] { await nodeFinder1.GetMyNodeLocationAsync() });

        var publishedShoutStorage = shoutExchanger1.GetPublishedShoutStorage();
        var subscribedShoutStorage = shoutExchanger2.GetSubscribedShoutStorage();

        var body = FixtureFactory.GetRandomBytes(10);
        var digitalSignature = OmniDigitalSignature.Create(FixtureFactory.GetRandomString(10), OmniDigitalSignatureAlgorithmType.EcDsa_P521_Sha2_256);
        var publishedShout = Shout.Create(Timestamp.FromDateTime(DateTime.UtcNow), new MemoryOwner<byte>(body), digitalSignature);

        await publishedShoutStorage.PublishShoutAsync(publishedShout, "test");

        var publishedShout2 = await publishedShoutStorage.ReadShoutAsync(digitalSignature.GetOmniSignature());
        publishedShout2.Should().Be(publishedShout);

        var publishedShout2CreatedTime = await publishedShoutStorage.ReadShoutCreatedTimeAsync(digitalSignature.GetOmniSignature());
        var truncateTimeSpan = TimeSpan.FromTicks(TimeSpan.TicksPerSecond);
        publishedShout2CreatedTime.Value.Truncate(truncateTimeSpan).Should().Be(publishedShout.CreatedTime.ToDateTime().Truncate(truncateTimeSpan));

        await subscribedShoutStorage.SubscribeShoutAsync(digitalSignature.GetOmniSignature(), "test");

        var sw = Stopwatch.StartNew();

        for (; ; )
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            var createdTime = await subscribedShoutStorage.ReadShoutCreatedTimeAsync(digitalSignature.GetOmniSignature());
            if (createdTime is not null) break;

            if (sw.Elapsed > timeout) throw new TimeoutException();
        }

        var subscribedShout = await subscribedShoutStorage.ReadShoutAsync(digitalSignature.GetOmniSignature());

        publishedShout.Should().Be(subscribedShout);
    }
}
