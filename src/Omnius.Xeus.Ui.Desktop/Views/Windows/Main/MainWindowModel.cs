using System.Reactive.Disposables;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Xeus.Ui.Desktop.Resources;
using Reactive.Bindings;
using Reactive.Bindings.Extensions;

namespace Omnius.Xeus.Ui.Desktop.Views.Windows.Main
{
    public interface IMainWindowModel
    {
        public SelectedTabState SelectedTab { get; }
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

    public class DesignMainWindowModel : DisposableBase, IMainWindowModel
    {
        private readonly CompositeDisposable _disposable = new();

        public DesignMainWindowModel()
        {
            this.SelectedTab.Status = new ReactivePropertySlim<bool>(true).AddTo(_disposable);
            this.SelectedTab.Peers = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
            this.SelectedTab.Search = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
            this.SelectedTab.Download = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
            this.SelectedTab.Upload = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
            this.SelectedTab.Settings = new ReactivePropertySlim<bool>(false).AddTo(_disposable);
        }

        protected override void OnDispose(bool disposing)
        {
            _disposable.Dispose();
        }

        public SelectedTabState SelectedTab { get; } = new();
    }

    public class MainWindowModel : AsyncDisposableBase, IMainWindowModel
    {
        private readonly AppState _state;

        private readonly CompositeDisposable _disposable = new();

        public MainWindowModel(AppState state)
        {
            _state = state;

            var uiState = _state.GetUiState();

            this.SelectedTab.Status = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTab_Status).AddTo(_disposable);
            this.SelectedTab.Peers = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTab_Peers).AddTo(_disposable);
            this.SelectedTab.Search = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTab_Search).AddTo(_disposable);
            this.SelectedTab.Download = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTab_Download).AddTo(_disposable);
            this.SelectedTab.Upload = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTab_Upload).AddTo(_disposable);
            this.SelectedTab.Settings = uiState.ToReactivePropertySlimAsSynchronized(n => n.MainWindowModel_SelectedTab_Settings).AddTo(_disposable);
        }

        protected override async ValueTask OnDisposeAsync()
        {
            _disposable.Dispose();
        }

        public SelectedTabState SelectedTab { get; } = new();
    }
}
