using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Dialogs.ResultTypes;

namespace CoursesManager.UI.Dialogs.ViewModels;

public class NotifyDialogViewModel : DialogViewModel<DialogResultType>
{
    private string _title = null!;
    private bool _IsDialogOpen;

    public bool IsDialogOpen
    {
        get => _IsDialogOpen;
        set => SetProperty(ref _IsDialogOpen, value);
    }
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _message = null!;
    public string Message
    {
        get => _message;
        set => SetProperty(ref _message, value);
    }

    public ICommand ConfirmationCommand { get; private set; }

    public NotifyDialogViewModel(DialogResultType? initialData) : base(initialData)
    {
        if (initialData is not null)
        {
            Message = initialData.DialogText;
            Title = initialData.DialogTitle;
        }

        ConfirmationCommand = new RelayCommand(() =>
        {
            InvokeResponseCallback(DialogResult<DialogResultType>.Builder()
                .SetSuccess(new DialogResultType
                {
                    Result = false
                }).Build());
        });
    }

    protected override void InvokeResponseCallback(DialogResult<DialogResultType> dialogResult)
    {
        ResponseCallback?.Invoke(dialogResult);
    }
}