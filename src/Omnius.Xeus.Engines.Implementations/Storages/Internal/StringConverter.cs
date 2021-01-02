using Omnius.Core;
using Omnius.Core.Cryptography;
using Omnius.Core.Cryptography.Functions;
using Omnius.Core.Serialization;

namespace Omnius.Xeus.Engines.Storages.Internal.Repositories
{
    internal static class StringConverter
    {
        public static string HashToString(OmniHash hash)
        {
            return hash.ToString(ConvertStringType.Base16);
        }

        public static string SignatureToString(OmniSignature signature)
        {
            using var hub = new BytesHub(BytesPool.Shared);
            signature.Export(hub.Writer, BytesPool.Shared);
            var hash = new OmniHash(OmniHashAlgorithmType.Sha2_256, Sha2_256.ComputeHash(hub.Reader.GetSequence()));
            return hash.ToString(ConvertStringType.Base16);
        }
    }
}
