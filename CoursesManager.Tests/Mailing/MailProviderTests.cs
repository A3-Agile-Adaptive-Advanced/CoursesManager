using System.Collections.ObjectModel;
using CoursesManager.MVVM.Mail;
using CoursesManager.MVVM.Mail.MailService;
using CoursesManager.UI.Mailing;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.CertificateRepository;
using CoursesManager.UI.Repositories.RegistrationRepository;
using CoursesManager.UI.Repositories.TemplateRepository;
using Moq;
using System.Net.Mail;
using MySqlX.XDevAPI.Common;

namespace CoursesManager.Tests.Mailing;
[TestFixture]
public class MailProviderTests
{
    private Mock<IMailService> _mockMailService;
    private Mock<ITemplateRepository> _mockTemplateRepository;
    private Mock<ICertificateRepository> _mockCertificateRepository;
    private MailProvider _mailProvider;

    [SetUp]
    public void Setup()
    {
        _mockMailService = new Mock<IMailService>();
        _mockTemplateRepository = new Mock<ITemplateRepository>();
        _mockCertificateRepository = new Mock<ICertificateRepository>();

        _mailProvider = new MailProvider(_mockMailService.Object, _mockTemplateRepository.Object, _mockCertificateRepository.Object);
    }

    [Test]
    public async Task Test_Send_Certificates_When_Students_Are_Eligible()
    {
        // Arrange
        var course = new Course { Id = 1, Students = new ObservableCollection<Student> { new Student { Id = 1, Email = "x@x.com", Registrations = new ObservableCollection<Registration> { new Registration { CourseId = 1, IsAchieved = true } } } } };
        var templateMail = new Template { Name = "CertificateMail", HtmlString = "[Cursist naam]", SubjectString = "CertificateMail" };
        var templateCertificate = new Template { Name = "Certificate", HtmlString = "[Cursist naam]", SubjectString = "Certificate" };

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("CertificateMail"))
            .Returns(templateMail);
        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("Certificate"))
            .Returns(templateCertificate);

        _mockMailService.Setup(service => service.SendMail(It.IsAny<List<MailMessage>>()))
            .ReturnsAsync(new List<MailResult> { new MailResult { Outcome = MailOutcome.Success } });

        // Act
        var results = await _mailProvider.SendCertificates(course);

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.All(r => r.Outcome == MailOutcome.Success));
    }

    [Test]
    public async Task Test_Send_Course_Start_Notifications()
    {
        // Arrange
        var course = new Course { Id = 1, Students = new ObservableCollection<Student> { new Student { Id = 1, Email = "x@x.com" }, new Student { Id = 2, Email = "x@x.com" } } };
        var template = new Template { Name = "CourseStartMail", HtmlString = "[Placeholder]", SubjectString = "Course Start" };

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("CourseStartMail"))
            .Returns(template);

        _mockMailService.Setup(service => service.SendMail(It.IsAny<List<MailMessage>>()))
            .ReturnsAsync(new List<MailResult> { new MailResult { Outcome = MailOutcome.Success } });

        // Act
        var results = await _mailProvider.SendCourseStartNotifications(course);

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.All(r => r.Outcome == MailOutcome.Success));
    }

    [Test]
    public async Task Test_Send_Payment_Notifications()
    {
        // Arrange
        var course = new Course
        {
            Id = 1,
            Students = new ObservableCollection<Student> { new Student { Id = 1, Email = "x@x.com" } },
            Registrations = new List<Registration> { new Registration { StudentId = 1, PaymentStatus = false } }
        };
        var template = new Template { HtmlString = "[Placeholder]", SubjectString = "Payment Due" };

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("PaymentMail"))
            .Returns(template);

        _mockMailService.Setup(service => service.SendMail(It.IsAny<List<MailMessage>>()))
            .ReturnsAsync(new List<MailResult> { new MailResult { Outcome = MailOutcome.Success } });

        // Act
        var results = await _mailProvider.SendPaymentNotifications(course);

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.All(r => r.Outcome == MailOutcome.Success));
    }

    [Test]
    public async Task Test_Send_Correct_Amount_Of_PaymentMails()
    {
        // Arrange
        var course = new Course
        {
            Id = 1,
            Students = new ObservableCollection<Student>
            {
                new Student { Id = 1, Email = "x@x.com"},
                new Student { Id = 2, Email = "x@x.com" },
                new Student { Id = 3, Email = "x@x.com" },
                new Student { Id = 4 , Email = "x@x.com"},
                new Student { Id = 5 , Email = "x@x.com"}
            },
            Registrations = new List<Registration>
            {
                new Registration { StudentId = 1, PaymentStatus = false },
                new Registration { StudentId = 2, PaymentStatus = true },
                new Registration { StudentId = 3, PaymentStatus = false },
                new Registration { StudentId = 4, PaymentStatus = true },
                new Registration { StudentId = 5, PaymentStatus = false }
            }
        };
        var template = new Template { HtmlString = "[Placeholder]", SubjectString = "Payment Due" };

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("PaymentMail"))
            .Returns(template);

        var sentMessages = new List<MailMessage>();
        _mockMailService
            .Setup(service => service.SendMail(It.IsAny<IEnumerable<MailMessage>>()))
            .Callback<IEnumerable<MailMessage>>(messages => sentMessages = messages.ToList())
            .ReturnsAsync(() => sentMessages.Select(_ => new MailResult { Outcome = MailOutcome.Success }).ToList());
        
        // Act
        var results = await _mailProvider.SendPaymentNotifications(course);

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(3));
        Assert.That(results.All(r => r.Outcome == MailOutcome.Success));

    }

    [Test]
    public async Task Test_Null_Certificate_Should_Throw_Exception()
    {
        // Arrange
        var course = new Course { Id = 1, Students = new ObservableCollection<Student> { new Student { Id = 1, Registrations = new ObservableCollection<Registration> { new Registration { CourseId = 1, IsAchieved = true } } } } };
        var templateMail = new Template { Name = "CertificateMail", HtmlString = "[Cursist naam]", SubjectString = "CertificateMail" };
        Template? templateCertificate = null;

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("CertificateMail"))
            .Returns(templateMail);
        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("Certificate"))
            .Returns(templateCertificate);

        _mockMailService.Setup(service => service.SendMail(It.IsAny<List<MailMessage>>()))
            .ReturnsAsync(new List<MailResult> { new MailResult { Outcome = MailOutcome.Success } });

        // Act & Assert
        var exception = Assert.ThrowsAsync<NullReferenceException>(async () => await _mailProvider.SendCertificates(course));
        Assert.That(exception.Message, Is.EqualTo("Template 'Certificate' not found."));
    }

    [Test]
    public async Task Test_Null_Template_Should_Throw_Exception()
    {
        // Arrange
        var course = new Course
        {
            Id = 1,
            Students = new ObservableCollection<Student> { new Student { Id = 1 } },
            Registrations = new List<Registration> { new Registration { StudentId = 1, PaymentStatus = false } }
        };
        Template? template = null;

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("PaymentMail"))
            .Returns(template);

        _mockMailService.Setup(service => service.SendMail(It.IsAny<List<MailMessage>>()))
            .ReturnsAsync(new List<MailResult> { new MailResult { Outcome = MailOutcome.Success } });

        // Act & Assert
        var exception = Assert.ThrowsAsync<NullReferenceException>(async () => await _mailProvider.SendPaymentNotifications(course));
        Assert.That(exception.Message, Is.EqualTo("Template 'Certificate' not found."));
    }

}
