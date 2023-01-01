using Omnius.Axus.Remoting;

namespace Omnius.Axus.Interactors;

internal interface IAxusServiceProvider
{
    bool IsConnected { get; }

    IAxusService Create();
}
