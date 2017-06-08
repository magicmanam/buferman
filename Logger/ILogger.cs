using System;

namespace Logger
{
	public interface ILogger
	{
		void Write(string message);

		void WriteError(string message, Exception exc);
	}
}
