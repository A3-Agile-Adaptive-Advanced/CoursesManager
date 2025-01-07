using System.Collections.ObjectModel;
using CoursesManager.UI.DataAccess;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.Base;
using CoursesManager.UI.Repositories.LocationRepository;
using CoursesManager.UI.Service;

namespace CoursesManager.UI.Repositories.CourseRepository
{
    public class CourseRepository : BaseRepository<Course>, ICourseRepository
    {
        private readonly CourseDataAccess _courseDataAccess;
        private readonly ILocationRepository _locationRepository;
        private readonly IStudentRegistrationCourseAggregator _studentRegistrationCourseAggregator;

        private readonly ObservableCollection<Course> _courses;

        private const string Cachekey = "coursesCache";

        private static readonly object SharedLock = new();

        public CourseRepository(CourseDataAccess courseDataAccess, ILocationRepository locationRepository,
            IStudentRegistrationCourseAggregator studentRegistrationCourseAggregator)
        {
            _studentRegistrationCourseAggregator = studentRegistrationCourseAggregator;
            _courseDataAccess = courseDataAccess;
            _locationRepository = locationRepository;

            try
            {
                _courses = GlobalCache.Instance.Get(Cachekey) as ObservableCollection<Course> ?? SetupCache(Cachekey);
            }
            catch
            {
                _courses = SetupCache(Cachekey);
            }
            finally
            {
                GetAll();
            }
        }

        public List<Course> GetAllBetweenDates(DateTime start, DateTime end)
        {
            return _courseDataAccess.GetAllBetweenDates(start, end);
        }

        public List<Course> RefreshAll()
        {
            lock (SharedLock)
            {
                if (_courses.Count == 0)
                {
                    _courseDataAccess.GetAll().ForEach(c =>
                    {
                        _courses.Add(c);
                        c.Location = _locationRepository.GetById(c.LocationId);
                    });

                    _studentRegistrationCourseAggregator.AggregateFromCourses(_courses);
                }

                return _courses;
            }
        }

        public Course? GetById(int id)
        {
            lock (SharedLock)
            {
                var item = _courses.FirstOrDefault(c => c.Id == id);

                if (item is null)
                {
                    item = _courseDataAccess.GetById(id);

                    if (item is not null) _courses.Add(item);
                }

                return item;
            }
        }

        public void Add(Course course)
        {
            lock (SharedLock)
            {
                _courseDataAccess.Add(course);
                _courses.Add(course);
            }
        }

        public void Update(Course course)
        {
            lock (SharedLock)
            {
                ArgumentNullException.ThrowIfNull(course);

                _courseDataAccess.Update(course);

                var item = GetById(course.Id) ?? throw new InvalidOperationException($"Course with id: {course.Id} does not exist.");

                OverwriteItemInPlace(item, course);
            }
        }

        public void Delete(Course course)
        {
            ArgumentNullException.ThrowIfNull(course);

            Delete(course.Id);
        }

        public void Delete(int id)
        {
            lock (SharedLock)
            {
                _courseDataAccess.Delete(id);

                var item = GetById(id) ?? throw new InvalidOperationException($"Course with id: {id} does not exist.");

                _courses.Remove(item);
            }
        }
    }
}