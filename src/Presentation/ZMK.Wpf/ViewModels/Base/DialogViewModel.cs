using CommunityToolkit.Mvvm.Input;
using ZMK.Wpf.Services;
using CommunityToolkit.Mvvm.Messaging;

namespace ZMK.Wpf.ViewModels.Base;

public abstract partial class DialogViewModel : TitledViewModel
{
    #region --Fields--

    protected readonly IUserDialogService _dialogService;

    #endregion

    #region --Properties--



    #endregion

    #region --Constructors--

    public DialogViewModel() { }

    public DialogViewModel(IUserDialogService userDialogService)
    {
        _dialogService = userDialogService;
    }

    #endregion

    #region --Commands--

    protected virtual bool CanCancel(object p) => true;

    [RelayCommand(CanExecute = nameof(CanCancel))]
    protected virtual Task Cancel(object p)
    {
        _dialogService.CloseDialog();
        return Task.CompletedTask;
    }

    protected virtual bool CanAccept(object p) => p is string { Length: > 0 };

    [RelayCommand(CanExecute = nameof(CanAccept))]
    protected abstract Task Accept(object action);

    #endregion

    #region --Methods--

    protected void SendMessage<M>(M message) where M : class, new()
    {
        Messenger.Send(message);
    }

    #endregion
}