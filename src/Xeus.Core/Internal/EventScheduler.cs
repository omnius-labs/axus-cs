using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Omnix.Base;
using Omnix.Base.Extensions;

namespace Xeus.Core.Internal
{
    internal sealed class EventScheduler : DisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private Action _callback;
        private readonly object _callbackLockObject = new object();

        private Timer _timer;
        private volatile bool _running;

        private readonly object _lockObject = new object();
        private volatile bool _isDisposed;

        public EventScheduler(Action callback)
        {
            _callback = callback;
            _timer = new Timer((_) => this.Timer(), null, Timeout.Infinite, Timeout.Infinite);
        }

        private void Timer()
        {
            if (_isDisposed)
            {
                return;
            }

            bool taken = false;

            try
            {
                Monitor.TryEnter(_callbackLockObject, ref taken);

                if (taken)
                {
                    if (!_running)
                    {
                        return;
                    }

                    _callback();
                }
            }
            catch (Exception e)
            {
                _logger.Error(e);
            }
            finally
            {
                if (taken)
                {
                    Monitor.Exit(_callbackLockObject);
                }
            }
        }

        public void Run()
        {
            lock (_lockObject)
            {
                if (!_running)
                {
                    return;
                }

                Task.Run(() => this.Timer());
            }
        }

        public void Start(TimeSpan period)
        {
            lock (_lockObject)
            {
                _running = true;

                _timer.Change(period, period);
            }
        }

        public void Start(TimeSpan start, TimeSpan period)
        {
            lock (_lockObject)
            {
                _running = true;

                _timer.Change(start, period);
            }
        }

        public void Start(int period)
        {
            lock (_lockObject)
            {
                _running = true;

                _timer.Change(period, period);
            }
        }

        public void Start(int start, int period)
        {
            lock (_lockObject)
            {
                _running = true;

                _timer.Change(start, period);
            }
        }

        public void Stop()
        {
            lock (_lockObject)
            {
                _timer.Change(Timeout.Infinite);

                bool taken = false;

                try
                {
                    Monitor.Enter(_callbackLockObject, ref taken);

                    if (taken)
                    {
                        _running = false;
                    }
                }
                finally
                {
                    if (taken)
                    {
                        Monitor.Exit(_callbackLockObject);
                    }
                }
            }
        }

        protected override void Dispose(bool isDisposing)
        {
            if (_isDisposed)
            {
                return;
            }

            _isDisposed = true;

            if (isDisposing)
            {
                _timer.Dispose();
            }
        }
    }
}
