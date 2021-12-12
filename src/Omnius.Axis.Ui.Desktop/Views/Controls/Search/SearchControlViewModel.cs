using Omnius.Core;

namespace Omnius.Axis.Ui.Desktop.Controls;

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
