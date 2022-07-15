using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Omnius.Axis.Interactors;
using Omnius.Axis.Interactors.Models;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Axis.Ui.Desktop.Models;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Windows.Main;

public class DownloadViewViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly UiStatus _uiState;
    private readonly IInteractorProvider _interactorProvider;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;

    private readonly CollectionViewUpdater<DownloadingFileViewModel, DownloadingFileReport> _downloadingFilesUpdater;

    private readonly CompositeDisposable _disposable = new();

    public DownloadViewViewModel(UiStatus uiState, IInteractorProvider interactorProvider, IApplicationDispatcher applicationDispatcher, IDialogService dialogService, IClipboardService clipboardService)
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
            return (x?.FileSeed == y?.FileSeed);
        }

        public int GetHashCode([DisallowNull] DownloadingFileReport obj)
        {
            return obj?.FileSeed?.GetHashCode() ?? 0;
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

        var text = await _dialogService.ShowTextEditWindowAsync();

        foreach (var seed in ParseFileSeeds(text))
        {
            await fileDownloader.RegisterAsync(seed);
        }
    }

    private static IEnumerable<FileSeed> ParseFileSeeds(string text)
    {
        var results = new List<FileSeed>();

        foreach (var line in text.Split(new string[] { "\r\n", "\n" }, StringSplitOptions.RemoveEmptyEntries).Select(n => n.Trim()))
        {
            if (!AxisMessageConverter.TryStringToFileSeed(line, out var seed)) continue;
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
            if (viewModel.Model?.FileSeed is FileSeed fileSeed)
            {
                await fileDownloader.UnregisterAsync(fileSeed);
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
            if (viewModel.Model?.FileSeed is null) continue;
            sb.AppendLine(AxisMessageConverter.FileSeedToString(viewModel.Model.FileSeed));
        }

        await _clipboardService.SetTextAsync(sb.ToString());
    }
}
