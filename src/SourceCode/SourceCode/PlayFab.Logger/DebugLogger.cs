using System;
using UnityEngine;

namespace PlayFab.Logger;

public class DebugLogger : ILogger
{
	private LogLevel minLogLevel;

	public DebugLogger(LogLevel minLogLevel = LogLevel.Information)
	{
		this.minLogLevel = minLogLevel;
	}

	public void Critical(string format, params object[] args)
	{
		Log(LogLevel.Critical, format, args);
	}

	public void Debug(string format, params object[] args)
	{
		Log(LogLevel.Debug, format, args);
	}

	public void Error(string format, params object[] args)
	{
		Log(LogLevel.Error, format, args);
	}

	public void Information(string format, params object[] args)
	{
		Log(LogLevel.Information, format, args);
	}

	public void Trace(string format, params object[] args)
	{
		Log(LogLevel.Trace, format, args);
	}

	public void Warning(string format, params object[] args)
	{
		Log(LogLevel.Warning, format, args);
	}

	public void Log(LogLevel logLevel, string format, params object[] args)
	{
		Type typeFromHandle = typeof(LogLevel);
		if (Enum.IsDefined(typeFromHandle, logLevel) && logLevel >= minLogLevel)
		{
			string message = $"{DateTime.Now} LOG {Enum.GetName(typeFromHandle, logLevel)}: {string.Format(format, args)}";
			switch (logLevel)
			{
			case LogLevel.Error:
			case LogLevel.Critical:
				UnityEngine.Debug.LogError(message);
				break;
			case LogLevel.Warning:
				UnityEngine.Debug.LogWarning(message);
				break;
			default:
				UnityEngine.Debug.Log(message);
				break;
			}
		}
	}
}
