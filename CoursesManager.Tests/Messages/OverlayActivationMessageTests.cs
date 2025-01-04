using CoursesManager.MVVM.Messages.DefaultMessages;

namespace CoursesManager.MVVM.Tests.Messages.DefaultMessages;

public class OverlayActivationMessageTests
{
    [Test]
    public void Clone_CreatesNewInstanceWithSameValues()
    {
        var original = new OverlayActivationMessage(true);

        var cloned = original.Clone();

        Assert.That(cloned, Is.Not.SameAs(original));
        Assert.That(cloned.IsVisible, Is.EqualTo(original.IsVisible));
        Assert.That(cloned.MessageId, Is.EqualTo(original.MessageId));
        Assert.That(cloned.TimeStamp, Is.EqualTo(original.TimeStamp));
    }

    [Test]
    public void Clone_DoesNotShareReferencesWithOriginal()
    {
        var original = new OverlayActivationMessage(true);

        var cloned = original.Clone();

        Assert.That(cloned, Is.Not.SameAs(original));
        Assert.That(cloned.IsVisible, Is.EqualTo(original.IsVisible));
        Assert.That(cloned.MessageId, Is.EqualTo(original.MessageId));
        Assert.That(cloned.TimeStamp, Is.EqualTo(original.TimeStamp));
    }

    [Test]
    public void Clone_ChangesInOriginal_DoNotAffectClonedInstance()
    {
        var original = new OverlayActivationMessage(true);

        var cloned = original.Clone();

        original.IsVisible = false;

        Assert.That(cloned.IsVisible, Is.EqualTo(true));
        Assert.That(cloned.MessageId, Is.EqualTo(original.MessageId));
        Assert.That(cloned.TimeStamp, Is.EqualTo(original.TimeStamp));
    }
}