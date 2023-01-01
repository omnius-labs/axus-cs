using System.Diagnostics;
using System.Net;
using Omnius.Axus.Daemon;
using Omnius.Axus.Interactors;
using Omnius.Axus.Messages;
using Omnius.Axus.Remoting;
using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axus.IntegrationTests;

public class AxusServiceTest
{
    [Fact]
    public async Task SimpleTest()
    {
        using (var tempDirDeleter = FixtureFactory.GenTempDirectory(out var tempDir))
        {
            var listenAddress1 = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 30001);
            await using var service1 = await this.GenServiceAsync(Path.Combine(tempDir, "service/1"), listenAddress1);

            var listenAddress2 = OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 30002);
            await using var service2 = await this.GenServiceAsync(Path.Combine(tempDir, "service/2"), listenAddress2);

            await service1.AddCloudNodeLocationsAsync(new AddCloudNodeLocationsRequest(new[] { new NodeLocation(new[] { listenAddress2 }) }));
            await service2.AddCloudNodeLocationsAsync(new AddCloudNodeLocationsRequest(new[] { new NodeLocation(new[] { listenAddress1 }) }));

            var sw = Stopwatch.StartNew();

            for (; ; )
            {
                await Task.Delay(TimeSpan.FromSeconds(1));

                var sessionReport1 = await service1.GetSessionsReportAsync();
                var sessionReport2 = await service2.GetSessionsReportAsync();

                if (sessionReport1.Sessions.Count == 1 && sessionReport2.Sessions.Count == 1) break;

                if (sw.Elapsed > TimeSpan.FromSeconds(60)) throw new TimeoutException();
            }

            await using var interactorProvider1 = await InteractorProvider.CreateAsync(Path.Combine(tempDir, "interactor_provider/1"), new AxusServiceMediator(service1), BytesPool.Shared);
            await using var interactorProvider2 = await InteractorProvider.CreateAsync(Path.Combine(tempDir, "interactor_provider/2"), new AxusServiceMediator(service2), BytesPool.Shared);
        }
    }

    private async ValueTask<AxusService> GenServiceAsync(string databaseDirectoryPath, OmniAddress listenAddress, CancellationToken cancellationToken = default)
    {
        var config = GenServiceConfig(listenAddress);
        var service = await AxusService.CreateAsync(databaseDirectoryPath);
        await service.SetConfigAsync(new SetConfigRequest(config));
        return service;
    }

    private ServiceConfig GenServiceConfig(OmniAddress listenAddress)
    {
        var config = new ServiceConfig(
            bandwidth: new BandwidthConfig(
                maxSendBytesPerSeconds: 1024 * 1024 * 32,
                maxReceiveBytesPerSeconds: 1024 * 1024 * 32),
            i2pConnector: new I2pConnectorConfig(
                isEnabled: true,
                samBridgeAddress: OmniAddress.Empty),
            i2pAccepter: new I2pAccepterConfig(
                isEnabled: true,
                samBridgeAddress: OmniAddress.Empty),
            tcpConnector: new TcpConnectorConfig(
                isEnabled: true,
                proxy: null),
            tcpAccepter: new TcpAccepterConfig(
                isEnabled: true,
                useUpnp: true,
                listenAddress: listenAddress));
        return config;
    }
}
