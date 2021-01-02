using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.Storages
{
    public interface IProfilePublisherFactory
    {
        ValueTask<IProfilePublisher> CreateAsync(ProfilePublisherOptions options);
    }

    public class ProfilePublisherOptions
    {
        public string? ConfigDirectoryPath { get; init; }

        public IXeusService? XeusService { get; init; }

        public IBytesPool? BytesPool { get; init; }
    }

    public interface IProfilePublisher
    {
        ValueTask<IEnumerable<OmniSignature>> GetRegisteredSignaturesAsync(CancellationToken cancellationToken = default);

        ValueTask PublishProfileAsync(DateTime creationTime, XeusProfile profile, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default);
    }
}
