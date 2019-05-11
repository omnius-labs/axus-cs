using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading;
using Omnix.Base;
using Omnix.Base.Extensions;
using Omnix.Collections;
using Omnix.Configuration;
using Omnix.Cryptography;
using Omnix.Io;
using Omnix.Network;
using Omnix.Network.Connection;
using Omnix.Network.Connection.Secure;
using Omnix.Serialization;
using Xeus.Core.Contents;
using Xeus.Core.Exchange.Internal;
using Xeus.Messages;

namespace Xeus.Core.Exchange
{
    sealed partial class ExchangeEngine : ServiceBase, ISettings
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private BufferPool _bufferPool;
        private ContentsStorage _contentsStorage;
        private MetadataManager _metadataManager;
        private BandwidthController _bandwidthController;
        private HopLimitComputer _hopLimitComputer;

        private Settings _settings;

        private string[] _passwords;
        private LockedHashSet<OmniAddress> _myNodeAddresses = new LockedHashSet<OmniAddress>();
        private LockedHashSet<OmniAddress> _cloudNodeAdressess = new LockedHashSet<OmniAddress>();
        private LockedHashSet<DiffuseBlockInfo> _myDiffuseBlockInfos = new LockedHashSet<DiffuseBlockInfo>();
        private LockedHashSet<DiffuseBlockInfo> _cloudDiffuseBlockInfos = new LockedHashSet<DiffuseBlockInfo>();

        private volatile byte[] _baseId;
        private LockedHashDictionary<OmniSecureConnection, SessionStatus> _connections = new LockedHashDictionary<OmniSecureConnection, SessionStatus>();

        private VolatileHashSet<OmniSignature> _wantBroadcastClueSet = new VolatileHashSet<OmniSignature>(new TimeSpan(0, 10, 0));
        private VolatileHashSet<OmniSignature> _wantUnicastClueSet = new VolatileHashSet<OmniSignature>(new TimeSpan(0, 10, 0));
        private VolatileHashSet<Channel> _wantMulticastClueSet = new VolatileHashSet<Channel>(new TimeSpan(0, 10, 0));
        private VolatileHashSet<OmniHash> _wantBlocksRequestSet = new VolatileHashSet<OmniHash>(new TimeSpan(0, 10, 0));

        private List<TaskManager> _connectTaskManagers = new List<TaskManager>();
        private List<TaskManager> _acceptTaskManagers = new List<TaskManager>();

        private TaskManager _doEventTaskManager;
        private TaskManager _computeTaskManager;

        private List<TaskManager> _enqueueTaskManagers = new List<TaskManager>();
        private List<TaskManager> _dequeueTaskManagers = new List<TaskManager>();

        private ServiceStateType _type = ServiceStateType.Stopped;

        private ReaderWriterLockProvider _connectionLockProvider = new ReaderWriterLockProvider(LockRecursionPolicy.SupportsRecursion);

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        private const int MaxLocationCount = 256;
        private const int MaxMetadataRequestCount = 256;
        private const int MaxMetadataResultCount = 256;
        private const int MaxBlockLinkCount = 256;
        private const int MaxBlockRequestCount = 256;

        private const int ConnectionCountUpperLimit = 8;

        private readonly int _threadCount = Math.Max((Environment.ProcessorCount / 2), 2);

        private static bool _decrementAtMyHopLimitMaximum;
        private static bool _decrementAtMyHopLimitMinimum;

        static ExchangeEngine()
        {
            var random = RandomProvider.GetThreadRandom();

            _decrementAtMyHopLimitMaximum = (random.Next(0, int.MaxValue) % 2 == 0);
            _decrementAtMyHopLimitMinimum = (random.Next(0, int.MaxValue) % 2 == 0);
        }

        public ExchangeEngine(string configPath, ContentsStorage contentsStorage, BufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _contentsStorage = contentsStorage;
            _metadataManager = new MetadataManager();
            _metadataManager.GetLockedSignaturesEvent += () => this.OnGetLockedSignatures();
            _bandwidthController = new BandwidthController();
            _hopLimitComputer = new HopLimitComputer();

            _settings = new Settings(configPath);

            for (int i = 0; i < 3; i++)
            {
                _connectTaskManagers.Add(new TaskManager(this.ConnectThread));
                _acceptTaskManagers.Add(new TaskManager(this.AcceptThread));
            }

            _doEventTaskManager = new TaskManager(this.DoEventThread);
            _computeTaskManager = new TaskManager(this.ComputeThread);

            foreach (int i in Enumerable.Range(0, _threadCount))
            {
                _enqueueTaskManagers.Add(new TaskManager((token) => this.EnqueueThread(i, token)));
                _dequeueTaskManagers.Add(new TaskManager((token) => this.DequeueThread(i, token)));
            }

            this.UpdateBaseId();
        }

        private volatile NetworkStatus _networkStatus = new NetworkStatus();

        public class NetworkStatus
        {
            public AtomicCounter ConnectCount { get; } = new AtomicCounter();
            public AtomicCounter AcceptCount { get; } = new AtomicCounter();

            public AtomicCounter ReceivedByteCount { get; } = new AtomicCounter();
            public AtomicCounter SentByteCount { get; } = new AtomicCounter();

            public AtomicCounter PushLocationCount { get; } = new AtomicCounter();
            public AtomicCounter PushBlockLinkCount { get; } = new AtomicCounter();
            public AtomicCounter PushBlockRequestCount { get; } = new AtomicCounter();
            public AtomicCounter PushBlockResultCount { get; } = new AtomicCounter();
            public AtomicCounter PushMessageRequestCount { get; } = new AtomicCounter();
            public AtomicCounter PushMessageResultCount { get; } = new AtomicCounter();

            public AtomicCounter PullLocationCount { get; } = new AtomicCounter();
            public AtomicCounter PullBlockLinkCount { get; } = new AtomicCounter();
            public AtomicCounter PullBlockRequestCount { get; } = new AtomicCounter();
            public AtomicCounter PullBlockResultCount { get; } = new AtomicCounter();
            public AtomicCounter PullMessageRequestCount { get; } = new AtomicCounter();
            public AtomicCounter PullMessageResultCount { get; } = new AtomicCounter();
        }

        public IEnumerable<RelayExchangeConnectionReport> GetRelayExchangeConnectionReports()
        {
            var list = new List<RelayExchangeConnectionReport>();

            foreach (var (connection, SessionStatus) in _connections.ToArray())
            {
                if (SessionStatus.Id == null) continue;

                list.Add(new RelayExchangeConnectionReport(
                    SessionStatus.Id,
                    connection.Type,
                    SessionStatus.Uri,
                    SessionStatus.Location,
                    SessionStatus.Priority.GetValue(),
                    connection.ReceivedByteCount,
                    connection.SentByteCount));
            }

            return list;
        }

        public void SetCloudNodeAddresses(IEnumerable<OmniAddress> addresses)
        {
            _cloudNodeAdressess.UnionWith(addresses);
        }

        public GetSignaturesEventHandler GetLockedSignaturesEvent { get; set; }

        public ConnectCapEventHandler ConnectCapEvent { private get; set; }
        public AcceptCapEventHandler AcceptCapEvent { private get; set; }

        private IEnumerable<OmniSignature> OnGetLockedSignatures()
        {
            return this.GetLockedSignaturesEvent?.Invoke() ?? Enumerable.Empty<OmniSignature>();
        }

        private Cap OnConnectCap(OmniAddress address)
        {
            return this.ConnectCapEvent?.Invoke(address);
        }

        private Cap OnAcceptCap(out OmniAddress address)
        {
            address = null;
            return this.AcceptCapEvent?.Invoke(out address);
        }

        private void UpdateBaseId()
        {
            var baseId = new byte[32];

            using (var random = RandomNumberGenerator.Create())
            {
                random.GetBytes(baseId);
            }

            _baseId = baseId;
        }

        private VolatileHashSet<OmniAddress> _connectedAddresses = new VolatileHashSet<OmniAddress>(new TimeSpan(0, 3, 0));

        private void ConnectThread(CancellationToken token)
        {
            try
            {
                var random = new Random();

                for (; ; )
                {
                    if (token.WaitHandle.WaitOne(1000)) return;

                    // 接続数を制限する。
                    {
                        int connectionCount = _connections.ToArray().Count(n => n.Key.Type == OmniSecureConnectionType.Connect);
                        if (connectionCount >= (ConnectionCountUpperLimit / 2)) continue;
                    }
                    OmniAddress targetAddress = null;

                    lock (_connectedAddresses.LockObject)
                    {
                        _connectedAddresses.Update();

                        switch (random.Next(0, 2))
                        {
                            case 0:
                                targetAddress = _cloudNodeAdressess.Randomize()
                                    .Where(n => !_connectedAddresses.Contains(n))
                                    .FirstOrDefault();
                                break;
                            case 1:
                                var sessionInfo = _connections.Randomize().Select(n => n.Value).FirstOrDefault();
                                if (sessionInfo == null) break;

                                targetAddress = sessionInfo.Receive.Queues.NodeAddressesSet.Randomize()
                                    .Where(n => !_connectedAddresses.Contains(n))
                                    .FirstOrDefault();
                                break;
                        }

                        if (targetAddress == null || _myNodeAddresses.Contains(targetAddress)
                            || _connections.Values.Any(n => n.NodeAddress == targetAddress)) continue;

                        _connectedAddresses.Add(targetAddress);
                    }

                    var cap = this.OnConnectCap(targetAddress);

                    if (cap == null)
                    {
                        lock (_cloudNodeAdressess.LockObject)
                        {
                            if (_cloudNodeAdressess.Count > 1024)
                            {
                                _cloudNodeAdressess.Remove(targetAddress);
                            }
                        }

                        continue;
                    }

                    _networkStatus.ConnectCount.Increment();

                    this.CreateConnection(cap, OmniSecureConnectionType.Connect, targetAddress);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private void AcceptThread(CancellationToken token)
        {
            try
            {
                for (; ; )
                {
                    if (token.WaitHandle.WaitOne(1000)) return;

                    // 接続数を制限する。
                    {
                        int connectionCount = _connections.ToArray().Count(n => n.Key.Type == OmniSecureConnectionType.Accept);
                        if (connectionCount >= (ConnectionCountUpperLimit / 2)) continue;
                    }

                    var cap = this.OnAcceptCap(out var targetAddress);
                    if (cap == null) continue;

                    _networkStatus.AcceptCount.Increment();

                    this.CreateConnection(cap, OmniSecureConnectionType.Accept, targetAddress);
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
        }

        private OmniNonblockingConnectionOptions CreateOmniNonblockingConnectionOptions()
        {
            return new OmniNonblockingConnectionOptions()
            {
                MaxSendByteCount = 1024 * 1024 * 4,
                MaxReceiveByteCount = 1024 * 1024 * 4,
                BandwidthController = _bandwidthController,
                BufferPool = _bufferPool,
            };
        }

        private OmniSecureConnectionOptions CreateOmniSecureConnectionOptions(OmniSecureConnectionType type)
        {
            return new OmniSecureConnectionOptions()
            {
                Passwords = _passwords,
                Type = type,
                BufferPool = _bufferPool,
            };
        }

        private void CreateConnection(Cap cap, OmniSecureConnectionType type, OmniAddress targetAddress)
        {
            OmniSecureConnection connection = null;

            {
                var garbages = new List<IDisposable>();

                try
                {

                    var nonblockingConnection = new OmniNonblockingConnection(cap, this.CreateOmniNonblockingConnectionOptions());
                    garbages.Add(nonblockingConnection);

                    var secureConnection = new OmniSecureConnection(nonblockingConnection, this.CreateOmniSecureConnectionOptions(type));
                    garbages.Add(secureConnection);

                    using (var tokenSource = new CancellationTokenSource(new TimeSpan(0, 0, 30)))
                    {
                        var task = secureConnection.Handshake(tokenSource.Token);

                        while (!task.IsCompleted)
                        {
                            Thread.Sleep(300);
                            secureConnection.DoEvents();
                        }
                    }

                    connection = secureConnection;
                }
                catch (Exception)
                {
                    foreach (var item in garbages)
                    {
                        item.Dispose();
                    }
                }
            }

            if (connection == null) return;

            lock (_connections.LockObject)
            {
                if (_connections.Count >= ConnectionCountUpperLimit) return;

                var sessionInfo = new SessionStatus();
                sessionInfo.NodeAddress = targetAddress;
                sessionInfo.ThreadId = GetThreadId();

                _connections.Add(connection, sessionInfo);
            }

            int GetThreadId()
            {
                var dic = new Dictionary<int, int>();

                lock (_connections.LockObject)
                {
                    foreach (int i in Enumerable.Range(0, _threadCount))
                    {
                        dic.Add(i, _connections.Values.Count(n => n.ThreadId == i));
                    }
                }

                var sortedList = dic.Randomize().ToList();
                sortedList.Sort((x, y) => x.Value.CompareTo(y.Value));

                return sortedList.First().Key;
            }
        }

        private void RemoveConnection(OmniSecureConnection connection)
        {
            using (_connectionLockProvider.WriteLock())
            {
                lock (_connections.LockObject)
                {
                    if (_connections.TryGetValue(connection, out var sessionInfo))
                    {
                        _connections.Remove(connection);

                        connection.Dispose();

                        _cloudNodeAdressess.Add(sessionInfo.NodeAddress);
                    }
                }
            }
        }

        private void DoEventThread(CancellationToken token)
        {
            try
            {
                for (; ; )
                {
                    // Wait
                    if (token.WaitHandle.WaitOne(300)) return;

                    // 
                    foreach (var (connection, sessionInfo) in _connections.Randomize())
                    {
                        try
                        {
                            using (_connectionLockProvider.ReadLock())
                            {
                                connection.DoEvents();
                            }
                        }
                        catch (Exception e)
                        {
                            _logger.Debug(e);

                            this.RemoveConnection(connection);
                        }
                    }
                }
            }
            catch (Exception e)
            {
                _logger.Debug(e);
            }
        }

        private void ComputeThread(CancellationToken token)
        {
            var random = new Random();

            var refreshStopwatch = Stopwatch.StartNew();
            var reduceStopwatch = Stopwatch.StartNew();

            var pushBlockUploadStopwatch = Stopwatch.StartNew();
            var pushBlockDownloadStopwatch = Stopwatch.StartNew();

            var pushMetadataUploadStopwatch = Stopwatch.StartNew();
            var pushMetadataDownloadStopwatch = Stopwatch.StartNew();

            try
            {
                for (; ; )
                {
                    if (token.WaitHandle.WaitOne(1000)) return;

                    if (refreshStopwatch.Elapsed.TotalSeconds >= 30)
                    {
                        refreshStopwatch.Restart();

                        // 不要なMetadataを削除する。
                        {
                            _metadataManager.Refresh();
                        }

                        // 古いPushリクエスト情報を削除する。
                        {
                            _wantBroadcastClueSet.Update();
                            _wantUnicastClueSet.Update();
                            _wantMulticastClueSet.Update();
                            _wantBlocksRequestSet.Update();
                        }

                        // 古いセッション情報を破棄する。
                        foreach (var SessionStatus in _connections.Values)
                        {
                            SessionStatus.Update();
                        }

                        // 長い間通信の無い接続を切断する。
                        foreach (var (connection, SessionStatus) in _connections.ToArray())
                        {
                            if (SessionStatus.Receive.Stopwatches.ReceiveBlockStopwatch.Elapsed.TotalMinutes < 3) continue;

                            this.RemoveConnection(connection);
                        }
                    }

                    if (reduceStopwatch.Elapsed.TotalMinutes >= 3)
                    {
                        reduceStopwatch.Restart();

                        // 優先度の低い通信を切断する。
                        {
                            var now = DateTime.UtcNow;

                            var tempList = _connections.ToArray().Where(n => (now - n.Value.CreationTime).TotalMinutes > 30).ToList();
                            random.Shuffle(tempList);
                            tempList.Sort((x, y) => x.Value.Priority.GetValue().CompareTo(y.Value.Priority.GetValue()));

                            foreach (var (connection, SessionStatus) in tempList.Take(3))
                            {
                                this.RemoveConnection(connection);
                            }
                        }

                        // 拡散アップロードするブロック数が多すぎる場合、maxCount以下まで削除する。
                        {
                            const int maxCount = 1024 * 256;

                            if (_cloudDiffuseBlockInfos.Count > maxCount)
                            {
                                var targetHashes = _cloudDiffuseBlockInfos.Randomize().Take(_cloudDiffuseBlockInfos.Count - maxCount).ToArray();
                                _cloudDiffuseBlockInfos.ExceptWith(targetHashes);
                            }
                        }

                        // キャッシュに存在しないブロックのアップロード情報を削除する。
                        {
                            {
                                var targetHashes = new HashSet<OmniHash>(_contentsStorage.ExceptFrom(_myDiffuseBlockInfos.Select(n => n.Hash).ToArray()));

                                foreach (var info in _myDiffuseBlockInfos)
                                {
                                    _myDiffuseBlockInfos.Remove(info);
                                }
                            }

                            {
                                var targetHashes = new HashSet<OmniHash>(_contentsStorage.ExceptFrom(_cloudDiffuseBlockInfos.Select(n => n.Hash).ToArray()));

                                foreach (var info in _cloudDiffuseBlockInfos)
                                {
                                    _cloudDiffuseBlockInfos.Remove(info);
                                }
                            }
                        }
                    }

                    var cloudNodes = new List<NodeInfo<SessionStatus>>();
                    {
                        foreach (var SessionStatus in _connections.Values.ToArray())
                        {
                            if (SessionStatus.Id == null) continue;

                            cloudNodes.Add(new NodeInfo<SessionStatus>(SessionStatus.Id, SessionStatus));
                        }

                        if (cloudNodes.Count < 3) continue;
                    }

                    // アップロード
                    if (pushBlockUploadStopwatch.Elapsed.TotalSeconds >= 5)
                    {
                        pushBlockUploadStopwatch.Restart();

                        var diffusionMap = new Dictionary<NodeInfo<SessionStatus>, List<Hash>>();
                        var uploadMap = new Dictionary<NodeInfo<SessionStatus>, List<Hash>>();

                        foreach (var hash in CollectionUtils.Unite(_diffusionBlockHashes.ToArray(), _uploadBlockHashes.ToArray()).Randomize())
                        {
                            foreach (var node in RouteTableMethods.Search(_baseId, hash.Value, cloudNodes, 2))
                            {
                                var tempList = diffusionMap.GetOrAdd(node, (_) => new List<Hash>());
                                if (tempList.Count > 128) continue;

                                tempList.Add(hash);
                            }
                        }

                        foreach (var node in cloudNodes)
                        {
                            uploadMap.GetOrAdd(node, (_) => new List<Hash>())
                                .AddRange(_contentsStorage.IntersectFrom(node.Value.Receive.PulledBlockRequestSet.Randomize()).Take(256));
                        }

                        foreach (var node in cloudNodes)
                        {
                            var tempList = new List<Hash>();
                            {
                                if (diffusionMap.TryGetValue(node, out var diffusionList))
                                {
                                    tempList.AddRange(diffusionList);
                                }

                                if (uploadMap.TryGetValue(node, out var uploadList))
                                {
                                    tempList.AddRange(uploadList);
                                }

                                random.Shuffle(tempList);
                            }

                            lock (node.Value.Send.PushBlockResultQueue.LockObject)
                            {
                                node.Value.Send.PushBlockResultQueue.Clear();

                                foreach (var item in tempList)
                                {
                                    node.Value.Send.PushBlockResultQueue.Enqueue(item);
                                }
                            }
                        }
                    }

                    // ダウンロード
                    if (pushBlockDownloadStopwatch.Elapsed.TotalSeconds >= 30)
                    {
                        pushBlockDownloadStopwatch.Restart();

                        var pushBlockLinkSet = new HashSet<Hash>();
                        var pushBlockRequestSet = new HashSet<Hash>();

                        {
                            // Link
                            {
                                {
                                    var tempList = _contentsStorage.ToArray();
                                    random.Shuffle(tempList);

                                    pushBlockLinkSet.UnionWith(tempList.Take(_maxBlockLinkCount * cloudNodes.Count));
                                }

                                {
                                    foreach (var node in cloudNodes)
                                    {
                                        var tempList = node.Value.Receive.PulledBlockLinkSet.ToArray();
                                        random.Shuffle(tempList);

                                        var count = Math.Max(16, _maxBlockLinkCount * node.Value.Priority.GetValue());
                                        pushBlockLinkSet.UnionWith(tempList.Take(count));
                                    }
                                }
                            }

                            // Request
                            {
                                {
                                    var tempList = _contentsStorage.ExceptFrom(_pushBlocksRequestSet.ToArray()).ToArray();
                                    random.Shuffle(tempList);

                                    pushBlockRequestSet.UnionWith(tempList.Take(_maxBlockRequestCount * cloudNodes.Count));
                                }

                                {
                                    foreach (var node in cloudNodes)
                                    {
                                        var tempList = _contentsStorage.ExceptFrom(node.Value.Receive.PulledBlockRequestSet.ToArray()).ToArray();
                                        random.Shuffle(tempList);

                                        var count = Math.Max(16, _maxBlockRequestCount * node.Value.Priority.GetValue());
                                        pushBlockRequestSet.UnionWith(tempList.Take(count));
                                    }
                                }
                            }
                        }

                        {
                            // Link
                            {
                                var tempMap = new Dictionary<NodeInfo<SessionStatus>, List<Hash>>();

                                foreach (var hash in pushBlockLinkSet.Randomize())
                                {
                                    foreach (var node in RouteTableMethods.Search(_baseId, hash.Value, cloudNodes, 16))
                                    {
                                        if (node.Value.Send.PushedBlockLinkFilter.Contains(hash)) continue;

                                        tempMap.GetOrAdd(node, (_) => new List<Hash>()).Add(hash);

                                        break;
                                    }
                                }

                                foreach (var (node, targets) in tempMap)
                                {
                                    random.Shuffle(targets);

                                    lock (node.Value.Send.PushBlockLinkQueue.LockObject)
                                    {
                                        node.Value.Send.PushBlockLinkQueue.Clear();

                                        foreach (var hash in targets.Take(_maxBlockLinkCount))
                                        {
                                            node.Value.Send.PushBlockLinkQueue.Enqueue(hash);
                                        }
                                    }
                                }
                            }

                            // Request
                            {
                                var tempMap = new Dictionary<NodeInfo<SessionStatus>, List<Hash>>();

                                foreach (var hash in pushBlockRequestSet.Randomize())
                                {
                                    foreach (var node in RouteTableMethods.Search(_baseId, hash.Value, cloudNodes, 16))
                                    {
                                        if (node.Value.Send.PushedBlockRequestFilter.Contains(hash)) continue;

                                        tempMap.GetOrAdd(node, (_) => new List<Hash>()).Add(hash);

                                        break;
                                    }

                                    foreach (var node in cloudNodes)
                                    {
                                        if (node.Value.Send.PushedBlockRequestFilter.Contains(hash)) continue;
                                        if (!node.Value.Receive.PulledBlockLinkFilter.Contains(hash)) continue;

                                        tempMap.GetOrAdd(node, (_) => new List<Hash>()).Add(hash);

                                        break;
                                    }
                                }

                                foreach (var (node, targets) in tempMap)
                                {
                                    random.Shuffle(targets);

                                    lock (node.Value.Send.PushBlockRequestQueue.LockObject)
                                    {
                                        node.Value.Send.PushBlockRequestQueue.Clear();

                                        foreach (var hash in targets.Take(_maxBlockRequestCount))
                                        {
                                            node.Value.Send.PushBlockRequestQueue.Enqueue(hash);
                                        }
                                    }
                                }
                            }
                        }
                    }

                    // アップロード
                    if (pushMetadataUploadStopwatch.Elapsed.TotalSeconds >= 30)
                    {
                        pushMetadataUploadStopwatch.Restart();

                        // BroadcastMetadata
                        foreach (var signature in _metadataManager.GetBroadcastSignatures())
                        {
                            foreach (var node in RouteTableMethods.Search(_baseId, signature.Id, cloudNodes, 1))
                            {
                                node.Value.Receive.PulledBroadcastMetadataRequestSet.Add(signature);
                            }
                        }

                        // UnicastMetadata
                        foreach (var signature in _metadataManager.GetUnicastSignatures())
                        {
                            foreach (var node in RouteTableMethods.Search(_baseId, signature.Id, cloudNodes, 1))
                            {
                                node.Value.Receive.PulledUnicastMetadataRequestSet.Add(signature);
                            }
                        }

                        // MulticastMetadata
                        foreach (var tag in _metadataManager.GetMulticastTags())
                        {
                            foreach (var node in RouteTableMethods.Search(_baseId, tag.Id, cloudNodes, 1))
                            {
                                node.Value.Receive.PulledMulticastMetadataRequestSet.Add(tag);
                            }
                        }
                    }

                    // ダウンロード
                    if (pushMetadataDownloadStopwatch.Elapsed.TotalSeconds >= 30)
                    {
                        pushMetadataDownloadStopwatch.Restart();

                        var pushBroadcastMetadatasRequestSet = new HashSet<Signature>();
                        var pushUnicastMetadatasRequestSet = new HashSet<Signature>();
                        var pushMulticastMetadatasRequestSet = new HashSet<Tag>();

                        {
                            // BroadcastMetadata
                            {
                                {
                                    var list = _pushBroadcastMetadatasRequestSet.ToArray();
                                    random.Shuffle(list);

                                    pushBroadcastMetadatasRequestSet.UnionWith(list.Take(_maxMetadataRequestCount));
                                }

                                foreach (var node in cloudNodes)
                                {
                                    var list = node.Value.Receive.PulledBroadcastMetadataRequestSet.ToArray();
                                    random.Shuffle(list);

                                    pushBroadcastMetadatasRequestSet.UnionWith(list.Take(_maxMetadataRequestCount));
                                }
                            }

                            // UnicastMetadata
                            {
                                {
                                    var list = _pushUnicastMetadatasRequestSet.ToArray();
                                    random.Shuffle(list);

                                    pushUnicastMetadatasRequestSet.UnionWith(list.Take(_maxMetadataRequestCount));
                                }

                                foreach (var node in cloudNodes)
                                {
                                    var list = node.Value.Receive.PulledUnicastMetadataRequestSet.ToArray();
                                    random.Shuffle(list);

                                    pushUnicastMetadatasRequestSet.UnionWith(list.Take(_maxMetadataRequestCount));
                                }
                            }

                            // MulticastMetadata
                            {
                                {
                                    var list = _pushMulticastMetadatasRequestSet.ToArray();
                                    random.Shuffle(list);

                                    pushMulticastMetadatasRequestSet.UnionWith(list.Take(_maxMetadataRequestCount));
                                }

                                foreach (var node in cloudNodes)
                                {
                                    var list = node.Value.Receive.PulledMulticastMetadataRequestSet.ToArray();
                                    random.Shuffle(list);

                                    pushMulticastMetadatasRequestSet.UnionWith(list.Take(_maxMetadataRequestCount));
                                }
                            }
                        }

                        {
                            // BroadcastMetadata
                            {
                                var tempMap = new Dictionary<NodeInfo<SessionStatus>, List<Signature>>();

                                foreach (var signature in pushBroadcastMetadatasRequestSet.Randomize())
                                {
                                    foreach (var node in RouteTableMethods.Search(_baseId, signature.Id, cloudNodes, 3))
                                    {
                                        tempMap.GetOrAdd(node, (_) => new List<Signature>()).Add(signature);
                                    }
                                }

                                foreach (var (node, targets) in tempMap)
                                {
                                    random.Shuffle(targets);

                                    lock (node.Value.Send.PushBroadcastMetadataRequestQueue.LockObject)
                                    {
                                        node.Value.Send.PushBroadcastMetadataRequestQueue.Clear();

                                        foreach (var signature in targets.Take(_maxMetadataRequestCount))
                                        {
                                            node.Value.Send.PushBroadcastMetadataRequestQueue.Enqueue(signature);
                                        }
                                    }
                                }
                            }

                            // UnicastMetadata
                            {
                                var tempMap = new Dictionary<NodeInfo<SessionStatus>, List<Signature>>();

                                foreach (var signature in pushUnicastMetadatasRequestSet.Randomize())
                                {
                                    foreach (var node in RouteTableMethods.Search(_baseId, signature.Id, cloudNodes, 3))
                                    {
                                        tempMap.GetOrAdd(node, (_) => new List<Signature>()).Add(signature);
                                    }
                                }

                                foreach (var (node, targets) in tempMap)
                                {
                                    random.Shuffle(targets);

                                    lock (node.Value.Send.PushUnicastMetadataRequestQueue.LockObject)
                                    {
                                        node.Value.Send.PushUnicastMetadataRequestQueue.Clear();

                                        foreach (var signature in targets.Take(_maxMetadataRequestCount))
                                        {
                                            node.Value.Send.PushUnicastMetadataRequestQueue.Enqueue(signature);
                                        }
                                    }
                                }
                            }

                            // MulticastMetadata
                            {
                                var tempMap = new Dictionary<NodeInfo<SessionStatus>, List<Tag>>();

                                foreach (var tag in pushMulticastMetadatasRequestSet.Randomize())
                                {
                                    foreach (var node in RouteTableMethods.Search(_baseId, tag.Id, cloudNodes, 3))
                                    {
                                        tempMap.GetOrAdd(node, (_) => new List<Tag>()).Add(tag);
                                    }
                                }

                                foreach (var (node, targets) in tempMap)
                                {
                                    random.Shuffle(targets);

                                    lock (node.Value.Send.PushMulticastMetadataRequestQueue.LockObject)
                                    {
                                        node.Value.Send.PushMulticastMetadataRequestQueue.Clear();

                                        foreach (var tag in targets.Take(_maxMetadataRequestCount))
                                        {
                                            node.Value.Send.PushMulticastMetadataRequestQueue.Enqueue(tag);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void EnqueueThread(int threadId, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                for (; ; )
                {
                    if (sw.ElapsedMilliseconds > 1000) Debug.WriteLine("Relay EnqueueThread: " + sw.ElapsedMilliseconds);

                    // Timer
                    {
                        if (token.WaitHandle.WaitOne((int)(Math.Max(0, 1000 - sw.ElapsedMilliseconds)))) return;
                        sw.Restart();
                    }

                    // Enqueue
                    {
                        foreach (var (connection, SessionStatus) in _connections.Where(n => n.Value.ThreadId == threadId).Randomize())
                        {
                            try
                            {
                                using (_connectionLockProvider.ReadLock())
                                {
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (!connection.TryEnqueue(() => this.GetSendStream(SessionStatus))) break;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug(e);

                                using (_connectionLockProvider.WriteLock())
                                {
                                    this.RemoveConnection(connection);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private void DequeueThread(int threadId, CancellationToken token)
        {
            var sw = Stopwatch.StartNew();

            try
            {
                for (; ; )
                {
                    if (sw.ElapsedMilliseconds > 1000) Debug.WriteLine("Relay DequeueThread: " + sw.ElapsedMilliseconds);

                    // Timer
                    {
                        if (token.WaitHandle.WaitOne((int)(Math.Max(0, 1000 - sw.ElapsedMilliseconds)))) return;
                        sw.Restart();
                    }

                    // Dequeue
                    {
                        foreach (var (connection, SessionStatus) in _connections.Where(n => n.Value.ThreadId == threadId).Randomize())
                        {
                            try
                            {
                                using (_connectionLockProvider.ReadLock())
                                {
                                    for (int i = 0; i < 3; i++)
                                    {
                                        if (!connection.TryDequeue((stream) => this.SetReceiveStream(SessionStatus, stream))) break;
                                    }
                                }
                            }
                            catch (Exception e)
                            {
                                Log.Debug(e);

                                using (_connectionLockProvider.WriteLock())
                                {
                                    this.RemoveConnection(connection);
                                }
                            }
                        }
                    }
                }
            }
            catch (Exception e)
            {
                Log.Error(e);
            }
        }

        private Stream GetSendStream(SessionStatus SessionStatus)
        {
            try
            {
                if (!SessionStatus.Send.IsSentVersion)
                {
                    SessionStatus.Send.IsSentVersion = true;

                    var versionStream = new RecyclableMemoryStream(_bufferPool);
                    Varint.SetUInt64(versionStream, (uint)ProtocolVersion.Version0);
                    versionStream.Seek(0, SeekOrigin.Begin);

                    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send Version");

                    return versionStream;
                }
                else if (!SessionStatus.Send.IsSentProfile)
                {
                    SessionStatus.Send.IsSentProfile = true;

                    var packet = new ProfilePacket(_baseId, _config.MyLocation);

                    var dataStream = packet.Export(_bufferPool);

                    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send Profile");

                    return dataStream;
                }
                else
                {
                    if (SessionStatus.Send.LocationResultStopwatch.Elapsed.TotalMinutes > 3)
                    {
                        SessionStatus.Send.LocationResultStopwatch.Restart();

                        var random = RandomProvider.GetThreadRandom();

                        var tempLocations = new List<Location>();
                        tempLocations.Add(_config.MyLocation);
                        tempLocations.AddRange(_cloudNodeAdressess);

                        random.Shuffle(tempLocations);

                        var packet = new LocationsPublishPacket(tempLocations.Take(_maxLocationCount).ToArray());

                        Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                        Varint.SetUInt64(typeStream, (uint)MessageId.LocationsPublish);

                        _networkStatus.PushLocationCount.Add(packet.Locations.Count());

                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send LocationResult");

                        return new UniteStream(typeStream, packet.Export(_bufferPool));
                    }
                    else if (SessionStatus.Send.PushBlockLinkQueue.Count > 0)
                    {
                        BlocksLinkPacket packet;

                        lock (SessionStatus.Send.PushBlockLinkQueue.LockObject)
                        {
                            SessionStatus.Send.PushedBlockLinkFilter.AddRange(SessionStatus.Send.PushBlockLinkQueue);

                            packet = new BlocksLinkPacket(SessionStatus.Send.PushBlockLinkQueue.ToArray());
                            SessionStatus.Send.PushBlockLinkQueue.Clear();
                        }

                        Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                        Varint.SetUInt64(typeStream, (uint)MessageId.BlocksLink);

                        _networkStatus.PushBlockLinkCount.Add(packet.Hashes.Count());

                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send BlockLink");

                        return new UniteStream(typeStream, packet.Export(_bufferPool));
                    }
                    else if (SessionStatus.Send.PushBlockRequestQueue.Count > 0)
                    {
                        BlocksRequestPacket packet;

                        lock (SessionStatus.Send.PushBlockRequestQueue.LockObject)
                        {
                            SessionStatus.Send.PushedBlockRequestFilter.AddRange(SessionStatus.Send.PushBlockRequestQueue);

                            packet = new BlocksRequestPacket(SessionStatus.Send.PushBlockRequestQueue.ToArray());
                            SessionStatus.Send.PushBlockRequestQueue.Clear();
                        }

                        Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                        Varint.SetUInt64(typeStream, (uint)MessageId.BlocksRequest);

                        _networkStatus.PushBlockRequestCount.Add(packet.Hashes.Count());

                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send BlockRequest");

                        return new UniteStream(typeStream, packet.Export(_bufferPool));
                    }
                    else if (SessionStatus.Send.BlockResultStopwatch.Elapsed.TotalSeconds > 0.5 && SessionStatus.Send.PushBlockResultQueue.Count > 0)
                    {
                        SessionStatus.Send.BlockResultStopwatch.Restart();

                        Hash? hash = null;

                        lock (SessionStatus.Send.PushBlockResultQueue.LockObject)
                        {
                            if (SessionStatus.Send.PushBlockResultQueue.Count > 0)
                            {
                                hash = SessionStatus.Send.PushBlockResultQueue.Dequeue();
                                SessionStatus.Receive.PulledBlockRequestSet.Remove(hash.Value);
                            }
                        }

                        if (hash != null)
                        {
                            Stream dataStream = null;
                            {
                                var buffer = new ArraySegment<byte>();

                                try
                                {
                                    buffer = _contentsStorage.GetBlock(hash.Value);

                                    dataStream = (new BlockResultPacket(hash.Value, buffer)).Export(_bufferPool);
                                }
                                catch (Exception)
                                {

                                }
                                finally
                                {
                                    if (buffer.Array != null)
                                    {
                                        _bufferPool.ReturnBuffer(buffer.Array);
                                    }
                                }
                            }

                            if (dataStream != null)
                            {
                                Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                                Varint.SetUInt64(typeStream, (uint)MessageId.BlockResult);

                                _networkStatus.PushBlockResultCount.Increment();

                                _diffusionBlockHashes.Remove(hash.Value);
                                _uploadBlockHashes.Remove(hash.Value);

                                Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send BlockResult " + NetworkConverter.ToBase64UrlString(hash.Value.Value));

                                return new UniteStream(typeStream, dataStream);
                            }
                        }
                    }
                    else if (SessionStatus.Send.PushBroadcastMetadataRequestQueue.Count > 0)
                    {
                        BroadcastMetadatasRequestPacket packet;

                        lock (SessionStatus.Send.PushBroadcastMetadataRequestQueue.LockObject)
                        {
                            packet = new BroadcastMetadatasRequestPacket(SessionStatus.Send.PushBroadcastMetadataRequestQueue.ToArray());
                            SessionStatus.Send.PushBroadcastMetadataRequestQueue.Clear();
                        }

                        Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                        Varint.SetUInt64(typeStream, (uint)MessageId.BroadcastMetadatasRequest);

                        _networkStatus.PushMessageRequestCount.Add(packet.Signatures.Count());

                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send BroadcastMetadataRequest");

                        return new UniteStream(typeStream, packet.Export(_bufferPool));
                    }
                    else if (SessionStatus.Send.BroadcastMetadataResultStopwatch.Elapsed.TotalSeconds > 30)
                    {
                        SessionStatus.Send.BroadcastMetadataResultStopwatch.Restart();

                        var random = RandomProvider.GetThreadRandom();

                        var broadcastMetadatas = new List<BroadcastMetadata>();

                        var signatures = new List<Signature>();

                        lock (SessionStatus.Receive.PulledBroadcastMetadataRequestSet.LockObject)
                        {
                            signatures.AddRange(SessionStatus.Receive.PulledBroadcastMetadataRequestSet);
                        }

                        random.Shuffle(signatures);

                        foreach (var signature in signatures)
                        {
                            foreach (var metadata in _metadataManager.GetBroadcastMetadatas(signature).Randomize())
                            {
                                broadcastMetadatas.Add(metadata);

                                if (broadcastMetadatas.Count >= _maxMetadataResultCount) goto End;
                            }
                        }

                        End:;

                        if (broadcastMetadatas.Count > 0)
                        {
                            var packet = new BroadcastMetadatasResultPacket(broadcastMetadatas);

                            Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                            Varint.SetUInt64(typeStream, (uint)MessageId.BroadcastMetadatasResult);

                            _networkStatus.PushMessageResultCount.Add(packet.BroadcastMetadatas.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send MetadataResult");

                            return new UniteStream(typeStream, packet.Export(_bufferPool));
                        }
                    }
                    else if (SessionStatus.Send.PushUnicastMetadataRequestQueue.Count > 0)
                    {
                        UnicastMetadatasRequestPacket packet;

                        lock (SessionStatus.Send.PushUnicastMetadataRequestQueue.LockObject)
                        {
                            packet = new UnicastMetadatasRequestPacket(SessionStatus.Send.PushUnicastMetadataRequestQueue.ToArray());
                            SessionStatus.Send.PushUnicastMetadataRequestQueue.Clear();
                        }

                        Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                        Varint.SetUInt64(typeStream, (uint)MessageId.UnicastMetadatasRequest);

                        _networkStatus.PushMessageRequestCount.Add(packet.Signatures.Count());

                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send UnicastMetadataRequest");

                        return new UniteStream(typeStream, packet.Export(_bufferPool));
                    }
                    else if (SessionStatus.Send.UnicastMetadataResultStopwatch.Elapsed.TotalSeconds > 30)
                    {
                        SessionStatus.Send.UnicastMetadataResultStopwatch.Restart();

                        var random = RandomProvider.GetThreadRandom();

                        var UnicastMetadatas = new List<UnicastMetadata>();

                        var signatures = new List<Signature>();

                        lock (SessionStatus.Receive.PulledUnicastMetadataRequestSet.LockObject)
                        {
                            signatures.AddRange(SessionStatus.Receive.PulledUnicastMetadataRequestSet);
                        }

                        random.Shuffle(signatures);

                        foreach (var signature in signatures)
                        {
                            foreach (var metadata in _metadataManager.GetUnicastMetadatas(signature).Randomize())
                            {
                                UnicastMetadatas.Add(metadata);

                                if (UnicastMetadatas.Count >= _maxMetadataResultCount) goto End;
                            }
                        }

                        End:;

                        if (UnicastMetadatas.Count > 0)
                        {
                            var packet = new UnicastMetadatasResultPacket(UnicastMetadatas);

                            Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                            Varint.SetUInt64(typeStream, (uint)MessageId.UnicastMetadatasResult);

                            _networkStatus.PushMessageResultCount.Add(packet.UnicastMetadatas.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send MetadataResult");

                            return new UniteStream(typeStream, packet.Export(_bufferPool));
                        }
                    }
                    else if (SessionStatus.Send.PushMulticastMetadataRequestQueue.Count > 0)
                    {
                        MulticastMetadatasRequestPacket packet;

                        lock (SessionStatus.Send.PushMulticastMetadataRequestQueue.LockObject)
                        {
                            packet = new MulticastMetadatasRequestPacket(SessionStatus.Send.PushMulticastMetadataRequestQueue.ToArray());
                            SessionStatus.Send.PushMulticastMetadataRequestQueue.Clear();
                        }

                        Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                        Varint.SetUInt64(typeStream, (uint)MessageId.MulticastMetadatasRequest);

                        _networkStatus.PushMessageRequestCount.Add(packet.Tags.Count());

                        Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send MulticastMetadataRequest");

                        return new UniteStream(typeStream, packet.Export(_bufferPool));
                    }
                    else if (SessionStatus.Send.MulticastMetadataResultStopwatch.Elapsed.TotalSeconds > 30)
                    {
                        SessionStatus.Send.MulticastMetadataResultStopwatch.Restart();

                        var random = RandomProvider.GetThreadRandom();

                        var MulticastMetadatas = new List<MulticastMetadata>();

                        var tags = new List<Tag>();

                        lock (SessionStatus.Receive.PulledMulticastMetadataRequestSet.LockObject)
                        {
                            tags.AddRange(SessionStatus.Receive.PulledMulticastMetadataRequestSet);
                        }

                        random.Shuffle(tags);

                        foreach (var tag in tags)
                        {
                            foreach (var metadata in _metadataManager.GetMulticastMetadatas(tag).Randomize())
                            {
                                MulticastMetadatas.Add(metadata);

                                if (MulticastMetadatas.Count >= _maxMetadataResultCount) goto End;
                            }
                        }

                        End:;

                        if (MulticastMetadatas.Count > 0)
                        {
                            var packet = new MulticastMetadatasResultPacket(MulticastMetadatas);

                            Stream typeStream = new RecyclableMemoryStream(_bufferPool);
                            Varint.SetUInt64(typeStream, (uint)MessageId.MulticastMetadatasResult);

                            _networkStatus.PushMessageResultCount.Add(packet.MulticastMetadatas.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Send MetadataResult");

                            return new UniteStream(typeStream, packet.Export(_bufferPool));
                        }
                    }
                }
            }
            catch (SendException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }

            return null;
        }

        private void SetReceiveStream(SessionStatus SessionStatus, Stream stream)
        {
            try
            {
                SessionStatus.Receive.Stopwatch.Restart();

                if (!SessionStatus.Receive.IsReceivedVersion)
                {
                    var targetVersion = (ProtocolVersion)Varint.GetUInt64(stream);
                    SessionStatus.Version = (ProtocolVersion)Math.Min((uint)targetVersion, (uint)ProtocolVersion.Version0);

                    SessionStatus.Receive.IsReceivedVersion = true;

                    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive Version");
                }
                else if (!SessionStatus.Receive.IsReceivedProfile)
                {
                    using (var dataStream = new RangeStream(stream))
                    {
                        var profile = ProfilePacket.Import(dataStream, _bufferPool);
                        if (profile.Id == null || profile.Location == null) throw new ReceiveException("ExchangeManager: Broken Profile");

                        if (BytesOperations.SequenceEqual(_baseId, profile.Id)) throw new ReceiveException("ExchangeManager: Circular Connect");

                        lock (_connections.LockObject)
                        {
                            var connectionIds = _connections.Select(n => n.Value.Id).Where(n => n != null).ToArray();
                            if (connectionIds.Any(n => BytesOperations.SequenceEqual(n, profile.Id))) throw new ReceiveException("ExchangeManager: Conflict");

                            var distance = RouteTableMethods.Distance(_baseId, profile.Id);
                            var count = connectionIds.Select(id => RouteTableMethods.Distance(_baseId, id)).Count(n => n == distance);

                            if (count > 128) throw new ReceiveException("ExchangeManager: RouteTable Overflow");
                        }

                        SessionStatus.Id = profile.Id;
                        SessionStatus.Location = profile.Location;
                    }

                    SessionStatus.Receive.IsReceivedProfile = true;

                    Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive Profile");
                }
                else
                {
                    int id = (int)Varint.GetUInt64(stream);

                    using (var dataStream = new RangeStream(stream))
                    {
                        if (id == (int)MessageId.LocationsPublish)
                        {
                            var packet = LocationsPublishPacket.Import(dataStream, _bufferPool);

                            if (SessionStatus.Receive.PulledLocationSet.Count + packet.Locations.Count()
                                > _maxLocationCount * SessionStatus.Receive.PulledLocationSet.SurvivalTime.TotalMinutes / 3) return;

                            SessionStatus.Receive.PulledLocationSet.UnionWith(packet.Locations);

                            _networkStatus.PullLocationCount.Add(packet.Locations.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive LocationResult");
                        }
                        else if (id == (int)MessageId.BlocksLink)
                        {
                            var packet = BlocksLinkPacket.Import(dataStream, _bufferPool);

                            if (SessionStatus.Receive.PulledBlockLinkSet.Count + packet.Hashes.Count()
                                > _maxBlockLinkCount * SessionStatus.Receive.PulledBlockLinkSet.SurvivalTime.TotalMinutes * 2) return;

                            SessionStatus.Receive.PulledBlockLinkSet.UnionWith(packet.Hashes);
                            SessionStatus.Receive.PulledBlockLinkFilter.AddRange(packet.Hashes);

                            _networkStatus.PullBlockLinkCount.Add(packet.Hashes.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive BlocksLink");
                        }
                        else if (id == (int)MessageId.BlocksRequest)
                        {
                            var packet = BlocksRequestPacket.Import(dataStream, _bufferPool);

                            if (SessionStatus.Receive.PulledBlockRequestSet.Count + packet.Hashes.Count()
                                > _maxBlockRequestCount * SessionStatus.Receive.PulledBlockRequestSet.SurvivalTime.TotalMinutes * 2) return;

                            SessionStatus.Receive.PulledBlockRequestSet.UnionWith(packet.Hashes);

                            _networkStatus.PullBlockRequestCount.Add(packet.Hashes.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive BlocksRequest");
                        }
                        else if (id == (int)MessageId.BlockResult)
                        {
                            var packet = BlockResultPacket.Import(dataStream, _bufferPool);

                            _networkStatus.PullBlockResultCount.Increment();

                            try
                            {
                                _contentsStorage.Set(packet.Hash, packet.Value);

                                if (SessionStatus.Send.PushedBlockRequestFilter.Contains(packet.Hash))
                                {
                                    var priority = (int)(SessionStatus.Send.PushedBlockRequestFilter.SurvivalTime.TotalMinutes - SessionStatus.Send.PushedBlockRequestFilter.GetElapsedTime(packet.Hash).TotalMinutes);
                                    SessionStatus.Priority.Add(priority);
                                }
                                else
                                {
                                    _diffusionBlockHashes.Add(packet.Hash);
                                }
                            }
                            finally
                            {
                                if (packet.Value.Array != null)
                                {
                                    _bufferPool.ReturnBuffer(packet.Value.Array);
                                }
                            }

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive BlockResult " + NetworkConverter.ToBase64UrlString(packet.Hash.Value));
                        }
                        else if (id == (int)MessageId.BroadcastMetadatasRequest)
                        {
                            var packet = BroadcastMetadatasRequestPacket.Import(dataStream, _bufferPool);

                            if (SessionStatus.Receive.PulledBroadcastMetadataRequestSet.Count + packet.Signatures.Count()
                                > _maxMetadataRequestCount * SessionStatus.Receive.PulledBroadcastMetadataRequestSet.SurvivalTime.TotalMinutes * 2) return;

                            SessionStatus.Receive.PulledBroadcastMetadataRequestSet.UnionWith(packet.Signatures);

                            _networkStatus.PullMessageRequestCount.Add(packet.Signatures.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive BroadcastMetadatasRequest");
                        }
                        else if (id == (int)MessageId.BroadcastMetadatasResult)
                        {
                            var packet = BroadcastMetadatasResultPacket.Import(dataStream, _bufferPool);

                            if (packet.BroadcastMetadatas.Count() > _maxMetadataResultCount) return;

                            _networkStatus.PullMessageResultCount.Add(packet.BroadcastMetadatas.Count());

                            foreach (var metadata in packet.BroadcastMetadatas)
                            {
                                _metadataManager.SetMetadata(metadata);
                            }

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive BroadcastMetadatasResult");
                        }
                        else if (id == (int)MessageId.UnicastMetadatasRequest)
                        {
                            var packet = UnicastMetadatasRequestPacket.Import(dataStream, _bufferPool);

                            if (SessionStatus.Receive.PulledUnicastMetadataRequestSet.Count + packet.Signatures.Count()
                                > _maxMetadataRequestCount * SessionStatus.Receive.PulledUnicastMetadataRequestSet.SurvivalTime.TotalMinutes * 2) return;

                            SessionStatus.Receive.PulledUnicastMetadataRequestSet.UnionWith(packet.Signatures);

                            _networkStatus.PullMessageRequestCount.Add(packet.Signatures.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive UnicastMetadatasRequest");
                        }
                        else if (id == (int)MessageId.UnicastMetadatasResult)
                        {
                            var packet = UnicastMetadatasResultPacket.Import(dataStream, _bufferPool);

                            if (packet.UnicastMetadatas.Count() > _maxMetadataResultCount) return;

                            _networkStatus.PullMessageResultCount.Add(packet.UnicastMetadatas.Count());

                            foreach (var metadata in packet.UnicastMetadatas)
                            {
                                _metadataManager.SetMetadata(metadata);
                            }

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive UnicastMetadatasResult");
                        }
                        else if (id == (int)MessageId.MulticastMetadatasRequest)
                        {
                            var packet = MulticastMetadatasRequestPacket.Import(dataStream, _bufferPool);

                            if (SessionStatus.Receive.PulledMulticastMetadataRequestSet.Count + packet.Tags.Count()
                                > _maxMetadataRequestCount * SessionStatus.Receive.PulledMulticastMetadataRequestSet.SurvivalTime.TotalMinutes * 2) return;

                            SessionStatus.Receive.PulledMulticastMetadataRequestSet.UnionWith(packet.Tags);

                            _networkStatus.PullMessageRequestCount.Add(packet.Tags.Count());

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive MulticastMetadatasRequest");
                        }
                        else if (id == (int)MessageId.MulticastMetadatasResult)
                        {
                            var packet = MulticastMetadatasResultPacket.Import(dataStream, _bufferPool);

                            if (packet.MulticastMetadatas.Count() > _maxMetadataResultCount) return;

                            _networkStatus.PullMessageResultCount.Add(packet.MulticastMetadatas.Count());

                            foreach (var metadata in packet.MulticastMetadatas)
                            {
                                _metadataManager.SetMetadata(metadata);
                            }

                            Debug.WriteLine(DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss") + " ExchangeManager: Receive MulticastMetadatasResult");
                        }
                    }
                }
            }
            catch (ReceiveException e)
            {
                throw e;
            }
            catch (Exception e)
            {
                Log.Debug(e);
            }
            finally
            {
                stream.Dispose();
                stream = null;
            }
        }

        public void DownloadBlocks(Metadata metadata, IEnumerable<Hash> hashes)
        {
            _pushBlocksRequestSet.UnionWith(hashes);
        }

        public void UploadMetadata(BroadcastMetadata metadata)
        {
            _metadataManager.SetMetadata(metadata);
        }

        public void UploadMetadata(UnicastMetadata metadata)
        {
            _metadataManager.SetMetadata(metadata);
        }

        public void UploadMetadata(MulticastMetadata metadata)
        {
            _metadataManager.SetMetadata(metadata);
        }

        public BroadcastMetadata GetBroadcastMetadata(Signature signature, string type)
        {
            _pushBroadcastMetadatasRequestSet.Add(signature);

            return _metadataManager.GetBroadcastMetadata(signature, type);
        }

        public IEnumerable<UnicastMetadata> GetUnicastMetadatas(Signature signature, string type)
        {
            _pushUnicastMetadatasRequestSet.Add(signature);

            return _metadataManager.GetUnicastMetadatas(signature, type);
        }

        public IEnumerable<MulticastMetadata> GetMulticastMetadatas(Tag tag, string type)
        {
            _pushMulticastMetadatasRequestSet.Add(tag);

            return _metadataManager.GetMulticastMetadatas(tag, type);
        }

        public void DiffuseContent(string path)
        {
            var hashes = _contentsStorage.GetContentHashes(path);
            _uploadBlockHashes.UnionWith(hashes);
        }

        public override ManagerState State
        {
            get
            {
                if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

                return _state;
            }
        }

        private readonly object _stateLockObject = new object();

        public override void Start()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

            lock (_stateLockObject)
            {
                lock (_lockObject)
                {
                    if (this.State == ManagerState.Start) return;
                    _state = ManagerState.Start;

                    _sendTaskManager.Start();
                    _receiveTaskManager.Start();

                    _computeTaskManager.Start();

                    foreach (var taskManager in _enqueueTaskManagers)
                    {
                        taskManager.Start();
                    }

                    foreach (var taskManager in _dequeueTaskManagers)
                    {
                        taskManager.Start();
                    }
                }
            }
        }

        public override void Stop()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

            lock (_stateLockObject)
            {
                lock (_lockObject)
                {
                    if (this.State == ManagerState.Stop) return;
                    _state = ManagerState.Stop;
                }

                _sendTaskManager.Stop();
                _receiveTaskManager.Stop();

                _computeTaskManager.Stop();

                foreach (var taskManager in _enqueueTaskManagers)
                {
                    taskManager.Stop();
                }

                foreach (var taskManager in _dequeueTaskManagers)
                {
                    taskManager.Stop();
                }
            }
        }

        #region ISettings

        public void Load()
        {
            lock (_lockObject)
            {
                int version = _settings.Load("Version", () => 0);

                _config = _settings.Load<RelayExchangeConfig>("Config", () => new RelayExchangeConfig(new Location(Array.Empty<string>()), 128, 1024 * 1024 * 32));

                this.SetCloudLocations(_settings.Load<IEnumerable<Location>>("CloudLocations", () => Array.Empty<Location>()));

                // MetadataManager
                {
                    foreach (var metadata in _settings.Load("BroadcastMetadatas", () => Array.Empty<BroadcastMetadata>()))
                    {
                        _metadataManager.SetMetadata(metadata);
                    }

                    foreach (var metadata in _settings.Load("UnicastMetadatas", () => Array.Empty<UnicastMetadata>()))
                    {
                        _metadataManager.SetMetadata(metadata);
                    }

                    foreach (var metadata in _settings.Load("MulticastMetadatas", () => Array.Empty<MulticastMetadata>()))
                    {
                        _metadataManager.SetMetadata(metadata);
                    }
                }

                _uploadBlockHashes.UnionWith(_settings.Load<IEnumerable<Hash>>("UploadBlockHashes", () => Array.Empty<Hash>()));
                _diffusionBlockHashes.UnionWith(_settings.Load<IEnumerable<Hash>>("DiffusionBlockHashes", () => Array.Empty<Hash>()));
            }
        }

        public void Save()
        {
            lock (_lockObject)
            {
                _settings.Save("Version", 0);

                _settings.Save("Config", _config);

                _settings.Save("CloudLocations", _cloudNodeAdressess);

                // MetadataManager
                {
                    _settings.Save("BroadcastMetadatas", _metadataManager.GetBroadcastMetadatas());
                    _settings.Save("UnicastMetadatas", _metadataManager.GetUnicastMetadatas());
                    _settings.Save("MulticastMetadatas", _metadataManager.GetMulticastMetadatas());
                }

                _settings.Save("UploadBlockHashes", _uploadBlockHashes);
                _settings.Save("DiffusionBlockHashes", _diffusionBlockHashes);
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                foreach (var taskManager in _connectTaskManagers)
                {
                    taskManager.Dispose();
                }
                _connectTaskManagers.Clear();

                foreach (var taskManager in _acceptTaskManagers)
                {
                    taskManager.Dispose();
                }
                _acceptTaskManagers.Clear();

                _sendTaskManager.Dispose();
                _sendTaskManager = null;

                _receiveTaskManager.Dispose();
                _receiveTaskManager = null;

                _computeTaskManager.Dispose();
                _computeTaskManager = null;

                foreach (var taskManager in _enqueueTaskManagers)
                {
                    taskManager.Dispose();
                }
                _enqueueTaskManagers.Clear();

                foreach (var taskManager in _dequeueTaskManagers)
                {
                    taskManager.Dispose();
                }
                _dequeueTaskManagers.Clear();

                if (_connectionLockProvider != null)
                {
                    _connectionLockProvider.Dispose();
                    _connectionLockProvider = null;
                }
            }
        }
    }

    class NetworkManagerException : ManagerException
    {
        public NetworkManagerException() : base() { }
        public NetworkManagerException(string message) : base(message) { }
        public NetworkManagerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
