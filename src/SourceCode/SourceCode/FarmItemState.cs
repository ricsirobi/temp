using System;
using System.Collections.Generic;

[Serializable]
public class FarmItemState
{
	public int ID;

	public List<FarmItemData> Data;

	public FarmItemState()
	{
		ID = 0;
		Data = new List<FarmItemData>();
	}

	public void Set(string inKey, string inValue)
	{
		bool flag = false;
		foreach (FarmItemData datum in Data)
		{
			if (datum.Key.Equals(inKey))
			{
				flag = true;
				datum.Value = inValue;
			}
		}
		if (!flag)
		{
			Data.Add(new FarmItemData(inKey, inValue));
		}
	}
}
