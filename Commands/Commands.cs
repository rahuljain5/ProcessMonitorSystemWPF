using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace PMSAppWPF.Commands
{
    public class PMSCommands : ICommand
    {
        private Action<object> execute;
        private Func<object, bool> canExecute;

        public PMSCommands(Action<object> execute, Func<object, bool> canExecute)
        {
            this.execute = execute;
            this.canExecute = canExecute;
        }

        public event EventHandler CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }

        public bool CanExecute(object parameter)
        {
            return canExecute(parameter);
        }

        public void Execute(object parameter)
        {
            execute(parameter);
        }
    }
}
