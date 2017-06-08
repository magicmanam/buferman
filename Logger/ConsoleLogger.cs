using System;

namespace Logger
{
	public class ConsoleLogger : ILogger
	{
		public void Write(string message)
		{
			Console.WriteLine(message);
		}

		public void WriteError(string message, Exception exc)
		{
			throw new NotImplementedException();
		}
	}
}
