using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics;
using System.Security.Cryptography;
using Omnius.Axis.Engines.Internal;
using Omnius.Axis.Engines.Internal.Models;
using Omnius.Axis.Engines.Internal.Repositories;
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

    private readonly NodeFinderRepository _nodeFinderRepo;

    private readonly FuncPipe<IEnumerable<ContentClue>> _getPushContentCluesFuncPipe = new();
    private readonly FuncPipe<IEnumerable<ContentClue>> _getWantContentCluesFuncPipe = new();
    private readonly Events _events;

    private readonly byte[] _myId;

    private readonly VolatileHashSet<OmniAddress> _connectedAddressSet;
    private ImmutableHashSet<SessionStatus> _sessionStatusSet = ImmutableHashSet<SessionStatus>.Empty;

    private readonly VolatileListDictionary<ContentClue, NodeLocation> _receivedPushContentLocationMap;
    private readonly VolatileListDictionary<ContentClue, NodeLocation> _receivedGiveContentLocationMap;

    private Task? _connectLoopTask;
    private Task? _acceptLoopTask;
    private Task? _sendLoopTask;
    private Task? _receiveLoopTask;
    private Task? _computeLoopTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly Random _random = new();

    private readonly object _lockObject = new();

    private const int MaxBucketLength = 20;
    private const int MaxNodeLocationCount = 1000;
    private const string Schema = "node_finder";

    public static async ValueTask<NodeFinder> CreateAsync(ISessionConnector sessionConnector, ISessionAccepter sessionAccepter, IBatchActionDispatcher batchActionDispatcher,
        IBytesPool bytesPool, NodeFinderOptions options, CancellationToken cancellationToken = default)
    {
        var nodeFinder = new NodeFinder(sessionConnector, sessionAccepter, batchActionDispatcher, bytesPool, options);
        await nodeFinder.InitAsync(cancellationToken);
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

        _nodeFinderRepo = new NodeFinderRepository(Path.Combine(_options.ConfigDirectoryPath, "status"));

        _events = new Events(_getPushContentCluesFuncPipe.Listener, _getWantContentCluesFuncPipe.Listener);

        _connectedAddressSet = new VolatileHashSet<OmniAddress>(TimeSpan.FromSeconds(30), TimeSpan.FromSeconds(10), _batchActionDispatcher);

        _receivedPushContentLocationMap = new VolatileListDictionary<ContentClue, NodeLocation>(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(30), _batchActionDispatcher);
        _receivedGiveContentLocationMap = new VolatileListDictionary<ContentClue, NodeLocation>(TimeSpan.FromMinutes(30), TimeSpan.FromSeconds(30), _batchActionDispatcher);
    }

    private async ValueTask InitAsync(CancellationToken cancellationToken = default)
    {
        await _nodeFinderRepo.MigrateAsync(cancellationToken);

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
        await Task.WhenAll(_connectLoopTask!, _acceptLoopTask!, _sendLoopTask!, _receiveLoopTask!, _computeLoopTask!);
        _cancellationTokenSource.Dispose();

        foreach (var sessionStatus in _sessionStatusSet)
        {
            await sessionStatus.DisposeAsync();
        }

        _sessionStatusSet = _sessionStatusSet.Clear();

        _connectedAddressSet.Dispose();
        _receivedPushContentLocationMap.Dispose();
        _receivedGiveContentLocationMap.Dispose();

        _nodeFinderRepo.Dispose();
    }

    public INodeFinderEvents GetEvents() => _events;

    public async ValueTask<IEnumerable<SessionReport>> GetSessionReportsAsync(CancellationToken cancellationToken = default)
    {
        var sessionReports = new List<SessionReport>();

        foreach (var status in _sessionStatusSet)
        {
            sessionReports.Add(new SessionReport(Schema, status.Session.HandshakeType, status.Session.Address));
        }

        return sessionReports.ToArray();
    }

    public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
    {
        var addresses = new List<OmniAddress>();
        addresses.AddRange(await _sessionAccepter.GetListenEndpointsAsync(cancellationToken));

        var myNodeLocation = new NodeLocation(addresses.ToArray());
        return myNodeLocation;
    }

    public async ValueTask<IEnumerable<NodeLocation>> GetCloudNodeLocationsAsync(CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            return _nodeFinderRepo.NodeLocations.FindAll();
        }
    }

    public async ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default)
    {
        lock (_lockObject)
        {
            foreach (var nodeLocation in nodeLocations)
            {
                _nodeFinderRepo.NodeLocations.TryInsert(nodeLocation, DateTime.UtcNow);
            }
        }
    }

    public async ValueTask<NodeLocation[]> FindNodeLocationsAsync(ContentClue contentClue, CancellationToken cancellationToken = default)
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

    private void RefreshCloudNodeLocation(NodeLocation nodeLocation)
    {
        lock (_lockObject)
        {
            _nodeFinderRepo.NodeLocations.Upsert(nodeLocation, DateTime.UtcNow, DateTime.UtcNow);
        }
    }

    private async Task ConnectLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                var sessionStatuses = _sessionStatusSet
                    .Where(n => n.Session.HandshakeType == SessionHandshakeType.Connected).ToList();

                if (sessionStatuses.Count > _options.MaxSessionCount / 2) continue;

                foreach (var nodeLocation in await this.FindNodeLocationsForConnecting(cancellationToken))
                {
                    var success = await this.TryConnectAsync(nodeLocation, cancellationToken);
                    if (success) break;
                }
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

    private async ValueTask<IEnumerable<NodeLocation>> FindNodeLocationsForConnecting(CancellationToken cancellationToken = default)
    {
        var nodeLocations = _nodeFinderRepo.NodeLocations.FindAll().ToList();
        _random.Shuffle(nodeLocations);

        var ignoredAddressSet = await this.GetIgnoredAddressSet(cancellationToken);

        return nodeLocations
            .Where(n => !n.Addresses.Any(n => ignoredAddressSet.Contains(n)))
            .ToArray();
    }

    private async ValueTask<HashSet<OmniAddress>> GetIgnoredAddressSet(CancellationToken cancellationToken)
    {
        var myNodeLocation = await this.GetMyNodeLocationAsync();

        var set = new HashSet<OmniAddress>();

        set.UnionWith(myNodeLocation.Addresses);
        set.UnionWith(_sessionStatusSet.Select(n => n.Session.Address));
        set.UnionWith(_connectedAddressSet);

        return set;
    }

    private async ValueTask<bool> TryConnectAsync(NodeLocation nodeLocation, CancellationToken cancellationToken = default)
    {
        try
        {
            foreach (var targetAddress in nodeLocation.Addresses)
            {
                _connectedAddressSet.Add(targetAddress);

                var session = await _sessionConnector.ConnectAsync(targetAddress, Schema, cancellationToken);
                if (session is null) continue;

                var success = await this.TryAddSessionAsync(session, cancellationToken);

                if (!success)
                {
                    await session.DisposeAsync();
                    continue;
                }

                this.RefreshCloudNodeLocation(nodeLocation);

                return true;
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

        return false;
    }

    private async Task AcceptLoopAsync(CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                await Task.Delay(1000, cancellationToken).ConfigureAwait(false);

                var sessionStatuses = _sessionStatusSet
                    .Where(n => n.Session.HandshakeType == SessionHandshakeType.Accepted).ToList();
                if (sessionStatuses.Count > _options.MaxSessionCount / 2) continue;

                var session = await _sessionAccepter.AcceptAsync(Schema, cancellationToken);
                if (session is null) continue;

                bool success = await this.TryAddSessionAsync(session, cancellationToken);
                if (!success) await session.DisposeAsync();
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

    private async ValueTask<bool> TryAddSessionAsync(ISession session, CancellationToken cancellationToken = default)
    {
        try
        {
            var version = await this.HandshakeVersionAsync(session.Connection, cancellationToken);

            if (version == NodeFinderVersion.Version1)
            {
                var profile = await this.HandshakeProfileAsync(session.Connection, cancellationToken);

                return this.InternalTryAddSession(session, profile.Id, profile.NodeLocation);
            }

            _logger.Debug("Unknown Version");

            return false;
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
        catch (ConnectionException)
        {
            _logger.Debug("Connection Exception");
        }
        catch (Exception e)
        {
            _logger.Error(e, "Unexpected Exception");
        }

        return false;
    }

    private async ValueTask<NodeFinderVersion> HandshakeVersionAsync(IConnection connection, CancellationToken cancellationToken = default)
    {
        var myHelloMessage = new NodeFinderHelloMessage(new[] { NodeFinderVersion.Version1 });
        var otherHelloMessage = await connection.ExchangeAsync(myHelloMessage, cancellationToken);

        var version = EnumHelper.GetOverlappedMaxValue(myHelloMessage.Versions, otherHelloMessage.Versions);
        return version ?? NodeFinderVersion.Unknown;
    }

    private async ValueTask<NodeFinderProfileMessage> HandshakeProfileAsync(IConnection connection, CancellationToken cancellationToken = default)
    {
        var myNodeLocation = await this.GetMyNodeLocationAsync(cancellationToken);

        var myProfileMessage = new NodeFinderProfileMessage(_myId, myNodeLocation);
        var otherProfileMessage = await connection.ExchangeAsync(myProfileMessage, cancellationToken);

        return otherProfileMessage;
    }

    private bool InternalTryAddSession(ISession session, ReadOnlyMemory<byte> id, NodeLocation location)
    {
        lock (_lockObject)
        {
            // 自分自身の場合は接続しない
            if (BytesOperations.Equals(_myId.AsSpan(), id.Span))
            {
                _logger.Debug($"Connected to myself");
                return false;
            }

            // 既に接続済みの場合は接続しない
            if (_sessionStatusSet.Any(n => n.Session.Signature == session.Signature))
            {
                _logger.Debug($"Already connected");
                return false;
            }

            // k-bucketに空きがある場合は追加する
            // kademliaのk-bucketの距離毎のノード数は最大20とする。(k=20)
            var countMap = new Dictionary<int, int>();
            foreach (var connectionStatus in _sessionStatusSet)
            {
                var nodeDistance = Kademlia.Distance(_myId.AsSpan(), id.Span);
                countMap.AddOrUpdate(nodeDistance, (_) => 1, (_, current) => current + 1);
            }

            var targetNodeDistance = Kademlia.Distance(_myId.AsSpan(), id.Span);
            countMap.TryGetValue(targetNodeDistance, out int count);

            if (count > MaxBucketLength)
            {
                _logger.Debug("Overflowed connections per bucket");
                return false;
            }

            _sessionStatusSet = _sessionStatusSet.Add(new SessionStatus(session, id, location, _batchActionDispatcher));

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

                        if (dataMessage.PushCloudNodeLocations.Count > 0)
                        {
                            _logger.Debug($"Send PushCloudNodeLocations: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.PushCloudNodeLocations.Count})");
                        }

                        if (dataMessage.WantContentClues.Count > 0)
                        {
                            _logger.Debug($"Send WantContentClues: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.WantContentClues.Count})");
                        }

                        if (dataMessage.PushContentLocations.Count > 0)
                        {
                            _logger.Debug($"Send PushContentLocations: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.PushContentLocations.Count})");
                        }

                        if (dataMessage.GiveContentLocations.Count > 0)
                        {
                            _logger.Debug($"Send GiveContentLocations: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.GiveContentLocations.Count})");
                        }

                        sessionStatus.SendingDataMessage = null;
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.Debug("Object Disposed");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                    catch (ConnectionException)
                    {
                        _logger.Debug("Connection Exception");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Unexpected Exception");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                }
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

                        sessionStatus.LastReceivedTime = DateTime.UtcNow;

                        if (dataMessage.PushCloudNodeLocations.Count > 0)
                        {
                            await this.AddCloudNodeLocationsAsync(dataMessage.PushCloudNodeLocations, cancellationToken);
                            _logger.Debug($"Receive PushCloudNodeLocations: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.PushCloudNodeLocations.Count})");
                        }

                        if (dataMessage.WantContentClues.Count > 0)
                        {
                            sessionStatus.ReceivedWantContentClues.UnionWith(dataMessage.WantContentClues);
                            _logger.Debug($"Receive WantContentClues: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.WantContentClues.Count})");
                        }

                        if (dataMessage.PushContentLocations.Count > 0)
                        {
                            foreach (var contentLocation in dataMessage.PushContentLocations)
                            {
                                _receivedPushContentLocationMap.AddRange(contentLocation.ContentClue, contentLocation.NodeLocations);
                            }

                            _logger.Debug($"Receive PushContentLocations: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.PushContentLocations.Count})");
                        }

                        if (dataMessage.GiveContentLocations.Count > 0)
                        {
                            foreach (var contentLocation in dataMessage.GiveContentLocations)
                            {
                                _receivedGiveContentLocationMap.AddRange(contentLocation.ContentClue, contentLocation.NodeLocations);
                            }

                            _logger.Debug($"Receive GiveContentLocations: (Address: {sessionStatus.Session.Address}, Count: {dataMessage.GiveContentLocations.Count})");
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        _logger.Debug("Object Disposed");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                    catch (ConnectionException)
                    {
                        _logger.Debug("Connection Exception");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                    catch (Exception e)
                    {
                        _logger.Error(e, "Unexpected Exception");

                        await this.RemoveSessionStatusAsync(sessionStatus);
                    }
                }
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

    private async Task ComputeLoopAsync(CancellationToken cancellationToken)
    {
        var nodeLocationsTrimExcessStopwatch = new Stopwatch();
        var trimDeadSessionsStopwatch = new Stopwatch();
        var computeSendingDataMessageStopwatch = new Stopwatch();

        try
        {
            for (; ; )
            {
                cancellationToken.ThrowIfCancellationRequested();

                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);

                if (nodeLocationsTrimExcessStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromMinutes(1)))
                {
                    _nodeFinderRepo.NodeLocations.TrimExcess(MaxNodeLocationCount);
                }

                if (trimDeadSessionsStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromMinutes(1)))
                {
                    await this.TrimDeadSessionsAsync(cancellationToken);
                }

                if (computeSendingDataMessageStopwatch.TryRestartIfElapsedOrStopped(TimeSpan.FromSeconds(10)))
                {
                    await this.ComputeSendingDataMessageAsync(cancellationToken);
                }
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

    private async ValueTask TrimDeadSessionsAsync(CancellationToken cancellationToken = default)
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
        _sessionStatusSet = _sessionStatusSet.Remove(sessionStatus);
        await sessionStatus.DisposeAsync();
    }

    private async ValueTask ComputeSendingDataMessageAsync(CancellationToken cancellationToken = default)
    {
        // 自分のノードロケーション
        var myNodeLocation = await this.GetMyNodeLocationAsync(cancellationToken);

        // ノード情報
        var nodeElements = new List<KademliaElement<NodeElement>>();

        // コンテンツのロケーション情報
        var contentLocationMap = new Dictionary<ContentClue, HashSet<NodeLocation>>();

        // 送信するノードロケーション
        var sendingPushNodeLocations = new List<NodeLocation>();

        // 送信するコンテンツのロケーション情報
        var sendingPushContentLocationMap = new Dictionary<ContentClue, HashSet<NodeLocation>>();

        // 送信するコンテンツのロケーションリクエスト情報
        var sendingWantContentClueSet = new HashSet<ContentClue>();

        // ノード情報を追加
        foreach (var connectionStatus in _sessionStatusSet)
        {
            nodeElements.Add(new KademliaElement<NodeElement>(connectionStatus.Id, new NodeElement(connectionStatus)));
        }

        // 自分のノードロケーションを追加
        sendingPushNodeLocations.Add(await this.GetMyNodeLocationAsync(cancellationToken));

        // 接続中のノードのノードロケーションを追加
        sendingPushNodeLocations.AddRange(nodeElements.Select(n => n.Value.SessionStatus.NodeLocation).Where(n => n.Addresses.Count > 0));

        foreach (var contentClue in _getPushContentCluesFuncPipe.Caller.Call().Flatten())
        {
            contentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                .Add(myNodeLocation);

            sendingPushContentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                .Add(myNodeLocation);
        }

        foreach (var contentClue in _getWantContentCluesFuncPipe.Caller.Call().Flatten())
        {
            contentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                .Add(myNodeLocation);

            sendingPushContentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                .Add(myNodeLocation);

            sendingWantContentClueSet.Add(contentClue);
        }

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

        foreach (var nodeElement in nodeElements)
        {
            foreach (var tag in nodeElement.Value.SessionStatus.ReceivedWantContentClues)
            {
                sendingWantContentClueSet.Add(tag);
            }
        }

        // Compute SendingPushCloudNodeLocations
        foreach (var element in nodeElements)
        {
            element.Value.SendingPushCloudNodeLocations.AddRange(sendingPushNodeLocations);
        }

        // Compute SendingPushContentLocations
        foreach (var (contentClue, nodeLocations) in sendingPushContentLocationMap)
        {
            foreach (var element in Kademlia.Search(_myId.AsSpan(), contentClue.RootHash.Value.Span, nodeElements, 1))
            {
                element.Value.SendingPushContentLocations[contentClue] = nodeLocations.ToList();
            }
        }

        // Compute SendingWantContentClues
        foreach (var contentClue in sendingWantContentClueSet)
        {
            foreach (var element in Kademlia.Search(_myId.AsSpan(), contentClue.RootHash.Value.Span, nodeElements, 1))
            {
                element.Value.SendingWantContentClues.Add(contentClue);
            }
        }

        // Compute SendingGiveContentLocations
        foreach (var nodeElement in nodeElements)
        {
            foreach (var contentClue in nodeElement.Value.SessionStatus.ReceivedWantContentClues)
            {
                if (!contentLocationMap.TryGetValue(contentClue, out var nodeLocations)) continue;

                nodeElement.Value.SendingGiveContentLocations[contentClue] = nodeLocations.Randomize().ToList();
            }
        }

        foreach (var element in nodeElements)
        {
            element.Value.SessionStatus.SendingDataMessage =
                new NodeFinderDataMessage(
                    element.Value.SendingPushCloudNodeLocations.Randomize().Take(NodeFinderDataMessage.MaxPushCloudNodeLocationsCount).ToArray(),
                    element.Value.SendingWantContentClues.Randomize().Take(NodeFinderDataMessage.MaxWantContentCluesCount).ToArray(),
                    element.Value.SendingPushContentLocations.Randomize().Select(n => new ContentLocation(n.Key, n.Value.Randomize().Take(ContentLocation.MaxNodeLocationsCount).ToArray())).Take(NodeFinderDataMessage.MaxPushContentLocationsCount).ToArray(),
                    element.Value.SendingGiveContentLocations.Randomize().Select(n => new ContentLocation(n.Key, n.Value.Randomize().Take(ContentLocation.MaxNodeLocationsCount).ToArray())).Take(NodeFinderDataMessage.MaxGiveContentLocationsCount).ToArray());
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
