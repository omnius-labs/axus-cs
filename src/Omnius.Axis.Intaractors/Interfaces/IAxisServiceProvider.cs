using Omnius.Axis.Remoting;

namespace Omnius.Axis.Intaractors;

internal interface IAxisServiceProvider : IAsyncDisposable
{
    bool IsConnected { get; }

    IAxisService GetService();
}
