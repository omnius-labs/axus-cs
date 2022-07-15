using System.Diagnostics.CodeAnalysis;
using Omnius.Axis.Interactors.Models;
using Omnius.Axis.Models;
using Omnius.Core;
using Omnius.Core.Pipelines;
using Omnius.Core.RocketPack;
using Omnius.Core.Serialization;

namespace Omnius.Axis.Interactors;

public static class AxisMessageConverter
{
    private static readonly string _schema = "axis";
    private static readonly string _nodePath = "node";
    private static readonly string _seedPath = "seed";

    public static string NodeToString(NodeLocation message)
    {
        return MessageToString<NodeLocation>(_nodePath, 1, message);
    }

    public static bool TryStringToNode(string text, [NotNullWhen(true)] out NodeLocation? message)
    {
        message = null;
        return TryStringToMessage<NodeLocation>(_nodePath, 1, text, out message);
    }

    public static string FileSeedToString(FileSeed message)
    {
        return MessageToString<FileSeed>(_seedPath, 1, message);
    }

    public static bool TryStringToFileSeed(string text, [NotNullWhen(true)] out FileSeed? message)
    {
        message = null;
        return TryStringToMessage<FileSeed>(_seedPath, 1, text, out message);
    }

    private static string MessageToString<T>(string path, int version, T message)
        where T : IRocketMessage<T>
    {
        var bytesPool = BytesPool.Shared;

        using var inBytesPipe = new BytesPipe(bytesPool);
        message.Export(inBytesPipe.Writer, bytesPool);

        using var outBytesPipe = new BytesPipe(bytesPool);
        if (!OmniMessageConverter.TryWrite(inBytesPipe.Reader.GetSequence(), outBytesPipe.Writer)) throw new Exception();

        return AddSchemaAndPath(path, version, OmniBase.Encode(outBytesPipe.Reader.GetSequence(), ConvertStringType.Base58)!);
    }

    private static bool TryStringToMessage<T>(string path, int version, string text, [NotNullWhen(true)] out T? message)
        where T : IRocketMessage<T>
    {
        message = default!;

        if (!TryRemoveSchemaAndPath(text, path, version, out var value)) return false;

        var bytesPool = BytesPool.Shared;

        using var inBytesPipe = new BytesPipe(bytesPool);
        if (!OmniBase.TryDecode(value, inBytesPipe.Writer)) return false;

        using var outBytesPipe = new BytesPipe(bytesPool);
        if (!OmniMessageConverter.TryRead(inBytesPipe.Reader.GetSequence(), outBytesPipe.Writer)) return false;

        message = IRocketMessage<T>.Import(outBytesPipe.Reader.GetSequence(), bytesPool);
        return true;
    }

    private static string AddSchemaAndPath(string path, int version, string value)
    {
        return $"{_schema}:{path}/v{version}/{value}";
    }

    private static bool TryRemoveSchemaAndPath(string text, string path, int version, [NotNullWhen(true)] out string? value)
    {
        var targetPrefix = $"{_schema}:{path}/v{version}/";

        value = null;
        if (!text.StartsWith(targetPrefix)) return false;

        value = text[targetPrefix.Length..];
        return true;
    }
}
