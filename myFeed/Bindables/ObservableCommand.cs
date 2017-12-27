using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace myFeed.Bindables
{
    public sealed class ObservableCommand : ICommand
    {
        private readonly Func<Task> _task;
        private bool _canExecute = true;

        public ObservableCommand(Func<Task> task) => _task = task;

        public ObservableCommand(Action action) => _task = () =>
        {
            action.Invoke();
            return Task.CompletedTask;
        };

        public bool CanExecute(object parameter) => _canExecute;

        public event EventHandler CanExecuteChanged;

        public async void Execute(object parameter) 
        {
            try
            {
                UpdateCanExecute(false);
                await _task.Invoke();
                UpdateCanExecute(true);
            }
            catch
            {
                // ignored
            }
            void UpdateCanExecute(bool value)
            {
                _canExecute = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
    }
}