using System;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Core.Avalonia;
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
        private readonly IClipboardService _clipboardService;
        private readonly Task _refreshTask;

        private readonly ObservableDictionary<string, UploadingFileElement> _uploadedFileMap = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public UploadControlViewModel(IFileUploader fileUploader, IDialogService dialogService, IClipboardService clipboardService)
        {
            _fileUploader = fileUploader;
            _dialogService = dialogService;
            _clipboardService = clipboardService;

            this.AddFileCommand = new ReactiveCommand().AddTo(_disposable);
            this.AddFileCommand.Subscribe(() => this.AddFile()).AddTo(_disposable);

            this.UploadedFiles = _uploadedFileMap.Values.ToReadOnlyReactiveCollection().AddTo(_disposable);

            this.SelectedItem = new ReactiveProperty<UploadingFileElement>().AddTo(_disposable);

            this.CopyCommand = new ReactiveCommand().AddTo(_disposable);
            this.CopyCommand.Subscribe(() => this.Copy()).AddTo(_disposable);


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

        public ReadOnlyReactiveCollection<UploadingFileElement> UploadedFiles { get; }

        public ReactiveProperty<UploadingFileElement> SelectedItem { get; }

        public ReactiveCommand CopyCommand { get; }

        private async void AddFile()
        {
            foreach (var filePath in await _dialogService.ShowOpenFileWindowAsync())
            {
                await _fileUploader.RegisterAsync(filePath, Path.GetFileName(filePath));
            }
        }

        private async void Copy()
        {
            var selectedItem = this.SelectedItem.Value;
            if (selectedItem == null) return;

            var seed = selectedItem.Model.Seed;
            if (seed == null) return;

            var text = XeusMessage.SeedToString(seed);
            await _clipboardService.SetTextAsync(text);
        }

        private async Task RefreshAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                for (; ; )
                {
                    await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                    var uploadedFileReports = await _fileUploader.GetUploadingFileReportsAsync(cancellationToken);
                    var elements = uploadedFileReports.Select(n => new UploadingFileElement(n))
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
