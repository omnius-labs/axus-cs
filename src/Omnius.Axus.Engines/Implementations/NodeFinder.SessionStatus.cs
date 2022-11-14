using Omnius.Axus.Engines.Internal.Models;
using Omnius.Axus.Engines.Models;
using Omnius.Axus.Messages;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Tasks;

namespace Omnius.Axus.Engines;

public sealed partial class NodeFinder
{
    private sealed class SessionStatus : AsyncDisposableBase
    {
        public SessionStatus(ISession session, ReadOnlyMemory<byte> id, NodeLocation nodeLocation, IBatchActionDispatcher batchActionDispatcher)
        {
            this.Session = session;
            this.Id = id;
            this.NodeLocation = nodeLocation;
            this.LastReceivedTime = DateTime.UtcNow;

            this.ReceivedWantContentClues = new VolatileHashSet<ContentClue>(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), batchActionDispatcher);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.Session.DisposeAsync();

            this.ReceivedWantContentClues.Dispose();
        }

        public ISession Session { get; }

        public ReadOnlyMemory<byte> Id { get; }

        public NodeLocation NodeLocation { get; }

        public DateTime LastReceivedTime { get; set; }

        public NodeFinderDataMessage? SendingDataMessage { get; set; } = null;

        public VolatileHashSet<ContentClue> ReceivedWantContentClues { get; }
    }
}
