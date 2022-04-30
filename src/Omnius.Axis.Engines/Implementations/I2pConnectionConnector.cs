using Omnius.Core;
using Omnius.Core.Net;
using Omnius.Core.Net.Caps;
using Omnius.Core.Net.Connections;
using Omnius.Core.Net.Connections.Bridge;
using Omnius.Core.Net.I2p;
using Omnius.Core.Tasks;

namespace Omnius.Axis.Engines;

public sealed partial class I2pConnectionConnector : AsyncDisposableBase, IConnectionConnector
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IBandwidthLimiter _senderBandwidthLimiter;
    private readonly IBandwidthLimiter _receiverBandwidthLimiter;
    private readonly IBatchActionDispatcher _batchActionDispatcher;
    private readonly IBytesPool _bytesPool;
    private readonly I2pConnectionConnectorOptions _options;

    private SamBridge? _samBridge;

    private Task? _watchLoopTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly AsyncLock _asyncLock = new();

    private const int MaxReceiveByteCount = 1024 * 1024 * 256;
    private const string Caption = "Axis (Connector)";

    public static async ValueTask<I2pConnectionConnector> CreateAsync(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, I2pConnectionConnectorOptions options, CancellationToken cancellationToken = default)
    {
        var i2pConnectionConnector = new I2pConnectionConnector(senderBandwidthLimiter, receiverBandwidthLimiter, batchActionDispatcher, bytesPool, options);
        await i2pConnectionConnector.InitAsync(cancellationToken);
        return i2pConnectionConnector;
    }

    private I2pConnectionConnector(IBandwidthLimiter senderBandwidthLimiter, IBandwidthLimiter receiverBandwidthLimiter, IBatchActionDispatcher batchActionDispatcher, IBytesPool bytesPool, I2pConnectionConnectorOptions options)
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

    public async ValueTask<IConnection?> ConnectAsync(OmniAddress address, CancellationToken cancellationToken = default)
    {
        this.ThrowIfDisposingRequested();

        var cap = await this.ConnectCapAsync(address, cancellationToken);
        if (cap == null) return null;

        var bridgeConnectionOptions = new BridgeConnectionOptions(MaxReceiveByteCount);
        var bridgeConnection = new BridgeConnection(cap, _senderBandwidthLimiter, _receiverBandwidthLimiter, _batchActionDispatcher, _bytesPool, bridgeConnectionOptions);
        return bridgeConnection;
    }

    private async ValueTask<ICap?> ConnectCapAsync(OmniAddress address, CancellationToken cancellationToken = default)
    {
        if (_samBridge is null) return null;
        if (!address.TryParseI2pEndpoint(out var i2pAddress)) return null;

        var disposableList = new List<IDisposable>();

        try
        {
            var socket = await _samBridge.ConnectAsync(i2pAddress, cancellationToken);
            disposableList.Add(socket);

            var cap = new SocketCap(socket);
            disposableList.Add(cap);

            return cap;
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");

            foreach (var item in disposableList)
            {
                item.Dispose();
            }
        }

        return null;
    }
}
