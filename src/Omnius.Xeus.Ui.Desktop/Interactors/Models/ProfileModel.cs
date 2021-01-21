using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Ui.Desktop.Interactors.Models.Primitives;

namespace Omnius.Xeus.Ui.Desktop.Interactors.Models
{
    public sealed class ProfileModel : BindableBase
    {
        public ProfileModel()
        {
        }

        private OmniSignature _signature = OmniSignature.Empty;

        public OmniSignature Signature
        {
            get => _signature;
            set => this.SetProperty(ref _signature, value);
        }

        private XeusProfileContent _profile = XeusProfileContent.Empty;

        public XeusProfileContent Profile
        {
            get => _profile;
            set => this.SetProperty(ref _profile, value);
        }
    }
}
