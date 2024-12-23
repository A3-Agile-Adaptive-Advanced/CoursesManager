﻿using CoursesManager.MVVM.Mail;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CertificateRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.TemplateRepository;
using DinkToPdf;
using iText.Html2pdf;
using System.Diagnostics;
using System.IO;
using System.Net.Mail;
using CoursesManager.MVVM.Mail.MailService;


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
        private List<MailResult> mailResults = new();
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
            try
            {
                List<MailMessage> messages = new();
                Template originalTemplate = GetTemplateByName("CertificateMail");

                foreach (Student student in course.Students)
                {
                    Registration? registration = student.Registrations.FirstOrDefault(r => r.CourseId == course.Id);
                    if (registration.IsAchieved)
                    {
                        byte[] certificate = GeneratePdf(course, student);
                        Template template = originalTemplate.Copy();
                        template.HtmlString = FillTemplate(template.HtmlString, student, course, null);
                        messages.Add(CreateMessage(student.Email, template.SubjectString, template.HtmlString, certificate));
                    }
                }
                if (messages.Any())
                {
                    mailResults = await _mailService.SendMail(messages);
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
            //Console.WriteLine(course.Students[1].Id);
            try
            {
                List<MailMessage> messages = new();
                Template originalTemplate = GetTemplateByName("CourseStartMail");

                foreach (Student student in course.Students)
                {
                    Template template = originalTemplate.Copy();
                    template.HtmlString = FillTemplate(template.HtmlString, student, course, null);

                    messages.Add(CreateMessage(student.Email, template.SubjectString, template.HtmlString, null));
                }
                if (messages.Any())
                {
                   return await _mailService.SendMail(messages);
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
            List<Registration> courseRegistrations = course.Registrations;
            Template originalTemplate = GetTemplateByName("PaymentMail");

            try
            {
                foreach (Registration registration in courseRegistrations)
                {

                    if (!registration.PaymentStatus)
                    {
                        Student student = course.Students.FirstOrDefault(s => s.Id == registration.StudentId);
                        Template template = originalTemplate.Copy();

                        template.HtmlString = FillTemplate(template.HtmlString, student, course, $"https://tinyurl.com/CourseManager/{student.Id}");
                        messages.Add(CreateMessage(student.Email, template.SubjectString, template.HtmlString, null));
                    }
                }
                if (messages.Any())
                {
                    return await _mailService.SendMail(messages);
                }
                return mailResults;
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }
        }

        #region Helper methods
        private byte[] GeneratePdf(Course course, Student student)
        {
            try
            {
                Template template = GetTemplateByName("Certificate");
                template.HtmlString = FillTemplate(template.HtmlString, student, course, null);
                using (var memoryStream = new MemoryStream())
                {
                    // Since the way a Certificate is saved is using a html string we can seperate the actual pdf from the template.
                    // First we are converting the html to pdf, in the event of failure the html is also not saved to the db, preventing the storage of a faulty html string.
                    HtmlConverter.ConvertToPdf(template.HtmlString, memoryStream);
                    SaveCertificate(template, course, student);
                    return memoryStream.ToArray();
                }

            }
            catch (Exception ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }
        }

        private Template GetTemplateByName(string name)
        {
                return _templateRepository.GetTemplateByName(name) ??
                       throw new NullReferenceException("Template 'Certificate' not found.");
        }

        private string FillTemplate(string template, Student student, Course course, string? URL)
        {
            try
            {
                template = course.ReplaceCoursePlaceholders(template, course);
                template = student.ReplaceStudentPlaceholders(template, student);
            }
            catch (Exception ex)
            {
                LogUtil.Error(ex.Message);
                throw;
            }

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
                var stream = new MemoryStream(certificate);
                var attachment = new Attachment(stream, "certificate.pdf", "application/pdf");
                message.Attachments.Add(attachment);
            }
            return message;
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
    }
}
