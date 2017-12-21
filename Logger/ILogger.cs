using System;

namespace Logging
{
	public interface ILogger
	{
		void Write(string message);

		void WriteError(string message, Exception exc);
	}
}
