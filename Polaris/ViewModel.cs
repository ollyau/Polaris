using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls.Primitives;
using System.Windows;
using System.Windows.Data;
using System.Windows.Input;
using Microsoft.Win32;
using System.Net.Http;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Diagnostics;
using System.Globalization;
using BeatlesBlog.SimConnect;

namespace Polaris
{
	internal class ViewModel : ObservableObject
	{
		//-----------------------------------------------------------------------------

		ISimulator Sim;
		LocationManager Locations;

		private string _simulatorStatus;
		public string SimulatorStatus
		{
			get { return _simulatorStatus; }
			private set { SetField( ref _simulatorStatus, value ); }
		}

		private Location _currentLocation;
		public Location CurrentLocation
		{
			get { return _currentLocation; }
			private set
			{
				SetField( ref _currentLocation, value );
				OnPropertyChanged( "LocationDetailsEnabled" );
			}
		}

		public bool LocationDetailsEnabled
		{
			get { return CurrentLocation != null; }
		}

		private bool _locationDetailsOpen;
		public bool LocationDetailsOpen
		{
			get { return _locationDetailsOpen; }
			set { SetField( ref _locationDetailsOpen, value ); }
		}

		private readonly CommandHandler _setNewLocation;
		public CommandHandler SetNewLocation { get { return _setNewLocation; } }

		private readonly CommandHandler _openBrowserToMap;
		public CommandHandler OpenBrowserToMap { get { return _openBrowserToMap; } }

		//-----------------------------------------------------------------------------

		public ViewModel()
		{
			SimulatorStatus = "Not connected to flight simulator.  Please restart the application to attempt connecting again.";

			_setNewLocation = new CommandHandler( () => SetNewLocation_Execute(), () => SetNewLocation_CanExecute() );
			_openBrowserToMap = new CommandHandler( () => OpenBrowserToMap_Execute(), () => OpenBrowserToMap_CanExecute() );

			Locations = new LocationManager();
			Locations.RegisterLocationProvider( new WeatherStationLocationProvider() );

			//Sim = new MockSimulator();
			//if ( Sim is MockSimulator m )
			//{
			//	m.PropertyChanged += Client_PropertyChanged;
			//	m.Initialize();
			//}

			Sim = new FlightSimulator();
			if ( Sim is FlightSimulator fs )
			{
				fs.OnInputEvent += Sim_OnInputEvent;
				fs.PropertyChanged += Client_PropertyChanged;
				fs.Initialize();
			}
		}

		//-----------------------------------------------------------------------------

		private void Sim_OnInputEvent( object? sender, EventArgs e )
		{
			if ( SetNewLocation_CanExecute() )
			{
				SetNewLocation_Execute();
			}
		}

		//-----------------------------------------------------------------------------

		private void SetNewLocation_Execute()
		{
			LocationDetailsOpen = false;
			CurrentLocation = Locations.GetNewLocation();
			Sim.SetUserLocation( CurrentLocation.Latitude, CurrentLocation.Longitude );
		}

		private bool SetNewLocation_CanExecute()
		{
			return Sim != null && Sim.Connected && Locations.HasLocations;
		}

		//-----------------------------------------------------------------------------

		private void OpenBrowserToMap_Execute()
		{
			var lat = CurrentLocation.Latitude;
			var lon = CurrentLocation.Longitude;
			FormattableString destination = $"https://www.google.com/maps?ll={lat:F4},{lon:F4}&q={lat:F4},{lon:F4}&t=h&z=9";
			Process.Start( new ProcessStartInfo( destination.ToString( CultureInfo.InvariantCulture ) ) { UseShellExecute = true } );
		}

		private bool OpenBrowserToMap_CanExecute()
		{
			return CurrentLocation != null;
		}

		//-----------------------------------------------------------------------------

		private void Client_PropertyChanged( object? sender, PropertyChangedEventArgs e )
		{
			var client = sender as ISimulator;
			switch ( e.PropertyName )
			{
				case nameof( client.Connected ):
				case nameof( client.SimulatorName ):
					// change state when simconnect connection status changes
					if ( client != null && client.Connected )
					{
						SimulatorStatus = $"Connected to {client.SimulatorName}.";

						Task.Run( async () =>
						{
							await Locations.Initialize();
							SetNewLocation.RaiseCanExecuteChanged();
						} );
					}
					else
					{
						SimulatorStatus = "Disconnected from flight simulator.  Please restart the application to connect again.";
					}
					break;
			}
		}

		//-----------------------------------------------------------------------------

		internal void Closing( object? sender, CancelEventArgs e )
		{
			if ( Sim is FlightSimulator fs )
			{
				fs.Uninitialize();
			}
		}

		//-----------------------------------------------------------------------------
	}
}
