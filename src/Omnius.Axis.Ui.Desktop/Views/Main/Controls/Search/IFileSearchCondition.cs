namespace Omnius.Axis.Ui.Desktop.Views.Main;

public enum FileSearchConditionType
{
    Allow,
    Deny,
}

public interface IFileSearchCondition<T>
{
    FileSearchConditionType Type { get; }

    bool IsMatch(T value);
}
