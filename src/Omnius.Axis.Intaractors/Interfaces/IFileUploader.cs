using Omnius.Axis.Intaractors.Models;

namespace Omnius.Axis.Intaractors;

public interface IFileUploader
{
    ValueTask<IEnumerable<UploadingFileReport>> GetUploadingFileReportsAsync(CancellationToken cancellationToken = default);

    ValueTask RegisterAsync(string filePath, string name, CancellationToken cancellationToken = default);

    ValueTask UnregisterAsync(string filePath, CancellationToken cancellationToken = default);
}
