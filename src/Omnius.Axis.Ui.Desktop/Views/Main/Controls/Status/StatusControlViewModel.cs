using Avalonia.Threading;
using Omnius.Axis.Intaractors;
using Omnius.Axis.Ui.Desktop.Internal;
using Omnius.Core;
using Omnius.Core.Avalonia;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public class StatusControlViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    private readonly IIntaractorProvider _intaractorAdapter;
    private readonly IClipboardService _clipboardService;
    private readonly INodesFetcher _nodesFetcher;

    private readonly Task _refreshTask;

    private readonly CancellationTokenSource _cancellationTokenSource = new();

    private readonly CompositeDisposable _disposable = new();

    public StatusControlViewModel(IIntaractorProvider intaractorAdapter, IClipboardService clipboardService, INodesFetcher nodesFetcher)
    {
        _intaractorAdapter = intaractorAdapter;
        _clipboardService = clipboardService;
        _nodesFetcher = nodesFetcher;

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

                var serviceAdapter = await _intaractorAdapter.GetServiceAdapterAsync();

                var myNodeLocation = await serviceAdapter.GetMyNodeLocationAsync(cancellationToken);

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    this.MyNodeLocation.Value = AxisMessage.NodeToString(myNodeLocation);
                });

                var cloudNodeLocations = await serviceAdapter.GetCloudNodeLocationsAsync(cancellationToken);

                if (cloudNodeLocations.Count() == 0)
                {
                    var fetchedNodeLocations = await _nodesFetcher.FetchAsync(cancellationToken);
                    await serviceAdapter.AddCloudNodeLocationsAsync(fetchedNodeLocations, cancellationToken);
                }
            }
        }
        catch (OperationCanceledException e)
        {
            _logger.Debug(e);
        }
    }
}
