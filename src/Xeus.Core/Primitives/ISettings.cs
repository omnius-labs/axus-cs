using System.Threading.Tasks;

namespace Xeus.Core.Primitives
{
    public interface ISettings
    {
        ValueTask LoadAsync();
        ValueTask SaveAsync();
    }
}
