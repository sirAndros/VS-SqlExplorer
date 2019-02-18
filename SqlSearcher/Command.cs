using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace SqlSearcher
{
    public class Command : ICommand
    {
        private readonly Action _action;

        private bool _canExecute;
        public bool CanExecute
        {
            get { return _canExecute; }
            set
            {
                if (_canExecute != value)
                {
                    _canExecute = value;
                    CanExecuteChanged?.Invoke(this, EventArgs.Empty);
                }
            }
        }
        public event EventHandler CanExecuteChanged;
        bool ICommand.CanExecute(object parameter)
        {
            return CanExecute;
        }

        public void Execute(object parameter)
        {
            _action?.Invoke();
        }


        public Command(Action action, bool canExecute = true)
        {
            _action = action;
            _canExecute = canExecute;
        }
    }
}
