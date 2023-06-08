using System.Collections.Generic;

public class UnlockManager
{
	public delegate void OnSceneUnlockedCallBack(bool success);

	private static List<IUnlock> mUnlockList = new List<IUnlock>();

	public static void Add(IUnlock unlockInfo)
	{
		if (unlockInfo != null)
		{
			mUnlockList.Add(unlockInfo);
		}
	}

	public static void Remove(IUnlock unlockInfo)
	{
		if (unlockInfo != null)
		{
			mUnlockList.Remove(unlockInfo);
		}
	}

	public static bool IsSceneUnlocked(string inSceneName, bool inShowUi = false, OnSceneUnlockedCallBack inCallback = null)
	{
		foreach (IUnlock mUnlock in mUnlockList)
		{
			if (mUnlock == null || !mUnlock.IsSceneUnlocked(inSceneName, inShowUi, delegate(bool success)
			{
				if (success)
				{
					OnAsyncUnlockAction(inSceneName, inShowUi, inCallback);
				}
				else if (inCallback != null)
				{
					inCallback(success: false);
				}
			}))
			{
				return false;
			}
		}
		return true;
	}

	public static void OnAsyncUnlockAction(string inSceneName, bool inShowUi = false, OnSceneUnlockedCallBack inCallback = null)
	{
		if (inCallback != null && IsSceneUnlocked(inSceneName, inShowUi, inCallback))
		{
			inCallback(success: true);
		}
	}
}
