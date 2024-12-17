using CoursesManager.MVVM.Mail;
using CoursesManager.UI.Models;

namespace CoursesManager.UI.Mailing
{
    public interface IMailProvider
    {
        public Task<List<MailResult>> SendCertificates(Course course);
        public Task<List<MailResult>> SendCourseStartNotifications(Course course);
        public Task<List<MailResult>> SendPaymentNotifications(Course course);
        public byte[] GeneratePDF(Course course, Student student);
    }
}