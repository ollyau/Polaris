using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace Polaris
{
	class Location
	{
		public string Name { get; private set; }
		public double Latitude { get; private set; }
		public double Longitude { get; private set; }

		public Location( string name, double latitude, double longitude )
		{
			Name = name;
			Latitude = latitude;
			Longitude = longitude;
		}
	}

	class LocationManager
	{
		private static readonly Random rng = new Random();
		private static readonly List<ILocationProvider> LocationProviders = new List<ILocationProvider>();
		private List<Location>? Locations;

		public bool HasLocations
		{
			get { return Locations != null && Locations.Count > 0; }
		}

		public LocationManager()
		{
		}

		public Location GetNewLocation()
		{
			var idx = rng.Next( Locations.Count );
			return Locations[ idx ];
		}

		public void RegisterLocationProvider( ILocationProvider provider )
		{
			LocationProviders.Add( provider );
		}

		public async Task Initialize()
		{
			Locations = new List<Location>();
			foreach ( var provider in LocationProviders )
			{
				var locations = await provider.GetLocations();
				Locations.AddRange( locations );
			}
		}
	}
}
