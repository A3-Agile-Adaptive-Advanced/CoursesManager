using CoursesManager.MVVM.Data;

namespace CoursesManager.MVVM.Navigation;

/// <summary>
/// Provides navigation functionality, including navigating to different view models and managing navigation history.
/// </summary>
public interface INavigationService
{
    /// <summary>
    /// Gets the <see cref="NavigationStore"/> that manages navigation history.
    /// </summary>
    NavigationStore NavigationStore { get; }

    /// <summary>
    /// Navigates to the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to navigate to, which must be a subclass of <see cref="ViewModel"/>.</typeparam>
    /// <exception cref="InvalidOperationException">Thrown when no factory is registered for the specified view model type.</exception>
    void NavigateTo<TViewModel>() where TViewModel : ViewModel;

    /// <summary>
    /// Navigates to the specified view model type with parameters.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to navigate to, which must be a subclass of <see cref="ViewModel"/>.</typeparam>
    /// <param name="parameter">A parameter to pass on to a different viewmodel.</param>
    /// <exception cref="InvalidOperationException">Thrown when no factory is registered for the specified view model type.</exception>
    void NavigateTo<TViewModel>(object parameter) where TViewModel : ViewModel;

    /// <summary>
    /// Navigates back to the previous view model in the navigation history.
    /// </summary>
    void GoBack();

    /// <summary>
    /// Navigates back to the previous view model in the navigation history but also ensures you can't go forward again.
    /// </summary>
    void GoBackAndClearForward();

    /// <summary>
    /// Determines whether it is possible to navigate back to the previous view model.
    /// </summary>
    /// <returns><c>true</c> if it is possible to go back; otherwise, <c>false</c>.</returns>
    bool CanGoBack();

    /// <summary>
    /// Navigates forward to the next view model in the navigation history.
    /// </summary>
    void GoForward();

    /// <summary>
    /// Determines whether it is possible to navigate forward to the next view model.
    /// </summary>
    /// <returns><c>true</c> if it is possible to go forward; otherwise, <c>false</c>.</returns>
    bool CanGoForward();

    /// <summary>
    /// Contains factories for creating instances of view models used in navigation.
    /// </summary>
    public static readonly Dictionary<Type, Delegate> ViewModelFactories = new();

    /// <summary>
    /// Registers a factory for creating instances of the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to register, which must be a subclass of <see cref="ViewModelWithNavigation"/>.</typeparam>
    /// <param name="viewModelFactory">A factory function that creates an instance of the view model.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="viewModelFactory"/> is null.</exception>
    public static void RegisterViewModelFactoryWithParameters<TViewModel>(Func<object?, INavigationService, TViewModel> viewModelFactory)
        where TViewModel : ViewModelWithNavigation
    {
        ArgumentNullException.ThrowIfNull(viewModelFactory);

        if (ViewModelFactories.ContainsKey(typeof(TViewModel))) return;

        ViewModelFactories.Add(typeof(TViewModel), viewModelFactory);
    }

    /// <summary>
    /// Registers a factory for creating instances of the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to register, which must be a subclass of <see cref="ViewModelWithNavigation"/>.</typeparam>
    /// <param name="viewModelFactory">A factory function that creates an instance of the view model.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="viewModelFactory"/> is null.</exception>
    public static void RegisterViewModelFactoryWithParameters<TViewModel>(Func<object, TViewModel> viewModelFactory)
        where TViewModel : ViewModel
    {
        ArgumentNullException.ThrowIfNull(viewModelFactory);

        if (ViewModelFactories.ContainsKey(typeof(TViewModel))) return;

        ViewModelFactories.Add(typeof(TViewModel), viewModelFactory);
    }

    /// <summary>
    /// Registers a factory for creating instances of the specified view model type.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to register, which must be a subclass of <see cref="ViewModelWithNavigation"/>.</typeparam>
    /// <param name="viewModelFactory">A factory function that creates an instance of the view model.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="viewModelFactory"/> is null.</exception>
    public static void RegisterViewModelFactory<TViewModel>(Func<INavigationService, TViewModel> viewModelFactory)
        where TViewModel : ViewModelWithNavigation
    {
        ArgumentNullException.ThrowIfNull(viewModelFactory);

        if (ViewModelFactories.ContainsKey(typeof(TViewModel))) return;

        ViewModelFactories.Add(typeof(TViewModel), viewModelFactory);
    }

    /// <summary>
    /// Registers a factory for creating instances of the specified view model type. Specifically for viewmodels that do not need a INavigation service.
    /// </summary>
    /// <typeparam name="TViewModel">The type of the view model to register, which must be a subclass of <see cref="ViewModelWithNavigation"/>.</typeparam>
    /// <param name="viewModelFactory">A factory function that creates an instance of the view model.</param>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="viewModelFactory"/> is null.</exception>
    public static void RegisterViewModelFactory<TViewModel>(Func<TViewModel> viewModelFactory)
        where TViewModel : ViewModel
    {
        ArgumentNullException.ThrowIfNull(viewModelFactory);

        if (ViewModelFactories.ContainsKey(typeof(TViewModel))) return;

        ViewModelFactories.Add(typeof(TViewModel), viewModelFactory);
    }
}