using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Polaris
{
	internal class Log
	{
		private static readonly StringBuilder logData;
		public static bool ShouldSave = false;

		static Log()
		{
			logData = new StringBuilder();
			var initTime = DateTime.Now.ToString( "yyyy-MM-ddTHH:mm:ss.fffzzz" );
			var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
			Info( $"Logging enabled at {initTime} (version {version})" );
		}

		private static string AssemblyLoadDirectory
		{
			get { return Path.GetDirectoryName( new Uri( System.Reflection.Assembly.GetExecutingAssembly().Location ).LocalPath ); }
		}

		private static void WriteLine( string level, string area, string s )
		{
			var message = $"[{DateTime.Now:HH:mm:ss.fff}][{level}][{area}] {s}";
			System.Diagnostics.Debug.WriteLine( message );
			logData.AppendLine( message );
		}

		[System.Diagnostics.Conditional( "DEBUG" )]
		public static void Debug( string s, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" )
		{
			var area = $"{Path.GetFileNameWithoutExtension( sourceFilePath )}::{memberName}";
			WriteLine( "Debug", area, s );
		}

		public static void Info( string s, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" )
		{
			var area = $"{Path.GetFileNameWithoutExtension( sourceFilePath )}::{memberName}";
			WriteLine( "Info", area, s );
		}

		public static void Warning( string s, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" )
		{
#if DEBUG
			ShouldSave = true;
#endif
			var area = $"{Path.GetFileNameWithoutExtension( sourceFilePath )}::{memberName}";
			WriteLine( "Warning", area, s );
		}

		public static void Error( string s, bool assert = false, [CallerFilePath] string sourceFilePath = "", [CallerMemberName] string memberName = "" )
		{
			ShouldSave = true;
			var area = $"{Path.GetFileNameWithoutExtension( sourceFilePath )}::{memberName}";
			WriteLine( "Error", area, s );
			if ( assert )
			{
				throw new Exception( s );
			}
		}

		public static void Save()
		{
			var filename = $"Log_{DateTime.Now:yyyy-MM-dd_HH-mm-ss}.txt";
			var path = Path.Combine( AssemblyLoadDirectory, filename );
			Save( path );
		}

		public static void Save( string path )
		{
			using ( var dest = new StreamWriter( path ) )
			{
				dest.Write( logData );
			}
		}

		public static void ConditionalSave()
		{
			if ( ShouldSave )
			{
				Save();
			}
		}
	}
}
