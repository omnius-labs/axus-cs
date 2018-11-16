using System;
using System.Runtime.Serialization;
using Omnius.Base;
using Omnius.Security;
using Omnius.Utils;

namespace Amoeba.Service
{
    partial class NetworkManager
    {
        public readonly struct Node<T> : IEquatable<Node<T>>
        {
            private readonly byte[] _id;
            private readonly T _value;

            public Node(byte[] id, T value)
            {
                _id = id;
                _value = value;
            }

            public byte[] Id
            {
                get
                {
                    return _id;
                }
            }

            public T Value
            {
                get
                {
                    return _value;
                }
            }

            public static bool operator ==(Node<T> x, Node<T> y)
            {
                return x.Equals(y);
            }

            public static bool operator !=(Node<T> x, Node<T> y)
            {
                return !(x == y);
            }

            public override int GetHashCode()
            {
                return (this.Id != null) ? ItemUtils.GetHashCode(this.Id) : 0;
            }

            public override bool Equals(object obj)
            {
                if ((object)obj == null || !(obj is Node<T>)) return false;

                return this.Equals((Node<T>)obj);
            }

            public bool Equals(Node<T> other)
            {
                if ((object)other == null) return false;

                if ((this.Id == null) != (other.Id == null)
                    || !this.Value.Equals(other.Value))
                {
                    return false;
                }

                if (this.Id != null && other.Id != null)
                {
                    if (!Unsafe.Equals(this.Id, other.Id)) return false;
                }

                return true;
            }
        }
    }
}
