using CoursesManager.MVVM.Data;
using System.Windows;

namespace CoursesManager.MVVM.Dialogs;

public sealed class DialogService(IDialogFactory dialogFactory) : IDialogService
{
    private readonly Dictionary<Type, (Type windowType, Func<object, ViewModel> viewModelFactory)> _dialogMapping = new();
    private readonly IDialogFactory _dialogFactory = dialogFactory;

    public void RegisterDialog<TDialogViewModel, TDialogWindow, TDialogResultType>(Func<TDialogResultType?, TDialogViewModel> viewModelFactory)
        where TDialogViewModel : DialogViewModel<TDialogResultType>
        where TDialogWindow : Window, new()
    {
        ArgumentNullException.ThrowIfNull(viewModelFactory);

        _dialogMapping[typeof(TDialogViewModel)] = (typeof(TDialogWindow), obj => viewModelFactory((TDialogResultType?)obj));
    }

    public Task<DialogResult<TDialogResultType>> ShowDialogAsync<TDialogViewModel, TDialogResultType>()
        where TDialogViewModel : DialogViewModel<TDialogResultType>
    {
        return ShowDialogInternalAsync<TDialogViewModel, TDialogResultType>();
    }

    public Task<DialogResult<TDialogResultType>> ShowDialogAsync<TDialogViewModel, TDialogResultType>(TDialogResultType initialData)
        where TDialogViewModel : DialogViewModel<TDialogResultType>
    {
        return ShowDialogInternalAsync<TDialogViewModel, TDialogResultType>(initialData);
    }

    private Task<DialogResult<TDialogResultType>> ShowDialogInternalAsync<TDialogViewModel, TDialogResultType>(TDialogResultType? initialData = default)
        where TDialogViewModel : DialogViewModel<TDialogResultType>
    {
        if (!_dialogMapping.TryGetValue(typeof(TDialogViewModel), out var mappingTuple))
        {
            throw new InvalidOperationException($"No mapping registered for {typeof(TDialogViewModel).Name}");
        }

        var (windowType, viewModelFactory) = mappingTuple;

        var viewModel = (DialogViewModel<TDialogResultType>)viewModelFactory(initialData!);

        var window = _dialogFactory.SetupWindow(windowType);

        window.DataContext = viewModel;

        var tcs = new TaskCompletionSource<DialogResult<TDialogResultType>>();

        viewModel.RegisterResponseCallback(response =>
        {
            if (!tcs.Task.IsCompleted)
            {
                tcs.SetResult(response);
            }
            window.Close();
        });

        window.Closed += (sender, args) =>
        {
            if (tcs.Task.IsCompleted) return;

            var failureResponse = DialogResult<TDialogResultType>.Builder()
                .SetCanceled("Dialog was closed by the user")
                .Build();

            tcs.SetResult(failureResponse);
        };

        _dialogFactory.OpenDialog(window);

        return tcs.Task;
    }
}