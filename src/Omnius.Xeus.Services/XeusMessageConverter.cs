using System;
using System.Diagnostics.CodeAnalysis;
using Omnius.Core;
using Omnius.Core.Serialization;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Services.Models;

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

            using var inHub = new BytesPipe(bytesPool);
            nodeLocation.Export(inHub.Writer, bytesPool);

            using var outHub = new BytesPipe(bytesPool);
            if (!OmniMessageConverter.TryWrite(1, inHub.Reader.GetSequence(), outHub.Writer)) throw new Exception();

            return AddSchemaAndPath(_nodeLocationPath, OmniBase.Encode(outHub.Reader.GetSequence(), ConvertStringType.Base58));
        }

        public static bool TryStringToNodeLocation(string text, [NotNullWhen(true)] out NodeLocation? nodeLocation)
        {
            nodeLocation = null;
            if (!TryRemoveSchemaAndPath(text, _nodeLocationPath, out var value)) return false;

            var bytesPool = BytesPool.Shared;

            using var inHub = new BytesPipe(bytesPool);
            if (!OmniBase.TryDecode(value, inHub.Writer)) return false;

            using var outHub = new BytesPipe(bytesPool);
            if (!OmniMessageConverter.TryRead(inHub.Reader.GetSequence(), out var version, outHub.Writer)) return false;

            nodeLocation = NodeLocation.Import(outHub.Reader.GetSequence(), bytesPool);
            return true;
        }
    }
}
