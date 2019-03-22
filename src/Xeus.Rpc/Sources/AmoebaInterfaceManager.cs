using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Amoeba.Messages;
using Omnix.Base;
using Omnix.Collections;
using Omnix.Io;
using Omnix.Security;
using Omnix.Serialization;

namespace Amoeba.Rpc
{
    public sealed class AmoebaInterfaceManager : StateManagerBase, IService, ISynchronized
    {
        private TcpClient _tcpClient;
        private MessagingManager _messagingManager;
        private BufferManager _bufferManager = BufferManager.Instance;

        private Random _random = new Random();

        private LockedHashDictionary<int, BlockingCollection<ResponseInfo>> _queueMap = new LockedHashDictionary<int, BlockingCollection<ResponseInfo>>();

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public AmoebaInterfaceManager()
        {

        }

        private void Check()
        {
            if (_disposed) throw new ObjectDisposedException(this.GetType().FullName);
        }

        public void Connect(IPEndPoint endpoint, CancellationToken token)
        {
            _tcpClient = new TcpClient();

            for (; ; )
            {
                try
                {
                    _tcpClient.Connect(endpoint);

                    break;
                }
                catch (Exception)
                {

                }

                token.WaitHandle.WaitOne(1000);
            }

            _messagingManager = new MessagingManager(_tcpClient.Client, _bufferManager);
            _messagingManager.ReceiveEvent += _messagingManager_ReceiveEvent;
            _messagingManager.Run();
        }

        private void _messagingManager_ReceiveEvent(Stream responseStream)
        {
            var reader = new MessageStreamReader(new WrapperStream(responseStream), _bufferManager);
            var type = (AmoebaFunctionResponseType)reader.GetUInt64();
            int id = (int)reader.GetUInt64();

            if (_queueMap.TryGetValue(id, out var queue))
            {
                queue.Add(new ResponseInfo() { Type = type, Stream = new RangeStream(responseStream) });
            }
        }

        private void Exit()
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.Exit, (object)null, CancellationToken.None);
            }
        }

        private int CreateId()
        {
            lock (_queueMap.LockObject)
            {
                for (; ; )
                {
                    int id = _random.Next();
                    if (!_queueMap.ContainsKey(id)) return id;
                }
            }
        }

        private (int, BlockingCollection<ResponseInfo>) Send<TArgument>(AmoebaFunctionType type, TArgument argument)
        {
            int id = this.CreateId();
            var queue = new BlockingCollection<ResponseInfo>(new ConcurrentQueue<ResponseInfo>());

            _queueMap.Add(id, queue);

            var messageStream = new RecyclableMemoryStream(_bufferManager);
            var writer = new MessageStreamWriter(messageStream, _bufferManager);

            writer.Write((ulong)type);
            writer.Write((ulong)id);

            Stream valueStream = null;

            if (argument != null)
            {
                try
                {
                    valueStream = new RecyclableMemoryStream(_bufferManager);
                    JsonUtils.Save(valueStream, argument);
                }
                catch (Exception)
                {
                    if (valueStream != null)
                    {
                        valueStream.Dispose();
                        valueStream = null;
                    }

                    throw;
                }
            }

            messageStream.Seek(0, SeekOrigin.Begin);
            _messagingManager.Send(new UniteStream(messageStream, valueStream));

            return (id, queue);
        }

        private void Cancel(int id)
        {
            var messageStream = new RecyclableMemoryStream(_bufferManager);
            var writer = new MessageStreamWriter(messageStream, _bufferManager);

            writer.Write((ulong)AmoebaFunctionType.Cancel);
            writer.Write((ulong)id);

            messageStream.Seek(0, SeekOrigin.Begin);
            _messagingManager.Send(messageStream);
        }

        private TResult Function<TResult, TArgument, TProgress>(AmoebaFunctionType type, TArgument argument, IProgress<TProgress> progress, CancellationToken token)
        {
            var (id, queue) = this.Send(type, argument);

            try
            {
                using (var register = token.Register(() => this.Cancel(id)))
                {
                    for (; ; )
                    {
                        var info = queue.Take();

                        try
                        {
                            if (info.Type == AmoebaFunctionResponseType.Result)
                            {
                                return JsonUtils.Load<TResult>(info.Stream);
                            }
                            else if (info.Type == AmoebaFunctionResponseType.Output)
                            {
                                progress.Report(JsonUtils.Load<TProgress>(info.Stream));
                            }
                            else if (info.Type == AmoebaFunctionResponseType.Cancel)
                            {
                                throw new OperationCanceledException();
                            }
                            else if (info.Type == AmoebaFunctionResponseType.Error)
                            {
                                throw new AmoebaInterfaceManagerException(JsonUtils.Load<AmoebaErrorMessage>(info.Stream));
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                        finally
                        {
                            info.Stream.Dispose();
                        }
                    }
                }
            }
            finally
            {
                queue.Dispose();
                _queueMap.Remove(id);
            }
        }

        private TResult Function<TResult, TArgument>(AmoebaFunctionType type, TArgument argument, CancellationToken token)
        {
            var (id, queue) = this.Send(type, argument);

            try
            {
                using (var register = token.Register(() => this.Cancel(id)))
                {
                    var info = queue.Take();

                    try
                    {
                        if (info.Type == AmoebaFunctionResponseType.Result)
                        {
                            return JsonUtils.Load<TResult>(info.Stream);
                        }
                        else if (info.Type == AmoebaFunctionResponseType.Cancel)
                        {
                            throw new OperationCanceledException();
                        }
                        else if (info.Type == AmoebaFunctionResponseType.Error)
                        {
                            throw new AmoebaInterfaceManagerException(JsonUtils.Load<AmoebaErrorMessage>(info.Stream));
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    finally
                    {
                        info.Stream.Dispose();
                    }
                }
            }
            finally
            {
                queue.Dispose();
                _queueMap.Remove(id);
            }
        }

        private void Action<TArgument, TProgress>(AmoebaFunctionType type, TArgument argument, Action<TProgress> progress, CancellationToken token)
        {
            var (id, queue) = this.Send(type, argument);

            try
            {
                using (var register = token.Register(() => this.Cancel(id)))
                {
                    for (; ; )
                    {
                        var info = queue.Take();

                        try
                        {
                            if (info.Type == AmoebaFunctionResponseType.Result)
                            {
                                return;
                            }
                            else if (info.Type == AmoebaFunctionResponseType.Output)
                            {
                                progress.Invoke(JsonUtils.Load<TProgress>(info.Stream));
                            }
                            else if (info.Type == AmoebaFunctionResponseType.Cancel)
                            {
                                throw new OperationCanceledException();
                            }
                            else if (info.Type == AmoebaFunctionResponseType.Error)
                            {
                                throw new AmoebaInterfaceManagerException(JsonUtils.Load<AmoebaErrorMessage>(info.Stream));
                            }
                            else
                            {
                                throw new NotSupportedException();
                            }
                        }
                        finally
                        {
                            info.Stream.Dispose();
                        }
                    }
                }
            }
            finally
            {
                queue.Dispose();
                _queueMap.Remove(id);
            }
        }

        private void Action<TArgument>(AmoebaFunctionType type, TArgument argument, CancellationToken token)
        {
            var (id, queue) = this.Send(type, argument);

            try
            {
                using (var register = token.Register(() => this.Cancel(id)))
                {
                    var info = queue.Take();

                    try
                    {
                        if (info.Type == AmoebaFunctionResponseType.Result)
                        {
                            return;
                        }
                        else if (info.Type == AmoebaFunctionResponseType.Cancel)
                        {
                            throw new OperationCanceledException();
                        }
                        else if (info.Type == AmoebaFunctionResponseType.Error)
                        {
                            throw new AmoebaInterfaceManagerException(JsonUtils.Load<AmoebaErrorMessage>(info.Stream));
                        }
                        else
                        {
                            throw new NotSupportedException();
                        }
                    }
                    finally
                    {
                        info.Stream.Dispose();
                    }
                }
            }
            finally
            {
                queue.Dispose();
                _queueMap.Remove(id);
            }
        }

        private class ResponseInfo
        {
            public AmoebaFunctionResponseType Type { get; set; }
            public Stream Stream { get; set; }
        }

        public ServiceReport Report
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return this.Function<ServiceReport, object>(AmoebaFunctionType.GetReport, null, CancellationToken.None);
                }
            }
        }

        public IEnumerable<NetworkConnectionReport> GetNetworkConnectionReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return this.Function<NetworkConnectionReport[], object>(AmoebaFunctionType.GetNetworkConnectionReports, null, CancellationToken.None);
            }
        }

        public IEnumerable<CacheContentReport> GetCacheContentReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return this.Function<CacheContentReport[], object>(AmoebaFunctionType.GetCacheContentReports, null, CancellationToken.None);
            }
        }

        public IEnumerable<DownloadContentReport> GetDownloadContentReports()
        {
            this.Check();

            lock (_lockObject)
            {
                return this.Function<DownloadContentReport[], object>(AmoebaFunctionType.GetDownloadContentReports, null, CancellationToken.None);
            }
        }

        public ServiceConfig Config
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return this.Function<ServiceConfig, object>(AmoebaFunctionType.GetConfig, null, CancellationToken.None);
                }
            }
        }

        public void SetConfig(ServiceConfig config)
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.SetConfig, config, CancellationToken.None);
            }
        }

        public void SetCloudLocations(IEnumerable<Location> locations)
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.SetCloudLocations, locations.ToArray(), CancellationToken.None);
            }
        }

        public long Size
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return this.Function<long, object>(AmoebaFunctionType.GetSize, null, CancellationToken.None);
                }
            }
        }

        public void Resize(long size)
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.Resize, size, CancellationToken.None);
            }
        }

        public Task CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    this.Action(AmoebaFunctionType.CheckBlocks, (object)null, progress, token);
                });
            }
        }

        public Task<Metadata> AddContent(string path, DateTime creationTime, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    return this.Function<Metadata, (string, DateTime)>(AmoebaFunctionType.AddContent, (path, creationTime), token);
                });
            }
        }

        public void RemoveContent(string path)
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.RemoveContent, path, CancellationToken.None);
            }
        }

        public void DiffuseContent(string path)
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.Diffusion, path, CancellationToken.None);
            }
        }

        public void AddDownload(Metadata metadata, string path, long maxLength)
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.AddDownload, (metadata, path, maxLength), CancellationToken.None);
            }
        }

        public void RemoveDownload(Metadata metadata, string path)
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.RemoveDownload, (metadata, path), CancellationToken.None);
            }
        }

        public void ResetDownload(Metadata metadata, string path)
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.ResetDownload, (metadata, path), CancellationToken.None);
            }
        }

        public Task<BroadcastProfileMessage> GetProfile(Signature signature, DateTime? creationTimeLowerLimit, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    return this.Function<BroadcastProfileMessage, Signature>(AmoebaFunctionType.GetProfile, signature, token);
                });
            }
        }

        public Task<BroadcastStoreMessage> GetStore(Signature signature, DateTime? creationTimeLowerLimit, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    return this.Function<BroadcastStoreMessage, Signature>(AmoebaFunctionType.GetStore, signature, token);
                });
            }
        }

        public Task<IEnumerable<UnicastCommentMessage>> GetUnicastCommentMessages(Signature signature, AgreementPrivateKey agreementPrivateKey, int messageCountUpperLimit, IEnumerable<MessageCondition> conditions, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    return this.Function<IEnumerable<UnicastCommentMessage>, (Signature, AgreementPrivateKey)>(AmoebaFunctionType.GetUnicastCommentMessages, (signature, agreementPrivateKey), token);
                });
            }
        }

        public Task<IEnumerable<MulticastCommentMessage>> GetMulticastCommentMessages(Tag tag, int trustMessageCountUpperLimit, int untrustMessageCountUpperLimit, IEnumerable<MessageCondition> conditions, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    return this.Function<IEnumerable<MulticastCommentMessage>, Tag>(AmoebaFunctionType.GetMulticastCommentMessages, tag, token);
                });
            }
        }

        public Task SetProfile(ProfileContent profile, DigitalSignature digitalSignature, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    this.Action(AmoebaFunctionType.SetProfile, (profile, digitalSignature), token);
                });
            }
        }

        public Task SetStore(StoreContent store, DigitalSignature digitalSignature, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    this.Action(AmoebaFunctionType.SetStore, (store, digitalSignature), token);
                });
            }
        }

        public Task SetUnicastCommentMessage(Signature targetSignature, CommentContent comment, AgreementPublicKey agreementPublicKey, DigitalSignature digitalSignature, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    this.Action(AmoebaFunctionType.SetUnicastCommentMessage, (targetSignature, comment, agreementPublicKey, digitalSignature), token);
                });
            }
        }

        public Task SetMulticastCommentMessage(Tag tag, CommentContent comment, DigitalSignature digitalSignature, TimeSpan miningTimeSpan, CancellationToken token)
        {
            this.Check();

            lock (_lockObject)
            {
                return Task.Run(() =>
                {
                    this.Action(AmoebaFunctionType.SetMulticastCommentMessage, (tag, comment, digitalSignature, miningTimeSpan), token);
                });
            }
        }

        public override ManagerState State
        {
            get
            {
                this.Check();

                lock (_lockObject)
                {
                    return this.Function<ManagerState, object>(AmoebaFunctionType.GetState, null, CancellationToken.None);
                }
            }
        }

        public override void Start()
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.Start, (object)null, CancellationToken.None);
            }
        }

        public override void Stop()
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.Stop, (object)null, CancellationToken.None);
            }
        }

        public void Load()
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.Load, (object)null, CancellationToken.None);
            }
        }

        public void Save()
        {
            this.Check();

            lock (_lockObject)
            {
                this.Action(AmoebaFunctionType.Save, (object)null, CancellationToken.None);
            }
        }

        protected override void Dispose(bool disposing)
        {
            this.Exit();

            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                _tcpClient.Dispose();
                _messagingManager.Stop();
                _messagingManager.Dispose();
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

    public class AmoebaInterfaceManagerException : StateManagerException
    {
        public AmoebaInterfaceManagerException() : base() { }
        public AmoebaInterfaceManagerException(string message) : base(message) { }
        public AmoebaInterfaceManagerException(string message, Exception innerException) : base(message, innerException) { }
        public AmoebaInterfaceManagerException(AmoebaErrorMessage errorMessage) : base(errorMessage.ToString()) { }
    }
}
