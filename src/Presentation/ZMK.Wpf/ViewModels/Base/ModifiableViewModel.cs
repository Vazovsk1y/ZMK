using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.ViewModels;

public abstract class ModifiableViewModel<T> :
    ObservableObject,
    IModifiable<T> where T : IModifiable<T>
{
    public T PreviousState { get; private set; } = default!;

    private UpdatableSign? _updatableSign;
    public virtual UpdatableSign? UpdatableSign
    {
        get => _updatableSign is null ? IsModified() ? new UpdatableSign() : null : _updatableSign;
        set
        {
            SetProperty(ref _updatableSign, value);
        }
    }

    public abstract bool IsModified();

    public abstract void RollBackChanges();

    public virtual void SaveState()
    {
        PreviousState = (T)MemberwiseClone();
        OnPropertyChanged(nameof(UpdatableSign));
    }
}

public partial class UpdatableSign(string value = "*") : ObservableObject
{
    [ObservableProperty]
    private string _value = value;
}