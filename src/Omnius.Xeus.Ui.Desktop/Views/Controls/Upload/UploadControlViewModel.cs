using System;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Ui.Desktop.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;
using Omnius.Xeus.Ui.Desktop.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Controls
{
    public class UploadControlViewModel : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IFileUploader _fileUploader;
        private readonly IDialogService _dialogService;

        private readonly Task _refreshTask;

        private readonly ObservableDictionary<string, UploadedFileElement> _uploadedFileMap = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public UploadControlViewModel(IFileUploader fileUploader, IDialogService dialogService)
        {
            _fileUploader = fileUploader;
            _dialogService = dialogService;

            this.AddFileCommand = new ReactiveCommand().AddTo(_disposable);
            _ = this.AddFileCommand.Subscribe(() => this.AddFile()).AddTo(_disposable);
            this.UploadedFiles = _uploadedFileMap.Values.ToReadOnlyReactiveCollection().AddTo(_disposable);

            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _refreshTask;
            _cancellationTokenSource.Dispose();

            _disposable.Dispose();
        }

        public ReactiveCommand AddFileCommand { get; }

        public ReadOnlyReactiveCollection<UploadedFileElement> UploadedFiles { get; }

        private async void AddFile()
        {
            foreach (var filePath in await _dialogService.ShowOpenFileWindowAsync())
            {
                await _fileUploader.RegisterAsync(filePath);
            }
        }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                for (; ; )
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                    var uploadedFileReports = await _fileUploader.GetUploadingFileReportsAsync(cancellationToken);
                    var elements = uploadedFileReports.Select(n => new UploadedFileElement(n))
                        .ToDictionary(n => n.FilePath);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        foreach (var key in _uploadedFileMap.Keys.ToArray())
                        {
                            if (elements.ContainsKey(key))
                            {
                                continue;
                            }

                            _ = _uploadedFileMap.Remove(key);
                        }

                        foreach (var (key, element) in elements)
                        {
                            if (!_uploadedFileMap.TryGetValue(key, out var viewModel))
                            {
                                _uploadedFileMap.Add(key, element);
                            }
                            else
                            {
                                viewModel.Model = element.Model;
                            }
                        }
                    });
                }
            }
            catch (OperationCanceledException e)
            {
                _logger.Debug(e);
            }
        }
    }
}
