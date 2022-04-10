namespace Omnius.Axis.Ui.Desktop.Windows.Main;

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
