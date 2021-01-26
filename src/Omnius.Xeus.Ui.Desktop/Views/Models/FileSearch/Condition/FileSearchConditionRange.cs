using System;
using System.Collections.Generic;

namespace Omnius.Xeus.Ui.Desktop.Views.Models.FileSearch.Condition
{
    public class FileSearchConditionRange<T> : IFileSearchCondition<T>, IEquatable<FileSearchConditionRange<T>>
        where T : IComparable<T>, IEquatable<T>
    {
        public ConditionType Type => throw new NotImplementedException();

        public T? Min { get; init; }

        public T? Max { get; init; }

        public bool IsMatch(T value)
        {
            if (this.Min is null)
            {
                throw new ArgumentNullException(nameof(this.Min));
            }

            if (this.Max is null)
            {
                throw new ArgumentNullException(nameof(this.Max));
            }

            var ret = (this.Min.CompareTo(value) <= 0 && this.Max.CompareTo(value) >= 0);

            return this.Type switch
            {
                ConditionType.Allow => ret,
                ConditionType.Deny => !ret,
                _ => throw new NotSupportedException(nameof(this.Type)),
            };
        }

        public override int GetHashCode()
        {
            return (this.Min?.GetHashCode() ?? 0) ^ (this.Max?.GetHashCode() ?? 0);
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as FileSearchConditionRange<T>);
        }

        public bool Equals(FileSearchConditionRange<T>? other)
        {
            if (other is null)
            {
                return false;
            }

            if (this.Type != other.Type)
            {
                return false;
            }

            if (!(this.Min is null ? other.Min is null : this.Min.Equals(other.Min)))
            {
                return false;
            }

            if (!(this.Max is null ? other.Max is null : this.Max.Equals(other.Max)))
            {
                return false;
            }

            return true;
        }

        public override string ToString()
        {
            var rets = new List<string>
            {
                $"{nameof(this.Type)} = {this.Type}",
                $"{nameof(this.Min)} = {this.Min}",
                $"{nameof(this.Max)} = {this.Max}",
            };
            return string.Join(", ", rets);
        }
    }
}
