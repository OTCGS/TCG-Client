using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Client.Store.Common
{
    public class ProlongedCommand : ICommand
    {
        public ProlongedCommand(Func<Task> execute)
            : this((obj) => execute(), (obj) => true)
        { }

        public ProlongedCommand(Func<object, Task> execute, Predicate<object> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public Task Execute(object parameter)
        {
            return execute(parameter);
        }

        public bool CanExecute(object parameter)
        {
            return canExecute(parameter);
        }

        public void RaiseCanExecuteChanged()
        {
            canExecuteChanged(this, EventArgs.Empty);
        }

        private event EventHandler canExecuteChanged;

        private Func<object, Task> execute;
        private Predicate<object> canExecute;

        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute(parameter);
        }

        event EventHandler ICommand.CanExecuteChanged
        {
            add { canExecuteChanged += value; }
            remove { canExecuteChanged -= value; }
        }

        async void ICommand.Execute(object parameter)
        {
            await Execute(parameter);
        }
    }
}