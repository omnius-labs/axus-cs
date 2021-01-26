using System;
using System.Buffers;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Api;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Service.Models;

namespace Omnius.Xeus.Service.Interactors
{
    public sealed class UserProfileSubscriber : DisposableBase, IUserProfileSubscriber
    {
        private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly IXeusService _xeusService;
        private readonly IBytesPool _bytesPool;

        private const string Registrant = "Omnius.Xeus.Service.Interactors.UserProfileSubscriber";

        internal sealed class UserProfileSubscriberFactory : IUserProfileSubscriberFactory
        {
            public async ValueTask<IUserProfileSubscriber> CreateAsync(UserProfileSubscriberOptions options)
            {
                var result = new UserProfileSubscriber(options);
                await result.InitAsync();

                return result;
            }
        }

        public UserProfileSubscriber(UserProfileSubscriberOptions options)
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

        public async ValueTask<IEnumerable<OmniSignature>> GetSignaturesAsync(CancellationToken cancellationToken = default)
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var output = await _xeusService.DeclaredMessageSubscriber_GetReportAsync(cancellationToken);
            return output.Report.DeclaredMessageSubscribedItems.Where(n => n.Registrant == Registrant).Select(n => n.Signature).ToArray();
        }

        public async ValueTask SubscribeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var input = new DeclaredMessageSubscriber_SubscribeMessage_Input(signature, Registrant);
            await _xeusService.DeclaredMessageSubscriber_SubscribeMessageAsync(input, cancellationToken);
        }

        public async ValueTask UnsubscribeAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            await Task.Delay(0, cancellationToken).ConfigureAwait(false);

            var input = new DeclaredMessageSubscriber_UnsubscribeMessage_Input(signature, Registrant);
            await _xeusService.DeclaredMessageSubscriber_UnsubscribeMessageAsync(input, cancellationToken);
        }

        public async ValueTask<XeusUserProfile?> GetUserProfileAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            using var declaredMessage = await this.ExportDeclaredMessageAsync(signature, cancellationToken);
            if (declaredMessage is null)
            {
                return null;
            }

            var contentHash = OmniHash.Import(new ReadOnlySequence<byte>(declaredMessage.Value), _bytesPool);
            var content = await this.ExportContentAsync<XeusUserProfileContent>(contentHash, cancellationToken);

            return new XeusUserProfile(signature, declaredMessage.CreationTime, content);
        }

        private async ValueTask<DeclaredMessage?> ExportDeclaredMessageAsync(OmniSignature signature, CancellationToken cancellationToken = default)
        {
            var input = new DeclaredMessageSubscriber_ExportMessage_Input(signature);
            var output = await _xeusService.DeclaredMessageSubscriber_ExportMessageAsync(input, cancellationToken);
            return output.DeclaredMessage;
        }

        private async ValueTask<T> ExportContentAsync<T>(OmniHash contentHash, CancellationToken cancellationToken = default)
            where T : IRocketPackObject<T>
        {
            var input = new ContentSubscriber_ExportContent_Memory_Input(contentHash);
            var output = await _xeusService.ContentSubscriber_ExportContentAsync(input, cancellationToken);
            return IRocketPackObject<T>.Import(new ReadOnlySequence<byte>(output.Memory), _bytesPool);
        }
    }
}
