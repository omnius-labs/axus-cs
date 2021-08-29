using System;
using Omnius.Core;
using Omnius.Xeus.Service.Engines.Internal;
using Omnius.Xeus.Service.Engines.Internal.Models;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class NodeFinder
    {
        private sealed class SessionStatus : ISynchronized
        {
            public SessionStatus(ISession session, ReadOnlyMemory<byte> id, NodeLocation nodeLocation)
            {
                this.Session = session;
                this.Id = id;
                this.NodeLocation = nodeLocation;
            }

            public object LockObject { get; } = new object();

            public ISession Session { get; }

            public ReadOnlyMemory<byte> Id { get; }

            public NodeLocation NodeLocation { get; }

            public NodeFinderDataMessage? SendingDataMessage { get; set; } = null;

            public VolatileHashSet<ContentClue> ReceivedWantContentClues { get; } = new VolatileHashSet<ContentClue>(TimeSpan.FromMinutes(30));

            public void Refresh()
            {
                this.ReceivedWantContentClues.Refresh();
            }
        }
    }
}
