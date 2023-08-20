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
		private readonly Action _action;
		private readonly Func<bool> _canExecute;

		public CommandHandler( Action action, Func<bool> canExecute )
		{
			_action = action;
			_canExecute = canExecute;
		}

		public event EventHandler? CanExecuteChanged
		{
			add { CommandManager.RequerySuggested += value; }
			remove { CommandManager.RequerySuggested -= value; }
		}

		public bool CanExecute( object? sender )
		{
			return _canExecute.Invoke();
		}

		public void Execute( object? sender )
		{
			_action();
		}
	}
}
