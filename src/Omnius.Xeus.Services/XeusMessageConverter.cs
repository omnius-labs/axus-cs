using System.Diagnostics.CodeAnalysis;
using Omnius.Core;
using Omnius.Core.Serialization;
using Omnius.Xeus.Engines.Models;
using Omnius.Xeus.Services.Models;

namespace Omnius.Xeus.Services
{
    public static class XeusMessageConverter
    {
        private static readonly string _prefix = "xeus:";

        private static string AddPrefix(string value)
        {
            return _prefix + value;
        }

        private static bool TryRemovePrefix(string text, [NotNullWhen(true)] out string? value)
        {
            value = null;
            if (!text.StartsWith(_prefix)) return false;

            value = text[_prefix.Length..];
            return true;
        }

        public static string NodeProfileToString(NodeProfile nodeProfile)
        {
            var bytesPool = BytesPool.Shared;

            using var inHub = new BytesHub(bytesPool);
            nodeProfile.Export(inHub.Writer, bytesPool);

            using var outHub = new BytesHub(bytesPool);
            OmniMessageConverter.Write(1, inHub.Reader.GetSequence(), outHub.Writer);

            return AddPrefix(OmniBase.Encode(outHub.Reader.GetSequence(), ConvertStringType.Base58));
        }

        public static bool TryStringToNodeProfile(string text, [NotNullWhen(true)] out NodeProfile? nodeProfile)
        {
            nodeProfile = null;
            if (!TryRemovePrefix(text, out var value)) return false;

            var bytesPool = BytesPool.Shared;

            using var inHub = new BytesHub(bytesPool);
            if (!OmniBase.TryDecode(value, inHub.Writer)) return false;

            using var outHub = new BytesHub(bytesPool);
            OmniMessageConverter.Read(inHub.Reader.GetSequence(), out var version, outHub.Writer);

            nodeProfile = NodeProfile.Import(outHub.Reader.GetSequence(), bytesPool);
            return true;
        }
    }
}
