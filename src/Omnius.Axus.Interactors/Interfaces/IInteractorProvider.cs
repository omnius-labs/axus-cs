namespace Omnius.Axus.Interactors;

public interface IInteractorProvider : IAsyncDisposable
{
    IProfileUploader GetProfileUploader();
    IProfileDownloader GetProfileDownloader();
    INoteUploader GetMemoUploader();
    INoteDownloader GetMemoDownloader();
    ISeedUploader GetSeedUploader();
    ISeedDownloader GetSeedDownloader();
    IFileUploader GetFileUploader();
    IFileDownloader GetFileDownloader();
}
