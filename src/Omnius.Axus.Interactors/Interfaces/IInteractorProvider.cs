namespace Omnius.Axus.Interactors;

public interface IInteractorProvider : IAsyncDisposable
{
    IProfilePublisher GetProfilePublisher();
    IProfileSubscriber GetProfileSubscriber();
    IBarkUploader GetBarkPublisher();
    IBarkDownloader GetBarkSubscriber();
    IFileUploader GetFileUploader();
    IFileDownloader GetFileDownloader();
}
