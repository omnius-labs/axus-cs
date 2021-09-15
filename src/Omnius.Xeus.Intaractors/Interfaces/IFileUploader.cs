using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace Omnius.Xeus.Intaractors
{
    public interface IFileUploader
    {
        ValueTask<IEnumerable<UploadedFileReport>> GetUploadedFileReportsAsync(CancellationToken cancellationToken = default);
        ValueTask RegisterAsync(string filePath, CancellationToken cancellationToken = default);
        ValueTask UnregisterAsync(string filePath, CancellationToken cancellationToken = default);
    }
}
