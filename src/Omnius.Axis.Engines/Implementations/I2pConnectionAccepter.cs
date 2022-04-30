using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.I2p;
using Omnius.Core.Tasks;

namespace Omnius.Axis.Engines;

public sealed partial class I2pConnectionAccepter : AsyncDisposableBase, IConnectionAccepter
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IBandwidthLimiter _senderBandwidthLimiter;
    private readonly IBandwidthLimiter _receiverBandwidthLimiter;
    private readonly IBatchActionDispatcher _batchActionDispatcher;
    private readonly IBytesPool _bytesPool;
    private readonly I2pConnectionAccepterOptions _options;

    private SamBridge? _samBridge;

    private Task? _watchLoopTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const int MaxReceiveByteCount = 1024 * 1024 * 256;
    private const string Caption = "Axis (Accepter)";

    public static async ValueTask<I2pConnectionAccepter> CreateAsync(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, I2pConnectionAccepterOptions options, CancellationToken cancellationToken = default)
    {
        var i2pConnectionAccepter = new I2pConnectionAccepter(senderBandwidthLimiter, receiverBandwidthLimiter, batchActionDispatcher, bytesPool, options);
        await i2pConnectionAccepter.InitAsync(cancellationToken);
        return i2pConnectionAccepter;
    }

    private I2pConnectionAccepter(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, I2pConnectionAccepterOptions options)
    {
        _senderBandwidthLimiter = senderBandwidthLimiter;
        _receiverBandwidthLimiter = receiverBandwidthLimiter;
        _batchActionDispatcher = batchActionDispatcher;
        _bytesPool = bytesPool;
        _options = options;
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        _watchLoopTask = this.WatchLoopAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await _watchLoopTask!;
        _cancellationTokenSource.Dispose();

        if (_samBridge is not null) await _samBridge.DisposeAsync();
    }

    private async Task WatchLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                if (_samBridge is not null && _samBridge.IsConnected) continue;

                if (_options.SamBridgeAddress.TryParseTcpEndpoint(out var samBridgeAddress, out var samBridgePort, true))
                {
                    try
                    {
                        _samBridge = await SamBridge.CreateAsync(samBridgeAddress, samBridgePort, Caption, cancellationToken);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Unexpected Exception");
                    }
                }

                await Task.Delay(1000 * 60, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }
    }

    public async ValueTask<ConnectionAcceptedResult?> AcceptAsync(CancellationToken cancellationToken = default)
    {
        using (await _asyncLock.LockAsync(cancellationToken))
        {
            var (cap, address) = await this.AcceptCapAsync(cancellationToken);
            if (cap is null || address is null) return null;

            var bridgeConnectionOptions = new BridgeConnectionOptions(MaxReceiveByteCount);
            var bridgeConnection = new BridgeConnection(cap, _senderBandwidthLimiter, _receiverBandwidthLimiter, _batchActionDispatcher, _bytesPool, bridgeConnectionOptions);
            return new ConnectionAcceptedResult(bridgeConnection, address);
        }
    }

    private async ValueTask<(ICap?, OmniAddress?)> AcceptCapAsync(CancellationToken cancellationToken = default)
    {
        if (_samBridge is null) return (null, null);

        try
        {
            var acceptResult = await _samBridge.AcceptAsync(cancellationToken);
            if (acceptResult is null) return (null, null);

            var address = OmniAddress.CreateI2pEndpoint(acceptResult.Destination);
            var cap = new SocketCap(acceptResult.Socket);

            return (cap, address);
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }

        return (null, null);
    }

    public async ValueTask<OmniAddress[]> GetListenEndpointsAsync(CancellationToken cancellationToken = default)
    {
        if (_samBridge is null || _samBridge.Base32Address is null)
        {
            return Array.Empty<OmniAddress>();
        }

        return new[] { OmniAddress.CreateI2pEndpoint(_samBridge.Base32Address) };
    }
}
