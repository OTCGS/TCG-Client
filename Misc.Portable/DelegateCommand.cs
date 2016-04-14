using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System.Windows.Input
{
    public class DelegateCommand : ICommand
    {
        private Action<object> execute;

        public event EventHandler CanExecuteChanged;

        public void FireCanExecuteChanged()
        {
            if (CanExecuteChanged != null)
                this.CanExecuteChanged(this, EventArgs.Empty);
        }

        public DelegateCommand(Action<object> execute)
        {
            this.execute = execute;
        }

        public DelegateCommand(Action execute) : this(parameter => execute())
        {
        }

        public bool CanExecute(object parameter)
        {
            return true;
        }

        public void Execute(object parameter)
        {
            this.execute(parameter);
        }
    }
}