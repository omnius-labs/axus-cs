using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Network;
using Omnius.Core.Network.Caps;
using Omnius.Core.Network.Connections;
using Omnius.Core.Network.Proxies;
using Omnius.Core.Network.Upnp;
using Omnius.Xeus.Service.Engines;

namespace Omnius.Xeus.Service.Repositories
{
    public interface IWantDeclaredMessageRepositoryFactory
    {
        ValueTask<IWantDeclaredMessageRepository> CreateAsync(WantDeclaredMessageRepositoryOptions options, IBytesPool bytesPool);
    }

    public interface IWantDeclaredMessageRepository
    {
        IEnumerable<OmniHash> GetWants();
        void AddWant(OmniHash hash);
        void RemoveWant(OmniHash hash);

        DeclaredMessage GetDeclaredMessage(OmniHash hash);
        void GetDeclaredMessageCreationTime(OmniHash hash);
        void AddDeclaredMessage(DeclaredMessage message);
        void RemoveDeclaredMessage(OmniHash hash);
    }
}
