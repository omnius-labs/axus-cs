using System;
using System.Collections.Generic;
using System.Text;
using System.Threading;

namespace Omnius.Xeus.Service.Primitives
{
    public interface IPublishStorage : IReadOnlyStorage
    {
        IAsyncEnumerable<PublishReport> GetReportsAsync(CancellationToken cancellationToken = default);
    }
}
