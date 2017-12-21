using System;

namespace Logging
{
	public static class Logger
    {
		public static ILogger Current { get; set; }

        public static void Write(string message)
        {
            Current.Write(message);
        }

        public static void WriteError(string message, Exception exc)
        {
            Current.WriteError(message, exc);
        }
    }
}
