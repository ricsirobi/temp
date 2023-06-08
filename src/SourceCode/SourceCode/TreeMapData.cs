using System;

[Serializable]
public class TreeMapData
{
	public string _Key;

	public LocaleString _Text;

	public TreeMapData(string inKey, LocaleString inValue)
	{
		_Key = inKey;
		_Text = inValue;
	}
}
