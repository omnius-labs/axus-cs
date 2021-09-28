using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Xeus.Intaractors.Models;

namespace Omnius.Xeus.Intaractors
{
    public interface IFileUploader
    {
        ValueTask<IEnumerable<UploadingFileReport>> GetUploadingFileReportsAsync(CancellationToken cancellationToken = default);

        ValueTask RegisterAsync(string filePath, string name, CancellationToken cancellationToken = default);

        ValueTask UnregisterAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
