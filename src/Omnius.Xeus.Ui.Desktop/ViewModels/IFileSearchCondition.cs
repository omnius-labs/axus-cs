namespace Omnius.Xeus.Ui.Desktop.ViewModels;

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