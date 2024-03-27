using CommunityToolkit.Mvvm.ComponentModel;
using Microsoft.Extensions.DependencyInjection;
using System.Windows;

namespace ZMK.Wpf.Services;

public class UserDialogService : IUserDialogService
{
    private readonly Stack<Window> _windows = new();

    public void CloseDialog()
    {
        if (_windows.TryPop(out var window))
        {
            window.Close();
        }
    }

    public void ShowDialog<T>() where T : Window
    {
        var scope = App.Services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<T>();

        EventHandler onWindowClose = default!;
        onWindowClose = (_, _) =>
        {
            scope.Dispose();
            _windows.TryPop(out _);
            window.Closed -= onWindowClose;
        };
        window.Closed += onWindowClose;

        _windows.Push(window);
        window.ShowDialog();
    }

    public void ShowDialog<T, TViewModel>(TViewModel dataContext) where TViewModel : ObservableObject where T : Window
    {
        var scope = App.Services.CreateScope();
        var window = scope.ServiceProvider.GetRequiredService<T>();
        window.DataContext = dataContext;

        EventHandler onWindowClose = default!;
        onWindowClose = (_, _) =>
        {
            scope.Dispose();
            _windows.TryPop(out _);
            window.Closed -= onWindowClose;
        };
        window.Closed += onWindowClose;

        _windows.Push(window);
        window.ShowDialog();
    }
}