using System;
using System.Reactive.Disposables;
using System.Threading;
using System.Threading.Tasks;
using Avalonia.Threading;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Omnius.Xeus.Intaractors;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Controls;

public class StatusControlViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IDashboard _dashboard;
    private readonly IClipboardService _clipboardService;

    private readonly Task _refreshTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly CompositeDisposable _disposable = new();

    public StatusControlViewModel(IDashboard dashboard, IClipboardService clipboardService)
    {
        _dashboard = dashboard;
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
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken);

                var myNodeLocation = await _dashboard.GetMyNodeLocationAsync(cancellationToken);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.MyNodeLocation.Value = XeusMessage.NodeLocationToString(myNodeLocation);
                });
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
    }
}