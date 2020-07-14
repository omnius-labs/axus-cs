using System;
using System.Collections.Generic;
using System.Text;

namespace Omnius.Xeus.Service.Drivers
{
    public interface IDataStoreFactory
    {
        public ValueTask<IDataStore> CreateAsync(DataStoreOptions options, IBytesPool bytesPool);
    }

    interface IDataStore<TKey, TMeta, TValue>
    {
        void AddOrUpdate(TKey key, TMeta meta, TValue value);
        
        bool TryGetMeta(TKey key, out TMeta meta);
        
        bool TryGetValue(TKey key, out TValue value);

        void Delete(TKey key);
        
        void Scrub();
        
        void CollectGarbage();
    }
}
