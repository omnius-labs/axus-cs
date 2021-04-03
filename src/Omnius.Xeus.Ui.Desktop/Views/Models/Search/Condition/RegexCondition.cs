using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;

namespace Omnius.Xeus.Ui.Desktop.Views.Models.Search.Condition
{
    public class RegexCondition : IFileSearchCondition<string>, IEquatable<RegexCondition>
    {
        public FileSearchConditionType Type => throw new NotImplementedException();

        public string Value { get; init; } = string.Empty;

        public bool IsIgnoreCase { get; init; }

        private Regex? _regex;

        private Regex GetRegex()
        {
            var o = RegexOptions.Compiled | RegexOptions.Singleline;
            if (this.IsIgnoreCase)
            {
                o |= RegexOptions.IgnoreCase;
            }

            if (_regex == null)
            {
                _regex = new Regex(this.Value, o);
            }

            return _regex;
        }

        public bool IsMatch(string value)
        {
            var ret = this.GetRegex().IsMatch(value);

            return this.Type switch
            {
                FileSearchConditionType.Allow => ret,
                FileSearchConditionType.Deny => !ret,
                _ => throw new NotSupportedException(nameof(this.Type)),
            };
        }

        public override int GetHashCode()
        {
            return this.Value.GetHashCode();
        }

        public override bool Equals(object? obj)
        {
            return this.Equals(obj as RegexCondition);
        }

        public bool Equals(RegexCondition? other)
        {
            if (other is null) return false;
            if (this.Type != other.Type) return false;
            if (this.IsIgnoreCase != other.IsIgnoreCase) return false;
            if (this.Value != other.Value) return false;

            return true;
        }

        public override string ToString()
        {
            var rets = new List<string>
            {
                $"{nameof(this.Type)} = {this.Type}",
                $"{nameof(this.IsIgnoreCase)} = {this.IsIgnoreCase}",
                $"{nameof(this.Value)} = {this.Value}",
            };
            return string.Join(", ", rets);
        }
    }
}
