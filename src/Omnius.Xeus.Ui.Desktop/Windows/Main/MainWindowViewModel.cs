using System.Reactive.Disposables;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Ui.Desktop.Configuration;
using Omnius.Xeus.Ui.Desktop.Windows.Main.Peers;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Windows.Main
{
    public interface IMainWindowViewModel
    {
        public SelectedTabState SelectedTabState { get; }
    }

    public sealed class SelectedTabState
    {
        public ReactivePropertySlim<bool>? Status { get; set; }

        public ReactivePropertySlim<bool>? Peers { get; set; }

        public ReactivePropertySlim<bool>? Search { get; set; }

        public ReactivePropertySlim<bool>? Download { get; set; }

        public ReactivePropertySlim<bool>? Upload { get; set; }

        public ReactivePropertySlim<bool>? Settings { get; set; }
    }

    public class DesignMainWindowViewModel : DisposableBase, IMainWindowViewModel
    {
        private readonly CompositeDisposable _disposable = new();

        public DesignMainWindowViewModel()
        {
            this.SelectedTabState.Status = new ReactivePropertySlim<bool>(true).AddTo(_disposable);
            this.SelectedTabState.Peers = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
            this.SelectedTabState.Search = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
            this.SelectedTabState.Download = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
            this.SelectedTabState.Upload = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
            this.SelectedTabState.Settings = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
        }

        protected override void OnDispose(bool disposing)
        {
            _disposable.Dispose();
        }

        public SelectedTabState SelectedTabState { get; } = new();
    }

    public class MainWindowViewModel : AsyncDisposableBase, IMainWindowViewModel
    {
        private readonly UiState _uiState;

        private readonly CompositeDisposable _disposable = new();

        public MainWindowViewModel(UiState uiState, IPeersControlViewModel peersControlViewModel)
        {
            _uiState = uiState;
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

        public IPeersControlViewModel PeersControlViewModel { get; }
    }
}
