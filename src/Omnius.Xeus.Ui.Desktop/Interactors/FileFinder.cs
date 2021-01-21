using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Threading;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Models;
using Omnius.Xeus.Ui.Desktop.Resources;

namespace Omnius.Xeus.Ui.Desktop.Interactors
{
    public sealed class FileFinder
    {
        private readonly ProfileFinder _profileFinder;
        private readonly UiSettings _uiSettings;

        public FileFinder(ProfileFinder profileFinder, UiSettings uiSettings)
        {
            _profileFinder = profileFinder;
            _uiSettings = uiSettings;
        }

        public async void FindAllAsync()
        {
            foreach (var trustedSignature in _profileFinder.GetTrustedSignatures())
            {
                var profile = _profileFinder.GetProfile(trustedSignature);
                if (profile is null)
                {
                    continue;
                }

                foreach (var fileMeta in profile.FileMetas)
                {

                }
            }



            var rootTrustedSignatures = _uiSettings.GetTrustedSignatures().ToList();
            var depth = _uiSettings.GetSearchDepth();

            var trustedSignatures = new List<OmniSignature>();
            trustedSignatures.AddRange(rootTrustedSignatures);

            for (int i = 0; i < depth; i++)
            {
                var temp = this.GetProfiles(signatures, cancellationToken);

            }
        }

        private async IAsyncEnumerable<XeusProfileContent> GetProfiles(IEnumerable<OmniSignature> signatures, [EnumeratorCancellation] CancellationToken cancellationToken = default)
        {
            foreach (var s in signatures)
            {
                cancellationToken.ThrowIfCancellationRequested();

                var result = await _profileSubscriber.GetProfileAsync(s, cancellationToken);
                if (result is null)
                {
                    continue;
                }

                yield return result;
            }
        }
    }
}
