using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;
using System.Windows;
using ZMK.Wpf.ViewModels;

namespace ZMK.Wpf.Extensions;

public class MarkEventsColumnVisibilityConverter : IValueConverter
{
    public const string MarkCreateOrModifyEventType = nameof(MarkCreateOrModifyEventType);

    public const string MarkCompleteOrCommonEventType = nameof(MarkCompleteOrCommonEventType);
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is ObservableCollection<MarkEventViewModel> collection && parameter is string eventType && collection.Count > 0)
        {
            return eventType switch
            {
                MarkCreateOrModifyEventType => 
                collection.All(e => e.EventType == MarkEventViewModel.CreateEventType && e.DisplayEventType != MarkEventViewModel.CommonEventType) 
                || collection.Any(e => e.EventType == MarkEventViewModel.ModifyEventType && e.DisplayEventType != MarkEventViewModel.CommonEventType) ? Visibility.Visible : Visibility.Collapsed,
                MarkEventViewModel.CompleteEventType => collection.All(e => e.EventType == eventType && e.DisplayEventType == MarkEventViewModel.CompleteEventType) ? Visibility.Visible : Visibility.Collapsed,
                MarkEventViewModel.CreateEventType => collection.All(e => e.EventType == eventType && e.DisplayEventType == MarkEventViewModel.CreateEventType)  ? Visibility.Visible : Visibility.Collapsed,
                MarkEventViewModel.ModifyEventType => collection.All(e => e.EventType == eventType && e.DisplayEventType == MarkEventViewModel.ModifyEventType) ? Visibility.Visible : Visibility.Collapsed,
                MarkEventViewModel.CommonEventType => collection.All(e => e.DisplayEventType == MarkEventViewModel.CommonEventType) ? Visibility.Visible : Visibility.Collapsed,
                MarkCompleteOrCommonEventType => collection.All(e => e.DisplayEventType == MarkEventViewModel.CommonEventType) 
                || collection.All(e => e.EventType == MarkEventViewModel.CompleteEventType && e.DisplayEventType == MarkEventViewModel.CompleteEventType) ? Visibility.Visible : Visibility.Collapsed,
                _ => throw new KeyNotFoundException(),
            };
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}