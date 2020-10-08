using Omnius.Core.Cryptography;
using Omnius.Xeus.Engines.Storages.Internal.Models;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal sealed class WantContentStatusEntity
    {
        public int Id { get; set; }
        public OmniHashEntity? Hash { get; set; }

        public static WantContentStatusEntity Import(WantContentStatus value)
        {
            return new WantContentStatusEntity() { Hash = OmniHashEntity.Import(value.Hash) };
        }

        public WantContentStatus Export()
        {
            return new WantContentStatus(this.Hash?.Export() ?? OmniHash.Empty);
        }
    }
}
