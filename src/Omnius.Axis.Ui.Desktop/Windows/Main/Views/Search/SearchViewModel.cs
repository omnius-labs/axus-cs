using Omnius.Core;

namespace Omnius.Axis.Ui.Desktop.Windows.Main;

public class SearchViewViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public SearchViewViewModel()
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
    }
}
