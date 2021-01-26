using System;
using System.Runtime.Serialization;

namespace Omnius.Xeus.Ui.Desktop.Views.Models.FileSearch.Condition
{
    public enum ConditionType
    {
        Allow,
        Deny,
    }

    public interface IFileSearchCondition<T>
    {
        ConditionType Type { get; }

        bool IsMatch(T value);
    }
}
