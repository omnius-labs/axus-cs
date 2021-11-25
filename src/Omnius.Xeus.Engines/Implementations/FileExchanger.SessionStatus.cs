using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Tasks;
using Omnius.Xeus.Engines.Internal.Models;

namespace Omnius.Xeus.Engines;

public sealed partial class FileExchanger
{
    private sealed class SessionStatus : AsyncDisposableBase
    {
        public SessionStatus(ISession session, OmniHash rootHash, IBatchActionDispatcher batchActionDispatcher)
        {
            this.Session = session;
            this.RootHash = rootHash;
            this.LastReceivedTime = DateTime.UtcNow;

            this.SentBlockHashes = new(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), batchActionDispatcher);
            this.ReceivedWantBlockHashes = new(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), batchActionDispatcher);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.Session.DisposeAsync();

            this.SentBlockHashes.Dispose();
            this.ReceivedWantBlockHashes.Dispose();
        }

        public ISession Session { get; }

        public OmniHash RootHash { get; }

        public DateTime LastReceivedTime { get; set; }

        public FileExchangerDataMessage? SendingDataMessage { get; set; }

        public VolatileHashSet<OmniHash> SentBlockHashes { get; }

        public VolatileHashSet<OmniHash> ReceivedWantBlockHashes { get; }
    }
}
