using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Extensions;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Api;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Interactors
{
    public sealed class UserProfilePublisher : AsyncDisposableBase, IUserProfilePublisher
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        private const string Registrant = "Omnius.Xeus.Service.Interactors.UserProfilePublisher";

        internal sealed class UserProfilePublisherFactory : IUserProfilePublisherFactory
        {
            public async ValueTask<IUserProfilePublisher> CreateAsync(UserProfilePublisherOptions options)
            {
                var result = new UserProfilePublisher(options);
                await result.InitAsync();

                return result;
            }
        }

        public static IUserProfilePublisherFactory Factory { get; } = new UserProfilePublisherFactory();

        public UserProfilePublisher(UserProfilePublisherOptions options)
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

        public async ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var output = await _xeusService.DeclaredMessagePublisher_GetReportAsync(cancellationToken);
            return output.Report.DeclaredMessagePublishedItems.Where(n => n.Registrant == Registrant).Select(n => n.Signature).ToArray();
        }

        public async ValueTask PublishAsync(XeusUserProfileContent content, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            var contentHash = await this.InternalPublishContentAsync(content, cancellationToken);
            await this.InternalPublishDeclaredMessageAsync(contentHash, digitalSignature, cancellationToken);
        }

        private async ValueTask<OmniHash> InternalPublishContentAsync<T>(T content, CancellationToken cancellationToken = default)
            where T : IRocketPackObject<T>
        {
            using var hub = new BytesHub();
            content.Export(hub.Writer, _bytesPool);

            var sequence = hub.Reader.GetSequence();
            using var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            sequence.CopyTo(memoryOwner.Memory.Span);

            var input = new ContentPublisher_PublishContent_Memory_Input(memoryOwner.Memory, Registrant);
            var output = await _xeusService.ContentPublisher_PublishContentAsync(input, cancellationToken);
            return output.Hash;
        }

        private async ValueTask InternalPublishDeclaredMessageAsync(OmniHash contentHash, OmniDigitalSignature digitalSignature, CancellationToken cancellationToken = default)
        {
            using var hub = new BytesHub();
            contentHash.Export(hub.Writer, _bytesPool);

            var sequence = hub.Reader.GetSequence();
            var memoryOwner = _bytesPool.Memory.Rent((int)sequence.Length).Shrink((int)sequence.Length);
            sequence.CopyTo(memoryOwner.Memory.Span);

            using var declaredMessage = DeclaredMessage.Create(Timestamp.FromDateTime(DateTime.UtcNow), memoryOwner, digitalSignature);
            var input = new DeclaredMessagePublisher_PublishMessage_Input(declaredMessage, Registrant);
            await _xeusService.DeclaredMessagePublisher_PublishMessageAsync(input, cancellationToken);
        }

        public async ValueTask UnpublishAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            await this.UnpublishIndexAsync(signature, cancellationToken);
        }

        private async ValueTask UnpublishIndexAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new DeclaredMessagePublisher_UnpublishMessage_Input(signature, Registrant);
            await _xeusService.DeclaredMessagePublisher_UnpublishMessageAsync(input, cancellationToken);
        }

        private async ValueTask UnpublishFilesAsync(OmniHash contentHash, CancellationToken cancellationToken = default)
        {
            var input = new ContentPublisher_UnpublishContent_Memory_Input(contentHash, Registrant);
            await _xeusService.ContentPublisher_UnpublishContentAsync(input, cancellationToken);
        }
    }
}
