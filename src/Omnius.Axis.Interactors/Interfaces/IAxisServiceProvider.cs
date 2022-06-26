using Omnius.Axis.Remoting;

namespace Omnius.Axis.Interactors;

internal interface IAxisServiceProvider : IAsyncDisposable
{
    bool IsConnected { get; }

    IAxisService GetService();
}
