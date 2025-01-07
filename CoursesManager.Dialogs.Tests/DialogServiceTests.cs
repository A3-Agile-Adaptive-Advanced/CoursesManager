using System.Windows;
using CoursesManager.MVVM.Dialogs;
using Moq;
#pragma warning disable CS8625 // Cannot convert null literal to non-nullable reference type.

namespace CoursesManager.Dialogs.Tests;

public class FakeWindow : Window
{
    public void SimulateClose() => OnClosed(EventArgs.Empty);
}

public class TestDialogViewModel : DialogViewModel<int>
{
    public TestDialogViewModel(int dialogResult) : base(dialogResult)
    {
        Data = dialogResult;
    }

    public int Data { get; set; }

    private readonly DialogResult<int> _default = DialogResult<int>.Builder().SetSuccess(10, "message").Build();

    public void CallInvokeResponseCallback(DialogResult<int>? dialogResult = null)
    {
        InvokeResponseCallback(dialogResult ?? _default);
    }

    protected override void InvokeResponseCallback(DialogResult<int> dialogResult)
    {
        ResponseCallback.Invoke(dialogResult);
    }
}

public class NotRegisteredTestDialogViewModel(int dialogResultType) : DialogViewModel<int>(dialogResultType)
{
    protected override void InvokeResponseCallback(DialogResult<int> dialogResult)
    {
        throw new NotImplementedException();
    }
}

[TestFixture, Apartment(ApartmentState.STA)]
public class DialogServiceTests
{
    private Mock<IDialogFactory> _dialogFactoryMock;
    private DialogService _dialogService;
    private TestDialogViewModel _viewModel;
    private FakeWindow _window;

    [SetUp]
    public void SetUp()
    {
        _dialogFactoryMock = new Mock<IDialogFactory>();
        _dialogFactoryMock
            .Setup(f => f.SetupWindow(It.IsAny<Type>()))
            .Returns(() =>
            {
                _window = new FakeWindow();
                return _window;
            });
        _dialogService = new DialogService(_dialogFactoryMock.Object);

        _viewModel = new(1);

        _dialogService.RegisterDialog<TestDialogViewModel, FakeWindow, int>((data) =>
        {
            _viewModel = new TestDialogViewModel(data);
            return _viewModel;
        });
    }

    [Test]
    public void RegisterDialog_ThrowsException_WhenGivenNull()
    {
        Assert.Throws<ArgumentNullException>(() =>
            _dialogService.RegisterDialog<TestDialogViewModel, FakeWindow, int>(null));
    }

    [Test]
    public void ShowDialogAsync_ThrowsException_WhenViewModelNotRegistered()
    {
        Assert.Throws<InvalidOperationException>(() => _dialogService.ShowDialogAsync<NotRegisteredTestDialogViewModel, int>());
    }

    [Test]
    public void ShowDialogAsync_WhenGivenInitialData_PassesItToViewModel()
    {
        _ = _dialogService.ShowDialogAsync<TestDialogViewModel, int>(10);

        Assert.That(_viewModel.Data, Is.EqualTo(10));
    }

    [Test]
    public async Task ShowDialogAsync_SendsResponse_AfterResponseCallback()
    {
        var task = _dialogService.ShowDialogAsync<TestDialogViewModel, int>();
        var expected = DialogResult<int>.Builder().SetSuccess(10, "message").Build();

        _viewModel.CallInvokeResponseCallback();

        var result = await task;

        Assert.That(result.Data, Is.EqualTo(expected.Data));
        Assert.That(result.OutcomeMessage, Is.EqualTo(expected.OutcomeMessage));
        Assert.That(result.Outcome, Is.EqualTo(expected.Outcome));
    }

    [Test]
    public async Task ShowDialogAsync_SendsResponse_AfterClosed()
    {
        var task = _dialogService.ShowDialogAsync<TestDialogViewModel, int>();
        var expected = DialogResult<int>.Builder().SetCanceled("Dialog was closed by the user").Build();

        _window.SimulateClose();

        var result = await task;

        Assert.That(result.Data, Is.EqualTo(expected.Data));
        Assert.That(result.OutcomeMessage, Is.EqualTo(expected.OutcomeMessage));
        Assert.That(result.Outcome, Is.EqualTo(expected.Outcome));
    }

    [Test]
    public async Task ShowDialogAsync_SendsCanceledResponse_AfterClosedEvenWithSuccessfulCallback()
    {
        var task = _dialogService.ShowDialogAsync<TestDialogViewModel, int>();
        var expected = DialogResult<int>.Builder().SetCanceled("Dialog was closed by the user").Build();

        _window.SimulateClose();
        _viewModel.CallInvokeResponseCallback();

        var result = await task;

        Assert.That(result.Data, Is.EqualTo(expected.Data));
        Assert.That(result.OutcomeMessage, Is.EqualTo(expected.OutcomeMessage));
        Assert.That(result.Outcome, Is.EqualTo(expected.Outcome));
    }

    [Test]
    public async Task ShowDialogAsync_SendsSuccessResponse_AfterResponseCallbackEvenWithClosed()
    {
        var task = _dialogService.ShowDialogAsync<TestDialogViewModel, int>();
        var expected = DialogResult<int>.Builder().SetSuccess(10, "message").Build();

        _viewModel.CallInvokeResponseCallback();
        _window.SimulateClose();

        var result = await task;

        Assert.That(result.Data, Is.EqualTo(expected.Data));
        Assert.That(result.OutcomeMessage, Is.EqualTo(expected.OutcomeMessage));
        Assert.That(result.Outcome, Is.EqualTo(expected.Outcome));
    }
}