using Omnius.Axus.Messages;

namespace Omnius.Axus.Core.Engine.Repositories.Models;

internal record CachedNodeLocation
{
    public required NodeLocation Value { get; init; }
    public required DateTime CreatedTime { get; init; }
    public required DateTime UpdatedTime { get; init; }
}
