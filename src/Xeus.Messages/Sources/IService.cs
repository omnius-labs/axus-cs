using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Omnius.Base;
using Omnius.Configuration;
using Omnius.Security;

namespace Amoeba.Messages
{
    public interface IService : ISettings
    {
        ServiceReport Report { get; }
        IEnumerable<NetworkConnectionReport> GetNetworkConnectionReports();
        IEnumerable<CacheContentReport> GetCacheContentReports();
        IEnumerable<DownloadContentReport> GetDownloadContentReports();
        ServiceConfig Config { get; }
        void SetConfig(ServiceConfig config);
        void SetCloudLocations(IEnumerable<Location> locations);
        long Size { get; }
        void Resize(long size);
        Task CheckBlocks(Action<CheckBlocksProgressReport> progress, CancellationToken token);
        Task<Metadata> AddContent(string path, DateTime creationTime, CancellationToken token);
        void RemoveContent(string path);
        void DiffuseContent(string path);
        void AddDownload(Metadata metadata, string path, long maxLength);
        void RemoveDownload(Metadata metadata, string path);
        void ResetDownload(Metadata metadata, string path);
        Task<BroadcastProfileMessage> GetProfile(Signature signature, DateTime? creationTimeLowerLimit, CancellationToken token);
        Task<BroadcastStoreMessage> GetStore(Signature signature, DateTime? creationTimeLowerLimit, CancellationToken token);
        Task<IEnumerable<UnicastCommentMessage>> GetUnicastCommentMessages(Signature signature, AgreementPrivateKey agreementPrivateKey, int messageCountUpperLimit, IEnumerable<MessageCondition> conditions, CancellationToken token);
        Task<IEnumerable<MulticastCommentMessage>> GetMulticastCommentMessages(Tag tag, int trustMessageCountUpperLimit, int untrustMessageCountUpperLimit, IEnumerable<MessageCondition> conditions, CancellationToken token);
        Task SetProfile(ProfileContent profile, DigitalSignature digitalSignature, CancellationToken token);
        Task SetStore(StoreContent store, DigitalSignature digitalSignature, CancellationToken token);
        Task SetUnicastCommentMessage(Signature targetSignature, CommentContent comment, AgreementPublicKey agreementPublicKey, DigitalSignature digitalSignature, CancellationToken token);
        Task SetMulticastCommentMessage(Tag tag, CommentContent comment, DigitalSignature digitalSignature, TimeSpan miningTime, CancellationToken token);
    }
}
