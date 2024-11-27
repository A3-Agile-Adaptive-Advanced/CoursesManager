using CoursesManager.MVVM.Env;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;

namespace CoursesManager.UI.Repositories.CourseRepository
{
    public class CourseRepository : ICourseRepository
    {
        private readonly CourseDataAccess _courseDataAccess = new();

        public List<Course> GetAll()
        {
            return _courseDataAccess.GetAll();
        }

        public Course? GetById(int id) => _courseDataAccess.FetchOneById(id);

        public void Add(Course course)
        {
            ArgumentNullException.ThrowIfNull(course);

            _courseDataAccess.Add(course);
        }

        public void Update(Course course)
        {
            ArgumentNullException.ThrowIfNull(course);

            _courseDataAccess.Update(course);
        }

        public void Delete(Course course)
        {
            ArgumentNullException.ThrowIfNull(course);

            _courseDataAccess.Delete(course.ID);
        }

        public void Delete(int id) => _courseDataAccess.Delete(id);

        public bool HasActiveRegistrations(Course course)
        {
            throw new NotImplementedException();
        }

        public void SetInactive(Course course)
        {
            throw new NotImplementedException();
        }
    }
}