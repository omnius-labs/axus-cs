using System;
using System.Buffers;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Extensions;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Api;
using Omnius.Xeus.Api.Models;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Interactors.Models;
using Omnius.Xeus.Interactors.Storages.Internal.Repositories;

namespace Omnius.Xeus.Interactors.Storages
{
    public sealed class ProfilePublisher : AsyncDisposableBase, IProfilePublisher
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        internal sealed class ProfilePublisherFactory : IProfilePublisherFactory
        {
            public async ValueTask<IProfilePublisher> CreateAsync(ProfilePublisherOptions options)
            {
                var result = new ProfilePublisher(options);
                await result.InitAsync();

                return result;
            }
        }

        public ProfilePublisher(ProfilePublisherOptions options)
        {
            _xeusService = options.XeusService ?? throw new ArgumentNullException(nameof(options.XeusService));
            _bytesPool = options.BytesPool ?? BytesPool.Shared;
        }

        public async ValueTask InitAsync()
        {
        }

        protected override async ValueTask OnDisposeAsync()
        {
        }

        public async ValueTask<IEnumerable<OmniSignature>> GetRegisteredSignaturesAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var results = new List<OmniSignature>();

            foreach (var report in await this.GetPushDeclaredMessagesReportAsync(cancellationToken))
            {
                results.Add(report.Signature);
            }

            return results;
        }

        private async Task<IEnumerable<PushDeclaredMessageReport>> GetPushDeclaredMessagesReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _xeusService.GetPushDeclaredMessagesReportAsync(cancellationToken);
            return result.PushDeclaredMessages;
        }

        public async ValueTask PublishProfileAsync(DateTime creationTime, XeusProfile profile, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesHub();
            profile.Export(hub.Writer, _bytesPool);

            var sequence = hub.Reader.GetSequence();
            var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            sequence.CopyTo(memoryOwner.Memory.Span);

            using var declaredMessage = DeclaredMessage.Create(Timestamp.FromDateTime(creationTime), memoryOwner, digitalSignature);
            var param = new RegisterPushDeclaredMessageParam(declaredMessage);
            await _xeusService.RegisterPushDeclaredMessageAsync(param, cancellationToken);
        }

        public async ValueTask UnpublishProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var param = new UnregisterPushDeclaredMessageParam(signature);
            await _xeusService.UnregisterPushDeclaredMessageAsync(param, cancellationToken);
        }
    }
}
