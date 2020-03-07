using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Omnius.Xeus.Service.Primitives
{
    public interface IWantStorage : IWritableStorage
    {
        IAsyncEnumerable<WantReport> GetReportsAsync(CancellationToken cancellationToken = default);
    }
}
