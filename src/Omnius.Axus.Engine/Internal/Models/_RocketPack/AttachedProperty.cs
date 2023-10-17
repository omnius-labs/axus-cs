
using System.Text.Json;

namespace Omnius.Axus.Core.Engine.Models;

internal sealed partial class AttachedProperty
{
    public static AttachedProperty Create<T>(T value)
    {
        return new AttachedProperty(JsonSerializer.Serialize(value));
    }

    public T GetValue<T>()
    {
        return JsonSerializer.Deserialize<T>(this.Value)!;
    }
}
