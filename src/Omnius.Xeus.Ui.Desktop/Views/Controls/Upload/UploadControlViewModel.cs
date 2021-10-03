using System;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
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

        private readonly ObservableDictionary<string, UploadingFileElement> _uploadingFileMap = new();
        private readonly ObservableCollection<UploadingFileElement> _selectedFiles = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public UploadControlViewModel(IFileUploader fileUploader, IDialogService dialogService, IClipboardService clipboardService)
        {
            _fileUploader = fileUploader;
            _dialogService = dialogService;
            _clipboardService = clipboardService;

            this.RegisterCommand = new ReactiveCommand().AddTo(_disposable);
            this.RegisterCommand.Subscribe(() => this.Register()).AddTo(_disposable);

            this.UploadingFiles = _uploadingFileMap.Values.ToReadOnlyReactiveCollection().AddTo(_disposable);

            this.ItemCopySeedCommand = new ReactiveCommand().AddTo(_disposable);
            this.ItemCopySeedCommand.Subscribe(() => this.ItemCopySeed()).AddTo(_disposable);

            _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _cancellationTokenSource.Cancel();
            await _refreshTask;
            _cancellationTokenSource.Dispose();

            _disposable.Dispose();
        }

        public ReactiveCommand RegisterCommand { get; }

        public ReadOnlyReactiveCollection<UploadingFileElement> UploadingFiles { get; }

        public ObservableCollection<UploadingFileElement> SelectedFiles => _selectedFiles;

        public ReactiveCommand ItemCopySeedCommand { get; }

        private async void Register()
        {
            foreach (var filePath in await _dialogService.ShowOpenFileWindowAsync())
            {
                await _fileUploader.RegisterAsync(filePath, Path.GetFileName(filePath));
            }
        }

        private async void ItemCopySeed()
        {
            var selectedFiles = this.SelectedFiles.ToArray();
            if (selectedFiles.Length == 0) return;

            var sb = new StringBuilder();
            foreach (var file in selectedFiles)
            {
                if (file.Model.Seed is null) continue;
                sb.AppendLine(XeusMessage.SeedToString(file.Model.Seed));
            }

            await _clipboardService.SetTextAsync(sb.ToString());
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
                        foreach (var key in _uploadingFileMap.Keys.ToArray())
                        {
                            if (elements.ContainsKey(key))
                            {
                                continue;
                            }

                            _uploadingFileMap.Remove(key);
                        }

                        foreach (var (key, element) in elements)
                        {
                            if (!_uploadingFileMap.TryGetValue(key, out var viewModel))
                            {
                                _uploadingFileMap.Add(key, element);
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
