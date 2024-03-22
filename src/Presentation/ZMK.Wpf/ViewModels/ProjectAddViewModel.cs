using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using static ZMK.Wpf.ViewModels.ProjectSettingsUpdateViewModel;

namespace ZMK.Wpf.ViewModels;

public partial class ProjectAddViewModel : DialogViewModel
{
    public ObservableCollection<SelectableAreaViewModel> Areas { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _factoryNumber = null!;

    [ObservableProperty]
    private string? _contractNumber;

    [ObservableProperty]
    private string? _customer;

    [ObservableProperty]
    private string? _vendor;

    [ObservableProperty]
    private string? _remark;

    [ObservableProperty]
    public bool _isEditable;

    [ObservableProperty]
    public bool _allowMarksDeleting;

    [ObservableProperty]
    public bool _allowMarksModifying;

    [ObservableProperty]
    public bool _allowMarksAdding;

    [ObservableProperty]
    public bool _areExecutorsRequired;

    public ProjectAddViewModel(IUserDialogService userDialogService) : base(userDialogService)
    {
        ControlTitle = "Добавление проекта";
    }

    protected override async Task Accept(object action)
    {
        var selectedAreas = Areas.Where(e => e.IsSelected).ToList();
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var projectService = scope.ServiceProvider.GetRequiredService<IProjectService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var dto = new ProjectAddDTO(
            FactoryNumber,
            ContractNumber,
            Customer,
            Vendor,
            Remark,
            IsEditable,
            AllowMarksDeleting,
            AllowMarksAdding,
            AllowMarksModifying,
            AreExecutorsRequired,
            selectedAreas.Select(e => e.Id)
            );

        var result = await projectService.AddAsync(dto);
        if (result.IsSuccess)
        {
            var project = await dbContext
            .Projects
            .Include(e => e.Areas)
            .ThenInclude(e => e.Area)
            .Include(e => e.Creator)
            .Include(e => e.Settings)
            .AsNoTracking()
            .SingleAsync(e => e.Id == result.Value)
            .ConfigureAwait(false);

            App.Current.Dispatcher.Invoke(() => _dialogService.CloseDialog());
            Messenger.Send(new ProjectAddedMessage(project.ToViewModel()));
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
    }

    protected override bool CanAccept(object p) => !string.IsNullOrWhiteSpace(FactoryNumber);

    protected async override void OnActivated()
    {
        base.OnActivated();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var areas = await dbContext
            .Areas
            .AsNoTracking()
            .OrderBy(e => e.Order)
            .Select(e => new SelectableAreaViewModel { Id = e.Id, Title = e.Title, Order = e.Order })
            .ToListAsync();

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            Areas.AddRange(areas);
        });
    }
}