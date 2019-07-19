using System;
using System.Collections.Generic;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Configuration;
using Omnix.Network;
using Xeus.Core.Primitives;
using Xeus.Messages;

namespace Xeus.Core.Connections.Internal
{
    sealed partial class ConnectionCreator : ServiceBase, ISettings
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly BufferPool _bufferPool;
        private readonly TcpConnectionCreator _tcpConnectionCreator;

        private readonly SettingsDatabase _settings;

        private readonly AsyncLock _asyncLock = new AsyncLock();
        private readonly object _lockObject = new object();

        public ConnectionCreator(string configPath, BufferPool bufferPool)
        {
            _bufferPool = bufferPool;
            _tcpConnectionCreator = new TcpConnectionCreator(configPath, bufferPool);

            _settings = new SettingsDatabase(Path.Combine(configPath, "TcpConnectionCreator"));
        }

        public void SetOptions(ConnectionCreatorOptions options)
        {
            lock (_lockObject)
            {
                _tcpConnectionCreator.SetTcpConnectOptions(options.TcpConnectOptions);
                _tcpConnectionCreator.SetTcpAcceptOptions(options.TcpAcceptOptions);
            }
        }

        public async ValueTask<Cap?> ConnectAsync(OmniAddress address, CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return null;
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return null;
            }

            Cap? result;

            if ((result = await _tcpConnectionCreator.ConnectAsync(address, token)) != null)
            {
                return result;
            }

            return null;
        }

        public async ValueTask<(Cap?, OmniAddress?)> AcceptAsync(CancellationToken token = default)
        {
            if (this.IsDisposed)
            {
                return default;
            }

            if (this.StateType != ServiceStateType.Running)
            {
                return default;
            }

            (Cap?, OmniAddress?) result;

            if ((result = await _tcpConnectionCreator.AcceptAsync(token)) != default)
            {
                return result;
            }

            return default;
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

        ValueTask ISettings.LoadAsync()
        {
            throw new NotImplementedException();
        }

        ValueTask ISettings.SaveAsync()
        {
            throw new NotImplementedException();
        }
    }
}
