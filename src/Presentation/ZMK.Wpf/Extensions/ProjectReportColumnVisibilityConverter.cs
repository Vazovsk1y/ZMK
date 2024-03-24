using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using ZMK.Wpf.ViewModels;
using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.Extensions;

public class ProjectReportColumnVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<ObservableObject> collection && parameter is string option && collection.Count > 0)
        {
            return option switch
            {
                ByAreaViewModel.ByAreasOption => collection.All(e => e is ByAreaViewModel) ? Visibility.Visible : Visibility.Collapsed,
                ByExecutorViewModel.ByExecutorsOption => collection.All(e => e is ByExecutorViewModel) ? Visibility.Visible : Visibility.Collapsed,
                _ => throw new KeyNotFoundException()
            };
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}