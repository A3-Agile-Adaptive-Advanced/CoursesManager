using CoursesManager.MVVM.Exceptions;
using CoursesManager.MVVM.Mail;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.TemplateRepository;
using DinkToPdf;
using System.IO;
using System.Net.Mail;



namespace CoursesManager.UI.Mailing
{
    public class MailProvider : IMailProvider
    {
        //service classes
        private readonly MailService mailService = new MailService();
        private readonly RegistrationRepository registrationRepository = new RegistrationRepository();
        private readonly TemplateRepository templateRepository = new TemplateRepository();

        // private attributes
        private List<Registration> courseRegistrations = new List<Registration>();
        private List<MailResult> mailResults = new List<MailResult>();

        public byte[] GeneratePDF(Course course, Student student)
        {
            string htmlContent = templateRepository.GetTemplateByName("Certificate").HtmlString;

            htmlContent = htmlContent
                .Replace("{{Cursus naam}}", course.Name)
                .Replace("{{Student naam}}", $"{student.FirstName} {student.LastName}")
                .Replace("{{Datum behalen cursus}}", course.EndDate.ToString("yyyy-MM-dd"));

            var converter = new SynchronizedConverter(new PdfTools());
            var doc = new HtmlToPdfDocument
            {
                GlobalSettings = new GlobalSettings
                {
                    ColorMode = ColorMode.Color,
                    Orientation = DinkToPdf.Orientation.Portrait,
                    PaperSize = PaperKind.A4,
                }
            };

            doc.Objects.Add(new ObjectSettings
            {
                HtmlContent = htmlContent,
                WebSettings = { DefaultEncoding = "utf-8" }
            });

            try
            {
                return converter.Convert(doc);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }
        }

        public async Task<List<MailResult>> SendCertificates(Course course)
        {
            try
            {
                List<MailMessage> messages = new();
                Template originalTemplate = templateRepository.GetTemplateByName("CertificateMail");
                foreach (Student student in course.Students)
                {
                    //byte[] certificate = GeneratePDF(course, student);
                    var template = originalTemplate.Copy();
                    template.HtmlString = FillTemplate(template.HtmlString, $"{student.FirstName} {student.LastName}", course.Name, null);
                    messages.Add(CreateMessage("jarnogerrets@gmail.com", template.SubjectString, template.HtmlString, null));
                }
                if (messages.Any())
                {
                    mailResults = await mailService.SendMail(messages);
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }
            return mailResults;
        }

        public async Task<List<MailResult>> SendCourseStartNotifications(Course course)
        {

            try
            {
                List<MailMessage> messages = new();
                Template originalTemplate = templateRepository.GetTemplateByName("CourseStartMail");

                foreach (Student student in course.Students)
                {
                    var template = originalTemplate.Copy();
                    template.HtmlString = FillTemplate(template.HtmlString, $"{student.FirstName} {student.LastName}", course.Name, null);
                    messages.Add(CreateMessage("jarnogerrets@gmail.com", template.SubjectString, template.HtmlString, null));
                }
                if (messages.Any())
                {
                    await mailService.SendMail(messages);
                }
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }
            return mailResults;
        }

        public async Task<List<MailResult>> SendPaymentNotifications(Course course)
        {
            List<MailMessage> messages = new List<MailMessage>();
            courseRegistrations = course.Registrations;
            Template originalTemplate = templateRepository.GetTemplateByName("PaymentMail");
            try
            {
                foreach (Registration registration in courseRegistrations)
                {

                    if (!registration.PaymentStatus)
                    {
                        Student student = course.Students.FirstOrDefault(s => s.Id == registration.StudentId);
                        var template = originalTemplate.Copy();
                        template.HtmlString = FillTemplate(template.HtmlString, $"{student.FirstName} {student.LastName}", course.Name, $"https://tinyurl.com/CourseManager/{student.Id}");
                        messages.Add(CreateMessage("jarnogerrets@gmail.com", template.SubjectString, template.HtmlString, null));
                    }
                }

                if (messages.Any())
                {
                    return await mailService.SendMail(messages);
                }
                return mailResults;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }
        }

        private string FillTemplate(string template, string name, string courseName, string? URL)
        {
            template = template.Replace("[Naam]", name);
            template = template.Replace("[Cursusnaam]", courseName);
            if (URL != null)
            {
                template = template.Replace("[Betaal Link]", URL);
            }
            return template;
        }

        private MailMessage CreateMessage(string toMail, string subject, string body, byte[]? certificate)
        {
            MailMessage message = new MailMessage();
            message.To.Add(toMail);
            message.Subject = subject;
            message.Body = body;
            message.IsBodyHtml = true;

            if (certificate != null)
            {
                foreach (var certBytes in certificate)
                {
                    // Convert byte array to MemoryStream and create an attachment
                    using var stream = new MemoryStream(certBytes);
                    var attachment = new Attachment(stream, "certificate.pdf", "application/pdf");
                    message.Attachments.Add(attachment);
                }
            }

            return message;
        }
    }
}
