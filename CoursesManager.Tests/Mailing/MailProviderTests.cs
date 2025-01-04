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
    #region Happy flow
    [Test]
    public async Task Test_Send_Certificates_When_Students_Are_Eligible()
    {
        var course = new Course
        {
            Id = 1
        };

        var student1 = new Student
        {
            Id = 1,
            Email = "x@x.com"
        };

        var student2 = new Student
        {
            Id = 2,
            Email = "x@x.com"
        };

        var registration1 = new Registration
        {
            Id = 1,
            Course = course,
            CourseId = course.Id,
            IsAchieved = true,
            IsActive = true,
            PaymentStatus = false,
            RegistrationDate = DateTime.Now,
            Student = student1,
            StudentId = student1.Id
        };

        var registration2 = new Registration
        {
            Id = 1,
            Course = course,
            CourseId = course.Id,
            IsAchieved = true,
            IsActive = true,
            PaymentStatus = false,
            RegistrationDate = DateTime.Now,
            Student = student2,
            StudentId = student2.Id
        };

        student1.Registrations = new ObservableCollection<Registration> { registration1 };
        student2.Registrations = new ObservableCollection<Registration> { registration2 };
        course.Registrations = new ObservableCollection<Registration> { registration1, registration2 };
        

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
        var course = new Course { Id = 1 };

        course.Registrations = new ObservableCollection<Registration>
        {
            new Registration
            {
                Id = 1,
                Course = course,
                CourseId = course.Id,
                IsAchieved = true,
                IsActive = true,
                PaymentStatus = true,
                RegistrationDate = DateTime.Now,
                Student = new Student { Id = 1, Email = "x@x.com" },
                StudentId = 1
            },
            new Registration
            {
                Id = 1,
                Course = course,
                CourseId = course.Id,
                IsAchieved = true,
                IsActive = true,
                PaymentStatus = true,
                RegistrationDate = DateTime.Now,
                Student = new Student { Id = 2, Email = "x@x.com" },
                StudentId = 2
            }
        };

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
            Id = 1
        };

        course.Registrations = new ObservableCollection<Registration>
        {
            new Registration
            {
                Id = 1,
                Course = course,
                CourseId = course.Id,
                IsAchieved = true,
                IsActive = true,
                PaymentStatus = false,
                RegistrationDate = DateTime.Now,
                Student = new Student
                {
                    Id = 1,
                    Email = "x@x.com"
                },
                StudentId = 1
            }
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
            Id = 1
        };

        course.Registrations = new ObservableCollection<Registration>
        {
            new Registration
                { StudentId = 1, Student = new Student { Id = 1, Email = "x@x.com" }, PaymentStatus = false },
            new Registration
                { StudentId = 2, Student = new Student { Id = 2, Email = "x@x.com" }, PaymentStatus = true },
            new Registration
                { StudentId = 3, Student = new Student { Id = 3, Email = "x@x.com" }, PaymentStatus = false },
            new Registration
                { StudentId = 4, Student = new Student { Id = 4, Email = "x@x.com" }, PaymentStatus = true },
            new Registration
                { StudentId = 5, Student = new Student { Id = 5, Email = "x@x.com" }, PaymentStatus = false },
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
#endregion
    #region Unhappy flow
    [Test]
    public async Task Test_Null_Certificate_Should_Throw_Exception()
    {
        // Arrange
        var course = new Course { Id = 1 };

        var student = new Student
        {
            Id = 1,
            Email = "x@x.com"
        };

        var registration = new Registration
        {
            Id = 1,
            Course = course,
            CourseId = course.Id,
            IsAchieved = true,
            IsActive = true,
            PaymentStatus = true,
            RegistrationDate = DateTime.Now,
            Student = student,
            StudentId = 1
        };

        student.Registrations = new ObservableCollection<Registration> { registration };

        course.Registrations = new ObservableCollection<Registration>
        {
            registration
        };

        var templateMail = new Template { Name = "CertificateMail", HtmlString = "[Cursist naam]", SubjectString = "CertificateMail" };
        Template? templateCertificate = null;

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("CertificateMail"))
            .Returns(templateMail);
        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("Certificate"))
            .Returns(templateCertificate);

        _mockMailService.Setup(service => service.SendMail(It.IsAny<List<MailMessage>>()))
            .ReturnsAsync(new List<MailResult> { new MailResult { Outcome = MailOutcome.Success } });

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _mailProvider.SendCertificates(course));
        Assert.That(exception.Message, Is.EqualTo("Template 'Certificate' not found."));
    }

    [Test]
    public async Task Test_Null_Template_Should_Throw_Exception()
    {
        // Arrange
        var course = new Course { Id = 1 };

        course.Registrations = new ObservableCollection<Registration>
        {
            new Registration
            {
                Id = 1,
                Course = course,
                CourseId = course.Id,
                IsAchieved = true,
                IsActive = true,
                PaymentStatus = true,
                RegistrationDate = DateTime.Now,
                Student = new Student
                {
                    Id = 1,
                    Email = "x@x.com"
                },
                StudentId = 1
            }
        };

        Template? template = null;

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("PaymentMail"))
            .Returns(template);

        _mockMailService.Setup(service => service.SendMail(It.IsAny<List<MailMessage>>()))
            .ReturnsAsync(new List<MailResult> { new MailResult { Outcome = MailOutcome.Success } });

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _mailProvider.SendPaymentNotifications(course));
        Assert.That(exception.Message, Is.EqualTo("Template 'PaymentMail' not found."));
    }

    [Test]
    public async Task Test_No_Students_Should_Throw_Exception()
    {
        // Arrange
        var course = new Course
        {
            Id = 1
        };
        var template = new Template { HtmlString = "[Placeholder]", SubjectString = "Payment Due" };

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("PaymentMail"))
            .Returns(template);

        _mockMailService.Setup(service => service.SendMail(It.IsAny<List<MailMessage>>()))
            .ReturnsAsync(new List<MailResult> { new MailResult { Outcome = MailOutcome.Success } });

        // Act & Assert
        var exception = Assert.ThrowsAsync<InvalidOperationException>(async () => await _mailProvider.SendPaymentNotifications(course));
        Assert.That(exception.Message, Is.EqualTo("There are no students attached to this course"));
    }

    [Test]
    public async Task Test_Empty_Email_Should_Throw_Exception()
    {
        // Arrange
        var course = new Course
        {
            Id = 1
        };

        course.Registrations = new ObservableCollection<Registration>
        {
            new Registration
                { StudentId = 1, Student = new Student { Id = 1, Email = "" }, PaymentStatus = false },
            new Registration
                { StudentId = 2, Student = new Student { Id = 2, Email = "x@x.com" }, PaymentStatus = false }
        };

        var template = new Template { HtmlString = "[Placeholder]", SubjectString = "Payment Due" };

        _mockTemplateRepository.Setup(repo => repo.GetTemplateByName("PaymentMail"))
            .Returns(template);

        MailAddress email = new MailAddress("x@x.com");
        _mockMailService.Setup(service => service.SendMail(It.Is<List<MailMessage>>(messages =>
                    messages.Any(m => m.To.Contains(email))
            )))
            .ReturnsAsync(new List<MailResult>
            {
                new MailResult { Outcome = MailOutcome.Success },
                new MailResult { Outcome = MailOutcome.Failure }
            });

        // Act
        var results = await _mailProvider.SendPaymentNotifications(course);

        // Assert
        Assert.That(results, Is.Not.Empty);
        Assert.That(results.Count, Is.EqualTo(3));
        Assert.That(results[0].Outcome, Is.EqualTo(MailOutcome.Failure));
        Assert.That(results[1].Outcome, Is.EqualTo(MailOutcome.Success));
    }
    #endregion
}
