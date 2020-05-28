using System;
using System.Collections;
using System.Collections.Generic;

namespace Omnius.Core.Collections
{
    public partial class VolatileListDictionary<TKey, TValue>
    {
        public sealed class KeyCollection : IReadOnlyCollection<TKey>, IEnumerable<TKey>, IEnumerable
        {
            private readonly ICollection<TKey> _collection;

            internal KeyCollection(ICollection<TKey> collection)
            {
                _collection = collection;
            }

            public TKey[] ToArray()
            {
                var array = new TKey[_collection.Count];
                _collection.CopyTo(array, 0);

                return array;
            }

            public int Count => _collection.Count;

            public IEnumerator<TKey> GetEnumerator()
            {
                foreach (var item in _collection)
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
}
