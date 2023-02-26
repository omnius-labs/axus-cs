namespace Omnius.Axus.Interactors;

public interface IInteractorProvider : IAsyncDisposable
{
    IProfileUploader GetProfileUploader();
    IProfileDownloader GetProfileDownloader();
    IMemoUploader GetMemoUploader();
    IMemoDownloader GetMemoDownloader();
    ISeedUploader GetSeedUploader();
    ISeedDownloader GetSeedDownloader();
    IFileUploader GetFileUploader();
    IFileDownloader GetFileDownloader();
}
