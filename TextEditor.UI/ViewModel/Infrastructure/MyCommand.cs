using System;
using System.Windows.Input;

namespace TextEditor.UI.ViewModel.Infrastructure
{
    public abstract class MyCommand
     : ICommand
    {
        private readonly Func<bool> _canExecutePredicate;

        public MyCommand(Func<bool> canExecutePredicate)
        {
            _canExecutePredicate = canExecutePredicate;
        }

        public event EventHandler CanExecuteChanged;

        protected virtual void OnCanExecuteChanged(EventArgs e)
        {
            var handler = CanExecuteChanged;

            if (handler != null)
            {
                handler(this, e);
            }
        }

        public void RaiseCanExecuteChanged()
        {
            OnCanExecuteChanged(EventArgs.Empty);
        }

        public bool CanExecute(object parameter)
        {
            return _canExecutePredicate == null ? true : _canExecutePredicate();
        }


        public abstract void Execute(object parameter);
    }
}
