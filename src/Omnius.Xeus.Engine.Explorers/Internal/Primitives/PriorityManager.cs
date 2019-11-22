using System;
using System.Collections.Generic;
using System.Linq;
using Omnius.Core;
using Omnius.Core.Extensions;

namespace Xeus.Engine.Internal.Search.Primitives
{
    internal sealed class PriorityManager
    {
        private readonly TimeSpan _survivalTime;

        private readonly Dictionary<DateTime, int> _table = new Dictionary<DateTime, int>();

        private readonly object _lockObject = new object();

        public PriorityManager(TimeSpan survivalTime)
        {
            _survivalTime = survivalTime;
        }

        public TimeSpan SurvivalTime => _survivalTime;

        public void Add(int value)
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow.Truncate(TimeSpan.FromSeconds(3));

                _table.AddOrUpdate(now, value, (_, origin) => origin + value);
            }
        }

        public int GetValue()
        {
            lock (_lockObject)
            {
                return _table.Sum(n => n.Value);
            }
        }

        public void Refresh()
        {
            lock (_lockObject)
            {
                var now = DateTime.UtcNow;

                foreach (var updateTime in _table.Keys.ToArray())
                {
                    if ((now - updateTime) < _survivalTime) continue;

                    _table.Remove(updateTime);
                }
            }
        }
    }
}
