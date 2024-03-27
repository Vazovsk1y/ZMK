namespace ZMK.Wpf.ViewModels.Base;

public interface IRefrashable
{
    Task RefreshAsync(CancellationToken cancellationToken = default);
}