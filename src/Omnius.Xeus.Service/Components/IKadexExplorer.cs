using System;
using System.Collections.Generic;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Network;

namespace Omnius.Xeus.Service.Components
{
    public interface IKadexExplorer
    {
        NodeProfile MyNodeProfile { get; }
        void SetMyNodeProfile(NodeProfile nodeProfile);
        IEnumerable<NodeProfile> FindNodeProfile(Span<byte> id);
    }
}
