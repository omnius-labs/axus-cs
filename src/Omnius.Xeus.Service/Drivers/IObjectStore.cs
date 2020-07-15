using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Serialization.RocketPack;

namespace Omnius.Xeus.Service.Drivers
{
    public interface IObjectStoreFactory
    {
        public IObjectStore Create(ObjectStoreOptions options, IBytesPool bytesPool);
    }

    public interface IObjectStore
    {
        void Scrub();
        void CollectGarbage();
        IEnumerable<string> GetKeys();
        bool Remove(string key);
        void SetContent<T>(string key, T value) where T : IRocketPackObject<T>;
        void SetStateState(ObjectStoreState state);
        bool TryGetContent<T>(string key, out T value) where T : IRocketPackObject<T>;
        bool TryGetStateState(out ObjectStoreState? state);
    }
}
