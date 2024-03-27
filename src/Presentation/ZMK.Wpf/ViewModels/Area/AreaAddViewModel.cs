using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.ViewModels.Base;

namespace ZMK.Wpf.ViewModels.Area;

public partial class AreaAddViewModel : DialogViewModel
{
    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    public string _title = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    public int _order;

    [ObservableProperty]
    public string? _remark;

    public AreaAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
        ControlTitle = "Добавление участка";
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var areaService = scope.ServiceProvider.GetRequiredService<IAreaService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var result = await areaService.AddAsync(new AreaAddDTO(Order, Title, Remark));
        if (result.IsSuccess)
        {
            var addedArea = await dbContext
                .Areas
                .AsNoTracking()
                .SingleAsync(e => e.Id == result.Value);

            App.Current.Dispatcher.Invoke(() => _dialogService.CloseDialog());
            Messenger.Send(new AreaAddedMessage(addedArea.ToViewModel()));
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
    }

    protected override bool CanAccept(object p) => Order > 0 && !string.IsNullOrWhiteSpace(Title);
}
