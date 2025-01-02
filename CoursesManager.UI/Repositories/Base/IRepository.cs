namespace CoursesManager.UI.Repositories.Base;

public interface IRepository<T> : IUpdateRepository<T>, IReadOnlyRepository<T>, IDeleteRepository<T>, IAddRepository<T>
{

}