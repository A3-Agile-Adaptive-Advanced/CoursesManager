using CoursesManager.MVVM.Commands;
using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Enums;
using CoursesManager.UI.Messages;
using CoursesManager.UI.ViewModels.Mailing;
using CoursesManager.UI.ViewModels.Students;
using System.Windows.Input;
using System.Windows.Media.Imaging;

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
    public ICommand GotoTemplateView { get; private set; }

    private CancellationTokenSource _toastCancellationTokenSource;
    private bool _isToastProcessing = false;
    private ToastNotificationMessage _nextMessage = null;
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

    private static bool _toastDisplayed;
    public bool ToastDisplayed
    {
        get => _toastDisplayed;
        set => SetProperty(ref _toastDisplayed, value);
    }

    private string _toastText;
    public string ToastText
    {
        get => _toastText;
        set => SetProperty(ref _toastText, value);
    }

    private ToastType _toastType;
    public ToastType ToastType
    {
        get => _toastType;
        set
        {
            _toastType = value;
            OnPropertyChanged(nameof(ToastType));
        }
    }

    public MainWindowViewModel(INavigationService navigationService, IMessageBroker messageBroker) : base(navigationService)
    {
        BackgroundImage = LoadImage($"Resources/Images/CourseManagerA3.png");
        _navigationService = navigationService;
        _messageBroker = messageBroker;
        _messageBroker.Subscribe<OverlayActivationMessage, MainWindowViewModel>(OverlayActivationHandler, this);
        _messageBroker.Subscribe<ToastNotificationMessage, MainWindowViewModel>(ShowToastNotification, this);

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
        GotoTemplateView = new RelayCommand(() =>
        {
            NavigationService.NavigateTo<EditMailTemplatesViewModel>();
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

    private async void OverlayActivationHandler(OverlayActivationMessage message)
    {
        IsDialogOpen = message.IsVisible;
    }

    private async void ShowToastNotification(ToastNotificationMessage message)
    {

        if (_isToastProcessing)
        {
            _nextMessage = message;
            _toastCancellationTokenSource?.Cancel();
            return;
        }

        _isToastProcessing = true;
        _nextMessage = null;
        _toastCancellationTokenSource = new CancellationTokenSource();
        var token = _toastCancellationTokenSource.Token;

        try
        {
            await SetToastMessageDetails(message.NotificationText, message.SetVisibillity, message.ToastType);

            await Task.Delay(5000, token);
        }
        catch (TaskCanceledException)
        {
            // Making sure that the above process is canceled when the token is used.
        }
        await SetToastMessageDetails(string.Empty, false, ToastType.None);
        _isToastProcessing = false;

        if (_toastCancellationTokenSource?.Token == token)
        {
            _toastCancellationTokenSource.Dispose();
            _toastCancellationTokenSource = null;
        }

        if (_nextMessage != null)
        {
            ShowToastNotification(_nextMessage);
        }
    }

    private async Task SetToastMessageDetails(string text, bool visibility, ToastType toastType)
    {
        ToastDisplayed = visibility;
        if (!visibility)
        {
            await Task.Delay(1000);
        }
        ToastText = text;
        ToastType = toastType;
    }

    private static BitmapImage LoadImage(string relativePath)
    {
        var uri = new Uri($"pack://application:,,,/{relativePath}", UriKind.Absolute);
        return new BitmapImage(uri);
    }
}