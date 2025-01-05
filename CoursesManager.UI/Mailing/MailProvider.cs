using System.Collections.ObjectModel;
using CoursesManager.MVVM.Mail;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CertificateRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.TemplateRepository;
using DinkToPdf;
using iText.Html2pdf;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using CoursesManager.MVVM.Exceptions;
using CoursesManager.MVVM.Mail.MailService;
using System.Text.RegularExpressions;


namespace CoursesManager.UI.Mailing
{
    public class MailProvider : IMailProvider
    {
        #region Services
        private readonly IMailService _mailService;
        private readonly ITemplateRepository _templateRepository;
        private readonly ICertificateRepository _certificateRepository;
        #endregion
        #region Attributes
        #endregion

        public MailProvider(IMailService mailService, ITemplateRepository templateRepository,
            ICertificateRepository certificateRepository)
        {
            _mailService = mailService;
            _templateRepository = templateRepository;
            _certificateRepository = certificateRepository;
        }

        public async Task<List<MailResult>> SendCertificates(Course course)
        {
            List<MailResult> allMailResults = new();
            List<MailMessage> messages = new();

            if (course.Students == null)
                throw new InvalidOperationException("There are no students attached to this course");

            Template originalTemplate = GetTemplateByName("CertificateMail");

            foreach (var student in course.Students)
            {
                MailMessage message;
                try
                {
                    message = CreateCertificateMessage(student, course, originalTemplate);
                    if (message != null)
                        messages.Add(message);
                }
                catch (InvalidOperationException)
                {
                }
            }

            if (messages.Any())
                allMailResults = await _mailService.SendMail(messages);

            return allMailResults;
        }

        public async Task<List<MailResult>> SendCourseStartNotifications(Course course)
        {
            List<MailResult> allMailResults = new();
            List<MailMessage> messages = new();
            if (course.Students == null)
                throw new InvalidOperationException("There are no students attached to this course");

            var originalTemplate = GetTemplateByName("CourseStartMail");

            ProcessResult processResult = ProcessDataAndCreateMessages(course.Students, originalTemplate, course);


            if (processResult.Messages.Any())
            {
                processResult.Results.AddRange(await _mailService.SendMail(processResult.Messages));
            }
            return processResult.Results;
        }

        public async Task<List<MailResult>> SendPaymentNotifications(Course course)
        {
            if (course.Students == null)
                throw new InvalidOperationException("There are no students attached to this course");

            var allMailResults = new List<MailResult>();
            var originalTemplate = GetTemplateByName("PaymentMail");

            var unpaidStudents = new ObservableCollection<Student>(course.Registrations
                .Where(r => !r.PaymentStatus)
                .Select(r => course.Students.FirstOrDefault(s => s.Id == r.StudentId))
                .Where(s => s != null)
                .ToList());

            if (!unpaidStudents.Any())
                return allMailResults;

            var processResult = ProcessDataAndCreateMessages(unpaidStudents, originalTemplate, course, "https://tinyurl.com/CourseManager/{student.Id}");

            allMailResults.AddRange(processResult.Results);

            if (processResult.Messages.Any())
            {
                allMailResults.AddRange(await _mailService.SendMail(processResult.Messages));
            }
            return allMailResults;
        }

        #region Helper methods

        private ProcessResult ProcessDataAndCreateMessages(ObservableCollection<Student> students,
            Template originalTemplate,
            Course course,
            string? url = null)
        {
            var processResult = new ProcessResult();
            var messages = new List<MailMessage>();

            foreach (var student in students)
            {
                if (ValidateStudentEmail(student))
                {
                    processResult.Results.Add(new MailResult { Outcome = MailOutcome.Failure, StudentName = $"{student.FirstName} {student.Insertion} {student.LastName}" });
                    continue;
                }

                var template = originalTemplate.Copy();
                template.HtmlString = FillTemplate(template.HtmlString, student, course, url);

                processResult.Messages.Add(CreateMessage(student.Email, template.SubjectString, template.HtmlString));
            }

            return processResult;
        }

        private MailMessage? CreateCertificateMessage(Student student, Course course, Template originalTemplate)
        {
            if (ValidateStudentEmail(student))
                throw new InvalidOperationException("Student email is not valid");

            Registration? registration = student.Registrations.FirstOrDefault(r => r.CourseId == course.Id);
            if (registration?.IsAchieved != true)
                return null;

            byte[]? certificate = GeneratePdf(course, student);
            if (certificate == null)
                return null;

            Template template = originalTemplate.Copy();
            template.HtmlString = FillTemplate(template.HtmlString, student, course, null);
            return CreateMessage("jarnogerrets@gmail.com", template.SubjectString, template.HtmlString, certificate);
        }
        private MailMessage CreateMessage(string toMail, string subject, string body, byte[]? certificate = null)
        {
            MailMessage message = new MailMessage();
            message.To.Add(toMail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            if (certificate != null)
            {
                var stream = new MemoryStream(certificate);
                var attachment = new Attachment(stream, "certificate.pdf", "application/pdf");
                message.Attachments.Add(attachment);
            }
            return message;
        }
        private bool ValidateStudentEmail(Student student)
        {
            if (!Regex.IsMatch(student.Email, @"^[^@]+@[^@]+\.[^@\s]{2,}$"))
            {
                return true;
            }
            return false;
        }
        private byte[]? GeneratePdf(Course course, Student student)
        {
            Template template = GetTemplateByName("Certificate");
            template.HtmlString = FillTemplate(template.HtmlString, student, course);
            using (var memoryStream = new MemoryStream())
            {
                // Since the way a Certificate is saved is using a html string we can seperate the actual pdf from the template.
                // First we are converting the html to pdf, in the event of failure the html is also not saved to the db, preventing the storage of a faulty html string.
                HtmlConverter.ConvertToPdf(template.HtmlString, memoryStream);
                SaveCertificate(template, course, student);
                return memoryStream.ToArray();
            }
        }
        private Template GetTemplateByName(string name)
        {
            try
            {
                return _templateRepository.GetTemplateByName(name) ??
                       throw new TemplateNotFoundException($"Template '{name}' not found.");
            }
            catch (DataAccessException)
            {
                throw new TemplateNotFoundException($"Template '{name}' not found.");
            }
        }
        private string FillTemplate(string template, Student student, Course course, string? URL = null)
        {
            template = course.ReplaceCoursePlaceholders(template, course);
            template = student.ReplaceStudentPlaceholders(template, student);

            if (URL != null)
            {
                template = template.Replace("[Betaal Link]", URL);
            }
            return template;
        }
        private void SaveCertificate(Template template, Course course, Student student)
        {
            Certificate certificate = new();
            certificate.PdfString = template.HtmlString;
            certificate.StudentCode = student.Id;
            certificate.CourseCode = course.Code;
            _certificateRepository.Add(certificate);

        }
        #endregion
        private class ProcessResult
        {
            public List<MailResult> Results { get; set; } = new();
            public List<MailMessage> Messages { get; set; } = new();
        }
    }
}
