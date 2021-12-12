using Omnius.Axis.Engines.Internal.Models;
using Omnius.Core.Cryptography;

namespace Omnius.Axis.Engines.Internal.Entities;

internal record SubscribedFileItemEntity
{
    public OmniHashEntity? RootHash { get; set; }

    public string? Registrant { get; set; }

    public static SubscribedFileItemEntity Import(SubscribedFileItem value)
    {
        return new SubscribedFileItemEntity()
        {
            RootHash = OmniHashEntity.Import(value.RootHash),
            Registrant = value.Registrant,
        };
    }

    public SubscribedFileItem Export()
    {
        return new SubscribedFileItem(this.RootHash?.Export() ?? OmniHash.Empty, this.Registrant ?? string.Empty);
    }
}
