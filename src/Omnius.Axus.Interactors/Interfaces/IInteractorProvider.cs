namespace Omnius.Axus.Interactors;

public interface IInteractorProvider : IAsyncDisposable
{
    IProfileUploader GetProfileUploader();
    IProfileDownloader GetProfileDownloader();
    IBarkUploader GetBarkUploader();
    IBarkDownloader GetBarkDownloader();
    ISeedUploader GetSeedUploader();
    ISeedDownloader GetSeedDownloader();
    IFileUploader GetFileUploader();
    IFileDownloader GetFileDownloader();
}
