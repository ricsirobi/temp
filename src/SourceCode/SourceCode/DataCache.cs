using System.Collections.Generic;

public class DataCache
{
	private static Dictionary<string, object> mCache = new Dictionary<string, object>();

	public static bool Get<TYPE>(string key, out TYPE inObject)
	{
		if (mCache.TryGetValue(key, out var value))
		{
			inObject = (TYPE)value;
			return true;
		}
		inObject = default(TYPE);
		return false;
	}

	public static void Set(string key, object o)
	{
		mCache[key] = o;
	}

	public static bool Remove(string key)
	{
		return mCache.Remove(key);
	}
}
