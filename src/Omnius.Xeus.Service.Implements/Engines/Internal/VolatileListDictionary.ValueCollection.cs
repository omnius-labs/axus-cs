using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace Omnius.Core.Collections
{
    public partial class VolatileListDictionary<TKey, TValue>
    {
        public sealed class ValueCollection : IReadOnlyCollection<IReadOnlyList<TValue>>, IEnumerable<IReadOnlyList<TValue>>, IEnumerable
        {
            private readonly ICollection<List<Entry<TValue>>> _collection;

            internal ValueCollection(ICollection<List<Entry<TValue>>> collection)
            {
                _collection = collection;
            }

            public IReadOnlyList<TValue>[] ToArray()
            {
                return _collection.Select(n => new ReadOnlyListSlim<TValue>(n.Select(m => m.Value).ToArray())).ToArray();
            }

            public int Count => _collection.Count;

            public IEnumerator<IReadOnlyList<TValue>> GetEnumerator()
            {
                foreach (var entries in _collection)
                {
                    yield return entries.Select(n => n.Value).ToArray();
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            {
                return this.GetEnumerator();
            }
        }
    }
}
