using Omnius.Axis.Models.Internal.Entities;
using Omnius.Core.Utils;

namespace Omnius.Axis.Models;

public sealed partial class ServiceConfig
{
    public static ServiceConfig Import(Stream stream)
    {
        var entity = YamlHelper.ReadStream<ServiceConfigEntity>(stream);
        return entity.Export();
    }

    public void Export(Stream stream)
    {
        var entity = ServiceConfigEntity.Import(this);
        YamlHelper.WriteStream(stream, entity);
    }
}
