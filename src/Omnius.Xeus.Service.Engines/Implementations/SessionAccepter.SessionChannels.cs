using System.Collections.Concurrent;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Channels;

namespace Omnius.Xeus.Service.Engines;

public sealed partial class SessionAccepter
{
    private sealed class SessionChannels
    {
        private readonly int _capacity;

        private readonly ConcurrentDictionary<string, Channel<ISession>> _sessionChannels = new();

        public SessionChannels(int capacity)
        {
            _capacity = capacity;
        }

        public bool TryGet(string scheme, [NotNullWhen(true)] out Channel<ISession>? channel)
        {
            return _sessionChannels.TryGetValue(scheme, out channel);
        }

        public Channel<ISession> GetOrCreate(string scheme)
        {
            return _sessionChannels.GetOrAdd(scheme, (_) => Channel.CreateBounded<ISession>(new BoundedChannelOptions(_capacity) { FullMode = BoundedChannelFullMode.Wait }));
        }

        public bool Contains(string scheme)
        {
            return _sessionChannels.ContainsKey(scheme);
        }
    }
}
