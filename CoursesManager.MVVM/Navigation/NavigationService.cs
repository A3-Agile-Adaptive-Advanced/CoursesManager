using CoursesManager.MVVM.Data;

namespace CoursesManager.MVVM.Navigation;

public class NavigationService : INavigationService
{
    public NavigationStore NavigationStore { get; } = new();

    private readonly Stack<ViewModel> _forwardViewModels = new();

    private readonly Stack<ViewModel> _backwardViewModels = new();

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

        _forwardViewModels.Clear();

        if (NavigationStore.CurrentViewModel is not null)
        {
            _backwardViewModels.Push(NavigationStore.CurrentViewModel);
        }

        var existingViewModel = _backwardViewModels.OfType<TViewModel>().FirstOrDefault();

        NavigationStore.CurrentViewModel = existingViewModel ?? CreateViewModel<TViewModel>(factory, parameter);
    }

    private ViewModel CreateViewModel<TViewModel>(Delegate factory, object? parameter) where TViewModel : ViewModel
    {
        if (typeof(ViewModelWithNavigation).IsAssignableFrom(typeof(TViewModel)))
        {
            if (factory is Func<object?, NavigationService, ViewModelWithNavigation> viewModelWithNavigationFactoryWithParams)
            {
                return viewModelWithNavigationFactoryWithParams(parameter, this);
            }

            if (factory is Func<INavigationService, ViewModelWithNavigation> viewModelWithNavigationFactory)
            {
                return viewModelWithNavigationFactory(this);
            }
        }

        if (factory is Func<ViewModel> simpleFactory)
        {
            return simpleFactory();
        }

        if (factory is Func<object?, ViewModel> simpleFactoryWithParams)
        {
            return simpleFactoryWithParams(parameter);
        }

        throw new InvalidOperationException($"Invalid factory type for {typeof(TViewModel).Name}");
    }

    public void GoBack()
    {
        if (!CanGoBack()) return;

        if (NavigationStore.CurrentViewModel is not null)
        {
            _forwardViewModels.Push(NavigationStore.CurrentViewModel);
        }

        NavigationStore.CurrentViewModel = _backwardViewModels.Pop();
    }

    public void GoBackAndClearForward()
    {
        GoBack();
        _forwardViewModels.Clear();
    }

    public bool CanGoBack() => _backwardViewModels.Any();

    public void GoForward()
    {
        if (!CanGoForward()) return;

        if (NavigationStore.CurrentViewModel is not null)
        {
            _backwardViewModels.Push(NavigationStore.CurrentViewModel);
        }

        NavigationStore.CurrentViewModel = _forwardViewModels.Pop();
    }

    public bool CanGoForward() => _forwardViewModels.Any();
}