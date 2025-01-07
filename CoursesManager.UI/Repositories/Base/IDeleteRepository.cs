namespace CoursesManager.UI.Repositories.Base;

public interface IDeleteRepository<in T>
{
    void Delete(T data);
    void Delete(int id);
}