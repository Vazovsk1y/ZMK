using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.Extensions.DependencyInjection;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.ViewModels.Base;

namespace ZMK.Wpf.ViewModels.Employee;

public partial class EmployeeAddViewModel : DialogViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _fullName = null!;

    [ObservableProperty]
    private string? _post = null!;

    [ObservableProperty]
    private string? _remark = null!;
    public EmployeeAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
        ControlTitle = "Добавление сотрудника";
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var employeeService = scope.ServiceProvider.GetRequiredService<IEmployeeService>();
        var result = await employeeService.AddAsync(new EmployeeAddDTO(FullName, Post, Remark));
        if (result.IsSuccess)
        {
            _dialogService.CloseDialog();
            Messenger.Send(new EmployeeAddedMessage(this.ToPanelViewModel(result.Value)));
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
    }

    protected override bool CanAccept(object p)
    {
        return !string.IsNullOrWhiteSpace(FullName);
    }
}