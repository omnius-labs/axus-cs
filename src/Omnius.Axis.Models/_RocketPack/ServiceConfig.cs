using Omnius.Axis.Models.Internal.Entities;
using Omnius.Core.Utils;

namespace Omnius.Axis.Models;

public sealed partial class ServiceConfig
{
    private static readonly NLog.Logger _logger = NLog.LogManager.GetCurrentClassLogger();

    public static ServiceConfig? LoadFile(string path)
    {
        try
        {
            if (!File.Exists(path)) return null;
            using var fileStream = new FileStream(path, FileMode.Open);
            var entity = YamlHelper.ReadStream<ServiceConfigEntity>(fileStream);
            return entity.Export();
        }
        catch (Exception e)
        {
            _logger.Error(e);
            return null;
        }
    }

    public void SaveFile(string path)
    {
        var entity = ServiceConfigEntity.Import(this);
        using var fileStream = new FileStream(path, FileMode.Truncate);
        YamlHelper.WriteStream(fileStream, entity);
    }
}
