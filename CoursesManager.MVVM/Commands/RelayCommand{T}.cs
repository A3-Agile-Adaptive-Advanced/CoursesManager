using System.Diagnostics.CodeAnalysis;
using System.Windows.Input;

namespace CoursesManager.MVVM.Commands
{
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;

        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            ArgumentNullException.ThrowIfNull(execute);

            _execute = execute;
            _canExecute = canExecute;
        }

        public void Execute(object? parameter)
        {
            if (parameter is T validParameter)
            {
                Execute(validParameter);
            }
            else
            {
                throw new ArgumentException($"Invalid parameter type. Expected {typeof(T)}, but got {parameter?.GetType()}.", nameof(parameter));
            }
        }

        public void Execute(T parameter)
        {
            _execute.Invoke(parameter);
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
            if (parameter is null) return false;

            if (parameter is T validParameter) return CanExecute(validParameter);

            throw new ArgumentException($"Invalid parameter type. Expected {typeof(T)}, but got {parameter.GetType()}.", nameof(parameter));
        }

        public bool CanExecute(T parameter)
        {
            return _canExecute == null || _canExecute(parameter);
        }

        [ExcludeFromCodeCoverage] // Can't test CommandManager
        public void RaiseCanExecuteChanged()
        {
            CommandManager.InvalidateRequerySuggested();
        }
    }
}