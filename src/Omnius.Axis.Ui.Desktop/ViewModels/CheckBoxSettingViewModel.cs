using Omnius.Core.Avalonia;

namespace Omnius.Axis.Ui.Desktop.ViewModels;

public class CheckBoxSettingViewModel : BindableBase
{
    private string? _title;
    private bool _value;
    private string? _description;

    public string? Title
    {
        get => _title;
        set => this.SetProperty(ref _title, value);
    }

    public bool Value
    {
        get => _value;
        set => this.SetProperty(ref _value, value);
    }

    public string? Description
    {
        get => _description;
        set => this.SetProperty(ref _description, value);
    }
}
