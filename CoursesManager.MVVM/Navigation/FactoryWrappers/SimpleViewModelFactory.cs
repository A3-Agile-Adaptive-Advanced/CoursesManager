using CoursesManager.MVVM.Data;

namespace CoursesManager.MVVM.Navigation.FactoryWrappers;

public class SimpleViewModelFactory(
    Func<ViewModel> viewModelFactory) : IViewModelFactoryWrapper
{
    public ViewModel Create()
    {
        return viewModelFactory();
    }
}