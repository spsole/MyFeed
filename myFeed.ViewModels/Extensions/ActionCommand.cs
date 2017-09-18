using System;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Windows.Input;

namespace myFeed.ViewModels.Extensions
{
    /// <summary>
    /// Command that dispatches task-actions.
    /// </summary>
    public class ActionCommand : ICommand
    {
        private bool _canExecute = true;
        private readonly Func<Task> _task;
        
        private ActionCommand(Func<Task> task) => _task = task;

        /// <summary>
        /// True if queue is unlocked and command can be executed.
        /// </summary>
        public bool CanExecute(object parameter) => _canExecute;

        /// <summary>
        /// Invoked when command state changes.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Executes a command.
        /// </summary>
        public async void Execute(object parameter)
        {
            UpdateCanExecute(false);
            try
            {
                await _task.Invoke();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.InnerException);
            }
            UpdateCanExecute(true); 
            
            void UpdateCanExecute(bool value)
            {
                _canExecute = value;
                CanExecuteChanged?.Invoke(this, EventArgs.Empty);
            }
        }
        
        /// <summary>
        /// Creates ActionCommand from Task.
        /// </summary>
        public static ActionCommand Of(Func<Task> task) => new ActionCommand(task);
        
        /// <summary>
        /// Creates ActionCommand from Action.
        /// </summary>
        public static ActionCommand Of(Action action) => new ActionCommand(() =>
        {
            action.Invoke();
            return Task.CompletedTask;
        });
    }
}