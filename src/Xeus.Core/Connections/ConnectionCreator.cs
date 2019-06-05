using System;
using System.Collections.Generic;
using System.IO;
using Omnix.Base;
using Omnix.Configuration;
using Xeus.Rpc.Primitives;

namespace Xeus.Core.Connections.Internal
{
    sealed partial class ConnectionCreator : DisposableBase, ISettings
    {
        private BufferPool _bufferPool;
        private CoreManager _coreManager;
        private CatharsisManager _catharsisManager;
        private TcpConnectionManager _tcpConnectionManager;
        private I2pConnectionManager _i2pConnectionManager;
        private CustomConnectionManager _customConnectionManager;
        private TimerScheduler _watchTimer;

        private volatile ManagerState _state = ManagerState.Stop;

        private readonly object _lockObject = new object();
        private volatile bool _disposed;

        public ConnectionManager(string configPath, CoreManager coreManager, BufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _coreManager = coreManager;
            _catharsisManager = new CatharsisManager(Path.Combine(configPath, "Catharsis"), _bufferPool);
            _tcpConnectionManager = new TcpConnectionManager(Path.Combine(configPath, "TcpConnection"), _catharsisManager, _bufferPool);
            _i2pConnectionManager = new I2pConnectionManager(Path.Combine(configPath, "I2pConnection"), _bufferPool);
            _customConnectionManager = new CustomConnectionManager(Path.Combine(configPath, "CustomConnection"), _catharsisManager, _bufferPool);

            _coreManager.ConnectCapEvent = (_, uri) => this.ConnectCap(uri);
            _coreManager.AcceptCapEvent = (object _, out string uri) => this.AcceptCap(out uri);

            _watchTimer = new TimerScheduler(this.WatchThread);
        }

        public ConnectionReport Report
        {
            get
            {
                lock (_lockObject)
                {
                    return new ConnectionReport(_tcpConnectionManager.Report, _customConnectionManager.Report);
                }
            }
        }

        public ConnectionConfig Config
        {
            get
            {
                lock (_lockObject)
                {
                    return new ConnectionConfig(_tcpConnectionManager.Config, _i2pConnectionManager.Config, _customConnectionManager.Config, _catharsisManager.Config);
                }
            }
        }

        public void SetConfig(ConnectionConfig config)
        {
            lock (_lockObject)
            {
                _catharsisManager.SetConfig(config.Catharsis);
                _customConnectionManager.SetConfig(config.Custom);
                _tcpConnectionManager.SetConfig(config.Tcp);
                _i2pConnectionManager.SetConfig(config.I2p);
            }
        }

        public Cap ConnectCap(string uri)
        {
            if (_disposed)
            {
                return null;
            }

            if (this.State == ManagerState.Stop)
            {
                return null;
            }

            Cap cap;
            if ((cap = _tcpConnectionManager.ConnectCap(uri)) != null)
            {
                return cap;
            }

            if ((cap = _i2pConnectionManager.ConnectCap(uri)) != null)
            {
                return cap;
            }

            if ((cap = _customConnectionManager.ConnectCap(uri)) != null)
            {
                return cap;
            }

            return null;
        }

        public Cap AcceptCap(out string uri)
        {
            uri = null;

            if (_disposed)
            {
                return null;
            }

            if (this.State == ManagerState.Stop)
            {
                return null;
            }

            Cap cap;
            if ((cap = _tcpConnectionManager.AcceptCap(out uri)) != null)
            {
                return cap;
            }

            if ((cap = _i2pConnectionManager.AcceptCap(out uri)) != null)
            {
                return cap;
            }

            if ((cap = _customConnectionManager.AcceptCap(out uri)) != null)
            {
                return cap;
            }

            return null;
        }

        private void WatchThread()
        {
            var targetUris = new List<string>();

            targetUris.AddRange(_tcpConnectionManager.LocationUris);
            targetUris.AddRange(_i2pConnectionManager.LocationUris);
            targetUris.AddRange(_customConnectionManager.LocationUris);

            _coreManager.SetMyLocation(new Location(targetUris));
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
                    if (this.State == ManagerState.Start)
                    {
                        return;
                    }

                    _state = ManagerState.Start;

                    _catharsisManager.Start();
                    _tcpConnectionManager.Start();
                    _i2pConnectionManager.Start();
                    _customConnectionManager.Start();

                    _watchTimer.Start(new TimeSpan(0, 0, 0), new TimeSpan(0, 0, 30));
                }
            }
        }

        public override void Stop()
        {
            lock (_stateLockObject)
            {
                lock (_lockObject)
                {
                    if (this.State == ManagerState.Stop)
                    {
                        return;
                    }

                    _state = ManagerState.Stop;
                }

                _watchTimer.Stop();

                _catharsisManager.Stop();
                _tcpConnectionManager.Stop();
                _i2pConnectionManager.Stop();
                _customConnectionManager.Stop();
            }
        }

        #region ISettings

        public void Load()
        {
            lock (_lockObject)
            {
                _catharsisManager.Load();
                _tcpConnectionManager.Load();
                _i2pConnectionManager.Load();
                _customConnectionManager.Load();
            }
        }

        public void Save()
        {
            lock (_lockObject)
            {
                _catharsisManager.Save();
                _tcpConnectionManager.Save();
                _i2pConnectionManager.Save();
                _customConnectionManager.Save();
            }
        }

        #endregion

        protected override void Dispose(bool disposing)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;

            if (disposing)
            {
                _catharsisManager.Dispose();
                _tcpConnectionManager.Dispose();
                _i2pConnectionManager.Dispose();
                _customConnectionManager.Dispose();
            }
        }
    }
}
