using CoursesManager.MVVM.Data;

namespace CoursesManager.MVVM.Navigation;

internal interface IViewModelFactoryWrapper
{
    ViewModel Create();
}

internal class ViewModelWithNavigationFactoryWithParamsWrapper(
    Func<object?, NavigationService, ViewModelWithNavigation> viewModelFactory,
    object? parameter,
    NavigationService navigationService) : IViewModelFactoryWrapper
{
    public ViewModel Create()
    {
        return viewModelFactory(parameter, navigationService);
    }
}

public class ViewModelWithNavigationFactoryWrapper(
    Func<INavigationService, ViewModelWithNavigation> viewModelFactory,
    NavigationService navigationService) : IViewModelFactoryWrapper
{
    public ViewModel Create()
    {
        return viewModelFactory(navigationService);
    }
}

public class SimpleViewModelFactory(
    Func<ViewModel> viewModelFactory) : IViewModelFactoryWrapper
{
    public ViewModel Create()
    {
        return viewModelFactory();
    }
}

public class SimpleViewModelFactoryWithParams(
    Func<object?, ViewModel> viewModelFactory,
    object? parameter) : IViewModelFactoryWrapper
{
    public ViewModel Create()
    {
        return viewModelFactory(parameter);
    }
}

public class NavigationService : INavigationService
{
    public NavigationStore NavigationStore { get; } = new();

    private readonly Stack<IViewModelFactoryWrapper> _forwardFactories = new();
    private readonly Stack<IViewModelFactoryWrapper> _backwardFactories = new();

    private IViewModelFactoryWrapper? _currentViewModelFactoryWrapper;

    public void NavigateTo<TViewModel>() where TViewModel : ViewModel
    {
        NavigateTo<TViewModel>(null);
    }

    public void NavigateTo<TViewModel>(object? parameter) where TViewModel : ViewModel
    {
        if (!INavigationService.ViewModelFactories.TryGetValue(typeof(TViewModel), out var factory))
        {
            throw new InvalidOperationException($"No factory registered for {typeof(TViewModel).Name}");
        }

        _forwardFactories.Clear();

        if (_currentViewModelFactoryWrapper is not null)
        {
            _backwardFactories.Push(_currentViewModelFactoryWrapper);
        }

        _currentViewModelFactoryWrapper = CreateViewModelFactoryWrapper<TViewModel>(factory, parameter);

        NavigationStore.CurrentViewModel = _currentViewModelFactoryWrapper.Create();
    }

    private IViewModelFactoryWrapper CreateViewModelFactoryWrapper<TViewModel>(Delegate factory, object? parameter) where TViewModel : ViewModel
    {
        if (typeof(ViewModelWithNavigation).IsAssignableFrom(typeof(TViewModel)))
        {
            if (factory is Func<object?, NavigationService, ViewModelWithNavigation> viewModelWithNavigationFactoryWithParams)
            {
                return new ViewModelWithNavigationFactoryWithParamsWrapper(
                    viewModelWithNavigationFactoryWithParams,
                    parameter,
                    this);
            }

            if (factory is Func<INavigationService, ViewModelWithNavigation> viewModelWithNavigationFactory)
            {
                return new ViewModelWithNavigationFactoryWrapper(
                    viewModelWithNavigationFactory,
                    this);
            }
        }

        if (factory is Func<ViewModel> simpleViewModelFactory)
        {
            return new SimpleViewModelFactory(
                simpleViewModelFactory);
        }

        if (factory is Func<object?, ViewModel> simpleViewModelFactoryWithParams)
        {
            return new SimpleViewModelFactoryWithParams(
                simpleViewModelFactoryWithParams,
                parameter);
        }

        throw new InvalidOperationException($"Invalid factory type for {typeof(TViewModel).Name}");
    }

    public void GoBack()
    {
        if (!CanGoBack()) return;

        if (_currentViewModelFactoryWrapper is not null) _forwardFactories.Push(_currentViewModelFactoryWrapper);

        _currentViewModelFactoryWrapper = _backwardFactories.Pop();

        NavigationStore.CurrentViewModel = _currentViewModelFactoryWrapper.Create();
    }

    public void GoBackAndClearForward()
    {
        GoBack();
        _forwardFactories.Clear();
    }

    public bool CanGoBack() => _backwardFactories.Any();

    public void GoForward()
    {
        if (!CanGoForward()) return;

        if (_currentViewModelFactoryWrapper is not null) _backwardFactories.Push(_currentViewModelFactoryWrapper);

        _currentViewModelFactoryWrapper = _forwardFactories.Pop();

        NavigationStore.CurrentViewModel = _currentViewModelFactoryWrapper.Create();
    }

    public bool CanGoForward() => _forwardFactories.Any();
}