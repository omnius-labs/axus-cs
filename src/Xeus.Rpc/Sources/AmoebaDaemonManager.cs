using System;
using System.IO;
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
    public sealed class AmoebaDaemonManager<TService> : DisposableBase
        where TService : StateManagerBase, IService
    {
        private Socket _socket;
        private TService _serviceManager;
        private BufferManager _bufferManager;
        private MessagingManager _messagingManager;

        private CancellationTokenSource _tokenSource = new CancellationTokenSource();

        private LockedHashDictionary<int, ResponseTask> _tasks = new LockedHashDictionary<int, ResponseTask>();

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public AmoebaDaemonManager(Socket socket, TService serviceManager, BufferManager bufferManager)
        {
            _socket = socket;
            _serviceManager = serviceManager;
            _bufferManager = bufferManager;
            _messagingManager = new MessagingManager(_socket, _bufferManager);
            _messagingManager.ReceiveEvent += this._messagingManager_ReceiveEvent;
        }

        public void Watch()
        {
            _messagingManager.Run();

            _tokenSource.Token.WaitHandle.WaitOne();

            _messagingManager.Stop();

            foreach (var responseTask in _tasks.Values)
            {
                try
                {
                    responseTask.Stop();
                }
                catch (Exception)
                {

                }
            }
        }

        private void _messagingManager_ReceiveEvent(Stream requestStream)
        {
            var reader = new MessageStreamReader(new WrapperStream(requestStream), _bufferManager);
            {
                var type = (AmoebaFunctionType)reader.GetUInt64();
                int id = (int)reader.GetUInt64();

                if (type == AmoebaFunctionType.Exit)
                {
                    SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                    _tokenSource.Cancel();
                }
                else if (type == AmoebaFunctionType.Cancel)
                {
                    if (_tasks.TryGetValue(id, out var responseTask))
                    {
                        responseTask.Stop();
                    }
                }
                else
                {
                    var responseTask = ResponseTask.Create((token) =>
                    {
                        try
                        {
                            switch (type)
                            {
                                case AmoebaFunctionType.GetReport:
                                    {
                                        SendResponse(AmoebaFunctionResponseType.Result, id, _serviceManager.Report);
                                        break;
                                    }
                                case AmoebaFunctionType.GetNetworkConnectionReports:
                                    {
                                        SendResponse(AmoebaFunctionResponseType.Result, id, _serviceManager.GetNetworkConnectionReports());
                                        break;
                                    }
                                case AmoebaFunctionType.GetCacheContentReports:
                                    {
                                        SendResponse(AmoebaFunctionResponseType.Result, id, _serviceManager.GetCacheContentReports());
                                        break;
                                    }
                                case AmoebaFunctionType.GetDownloadContentReports:
                                    {
                                        SendResponse(AmoebaFunctionResponseType.Result, id, _serviceManager.GetDownloadContentReports());
                                        break;
                                    }
                                case AmoebaFunctionType.GetConfig:
                                    {
                                        SendResponse(AmoebaFunctionResponseType.Result, id, _serviceManager.Config);
                                        break;
                                    }
                                case AmoebaFunctionType.SetConfig:
                                    {
                                        var config = JsonUtils.Load<ServiceConfig>(requestStream);
                                        _serviceManager.SetConfig(config);
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.SetCloudLocations:
                                    {
                                        var cloudLocations = JsonUtils.Load<Location[]>(requestStream);
                                        _serviceManager.SetCloudLocations(cloudLocations);
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.GetSize:
                                    {
                                        SendResponse(AmoebaFunctionResponseType.Result, id, _serviceManager.Size);
                                        break;
                                    }
                                case AmoebaFunctionType.Resize:
                                    {
                                        long size = JsonUtils.Load<long>(requestStream);
                                        _serviceManager.Resize(size);
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.CheckBlocks:
                                    {
                                        _serviceManager.CheckBlocks(new Action<CheckBlocksProgressReport>((report) =>
                                        {
                                            SendResponse(AmoebaFunctionResponseType.Output, id, report);
                                        }), token).Wait();

                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.AddContent:
                                    {
                                        var arguments = JsonUtils.Load<(string, DateTime)>(requestStream);
                                        var result = _serviceManager.AddContent(arguments.Item1, arguments.Item2, token).Result;
                                        SendResponse(AmoebaFunctionResponseType.Result, id, result);
                                        break;
                                    }
                                case AmoebaFunctionType.RemoveContent:
                                    {
                                        string path = JsonUtils.Load<string>(requestStream);
                                        _serviceManager.RemoveContent(path);
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.Diffusion:
                                    {
                                        string path = JsonUtils.Load<string>(requestStream);
                                        _serviceManager.DiffuseContent(path);
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.AddDownload:
                                    {
                                        var arguments = JsonUtils.Load<(Metadata, string, long)>(requestStream);
                                        _serviceManager.AddDownload(arguments.Item1, arguments.Item2, arguments.Item3);
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.RemoveDownload:
                                    {
                                        var arguments = JsonUtils.Load<(Metadata, string)>(requestStream);
                                        _serviceManager.RemoveDownload(arguments.Item1, arguments.Item2);
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.ResetDownload:
                                    {
                                        var arguments = JsonUtils.Load<(Metadata, string)>(requestStream);
                                        _serviceManager.ResetDownload(arguments.Item1, arguments.Item2);
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.SetProfile:
                                    {
                                        var arguments = JsonUtils.Load<(ProfileContent, DigitalSignature)>(requestStream);
                                        _serviceManager.SetProfile(arguments.Item1, arguments.Item2, token).Wait();
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.SetStore:
                                    {
                                        var arguments = JsonUtils.Load<(StoreContent, DigitalSignature)>(requestStream);
                                        _serviceManager.SetStore(arguments.Item1, arguments.Item2, token).Wait();
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.SetUnicastCommentMessage:
                                    {
                                        var arguments = JsonUtils.Load<(Signature, CommentContent, AgreementPublicKey, DigitalSignature)>(requestStream);
                                        _serviceManager.SetUnicastCommentMessage(arguments.Item1, arguments.Item2, arguments.Item3, arguments.Item4, token).Wait();
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.SetMulticastCommentMessage:
                                    {
                                        var arguments = JsonUtils.Load<(Tag, CommentContent, DigitalSignature, TimeSpan)>(requestStream);
                                        _serviceManager.SetMulticastCommentMessage(arguments.Item1, arguments.Item2, arguments.Item3, arguments.Item4, token).Wait();
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.GetProfile:
                                    {
                                        var arguments = JsonUtils.Load<(Signature, DateTime?)>(requestStream);
                                        var result = _serviceManager.GetProfile(arguments.Item1, arguments.Item2, token).Result;
                                        SendResponse(AmoebaFunctionResponseType.Result, id, result);
                                        break;
                                    }
                                case AmoebaFunctionType.GetStore:
                                    {
                                        var arguments = JsonUtils.Load<(Signature, DateTime?)>(requestStream);
                                        var result = _serviceManager.GetStore(arguments.Item1, arguments.Item2, token).Result;
                                        SendResponse(AmoebaFunctionResponseType.Result, id, result);
                                        break;
                                    }
                                case AmoebaFunctionType.GetUnicastCommentMessages:
                                    {
                                        var arguments = JsonUtils.Load<(Signature, AgreementPrivateKey, int, MessageCondition[])>(requestStream);
                                        var result = _serviceManager.GetUnicastCommentMessages(arguments.Item1, arguments.Item2, arguments.Item3, arguments.Item4, token).Result;
                                        SendResponse(AmoebaFunctionResponseType.Result, id, result);
                                        break;
                                    }
                                case AmoebaFunctionType.GetMulticastCommentMessages:
                                    {
                                        var arguments = JsonUtils.Load<(Tag, int, int, MessageCondition[])>(requestStream);
                                        var result = _serviceManager.GetMulticastCommentMessages(arguments.Item1, arguments.Item2, arguments.Item3, arguments.Item4, token).Result;
                                        SendResponse(AmoebaFunctionResponseType.Result, id, result);
                                        break;
                                    }
                                case AmoebaFunctionType.GetState:
                                    {
                                        SendResponse(AmoebaFunctionResponseType.Result, id, _serviceManager.State);
                                        break;
                                    }
                                case AmoebaFunctionType.Start:
                                    {
                                        _serviceManager.Start();
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.Stop:
                                    {
                                        _serviceManager.Stop();
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.Load:
                                    {
                                        _serviceManager.Load();
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                                case AmoebaFunctionType.Save:
                                    {
                                        _serviceManager.Save();
                                        SendResponse(AmoebaFunctionResponseType.Result, id, (object)null);
                                        break;
                                    }
                            }
                        }
                        catch (OperationCanceledException)
                        {
                            SendResponse(AmoebaFunctionResponseType.Cancel, id, (object)null);
                        }
                        catch (Exception e)
                        {
                            Log.Error(string.Format("Rpc Error: {0}", type.ToString()));
                            Log.Error(e);

                            var argument = new AmoebaErrorMessage(e.GetType().ToString(), e.Message, e.StackTrace?.ToString());
                            SendResponse(AmoebaFunctionResponseType.Error, id, argument);
                        }
                        finally
                        {
                            requestStream.Dispose();
                            _tasks.Remove(id);
                        }
                    });

                    _tasks.Add(id, responseTask);
                    responseTask.Start();
                }
            }

            void SendResponse<T>(AmoebaFunctionResponseType type, int id, T value)
            {
                var messageStream = new RecyclableMemoryStream(_bufferManager);
                var writer = new MessageStreamWriter(messageStream, _bufferManager);

                writer.Write((ulong)type);
                writer.Write((ulong)id);

                Stream valueStream = null;

                if (value != null)
                {
                    try
                    {
                        valueStream = new RecyclableMemoryStream(_bufferManager);
                        JsonUtils.Save(valueStream, value);
                    }
                    catch (Exception)
                    {
                        if (valueStream != null)
                        {
                            valueStream.Dispose();
                            valueStream = null;
                        }

                        return;
                    }
                }

                messageStream.Seek(0, SeekOrigin.Begin);
                _messagingManager.Send(new UniteStream(messageStream, valueStream));
            }
        }

        public class ResponseTask
        {
            private CancellationTokenSource _tokenSource;
            private Task _task;

            private ResponseTask(Action<CancellationToken> action)
            {
                _tokenSource = new CancellationTokenSource();
                _task = new Task(() => action(_tokenSource.Token));
            }

            public static ResponseTask Create(Action<CancellationToken> action)
            {
                return new ResponseTask(action);
            }

            public void Start()
            {
                _task.Start();
            }

            public void Stop()
            {
                _tokenSource.Cancel();
                _task.Wait();
            }
        }

        protected override void Dispose(bool disposing)
        {
            if (_disposed) return;
            _disposed = true;

            if (disposing)
            {
                _socket.Dispose();
                _serviceManager.Dispose();
                _messagingManager.Dispose();
            }
        }
    }
}
