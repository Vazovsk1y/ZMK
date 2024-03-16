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

public partial class MarksPanelViewModel : ObservableRecipient,
        IRecipient<MarksAddedMessage>,
        IRefrashable
{
    public ObservableCollection<MarkViewModel> Marks { get; } = [];

    public ProjectViewModel SelectedProject { get; }

    [ObservableProperty]
    [NotifyCanExecuteChangedFor(nameof(DeleteCommand))]
    private MarkViewModel? _selectedMark;

    public MarksPanelViewModel(ProjectsPanelViewModel projectsPanelViewModel)
    {
        ArgumentNullException.ThrowIfNull(projectsPanelViewModel.SelectedProject);
        SelectedProject = projectsPanelViewModel.SelectedProject;
    }

    [RelayCommand(CanExecute = nameof(CanDelete))]
    public async Task Delete()
    {
        if (DeleteCommand.IsRunning)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();

        var dialogResult = MessageBoxHelper.ShowDialogBoxYesNo($"Вы уверены, что желаете удалить марку '{SelectedMark!.Title} - {SelectedMark!.Code}'?");
        if (dialogResult == System.Windows.MessageBoxResult.Yes)
        {
            var result = await markService.DeleteAsync(SelectedMark.Id).ConfigureAwait(false);
            if (result.IsSuccess)
            {
                App.Current.Dispatcher.Invoke(() => Marks.Remove(SelectedMark));
                MessageBoxHelper.ShowInfoBox("Марка успешно удалена.");
            }
            else
            {
                MessageBoxHelper.ShowErrorBox(result.Errors.Display());
            }
        }
    }

    public bool CanDelete()
    {
        return SelectedMark is not null;
    }

    [RelayCommand]
    public void Add()
    {
        using var scope = App.Services.CreateScope();
        var dialogService = scope.ServiceProvider.GetRequiredService<IUserDialogService>();
        dialogService.ShowDialog<MarkAddWindow>();
    }

    [RelayCommand]
    public async Task ImportFromExcel()
    {
        if (ImportFromExcelCommand.IsRunning || SelectedProject is null)
        {
            return;
        }

        const string Filter = "Excel Files (*.xlsx; *.xls)|*.xlsx; *.xls";
        const string Title = "Выберите файл:";
        var fileDialog = new Microsoft.Win32.OpenFileDialog
        {
            Multiselect = false,
            Filter = Filter,
            Title = Title,
            RestoreDirectory = true,
        };

        var dialogResult = fileDialog.ShowDialog();

        string selectedFile;
        if (dialogResult is bool dresult && dresult is true)
        {
            selectedFile = fileDialog.FileName;
        }
        else
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var result = await markService.AddFromXlsxAsync(selectedFile, SelectedProject.Id);
        if (result.IsSuccess)
        {
            var addedMarks = await dbContext
            .Marks
            .AsNoTracking()
            .Where(e => result.Value.Contains(e.Id))
            .Select(e => e.ToViewModel())
            .ToListAsync()
            .ConfigureAwait(false);

            await App.Current.Dispatcher.InvokeAsync(() =>
            {
                Marks.AddRange(addedMarks);
                MessageBoxHelper.ShowInfoBox($"{result.Value.Count} марок было успешно добавлено.");
            });
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(result.Errors.Display());
        }
    }

    [RelayCommand]
    private async Task SaveChanges()
    {
        var modifiedMarks = Marks.Where(e => e.IsModified()).ToList();
        if (SaveChangesCommand.IsRunning || modifiedMarks.Count == 0)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var markService = scope.ServiceProvider.GetRequiredService<IMarkService>();

        var results = new List<Result>();
        foreach (var mark in modifiedMarks)
        {
            var dto = new MarkUpdateDTO(mark.Id, mark.Code, mark.Title, mark.Order, mark.Weight, mark.Count, mark.Remark);
            var updateResult = await markService.UpdateAsync(dto);

            if (updateResult.IsFailure)
            {
                mark.RollBackChanges();
            }
            else
            {
                mark.SaveState();
            }

            results.Add(updateResult);
        }

        if (results.Where(e => e.IsSuccess).Any())
        {
            MessageBoxHelper.ShowInfoBox($"Информация об {results.Where(e => e.IsSuccess).Count()} марках была обновлена успешно.");
        }
        else
        {
            MessageBoxHelper.ShowErrorBox(results.Where(e => e.IsFailure).SelectMany(e => e.Errors).Display());
        }
    }

    public void Receive(MarksAddedMessage message)
    {
        App.Current.Dispatcher.Invoke(() =>
        {
            Marks.AddRange(message.Marks);
            MessageBoxHelper.ShowInfoBox("Марки были успешно добавлены.");
        });
    }

    protected override async void OnActivated()
    {
        base.OnActivated();
        await RefreshAsync();
    }

    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (SelectedProject is null)
        {
            return;
        }

        using var scope = App.Services.CreateScope();
        var dbContext = scope.ServiceProvider.GetRequiredService<ZMKDbContext>();

        var marks = await dbContext
            .Marks
            .AsNoTracking()
            .OrderBy(e => e.Title)
            .Where(e => e.ProjectId == SelectedProject.Id)
            .Select(e => e.ToViewModel())
            .ToListAsync(cancellationToken)
            .ConfigureAwait(false);

        await App.Current.Dispatcher.InvokeAsync(() =>
        {
            Marks.Clear();
            SelectedMark = null;
            Marks.AddRange(marks);
        });
    }
}
