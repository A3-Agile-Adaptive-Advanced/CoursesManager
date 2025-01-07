namespace CoursesManager.UI.Repositories.Base;

public interface IUpdateRepository<in T>
{
    void Update(T data);
}