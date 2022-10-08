namespace Omnius.Axus.Interactors;

public interface IInteractorProvider : IAsyncDisposable
{
    IProfilePublisher GetProfilePublisher();
    IProfileSubscriber GetProfileSubscriber();
    IBarkPublisher GetBarkPublisher();
    IBarkSubscriber GetBarkSubscriber();
    IFileUploader GetFileUploader();
    IFileDownloader GetFileDownloader();
}
