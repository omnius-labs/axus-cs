using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Service.Interactors;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Presenters
{
    public interface IUserProfileFinderFactory
    {
        ValueTask<IUserProfileFinder> CreateAsync(UserProfileFinderOptions options);
    }

    public class UserProfileFinderOptions
    {
        public string? ConfigDirectoryPath { get; init; }

        public IUserProfileSubscriber? UserProfileSubscriber { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IUserProfileFinder
    {
        UserProfileFinderConfig Config { get; }

        ValueTask SetConfigAsync(UserProfileFinderConfig config, CancellationToken cancellationToken = default);

        ValueTask<XeusUserProfile?> GetUserProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<XeusUserProfile>?> GetUserProfilesAsync(CancellationToken cancellationToken = default);
    }

    public class UserProfileFinderConfig
    {
        public UserProfileFinderConfig(IEnumerable<OmniSignature> trustedSignatures, IEnumerable<OmniSignature> blockedSignatures, int searchDepth)
        {
            this.TrustedSignatures = new ReadOnlyListSlim<OmniSignature>(trustedSignatures.ToArray());
            this.BlockedSignatures = new ReadOnlyListSlim<OmniSignature>(blockedSignatures.ToArray());
            this.SearchDepth = searchDepth;
        }

        public IReadOnlyList<OmniSignature> TrustedSignatures { get; }

        public IReadOnlyList<OmniSignature> BlockedSignatures { get; }

        public int SearchDepth { get; }
    }
}
