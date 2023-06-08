using UnityEngine;

public class UtDebug
{
	private const uint LOG_MASK_NONE = 0u;

	private const uint LOG_MASK_ALL = uint.MaxValue;

	public static int _Level;

	public static bool _IsOverridenToError;

	public static uint _Mask;

	public static void Assert(bool inExpressonResult)
	{
		Assert(inExpressonResult, "--NO MESSAGE--");
	}

	public static void Assert(bool inExpressonResult, string message)
	{
		if (!inExpressonResult)
		{
			LogWarning("ASSERT FAILED: " + message);
		}
	}

	public static void Log(object message)
	{
		Log(message, 50);
	}

	public static void Log(object message, int priority)
	{
		if (ProductConfig.pInstance != null && ProductConfig.pInstance.EnableDebugLog.HasValue && ProductConfig.pInstance.EnableDebugLog.Value)
		{
			_Level = 100;
		}
		if (priority <= _Level)
		{
			if (!_IsOverridenToError)
			{
				Debug.Log(message);
			}
			else
			{
				Debug.LogError(message);
			}
		}
	}

	public static void Log(object message, uint mask)
	{
		if (ProductConfig.pInstance != null && ProductConfig.pInstance.EnableDebugLog.HasValue && ProductConfig.pInstance.EnableDebugLog.Value)
		{
			_Mask = uint.MaxValue;
		}
		if ((_Mask & mask) != 0)
		{
			if (!_IsOverridenToError)
			{
				Debug.Log(message);
			}
			else
			{
				Debug.LogError(message);
			}
		}
	}

	public static void LogFastP(int priority, params object[] message)
	{
		if (priority <= _Level)
		{
			if (!_IsOverridenToError)
			{
				Debug.Log(string.Concat(message));
			}
			else
			{
				Debug.LogError(string.Concat(message));
			}
		}
	}

	public static void LogFastM(uint mask, params object[] message)
	{
		if ((_Mask & mask) != 0)
		{
			if (!_IsOverridenToError)
			{
				Debug.Log(string.Concat(message));
			}
			else
			{
				Debug.LogError(string.Concat(message));
			}
		}
	}

	public static void LogWarning(object message)
	{
		LogWarning(message, 50);
	}

	public static void LogWarning(object message, int priority)
	{
		if (ProductConfig.pInstance != null && ProductConfig.pInstance.EnableDebugLog.HasValue && ProductConfig.pInstance.EnableDebugLog.Value)
		{
			_Level = 100;
		}
		if (priority <= _Level)
		{
			if (!_IsOverridenToError)
			{
				Debug.LogWarning(message);
			}
			else
			{
				Debug.LogError(message);
			}
		}
	}

	public static void LogError(object message)
	{
		LogError(message, 50);
	}

	public static void LogError(object message, int priority)
	{
		if (ProductConfig.pInstance != null && ProductConfig.pInstance.EnableDebugLog.HasValue && ProductConfig.pInstance.EnableDebugLog.Value)
		{
			_Level = 100;
		}
		if (priority <= _Level)
		{
			Debug.LogError(message);
		}
	}
}
