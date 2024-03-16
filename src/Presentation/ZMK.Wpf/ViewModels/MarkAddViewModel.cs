using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Globalization;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;

namespace ZMK.Wpf.ViewModels;

public partial class MarkAddViewModel : DialogViewModel
{
    public ProjectViewModel SelectedProject { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _code = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _title = null!;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private int _order;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private string _weight;

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(AcceptCommand))]
    private int _count;

    [ObservableProperty]
    private string? _remark;

    public MarkAddViewModel()
    {
        ControlTitle = "Добавление марки";
    }

    public MarkAddViewModel(
        IUserDialogService userDialogService,
        ProjectsPanelViewModel projectsPanel) : base(userDialogService)
    {
        ArgumentNullException.ThrowIfNull(projectsPanel.SelectedProject);
        SelectedProject = projectsPanel.SelectedProject;
        ControlTitle = "Добавление марки";
    }

    protected override async Task Accept(object action)
    {
        if (AcceptCommand.IsRunning)
        {
            return;
        }

        if (!double.TryParse(Weight, NumberStyles.Any, CultureInfo.CurrentCulture, out double weight) &&
            //Then try in US english
            !double.TryParse(Weight, NumberStyles.Any, CultureInfo.GetCultureInfo("en-US"), out weight) &&
            //Then in neutral language
            !double.TryParse(Weight, NumberStyles.Any, CultureInfo.InvariantCulture, out weight))
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var result = await markService.AddAsync(new MarkAddDTO(SelectedProject.Id, Code, Title, Order, weight, Count, Remark));
        if (result.IsSuccess)
        {
            var addedMark = await dbContext
                .Marks
                .AsNoTracking()
                .SingleAsync(e => e.Id == result.Value);

            Messenger.Send(new MarksAddedMessage([ addedMark.ToViewModel() ]));
            App.Current.Dispatcher.Invoke(() => _dialogService.CloseDialog());
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
    }

    protected override bool CanAccept(object p) => 
        !string.IsNullOrWhiteSpace(Code)
        && !string.IsNullOrWhiteSpace(Title)
        && Count > 0
        && !string.IsNullOrWhiteSpace(Weight)
        && Order > 0;
}