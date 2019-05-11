using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Configuration;
using Omnix.Cryptography;
using Omnix.Network;

namespace Xeus.Core
{
    public delegate IEnumerable<OmniSignature> GetSignaturesEventHandler();
    public delegate Cap ConnectCapEventHandler(OmniAddress address);
    public delegate Cap AcceptCapEventHandler(out OmniAddress address);

    public sealed class ServiceManager : StateManagerBase, IService, ISettings, ISynchronized
    {
        private BufferPool _bufferPool;
        private CoreManager _coreManager;
        private ConnectionManager _connectionManager;
        private MessageManager _messageManager;

        private volatile ManagerState _state = ManagerState.Stop;

        private bool _isLoaded = false;

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public ServiceManager(string configPath, string blocksPath, BufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _coreManager = new CoreManager(Path.Combine(configPath, "Core"), blocksPath, _bufferPool);
            _connectionManager = new ConnectionManager(Path.Combine(configPath, "Connection"), _coreManager, _bufferPool);
            _messageManager = new MessageManager(Path.Combine(configPath, "Message"), _coreManager, _bufferPool);
        }

        private void Check()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);
            if (!_isLoaded) throw new ServiceManagerException("ServiceManager is not loaded.");
        }

        public ServiceReport Report
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return new ServiceReport(_coreManager.Report, _connectionManager.Report);
                }
            }
        }

        public IEnumerable<NetworkConnectionReport> GetNetworkConnectionReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return _coreManager.GetNetworkConnectionReports();
            }
        }

        public IEnumerable<CacheContentReport> GetCacheContentReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return _coreManager.GetCacheContentReports();
            }
        }

        public IEnumerable<DownloadContentReport> GetDownloadContentReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return _coreManager.GetDownloadContentReports();
            }
        }

        public ServiceConfig Config
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return new ServiceConfig(_coreManager.Config, _connectionManager.Config, _messageManager.Config);
                }
            }
        }

        public void SetConfig(ServiceConfig config)
        {
            this.Check();

            lock (_lockObject)
            {
                _coreManager.SetConfig(config.Core);
                _connectionManager.SetConfig(config.Connection);
                _messageManager.SetConfig(config.Message);
            }
        }

        public void SetCloudLocations(IEnumerable<Location> locations)
        {
            this.Check();

            lock (_lockObject)
            {
                _coreManager.SetCloudLocations(locations);
            }
        }

        public long Size
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return _coreManager.Size;
                }
            }
        }

        public void Resize(long size)
        {
            this.Check();

            lock (_lockObject)
            {
                _coreManager.Resize(size).Wait();
            }
        }

        public Task CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _coreManager.CheckBlocks(progress, token);
            }
        }

        public Task<Metadata> AddContent(string path, DateTime creationTime, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _coreManager.Import(path, creationTime, token);
            }
        }

        public void RemoveContent(string path)
        {
            this.Check();

            lock (_lockObject)
            {
                _coreManager.RemoveContent(path);
            }
        }

        public void DiffuseContent(string path)
        {
            this.Check();

            lock (_lockObject)
            {
                _coreManager.DiffuseContent(path);
            }
        }

        public void AddDownload(Metadata metadata, string path, long maxLength)
        {
            this.Check();

            lock (_lockObject)
            {
                _coreManager.AddDownload(metadata, path, maxLength);
            }
        }

        public void RemoveDownload(Metadata metadata, string path)
        {
            this.Check();

            lock (_lockObject)
            {
                _coreManager.RemoveDownload(metadata, path);
            }
        }

        public void ResetDownload(Metadata metadata, string path)
        {
            this.Check();

            lock (_lockObject)
            {
                _coreManager.ResetDownload(metadata, path);
            }
        }

        public Task<BroadcastProfileMessage> GetProfile(Signature signature, DateTime? creationTimeLowerLimit, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _messageManager.GetProfile(signature, creationTimeLowerLimit);
            }
        }

        public Task<BroadcastStoreMessage> GetStore(Signature signature, DateTime? creationTimeLowerLimit, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _messageManager.GetStore(signature, creationTimeLowerLimit);
            }
        }

        public Task<IEnumerable<UnicastCommentMessage>> GetUnicastCommentMessages(Signature signature, AgreementPrivateKey agreementPrivateKey, int messageCountUpperLimit, IEnumerable<MessageCondition> conditions, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _messageManager.GetUnicastCommentMessages(signature, agreementPrivateKey, messageCountUpperLimit, conditions);
            }
        }

        public Task<IEnumerable<MulticastCommentMessage>> GetMulticastCommentMessages(Tag tag, int trustMessageCountUpperLimit, int untrustMessageCountUpperLimit, IEnumerable<MessageCondition> conditions, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _messageManager.GetMulticastCommentMessages(tag, trustMessageCountUpperLimit, untrustMessageCountUpperLimit, conditions);
            }
        }

        public Task SetProfile(ProfileContent profile, DigitalSignature digitalSignature, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _messageManager.Upload(profile, digitalSignature, token);
            }
        }

        public Task SetStore(StoreContent store, DigitalSignature digitalSignature, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _messageManager.Upload(store, digitalSignature, token);
            }
        }

        public Task SetUnicastCommentMessage(Signature targetSignature, CommentContent comment, AgreementPublicKey agreementPublicKey, DigitalSignature digitalSignature, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _messageManager.Upload(targetSignature, comment, agreementPublicKey, digitalSignature, token);
            }
        }

        public Task SetMulticastCommentMessage(Tag tag, CommentContent comment, DigitalSignature digitalSignature, TimeSpan miningTime, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return _messageManager.Upload(tag, comment, digitalSignature, miningTime, token);
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

                _coreManager.Start();
                _connectionManager.Start();
            }
        }

        public override void Stop()
        {
            this.Check();

            lock (_lockObject)
            {
                if (this.State == ManagerState.Stop) return;
                _state = ManagerState.Stop;

                _connectionManager.Stop();
                _coreManager.Stop();
            }
        }

        #region ISettings

        public void Load()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);

            lock (_lockObject)
            {
                if (_isLoaded) throw new ServiceManagerException("ServiceManager was already loaded.");
                _isLoaded = true;

#if DEBUG
                var stopwatch = new Stopwatch();
                stopwatch.Start();
#endif

                {
                    var tasks = new List<Task>();

                    tasks.Add(Task.Run(() => _coreManager.Load()));

                    Task.WaitAll(tasks.ToArray());
                }

                {
                    var tasks = new List<Task>();

                    tasks.Add(Task.Run(() => _connectionManager.Load()));
                    tasks.Add(Task.Run(() => _messageManager.Load()));

                    Task.WaitAll(tasks.ToArray());
                }

#if DEBUG
                stopwatch.Stop();
                Debug.WriteLine("ServiceManager Load: {0}", stopwatch.ElapsedMilliseconds);
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

                    tasks.Add(Task.Run(() => _messageManager.Save()));
                    tasks.Add(Task.Run(() => _connectionManager.Save()));

                    Task.WaitAll(tasks.ToArray());
                }

                {
                    var tasks = new List<Task>();

                    tasks.Add(Task.Run(() => _coreManager.Save()));

                    Task.WaitAll(tasks.ToArray());
                }

#if DEBUG
                stopwatch.Stop();
                Debug.WriteLine("ServiceManager Save: {0}", stopwatch.ElapsedMilliseconds);
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
                _messageManager.Dispose();
                _connectionManager.Dispose();
                _coreManager.Dispose();
            }
        }

        public object LockObject
        {
            get
            {
                return _lockObject;
            }
        }
    }

    class ServiceManagerException : StateManagerException
    {
        public ServiceManagerException() : base() { }
        public ServiceManagerException(string message) : base(message) { }
        public ServiceManagerException(string message, Exception innerException) : base(message, innerException) { }
    }
}
