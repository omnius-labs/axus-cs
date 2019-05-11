using System;
using System.Collections.Generic;
using Amoeba.Messages;
using Omnix.Base;
using Omnix.Configuration;
using Omnix.Net;
using Omnix.Net.I2p;
using Omnix.Utils;

namespace Xeus.Core
{
    partial class ConnectionManager
    {
        public sealed class I2pConnectionManager : StateManagerBase, ISettings
        {
            private BufferPool _bufferPool;

            private Settings _settings;

            private I2pConnectionConfig _config;

            private SamManager _samManager;

            private TimerScheduler _watchTimer;

            private List<string> _locationUris = new List<string>();

            private volatile ManagerState _state = ManagerState.Stop;

            private AtomicCounter _blockCount = new AtomicCounter();

            private readonly object _lockObject = new object();
            private volatile bool _disposed;

            public I2pConnectionManager(string configPath, BufferPool bufferPool)
            {
                _bufferPool = bufferPool;

                _settings = new Settings(configPath);

                _watchTimer = new TimerScheduler(this.WatchThread);
            }

            public I2pConnectionConfig Config
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _config;
                    }
                }
            }

            public void SetConfig(I2pConnectionConfig config)
            {
                lock (_lockObject)
                {
                    if (_config == config) return;
                    _config = config;
                }

                _watchTimer.RunOnce();
            }

            public IEnumerable<string> LocationUris
            {
                get
                {
                    lock (_lockObject)
                    {
                        return _locationUris.ToArray();
                    }
                }
            }

            public Cap ConnectCap(string uri)
            {
                if (_disposed) return null;
                if (this.State == ManagerState.Stop) return null;
                if (!this.Config.IsEnabled) return null;

                if (!uri.StartsWith("i2p:")) return null;

                try
                {
                    var result = UriUtils.Parse(uri);
                    if (result == null) return null;

                    string scheme = result.GetValue<string>("Scheme");
                    if (scheme != "i2p") return null;

                    string address = result.GetValue<string>("Address");

                    var socket = _samManager.Connect(address);
                    if (socket == null) return null;

                    return new SocketCap(socket);
                }
                catch (Exception)
                {

                }

                return null;
            }

            public Cap AcceptCap(out string uri)
            {
                uri = null;

                if (_disposed) return null;
                if (this.State == ManagerState.Stop) return null;
                if (!this.Config.IsEnabled) return null;

                try
                {
                    var socket = _samManager.Accept(out string base32Address);
                    if (socket == null) return null;

                    uri = $"i2p:{base32Address}";

                    return new SocketCap(socket);
                }
                catch (Exception)
                {

                }

                return null;
            }

            private volatile string _watchSamBridgeUri = null;

            private void WatchThread()
            {
                try
                {
                    for (; ; )
                    {
                        var config = this.Config;

                        string i2pUri = null;

                        if (config.IsEnabled)
                        {
                            if ((_samManager == null || !_samManager.IsConnected)
                                || _watchSamBridgeUri != config.SamBridgeUri)
                            {
                                try
                                {
                                    var result = UriUtils.Parse(config.SamBridgeUri);
                                    if (result == null) throw new Exception();

                                    string scheme = result.GetValue<string>("Scheme");
                                    if (scheme != "tcp") throw new Exception();

                                    string address = result.GetValue<string>("Address");
                                    int port = result.GetValueOrDefault<int>("Port", () => 7656);

                                    if (_samManager != null)
                                    {
                                        _samManager.Dispose();
                                        _samManager = null;
                                    }

                                    _samManager = new SamManager(address, port, "Amoeba");
                                    _samManager.Start();

                                    _watchSamBridgeUri = config.SamBridgeUri;
                                }
                                catch (Exception)
                                {
                                    if (_samManager != null)
                                    {
                                        _samManager.Dispose();
                                        _samManager = null;
                                    }
                                }
                            }

                            if (_samManager.Base32Address != null)
                            {
                                i2pUri = string.Format("i2p:{0}", _samManager.Base32Address);
                            }
                        }
                        else
                        {
                            if (_samManager != null)
                            {
                                _samManager.Dispose();
                                _samManager = null;
                            }
                        }

                        lock (_lockObject)
                        {
                            if (this.Config != config) continue;

                            _locationUris.Clear();
                            if (i2pUri != null) _locationUris.Add(i2pUri);
                        }

                        return;
                    }
                }
                catch (Exception e)
                {
                    Log.Error(e);
                }
            }

            public override ManagerState State
            {
                get
                {
                    return _state;
                }
            }

            private readonly object _stateLockObject = new object();

            public override void Start()
            {
                lock (_stateLockObject)
                {
                    lock (_lockObject)
                    {
                        if (this.State == ManagerState.Start) return;
                        _state = ManagerState.Start;

                        _watchTimer.Start(new TimeSpan(0, 0, 0), new TimeSpan(0, 3, 0));
                    }
                }
            }

            public override void Stop()
            {
                lock (_stateLockObject)
                {
                    lock (_lockObject)
                    {
                        if (this.State == ManagerState.Stop) return;
                        _state = ManagerState.Stop;
                    }

                    _watchTimer.Stop();

                    if (_samManager != null)
                    {
                        _samManager.Dispose();
                        _samManager = null;
                    }
                }
            }

            #region ISettings

            public void Load()
            {
                lock (_lockObject)
                {
                    int version = _settings.Load("Version", () => 0);

                    _config = _settings.Load<I2pConnectionConfig>("Config", () => new I2pConnectionConfig(true, "tcp:127.0.0.1:7656"));
                }
            }

            public void Save()
            {
                lock (_lockObject)
                {
                    _settings.Save("Version", 0);

                    _settings.Save("Config", _config);
                }
            }

            #endregion

            protected override void Dispose(bool disposing)
            {
                if (_disposed) return;
                _disposed = true;

                if (disposing)
                {
                    if (_watchTimer != null)
                    {
                        _watchTimer.Dispose();
                        _watchTimer = null;
                    }

                    if (_samManager != null)
                    {
                        _samManager.Dispose();
                        _samManager = null;
                    }
                }
            }
        }
    }
}
