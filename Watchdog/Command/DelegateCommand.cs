using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Windows.Input;

namespace Watchdog.Command
{
    class DelegateCommand : ICommand
    {
        #region Fields

    readonly Action _execute;
    readonly Predicate<object> _canExecute;        

    #endregion // Fields

    #region Constructors

    public DelegateCommand(Action execute)
    : this(execute, null)
    {
    }

    public DelegateCommand(Action execute, Predicate<object> canExecute)
    {
        if (execute == null)
            throw new ArgumentNullException("execute");

        _execute = execute;
        _canExecute = canExecute;           
    }
    #endregion // Constructors

    #region ICommand Members

        public void Execute(object parameter)
        {
            _execute();
        }

        [DebuggerStepThrough]
    public bool CanExecute(object parameter)
    {
        return _canExecute == null ? true : _canExecute(parameter);
    }

    public event EventHandler CanExecuteChanged
    {
        add { CommandManager.RequerySuggested += value; }
        remove { CommandManager.RequerySuggested -= value; }
    }

    #endregion // ICommand Members
    }
}
