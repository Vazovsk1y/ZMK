using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels;

public partial class AreasPanelViewModel : ObservableRecipient, IRecipient<AreaAddedMessage>, IRefrashable
{
    public ObservableCollection<AreaViewModel> Areas { get; } = [];

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private AreaViewModel? _selectedArea;

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public async Task Delete()
    {
        if (DeleteCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var areaService = scope.ServiceProvider.GetRequiredService<IAreaService>();

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что хотите удалить участок '{SelectedArea?.Title}'?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            var result = await areaService.DeleteAsync(SelectedArea!.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                App.Current.Dispatcher.Invoke(() => Areas.Remove(SelectedArea));
                MessageBoxHelper.ShowInfoBox("Участок успешно удален.");
            }
            else
            {
                MessageBoxHelper.ShowErrorBox(result.Errors.Display());
            }
        }
    }

    public bool CanDelete()
    {
        return SelectedArea is not null;
    }

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<AreaAddWindow>();
    }

    [RelayCommand]
    public async Task SaveChanges()
    {
        var modifiedAreas = Areas.Where(e => e.IsModified()).ToList();
        if (SaveChangesCommand.IsRunning || modifiedAreas.Count == 0)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var areaService = scope.ServiceProvider.GetRequiredService<IAreaService>();

        var results = new List<Result>();
        foreach (var area in modifiedAreas)
        {
            var dto = new AreaUpdateDTO(area.Id, area.Order, area.Title, area.Remark);

            var updateResult = await areaService.UpdateAsync(dto);
            if (updateResult.IsFailure)
            {
                area.RollBackChanges();
            }
            else
            {
                area.SaveState();
            }

            results.Add(updateResult);
        }

        if (results.Where(e => e.IsSuccess).Any())
        {
            MessageBoxHelper.ShowInfoBox($"Информация о {results.Where(e => e.IsSuccess).Count()} участках была обновлена успешно.");
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(results.Where(e => e.IsFailure).SelectMany(e => e.Errors).Display());
        }
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var areas = await dbContext.Areas
            .AsNoTracking()
            .OrderBy(a => a.Title)
            .Select(a => a.ToViewModel())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            Areas.Clear();
            SelectedArea = null;
            Areas.AddRange(areas);
        });
    }

    public void Receive(AreaAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Areas.Add(message.Area);
            MessageBoxHelper.ShowInfoBox("Участок был успешно добавлен.");
        });
    }
}
