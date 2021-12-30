using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Text;
using Omnius.Axis.Intaractors;
using Omnius.Axis.Intaractors.Models;
using Omnius.Axis.Ui.Desktop.Configuration;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Axis.Ui.Desktop.ViewModels;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public class DownloadControlViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly UiStatus _uiState;
    private readonly IIntaractorProvider _intaractorAdapter;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;

    private readonly CollectionViewUpdater<DownloadingFileViewModel, DownloadingFileReport> _downloadingFilesUpdater;

    private readonly CompositeDisposable _disposable = new();

    public DownloadControlViewModel(UiStatus uiState, IIntaractorProvider intaractorAdapter, IApplicationDispatcher applicationDispatcher, IDialogService dialogService, IClipboardService clipboardService)
    {
        _uiState = uiState;
        _intaractorAdapter = intaractorAdapter;
        _applicationDispatcher = applicationDispatcher;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        _downloadingFilesUpdater = new CollectionViewUpdater<DownloadingFileViewModel, DownloadingFileReport>(_applicationDispatcher, this.GetDownloadingFileReports, TimeSpan.FromSeconds(3), DownloadingFileReportEqualityComparer.Default);

        this.RegisterCommand = new ReactiveCommand().AddTo(_disposable);
        this.RegisterCommand.Subscribe(() => this.Register()).AddTo(_disposable);

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

    public DownloadControlStatus Status => _uiState.DownloadControl ??= new DownloadControlStatus();

    private async ValueTask<IEnumerable<DownloadingFileReport>> GetDownloadingFileReports(CancellationToken cancellationToken)
    {
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync(cancellationToken);

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

    public ReactiveCommand RegisterCommand { get; }

    public ReadOnlyObservableCollection<DownloadingFileViewModel> DownloadingFiles => _downloadingFilesUpdater.Collection;

    public ObservableCollection<DownloadingFileViewModel> SelectedFiles { get; } = new();

    public ReactiveCommand ItemDeleteCommand { get; }

    public ReactiveCommand? ItemCopySeedCommand { get; }

    private async void Register()
    {
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync();

        var text = await _dialogService.ShowTextWindowAsync();

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
            if (!AxisMessage.TryStringToSeed(line, out var seed)) continue;
            results.Add(seed);
        }

        return results;
    }

    private async void ItemDelete()
    {
        var fileDownloader = await _intaractorAdapter.GetFileDownloaderAsync();

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
            sb.AppendLine(AxisMessage.SeedToString(viewModel.Model.Seed));
        }

        await _clipboardService.SetTextAsync(sb.ToString());
    }
}
