using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Engines.Internal.Models;

namespace Omnius.Xeus.Service.Engines
{
    public sealed partial class FileExchanger
    {
        private sealed class SessionStatus : ISynchronized
        {
            public SessionStatus(ISession session, OmniHash rootHash)
            {
                this.Session = session;
                this.RootHash = rootHash;
            }

            public object LockObject { get; } = new object();

            public ISession Session { get; }

            public OmniHash RootHash { get; }

            public FileExchangerDataMessage? SendingDataMessage { get; set; }

            public OmniHash[]? ReceivedWantBlockHashes { get; set; }
        }
    }
}
