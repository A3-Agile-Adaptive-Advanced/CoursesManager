namespace CoursesManager.UI.Models
{
    public class Registration
    {
        public int Id { get; set; }
        public int StudentId { get; set; }
        public Student Student { get; set; }
        public int CourseId { get; set; }
        public  Course Course { get; set; }
        public DateTime RegistrationDate { get; set; }
        public bool PaymentStatus { get; set; }
        public bool IsActive { get; set; }
        public bool IsAchieved { get; set; }
    }
}
