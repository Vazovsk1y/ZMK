using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ZMK.Wpf.Services;

public class UserDialogService : IUserDialogService
{
    protected Window? _window;

    public void CloseDialog()
    {
        _window?.Close();
        _window = null;
    }

    public void ShowDialog<T>() where T : Window
    {
        CloseDialog();

        var scope = App.Services.CreateScope();
        _window = scope.ServiceProvider.GetRequiredService<T>();

        _window.Closed += (_, _) =>
        {
            scope.Dispose();
            _window = null;
        };

        _window.ShowDialog();
    }

    public void ShowDialog<T, TViewModel>(TViewModel dataContext) where TViewModel : ObservableObject where T : Window
    {
        CloseDialog();

        var scope = App.Services.CreateScope();
        _window = scope.ServiceProvider.GetRequiredService<T>();
        _window.DataContext = dataContext;
        _window.Closed += (_, _) => scope.Dispose();
        _window.ShowDialog();
    }
}