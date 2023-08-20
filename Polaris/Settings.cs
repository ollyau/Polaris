using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Threading.Tasks;

namespace Polaris
{
	class Settings
	{
		private static readonly Dictionary<string, object> _data = new Dictionary<string, object>();
		private static readonly string _settingsFilename = "settings.json";

		static Settings()
		{
			if ( File.Exists( _settingsFilename ) )
			{
				using ( var fs = new FileStream( _settingsFilename, FileMode.Open ) )
				{
					var data = JsonSerializer.Deserialize( fs, typeof( Dictionary<string, object> ) );
					if ( data != null )
					{
						_data = ( Dictionary<string, object> )data;
					}
				}
			}
		}

		public static T Get<T>( string key )
		{
			object value;
			var result = _data.TryGetValue( key, out value );
			if ( result )
			{
				if ( value is JsonElement )
				{
					return ( ( JsonElement )value ).Deserialize<T>();
				}
				return ( T )value;
			}
			else
			{
				throw new KeyNotFoundException( $"Key '{key}' not found in configuration." );
			}
		}

		public static T Get<T>( string key, T defaultValue )
		{
			try
			{
				return Get<T>( key );
			}
			catch ( KeyNotFoundException )
			{
				return defaultValue;
			}
		}
	}
}
