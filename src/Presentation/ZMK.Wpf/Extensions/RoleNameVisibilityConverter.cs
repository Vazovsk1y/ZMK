using System.Globalization;
using System.Windows.Data;
using System.Windows;

namespace ZMK.Wpf.Extensions;

public class RoleNameVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string role && parameter is string requiredRole && requiredRole == role)
        {
            return Visibility.Visible;
        }

        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}