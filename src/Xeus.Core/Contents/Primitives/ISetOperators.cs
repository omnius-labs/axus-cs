using System.Collections.Generic;
using Amoeba.Messages;

namespace Xeus.Core.Contents.Primitives
{
    interface ISetOperators<T>
    {
        IEnumerable<T> IntersectFrom(IEnumerable<T> collection);
        IEnumerable<T> ExceptFrom(IEnumerable<T> collection);
    }
}
