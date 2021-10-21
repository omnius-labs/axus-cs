using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Reactive.Disposables;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.Models;
using Omnius.Xeus.Ui.Desktop.Models.Primitives;
using Omnius.Xeus.Ui.Desktop.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Controls
{
    public class DownloadControlViewModel : AsyncDisposableBase
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
        private readonly UiState _uiState;
        private readonly IFileDownloader _fileDownloader;
        private readonly IDialogService _dialogService;
        private readonly IClipboardService _clipboardService;
        private readonly Task _refreshTask;

        private readonly ObservableDictionary<Seed, DownloadingFileElement> _downloadingFileMap = new();
        private readonly ObservableCollection<DownloadingFileElement> _selectedFiles = new();

        private readonly CancellationTokenSource _cancellationTokenSource = new();

        private readonly CompositeDisposable _disposable = new();

        public DownloadControlViewModel(UiState uiState, IFileDownloader fileDownloader, IDialogService dialogService, IClipboardService clipboardService)
        {
            _uiState = uiState;
            _fileDownloader = fileDownloader;
            _dialogService = dialogService;
            _clipboardService = clipboardService;

            this.RegisterCommand = new ReactiveCommand().AddTo(_disposable);
            this.RegisterCommand.Subscribe(() => this.Register()).AddTo(_disposable);

            this.DownloadingFiles = _downloadingFileMap.Values.ToReadOnlyReactiveCollection().AddTo(_disposable);

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

        public ReactiveProperty<long> Width { get; }

        public DynamicState DynamicState => _uiState.DownloadControl.Dynamic;

        public ReactiveCommand RegisterCommand { get; }

        public ReadOnlyReactiveCollection<DownloadingFileElement> DownloadingFiles { get; }

        public ObservableCollection<DownloadingFileElement> SelectedFiles => _selectedFiles;

        public ReactiveCommand ItemCopySeedCommand { get; }

        private async void Register()
        {
            var text = await _dialogService.GetTextWindowAsync();

            foreach (var seed in ParseSeeds(text))
            {
                await _fileDownloader.RegisterAsync(seed);
            }
        }

        private static IEnumerable<Seed> ParseSeeds(string text)
        {
            var results = new List<Seed>();

            foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
            {
                if (!XeusMessage.TryStringToSeed(line, out var seed)) continue;
                results.Add(seed);
            }

            return results;
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

                    var downloadedFileReports = await _fileDownloader.GetDownloadingFileReportsAsync(cancellationToken);
                    var elements = downloadedFileReports.Select(n => new DownloadingFileElement(n))
                        .ToDictionary(n => n.Model.Seed);

                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        foreach (var key in _downloadingFileMap.Keys.ToArray())
                        {
                            if (elements.ContainsKey(key))
                            {
                                continue;
                            }

                            _downloadingFileMap.Remove(key);
                        }

                        foreach (var (key, element) in elements)
                        {
                            if (!_downloadingFileMap.TryGetValue(key, out var viewModel))
                            {
                                _downloadingFileMap.Add(key, element);
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
