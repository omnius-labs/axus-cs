namespace Omnius.Lxna.Ui.Desktop.Presenters.Primitives
{
    public interface IDropable
    {
        bool TryAdd(object value);

        bool TryRemove(object value);
    }
}
