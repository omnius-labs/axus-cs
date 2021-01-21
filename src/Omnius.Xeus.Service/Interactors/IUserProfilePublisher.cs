using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Interactors
{
    public interface IUserProfilePublisherFactory
    {
        ValueTask<IUserProfilePublisher> CreateAsync(UserProfilePublisherOptions options);
    }

    public class UserProfilePublisherOptions
    {
        public string? ConfigDirectoryPath { get; init; }

        public IXeusService? XeusService { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IUserProfilePublisher
    {
        ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default);

        ValueTask PublishAsync(XeusUserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default);

        ValueTask UnpublishAsync(OmniSignature signature, CancellationToken cancellationToken = default);
    }
}
