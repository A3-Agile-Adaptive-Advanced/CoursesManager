using CoursesManager.MVVM.Messages.DefaultMessages;

namespace CoursesManager.MVVM.Tests.Messages.DefaultMessages;

public class ApplicationCloseRequestedMessageTests
{
    [Test]
    public void Clone_CreatesNewInstanceWithSameValues()
    {
        // Arrange
        var original = new ApplicationCloseRequestedMessage();

        // Act
        var cloned = original.Clone();

        // Assert
        Assert.That(cloned, Is.Not.SameAs(original), "Clone should return a new instance.");
        Assert.That(cloned.MessageId, Is.EqualTo(original.MessageId), "Cloned MessageId property should match original.");
        Assert.That(cloned.TimeStamp, Is.EqualTo(original.TimeStamp), "Cloned TimeStamp property should match original.");
    }

    [Test]
    public void Clone_DoesNotShareReferencesWithOriginal()
    {
        // Arrange
        var original = new ApplicationCloseRequestedMessage();

        // Act
        var cloned = original.Clone();

        // Assert
        Assert.That(cloned, Is.Not.SameAs(original), "Cloned object should not reference the original.");
        Assert.That(cloned.MessageId, Is.EqualTo(original.MessageId), "MessageId should be identical as a value but not a shared reference.");
        Assert.That(cloned.TimeStamp, Is.EqualTo(original.TimeStamp), "Cloned TimeStamp should match original.");
    }
}