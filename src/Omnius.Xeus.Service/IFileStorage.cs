using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Primitives;

namespace Omnius.Xeus.Service
{
    public interface IFileStorage : IPrimitiveStorage
    {
        ValueTask<OmniHash> AddPublishFile(string filePath, CancellationToken cancellationToken = default);
        void RemovePublishFile(string filePath);
        IEnumerable<PublishFileReport> GetPublishFileReports();

        void AddWantFile(string filePath, OmniHash rootHash);
        void RemoveWantFile(string filePath, OmniHash rootHash);
        IEnumerable<WantFileReport> GetWantFileReports();
    }
}
