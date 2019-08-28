using System.Collections.Generic;

namespace Xeus.Core.Internal.Storage.Primitives
{
    internal interface ISetOperators<T>
    {
        IEnumerable<T> IntersectFrom(IEnumerable<T> collection);
        IEnumerable<T> ExceptFrom(IEnumerable<T> collection);
    }
}
