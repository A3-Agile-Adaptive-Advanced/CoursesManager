using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Messages;
using CoursesManager.UI.Dialogs.ResultTypes;
using System.Windows.Input;

namespace CoursesManager.UI.Dialogs.ViewModels
{

    public class TemplatePreviewDialogViewModel : DialogViewModel<DialogResultType>
    {
        private readonly IMessageBroker _messageBroker;
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

        public ICommand CloseCommand { get; private set; }
        public TemplatePreviewDialogViewModel(DialogResultType? initialData, IMessageBroker messageBroker) : base(initialData)
        {
            _messageBroker = messageBroker;
            if (initialData is not null)
            {
                Message = initialData.DialogText;
                Title = initialData.DialogTitle;
            }
            IsStartAnimationTriggered = true;
            CloseCommand = new RelayCommand(OnClose);
        }

        public async void OnClose()
        {
            await TriggerEndAnimationAsync();

            InvokeResponseCallback(DialogResult<DialogResultType>.Builder().SetSuccess(new DialogResultType
            {
                Result = true
            }).Build());
        }

        protected override void InvokeResponseCallback(DialogResult<DialogResultType> dialogResult)
        {
            ResponseCallback?.Invoke(dialogResult);
        }
    }
}
