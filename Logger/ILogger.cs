using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Logger
{
	public interface ILogger
	{
		void Write(string message);
	}
}
