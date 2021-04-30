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

namespace Omnius.Xeus.Ui.Desktop.Configuration
{
    public sealed class AppSettings
    {
        public string? DaemonAddress { get; init; }

        public List<string> TrustedSignatures { get; init; } = new List<string>();

        public List<string> BlockedSignatures { get; init; } = new List<string>();

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
