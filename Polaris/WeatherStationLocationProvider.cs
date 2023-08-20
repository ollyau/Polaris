using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Polaris
{
	class WeatherStationLocationProvider : ILocationProvider
	{
		private static readonly HttpClient WebClient = new HttpClient();
		private static readonly string Endpoint = "https://www.aviationweather.gov/docs/metar/stations.txt";

		public async Task<IEnumerable<Location>> GetLocations()
		{
			var result = new List<Location>();

			var data = await WebClient.GetStreamAsync( Endpoint );
			using ( var reader = new StreamReader( data ) )
			{
				var regexLine = new Regex( @"^([A-Z ]{2}) (.{16}) ([A-Z0-9 ]{4})  ([A-Z0-9 ]{3})   ([0-9 ]{5})  ([NS0-9 ]{6})  ([EW0-9 ]{7}) ([0-9\- ]{4})   ([A-Z ])  ([A-Z ])  ([A-Z ])  ([A-Z ])  ([A-Z ])  ([FRC ]) ([0-9]) ([A-Z]{2})$", RegexOptions.Compiled );
				var regexCoord = new Regex( @"(\d+)\s+(\d+)\s*([NSEW])", RegexOptions.Compiled );

				while ( !reader.EndOfStream )
				{
					var text = await reader.ReadLineAsync();
					if ( string.IsNullOrEmpty( text ) )
					{
						continue;
					}
					var match = regexLine.Match( text );
					if ( match.Success )
					{
						try
						{
							var stateOrProvince = match.Groups[ 1 ].Value.Trim();
							var name = match.Groups[ 2 ].Value.Trim();
							var icao = match.Groups[ 3 ].Value.Trim();

							var lat = regexCoord.Match( match.Groups[ 6 ].Value.Trim() );
							var lon = regexCoord.Match( match.Groups[ 7 ].Value.Trim() );

							var country = match.Groups[ 16 ].Value.Trim();
							try
							{
								country = new RegionInfo( country ).EnglishName;
							}
							catch { }

							var latitude = ( int.Parse( lat.Groups[ 1 ].Value ) + ( int.Parse( lat.Groups[ 2 ].Value ) / 60.0 ) ) * ( lat.Groups[ 3 ].Value == "N" ? 1 : -1 );
							var longitude = ( int.Parse( lon.Groups[ 1 ].Value ) + ( int.Parse( lon.Groups[ 2 ].Value ) / 60.0 ) ) * ( lon.Groups[ 3 ].Value == "E" ? 1 : -1 );

							var prepend = string.IsNullOrEmpty( icao ) ? "" : $"{icao} - ";

							// I hate it too
							var append = string.IsNullOrEmpty(country) ? "" : (string.IsNullOrEmpty( stateOrProvince ) ? $" ({country})" : $" ({stateOrProvince}, {country})");

							var locationName = $"{prepend}{name}{append}";
							if ( !string.IsNullOrWhiteSpace( locationName ) )
							{
								result.Add( new Location( locationName, latitude, longitude ) );
							}
						}
						catch { }
					}
				}
			}

			return result;
		}

	}
}
