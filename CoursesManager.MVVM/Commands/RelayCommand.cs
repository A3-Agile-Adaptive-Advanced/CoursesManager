using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace CoursesManager.MVVM.Commands
{
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;

        private readonly Func<bool>? _canExecute;

        public RelayCommand(Action execute, Func<bool>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        [ExcludeFromCodeCoverage] // Can't test CommandManager
        public event EventHandler? CanExecuteChanged
        {
            [ExcludeFromCodeCoverage] // Can't test CommandManager
            add => CommandManager.RequerySuggested += value;
            [ExcludeFromCodeCoverage] // Can't test CommandManager
            remove => CommandManager.RequerySuggested -= value;
        }

        public bool CanExecute(object? parameter)
        {
            return _canExecute == null || _canExecute();
        }

        public void Execute(object? parameter)
        {
            _execute.Invoke();
        }

        [ExcludeFromCodeCoverage] // Can't test CommandManager
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}