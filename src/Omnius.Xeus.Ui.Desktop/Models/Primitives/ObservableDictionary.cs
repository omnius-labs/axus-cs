using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using Omnius.Core;

namespace Omnius.Xeus.Ui.Desktop.Models.Primitives
{
    internal class ObservableDictionary<TKey, TValue> : IDictionary<TKey, TValue>
        where TKey : notnull
        where TValue : notnull
    {
        private readonly Dictionary<TKey, TValue> _dict;
        private readonly ObservableCollection<TValue> _collection = new();

        public ObservableDictionary()
            : this(EqualityComparer<TKey>.Default)
        {
        }

        public ObservableDictionary(IEqualityComparer<TKey> equalityComparer)
        {
            _dict = new Dictionary<TKey, TValue>(equalityComparer);
        }

        public void Sort(Comparison<TValue> comparison)
        {
            var list = _collection.ToList();
            list.Sort(comparison);

            for (int i = 0; i < list.Count; i++)
            {
                int o = _collection.IndexOf(list[i]);
                if (i != o)
                {
                    _collection.Move(o, i);
                }
            }
        }

        public TValue this[TKey key]
        {
            get
            {
                return _dict[key];
            }

            set
            {
                if (_dict.TryGetValue(key, out var oldValue))
                {
                    _ = _collection.Remove(oldValue);
                }

                _dict[key] = value;
                _collection.Add(value);
            }
        }

        public ICollection<TKey> Keys
        {
            get
            {
                return _dict.Keys;
            }
        }

        ICollection<TValue> IDictionary<TKey, TValue>.Values
        {
            get
            {
                return _dict.Values;
            }
        }

        private ReadOnlyObservableCollection<TValue>? _readOnlyValues;

        public ReadOnlyObservableCollection<TValue> Values
        {
            get
            {
                return _readOnlyValues ??= new ReadOnlyObservableCollection<TValue>(_collection);
            }
        }

        public int Count
        {
            get
            {
                return _dict.Count;
            }
        }

        public bool IsReadOnly => false;

        public void Add(TKey key, TValue value)
        {
            this[key] = value;
        }

        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item)
        {
            this[item.Key] = item.Value;
        }

        public void Clear()
        {
            _dict.Clear();
            _collection.Clear();
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item)
        {
            return ((ICollection<KeyValuePair<TKey, TValue>>)_dict).Contains(item);
        }

        public bool ContainsKey(TKey key)
        {
            return _dict.ContainsKey(key);
        }

        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
        {
            ((ICollection<KeyValuePair<TKey, TValue>>)_dict).CopyTo(array, arrayIndex);
        }

        public bool Remove(TKey key)
        {
            if (_dict.TryRemove(key, out var oldValue) && oldValue is not null)
            {
                _ = _collection.Remove(oldValue);
                return true;
            }

            return false;
        }

        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item)
        {
            if (((ICollection<KeyValuePair<TKey, TValue>>)_dict).Remove(item))
            {
                _ = _collection.Remove(item.Value);
                return true;
            }

            return false;
        }

        public bool TryGetValue(TKey key, [MaybeNullWhen(false)] out TValue value)
        {
            return _dict.TryGetValue(key, out value);
        }

        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator()
        {
            foreach (var item in _dict)
            {
                yield return item;
            }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
