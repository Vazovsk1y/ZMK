﻿using System.Windows;

namespace ZMK.Wpf.Extensions;

public static class MessageBoxHelper
{
    public static void ShowErrorBox(string message, string caption = "Error")
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Error);
    }

    public static void ShowInfoBox(string message, string caption = "Info")
    {
        MessageBox.Show(message, caption, MessageBoxButton.OK, MessageBoxImage.Information);
    }

    public static MessageBoxResult ShowDialogBoxYesNo(string message, string caption = "Question")
    {
        return MessageBox.Show(message, caption, MessageBoxButton.YesNo, MessageBoxImage.Question);
    }
}
