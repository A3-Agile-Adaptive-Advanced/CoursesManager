using CoursesManager.MVVM.Data;

namespace CoursesManager.MVVM.Navigation.FactoryWrappers;

public class SimpleViewModelFactoryWithParams(
    Func<object?, ViewModel> viewModelFactory,
    object? parameter) : IViewModelFactoryWrapper
{
    public ViewModel Create()
    {
        return viewModelFactory(parameter);
    }
}