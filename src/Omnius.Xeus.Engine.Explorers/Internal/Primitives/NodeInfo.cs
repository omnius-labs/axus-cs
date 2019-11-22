using System;
using Omnius.Core;
using Omnius.Core.Helpers;

namespace Xeus.Engine.Internal.Search.Primitives
{
    public readonly struct NodeInfo<T> : IEquatable<NodeInfo<T>>
        where T : notnull
    {
        private readonly byte[] _id;
        private readonly T _value;

        public NodeInfo(byte[] id, T value)
        {
            _id = id;
            _value = value;
        }

        public byte[] Id => _id;
        public T Value => _value;

        public static bool operator ==(NodeInfo<T> x, NodeInfo<T> y)
        {
            return x.Equals(y);
        }

        public static bool operator !=(NodeInfo<T> x, NodeInfo<T> y)
        {
            return !(x == y);
        }

        public override int GetHashCode()
        {
            return (this.Id is null) ? 0 : ObjectHelper.GetHashCode(this.Id);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null || !(obj is NodeInfo<T>)) return false;

            return this.Equals((NodeInfo<T>)obj);
        }

        public bool Equals(NodeInfo<T> other)
        {
            if ((this.Id == null) != (other.Id == null)
                || !this.Value.Equals(other.Value))
            {
                return false;
            }

            if (this.Id != null && other.Id != null)
            {
                if (!BytesOperations.SequenceEqual(this.Id, other.Id)) return false;
            }

            return true;
        }
    }
}
