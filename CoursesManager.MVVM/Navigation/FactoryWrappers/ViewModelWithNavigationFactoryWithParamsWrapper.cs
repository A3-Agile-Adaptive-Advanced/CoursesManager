using CoursesManager.MVVM.Data;

namespace CoursesManager.MVVM.Navigation.FactoryWrappers;

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