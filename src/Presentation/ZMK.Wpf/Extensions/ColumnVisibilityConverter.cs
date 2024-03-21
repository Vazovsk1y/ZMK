using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using ZMK.Wpf.ViewModels;

namespace ZMK.Wpf.Extensions;

public class ColumnVisibilityConverter : IValueConverter
{
    public const string MarkCreateOrModifyEventType = nameof(MarkCreateOrModifyEventType);
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<MarkEventViewModel> collection && parameter is string collectionType && collection.Count > 0)
        {
            return collectionType switch
            {
                MarkCreateOrModifyEventType => collection.All(e => e.EventType == MarkEventViewModel.CreateEventType) || collection.All(e => e.EventType == MarkEventViewModel.ModifyEventType) ? Visibility.Visible : Visibility.Collapsed,
                MarkEventViewModel.CompleteEventType => collection.All(e => e.EventType == collectionType) ? Visibility.Visible : Visibility.Collapsed,
                MarkEventViewModel.CreateEventType => collection.All(e => e.EventType == collectionType)  ? Visibility.Visible : Visibility.Collapsed,
                MarkEventViewModel.ModifyEventType => collection.All(e => e.EventType == collectionType) ? Visibility.Visible : Visibility.Collapsed,
                MarkEventViewModel.CommonEventType => collection.Select(e => e.EventType).Distinct().Count() == 1 ? Visibility.Collapsed : Visibility.Visible,
                _ => Visibility.Collapsed,
            };
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}