using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Service.Components
{
    public interface IFileStorage : IPrimitiveStorage
    {
        ValueTask<OmniHash> AddPublishFile(string filePath, CancellationToken cancellationToken = default);
        void RemovePublishFile(string filePath);
        IEnumerable<PublishFileReport> GetPublishFileReports();

        void AddWantFile(OmniHash rootHash, string filePath);
        void RemoveWantFile(OmniHash rootHash, string filePath);
        IEnumerable<WantFileReport> GetWantFileReports();
    }
}
