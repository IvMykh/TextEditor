using System;

namespace TextEditor.UI.ViewModel.Infrastructure
{
    public class DelegateCommand
        : MyCommand
    {
        private readonly Action _action;

        public DelegateCommand(Action action, Func<bool> canExecutePredicate = null)
            : base(canExecutePredicate)
        {
            _action = action;
        }

        public override void Execute(object parameter)
        {
            _action();
        }
    }
}
