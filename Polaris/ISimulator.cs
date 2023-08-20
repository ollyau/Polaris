using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Polaris
{
	interface ISimulator
	{
		bool Connected { get; }
		string SimulatorName { get; }
		void SetUserLocation( double latitude, double longitude );
	}

	class MockSimulator : ObservableObject, ISimulator
	{
		public bool Connected { get { return true; } }
		public string SimulatorName { get { return "Mock Simulator Implementation"; } }
		public void SetUserLocation( double latitude, double longitude )
		{
			Log.Debug( $"requested set latitude = {latitude}, longitude = {longitude}" );
		}
		public void Initialize()
		{
			OnPropertyChanged( "Connected" );
		}
	}
}
