using Omnius.Axus.Core.Engine.Models;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Tasks;

namespace Omnius.Axus.Core.Engine.Services;

internal sealed partial class FileExchanger
{
    private sealed class SessionStatus : AsyncDisposableBase
    {
        public SessionStatus(Session session, ExchangeType exchangeType, OmniHash rootHash, ISystemClock systemClock, IBatchActionDispatcher batchActionDispatcher)
        {
            this.Session = session;
            this.ExchangeType = exchangeType;
            this.RootHash = rootHash;
            this.BatchActionDispatcher = batchActionDispatcher;
            this.LastReceivedTime = DateTime.UtcNow;

            this.SentWantBlockHashes = new VolatileHashSet<OmniHash>(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), systemClock, batchActionDispatcher);
            this.SentBlockHashes = new VolatileHashSet<OmniHash>(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), systemClock, batchActionDispatcher);
            this.ReceivedWantBlockHashes = new VolatileHashSet<OmniHash>(TimeSpan.FromMinutes(3), TimeSpan.FromSeconds(30), systemClock, batchActionDispatcher);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.Session.DisposeAsync();

            this.SentBlockHashes.Dispose();
            this.ReceivedWantBlockHashes.Dispose();
        }

        public Session Session { get; }
        public ExchangeType ExchangeType { get; }
        public OmniHash RootHash { get; }
        public IBatchActionDispatcher BatchActionDispatcher { get; }
        public DateTime LastReceivedTime { get; set; }

        public FileExchangerDataMessage? SendingDataMessage { get; set; }

        public VolatileHashSet<OmniHash> SentWantBlockHashes { get; }
        public VolatileHashSet<OmniHash> SentBlockHashes { get; }

        public VolatileHashSet<OmniHash> ReceivedWantBlockHashes { get; }
    }

    private enum ExchangeType
    {
        Unknown,
        Published,
        Subscribed,
    }
}