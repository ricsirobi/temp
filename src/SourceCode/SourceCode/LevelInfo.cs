using System;

[Serializable]
public class LevelInfo
{
	public int _PlayerLevel;

	public FarmItemInfo[] _Data;

	public float GetValue(string inKey)
	{
		float result = 0f;
		FarmItemInfo[] data = _Data;
		foreach (FarmItemInfo farmItemInfo in data)
		{
			if (farmItemInfo._Key.Equals(inKey))
			{
				result = farmItemInfo._Value;
				break;
			}
		}
		return result;
	}
}
