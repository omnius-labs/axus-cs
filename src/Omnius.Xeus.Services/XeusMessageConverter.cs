using System;
using System.Diagnostics.CodeAnalysis;
using Omnius.Core;
using Omnius.Core.Pipelines;
using Omnius.Core.Serialization;
using Omnius.Xeus.Models;

namespace Omnius.Xeus.Services
{
    public static class XeusMessageConverter
    {
        private static readonly string _schema = "xeus";
        private static readonly string _nodeLocationPath = "node-profile";

        private static string AddSchemaAndPath(string path, string value)
        {
            return $"{_schema}:{path}/{value}";
        }

        private static bool TryRemoveSchemaAndPath(string text, string path, [NotNullWhen(true)] out string? value)
        {
            var targetPrefix = $"{_schema}:{path}/";

            value = null;
            if (!text.StartsWith(targetPrefix)) return false;

            value = text[targetPrefix.Length..];
            return true;
        }

        public static string NodeLocationToString(NodeLocation nodeLocation)
        {
            var bytesPool = BytesPool.Shared;

            using var inBytesPipe = new BytesPipe(bytesPool);
            nodeLocation.Export(inBytesPipe.Writer, bytesPool);

            using var outBytesPipe = new BytesPipe(bytesPool);
            if (!OmniMessageConverter.TryWrite(1, inBytesPipe.Reader.GetSequence(), outBytesPipe.Writer)) throw new Exception();

            return AddSchemaAndPath(_nodeLocationPath, OmniBase.Encode(outBytesPipe.Reader.GetSequence(), ConvertStringType.Base58)!);
        }

        public static bool TryStringToNodeLocation(string text, [NotNullWhen(true)] out NodeLocation? nodeLocation)
        {
            nodeLocation = null;
            if (!TryRemoveSchemaAndPath(text, _nodeLocationPath, out var value)) return false;

            var bytesPool = BytesPool.Shared;

            using var inBytesPipe = new BytesPipe(bytesPool);
            if (!OmniBase.TryDecode(value, inBytesPipe.Writer)) return false;

            using var outBytesPipe = new BytesPipe(bytesPool);
            if (!OmniMessageConverter.TryRead(inBytesPipe.Reader.GetSequence(), out var version, outBytesPipe.Writer)) return false;

            nodeLocation = NodeLocation.Import(outBytesPipe.Reader.GetSequence(), bytesPool);
            return true;
        }
    }
}
