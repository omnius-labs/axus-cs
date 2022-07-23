using Omnius.Axus.Remoting;

namespace Omnius.Axus.Interactors;

internal interface IAxusServiceProvider : IAsyncDisposable
{
    bool IsConnected { get; }

    IAxusService GetService();
}
