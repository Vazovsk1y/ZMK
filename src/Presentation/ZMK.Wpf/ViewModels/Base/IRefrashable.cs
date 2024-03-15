namespace ZMK.Wpf.ViewModels;

public interface IRefrashable
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}