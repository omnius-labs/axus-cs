namespace Omnius.Xeus.Ui.Desktop.Views.Models.Primitives
{
    public interface IDropable
    {
        bool TryAdd(object value);

        bool TryRemove(object value);
    }
}
