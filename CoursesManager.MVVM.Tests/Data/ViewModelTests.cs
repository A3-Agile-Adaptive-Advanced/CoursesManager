using CoursesManager.MVVM.Data;
using CoursesManager.MVVM.Messages;
using CoursesManager.MVVM.Messages.DefaultMessages;
using Moq;

namespace CoursesManager.MVVM.Tests.Data;

internal class TestViewModel : ViewModel
{
}

public class ViewModelTests
{
    private TestViewModel _viewModel;

    [SetUp]
    public void Setup()
    {
        _viewModel = new TestViewModel();
    }

    [Test]
    public void AddError_AddsErrorMessageToProperty()
    {
        var propertyName = "TestProperty";
        var errorMessage = "This is an error.";

        _viewModel.AddError(propertyName, errorMessage);

        Assert.That(_viewModel.HasErrors, Is.True);
        var errors = _viewModel.GetErrors(propertyName) as List<string>;
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors, Contains.Item(errorMessage));
    }

    [Test]
    public void ClearErrors_RemovesAllErrorsFromProperty()
    {
        var propertyName = "TestProperty";
        _viewModel.AddError(propertyName, "Error 1");
        _viewModel.AddError(propertyName, "Error 2");

        _viewModel.ClearErrors(propertyName);

        Assert.That(_viewModel.HasErrors, Is.False);
        var errors = _viewModel.GetErrors(propertyName) as List<string>;
        Assert.That(errors, Is.Not.Null);
        Assert.That(errors.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetErrors_ReturnsEmptyList_WhenNoErrors()
    {
        var propertyName = "NonExistentProperty";

        var errors = _viewModel.GetErrors(propertyName) as List<string>;

        Assert.That(errors, Is.Not.Null);
        Assert.That(errors.Count, Is.EqualTo(0));
    }

    [Test]
    public void GetErrors_ReturnsEmptyList_WhenPropertyNameIsNull()
    {
        string? propertyName = null;

        var errors = _viewModel.GetErrors(propertyName) as List<string>;

        Assert.That(errors, Is.Not.Null);
        Assert.That(errors.Count, Is.EqualTo(0));
    }

    [Test]
    public void ErrorsChanged_EventIsRaised_WhenErrorIsAdded()
    {
        var propertyName = "TestProperty";
        bool eventRaised = false;
        _viewModel.ErrorsChanged += (sender, args) =>
        {
            if (args.PropertyName == propertyName)
            {
                eventRaised = true;
            }
        };

        _viewModel.AddError(propertyName, "Error message");

        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void ErrorsChanged_EventIsRaised_WhenErrorIsCleared()
    {
        var propertyName = "TestProperty";
        _viewModel.AddError(propertyName, "Error message");
        bool eventRaised = false;
        _viewModel.ErrorsChanged += (sender, args) =>
        {
            if (args.PropertyName == propertyName)
            {
                eventRaised = true;
            }
        };

        _viewModel.ClearErrors(propertyName);

        Assert.That(eventRaised, Is.True);
    }

    [Test]
    public void IsDialogOpen_RaisesPropertyChanged_WhenValueUpdated()
    {
        var eventRaised = false;
        var originalValue = _viewModel.IsDialogOpen;

        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.IsDialogOpen)) eventRaised = true;
        };

        _viewModel.IsDialogOpen = !originalValue;
        var newValue = _viewModel.IsDialogOpen;

        Assert.That(eventRaised, Is.True);
        Assert.That(originalValue, Is.Not.EqualTo(newValue));
    }

    [Test]
    public void ViewTitle_RaisesPropertyChanged_WhenValueUpdated()
    {
        var eventRaised = false;
        var originalValue = _viewModel.ViewTitle;

        _viewModel.PropertyChanged += (sender, e) =>
        {
            if (e.PropertyName == nameof(_viewModel.ViewTitle)) eventRaised = true;
        };

        _viewModel.ViewTitle = originalValue + "_changed";
        var newValue = _viewModel.ViewTitle;

        Assert.That(eventRaised, Is.True);
        Assert.That(originalValue, Is.Not.EqualTo(newValue));
    }

    [Test]
    public async Task ExecuteWithOverlayAsync_PublishesActivationMessagesInOrder()
    {
        var mockMessageBroker = new Mock<IMessageBroker>();
        var actionExecuted = false;

        async Task Action()
        {
            await Task.Delay(100);
            actionExecuted = true;
        }

        await _viewModel.ExecuteWithOverlayAsync(mockMessageBroker.Object, Action);

        mockMessageBroker.Verify(mb => mb.Publish(It.Is<OverlayActivationMessage>(m => m.IsVisible == true)), Times.Once);
        mockMessageBroker.Verify(mb => mb.Publish(It.Is<OverlayActivationMessage>(m => m.IsVisible == false)), Times.Once);
        Assert.That(actionExecuted, Is.True);
    }

    [Test]
    public void ExecuteWithOverlayAsync_EnsuresDeactivationOnException()
    {
        var mockMessageBroker = new Mock<IMessageBroker>();

        Func<Task> action = async () =>
        {
            await Task.Delay(100);
            throw new InvalidOperationException("Test exception");
        };

        var exception = Assert.ThrowsAsync<InvalidOperationException>(() =>
            _viewModel.ExecuteWithOverlayAsync(mockMessageBroker.Object, action)
        );

        Assert.That(exception, Is.Not.Null);
        Assert.That(exception.Message, Is.EqualTo("Test exception"));

        mockMessageBroker.Verify(mb => mb.Publish(It.Is<OverlayActivationMessage>(m => m.IsVisible == true)), Times.Once);
        mockMessageBroker.Verify(mb => mb.Publish(It.Is<OverlayActivationMessage>(m => m.IsVisible == false)), Times.Once);
    }
}