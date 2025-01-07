using System.Collections.ObjectModel;

namespace CoursesManager.UI.Repositories.Base;

public interface IReadOnlyRepository<T>
{
    //List<T> GetAll();
    ObservableCollection<T> GetAll();

    T? GetById(int id);
}