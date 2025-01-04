using CoursesManager.MVVM.Dialogs;
using CoursesManager.MVVM.Exceptions;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Dialogs.ResultTypes;
using CoursesManager.UI.Dialogs.ViewModels;
using CoursesManager.UI.Models;
using CoursesManager.UI.Repositories.TemplateRepository;
using CoursesManager.UI.ViewModels.Mailing;
using Moq;
using System.Windows.Documents;
using CoursesManager.UI.Messages;

namespace CoursesManager.Tests.Mailing
{
    [TestFixture]
    internal class EditTemplateViewModelTests
    {
        private Mock<IMessageBroker> _mockMessageBroker;
        private Mock<IDialogService> _mockDialogService;
        private Mock<ITemplateRepository> _mockTemplateRepository;
        private Mock<INavigationService> _mockNavigationService;
        private EditMailTemplatesViewModel _viewModel;

        [SetUp]
        public void Setup()
        {
            // Set up mocks
            _mockMessageBroker = new Mock<IMessageBroker>();
            _mockDialogService = new Mock<IDialogService>();
            _mockTemplateRepository = new Mock<ITemplateRepository>();
            _mockNavigationService = new Mock<INavigationService>();

            // Create a template object to return in the mock
            var mockTemplate = new Template
            {
                HtmlString = "<html><body>Test content for template.</body></html>",
                Name = "CertificateMail"
            };

            // Mock the repository method to return the mock template
            _mockTemplateRepository.Setup(repo => repo.GetTemplateByName(It.IsAny<string>()))
                .Returns(mockTemplate);

            // Set up the ViewModel
            _viewModel = new EditMailTemplatesViewModel(
                _mockTemplateRepository.Object,
                _mockDialogService.Object,
                _mockMessageBroker.Object,
                _mockNavigationService.Object
            );
        }

        [Test]
        public void Test_Save_Valid_Template_Updates_Template_And_Shows_Confirmation()
        {
            // Arrange
            string validTemplateContent = "This is a valid template with [Cursus naam] and [Cursus locatie naam].";
            _viewModel.VisibleText = new System.Windows.Documents.FlowDocument(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(validTemplateContent)));
            _mockTemplateRepository.Setup(repo => repo.Update(It.IsAny<Template>()));

            // Act
            _viewModel.SaveTemplate();

            // Assert
            _mockTemplateRepository.Verify(repo => repo.Update(It.IsAny<Template>()), Times.Once);
            _mockMessageBroker.Verify(mb => mb.Publish(It.Is<ToastNotificationMessage>(msg => msg.NotificationText.Contains("Template opgeslagen"))), Times.Once);
        }

        [Test]
        public void Test_ShowMailCommand_Should_Update_VisibleText()
        {
            // Arrange
            string templateName = "CertificateMail";
            var template = new Template { HtmlString = "<html><body>New HTML content</body></html>" };
            _mockTemplateRepository.Setup(repo => repo.GetTemplateByName(templateName)).Returns(template);

            // Act
            _viewModel.ShowMailCommand.Execute(templateName);

            // Assert
            var visibleText = GetTextFromFlowDocument(_viewModel.VisibleText);
            Assert.That(visibleText, Is.EqualTo("New HTML content"));

            string GetTextFromFlowDocument(FlowDocument document)
            {
                if (document == null)
                    return string.Empty;

                var textRange = new TextRange(document.ContentStart, document.ContentEnd);
                return textRange.Text.TrimEnd();
            }
        }



        [Test]
        public void Test_Open_Template_Viewer_Should_Show_Dialog()
        {
            // Arrange
            var dialogResultType = new DialogResultType { DialogTitle = "Test Template", DialogText = "Test content" };
            _mockDialogService.Setup(ds => ds.ShowDialogAsync<TemplatePreviewDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()))
                .ReturnsAsync(DialogResult<DialogResultType>.Builder()
                    .SetSuccess(new DialogResultType { Result = true })
                    .Build()); ;

            // Act
            _viewModel.OpenTemplateViewer();

            // Assert
            _mockDialogService.Verify(ds => ds.ShowDialogAsync<TemplatePreviewDialogViewModel, DialogResultType>(It.IsAny<DialogResultType>()), Times.Once);
        }

        [Test]
        public void Test_Save_Invalid_Template_Shows_Warning()
        {
            // Arrange
            string templateContent = "This is a test with an invalid placeholder: [Invalid Placeholder]";
            _viewModel.VisibleText = new System.Windows.Documents.FlowDocument(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(templateContent)));
            _mockTemplateRepository.Setup(repo => repo.Update(It.IsAny<Template>())).Throws<System.Exception>();

            // Act
            _viewModel.SaveTemplateCommand.Execute(null);

            // Assert
            _mockMessageBroker.Verify(mb => mb.Publish(It.Is<ToastNotificationMessage>(msg => msg.NotificationText.Contains("incorrect"))), Times.Once);
        }

        [Test]
        public void Test_Retrieve_Template_With_Invalid_Name_Shows_Warning()
        {
            // Arrange
            string templateName = "InvalidTemplate";
            string validTemplateContent = "This is a valid template with [Cursus naam] and [Cursus locatie naam].";
            _viewModel.VisibleText = new System.Windows.Documents.FlowDocument(new System.Windows.Documents.Paragraph(new System.Windows.Documents.Run(validTemplateContent)));
            _mockTemplateRepository.Setup(repo => repo.Update(It.IsAny<Template>()));

            // Mocking the repository to throw an exception when trying to fetch the template
            _mockTemplateRepository.Setup(repo => repo.GetTemplateByName(templateName))
                .Throws(new DataAccessException());

            // Act
            _viewModel.ShowMailCommand.Execute(templateName);

            // Assert
            _mockMessageBroker.Verify(mb => mb.Publish(It.Is<ToastNotificationMessage>(msg => msg.NotificationText.Contains("fout"))), Times.Once);

        }

    }
}
