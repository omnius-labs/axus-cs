using System.Reactive.Disposables;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.Windows.Main.Peers;
using Omnius.Xeus.Ui.Desktop.Windows.Main.Status;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main
{
    public sealed class SelectedTabState
    {
        public ReactivePropertySlim<bool>? Status { get; set; }

        public ReactivePropertySlim<bool>? Peers { get; set; }

        public ReactivePropertySlim<bool>? Search { get; set; }

        public ReactivePropertySlim<bool>? Download { get; set; }

        public ReactivePropertySlim<bool>? Upload { get; set; }

        public ReactivePropertySlim<bool>? Settings { get; set; }
    }

    public class MainWindowViewModel : AsyncDisposableBase
    {
        private readonly UiState _uiState;

        private readonly CompositeDisposable _disposable = new();

        public MainWindowViewModel(UiState uiState, StatusControlViewModel statusControlViewModel, PeersControlViewModel peersControlViewModel)
        {
            _uiState = uiState;
            this.StatusControlViewModel = statusControlViewModel;
            this.PeersControlViewModel = peersControlViewModel;

            this.SelectedTabState.Status = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTabState_Status).AddTo(_disposable);
            this.SelectedTabState.Peers = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTabState_Peers).AddTo(_disposable);
            this.SelectedTabState.Search = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTabState_Search).AddTo(_disposable);
            this.SelectedTabState.Download = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTabState_Download).AddTo(_disposable);
            this.SelectedTabState.Upload = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTabState_Upload).AddTo(_disposable);
            this.SelectedTabState.Settings = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTabState_Settings).AddTo(_disposable);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _disposable.Dispose();
        }

        public SelectedTabState SelectedTabState { get; } = new();

        public StatusControlViewModel StatusControlViewModel { get; }

        public PeersControlViewModel PeersControlViewModel { get; }
    }
}
