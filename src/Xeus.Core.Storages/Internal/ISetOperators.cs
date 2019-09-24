using System.Collections.Generic;

namespace Xeus.Core.Storage.Internal
{
    internal interface ISetOperators<T>
    {
        IEnumerable<T> IntersectFrom(IEnumerable<T> collection);
        IEnumerable<T> ExceptFrom(IEnumerable<T> collection);
    }
}
