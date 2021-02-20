namespace Omnius.Xeus.Ui.Desktop.Interactors.Models.FileSearch.Condition
{
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
}
