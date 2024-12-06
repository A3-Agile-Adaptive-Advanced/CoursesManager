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
            throw new NotImplementedException();
        }

        public List<Course> RefreshAll()
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

            _courseDataAccess.Delete(course.Id);
        }

        public void Delete(int id) => _courseDataAccess.Delete(id);
    }
}