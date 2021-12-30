using Omnius.Core;

namespace Omnius.Axis.Ui.Desktop.Views.Main;

public class SearchControlViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public SearchControlViewModel()
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
    }
}
