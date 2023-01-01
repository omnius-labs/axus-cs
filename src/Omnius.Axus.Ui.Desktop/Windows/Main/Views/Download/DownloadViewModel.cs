using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Omnius.Axus.Interactors;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Axus.Ui.Desktop.Configuration;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public class DownloadViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly UiStatus _uiState;
    private readonly IInteractorProvider _interactorProvider;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;

    private readonly CollectionViewUpdater<DownloadingFileViewModel, DownloadingFileReport> _downloadingFilesUpdater;

    private readonly CompositeDisposable _disposable = new();

    public DownloadViewModel(UiStatus uiState, IInteractorProvider interactorProvider, IApplicationDispatcher applicationDispatcher, IDialogService dialogService, IClipboardService clipboardService)
    {
        _uiState = uiState;
        _interactorProvider = interactorProvider;
        _applicationDispatcher = applicationDispatcher;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        _downloadingFilesUpdater = new CollectionViewUpdater<DownloadingFileViewModel, DownloadingFileReport>(_applicationDispatcher, this.GetDownloadingFileReports, TimeSpan.FromSeconds(3), DownloadingFileReportEqualityComparer.Default);

        this.AddCommand = new ReactiveCommand().AddTo(_disposable);
        this.AddCommand.Subscribe(() => this.Register()).AddTo(_disposable);

        this.ItemDeleteCommand = new ReactiveCommand().AddTo(_disposable);
        this.ItemDeleteCommand.Subscribe(() => this.ItemDelete()).AddTo(_disposable);

        this.ItemCopySeedCommand = new ReactiveCommand().AddTo(_disposable);
        this.ItemCopySeedCommand.Subscribe(() => this.ItemCopySeed()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
        await _downloadingFilesUpdater.DisposeAsync();
    }

    public DownloadViewStatus Status => _uiState.DownloadView ??= new DownloadViewStatus();

    private async ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReports(CancellationToken cancellationToken)
    {
        var fileDownloader = _interactorProvider.GetFileDownloader();

        return await fileDownloader.GetDownloadingFileReportsAsync(cancellationToken);
    }

    private class DownloadingFileReportEqualityComparer : IEqualityComparer<DownloadingFileReport>
    {
        public static DownloadingFileReportEqualityComparer Default { get; } = new();

        public bool Equals(DownloadingFileReport? x, DownloadingFileReport? y)
        {
            return (x?.Seed == y?.Seed);
        }

        public int GetHashCode([DisallowNull] DownloadingFileReport obj)
        {
            return obj?.Seed?.GetHashCode() ?? 0;
        }
    }

    public ReactiveCommand AddCommand { get; }

    public ReadOnlyObservableCollection<DownloadingFileViewModel> DownloadingFiles => _downloadingFilesUpdater.Collection;

    public ObservableCollection<DownloadingFileViewModel> SelectedFiles { get; } = new();

    public ReactiveCommand ItemDeleteCommand { get; }

    public ReactiveCommand? ItemCopySeedCommand { get; }

    private async void Register()
    {
        var fileDownloader = _interactorProvider.GetFileDownloader();

        var text = await _dialogService.ShowMultilineTextEditAsync();

        foreach (var seed in ParseSeeds(text))
        {
            await fileDownloader.RegisterAsync(seed);
        }
    }

    private static IEnumerable<Seed> ParseSeeds(string text)
    {
        var results = new List<Seed>();

        foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
        {
            if (!AxusUriConverter.Instance.TryStringToSeed(line, out var seed)) continue;
            results.Add(seed);
        }

        return results;
    }

    private async void ItemDelete()
    {
        var fileDownloader = _interactorProvider.GetFileDownloader();

        var selectedFiles = this.SelectedFiles.ToArray();
        if (selectedFiles.Length == 0) return;

        foreach (var viewModel in selectedFiles)
        {
            if (viewModel.Model?.Seed is Seed seed)
            {
                await fileDownloader.UnregisterAsync(seed);
            }
        }
    }

    private async void ItemCopySeed()
    {
        var selectedFiles = this.SelectedFiles.ToArray();
        if (selectedFiles.Length == 0) return;

        var sb = new StringBuilder();

        foreach (var viewModel in selectedFiles)
        {
            if (viewModel.Model?.Seed is null) continue;
            sb.AppendLine(AxusUriConverter.Instance.SeedToString(viewModel.Model.Seed));
        }

        await _clipboardService.SetTextAsync(sb.ToString());
    }
}
