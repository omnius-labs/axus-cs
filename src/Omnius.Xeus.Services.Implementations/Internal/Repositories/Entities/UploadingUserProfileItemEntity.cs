using System;
using System.Buffers;
using System.Linq;
using LiteDB;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;
using Omnius.Xeus.Services.Internal.Models;

namespace Omnius.Xeus.Services.Internal.Repositories.Entities
{
    internal record UploadingUserProfileItemEntity
    {
        public OmniSignatureEntity? Signature { get; set; }

        public OmniHashEntity? RootHash { get; set; }

        public DateTime CreationTime { get; set; }

        public static UploadingUserProfileItemEntity Import(UploadingUserProfileItem value)
        {
            return new UploadingUserProfileItemEntity()
            {
                Signature = OmniSignatureEntity.Import(value.Signature),
                RootHash = OmniHashEntity.Import(value.RootHash),
                CreationTime = value.CreationTime.ToDateTime(),
            };
        }

        public UploadingUserProfileItem Export()
        {
            return new UploadingUserProfileItem(this.Signature?.Export() ?? OmniSignature.Empty, this.RootHash?.Export() ?? OmniHash.Empty, Timestamp.FromDateTime(this.CreationTime));
        }
    }
}
