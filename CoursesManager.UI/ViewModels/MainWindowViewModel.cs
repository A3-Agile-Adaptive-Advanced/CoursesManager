using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using System.Windows.Input;
using CoursesManager.MVVM.Data;
using CoursesManager.UI.Messages;
using CoursesManager.UI.ViewModels.Courses;
using System.Windows.Navigation;
using System.Diagnostics;

namespace CoursesManager.UI.ViewModels;

public class MainWindowViewModel : NavigatableViewModel
{
    private readonly IMessageBroker _messageBroker;


    public ICommand CloseCommand { get; private set; }

    public ICommand OpenSidebarCommand { get; private set; }

    public ICommand GoForwardCommand { get; private set; }

    public ICommand GoBackCommand { get; private set; }

    public ICommand MouseEnterButtonCommand { get; private set; }
    public ICommand MouseEnterBorderCommand { get; private set; }
    public ICommand MouseLeaveButtonCommand { get; private set; }
    public ICommand MouseLeaveBorderCommand { get; private set; }
    public ICommand GoToStudentManagementView { get; private set; }
    public ICommand GoToCourseManagementView { get; private set; }

    private INavigationService _navigationService;
    public INavigationService NavigationService
    {
        get => _navigationService;
        set => SetProperty(ref _navigationService, value);
    }

    private bool _isSidebarHidden;
    public bool IsSidebarHidden
    {
        get => _isSidebarHidden;
        set => SetProperty(ref _isSidebarHidden, value);
    }

    private bool _isMouseOverButton;
    public bool IsMouseOverButton
    {
        get => _isMouseOverButton;
        set => SetProperty(ref _isMouseOverButton, value);
    }

    private bool _isMouseOverBorder;
    public bool IsMouseOverBorder
    {
        get => _isMouseOverBorder;
        set => SetProperty(ref _isMouseOverBorder, value);
    }

    private static bool _isDialogOpen;
    public bool IsDialogOpen
    {
        get => _isDialogOpen;
        set => SetProperty(ref _isDialogOpen, value);
    }


    private bool _isEndAnimationTriggered;

    public bool IsEndAnimationTriggered
    {
        get => _isEndAnimationTriggered;
        set => SetProperty(ref _isEndAnimationTriggered, value);
    }

    private bool _isStartAnimationTriggered;

    public bool IsStartAnimationTriggeredTest
    {
        get => _isStartAnimationTriggered;
        set => SetProperty(ref _isStartAnimationTriggered, value);
    }

    public MainWindowViewModel(INavigationService navigationService, IMessageBroker messageBroker) : base(navigationService)
    {
        _navigationService = navigationService;
        _messageBroker = messageBroker;
        _messageBroker.Subscribe<OverlayActivationMessage, MainWindowViewModel>(OverlayActivationHandler, this);
        _messageBroker.Subscribe<AnimationProcedureMessage, MainWindowViewModel>(AnimationProcedureHandler, this);

        CloseCommand = new RelayCommand(() =>
        {
            _messageBroker.Publish<ApplicationCloseRequestedMessage>(new ApplicationCloseRequestedMessage());
        });

        OpenSidebarCommand = new RelayCommand(() =>
        {
            IsSidebarHidden = !IsSidebarHidden;
        });

        GoForwardCommand = new RelayCommand(async () =>
        {
            GoForward();
        }, NavigationService.CanGoForward);

        GoBackCommand = new RelayCommand( () =>
        {
            GoBack();
        }, NavigationService.CanGoBack);

        MouseEnterButtonCommand = new RelayCommand(() =>
        {
            IsMouseOverButton = true;
            UpdateSidebarVisibility();
        });
        MouseEnterBorderCommand = new RelayCommand(() =>
        {
            IsMouseOverBorder = true;
            UpdateSidebarVisibility();
        });
        MouseLeaveBorderCommand = new RelayCommand(() =>
        {
            IsMouseOverBorder = false;
            UpdateSidebarVisibility();
        });
        MouseLeaveButtonCommand = new RelayCommand(() =>
        {
            IsMouseOverButton = false;
            UpdateSidebarVisibility();
        });
        GoToStudentManagementView = new RelayCommand(() =>
        {
            GoToStudentManagement();
        });
        GoToCourseManagementView = new RelayCommand(() =>
        {
            GoToCourseManagement();
        });
    }

    private async void GoBack()
    {
        await ExecuteWithAnimation(async () =>
        {
            await Task.Delay(60);
            NavigationService.GoBack();
        });
    }

    private async void GoForward()
    {
        await ExecuteWithAnimation(async () =>
        {
            await Task.Delay(100);
            NavigationService.GoForward();
        });
    }

    private async void GoToStudentManagement()
    {
        await ExecuteWithAnimation(async () =>
        {
            IsSidebarHidden = false;
            await Task.Delay(100);
            NavigationService.NavigateTo<StudentManagerViewModel>();
        });
    }

    private async void GoToCourseManagement()
    {
        await ExecuteWithAnimation(async () =>
        {
            IsSidebarHidden = false;
            await Task.Delay(100);
            NavigationService.NavigateTo<CoursesManagerViewModel>();
        });
    }

    private async void UpdateSidebarVisibility()
    {
        await Task.Delay(300);
        if (IsMouseOverButton || IsMouseOverBorder)
        {
            IsSidebarHidden = true;
        }
        else
        {
            IsSidebarHidden = false;
        }
    }

    private async void OverlayActivationHandler(OverlayActivationMessage obj)
    {
        OverlayActivationMessage overlayActivationMessage = obj as OverlayActivationMessage;
        IsDialogOpen = overlayActivationMessage.IsVisible;
    }

    private async void AnimationProcedureHandler(AnimationProcedureMessage ob)
    {
        AnimationProcedureMessage animationProcedureMessage = ob as AnimationProcedureMessage;
        IsEndAnimationTriggered = animationProcedureMessage.animationTrigger;
    }

    private async Task ExecuteWithAnimation(Func<Task> action)
    {
        IsEndAnimationTriggered = true;
        try
        {
            await action();
        }
        finally
        {
            IsEndAnimationTriggered = false;
        }
    }
}