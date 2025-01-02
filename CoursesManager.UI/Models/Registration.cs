using CoursesManager.MVVM.Data;

namespace CoursesManager.UI.Models
{
    public class Registration : IsObservable
    {
        private int _id;

        public int Id
        {
            get => _id;
            set => SetProperty(ref _id, value);
        }

        private int _studentId;

        public int StudentId
        {
            get => _studentId;
            set => SetProperty(ref _studentId, value);
        }

        private Student? _student;

        public Student? Student
        {
            get => _student;
            set => SetProperty(ref _student, value);
        }

        private int _courseId;

        public int CourseId
        {
            get => _courseId;
            set => SetProperty(ref _courseId, value);
        }

        private Course? _course;

        public Course? Course
        {
            get => _course;
            set => SetProperty(ref _course, value);
        }

        private DateTime _registrationDate;

        public DateTime RegistrationDate
        {
            get => _registrationDate;
            set => SetProperty(ref _registrationDate, value);
        }

        private bool _paymentStatus;

        public bool PaymentStatus
        {
            get => _paymentStatus;
            set => SetProperty(ref _paymentStatus, value);
        }

        private bool _isActive;

        public bool IsActive
        {
            get => _isActive;
            set => SetProperty(ref _isActive, value);
        }

        private bool _isAchieved;

        public bool IsAchieved
        {
            get => _isAchieved;
            set => SetProperty(ref _isAchieved, value);
        }
    }
}