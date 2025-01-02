using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation.FactoryWrappers;

namespace CoursesManager.MVVM.Navigation;

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

    public bool CanGoBack() => _backwardFactories.Count != 0;

    public void GoForward()
    {
        if (!CanGoForward()) return;

        if (_currentViewModelFactoryWrapper is not null) _backwardFactories.Push(_currentViewModelFactoryWrapper);

        _currentViewModelFactoryWrapper = _forwardFactories.Pop();

        NavigationStore.CurrentViewModel = _currentViewModelFactoryWrapper.Create();
    }

    public bool CanGoForward() => _forwardFactories.Count != 0;
}