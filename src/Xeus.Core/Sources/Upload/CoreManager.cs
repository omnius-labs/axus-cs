using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Amoeba.Messages;
using Omnius.Base;
using Omnius.Configuration;
using Omnius.Net;
using Omnius.Security;

namespace Amoeba.Service
{
    sealed class CoreManager : StateManagerBase, ISettings
    {
        private BufferManager _bufferManager;
        private CacheManager _cacheManager;
        private NetworkManager _networkManager;
        private DownloadManager _downloadManager;

        private volatile ManagerState _state = ManagerState.Stop;

        private bool _isLoaded = false;

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public CoreManager(string configPath, string blocksPath, BufferManager bufferManager)
        {
            _bufferManager = bufferManager;
            _cacheManager = new CacheManager(Path.Combine(configPath, "Cache"), blocksPath, _bufferManager);
            _networkManager = new NetworkManager(Path.Combine(configPath, "Network"), _cacheManager, _bufferManager);
            _downloadManager = new DownloadManager(Path.Combine(configPath, "Download"), _networkManager, _cacheManager, _bufferManager);

            _networkManager.ConnectCapEvent = (_, uri) => this.OnConnectCap(uri);
            _networkManager.AcceptCapEvent = (object _, out string uri) => this.OnAcceptCap(out uri);
            _networkManager.GetLockSignaturesEvent = (_) => this.OnGetLockSignatures();
        }

        private void Check()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);
            if (!_isLoaded) throw new CoreManagerException("CoreManager is not loaded.");
        }

        public ConnectCapEventHandler ConnectCapEvent { get; set; }
        public AcceptCapEventHandler AcceptCapEvent { get; set; }

        public GetSignaturesEventHandler GetLockSignaturesEvent { get; set; }

        private Cap OnConnectCap(string uri)
        {
            return this.ConnectCapEvent?.Invoke(this, uri);
        }

        private Cap OnAcceptCap(out string uri)
        {
            uri = null;
            return this.AcceptCapEvent?.Invoke(this, out uri);
        }

        private IEnumerable<Signature> OnGetLockSignatures()
        {
            return this.GetLockSignaturesEvent?.Invoke(this) ?? new Signature[0];
        }

        public CoreReport Report
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return new CoreReport(_cacheManager.Report, _networkManager.Report);
                }
            }
        }

        public IEnumerable<NetworkConnectionReport> GetNetworkConnectionReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return _networkManager.GetNetworkConnectionReports();
            }
        }

        public IEnumerable<CacheContentReport> GetCacheContentReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return _cacheManager.GetCacheContentReports();
            }
        }

        public IEnumerable<DownloadContentReport> GetDownloadContentReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return _downloadManager.GetDownloadContentReports();
            }
        }

        public CoreConfig Config
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return new CoreConfig(_networkManager.Config, _downloadManager.Config);
                }
            }
        }

        public void SetConfig(CoreConfig config)
        {
            this.Check();

            lock (_lockObject)
            {
                _networkManager.SetConfig(config.Network);
                _downloadManager.SetConfig(config.Download);
            }
        }

        public Location MyLocation
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return _networkManager.MyLocation;
                }
            }
        }

        public void SetMyLocation(Location myLocation)
        {
            this.Check();

            lock (_lockObject)
            {
                _networkManager.SetMyLocation(myLocation);
            }
        }

        public IEnumerable<Location> CloudLocations
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return _networkManager.CloudLocations;
                }
            }
        }

        public void SetCloudLocations(IEnumerable<Location> locations)
        {
            this.Check();

            lock (_lockObject)
            {
                _networkManager.SetCloudLocations(locations);
            }
        }

        public long Size
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return _cacheManager.Size;
                }
            }
        }

        public Task Resize(long size)
        {
            this.Check();

            return Task.Run(() =>
            {
                lock (_lockObject)
                {
                    if (this.State == ManagerState.Start)
                    {
                        _downloadManager.Stop();
                    }

                    _cacheManager.Resize(size);

                    if (this.State == ManagerState.Start)
                    {
                        _downloadManager.Start();
                    }
                }
            });
        }

        public Task CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _cacheManager.CheckBlocks(progress, token);
            }
        }

        public Task<Metadata> VolatileSetStream(Stream stream, TimeSpan lifeSpan, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _cacheManager.Import(stream, lifeSpan, token);
            }
        }

        public Stream VolatileGetStream(Metadata metadata, long maxLength)
        {
            this.Check();

            lock (_lockObject)
            {
                return _downloadManager.GetStream(metadata, maxLength);
            }
        }

        public Task<Metadata> Import(string path, DateTime creationTime, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _cacheManager.Import(path, creationTime, token);
            }
        }

        public void RemoveContent(string path)
        {
            this.Check();

            lock (_lockObject)
            {
                _cacheManager.RemoveContent(path);
            }
        }

        public void DiffuseContent(string path)
        {
            this.Check();

            lock (_lockObject)
            {
                _networkManager.Diffuse(path);
            }
        }

        public void AddDownload(Metadata metadata, string path, long maxLength)
        {
            this.Check();

            lock (_lockObject)
            {
                _downloadManager.Add(metadata, path, maxLength);
            }
        }

        public void RemoveDownload(Metadata metadata, string path)
        {
            this.Check();

            lock (_lockObject)
            {
                _downloadManager.Remove(metadata, path);
            }
        }

        public void ResetDownload(Metadata metadata, string path)
        {
            this.Check();

            lock (_lockObject)
            {
                _downloadManager.Reset(metadata, path);
            }
        }

        public void UploadMetadata(BroadcastMetadata metadata)
        {
            this.Check();

            lock (_lockObject)
            {
                _networkManager.Upload(metadata);
            }
        }

        public void UploadMetadata(UnicastMetadata metadata)
        {
            this.Check();

            lock (_lockObject)
            {
                _networkManager.Upload(metadata);
            }
        }

        public void UploadMetadata(MulticastMetadata metadata)
        {
            this.Check();

            lock (_lockObject)
            {
                _networkManager.Upload(metadata);
            }
        }

        public BroadcastMetadata GetBroadcastMetadata(Signature signature, string type)
        {
            this.Check();

            lock (_lockObject)
            {
                return _networkManager.GetBroadcastMetadata(signature, type);
            }
        }

        public IEnumerable<UnicastMetadata> GetUnicastMetadatas(Signature signature, string type)
        {
            this.Check();

            lock (_lockObject)
            {
                return _networkManager.GetUnicastMetadatas(signature, type);
            }
        }

        public IEnumerable<MulticastMetadata> GetMulticastMetadatas(Tag tag, string type)
        {
            this.Check();

            lock (_lockObject)
            {
                return _networkManager.GetMulticastMetadatas(tag, type);
            }
        }

        public override ManagerState State
        {
            get
            {
                this.Check();

                return _state;
            }
        }

        public override void Start()
        {
            this.Check();

            lock (_lockObject)
            {
                if (this.State == ManagerState.Start) return;
                _state = ManagerState.Start;

                _networkManager.Start();
                _downloadManager.Start();
            }
        }

        public override void Stop()
        {
            this.Check();

            lock (_lockObject)
            {
                if (this.State == ManagerState.Stop) return;
                _state = ManagerState.Stop;

                _downloadManager.Stop();
                _networkManager.Stop();
            }
        }

        #region ISettings

        public void Load()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

            lock (_lockObject)
            {
                if (_isLoaded) throw new CoreManagerException("CoreManager was already loaded.");
                _isLoaded = true;

#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif

                {
                    var tasks = new List<Task>();

                    tasks.Add(Task.Run(() => _cacheManager.Load()));
                    tasks.Add(Task.Run(() => _networkManager.Load()));

                    Task.WaitAll(tasks.ToArray());
                }

                {
                    var tasks = new List<Task>();

                    tasks.Add(Task.Run(() => _downloadManager.Load()));

                    Task.WaitAll(tasks.ToArray());
                }

#if DEBUG
                stopwatch.Stop();
                Debug.WriteLine("CoreManager Load: {0}", stopwatch.ElapsedMilliseconds);
#endif
            }
        }

        public void Save()
        {
            this.Check();

            lock (_lockObject)
            {
#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif

                {
                    var tasks = new List<Task>();

                    tasks.Add(Task.Run(() => _downloadManager.Save()));

                    Task.WaitAll(tasks.ToArray());
                }

                {
                    var tasks = new List<Task>();

                    tasks.Add(Task.Run(() => _networkManager.Save()));
                    tasks.Add(Task.Run(() => _cacheManager.Save()));

                    Task.WaitAll(tasks.ToArray());
                }

#if DEBUG
                stopwatch.Stop();
                Debug.WriteLine("CoreManager Save: {0}", stopwatch.ElapsedMilliseconds);
#endif
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                _downloadManager.Dispose();
                _networkManager.Dispose();
                _cacheManager.Dispose();
            }
        }
    }

    class CoreManagerException : StateManagerException
    {
        public CoreManagerException() : base() { }
        public CoreManagerException(string message) : base(message) { }
        public CoreManagerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
