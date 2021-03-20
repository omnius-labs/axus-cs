using Omnius.Core.Cryptography;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories.Entities
{
    internal record OmniSignatureEntity
    {
        public string? Name { get; set; }

        public OmniHashEntity? Hash { get; set; }

        public static OmniSignatureEntity Import(OmniSignature value)
        {
            return new OmniSignatureEntity() { Name = value.Name, Hash = OmniHashEntity.Import(value.Hash) };
        }

        public OmniSignature Export()
        {
            return new OmniSignature(this.Name!, this.Hash!.Export());
        }
    }
}
