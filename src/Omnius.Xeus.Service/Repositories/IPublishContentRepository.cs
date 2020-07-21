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
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Repositories
{
    public interface IPublishContentRepository
    {
        ContentBlock GetContentBlock(OmniHash rootHash, OmniHash targetHash);
        void RemoveContentBlock(OmniHash rootHash, OmniHash targetHash);
    }
}
