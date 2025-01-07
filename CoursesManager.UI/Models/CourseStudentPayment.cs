using CoursesManager.MVVM.Data;

namespace CoursesManager.UI.Models
{

    /// <summary>
    /// Class designed to make data available to the UI.
    /// This class makes it possible to present the payment status in multiple pages without adding any extra code.
    /// Using overloading for the constructor this can be used this both on courses and on students.
    /// This class removes the need for extensive if loops in a viewmodel to make the data available to the UI.
    /// </summary>
    public class CourseStudentPayment : IsObservable
    {
        private bool _isPaid;
        private bool _isAchieved;
        public Student? Student { get; set; }
        public Course? Course { get; set; }
        public bool IsPaid
        {
            get => _isPaid;
            set
            {
                if (value == false && IsAchieved)
                {
                    return;
                }
                SetProperty(ref _isPaid, value);
            }
        }
        public bool IsAchieved
        {
            get => _isAchieved;
            set => SetProperty(ref _isAchieved, value);
        }
        public string? FullName { get; set; }

        public CourseStudentPayment(Student student, Registration registration)
        {
            Student = student;
            IsPaid = registration.PaymentStatus;
            FullName = $"{student.FirstName} {student.Insertion} {student.LastName}";
            IsAchieved = registration.IsAchieved;
        }

        public CourseStudentPayment(Course course, Registration registration)
        {
            if (course == null)
            {
                throw new ArgumentNullException(nameof(course), "Course cannot be null");
            }
            FullName = course.Name;
            Course = course;
            IsPaid = registration.PaymentStatus;
            IsAchieved = registration.IsAchieved;
        }
    }
}
