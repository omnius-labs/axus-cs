using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Omnius.Xeus.Engines.Engines.Internal
{
    public class VolatileHashSet<T> : ISet<T>, ICollection<T>, IEnumerable<T>
        where T : notnull
    {
        private readonly Dictionary<T, DateTime> _map;
        private readonly TimeSpan _survivalTime;

        public VolatileHashSet(TimeSpan survivalTime)
        {
            _map = new Dictionary<T, DateTime>();
            _survivalTime = survivalTime;
        }

        public VolatileHashSet(TimeSpan survivalTime, IEqualityComparer<T> comparer)
        {
            _map = new Dictionary<T, DateTime>(comparer);
            _survivalTime = survivalTime;
        }

        public TimeSpan SurvivalTime => _survivalTime;

        public T[] ToArray()
        {
            return _map.Keys.ToArray();
        }

        public void Refresh()
        {
            var now = DateTime.UtcNow;

            var removingKeys = new List<T>();

            foreach (var (key, value) in _map)
            {
                if ((now - value) > _survivalTime)
                {
                    removingKeys.Add(key);
                }
            }

            foreach (var key in removingKeys)
            {
                _map.Remove(key);
            }
        }

        public TimeSpan GetElapsedTime(T item)
        {
            if (!_map.TryGetValue(item, out var updateTime))
            {
                return _survivalTime;
            }

            var now = DateTime.UtcNow;
            return (now - updateTime);
        }

        public IEqualityComparer<T> Comparer
        {
            get
            {
                return _map.Comparer;
            }
        }

        public int Count
        {
            get
            {
                return _map.Count;
            }
        }

        public bool Add(T item)
        {
            int count = _map.Count;
            _map[item] = DateTime.UtcNow;

            return (count != _map.Count);
        }

        public void Clear()
        {
            _map.Clear();
        }

        public bool Contains(T item)
        {
            return _map.ContainsKey(item);
        }

        public void CopyTo(T[] array, int arrayIndex)
        {
            _map.Keys.CopyTo(array, arrayIndex);
        }

        public bool Remove(T item)
        {
            return _map.Remove(item);
        }

        bool ICollection<T>.IsReadOnly => false;

        void ICollection<T>.Add(T item)
        {
            this.Add(item);
        }

        public IEnumerator<T> GetEnumerator()
        {
            foreach (var item in _map.Keys)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void UnionWith(IEnumerable<T> other)
        {
            var now = DateTime.UtcNow;

            foreach (var value in other)
            {
                _map[value] = now;
            }
        }

        public void ExceptWith(IEnumerable<T> other)
        {
            foreach (var value in other)
            {
                _map.Remove(value);
            }
        }

        public void IntersectWith(IEnumerable<T> other)
        {
            var tempList = new List<T>();

            foreach (var value in other)
            {
                if (_map.ContainsKey(value))
                {
                    continue;
                }

                tempList.Add(value);
            }

            foreach (var key in tempList)
            {
                _map.Remove(key);
            }
        }

        public bool IsProperSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsProperSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSubsetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool IsSupersetOf(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool Overlaps(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public bool SetEquals(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }

        public void SymmetricExceptWith(IEnumerable<T> other)
        {
            throw new NotImplementedException();
        }
    }
}
