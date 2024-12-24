using CoursesManager.UI.Repositories.StudentRepository;

namespace CoursesManager.UI.Services
{
    /// <summary>
    /// Service responsible for cleaning up student records marked as deleted.
    /// Automatically removes records that have been deleted for over a specified time.
    /// </summary>
    public class StudentCleanupService
    {
        private readonly IStudentRepository _studentRepository;

        public StudentCleanupService(IStudentRepository studentRepository)
        {
            _studentRepository = studentRepository;
        }

        /// <summary>
        /// Cleans up student records that were marked as deleted more than two years ago.
        /// </summary>
        public void CleanupDeletedStudents()
        {
            var students = _studentRepository.GetDeletedStudents();
            var maxHoldYears = DateTime.Now.AddYears(-2);

            foreach (var student in students)
            {
                if (student.DeletedAt.HasValue && student.DeletedAt.Value < maxHoldYears)
                {
                    _studentRepository.Delete(student.Id);
                }
            }
        }
    }
}