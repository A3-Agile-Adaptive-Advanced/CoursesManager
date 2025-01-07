using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Navigation;
using CoursesManager.UI.Messages;
using CoursesManager.UI.Service;
using CoursesManager.UI.ViewModels;
using Moq;

namespace CoursesManager.Tests.Services
{
    [TestFixture]
    public class StartupManagerTests
    {
        private Mock<IConfigurationService> _configurationServiceMock;
        private Mock<INavigationService> _navigationServiceMock;
        private Mock<IMessageBroker> _messageBrokerMock;
        private StartupManager _startupManager;

        [SetUp]

        public void SetUp()
        {
            _configurationServiceMock = new Mock<IConfigurationService>();
            _navigationServiceMock = new Mock<INavigationService>();
            _messageBrokerMock = new Mock<IMessageBroker>();

            _startupManager = new StartupManager(
                _configurationServiceMock.Object,
                _navigationServiceMock.Object,
                _messageBrokerMock.Object
                );
        }

        [Test]
        public void CheckConfigurationOnStartup_ShouldNavigateToStartPage_WhenConfigurationIsValid()
        {
            //arrange
            _configurationServiceMock
                .Setup(cs => cs.ValidateSettings())
                .Returns( true );

            //act
            _startupManager.CheckConfigurationOnStartup();

            //assert
            _navigationServiceMock.Verify(ns => ns.NavigateTo<CoursesManagerViewModel>(), Times.Once,
                "Er moet genavigeerd worden naar de startpagina als de configuratie geldig is.");

            _messageBrokerMock.Verify(mb => mb.Publish(It.IsAny<ToastNotificationMessage>()), Times.Once,
                "Er moet een notificatie worden verstuurd als de configuratie geldig is.");
        }

        [Test]
        public void CheckConfigurationOnStartup_ShouldOpenConfigurationUi_WhenConfigurationIsInvalid()
        {
            // Arrange
            _configurationServiceMock
                .Setup(cs => cs.ValidateSettings())
                .Returns(false);

            // Act
            _startupManager.CheckConfigurationOnStartup();

            // Assert
            _navigationServiceMock.Verify(ns => ns.NavigateTo<ConfigurationViewModel>(), Times.Once,
                "De configuratie-UI moet worden geopend als de configuratie ongeldig is.");
        }

        [Test]
        public void CheckConfigurationOnStartup_ShouldOpenConfigurationUi_WhenExceptionIsThrown()
        {
            // Arrange
            _configurationServiceMock
                .Setup(cs => cs.ValidateSettings())
                .Throws(new Exception("Test exception"));

            // Act
            _startupManager.CheckConfigurationOnStartup();

            // Assert
            _navigationServiceMock.Verify(ns => ns.NavigateTo<ConfigurationViewModel>(), Times.Once,
                "De configuratie-UI moet worden geopend als er een uitzondering wordt gegooid.");
        }

    }
}
