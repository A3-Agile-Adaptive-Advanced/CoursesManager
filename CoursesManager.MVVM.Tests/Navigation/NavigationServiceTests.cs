﻿using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation;
#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace CoursesManager.MVVM.Tests.Navigation;

internal class ViewModelWithNavigationForTest : ViewModelWithNavigation
{
    public INavigationService NavigationService { get; set; }

    public object? PassedObject;

    public ViewModelWithNavigationForTest(INavigationService navigationService) : base(navigationService)
    {
        NavigationService = navigationService;
    }

    public ViewModelWithNavigationForTest(object? param, INavigationService? navigationService) : base(navigationService)
    {
        NavigationService = navigationService;
        PassedObject = param;
    }
}

internal class ViewModelWithoutNavigate : ViewModel
{
    public object? PassedObject;

    public ViewModelWithoutNavigate()
    {

    }

    public ViewModelWithoutNavigate(object? param)
    {
        PassedObject = param;
    }
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
        INavigationService.RegisterViewModelFactory((nav) => new ViewModelWithNavigationForTest(nav));
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
            INavigationService.RegisterViewModelFactory((Func<INavigationService, ViewModelWithNavigationForTest>)null);
        });
    }

    [Test]
    public void RegisterViewModelFactoryWithNaviagtionService_RegistrationAdded()
    {
        INavigationService.ViewModelFactories.Clear();

        INavigationService.RegisterViewModelFactory((navigationService) => new ViewModelWithNavigationForTest(navigationService));

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtLeast(1));
    }

    [Test]
    public void RegisterViewModelFactoryWithNaviagtionService_OnlyAddedOnce()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactory((navigationService) => new ViewModelWithNavigationForTest(navigationService));
        INavigationService.RegisterViewModelFactory((navigationService) => new ViewModelWithNavigationForTest(navigationService));

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtMost(1));
    }

    [Test]
    public void RegisterViewModelFactoryWithParametersWithNaviagtionService_ThrowsArgumentException_WhenViewModelFactoryIsNull()
    {
        INavigationService.ViewModelFactories.Clear();

        Assert.Throws<ArgumentNullException>(() =>
        {
            INavigationService.RegisterViewModelFactoryWithParameters<ViewModelWithNavigationForTest>((Func<object?, INavigationService, ViewModelWithNavigationForTest>)null);
        });
    }

    [Test]
    public void RegisterViewModelFactoryWithParametersWithNaviagtionService_RegistrationAdded()
    {
        INavigationService.ViewModelFactories.Clear();

        INavigationService.RegisterViewModelFactoryWithParameters((param, navigationService) => new ViewModelWithNavigationForTest(param, navigationService));

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtLeast(1));
    }

    [Test]
    public void RegisterViewModelFactoryWithParametersWithNaviagtionService_OnlyAddedOnce()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactoryWithParameters((param, navigationService) => new ViewModelWithNavigationForTest(param, navigationService));
        INavigationService.RegisterViewModelFactoryWithParameters((param, navigationService) => new ViewModelWithNavigationForTest(param, navigationService));

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtMost(1));
    }

    [Test]
    public void RegisterViewModelFactoryWithParameters_ThrowsArgumentException_WhenViewModelFactoryIsNull()
    {
        INavigationService.ViewModelFactories.Clear();

        Assert.Throws<ArgumentNullException>(() =>
        {
            INavigationService.RegisterViewModelFactoryWithParameters<ViewModelWithoutNavigate>(null);
        });
    }

    [Test]
    public void RegisterViewModelFactoryWithParameters_RegistrationAdded()
    {
        INavigationService.ViewModelFactories.Clear();

        INavigationService.RegisterViewModelFactoryWithParameters((param) => new ViewModelWithoutNavigate(param));

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtLeast(1));
    }

    [Test]
    public void RegisterViewModelFactoryWithParameters_OnlyAddedOnce()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactoryWithParameters((param) => new ViewModelWithoutNavigate(param));
        INavigationService.RegisterViewModelFactoryWithParameters((param) => new ViewModelWithoutNavigate(param));

        Assert.That(INavigationService.ViewModelFactories.Count, Is.AtMost(1));
    }

    [Test]
    public void CanGoBack_ReturnsFalse_WhenNoForwardNavigationHasOccured()
    {
        Assert.That(_navigationService.CanGoBack(), Is.False);
    }

    [Test]
    public void CanGoBack_ReturnsTrue_WhenForwardNavigationHasOccured()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        Assert.That(_navigationService.CanGoBack(), Is.True);
    }

    [Test]
    public void CanGoBack_ReturnsFalse_WhenItWentBackToBeginning()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();

        Assert.That(_navigationService.CanGoBack(), Is.False);
    }

    [Test]
    public void CanGoBack_ReturnsFalse_WhenItHasNotGoneAllTheWayBack()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();

        Assert.That(_navigationService.CanGoBack(), Is.True);
    }

    [Test]
    public void CanGoForward_ReturnsFalse_WhenNoBackwardHasHappened()
    {
        Assert.That(_navigationService.CanGoForward(), Is.False);
    }

    [Test]
    public void CanGoForward_ReturnsFalse_WhenNoBackwardHasHappenedAfterNavigation()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();

        Assert.That(_navigationService.CanGoForward(), Is.False);
    }

    [Test]
    public void CanGoForward_ReturnsTrue_WhenBackwardsNavigationHasHappened()
    {
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.NavigateTo<ViewModelWithoutNavigate>();
        _navigationService.GoBack();

        Assert.That(_navigationService.CanGoForward(), Is.True);
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

        Assert.That(_navigationService.CanGoForward(), Is.False);
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

        Assert.That(_navigationService.CanGoForward(), Is.True);
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

        Assert.That(shouldHaveBeenTrue, Is.True);
        Assert.That(shouldHaveBeenFalse, Is.False);
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

        Assert.That(_navigationService.CanGoForward(), Is.False);
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

    [Test]
    public void NavigateTo_WhenPassingNavigationServiceAndObject_PassesOnObject()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactoryWithParameters((param) => new ViewModelWithoutNavigate(param));
        INavigationService.RegisterViewModelFactoryWithParameters((param, nav) => new ViewModelWithNavigationForTest(param, nav));

        var testObject = new object();

        _navigationService.NavigateTo<ViewModelWithNavigationForTest>(testObject);

        if (_navigationService.NavigationStore.CurrentViewModel is ViewModelWithNavigationForTest currentVm)
        {
            Assert.That(currentVm.PassedObject, Is.Not.Null);
            Assert.That(testObject, Is.SameAs(currentVm.PassedObject));
        }
        else
        {
            throw new Exception("Something really bizarre has happened");
        }
    }

    [Test]
    public void NavigateTo_WhenPassingObject_PassesOnObject()
    {
        INavigationService.ViewModelFactories.Clear();
        INavigationService.RegisterViewModelFactoryWithParameters((param) => new ViewModelWithoutNavigate(param));
        INavigationService.RegisterViewModelFactoryWithParameters((param, nav) => new ViewModelWithNavigationForTest(param, nav));

        var testObject = new object();

        _navigationService.NavigateTo<ViewModelWithoutNavigate>(testObject);

        if (_navigationService.NavigationStore.CurrentViewModel is ViewModelWithoutNavigate currentVm)
        {
            Assert.That(currentVm.PassedObject, Is.Not.Null);
            Assert.That(testObject, Is.SameAs(currentVm.PassedObject));
        }
        else
        {
            throw new Exception("Something really bizarre has happened");
        }
    }
}