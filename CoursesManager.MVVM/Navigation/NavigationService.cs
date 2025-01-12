using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Navigation.FactoryWrappers;

namespace CoursesManager.MVVM.Navigation;

/// <summary>
/// Provides navigation functionality, including navigating to different view models and managing navigation history.
/// </summary>
public class NavigationService : INavigationService
{
    /// <summary>
    /// Gets the <see cref="NavigationStore"/> that manages navigation history.
    /// </summary>
    public NavigationStore NavigationStore { get; } = new();

    private readonly Stack<IViewModelFactoryWrapper> _forwardFactories = new();
    private readonly Stack<IViewModelFactoryWrapper> _backwardFactories = new();

    private IViewModelFactoryWrapper? _currentViewModelFactoryWrapper;

    /// <summary>
    /// Navigates to the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to navigate to, which must be a subclass of <see cref="ViewModel"/>.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown when no factory is registered for the specified view model type.</exception>
    public void NavigateTo<TViewModel>() where TViewModel : ViewModel
    {
        NavigateTo<TViewModel>(null);
    }

    /// <summary>
    /// Navigates to the specified view model type with parameters.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to navigate to, which must be a subclass of <see cref="ViewModel"/>.</typeparam>
    /// <param name="parameter">A parameter to pass on to a different viewmodel.</param>
    /// <exception cref="InvalidOperationException">Thrown when no factory is registered for the specified view model type.</exception>
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

    /// <summary>
    /// Creates a wrapper with the factory inside of it. This wrapper has the ability to group the given parameter with a given factory so that it can be navigated to and saved in forward and backward stacks.
    /// </summary>
    /// <typeparam name="TViewModel"></typeparam>
    /// <param name="factory"></param>
    /// <param name="parameter"></param>
    /// <returns></returns>
    /// <exception cref="InvalidOperationException"></exception>
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

    /// <summary>
    /// Navigates back to the previous view model in the navigation history.
    /// </summary>
    public void GoBack()
    {
        if (!CanGoBack()) return;

        if (_currentViewModelFactoryWrapper is not null) _forwardFactories.Push(_currentViewModelFactoryWrapper);

        _currentViewModelFactoryWrapper = _backwardFactories.Pop();

        NavigationStore.CurrentViewModel = _currentViewModelFactoryWrapper.Create();
    }

    /// <summary>
    /// Navigates back to the previous view model in the navigation history but also ensures you can't go forward again.
    /// </summary>
    public void GoBackAndClearForward()
    {
        GoBack();
        _forwardFactories.Clear();
    }

    /// <summary>
    /// Determines whether it is possible to navigate back to the previous view model.
    /// </summary>
    /// <returns><c>true</c> if it is possible to go back; otherwise, <c>false</c>.</returns>
    public bool CanGoBack() => _backwardFactories.Count != 0;

    /// <summary>
    /// Navigates forward to the next view model in the navigation history.
    /// </summary>
    public void GoForward()
    {
        if (!CanGoForward()) return;

        if (_currentViewModelFactoryWrapper is not null) _backwardFactories.Push(_currentViewModelFactoryWrapper);

        _currentViewModelFactoryWrapper = _forwardFactories.Pop();

        NavigationStore.CurrentViewModel = _currentViewModelFactoryWrapper.Create();
    }

    /// <summary>
    /// Determines whether it is possible to navigate forward to the next view model.
    /// </summary>
    /// <returns><c>true</c> if it is possible to go forward; otherwise, <c>false</c>.</returns>
    public bool CanGoForward() => _forwardFactories.Count != 0;
}