using System.Globalization;
using Avalonia.Data;
using Avalonia.Data.Converters;
using Omnius.Core.Cryptography;

namespace Omnius.Axus.Ui.Desktop.Converters;

public class OmniDigitalSignatureToStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is OmniDigitalSignature digitalSignature)
        {
            return digitalSignature.ToString();
        }

        return new BindingNotification(new InvalidCastException(), BindingErrorType.Error);
    }

    public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotSupportedException();
    }
}
