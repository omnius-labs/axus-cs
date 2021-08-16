namespace Omnius.Xeus.Engines.Exchangers
{
    public sealed partial class ShoutExchanger
    {
        private sealed class SessionStatus
        {
            public SessionStatus(ISession session)
            {
                this.Session = session;
            }

            public ISession Session { get; }
        }
    }
}
