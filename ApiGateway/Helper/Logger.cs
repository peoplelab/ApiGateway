using System;
using System.IO;

namespace ApiGateway.Helper
{
	public sealed class Logger : IDisposable
	{
		public enum eLogLevels {
			DEBUG = 0,
			INFO = 1,
			WARNING = 2,
			ERROR = 3
		}
		private static readonly Logger instance = new Logger();
		private TextWriter _console = null;

		private Logger(){
			this._console = Console.Out;
		}

		public static Logger Instance{ get { return instance; } }

		public void Log(string message, eLogLevels loglevel){
			if (this._console == null) throw new Exception("You have not instance of Logger!");

			this._console.WriteLine("LOG " + DateTime.Now + " - " + message);
			this._console.Flush();
		}

		public void Dispose(){
			this._console.Close();
		}
	}
}
