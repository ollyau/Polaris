﻿using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Polaris
{
	abstract class ObservableObject : INotifyPropertyChanged
	{
		public event PropertyChangedEventHandler PropertyChanged;

		protected virtual void OnPropertyChanged( [CallerMemberName] string propertyName = null )
		{
			VerifyPropertyName( propertyName );
			PropertyChanged?.Invoke( this, new PropertyChangedEventArgs( propertyName ) );
		}

		protected virtual bool SetField<T>( ref T field, T value, [CallerMemberName] string propertyName = null )
		{
			if ( EqualityComparer<T>.Default.Equals( field, value ) )
			{
				return false;
			}
			field = value;
			OnPropertyChanged( propertyName );
			return true;
		}


		[Conditional( "DEBUG" )]
		[DebuggerStepThrough]
		public virtual void VerifyPropertyName( string propertyName )
		{
			// Verify that the property name matches a real,
			// public, instance property on this object.
			if ( TypeDescriptor.GetProperties( this )[ propertyName ] == null )
			{
				var message = $"Invalid property name: {propertyName}";
				if ( ThrowOnInvalidPropertyName )
				{
					throw new Exception( message );
				}
				else
				{
					Debug.Fail( message );
				}
			}
		}

		protected virtual bool ThrowOnInvalidPropertyName { get; private set; }
	}
}
