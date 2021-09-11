namespace Omnius.Xeus.Ui.Desktop.Models
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
