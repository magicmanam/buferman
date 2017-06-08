using log4net;
using System;

namespace Logger
{
	public class Log4netLogger : ILogger
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(Log4netLogger));

		public void Write(string message)
		{
			this._logger.Info(message);
		}

		public void WriteError(string message, Exception exc)
		{
			this._logger.Error(message, exc);
		}
	}
}
