using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Helpers;
using Omnius.Core.Net;
using Omnius.Core.Net.Connections;
using Omnius.Xeus.Engines.Internal;
using Omnius.Xeus.Engines.Internal.Models;
using Omnius.Xeus.Engines.Primitives;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public sealed partial class NodeFinder : AsyncDisposableBase, INodeFinder
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly NodeFinderOptions _options;

        private readonly byte[] _myId;

        private readonly HashSet<SessionStatus> _sessionStatusSet = new();
        private readonly LinkedList<NodeLocation> _cloudNodeLocations = new();

        private readonly HashSet<string> _availableEngineNameSet = new();
        private readonly object _availableEngineNameSetLockObject = new();

        private readonly VolatileListDictionary<ContentClue, NodeLocation> _receivedPushContentLocationMap = new(TimeSpan.FromMinutes(30));
        private readonly VolatileListDictionary<ContentClue, NodeLocation> _receivedGiveContentLocationMap = new(TimeSpan.FromMinutes(30));

        private readonly Task _connectLoopTask;
        private readonly Task _acceptLoopTask;
        private readonly Task _sendLoopTask;
        private readonly Task _receiveLoopTask;
        private readonly Task _computeLoopTask;

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly object _lockObject = new();

        private const int MaxBucketLength = 20;
        private const string ServiceName = "node_finder";

        internal NodeFinder(NodeFinderOptions options)
        {
            _options = options;
            _myId = GenId();

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
        }

        public async ValueTask<NodeFinderReport> GetReportAsync(CancellationToken cancellationToken = default)
        {
            var sessionReports = new List<SessionReport>();

            foreach (var status in _sessionStatusSet)
            {
                sessionReports.Add(new SessionReport(status.Session.HandshakeType, status.Session.Address));
            }

            return new NodeFinderReport(0, 0, sessionReports.ToArray());
        }

        public async ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default)
        {
            var addresses = new List<OmniAddress>();
            foreach (var accepter in _options.Accepters ?? Enumerable.Empty<ISessionAccepter>())
            {
                addresses.AddRange(await accepter.GetListenEndpointsAsync(cancellationToken));
            }

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

        public async ValueTask<NodeLocation[]> FindNodeLocationsAsync(ContentClue clue, CancellationToken cancellationToken = default)
        {
            lock (_lockObject)
            {
                var result = new HashSet<NodeLocation>();

                if (_receivedPushContentLocationMap.TryGetValue(clue, out var nodeLocations1))
                {
                    result.UnionWith(nodeLocations1);
                }

                if (_receivedGiveContentLocationMap.TryGetValue(clue, out var nodeLocations2))
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

        private readonly VolatileHashSet<OmniAddress> _connectedAddressSet = new(TimeSpan.FromMinutes(3));

        private async Task ConnectLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);

                    lock (_lockObject)
                    {
                        int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Connected).Count();

                        if (_sessionStatusSet.Count > (_options.MaxSessionCount / 2)) continue;
                    }

                    NodeLocation? targetNodeLocation = null;

                    lock (_lockObject)
                    {
                        var nodeLocations = _cloudNodeLocations.ToArray();
                        random.Shuffle(nodeLocations);

                        var ignoreAddressSet = new HashSet<OmniAddress>();
                        ignoreAddressSet.UnionWith(_sessionStatusSet.Select(n => n.Session.Address));
                        ignoreAddressSet.UnionWith(_connectedAddressSet);

                        targetNodeLocation = nodeLocations
                            .Where(n => !n.Addresses.Any(n => ignoreAddressSet.Contains(n)))
                            .FirstOrDefault();
                    }

                    if (targetNodeLocation == null) continue;

                    bool succeeded = false;

                    foreach (var targetAddress in targetNodeLocation.Addresses)
                    {
                        foreach (var connector in _options.Connectors ?? Enumerable.Empty<ISessionConnector>())
                        {
                            var session = await connector.ConnectAsync(targetAddress, ServiceName, cancellationToken);
                            if (session is null) continue;

                            _connectedAddressSet.Add(targetAddress);

                            if (await this.TryAddSessionAsync(session, cancellationToken))
                            {
                                succeeded = true;
                                goto End;
                            }
                        }
                    }

                End:

                    if (succeeded)
                    {
                        this.RefreshCloudNodeLocation(targetNodeLocation);
                    }
                    else
                    {
                        this.RemoveCloudNodeLocation(targetNodeLocation);
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

        private async Task AcceptLoopAsync(CancellationToken cancellationToken)
        {
            try
            {
                var random = new Random();

                while (!cancellationToken.IsCancellationRequested)
                {
                    await Task.Delay(1000, cancellationToken);

                    lock (_lockObject)
                    {
                        int connectionCount = _sessionStatusSet.Select(n => n.Session.HandshakeType == SessionHandshakeType.Accepted).Count();

                        if (_sessionStatusSet.Count > (_options.MaxSessionCount / 2)) continue;
                    }

                    foreach (var accepter in _options.Accepters ?? Enumerable.Empty<ISessionAccepter>())
                    {
                        var session = await accepter.AcceptAsync(ServiceName, cancellationToken);
                        if (session is null) continue;

                        await this.TryAddSessionAsync(session, cancellationToken);
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
                if (version is null) return false;

                if (version == NodeFinderVersion.Version1)
                {
                    var profile = await this.HandshakeProfileAsync(session.Connection, cancellationToken);
                    if (profile is null) return false;

                    var sessionStatus = new SessionStatus(session, profile.Id, profile.NodeLocation);
                    return this.TryAddSessionStatus(sessionStatus);
                }

                throw new NotSupportedException();
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);
            }
            catch (Exception e)
            {
                _logger.Warn(e);
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
                if (_sessionStatusSet.Any(n => BytesOperations.Equals(n.Id.Span, sessionStatus.Id.Span))) return false;

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

                _sessionStatusSet.Add(sessionStatus);

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

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    foreach (var sessionStatus in _sessionStatusSet.ToArray())
                    {
                        try
                        {
                            lock (sessionStatus.LockObject)
                            {
                                if (sessionStatus.SendingDataMessage != null)
                                {
                                    if (sessionStatus.Session.Connection.Sender.TrySend(sessionStatus.SendingDataMessage))
                                    {
                                        sessionStatus.SendingDataMessage = null;
                                    }
                                }
                            }
                        }
                        catch (ConnectionException e)
                        {
                            _logger.Debug(e);

                            lock (_lockObject)
                            {
                                _sessionStatusSet.Remove(sessionStatus);
                            }
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

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    foreach (var sessionStatus in _sessionStatusSet.ToArray())
                    {
                        try
                        {
                            if (sessionStatus.Session.Connection.Receiver.TryReceive<NodeFinderDataMessage>(out var dataMessage))
                            {
                                await this.AddCloudNodeLocationsAsync(dataMessage.PushCloudNodeLocations, cancellationToken);

                                lock (sessionStatus.LockObject)
                                {
                                    sessionStatus.ReceivedWantContentClues.UnionWith(dataMessage.WantContentClues);
                                }

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
                        }
                        catch (ConnectionException e)
                        {
                            _logger.Debug(e);

                            lock (_lockObject)
                            {
                                _sessionStatusSet.Remove(sessionStatus);
                            }
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

                    await Task.Delay(TimeSpan.FromSeconds(30), cancellationToken);

                    lock (_lockObject)
                    {
                        _connectedAddressSet.Refresh();
                        _receivedPushContentLocationMap.Refresh();
                        _receivedGiveContentLocationMap.Refresh();

                        foreach (var connectionStatus in _sessionStatusSet)
                        {
                            connectionStatus.Refresh();
                        }
                    }

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

                    lock (_lockObject)
                    {
                        foreach (var connectionStatus in _sessionStatusSet)
                        {
                            nodeElements.Add(new KademliaElement<NodeElement>(connectionStatus.Id, new NodeElement(connectionStatus)));
                        }
                    }

                    // 自分のノードプロファイルを追加
                    sendingPushNodeLocations.Add(await this.GetMyNodeLocationAsync(cancellationToken));

                    lock (_lockObject)
                    {
                        sendingPushNodeLocations.AddRange(_cloudNodeLocations);
                    }

                    foreach (var exchanger in _options.Exchangers ?? Enumerable.Empty<IContentExchanger>())
                    {
                        foreach (var contentClue in exchanger.GetPushContentClues())
                        {
                            contentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                                .Add(myNodeLocation);
                        }

                        foreach (var contentClue in exchanger.GetPushContentClues())
                        {
                            sendingPushContentLocationMap.GetOrAdd(contentClue, (_) => new HashSet<NodeLocation>())
                                .Add(myNodeLocation);
                        }

                        foreach (var contentClue in exchanger.GetWantContentClues())
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
                        lock (nodeElement.Value.SessionStatus.LockObject)
                        {
                            foreach (var tag in nodeElement.Value.SessionStatus.ReceivedWantContentClues)
                            {
                                sendingWantContentClueSet.Add(tag);
                            }
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
                        foreach (var element in Kademlia.Search(_myId.AsSpan(), contentClue.ContentHash.Value.Span, nodeElements, 1))
                        {
                            element.Value.SendingPushContentLocations[contentClue] = nodeLocations.ToList();
                        }
                    }

                    // Compute WantLocations
                    foreach (var contentClue in sendingWantContentClueSet)
                    {
                        foreach (var element in Kademlia.Search(_myId.AsSpan(), contentClue.ContentHash.Value.Span, nodeElements, 1))
                        {
                            element.Value.SendingWantContentClues.Add(contentClue);
                        }
                    }

                    // Compute GiveLocations
                    foreach (var nodeElement in nodeElements)
                    {
                        lock (nodeElement.Value.SessionStatus.LockObject)
                        {
                            foreach (var contentClue in nodeElement.Value.SessionStatus.ReceivedWantContentClues)
                            {
                                if (!contentLocationMap.TryGetValue(contentClue, out var nodeLocations)) continue;

                                nodeElement.Value.SendingGiveContentLocations[contentClue] = nodeLocations.ToList();
                            }
                        }
                    }

                    foreach (var element in nodeElements)
                    {
                        lock (element.Value.SessionStatus.LockObject)
                        {
                            element.Value.SessionStatus.SendingDataMessage =
                                new NodeFinderDataMessage(
                                    element.Value.SendingPushCloudNodeLocations.Take(NodeFinderDataMessage.MaxPushCloudNodeLocationsCount).ToArray(),
                                    element.Value.SendingWantContentClues.Take(NodeFinderDataMessage.MaxWantContentCluesCount).ToArray(),
                                    element.Value.SendingPushContentLocations.Select(n => new ContentLocation(n.Key, n.Value.Take(ContentLocation.MaxNodeLocationsCount).ToArray())).Take(NodeFinderDataMessage.MaxPushContentLocationsCount).ToArray(),
                                    element.Value.SendingGiveContentLocations.Select(n => new ContentLocation(n.Key, n.Value.Take(ContentLocation.MaxNodeLocationsCount).ToArray())).Take(NodeFinderDataMessage.MaxGiveContentLocationsCount).ToArray());
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
}