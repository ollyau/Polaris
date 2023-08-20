using BeatlesBlog.SimConnect;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

namespace Polaris
{
	class FlightSimulator : SimConnectClient, ISimulator
	{
		//-----------------------------------------------------------------------------

		enum Events
		{
			KEY_CLOCK_HOURS_SET,
			KEY_CLOCK_MINUTES_SET,
			KeyboardClientEvent
		}

		enum InputGroup
		{
			KeyboardShortcut
		}

		enum NotificationGroup
		{
			One
		}

		//-----------------------------------------------------------------------------

		public event EventHandler OnInputEvent;
		protected virtual void RaiseInputEvent( object s, EventArgs e )
		{
			EventHandler handler = OnInputEvent;
			handler?.Invoke( s, e );
		}

		private string _newLocationShortcut;
		public string NewLocationShortcut
		{
			get { return _newLocationShortcut; }
			private set { SetField( ref _newLocationShortcut, value ); }
		}

		//-----------------------------------------------------------------------------

		public FlightSimulator() : base( "Polaris" )
		{
			Client.OnRecvException += OnRecvException;
			Client.OnRecvEvent += OnRecvEvent;
		}

		protected override void PreInitialize()
		{
			base.PreInitialize();

			// get shortcut from settings file
			NewLocationShortcut = Settings.Get( "new_location_shortcut", "VK_LSHIFT+D" );
			Log.Debug( $"new location shortcut set to: {NewLocationShortcut}" );
		}

		protected override void PreUninitialize()
		{
			base.PreUninitialize();

			// https://devsupport.flightsimulator.com/questions/6515/simconnect-clearinputgroup-not-working.html
			Client.SetInputGroupState( InputGroup.KeyboardShortcut, ( uint )SIMCONNECT_STATE.OFF );
			Client.ClearInputGroup( InputGroup.KeyboardShortcut );
		}

		protected override void OnRecvOpen( SimConnect sender, SIMCONNECT_RECV_OPEN data )
		{
			// call parent class for default behavior
			base.OnRecvOpen( sender, data );

			// this requires all Events enum entries starting with KEY_ to match their appropriate SimConnect event ID name
			var eventNames = Enum.GetNames( typeof( Events ) );
			var eventValues = ( Events[] )Enum.GetValues( typeof( Events ) );
			for ( var i = 0; i < eventNames.Length; i++ )
			{
				if ( !eventNames[ i ].StartsWith( "KEY_" ) )
				{
					continue;
				}
				var name = eventNames[ i ].Substring( 4 );
				Log.Debug( $"Mapping event: {name}" );
				Client.MapClientEventToSimEvent( eventValues[ i ], name );
			}

			// shortcut
			// https://learn.microsoft.com/en-us/previous-versions/cc707129(v=msdn.10)?redirectedfrom=MSDN
			// https://devsupport.flightsimulator.com/questions/10754/mapinputeventtoclientevent-and-ctrl-shift-alt.html
			Client.MapClientEventToSimEvent( Events.KeyboardClientEvent );
			Client.MapInputEventToClientEvent( InputGroup.KeyboardShortcut, NewLocationShortcut, Events.KeyboardClientEvent );

			// https://devsupport.flightsimulator.com/questions/383/simconnect-setinputgrouppriority-not-working.html
			Client.SetInputGroupPriority( InputGroup.KeyboardShortcut, ( uint )SIMCONNECT_GROUP_PRIORITY.HIGHEST );
			Client.SetInputGroupState( InputGroup.KeyboardShortcut, ( uint )SIMCONNECT_STATE.ON );

			// notifications
			Client.AddClientEventToNotificationGroup( NotificationGroup.One, Events.KeyboardClientEvent );
			Client.SetNotificationGroupPriority( NotificationGroup.One, ( uint )SIMCONNECT_GROUP_PRIORITY.HIGHEST );
		}

		private void OnRecvException( SimConnect sender, SIMCONNECT_RECV_EXCEPTION data )
		{
			var exceptionName = Enum.GetName( typeof( SIMCONNECT_EXCEPTION ), data.dwException );
			var message = $"{exceptionName} (Exception = {data.dwException}, SendID = {data.dwSendID}, Index = {data.dwIndex})";
			Log.Warning( message );
		}

		private void OnRecvEvent( SimConnect sender, SIMCONNECT_RECV_EVENT data )
		{
			switch ( ( Events )data.uEventID )
			{
				case Events.KeyboardClientEvent:
					Log.Debug( "keyboard event received" );
					RaiseInputEvent( this, EventArgs.Empty );
					break;
				default:
					Log.Debug( $"unknown event {data.uEventID}" );
					break;
			}
		}

		//-----------------------------------------------------------------------------

		public void SetUserLocation( double latitude, double longitude )
		{
			if ( Connected )
			{
				var thing = new SIMCONNECT_DATA_INITPOSITION( latitude, longitude, 0, 0, 0, 0, true, 0 );
				Client.SetDataOnUserSimObject( thing );
				Client.TransmitClientEventToUser( Events.KEY_CLOCK_HOURS_SET, 12, SIMCONNECT_GROUP_PRIORITY.HIGHEST );
				Client.TransmitClientEventToUser( Events.KEY_CLOCK_MINUTES_SET, 0, SIMCONNECT_GROUP_PRIORITY.HIGHEST );
			}
		}

		//-----------------------------------------------------------------------------
	}
}
