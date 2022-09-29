using Avalonia.Threading;
using Omnius.Axus.Interactors;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public class StatusViewViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IServiceMediator _serviceMediator;
    private readonly IClipboardService _clipboardService;

    private readonly Task _refreshTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly CompositeDisposable _disposable = new();

    public StatusViewViewModel(IServiceMediator serviceMediator, IClipboardService clipboardService)
    {
        _serviceMediator = serviceMediator;
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
        await Task.Delay(1).ConfigureAwait(false);

        try
        {
            for (; ; )
            {
                await Task.Delay(TimeSpan.FromSeconds(3), cancellationToken).ConfigureAwait(false);

                var myNodeLocation = await _serviceMediator.GetMyNodeLocationAsync(cancellationToken);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.MyNodeLocation.Value = AxusMessageConverter.NodeToString(myNodeLocation);
                });
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e, "Operation Canceled");
        }
    }
}
