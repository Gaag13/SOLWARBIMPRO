using System;
using System.Windows.Input;

namespace WARBIMPRO.Utils
{
    public class RelayCommand : ICommand
    {
        private readonly Action<object> _execute;
        private readonly Func<object, bool> _canExecute;

        // ── Constructor CON parámetro (Action<object>) ────────────────────
        public RelayCommand(Action<object> execute,
                            Func<object, bool> canExecute = null)
        {
            _execute = execute ?? throw new ArgumentNullException(nameof(execute));
            _canExecute = canExecute;
        }

        // ── Constructor SIN parámetro (Action) ────────────────────────────
        // Esto resuelve el error: no se puede convertir Action en Action<object>
        public RelayCommand(Action execute,
                            Func<bool> canExecute = null)
        {
            _execute = _ => execute();
            _canExecute = canExecute == null ? null : _ => canExecute();
        }

        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
            => _canExecute == null || _canExecute(parameter);

        public void Execute(object parameter)
            => _execute(parameter);

        public void RaiseCanExecuteChanged()
            => CanExecuteChanged?.Invoke(this, EventArgs.Empty);
    }
}