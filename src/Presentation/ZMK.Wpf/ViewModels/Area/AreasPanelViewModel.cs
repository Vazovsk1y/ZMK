using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using CommunityToolkit.Mvvm.Messaging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using System.Collections.ObjectModel;
using ZMK.Application.Contracts;
using ZMK.Application.Services;
using ZMK.Domain.Shared;
using ZMK.PostgresDAL;
using ZMK.Wpf.Constants;
using ZMK.Wpf.Extensions;
using ZMK.Wpf.Messages;
using ZMK.Wpf.Services;
using ZMK.Wpf.Views.Windows;

namespace ZMK.Wpf.ViewModels;

public partial class AreasPanelViewModel : TitledViewModel, IRecipient<AreaAddedMessage>, IRefrashable
{
    private readonly IMemoryCache _cache;

    public AreasPanelViewModel(IMemoryCache cache)
    {
        _cache = cache;
    }

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
            }
            else
            {
                MessageBoxHelper.ShowErrorBox(result.Errors.Display());
            }
        }
    }

    public bool CanDelete() => SelectedArea is not null;

    [RelayCommand]
    public void RollbackChanges()
    {
        var modifiedAreas = Areas.Where(e => e.IsModified()).ToList();
        if (modifiedAreas.Count == 0)
        {
            return;
        }

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете отменить все текущие изменения?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            modifiedAreas.ForEach(e => e.RollBackChanges());
        }
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

        IsEnabled = false;
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

        results.DisplayUpdateResultMessageBox();
        IsEnabled = true;
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (_cache.TryGetValue(Cache.AreasPanelCacheKey, out _))
        {
            return;
        }

        IsEnabled = false;

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
            object cacheStub = new();
            Areas.Clear();
            SelectedArea = null;
            Areas.AddRange(areas);
            _cache.Set(Cache.AreasPanelCacheKey, cacheStub, Cache.AreasPanelCacheExpiration);
            IsEnabled = true;
        });
    }

    public void Receive(AreaAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Areas.Add(message.Area);
        });
    }
}
