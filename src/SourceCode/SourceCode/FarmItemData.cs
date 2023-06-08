using System;

[Serializable]
public class FarmItemData
{
	public string Key;

	public string Value;

	public FarmItemData()
	{
		Key = string.Empty;
		Value = string.Empty;
	}

	public FarmItemData(string inKey, string inValue)
	{
		Key = inKey;
		Value = inValue;
	}
}
