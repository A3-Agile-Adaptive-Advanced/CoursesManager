using CoursesManager.MVVM.Commands;

namespace CoursesManager.MVVM.Tests.Commands;

public class RelayCommandGenericTests
{
    [Test]
    public void Constructor_ThrowsException_WhenExecuteDelegateIsNull()
    {
        Assert.Throws<ArgumentNullException>(() => new RelayCommand<string>(null));
    }

    [Test]
    public void CanExecute_ReturnsTrue_WhenCanExecuteIsNull()
    {
        var command = new RelayCommand<string>(_ => { });

        bool result = command.CanExecute("test");

        Assert.That(result, Is.True);
    }

    [Test]
    public void CanExecute_UsesPredicate_WhenCanExecuteIsProvided()
    {
        var command = new RelayCommand<string>(_ => { }, param => param == "valid");

        Assert.That(command.CanExecute("valid"), Is.True);
        Assert.That(command.CanExecute("invalid"), Is.False);
    }

    [Test]
    public void CanExecute_ThrowsException_WhenParameterIsInvalid()
    {
        var command = new RelayCommand<string>(_ => { });

        Assert.Throws<ArgumentException>(() => command.CanExecute(123)); // 123 is an invalid type
    }

    [Test]
    public void CanExecute_ReturnsFalse_WhenCanExecutePredicateIsFalse()
    {
        var command = new RelayCommand<int>(_ => { }, param => param > 10);

        bool result = command.CanExecute(5);

        Assert.That(result, Is.False);
    }

    [Test]
    public void CanExecute_ReturnsTrue_WhenCanExecutePredicateIsTrue()
    {
        var command = new RelayCommand<int>(_ => { }, param => param > 10);

        bool result = command.CanExecute(15);

        Assert.That(result, Is.True);
    }

    [Test]
    public void CanExecute_ReturnsFalse_WhenParameterIsNull()
    {
        object testParam = null;
        var command = new RelayCommand<int>(_ => { }, param => true);

        Assert.That(command.CanExecute(testParam), Is.False);
    }

    [Test]
    public void CanExecute_ReturnsTrue_WhenValidObjectIsProvided()
    {
        object testParam = 10;
        var command = new RelayCommand<int>(_ => { }, param => true);

        Assert.That(command.CanExecute(testParam), Is.True);
    }

    [Test]
    public void Execute_CallsAction_WhenParameterIsValid()
    {
        bool actionExecuted = false;
        var command = new RelayCommand<string>(_ => actionExecuted = true);

        command.Execute("test");

        Assert.That(actionExecuted, Is.True);
    }

    [Test]
    public void Execute_CallsAction_WhenParameterIsValidObject()
    {
        bool actionExecuted = false;
        var command = new RelayCommand<string>(_ => actionExecuted = true);
        object value = "test";


        command.Execute(value);

        Assert.That(actionExecuted, Is.True);
    }

    [Test]
    public void CanExecute_ThrowsException_WhenNullParameterIsInvalid()
    {
        var command = new RelayCommand<int>(_ => { });

        Assert.Throws<ArgumentException>(() =>
        {
            command.Execute(null);
        });
    }

    [Test]
    public void Execute_ThrowsException_WhenParameterIsInvalid()
    {
        var command = new RelayCommand<string>(_ => { });

        Assert.Throws<ArgumentException>(() => command.Execute(123)); // 123 is an invalid type
    }

    [Test]
    public void Execute_CallsAction_WithCorrectParameter()
    {
        string expected = "expected";
        string actual = string.Empty;
        var command = new RelayCommand<string>(param => actual = param);

        command.Execute(expected);

        Assert.That(actual, Is.EqualTo(expected));
    }
}