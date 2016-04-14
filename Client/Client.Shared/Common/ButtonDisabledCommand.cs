using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Input;

namespace Client.Common
{
    public class ButtonDisabledCommand : ICommand
    {
        public event EventHandler CanExecuteChanged;

        public bool CanExecute(object parameter)
        {
            return false;
        }

        public void Execute(object parameter)
        {
            throw new NotSupportedException("Das nutzen dieser Operation sollte niemals geschen, da es nur ein Stub ist.");
        }
    }
}
