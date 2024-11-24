namespace CoursesManager.UI.Repositories;

public interface IRepository<T>
{
    List<T> GetAll();

    T? GetById(int id);

    void Add(T data);

    void Update(T data);

    void Delete(T data);

    void Delete(int id);
}