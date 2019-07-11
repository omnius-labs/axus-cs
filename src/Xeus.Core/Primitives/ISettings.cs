using System.Threading.Tasks;

namespace Xeus.Core.Primitives
{
    internal interface ISettings
    {
        ValueTask LoadAsync();
        ValueTask SaveAsync();
    }
}
