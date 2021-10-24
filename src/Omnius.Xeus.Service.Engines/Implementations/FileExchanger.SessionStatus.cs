using System.Collections.Generic;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Engines.Internal.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class FileExchanger
    {
        private sealed class SessionStatus
        {
            public SessionStatus(ISession session, OmniHash rootHash)
            {
                this.Session = session;
                this.RootHash = rootHash;
            }

            public ISession Session { get; }

            public OmniHash RootHash { get; }

            public FileExchangerDataMessage? SendingDataMessage { get; set; }

            public LockedSet<OmniHash> SentBlockHashes { get; } = new(new HashSet<OmniHash>());

            public LockedSet<OmniHash> ReceivedWantBlockHashes { get; set; } = new(new HashSet<OmniHash>());
        }
    }
}
