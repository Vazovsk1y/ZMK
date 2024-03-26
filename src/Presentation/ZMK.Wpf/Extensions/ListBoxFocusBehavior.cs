using System.Windows;
using System.Windows.Controls;

namespace ZMK.Wpf.Extensions;

public static class ListBoxFocusBehavior
{
    public static readonly DependencyProperty FocusSelectedItemsProperty =
        DependencyProperty.RegisterAttached("FocusSelectedItems", typeof(bool), typeof(ListBoxFocusBehavior), new PropertyMetadata(false, OnFocusSelectedItemsChanged));

    public static bool GetFocusSelectedItems(DependencyObject obj)
    {
        return (bool)obj.GetValue(FocusSelectedItemsProperty);
    }

    public static void SetFocusSelectedItems(DependencyObject obj, bool value)
    {
        obj.SetValue(FocusSelectedItemsProperty, value);
    }

    private static void OnFocusSelectedItemsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
    {
        if (d is ListBox listBox)
        {
            if ((bool)e.NewValue)
            {
                listBox.SelectionChanged += OnSelectionChanged;
            }
            else
            {
                listBox.SelectionChanged -= OnSelectionChanged;
            }
        }
    }

    private static void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ListBox listBox)
        {
            foreach (var selectedItem in e.AddedItems)
            {
                ListBoxItem listBoxItem = (ListBoxItem)listBox.ItemContainerGenerator.ContainerFromItem(selectedItem);
                if (listBoxItem != null)
                {
                    listBoxItem.Focus();
                }
            }
        }
    }
}