using CoursesManager.MVVM.Dialogs;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace CoursesManager.Dialogs.Tests;

public class DialogResultTests
{
    [Test]
    public void SetOutcome_WhenProvidedOutcome_SetsOutcome()
    {
        var test = DialogResult<int>.Builder().SetOutcome(DialogOutcome.Canceled).Build();

        Assert.That(test.Outcome, Is.EqualTo(DialogOutcome.Canceled));
    }

    [Test]
    public void SetData_WhenProvidedData_SetsData()
    {
        var test = DialogResult<int>.Builder().SetData(10).Build();

        Assert.That(test.Data, Is.EqualTo(10));
    }

    [Test]
    public void SetOutcome_ThrowsArgumentException_WhenGivenEmptyString()
    {
        Assert.Throws<ArgumentException>(() => DialogResult<int>.Builder().SetOutcomeMessage(string.Empty));
    }
    
    [Test]
    public void SetOutcome_ThrowsArgumentException_WhenGivenNull()
    {
        Assert.Throws<ArgumentNullException>(() => DialogResult<int>.Builder().SetOutcomeMessage(null));
    }

    [Test]
    public void SetSuccess_SetsResultToSuccessful()
    {
        var test = DialogResult<int>.Builder().SetSuccess(10, "message").Build();

        Assert.That(test.Data, Is.EqualTo(10));
        Assert.That(test.OutcomeMessage, Is.EqualTo("message"));
        Assert.That(test.Outcome, Is.EqualTo(DialogOutcome.Success));
    }

    [Test]
    public void SetFailure_SetsResultToFailure()
    {
        var test = DialogResult<int>.Builder().SetFailure("message").Build();

        Assert.That(test.OutcomeMessage, Is.EqualTo("message"));
        Assert.That(test.Outcome, Is.EqualTo(DialogOutcome.Failure));
    }

    [Test]
    public void SetCanceled_SetsResultToCancelled()
    {
        var test = DialogResult<int>.Builder().SetCanceled("message").Build();

        Assert.That(test.OutcomeMessage, Is.EqualTo("message"));
        Assert.That(test.Outcome, Is.EqualTo(DialogOutcome.Canceled));
    }
}