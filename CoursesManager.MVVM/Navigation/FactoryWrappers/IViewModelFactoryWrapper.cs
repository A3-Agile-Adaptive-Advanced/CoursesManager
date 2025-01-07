using CoursesManager.MVVM.Data;

namespace CoursesManager.MVVM.Navigation.FactoryWrappers;

internal interface IViewModelFactoryWrapper
{
    ViewModel Create();
}