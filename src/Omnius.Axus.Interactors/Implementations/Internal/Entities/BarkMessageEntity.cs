using Omnius.Axus.Interactors.Internal.Models;
using Omnius.Core.Cryptography;
using Omnius.Core.RocketPack;

namespace Omnius.Axus.Interactors.Internal.Entities;

internal record BarkMessageEntity
{
    public string? Tag { get; set; }

    public DateTime CreatedTime { get; set; }

    public string? Comment { get; set; }

    public OmniHashEntity? AnchorHash { get; set; }

    public static BarkMessageEntity Import(BarkMessage item)
    {
        return new BarkMessageEntity()
        {
            Tag = item.Tag,
            CreatedTime = item.CreatedTime.ToDateTime(),
            Comment = item.Comment,
            AnchorHash = OmniHashEntity.Import(item.AnchorHash),
        };
    }

    public BarkMessage Export()
    {
        return new BarkMessage(
            this.Tag ?? string.Empty,
            Timestamp64.FromDateTime(this.CreatedTime),
            this.Comment ?? string.Empty,
            this.AnchorHash?.Export() ?? OmniHash.Empty);
    }
}
