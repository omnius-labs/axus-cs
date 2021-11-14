using System.Collections.ObjectModel;
using System.Diagnostics.CodeAnalysis;
using System.Reactive.Disposables;
using System.Text;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Xeus.Intaractors;
using Omnius.Xeus.Intaractors.Models;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.ViewModels;
using Omnius.Xeus.Ui.Desktop.Windows;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Controls;

public class UploadControlViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();
    private readonly UiState _uiState;
    private readonly IFileUploader _fileUploader;
    private readonly IApplicationDispatcher _applicationDispatcher;
    private readonly IDialogService _dialogService;
    private readonly IClipboardService _clipboardService;

    private readonly CollectionViewUpdater<UploadingFileViewModel, UploadingFileReport> _uploadingFilesUpdater;
    private readonly ObservableCollection<UploadingFileViewModel> _selectedFiles = new();

    private readonly CompositeDisposable _disposable = new();

    public UploadControlViewModel(UiState uiState, IFileUploader fileUploader, IApplicationDispatcher applicationDispatcher, IDialogService dialogService, IClipboardService clipboardService)
    {
        _uiState = uiState;
        _fileUploader = fileUploader;
        _applicationDispatcher = applicationDispatcher;
        _dialogService = dialogService;
        _clipboardService = clipboardService;

        _uploadingFilesUpdater = new CollectionViewUpdater<UploadingFileViewModel, UploadingFileReport>(_applicationDispatcher, this.GetUploadingFileReports, TimeSpan.FromSeconds(3), UploadingFileReportEqualityComparer.Default);

        this.RegisterCommand = new ReactiveCommand().AddTo(_disposable);
        this.RegisterCommand.Subscribe(() => this.Register()).AddTo(_disposable);

        this.ItemCopySeedCommand = new ReactiveCommand().AddTo(_disposable);
        this.ItemCopySeedCommand.Subscribe(() => this.ItemCopySeed()).AddTo(_disposable);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _disposable.Dispose();
        await _uploadingFilesUpdater.DisposeAsync();
    }

    private async ValueTask<IEnumerable<UploadingFileReport>> GetUploadingFileReports()
    {
        return await _fileUploader.GetUploadingFileReportsAsync();
    }

    private class UploadingFileReportEqualityComparer : IEqualityComparer<UploadingFileReport>
    {
        public static UploadingFileReportEqualityComparer Default { get; } = new();

        public bool Equals(UploadingFileReport? x, UploadingFileReport? y)
        {
            return (x?.Seed == y?.Seed);
        }

        public int GetHashCode([DisallowNull] UploadingFileReport obj)
        {
            return obj?.Seed?.GetHashCode() ?? 0;
        }
    }

    public ReactiveCommand RegisterCommand { get; }

    public ReadOnlyObservableCollection<UploadingFileViewModel> UploadingFiles => _uploadingFilesUpdater.Collection;

    public ObservableCollection<UploadingFileViewModel> SelectedFiles { get; } = new();

    public ReactiveCommand? ItemCopySeedCommand { get; }

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
            if (file.Model?.Seed is null) continue;
            sb.AppendLine(XeusMessage.SeedToString(file.Model.Seed));
        }

        await _clipboardService.SetTextAsync(sb.ToString());
    }
}
