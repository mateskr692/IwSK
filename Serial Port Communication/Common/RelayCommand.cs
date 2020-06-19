using System;
using System.Windows.Input;

namespace Common
{
    public class RelayCommand : ICommand
    {
        protected readonly Action<object> _execute;
        protected readonly Predicate<object> _canExecute;

        public RelayCommand( Action<object> execute )
        {
            this._execute = execute;
        }
        public RelayCommand( Action<object> execute, Predicate<object> canExecute )
        {
            this._execute = execute;
            this._canExecute = canExecute;
        }


        public bool CanExecute( object parameter )
        {
            return this._canExecute == null ? true : this._canExecute( parameter );
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
        public void Execute( object parameter ) => this._execute( parameter );

    }
}
