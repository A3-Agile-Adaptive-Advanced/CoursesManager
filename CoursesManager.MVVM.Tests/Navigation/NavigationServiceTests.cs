using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;

namespace CoursesManager.MVVM.Tests.Navigation;

internal class ViewModelWithNavigationForTest : ViewModelWithNavigation
{
    public INavigationService NavigationService { get; set; }

    public ViewModelWithNavigationForTest(INavigationService navigationService) : base(navigationService)
    {
        NavigationService = navigationService;
    }
}

internal class ViewModelWithoutNavigate : ViewModel
{
}

internal class UnregisteredViewModelWithoutNavigate : ViewModel
{
}

public class NavigationServiceTests
{
    private INavigationService _navigationService;

    [SetUp]
    public void SetUp()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactory(() => new ViewModelWithoutNavigate());
        INavigationService.RegisterViewModelFactory((nav) => new ViewModelWithNavigationForTest((INavigationService)nav));
        _navigationService = new NavigationService();
    }

    [Test]
    public void RegisterViewModelFactory_ThrowsArgumentNullException_WhenViewModelFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            INavigationService.RegisterViewModelFactory<ViewModelWithoutNavigate>(null);
        });
    }

    [Test]
    public void RegisterViewModelFactory_RegistrationAdded()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactory(() => new ViewModelWithoutNavigate());

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtLeast(1));
    }

    [Test]
    public void RegisterViewModelFactory_OnlyAddedOnce()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactory(() => new ViewModelWithoutNavigate());
        INavigationService.RegisterViewModelFactory(() => new ViewModelWithoutNavigate());

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtMost(1));
    }

    [Test]
    public void RegisterViewModelFactoryWithNaviagtionService_ThrowsArgumentException_WhenViewModelFactoryIsNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
        {
            INavigationService.RegisterViewModelFactory<ViewModelWithNavigationForTest>((Func<object, ViewModelWithNavigationForTest>)(Func<INavigationService, ViewModelWithNavigationForTest>)null);
        });
    }

    [Test]
    public void RegisterViewModelFactoryWithNaviagtionService_RegistrationAdded()
    {
        INavigationService.ViewModelFactories.Clear();

        INavigationService.RegisterViewModelFactory<ViewModelWithNavigationForTest>((navigationService) => new ViewModelWithNavigationForTest((INavigationService)navigationService));

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtLeast(1));
    }

    [Test]
    public void RegisterViewModelFactoryWithNaviagtionService_OnlyAddedOnce()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactory((navigationService) => new ViewModelWithNavigationForTest((INavigationService)navigationService));
        INavigationService.RegisterViewModelFactory((navigationService) => new ViewModelWithNavigationForTest((INavigationService)navigationService));

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtMost(1));
    }

    [Test]
    public void CanGoBack_ReturnsFalse_WhenNoForwardNavigationHasOccured()
    {
        Assert.IsFalse(_navigationService.CanGoBack());
    }

    [Test]
    public void CanGoBack_ReturnsTrue_WhenForwardNavigationHasOccured()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        Assert.IsTrue(_navigationService.CanGoBack());
    }

    [Test]
    public void CanGoBack_ReturnsFalse_WhenItWentBackToBeginning()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();

        Assert.IsFalse(_navigationService.CanGoBack());
    }

    [Test]
    public void CanGoBack_ReturnsFalse_WhenItHasNotGoneAllTheWayBack()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();

        Assert.IsTrue(_navigationService.CanGoBack());
    }

    [Test]
    public void CanGoForward_ReturnsFalse_WhenNoBackwardHasHappened()
    {
        Assert.IsFalse(_navigationService.CanGoForward());
    }

    [Test]
    public void CanGoForward_ReturnsFalse_WhenNoBackwardHasHappenedAfterNavigation()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        Assert.IsFalse(_navigationService.CanGoForward());
    }

    [Test]
    public void CanGoForward_ReturnsTrue_WhenBackwardsNavigationHasHappened()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();

        Assert.IsTrue(_navigationService.CanGoForward());
    }

    [Test]
    public void CanGoForward_ReturnsFalse_WhenItHasGoneAllTheWayForward()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();
        _navigationService.GoBack();
        _navigationService.GoForward();
        _navigationService.GoForward();

        Assert.IsFalse(_navigationService.CanGoForward());
    }

    [Test]
    public void CanGoForward_ReturnsTrue_WhenItHasNotGoneAllTheWayForward()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();
        _navigationService.GoBack();
        _navigationService.GoForward();

        Assert.IsTrue(_navigationService.CanGoForward());
    }

    [Test]
    public void NavigateTo_MakesGoForwardReturnFalse_AfterNewNavigationPath()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();
        _navigationService.GoBack();
        _navigationService.GoForward();

        var shouldHaveBeenTrue = _navigationService.CanGoForward();

        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        var shouldHaveBeenFalse = _navigationService.CanGoForward();

        Assert.IsTrue(shouldHaveBeenTrue);
        Assert.IsFalse(shouldHaveBeenFalse);
    }

    [Test]
    public void GoBack_CurrentViewModelChanges()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithNavigationForTest>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        var beforeGoBack = _navigationService.NavigationStore.CurrentViewModel;

        _navigationService.GoBack();

        var afterGoBack = _navigationService.NavigationStore.CurrentViewModel;

        Assert.That(beforeGoBack, Is.Not.EqualTo(afterGoBack));
    }

    [Test]
    public void GoBackAndClearForward_ClearsForwardStack()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithNavigationForTest>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        _navigationService.GoBackAndClearForward();

        Assert.IsFalse(_navigationService.CanGoForward());
    }

    [Test]
    public void GoForward_CurrentViewModelChanges()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithNavigationForTest>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();

        var beforeGoForward = _navigationService.NavigationStore.CurrentViewModel;

        _navigationService.GoForward();

        var afterGoForward = _navigationService.NavigationStore.CurrentViewModel;

        Assert.That(beforeGoForward, Is.Not.EqualTo(afterGoForward));
    }

    [Test]
    public void NavigateTo_ThrowsInvalidOperationException_WhenViewModelNotRegistered()
    {
        Assert.Throws<InvalidOperationException>(() =>
        {
            _navigationService.NavigateTo<UnregisteredViewModelWithoutNavigate>();
        });
    }

    [Test]
    public void NavigateTo_ClearsForwardStack()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithNavigationForTest>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();

        var beforeNavigate = _navigationService.CanGoForward();

        _navigationService.NavigateTo<ViewModelWithNavigationForTest>();

        var afterNavigate = _navigationService.CanGoForward();

        Assert.That(beforeNavigate, Is.Not.EqualTo(afterNavigate));
    }

    [Test]
    public void NavigateTo_AddsToBackwardsStack()
    {
        var beforeNavigate = _navigationService.CanGoBack();

        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithNavigationForTest>();

        var afterNavigate = _navigationService.CanGoBack();

        Assert.That(beforeNavigate, Is.Not.EqualTo(afterNavigate));
    }

    [Test]
    public void NavigateTo_DoesNotCreateNewInstance_WhenAlreadyNavigatedBefore()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        var beforeSecondNavigation = _navigationService.NavigationStore.CurrentViewModel;

        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        var afterSecondNavigation = _navigationService.NavigationStore.CurrentViewModel;

        Assert.That(beforeSecondNavigation, Is.EqualTo(afterSecondNavigation));
    }

    [Test]
    public void NavigateTo_ActuallyNavigatesTo()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        Assert.That(_navigationService.NavigationStore.CurrentViewModel, Is.TypeOf<ViewModelWithoutNavigate>());
    }

    [Test]
    public void NavigateTo_PassesItselfToViewModel()
    {
        _navigationService.NavigateTo<ViewModelWithNavigationForTest>();

        if (_navigationService.NavigationStore.CurrentViewModel is ViewModelWithNavigationForTest currentVm)
        {
            Assert.That(currentVm.NavigationService, Is.EqualTo(_navigationService));
        }
        else
        {
            throw new Exception("Something really bizarre has happened");
        }
    }
}