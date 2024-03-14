using CommunityToolkit.Mvvm.ComponentModel;

namespace ZMK.Wpf.ViewModels;

public abstract class ModifiableViewModel<T> :
    ObservableObject,
    IModifiable<T> where T : IModifiable<T>
{
    public T PreviousState { get; private set; } = default!;

    public UpdatableSign? UpdatableSign => IsModified() ? new UpdatableSign() : null;

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