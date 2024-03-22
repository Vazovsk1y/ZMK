﻿using CommunityToolkit.Mvvm.ComponentModel;
using System.Collections.ObjectModel;

namespace ZMK.Wpf.ViewModels;

public partial class ProjectSettingsViewModel : ModifiableViewModel<ProjectSettingsViewModel>
{
    public required Guid ProjectId { get; init; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    private ObservableCollection<ProjectAreaViewModel> _areas = [];

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _isEditable;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _allowMarksDeleting;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _allowMarksModifying;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _allowMarksAdding;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(UpdatableSign))]
    public bool _areExecutorsRequired;

    public override bool IsModified()
    {
        return IsEditable != PreviousState.IsEditable
            || AllowMarksDeleting != PreviousState.AllowMarksDeleting
            || AllowMarksModifying != PreviousState.AllowMarksModifying
            || AllowMarksAdding != PreviousState.AllowMarksAdding
            || AreExecutorsRequired != PreviousState.AreExecutorsRequired
            || !Areas.OrderBy(e => e.Title).Select(e => e.Id).SequenceEqual(PreviousState.Areas.OrderBy(e => e.Title).Select(e => e.Id));
    }

    public override void RollBackChanges()
    {
        Areas = PreviousState.Areas;
        IsEditable = PreviousState.IsEditable;
        AllowMarksDeleting = PreviousState.AllowMarksDeleting;
        AllowMarksModifying = PreviousState.AllowMarksModifying;
        AllowMarksAdding = PreviousState.AllowMarksAdding;
        AreExecutorsRequired = PreviousState.AreExecutorsRequired;
    }

    public override void SaveState()
    {
        base.SaveState();
        PreviousState.Areas = new(Areas);
    }

    public record ProjectAreaViewModel(Guid Id, string Title, int Order);
}