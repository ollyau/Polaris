using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace Polaris
{
	public class CommandHandler : ICommand
	{
		private event EventHandler _internalCanExecuteChanged;
		private readonly Action _action;
		private readonly Func<bool> _canExecute;

		public CommandHandler( Action action, Func<bool> canExecute )
		{
			_action = action;
			_canExecute = canExecute;
		}

		public event EventHandler? CanExecuteChanged
		{
			add
			{
				_internalCanExecuteChanged += value;
				CommandManager.RequerySuggested += value;
			}
			remove
			{
				_internalCanExecuteChanged -= value;
				CommandManager.RequerySuggested -= value;
			}
		}

		public bool CanExecute( object? sender )
		{
			return _canExecute.Invoke();
		}

		public void Execute( object? sender )
		{
			_action();
		}

		public void RaiseCanExecuteChanged()
		{
			EventHandler handler = _internalCanExecuteChanged;
			handler?.Invoke( this, EventArgs.Empty );
		}
	}
}
