using System;
using System.IO;
using System.Threading.Tasks;
using Omnius.Core.Avalonia;
using Omnius.Core.Helpers;
using Omnius.Core.Utils;

namespace Omnius.Xeus.Ui.Desktop.Configuration
{
    public sealed partial class UiState
    {
        public DynamicState DownloadControl_DynamicState { get; } = new();

        public DynamicState UploadControl_DynamicState { get; } = new();

        public static async ValueTask<UiState?> LoadAsync(string configPath)
        {
            try
            {
                return await JsonHelper.ReadFileAsync<UiState>(configPath);
            }
            catch (Exception)
            {
                return null;
            }
        }

        public async ValueTask SaveAsync(string configPath)
        {
            DirectoryHelper.CreateDirectory(Path.GetDirectoryName(configPath)!);
            await JsonHelper.WriteFileAsync(configPath, this, true);
        }
    }
}
