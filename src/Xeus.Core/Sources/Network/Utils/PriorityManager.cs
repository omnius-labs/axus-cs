using System;
using System.Collections.Generic;
using System.Linq;
using Omnius.Base;

namespace Amoeba.Service
{
    partial class NetworkManager
    {
        public sealed class PriorityManager
        {
            private readonly TimeSpan _survivalTime;

            private Dictionary<DateTime, int> _table = new Dictionary<DateTime, int>();

            private readonly object _lockObject = new object();

            public PriorityManager(TimeSpan survivalTime)
            {
                _survivalTime = survivalTime;
            }

            public TimeSpan SurvivalTime
            {
                get
                {
                    return _survivalTime;
                }
            }

            public void Add(int value)
            {
                lock (_lockObject)
                {
                    var now = DateTime.UtcNow;

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

            public void Update()
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
}
