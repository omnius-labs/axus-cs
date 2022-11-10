using Omnius.Axus.Remoting;

namespace Omnius.Axus.Interactors;

internal interface IServiceProvider
{
    bool IsConnected { get; }

    IAxusService Create();
}
