using System.Collections.Generic;

public class KAUITreeListItemData : KAWidgetUserData
{
	public string _Name;

	public bool _Collapsed;

	public LocaleString _DisplayText;

	public List<KAUITreeListItemData> _ChildList;

	private KAUITreeListItemData mParent;

	private KAWidgetUserData mUserData;

	public KAUITreeListItemData pParent => mParent;

	public KAWidgetUserData pUserData => mUserData;

	public KAUITreeListItemData(KAUITreeListItemData inParent, string inName, KAWidgetUserData data)
	{
		_Name = inName;
		mUserData = data;
		_Collapsed = false;
		_ChildList = new List<KAUITreeListItemData>();
		mParent = inParent;
	}

	public KAUITreeListItemData(KAUITreeListItemData inParent, string inName, LocaleString inLocaleString)
	{
		_Name = inName;
		_DisplayText = inLocaleString;
		_Collapsed = false;
		_ChildList = new List<KAUITreeListItemData>();
		mParent = inParent;
	}

	public KAUITreeListItemData(KAUITreeListItemData inParent, string inName, LocaleString inLocaleString, bool inCollapsed, List<KAUITreeListItemData> inChildList)
		: this(inParent, inName, inLocaleString)
	{
		_Collapsed = inCollapsed;
		_ChildList = ((inChildList == null) ? new List<KAUITreeListItemData>() : new List<KAUITreeListItemData>(inChildList));
	}

	public void AddChild(string inName, LocaleString inLocaleString)
	{
		if (_ChildList != null)
		{
			_ChildList.Add(new KAUITreeListItemData(this, inName, inLocaleString));
		}
	}

	public void AddChild(string inName, LocaleString inLocaleString, bool inCollapsed, List<KAUITreeListItemData> inChildList)
	{
		if (_ChildList != null)
		{
			_ChildList.Add(new KAUITreeListItemData(this, inName, inLocaleString, inCollapsed, inChildList));
		}
	}

	public void AddChild(KAUITreeListItemData inChild)
	{
		if (_ChildList != null)
		{
			_ChildList.Add(inChild);
		}
	}

	public bool IsRootItem()
	{
		return mParent == null;
	}

	public bool IsLeafItem()
	{
		if (_ChildList != null)
		{
			return _ChildList.Count <= 0;
		}
		return true;
	}
}
