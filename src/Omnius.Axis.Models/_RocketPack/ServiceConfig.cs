using Omnius.Axis.Models.Internal.Entities;
using Omnius.Core.Utils;

namespace Omnius.Axis.Models;

public sealed partial class ServiceConfig
{
    public static ServiceConfig? LoadFile(string path)
    {
        if (!File.Exists(path)) return null;
        using var fileStream = new FileStream(path, FileMode.Open);
        var entity = YamlHelper.ReadStream<ServiceConfigEntity>(fileStream);
        return entity.Export();
    }

    public void SaveFile(string path)
    {
        var entity = ServiceConfigEntity.Import(this);
        using var fileStream = new FileStream(path, FileMode.OpenOrCreate);
        YamlHelper.WriteStream(fileStream, entity);
    }
}
