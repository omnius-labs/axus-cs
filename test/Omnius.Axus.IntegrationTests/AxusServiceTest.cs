using System.Diagnostics;
using System.Net;
using Omnius.Axus.Daemon;
using Omnius.Axus.Models;
using Omnius.Axus.Remoting;
using Omnius.Core.Net;
using Omnius.Core.UnitTestToolkit;
using Xunit;

namespace Omnius.Axus.IntegrationTests;

public class AxusServiceTest
{
    [Fact]
    public async Task SimpleTest()
    {
        using var tempDirectoryDeleter = FixtureFactory.GenTempDirectory(out var tempDirectoryPath);

        var serviceConfig1 = this.GenServiceConfig(OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 30001));
        var service1 = await this.GenServiceAsync(Path.Combine(tempDirectoryPath, "test1"), serviceConfig1);

        var serviceConfig2 = this.GenServiceConfig(OmniAddress.CreateTcpEndpoint(IPAddress.Loopback, 30002));
        var service2 = await this.GenServiceAsync(Path.Combine(tempDirectoryPath, "test2"), serviceConfig2);

        await service1.AddCloudNodeLocationsAsync(new AddCloudNodeLocationsRequest(new[] { new NodeLocation(new[] { serviceConfig2.TcpAccepter!.ListenAddress }) }));
        await service2.AddCloudNodeLocationsAsync(new AddCloudNodeLocationsRequest(new[] { new NodeLocation(new[] { serviceConfig1.TcpAccepter!.ListenAddress }) }));

        var sw = Stopwatch.StartNew();

        for (; ; )
        {
            await Task.Delay(TimeSpan.FromSeconds(1));

            var r1 = await service1.GetSessionsReportAsync();
            var r2 = await service2.GetSessionsReportAsync();

            if (r1.Sessions.Count == 1 && r2.Sessions.Count == 1) break;

            if (sw.Elapsed > TimeSpan.FromSeconds(60)) throw new TimeoutException();
        }
    }

    private async ValueTask<IAxusService> GenServiceAsync(string databaseDirectoryPath, ServiceConfig config, CancellationToken cancellationToken = default)
    {
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
