namespace ZMK.Wpf.ViewModels.Base;

public interface IModifiable<T> where T : IModifiable<T>
{
    bool IsModified();

    T PreviousState { get; }

    void RollBackChanges();

    void SaveState();
}