using System;
using System.Collections.Generic;

[Serializable]
public class KAUITreeDataGroup
{
	public string _Name;

	public LocaleString _DisplayText;

	public bool _Collapsed;

	public List<KAUITreeDataGroupChild> _ChildList;

	public KAUITreeDataGroup(string inName, LocaleString inLocaleString, bool inCollapsed, KAUITreeDataGroupChild[] inChildList)
	{
		_Name = inName;
		_DisplayText = inLocaleString;
		_Collapsed = inCollapsed;
		_ChildList = new List<KAUITreeDataGroupChild>(inChildList);
	}

	public KAUITreeDataGroup(string inName, LocaleString inLocaleString)
	{
		_Name = inName;
		_DisplayText = inLocaleString;
		_Collapsed = false;
		_ChildList = new List<KAUITreeDataGroupChild>();
	}

	public void AddChild(string inName, LocaleString inLocaleString)
	{
		if (_ChildList != null)
		{
			_ChildList.Add(new KAUITreeDataGroupChild(inName, inLocaleString));
		}
	}

	public void AddChild(string inName, LocaleString inLocaleString, string inURL, TreeMapData[] inDataList)
	{
		if (_ChildList != null)
		{
			_ChildList.Add(new KAUITreeDataGroupChild(inName, inLocaleString, inURL, inDataList));
		}
	}
}
