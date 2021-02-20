namespace Omnius.Xeus.Ui.Desktop.Interactors.Models.Primitives
{
    public interface IDropable
    {
        bool TryAdd(object value);

        bool TryRemove(object value);
    }
}
