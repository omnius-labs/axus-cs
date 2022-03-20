using Omnius.Core;

namespace Omnius.Axis.Engines;

public sealed partial class ShoutExchanger
{
    private sealed class SessionStatus : AsyncDisposableBase
    {
        public SessionStatus(ISession session)
        {
            this.Session = session;
        }

        protected override async ValueTask OnDisposeAsync()
        {
            await this.Session.DisposeAsync();
        }

        public ISession Session { get; }
    }
}
