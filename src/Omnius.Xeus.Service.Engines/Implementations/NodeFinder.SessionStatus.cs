using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Tasks;
using Omnius.Xeus.Service.Engines.Internal.Models;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Engines;

public sealed partial class NodeFinder
{
    private sealed class SessionStatus : DisposableBase
    {
        public SessionStatus(ISession session, ReadOnlyMemory<byte> id, NodeLocation nodeLocation, IBatchActionDispatcher batchActionDispatcher)
        {
            this.Session = session;
            this.Id = id;
            this.NodeLocation = nodeLocation;

            _volatileReceivedWantContentClues = new VolatileHashSet<ContentClue>(TimeSpan.FromMinutes(3), batchActionDispatcher);
            this.ReceivedWantContentClues = new(_volatileReceivedWantContentClues);
        }

        protected override void OnDispose(bool disposing)
        {
            if (disposing)
            {
                _volatileReceivedWantContentClues.Dispose();
            }
        }

        public ISession Session { get; }

        public ReadOnlyMemory<byte> Id { get; }

        public NodeLocation NodeLocation { get; }

        public NodeFinderDataMessage? SendingDataMessage { get; set; } = null;

        private readonly VolatileHashSet<ContentClue> _volatileReceivedWantContentClues;

        public LockedSet<ContentClue> ReceivedWantContentClues { get; }
    }
}
