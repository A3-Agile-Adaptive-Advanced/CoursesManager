using CoursesManager.MVVM.Data;

namespace CoursesManager.MVVM.Navigation.FactoryWrappers;

public class ViewModelWithNavigationFactoryWrapper(
    Func<INavigationService, ViewModelWithNavigation> viewModelFactory,
    NavigationService navigationService) : IViewModelFactoryWrapper
{
    public ViewModel Create()
    {
        return viewModelFactory(navigationService);
    }
}