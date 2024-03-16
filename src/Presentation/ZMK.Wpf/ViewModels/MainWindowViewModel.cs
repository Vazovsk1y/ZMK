using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace ZMK.Wpf.ViewModels;

public partial class MainWindowViewModel : TitledViewModel
{
    public StatusPanelViewModel StatusPanelViewModel { get; }

    public UsersPanelViewModel UsersPanelViewModel { get; }

    public EmployeesPanelViewModel EmployeesPanelViewModel { get; }

    public ProjectsPanelViewModel ProjectsPanelViewModel { get; }

    public AreasPanelViewModel AreasPanelViewModel { get; }

    [ObservableProperty]
    public IRefrashable? _selectedMenuItem;

    private CancellationTokenSource? _tokenSource;

    public MainWindowViewModel(
        StatusPanelViewModel statusPanelViewModel,
        UsersPanelViewModel usersPanelViewModel,
        EmployeesPanelViewModel employeesPanelViewModel,
        ProjectsPanelViewModel projectsPanelViewModel,
        AreasPanelViewModel areasPanelViewModel)
    {
        ControlTitle = App.Title;
        StatusPanelViewModel = statusPanelViewModel;
        UsersPanelViewModel = usersPanelViewModel;
        EmployeesPanelViewModel = employeesPanelViewModel;
        ProjectsPanelViewModel = projectsPanelViewModel;
        AreasPanelViewModel = areasPanelViewModel;

        ActivateAllRecipients();
    }

    public MainWindowViewModel()
    {
        ControlTitle = App.Title;
    }

    [RelayCommand]
    public async Task MenuItemChanged(object param)
    {
        if (param is not IRefrashable refrashable)
        {
            return;
        }

        _tokenSource?.Cancel();
        _tokenSource ??= new();

        try
        {
            if (MenuItemChangedCommand.IsRunning)
            {
                MenuItemChangedCommand.Cancel();
            }

            await refrashable.RefreshAsync(_tokenSource.Token).ConfigureAwait(false);

            App.Current.Dispatcher.Invoke(() => SelectedMenuItem = refrashable);
        }
        catch (OperationCanceledException)
        {
            _tokenSource?.Dispose();
            _tokenSource = null;
            return;
        }

        _tokenSource?.Dispose();
        _tokenSource = null;
    }
}