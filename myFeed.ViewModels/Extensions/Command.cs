using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace myFeed.ViewModels.Extensions
{
    public class Command : ICommand
    {
        private readonly Func<Task> _task;
        private bool _canExecute = true;

        public Command(Func<Task> task) => _task = task;

        public Command(Action action) => _task = () =>
        {
            action.Invoke();
            return Task.CompletedTask;
        };

        public bool CanExecute(object parameter) => _canExecute;

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter)
        {
            UpdateCanExecute(false);
            await _task.Invoke();
            UpdateCanExecute(true);

            void UpdateCanExecute(bool value)
            {
                _canExecute = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}