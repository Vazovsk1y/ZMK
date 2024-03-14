using CommunityToolkit.Mvvm.ComponentModel;
using System.Windows;

namespace ZMK.Wpf.Services;

public interface IUserDialogService
{
    void ShowDialog<T>() where T : Window;

    void CloseDialog();

    void ShowDialog<T, TViewModel>(TViewModel dataContext) where TViewModel : ObservableObject where T : Window;
}