using System.Windows.Input;
using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.UI.Dialogs.ResultTypes;

namespace CoursesManager.UI.Dialogs.ViewModels;

public class ConfirmationDialogViewModel : DialogViewModel<DialogResultType>
{
    private string _title = null!;

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

    public ICommand YesCommand { get; private set; }

    public ICommand NoCommand { get; private set; }

    public ConfirmationDialogViewModel(DialogResultType? initialData) : base(initialData)
    {
        IsStartAnimationTriggered = true;
        if (initialData is not null)
        {
            Message = initialData.DialogText;
            Title = initialData.DialogTitle;
        }

        YesCommand = new RelayCommand(async () =>
        {
            IsEndAnimationTriggered = true;
            await Task.Delay(100);
            InvokeResponseCallback(DialogResult<DialogResultType>.Builder().SetSuccess(new DialogResultType
            {
                Result = true
            }).Build());
            
        });

        NoCommand = new RelayCommand(async () =>
        {
            IsEndAnimationTriggered = true;
            await Task.Delay(100);
            InvokeResponseCallback(DialogResult<DialogResultType>.Builder().SetSuccess(new DialogResultType
            {
                Result = false
            }).Build());
            
        });
    }

    protected override void InvokeResponseCallback(DialogResult<DialogResultType> dialogResult)
    {
        ResponseCallback.Invoke(dialogResult);
    }
}