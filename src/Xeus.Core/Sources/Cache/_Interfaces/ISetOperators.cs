using System.Collections.Generic;
using Amoeba.Messages;

namespace Amoeba.Service
{
    interface ISetOperators<T>
    {
        IEnumerable<T> IntersectFrom(IEnumerable<T> collection);
        IEnumerable<T> ExceptFrom(IEnumerable<T> collection);
    }
}
