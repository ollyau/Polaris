using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO.Compression;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;

namespace Polaris
{
	struct WeatherStation
	{
		public string country;
		public int elev;
		public string faaId;
		public string iataId;
		public string icaoId;
		public double lat;
		public double lon;
		public int priority;
		public string site;
		public string state;
		public string wmoId;
	}

	class WeatherStationLocationProvider : ILocationProvider
	{
		private static readonly HttpClient WebClient = new HttpClient();
		private static readonly string Endpoint = "https://aviationweather.gov/data/cache/stations.cache.json.gz";
		private static readonly JsonSerializerOptions SerializerOptionsWithFields = new() { IncludeFields = true };

		private bool IsInvalidString( string str )
		{
			return string.IsNullOrWhiteSpace( str.Trim( '-' ) );
		}

		public async Task<IEnumerable<Location>> GetLocations()
		{
			var Result = new List<Location>();

			using ( var Stream = await WebClient.GetStreamAsync( Endpoint ) )
			using ( var DecompressedStream = new GZipStream( Stream, CompressionMode.Decompress ) )
			{
				try
				{
					await foreach ( var station in JsonSerializer.DeserializeAsyncEnumerable<WeatherStation>( DecompressedStream, SerializerOptionsWithFields ) )
					{
						if ( Math.Abs( station.lat ) > 90 )
						{
							continue;
						}

						var country = station.country;
						try
						{
							country = new RegionInfo( country ).EnglishName;
						}
						catch { }

						var prepend = IsInvalidString( station.icaoId ) ? "" : $"{station.icaoId} - ";

						// I hate it too
						var append = IsInvalidString( country ) ? "" : ( ( IsInvalidString( station.state ) || station.country != "US" ) ? $" ({country})" : $" ({station.state}, {country})" );

						var locationName = $"{prepend}{station.site}{append}";
						if ( !IsInvalidString( locationName ) )
						{
							Result.Add( new Location( locationName, station.lat, station.lon ) );
						}
					}
				}
				catch { }
			}

			return Result;
		}

	}
}
