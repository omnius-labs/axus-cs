using System;
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
        public UserProfileFinderOptions(IEnumerable<OmniSignature> trustedSignatures, IEnumerable<OmniSignature> blockedSignatures, int searchDepth, string configDirectoryPath,
            IUserProfileSubscriber userProfileSubscriber, IBytesPool bytesPool)
        {
            this.TrustedSignatures = new ReadOnlyListSlim<OmniSignature>(trustedSignatures.ToArray());
            this.BlockedSignatures = new ReadOnlyListSlim<OmniSignature>(blockedSignatures.ToArray());
            this.SearchDepth = searchDepth;
            this.ConfigDirectoryPath = configDirectoryPath;
            this.UserProfileSubscriber = userProfileSubscriber;
            this.BytesPool = bytesPool;
        }

        public IReadOnlyList<OmniSignature> TrustedSignatures { get; }

        public IReadOnlyList<OmniSignature> BlockedSignatures { get; }

        public int SearchDepth { get; }

        public string ConfigDirectoryPath { get; }

        public IUserProfileSubscriber UserProfileSubscriber { get; }

        public IBytesPool BytesPool { get; }
    }

    public interface IUserProfileFinder : IAsyncDisposable
    {
        ValueTask<XeusUserProfile?> GetUserProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default);

        ValueTask<IEnumerable<XeusUserProfile>> GetUserProfilesAsync(CancellationToken cancellationToken = default);
    }
}
