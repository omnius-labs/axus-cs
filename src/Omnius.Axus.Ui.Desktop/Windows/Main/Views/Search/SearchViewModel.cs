using Omnius.Core;

namespace Omnius.Axus.Ui.Desktop.Windows.Main;

public class SearchViewModel : AsyncDisposableBase
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public SearchViewModel()
    {
    }

    protected override async ValueTask OnDisposeAsync()
    {
    }
}
