using System;
using System.Threading.Tasks;

namespace Xeus.Core.Internal.Common
{
    internal interface ISettings
    {
        ValueTask LoadAsync();
        ValueTask SaveAsync();
    }

    internal sealed class SettingsAlreadyLoadedException : Exception
    {
        public SettingsAlreadyLoadedException() { }
        public SettingsAlreadyLoadedException(string message) : base(message) { }
        public SettingsAlreadyLoadedException(string message, Exception innerException) : base(message, innerException) { }
    }
}
