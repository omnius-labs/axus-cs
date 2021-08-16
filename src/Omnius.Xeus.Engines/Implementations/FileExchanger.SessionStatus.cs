using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Internal.Models;

namespace Omnius.Xeus.Engines.Exchangers
{
    public sealed partial class FileExchanger
    {
        private sealed class SessionStatus : ISynchronized
        {
            public SessionStatus(ISession session, OmniHash contentHash)
            {
                this.Session = session;
                this.ContentHash = contentHash;
            }

            public object LockObject { get; } = new object();

            public ISession Session { get; }

            public OmniHash ContentHash { get; }

            public FileExchangerDataMessage? SendingDataMessage { get; set; }

            public OmniHash[]? ReceivedWantBlockHashes { get; set; }
        }
    }
}
