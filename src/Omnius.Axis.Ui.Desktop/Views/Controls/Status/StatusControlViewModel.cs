using System.Reactive.Disposables;
using Avalonia.Threading;
using Omnius.Axis.Intaractors;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Controls;

public class StatusControlViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IIntaractorProvider _intaractorAdapter;
    private readonly IClipboardService _clipboardService;

    private readonly Task _refreshTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly CompositeDisposable _disposable = new();

    public StatusControlViewModel(IIntaractorProvider intaractorAdapter, IClipboardService clipboardService)
    {
        _intaractorAdapter = intaractorAdapter;
        _clipboardService = clipboardService;

        this.MyNodeLocation = new ReactiveProperty<string>().AddTo(_disposable);
        this.CopyMyNodeLocationCommand = new ReactiveCommand().AddTo(_disposable);
        this.CopyMyNodeLocationCommand.Subscribe(() => this.CopyMyNodeLocation()).AddTo(_disposable);

        _refreshTask = this.RefreshAsync(_cancellationTokenSource.Token);
    }

    protected override async ValueTask OnDisposeAsync()
    {
        _cancellationTokenSource.Cancel();

        await _refreshTask;

        _cancellationTokenSource.Dispose();
    }

    public ReactiveProperty<string> MyNodeLocation { get; }

    public ReactiveCommand CopyMyNodeLocationCommand { get; }

    private async void CopyMyNodeLocation()
    {
        await _clipboardService.SetTextAsync(this.MyNodeLocation.Value);
    }

    private async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            for (; ; )
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);

                var dashboard = await _intaractorAdapter.GetDashboardAsync();

                var myNodeLocation = await dashboard.GetMyNodeLocationAsync(cancellationToken);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.MyNodeLocation.Value = AxisMessage.NodeLocationToString(myNodeLocation);
                });
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
    }
}
