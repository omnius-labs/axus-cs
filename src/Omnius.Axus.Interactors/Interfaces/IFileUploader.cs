using Omnius.Axus.Interactors.Models;

namespace Omnius.Axus.Interactors;

public interface IFileUploader : IAsyncDisposable
{
    ValueTask<IEnumerable<UploadingFileReport>> GetUploadingFileReportsAsync(CancellationToken cancellationToken = default);
    ValueTask RegisterAsync(string filePath, string name, CancellationToken cancellationToken = default);
    ValueTask UnregisterAsync(string filePath, CancellationToken cancellationToken = default);
}
