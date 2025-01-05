using CoursesManager.UI.Models;
using System.Collections.ObjectModel;

namespace CoursesManager.UI.Repositories.CourseRepository;

public class DummyCourseRepository : ICourseRepository
{
    private readonly ObservableCollection<Course> _courses;

    public DummyCourseRepository(ObservableCollection<Course> courses)
    {
        _courses = courses ?? throw new ArgumentNullException(nameof(courses), "Courses collection cannot be null.");
    }

    public List<Course> GetAll()
    {
        return _courses?.ToList() ?? new List<Course>();
    }

    public List<Course> RefreshAll()
    {
        throw new NotImplementedException();
    }

    public Course GetById(int id)
    {
        if (id <= 0) throw new ArgumentException("Course Id must be greater than zero.", nameof(id));

        return _courses.FirstOrDefault(c => c.Id == id);
    }

    public void Add(Course course)
    {
        if (course == null) throw new ArgumentNullException(nameof(course), "Course cannot be null.");

        course.DateCreated = DateTime.Now;
        course.Id = _courses.Any() ? _courses.Max(c => c.Id) + 1 : 1;
        _courses.Add(course);
    }

    public void Update(Course course)
    {
        if (course == null) throw new ArgumentNullException(nameof(course), "Course cannot be null.");

        var existingCourse = GetById(course.Id);
        if (existingCourse == null) throw new InvalidOperationException($"Course with Id {course.Id} does not exist.");

        existingCourse.Name = course.Name;
        existingCourse.Description = course.Description;
        existingCourse.IsActive = course.IsActive;
        existingCourse.Category = course.Category;
        existingCourse.StartDate = course.StartDate;
        existingCourse.EndDate = course.EndDate;
        existingCourse.LocationId = course.LocationId;
        existingCourse.Image = course.Image;
    }

    public void Delete(Course course)
    {
        _courses.Remove(course);
    }

    public void Delete(int id)
    {
        var course = GetById(id);
        if (course == null) throw new InvalidOperationException($"Course with Id {id} does not exist.");

        _courses.Remove(course);
    }

    public bool HasActiveRegistrations(Course course)
    {
        return false;
    }

    public void SetInactive(Course course)
    {
        course.IsActive = false;
    }

    public List<Course> GetAllBetweenDates(DateTime start, DateTime end)
    {
        throw new NotImplementedException();
    }
}