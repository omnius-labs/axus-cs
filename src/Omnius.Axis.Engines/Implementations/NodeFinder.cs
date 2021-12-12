using System.Buffers;
using System.Collections.Immutable;
using System.Security.Cryptography;
using Omnius.Axis.Engines.Internal;
using Omnius.Axis.Engines.Internal.Models;
using Omnius.Axis.Engines.Primitives;
using Omnius.Axis.Models;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Core.Pipelines;
using Omnius.Core.Tasks;

namespace Omnius.Axis.Engines;

public sealed partial class NodeFinder : AsyncDisposableBase, INodeFinder
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly ISessionConnector _sessionConnector;
    private readonly ISessionAccepter _sessionAccepter;
    private readonly IBatchActionDispatcher _batchActionDispatcher;
    private readonly IBytesPool _bytesPool;
    private readonly NodeFinderOptions _options;

    private readonly EventPipe<IContentExchanger> _contentExchangerEventPipe = new();
    private readonly Events _events;

    private readonly byte[] _myId;

    private readonly LinkedList<NodeLocation> _cloudNodeLocations = new();
    private readonly VolatileHashSet<OmniAddress> _connectedAddressSet;
    private ImmutableHashSet<SessionStatus> _sessionStatusSet = ImmutableHashSet<SessionStatus>.Empty;

    private readonly VolatileListDictionary<ContentClue, NodeLocation> _receivedPushContentLocationMap;
    private readonly VolatileListDictionary<ContentClue, NodeLocation> _receivedGiveContentLocationMap;

    private readonly Task _connectLoopTask;
    private readonly Task _acceptLoopTask;
    private readonly Task _sendLoopTask;
    private readonly Task _receiveLoopTask;
    private readonly Task _computeLoopTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly Random _random = new();

    private readonly object _lockObject = new();

    private const int MaxBucketLength = 20;
    private const string ServiceName = "node_finder";

    public static async ValueTask<NodeFinder> CreateAsync(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, IBatchActionDispatcher batchActionDispatcher,
        IBytesPool bytesPool, NodeFinderOptions options, CancellationToken cancellationToken = default)
    {
        var nodeFinder = new NodeFinder(sessionConnector, sessionAccepter, batchActionDispatcher, bytesPool, options);
        return nodeFinder;
    }

    private NodeFinder(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, IBatchActionDispatcher batchActionDispatcher,
        IBytesPool bytesPool, NodeFinderOptions options)
    {
        _sessionConnector = sessionConnector;
        _sessionAccepter = sessionAccepter;
        _batchActionDispatcher = batchActionDispatcher;
        _bytesPool = bytesPool;
        _options = options;
        _myId = GenId();

        _events = new Events(_contentExchangerEventPipe.Subscriber);

        _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), _batchActionDispatcher);

        _receivedPushContentLocationMap = new VolatileListDictionary<ContentClue, NodeLocation>(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(30), _batchActionDispatcher);
        _receivedGiveContentLocationMap = new VolatileListDictionary<ContentClue, NodeLocation>(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(30), _batchActionDispatcher);

        _connectLoopTask = this.ConnectLoopAsync(_cancellationTokenSource.Token);
        _acceptLoopTask = this.AcceptLoopAsync(_cancellationTokenSource.Token);
        _sendLoopTask = this.SendLoopAsync(_cancellationTokenSource.Token);
        _receiveLoopTask = this.ReceiveLoopAsync(_cancellationTokenSource.Token);
        _computeLoopTask = this.ComputeLoopAsync(_cancellationTokenSource.Token);
    }

    private static byte[] GenId()
    {
        var id = new byte[32];
        using var random = RandomNumberGenerator.Create();
        random.GetBytes(id);
        return id;
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();
        await Task.WhenAll(_connectLoopTask, _acceptLoopTask, _sendLoopTask, _receiveLoopTask, _computeLoopTask);
        _cancellationTokenSource.Dispose();

        _connectedAddressSet.Dispose();
        _receivedPushContentLocationMap.Dispose();
        _receivedGiveContentLocationMap.Dispose();
    }

    public INodeFinderEvents GetEvents() => _events;

    public async ValueTask<NodeFinderReport> GetReportAsync(CancellationToken cancellationToken = default)
    {
        var sessionReports = new List<SessionReport>();

        foreach (var status in _sessionStatusSet)
        {
            sessionReports.Add(new SessionReport(ServiceName, status.Session.HandshakeType, status.Session.Address));
        }

        return new NodeFinderReport(0, 0, sessionReports.ToArray());
    }

    public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
    {
        var addresses = new List<OmniAddress>();
        addresses.AddRange(await _sessionAccepter.GetListenEndpointsAsync(cancellationToken));

        var myNodeLocation = new NodeLocation(addresses.ToArray());
        return myNodeLocation;
    }

    public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            foreach (var nodeLocation in nodeLocations)
            {
                if (_cloudNodeLocations.Count >= 2048) return;
                if (_cloudNodeLocations.Any(n => n.Addresses.Any(m => nodeLocation.Addresses.Contains(m)))) continue;
                _cloudNodeLocations.AddLast(nodeLocation);
            }
        }
    }

    public async ValueTask<NodeLocation[]> FindNodeLocationsAsync(ContentClue contentClue, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            var result = new HashSet<NodeLocation>();

            if (_receivedPushContentLocationMap.TryGetValue(contentClue, out var nodeLocations1))
            {
                result.UnionWith(nodeLocations1);
            }

            if (_receivedGiveContentLocationMap.TryGetValue(contentClue, out var nodeLocations2))
            {
                result.UnionWith(nodeLocations2);
            }

            return result.ToArray();
        }
    }

    private void RefreshCloudNodeLocation(NodeLocation nodeLocation)
    {
        lock (_lockObject)
        {
            _cloudNodeLocations.RemoveAll(n => n.Addresses.Any(m => nodeLocation.Addresses.Contains(m)));
            _cloudNodeLocations.AddFirst(nodeLocation);
        }
    }

    private bool RemoveCloudNodeLocation(NodeLocation nodeLocation)
    {
        lock (_lockObject)
        {
            if (_cloudNodeLocations.Count >= 1024)
            {
                _cloudNodeLocations.Remove(nodeLocation);
            }

            return false;
        }
    }

    private async Task ConnectLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Connected).Count();
                if (connectionCount > (_options.MaxSessionCount / 2)) continue;

                var nodeLocation = this.FindNodeLocationToConnecting();
                if (nodeLocation == null) continue;

                foreach (var targetAddress in nodeLocation.Addresses)
                {
                    _connectedAddressSet.Add(targetAddress);

                    var session = await _sessionConnector.ConnectAsync(targetAddress, ServiceName, cancellationToken);
                    if (session is null) continue;

                    if (await this.TryAddSessionAsync(session, cancellationToken))
                    {
                        _logger.Debug("Connected: {0}", targetAddress);
                        this.RefreshCloudNodeLocation(nodeLocation);
                    }
                    else
                    {
                        this.RemoveCloudNodeLocation(nodeLocation);
                    }

                    break;
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private NodeLocation? FindNodeLocationToConnecting()
    {
        lock (_lockObject)
        {
            var nodeLocations = _cloudNodeLocations.ToArray();
            _random.Shuffle(nodeLocations);

            var ignoreAddressSet = new HashSet<OmniAddress>();
            ignoreAddressSet.UnionWith(_sessionStatusSet.Select(n => n.Session.Address));
            ignoreAddressSet.UnionWith(_connectedAddressSet);

            return nodeLocations
                .Where(n => !n.Addresses.Any(n => ignoreAddressSet.Contains(n)))
                .FirstOrDefault();
        }
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Accepted).Count();
                if (connectionCount > (_options.MaxSessionCount / 2)) continue;

                var session = await _sessionAccepter.AcceptAsync(ServiceName, cancellationToken);
                if (session is null) continue;

                if (await this.TryAddSessionAsync(session, cancellationToken))
                {
                    _logger.Debug("Accepted: {0}", session.Address);
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);
        }
    }

    private async ValueTask<bool> TryAddSessionAsync(ISession session, CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await this.HandshakeVersionAsync(session.Connection, cancellationToken);
            if (version is null) throw new Exception("Handshake failed.");

            if (version == NodeFinderVersion.Version1)
            {
                var profile = await this.HandshakeProfileAsync(session.Connection, cancellationToken);
                if (profile is null) throw new Exception("Handshake failed.");

                var sessionStatus = new SessionStatus(session, profile.Id, profile.NodeLocation, _batchActionDispatcher);
                if (!this.TryAddSessionStatus(sessionStatus)) throw new Exception("Handshake failed.");

                return true;
            }

            throw new NotSupportedException();
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);

            await session.DisposeAsync();
        }
        catch (Exception e)
        {
            _logger.Warn(e);

            await session.DisposeAsync();
        }

        return false;
    }

    private async ValueTask<NodeFinderVersion?> HandshakeVersionAsync(IConnection connection, CancellationToken cancellationToken = default)
    {
        var myHelloMessage = new NodeFinderHelloMessage(new[] { NodeFinderVersion.Version1 });
        var otherHelloMessage = await connection.ExchangeAsync(myHelloMessage, cancellationToken);

        var version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
        return version;
    }

    private async ValueTask<NodeFinderProfileMessage?> HandshakeProfileAsync(IConnection connection, CancellationToken cancellationToken = default)
    {
        var myNodeLocation = await this.GetMyNodeLocationAsync(cancellationToken);

        var myProfileMessage = new NodeFinderProfileMessage(_myId, myNodeLocation);
        var otherProfileMessage = await connection.ExchangeAsync(myProfileMessage, cancellationToken);

        return otherProfileMessage;
    }

    private bool TryAddSessionStatus(SessionStatus sessionStatus)
    {
        lock (_lockObject)
        {
            // 自分自身の場合は接続しない
            if (BytesOperations.Equals(_myId.AsSpan(), sessionStatus.Id.Span)) return false;

            // 既に接続済みの場合は接続しない
            if (_sessionStatusSet.Any(n => n.Session.Signature == sessionStatus.Session.Signature)) return false;

            // k-bucketに空きがある場合は追加する
            // kademliaのk-bucketの距離毎のノード数は最大20とする。(k=20)
            var targetNodeDistance = Kademlia.Distance(_myId.AsSpan(), sessionStatus.Id.Span);

            var countMap = new Dictionary<int, int>();
            foreach (var connectionStatus in _sessionStatusSet)
            {
                var nodeDistance = Kademlia.Distance(_myId.AsSpan(), sessionStatus.Id.Span);
                countMap.AddOrUpdate(nodeDistance, (_) => 1, (_, current) => current + 1);
            }

            countMap.TryGetValue(targetNodeDistance, out int count);
            if (count > MaxBucketLength) return false;

            _sessionStatusSet = _sessionStatusSet.Add(sessionStatus);

            return true;
        }
    }

    private async Task SendLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

                foreach (var sessionStatus in _sessionStatusSet)
                {
                    try
                    {
                        var dataMessage = sessionStatus.SendingDataMessage;
                        if (dataMessage is null || !sessionStatus.Session.Connection.Sender.TrySend(dataMessage)) continue;

                        _logger.Debug($"Send data message: {sessionStatus.Session.Address}");

                        sessionStatus.SendingDataMessage = null;
                    }
                    catch (Exception e)
                    {
                        _logger.Debug(e);

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
        }
    }

    private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken).ConfigureAwait(false);

                foreach (var sessionStatus in _sessionStatusSet)
                {
                    try
                    {
                        if (!sessionStatus.Session.Connection.Receiver.TryReceive<NodeFinderDataMessage>(out var dataMessage)) continue;

                        _logger.Debug($"Received data message: {sessionStatus.Session.Address}");

                        sessionStatus.LastReceivedTime = DateTime.UtcNow;

                        await this.AddCloudNodeLocationsAsync(dataMessage.PushCloudNodeLocations, cancellationToken);

                        sessionStatus.ReceivedWantContentClues.UnionWith(dataMessage.WantContentClues);

                        lock (_lockObject)
                        {
                            foreach (var contentLocation in dataMessage.PushContentLocations)
                            {
                                _receivedPushContentLocationMap.AddRange(contentLocation.ContentClue, contentLocation.NodeLocations);
                            }

                            foreach (var contentLocation in dataMessage.GiveContentLocations)
                            {
                                _receivedGiveContentLocationMap.AddRange(contentLocation.ContentClue, contentLocation.NodeLocations);
                            }
                        }
                    }
                    catch (Exception e)
                    {
                        _logger.Debug(e);

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
        }
    }

    private async Task ComputeLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(10), cancellationToken).ConfigureAwait(false);

                await this.AliveSessionMonitoring(cancellationToken);
                await this.ComputeSendingDataMessage(cancellationToken);
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
        catch (Exception e)
        {
            _logger.Error(e);

            throw;
        }
    }

    private async ValueTask AliveSessionMonitoring(CancellationToken cancellationToken = default)
    {
        var now = DateTime.UtcNow;

        foreach (var sessionStatus in _sessionStatusSet)
        {
            var elapsed = (now - sessionStatus.LastReceivedTime);
            if (elapsed.TotalMinutes < 3) continue;

            await this.RemoveSessionStatusAsync(sessionStatus);
        }
    }

    private async ValueTask RemoveSessionStatusAsync(SessionStatus sessionStatus)
    {
        lock (_lockObject)
        {
            _sessionStatusSet = _sessionStatusSet.Remove(sessionStatus);
        }

        await sessionStatus.DisposeAsync();
    }

    private async ValueTask ComputeSendingDataMessage(CancellationToken cancellationToken = default)
    {
        // 自分のノードプロファイル
        var myNodeLocation = await this.GetMyNodeLocationAsync(cancellationToken);

        // ノード情報
        var nodeElements = new List<KademliaElement<NodeElement>>();

        // コンテンツのロケーション情報
        var contentLocationMap = new Dictionary<ContentClue, HashSet<NodeLocation>>();

        // 送信するノードプロファイル
        var sendingPushNodeLocations = new List<NodeLocation>();

        // 送信するコンテンツのロケーション情報
        var sendingPushContentLocationMap = new Dictionary<ContentClue, HashSet<NodeLocation>>();

        // 送信するコンテンツのロケーションリクエスト情報
        var sendingWantContentClueSet = new HashSet<ContentClue>();

        foreach (var connectionStatus in _sessionStatusSet)
        {
            nodeElements.Add(new KademliaElement<NodeElement>(connectionStatus.Id, new NodeElement(connectionStatus)));
        }

        // 自分のノードプロファイルを追加
        sendingPushNodeLocations.Add(await this.GetMyNodeLocationAsync(cancellationToken));

        lock (_lockObject)
        {
            sendingPushNodeLocations.AddRange(_cloudNodeLocations);
        }

        foreach (var contentExchanger in _contentExchangerEventPipe.Publicher.Publish())
        {
            foreach (var contentClue in contentExchanger.GetPushContentClues())
            {
                contentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                    .Add(myNodeLocation);
            }

            foreach (var contentClue in contentExchanger.GetPushContentClues())
            {
                sendingPushContentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                    .Add(myNodeLocation);
            }

            foreach (var contentClue in contentExchanger.GetWantContentClues())
            {
                sendingWantContentClueSet.Add(contentClue);
            }
        }

        lock (_lockObject)
        {
            foreach (var (contentClue, nodeLocations) in _receivedPushContentLocationMap)
            {
                contentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                    .UnionWith(nodeLocations);

                sendingPushContentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                    .UnionWith(nodeLocations);
            }

            foreach (var (tag, nodeLocations) in _receivedGiveContentLocationMap)
            {
                contentLocationMap.GetOrAdd(tag, (_) => new HashSet<NodeLocation>())
                    .UnionWith(nodeLocations);
            }
        }

        foreach (var nodeElement in nodeElements)
        {
            foreach (var tag in nodeElement.Value.SessionStatus.ReceivedWantContentClues)
            {
                sendingWantContentClueSet.Add(tag);
            }
        }

        // Compute PushNodeLocations
        foreach (var element in nodeElements)
        {
            element.Value.SendingPushCloudNodeLocations.AddRange(sendingPushNodeLocations);
        }

        // Compute PushContentLocations
        foreach (var (contentClue, nodeLocations) in sendingPushContentLocationMap)
        {
            foreach (var element in Kademlia.Search(_myId.AsSpan(), contentClue.RootHash.Value.Span, nodeElements, 1))
            {
                element.Value.SendingPushContentLocations[contentClue] = nodeLocations.ToList();
            }
        }

        // Compute WantLocations
        foreach (var contentClue in sendingWantContentClueSet)
        {
            foreach (var element in Kademlia.Search(_myId.AsSpan(), contentClue.RootHash.Value.Span, nodeElements, 1))
            {
                element.Value.SendingWantContentClues.Add(contentClue);
            }
        }

        // Compute GiveLocations
        foreach (var nodeElement in nodeElements)
        {
            foreach (var contentClue in nodeElement.Value.SessionStatus.ReceivedWantContentClues)
            {
                if (!contentLocationMap.TryGetValue(contentClue, out var nodeLocations)) continue;

                nodeElement.Value.SendingGiveContentLocations[contentClue] = nodeLocations.ToList();
            }
        }

        foreach (var element in nodeElements)
        {
            element.Value.SessionStatus.SendingDataMessage =
                new NodeFinderDataMessage(
                    element.Value.SendingPushCloudNodeLocations.Take(NodeFinderDataMessage.MaxPushCloudNodeLocationsCount).ToArray(),
                    element.Value.SendingWantContentClues.Take(NodeFinderDataMessage.MaxWantContentCluesCount).ToArray(),
                    element.Value.SendingPushContentLocations.Select(n => new ContentLocation(n.Key, n.Value.Take(ContentLocation.MaxNodeLocationsCount).ToArray())).Take(NodeFinderDataMessage.MaxPushContentLocationsCount).ToArray(),
                    element.Value.SendingGiveContentLocations.Select(n => new ContentLocation(n.Key, n.Value.Take(ContentLocation.MaxNodeLocationsCount).ToArray())).Take(NodeFinderDataMessage.MaxGiveContentLocationsCount).ToArray());
        }
    }

    private sealed class NodeElement
    {
        public NodeElement(SessionStatus sessionStatus)
        {
            this.SessionStatus = sessionStatus;
        }

        public SessionStatus SessionStatus { get; }

        public List<NodeLocation> SendingPushCloudNodeLocations { get; } = new List<NodeLocation>();

        public List<ContentClue> SendingWantContentClues { get; } = new List<ContentClue>();

        public Dictionary<ContentClue, List<NodeLocation>> SendingPushContentLocations { get; } = new Dictionary<ContentClue, List<NodeLocation>>();

        public Dictionary<ContentClue, List<NodeLocation>> SendingGiveContentLocations { get; } = new Dictionary<ContentClue, List<NodeLocation>>();
    }
}
