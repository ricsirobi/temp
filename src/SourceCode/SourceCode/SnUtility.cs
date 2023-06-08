using UnityEngine;

public class SnUtility
{
	public static string RemoveExt(string inName)
	{
		int num = inName.LastIndexOf('.');
		if (num != -1)
		{
			return inName.Substring(0, num).ToLower();
		}
		return inName.ToLower();
	}

	public static bool SafeClipCompare(AudioClip inClip1, AudioClip inClip2)
	{
		if (inClip1 == null)
		{
			return inClip2 == null;
		}
		if (inClip2 == null)
		{
			return false;
		}
		if (RemoveExt(inClip1.name) == RemoveExt(inClip2.name))
		{
			return true;
		}
		return false;
	}
}
