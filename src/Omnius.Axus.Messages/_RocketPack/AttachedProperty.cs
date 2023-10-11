
using System.Text.Json;

namespace Omnius.Axus.Messages;

public sealed partial class AttachedProperty
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
