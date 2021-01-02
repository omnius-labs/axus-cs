using System;
using System.Buffers;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Xeus.Api;
using Omnius.Xeus.Api.Models;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Interactors.Models;

namespace Omnius.Xeus.Interactors.Storages
{
    public sealed class ProfileSubscriber : DisposableBase, IProfileSubscriber
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        internal sealed class ProfileSubscriberFactory : IProfileSubscriberFactory
        {
            public async ValueTask<IProfileSubscriber> CreateAsync(ProfileSubscriberOptions options)
            {
                var result = new ProfileSubscriber(options);
                await result.InitAsync();

                return result;
            }
        }

        public ProfileSubscriber(ProfileSubscriberOptions options)
        {
            _xeusService = options.XeusService ?? throw new ArgumentNullException(nameof(options.XeusService));
            _bytesPool = options.BytesPool ?? BytesPool.Shared;
        }

        public async ValueTask InitAsync()
        {
        }

        protected override void OnDispose(bool disposing)
        {
        }

        public async ValueTask<IEnumerable<OmniSignature>> GetRegisteredSignaturesAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var results = new List<OmniSignature>();

            foreach (var report in await this.GetWantDeclaredMessagesReportAsync(cancellationToken))
            {
                results.Add(report.Signature);
            }

            return results;
        }

        private async Task<IEnumerable<WantDeclaredMessageReport>> GetWantDeclaredMessagesReportAsync(CancellationToken cancellationToken = default)
        {
            var result = await _xeusService.GetWantDeclaredMessagesReportAsync(cancellationToken);
            return result.WantDeclaredMessages;
        }

        public async ValueTask RegisterSignaturesAsync(IEnumerable<OmniSignature> signatures, CancellationToken cancellationToken = default)
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            foreach (var signature in signatures)
            {
                var param = new RegisterWantDeclaredMessageParam(signature);
                await _xeusService.RegisterWantDeclaredMessageAsync(param, cancellationToken);
            }
        }

        public async ValueTask UnregisterSignaturesAsync(IEnumerable<OmniSignature> signatures, CancellationToken cancellationToken = default)
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            foreach (var signature in signatures)
            {
                var param = new UnregisterWantDeclaredMessageParam(signature);
                await _xeusService.UnregisterWantDeclaredMessageAsync(param, cancellationToken);
            }
        }

        public async ValueTask<ProfileSubscriberGetProfileResult> GetProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using var declaredMessage = await this.ExportWantDeclaredMessageAsync(signature, cancellationToken);
            var xeusProfile = XeusProfile.Import(new ReadOnlySequence<byte>(declaredMessage.Value), _bytesPool);

            return new ProfileSubscriberGetProfileResult() { CreationTime = declaredMessage.CreationTime.ToDateTime(), Profile = xeusProfile };
        }

        private async ValueTask<DeclaredMessage> ExportWantDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var param = new ExportWantDeclaredMessageParam(signature);
            var result = await _xeusService.ExportWantDeclaredMessageAsync(param, cancellationToken);
            return result.DeclaredMessage;
        }
    }
}
