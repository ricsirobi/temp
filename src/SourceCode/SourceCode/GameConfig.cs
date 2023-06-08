using System;
using System.Collections.Generic;
using UnityEngine;

public class GameConfig : ScriptableObject
{
	[Serializable]
	public class GameConfigKeys
	{
		public string _Key;

		public string _Web;

		public string _Mobile;
	}

	public GameConfigKeys[] _KeyData;

	private Dictionary<string, string> mCachedKeyData = new Dictionary<string, string>();

	private static GameConfig mInstance;

	public static GameConfig pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = Resources.Load("GameConfig") as GameConfig;
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<GameConfig>();
				}
				else if (mInstance.mCachedKeyData.Count == 0)
				{
					for (int i = 0; i < mInstance._KeyData.Length; i++)
					{
						if (UtPlatform.IsMobile() && !string.IsNullOrEmpty(mInstance._KeyData[i]._Mobile))
						{
							mInstance.mCachedKeyData.Add(mInstance._KeyData[i]._Key, mInstance._KeyData[i]._Mobile);
						}
						else if (!string.IsNullOrEmpty(mInstance._KeyData[i]._Web))
						{
							mInstance.mCachedKeyData.Add(mInstance._KeyData[i]._Key, mInstance._KeyData[i]._Web);
						}
					}
				}
			}
			return mInstance;
		}
	}

	public static string GetKeyData(string inKey)
	{
		if (pInstance.mCachedKeyData.Count == 0)
		{
			UtDebug.LogError("GameConfig: No keys defined. Please populate the necessary data.");
			return null;
		}
		string value = "";
		if (!pInstance.mCachedKeyData.TryGetValue(inKey, out value))
		{
			UtDebug.LogError("GameConfig: Could not find key " + inKey);
			return null;
		}
		return value;
	}
}
