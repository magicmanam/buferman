using log4net;

namespace Logger
{
	public class Log4netLogger : ILogger
	{
		private readonly ILog _logger = LogManager.GetLogger(typeof(Log4netLogger));

		public void Write(string message)
		{
			this._logger.Info(message);
		}
	}
}
