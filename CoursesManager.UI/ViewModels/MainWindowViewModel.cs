using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using System.Windows.Input;
using CoursesManager.MVVM.Data;
using CoursesManager.UI.Messages;
using CoursesManager.UI.ViewModels.Students;
using System.Windows.Media.Imaging;
using System;

namespace CoursesManager.UI.ViewModels;

public class MainWindowViewModel : ViewModelWithNavigation
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

    public ICommand GoToConfigurationView { get; private set; }

    public BitmapImage BackgroundImage { get; private set; }

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

    public MainWindowViewModel(INavigationService navigationService, IMessageBroker messageBroker) : base(navigationService)
    {
        BackgroundImage = LoadImage($"Resources/Images/CourseManagerA3.png");
        _navigationService = navigationService;
        _messageBroker = messageBroker;
        _messageBroker.Subscribe<OverlayActivationMessage, MainWindowViewModel>(OverlayActivationHandler, this);

        CloseCommand = new RelayCommand(() =>
        {
            _messageBroker.Publish<ApplicationCloseRequestedMessage>(new ApplicationCloseRequestedMessage());
        });

        OpenSidebarCommand = new RelayCommand(() =>
        {
            IsSidebarHidden = !IsSidebarHidden;
        });

        GoForwardCommand = new RelayCommand(() =>
        {
            NavigationService.GoForward();
        }, NavigationService.CanGoForward);

        GoBackCommand = new RelayCommand(() =>
        {
            NavigationService.GoBack();
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
            NavigationService.NavigateTo<StudentManagerViewModel>();
            IsSidebarHidden = false;
        }, () => INavigationService.CanNavigate);

        GoToCourseManagementView = new RelayCommand(() =>
        {
            NavigationService.NavigateTo<CoursesManagerViewModel>();
            IsSidebarHidden = false;
        }, () => INavigationService.CanNavigate);

        GoToConfigurationView = new RelayCommand(() =>
        {
            NavigationService.NavigateTo<ConfigurationViewModel>();
            IsSidebarHidden = false;
        }, () => INavigationService.CanNavigate);

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

    private static BitmapImage LoadImage(string relativePath)
    {
        var uri = new Uri($"pack://application:,,,/{relativePath}", UriKind.Absolute);
        return new BitmapImage(uri);
    }
}