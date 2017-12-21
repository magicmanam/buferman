using System;

namespace Logging
{
	public class ConsoleLogger : ILogger
	{
		public void Write(string message)
		{
			Console.WriteLine(message);
		}

		public void WriteError(string message, Exception exc)
		{
            Console.Write(message, exc.StackTrace);
		}
	}
}
