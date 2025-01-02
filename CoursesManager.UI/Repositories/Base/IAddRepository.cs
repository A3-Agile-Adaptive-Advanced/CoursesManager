namespace CoursesManager.UI.Repositories.Base;

public interface IAddRepository<in T>
{
    void Add(T data);
}