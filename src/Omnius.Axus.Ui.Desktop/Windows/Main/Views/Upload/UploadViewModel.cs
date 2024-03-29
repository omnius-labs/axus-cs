using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Text;
using Omnius.Axus.Interactors;
using Omnius.Axus.Interactors.Models;
using Omnius.Axus.Messages;
using Omnius.Axus.Ui.Desktop.Configuration;
using Omnius.Axus.Ui.Desktop.Internal;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public class UploadViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly UiStatus _uiState;
    private readonly IInteractorProvider _interactorProvider;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;

    private readonly CollectionViewUpdater<UploadingFileViewModel, FileUploadingReport> _uploadingFilesUpdater;
    private readonly ObservableCollection<UploadingFileViewModel> _selectedFiles = new();

    private readonly CompositeDisposable _disposable = new();

    public UploadViewModel(UiStatus uiState, IInteractorProvider interactorProvider, IApplicationDispatcher applicationDispatcher, IDialogService dialogService, IClipboardService clipboardService)
    {
        _uiState = uiState;
        _interactorProvider = interactorProvider;
        _applicationDispatcher = applicationDispatcher;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        _uploadingFilesUpdater = new CollectionViewUpdater<UploadingFileViewModel, FileUploadingReport>(_applicationDispatcher, this.GetUploadingFileReports, TimeSpan.FromSeconds(3), UploadingFileReportEqualityComparer.Default);

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
        await _uploadingFilesUpdater.DisposeAsync();
    }

    private async ValueTask<IEnumerable<FileUploadingReport>> GetUploadingFileReports(CancellationToken cancellationToken)
    {
        var fileUploader = _interactorProvider.GetFileUploader();

        return await fileUploader.GetUploadingFileReportsAsync(cancellationToken);
    }

    private class UploadingFileReportEqualityComparer : IEqualityComparer<FileUploadingReport>
    {
        public static UploadingFileReportEqualityComparer Default { get; } = new();

        public bool Equals(FileUploadingReport? x, FileUploadingReport? y)
        {
            return (x?.FilePath == y?.FilePath);
        }

        public int GetHashCode([DisallowNull] FileUploadingReport obj)
        {
            return obj?.FilePath?.GetHashCode() ?? 0;
        }
    }

    public ReactiveCommand AddCommand { get; }

    public ReadOnlyObservableCollection<UploadingFileViewModel> UploadingFiles => _uploadingFilesUpdater.Collection;

    public ObservableCollection<UploadingFileViewModel> SelectedFiles { get; } = new();

    public ReactiveCommand ItemDeleteCommand { get; }

    public ReactiveCommand? ItemCopySeedCommand { get; }

    private async void Register()
    {
        var fileUploader = _interactorProvider.GetFileUploader();

        foreach (var filePath in await _dialogService.ShowOpenFileWindowAsync())
        {
            await fileUploader.RegisterAsync(filePath, Path.GetFileName(filePath));
        }
    }

    private async void ItemDelete()
    {
        var fileUploader = _interactorProvider.GetFileUploader();

        var selectedFiles = this.SelectedFiles.ToArray();
        if (selectedFiles.Length == 0) return;

        foreach (var viewModel in selectedFiles)
        {
            if (viewModel.Model?.FilePath is string filePath)
            {
                await fileUploader.UnregisterAsync(filePath);
            }
        }
    }

    private async void ItemCopySeed()
    {
        var selectedFiles = this.SelectedFiles.ToArray();
        if (selectedFiles.Length == 0) return;

        var sb = new StringBuilder();
        foreach (var file in selectedFiles)
        {
            if (file.Model?.Seed is null) continue;
            sb.AppendLine(AxusUriConverter.Instance.SeedToString(file.Model.Seed));
        }

        await _clipboardService.SetTextAsync(sb.ToString());
    }
}
