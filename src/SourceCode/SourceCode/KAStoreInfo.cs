using System;
using UnityEngine;

[Serializable]
public class KAStoreInfo : KAWidgetUserData
{
	public string _Name = "";

	public LocaleString _ToolTipText;

	public LocaleString _DisplayText;

	public string _DefaultCatName = "";

	public string _MobileDefaultCatName = "";

	public string _GUIName = "";

	public string _PfName = "";

	public int[] _IDs;

	public AudioClip _RVO;

	public AudioClip _CompletedVO;

	public Texture2D _Icon;

	[NonSerialized]
	public UnityEngine.Object _UserData;

	public int _RankTypeID;

	public LocaleString _RankLockedText = new LocaleString("You need to level up to buy this item");

	[NonSerialized]
	public float _LoadedTime;

	public KAStoreMenuItemData[] _MenuData;

	public bool _OnlyForPet;

	public bool _ShowSale = true;

	public KAStoreMenuItemData GetCategory(string inName)
	{
		if (_MenuData == null || _MenuData.Length == 0 || string.IsNullOrEmpty(inName))
		{
			return null;
		}
		KAStoreMenuItemData[] menuData = _MenuData;
		foreach (KAStoreMenuItemData kAStoreMenuItemData in menuData)
		{
			if (kAStoreMenuItemData != null && kAStoreMenuItemData._Name == inName)
			{
				return kAStoreMenuItemData;
			}
		}
		return null;
	}

	public KAStoreMenuItemData GetDefaultCategory()
	{
		KAStoreMenuItemData category = GetCategory(_DefaultCatName);
		if (category == null)
		{
			return null;
		}
		return category;
	}

	public StoreFilter GetDefaultFilter()
	{
		return GetDefaultCategory()?._Filter;
	}
}
