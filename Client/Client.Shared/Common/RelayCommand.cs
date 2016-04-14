using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using Windows.UI.Xaml;
using System.Reflection;

namespace Client.Common
{
    /// <summary>
    /// Ein Befehl mit dem einzigen Zweck, seine Funktionalität zu vermitteln 
    /// zu anderen Objekten durch Aufrufen von Delegaten. 
    /// Der Standardrückgabewert für die CanExecute-Methode ist 'true'.
    /// <see cref="RaiseCanExecuteChanged"/> muss jedes mal aufgerufen werden, wenn
    /// <see cref="CanExecute"/> muss einen anderen Wert zurückgeben.
    /// </summary>
    public class RelayCommand : ICommand
    {
        private readonly Action _execute;
        private readonly Func<bool> _canExecute;

        /// <summary>
        /// Wird ausgelöst, wenn RaiseCanExecuteChanged aufgerufen wird.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Erstellt einen neuen Befehl, der immer ausgeführt werden kann.
        /// </summary>
        /// <param name="execute">Die Ausführungslogik.</param>
        public RelayCommand(Action execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Erstellt einen neuen Befehl.
        /// </summary>
        /// <param name="execute">Die Ausführungslogik.</param>
        /// <param name="canExecute">Die Logik des Ausführungsstatus.</param>
        public RelayCommand(Action execute, Func<bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Legt fest, ob dieser <see cref="RelayCommand"/> im aktuellen Zustand ausgeführt werden kann.
        /// </summary>
        /// <param name="parameter">
        /// Die vom Befehl verwendeten Daten. Wenn für den Befehl keine Datenübergabe erforderlich ist, kann dieses Objekt auf NULL festgelegt werden.
        /// </param>
        /// <returns>True, wenn dieser Befehl ausgeführt werden kann, andernfalls False.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute();
        }

        /// <summary>
        /// Führt den <see cref="RelayCommand"/> im aktuellen Befehlsziel aus.
        /// </summary>
        /// <param name="parameter">
        /// Die vom Befehl verwendeten Daten. Wenn für den Befehl keine Datenübergabe erforderlich ist, kann dieses Objekt auf NULL festgelegt werden.
        /// </param>
        public void Execute(object parameter)
        {
            _execute();
        }

        /// <summary>
        /// Zum Aufrufen des <see cref="CanExecuteChanged"/>-Ereignisses verwendete Methode
        /// um anzugeben, dass der Rückgabewert von <see cref="CanExecute"/>
        /// Die Methode hat sich geändert.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

    public class RelayCommand<T> :DependencyObject,  ICommand
    {
        private readonly Action<T> _execute;
        private readonly Func<T,bool> _canExecute;



        public Object Parameter
        {
            get { return (Object)GetValue(ParameterProperty); }
            set { SetValue(ParameterProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Parameter.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ParameterProperty =
            DependencyProperty.Register("Parameter", typeof(Object), typeof(RelayCommand<>), new PropertyMetadata(null, ParameterChanged));

        private static void ParameterChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var t =d.GetType();
            t.GetRuntimeMethod("RaiseCanExecuteChanged", new Type[0]).Invoke(d, new object[0]);
        }




        /// <summary>
        /// Wird ausgelöst, wenn RaiseCanExecuteChanged aufgerufen wird.
        /// </summary>
        public event EventHandler CanExecuteChanged;

        /// <summary>
        /// Erstellt einen neuen Befehl, der immer ausgeführt werden kann.
        /// </summary>
        /// <param name="execute">Die Ausführungslogik.</param>
        public RelayCommand(Action<T> execute)
            : this(execute, null)
        {
        }

        /// <summary>
        /// Erstellt einen neuen Befehl.
        /// </summary>
        /// <param name="execute">Die Ausführungslogik.</param>
        /// <param name="canExecute">Die Logik des Ausführungsstatus.</param>
        public RelayCommand(Action<T> execute, Func<T,bool> canExecute)
        {
            if (execute == null)
                throw new ArgumentNullException("execute");
            _execute = execute;
            _canExecute = canExecute;
        }

        /// <summary>
        /// Legt fest, ob dieser <see cref="RelayCommand"/> im aktuellen Zustand ausgeführt werden kann.
        /// </summary>
        /// <param name="parameter">
        /// Die vom Befehl verwendeten Daten. Wenn für den Befehl keine Datenübergabe erforderlich ist, kann dieses Objekt auf NULL festgelegt werden.
        /// </param>
        /// <returns>True, wenn dieser Befehl ausgeführt werden kann, andernfalls False.</returns>
        public bool CanExecute(object parameter)
        {
            return _canExecute == null ? true : _canExecute((T)parameter);
        }

        /// <summary>
        /// Führt den <see cref="RelayCommand"/> im aktuellen Befehlsziel aus.
        /// </summary>
        /// <param name="parameter">
        /// Die vom Befehl verwendeten Daten. Wenn für den Befehl keine Datenübergabe erforderlich ist, kann dieses Objekt auf NULL festgelegt werden.
        /// </param>
        public void Execute(object parameter)
        {
            _execute((T)parameter);
        }

        /// <summary>
        /// Zum Aufrufen des <see cref="CanExecuteChanged"/>-Ereignisses verwendete Methode
        /// um anzugeben, dass der Rückgabewert von <see cref="CanExecute"/>
        /// Die Methode hat sich geändert.
        /// </summary>
        public void RaiseCanExecuteChanged()
        {
            var handler = CanExecuteChanged;
            if (handler != null)
            {
                handler(this, EventArgs.Empty);
            }
        }
    }

}