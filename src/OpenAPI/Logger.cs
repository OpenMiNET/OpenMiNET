using System;
using System.Reflection;
using System.Runtime.CompilerServices;
using log4net;

namespace OpenAPI
{
	public class Logger
	{
		private readonly ILog _log;

		[MethodImpl(MethodImplOptions.NoInlining)]
		private Logger(ILog log4NetLogger)
		{
			_log = log4NetLogger;
		}

		public void Info(string message)
		{
			_log.Info(message);
		}

		public void Warn(string message)
		{
			_log.Warn(message);
		}

		public void Error(string message)
		{
			_log.Error(message);
		}

		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static Logger GetLogger(string name)
		{
			return new Logger(LogManager.GetLogger(Assembly.GetCallingAssembly(), name));
		}

		[MethodImplAttribute(MethodImplOptions.NoInlining)]
		public static Logger GetLogger(Type type)
		{
			return new Logger(LogManager.GetLogger(type.Assembly, type));
		}
	}
}
