using System;

namespace TextEditor.UI.ViewModel.Infrastructure
{
    public class ParameterizedDelegateCommand
        : MyCommand
    {
        private readonly Action<object> _action;

        public ParameterizedDelegateCommand(
            Action<object> action, Func<bool> canExecutePredicate = null)
            : base(canExecutePredicate)
        {
            _action = action;
        }

        public override void Execute(object parameter)
        {
            _action(parameter);
        }
    }
}
