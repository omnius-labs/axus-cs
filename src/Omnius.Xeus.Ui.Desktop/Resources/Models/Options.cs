using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;

namespace Omnius.Xeus.Ui.Desktop.Resources.Models
{
    public sealed class Options
    {
        public Options(IEnumerable<OmniSignature>? trustedSignatures = null, IEnumerable<OmniSignature>? blockedSignatures = null, int searchProfileDepth = 0)
        {
            this.TrustedSignatures = new ReadOnlyListSlim<OmniSignature>(trustedSignatures?.ToArray() ?? Array.Empty<OmniSignature>());
            this.BlockedSignatures = new ReadOnlyListSlim<OmniSignature>(blockedSignatures?.ToArray() ?? Array.Empty<OmniSignature>());
            this.SearchProfileDepth = searchProfileDepth;
        }

        public ReadOnlyListSlim<OmniSignature> TrustedSignatures { get; }

        public ReadOnlyListSlim<OmniSignature> BlockedSignatures { get; }

        public int SearchProfileDepth { get; }

        public static async ValueTask<Options?> LoadAsync(string configPath)
        {
            try
            {
                using var stream = new FileStream(configPath, FileMode.Open);
                var serializeOptions = new JsonSerializerOptions();
                return await JsonSerializer.DeserializeAsync<Options>(stream, serializeOptions);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async ValueTask SaveAsync(string configPath)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(configPath)!);

            using var stream = new FileStream(configPath, FileMode.Create);
            var serializeOptions = new JsonSerializerOptions() { WriteIndented = true };
            await JsonSerializer.SerializeAsync(stream, this, serializeOptions);
        }
    }
}
