using Omnius.Axus.Remoting;

namespace Omnius.Axus.Interactors;

internal interface IServiceProvider : IAsyncDisposable
{
    bool IsConnected { get; }

    IAxusService Create();
}
