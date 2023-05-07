using Omnius.Axus.Messages;

namespace Omnius.Axus.Engines.Internal.Entities;

internal record AttachedPropertyEntity
{
    public string? Name { get; set; }
    public byte[]? Value { get; set; }

    public static AttachedPropertyEntity Import(AttachedProperty item)
    {
        return new AttachedPropertyEntity()
        {
            Name = item.Name,
            Value = item.Value.ToArray(),
        };
    }

    public AttachedProperty Export()
    {
        return new AttachedProperty(this.Name ?? string.Empty, this.Value);
    }
}
