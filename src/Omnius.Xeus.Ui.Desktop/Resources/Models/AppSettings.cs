using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading.Tasks;
using Omnius.Core.Collections;
using Omnius.Core.Cryptography;
using Omnius.Core.Helpers;
using Omnius.Xeus.Utils;

namespace Omnius.Xeus.Ui.Desktop.Resources.Models
{
    public sealed class AppSettings
    {
        public List<OmniSignature> TrustedSignatures { get; init; } = new List<OmniSignature>();

        public List<OmniSignature> BlockedSignatures { get; init; } = new List<OmniSignature>();

        public int SearchProfileDepth { get; }

        public static async ValueTask<AppSettings?> LoadAsync(string configPath)
        {
            try
            {
                return await JsonHelper.ReadFileAsync<AppSettings>(configPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async ValueTask SaveAsync(string configPath)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(configPath)!);
            await JsonHelper.WriteFileAsync(configPath, this);
        }
    }
}
