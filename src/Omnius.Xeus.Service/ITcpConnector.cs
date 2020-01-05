using System;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface ITcpConnector : IPrimitiveConnector, IAsyncDisposable
    {
    }
}
