using System;
using System.Buffers;
using System.Linq;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Interactors.Internal.Models;

namespace Omnius.Xeus.Interactors.Internal.Repositories.Entities
{
    internal record DownloadingUserProfileItemEntity
    {
        public OmniSignatureEntity? Signature { get; set; }

        public OmniHashEntity? ContentHash { get; set; }

        public DateTime CreationTime { get; set; }

        public static DownloadingUserProfileItemEntity Import(DownloadingUserProfileItem value)
        {
            return new DownloadingUserProfileItemEntity()
            {
                Signature = OmniSignatureEntity.Import(value.Signature),
                ContentHash = OmniHashEntity.Import(value.ContentHash),
                CreationTime = value.CreationTime.ToDateTime(),
            };
        }

        public DownloadingUserProfileItem Export()
        {
            return new DownloadingUserProfileItem(this.Signature?.Export() ?? OmniSignature.Empty, this.ContentHash?.Export() ?? OmniHash.Empty, Timestamp.FromDateTime(this.CreationTime));
        }
    }
}
