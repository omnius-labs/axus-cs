using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Network;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service.Primitives
{
    public interface IExplorer
    {
        IEnumerable<NodeProfile> FindNodeProfiles(Span<byte> id);
    }
}
