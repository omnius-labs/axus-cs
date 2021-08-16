using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Engines
{
    public interface INodeFinder : IAsyncDisposable
    {
        ValueTask<NodeFinderReport> GetReportAsync(CancellationToken cancellationToken = default);

        ValueTask<NodeLocation> GetMyNodeLocationAsync(CancellationToken cancellationToken = default);

        ValueTask AddCloudNodeLocationsAsync(IEnumerable<NodeLocation> nodeLocations, CancellationToken cancellationToken = default);

        ValueTask<NodeLocation[]> FindNodeLocationsAsync(ContentClue clue, CancellationToken cancellationToken = default);
    }
}
