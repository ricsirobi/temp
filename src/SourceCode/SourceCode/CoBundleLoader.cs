using System.Collections.Generic;
using UnityEngine;

public class CoBundleLoader
{
	public List<AssetBundle> _Bundles = new List<AssetBundle>();

	public List<string> _BundleNames = new List<string>();

	public List<UtTable> _LevelTables = new List<UtTable>();

	public UtTable _MasterTbl;

	public UtTableXMLReader _TableParser;

	public string _CursorLoadingItemName = "Loading";

	public bool _Locked = true;

	protected int mNumBundleLoaded;

	protected int mNumBundles;

	protected string mMasterTblName = "";

	public static bool _ForceUnlockAll;

	public void SetLocked(bool locked)
	{
		_Locked = !SubscriptionInfo.pIsMember;
	}

	public bool IsNotLocked(int lockFlag)
	{
		if (_ForceUnlockAll)
		{
			return true;
		}
		if (_Locked)
		{
			return lockFlag == 1;
		}
		return true;
	}

	public static Texture LoadTexture(AssetBundle bd, string s)
	{
		if (bd == null)
		{
			UtDebug.LogError("Bundle for " + s + " not loaded yet.");
			return null;
		}
		if (s == null || s.Length == 0)
		{
			return null;
		}
		Texture obj = (Texture)bd.LoadAsset(s, typeof(Texture));
		if (obj == null)
		{
			UtDebug.LogError("Resource [" + s + "] doesn't exist in bundle " + bd.name);
		}
		return obj;
	}

	public bool IsLoading()
	{
		return mNumBundleLoaded < mNumBundles;
	}

	public static GameObject LoadGameObject(AssetBundle bd, string s)
	{
		if (bd == null)
		{
			UtDebug.LogError("Bundle for " + s + " not loaded yet.");
			return null;
		}
		if (s == null || s.Length == 0)
		{
			return null;
		}
		GameObject obj = (GameObject)bd.LoadAsset(s, typeof(GameObject));
		if (obj == null)
		{
			UtDebug.LogError("Resource [" + s + "] doesn't exist.");
		}
		return obj;
	}

	public static AudioClip LoadAudioClip(AssetBundle bd, string s)
	{
		if (bd == null)
		{
			UtDebug.LogError("Bundle for " + s + " not loaded yet.");
			return null;
		}
		if (s == null || s.Length == 0)
		{
			return null;
		}
		AudioClip obj = (AudioClip)bd.LoadAsset(s, typeof(AudioClip));
		if (obj == null)
		{
			UtDebug.LogError("Resource [" + s + "] doesn't exist.");
		}
		return obj;
	}

	public bool IsBundleInList(string bname)
	{
		foreach (string bundleName in _BundleNames)
		{
			if (bundleName == bname)
			{
				return true;
			}
		}
		return false;
	}

	public static AssetBundle FindBundle(string bname)
	{
		AssetBundle obj = (AssetBundle)RsResourceManager.Load(bname, null);
		if (obj == null)
		{
			UtDebug.LogError("Bundle [" + bname + "] not loaded yet.");
		}
		return obj;
	}

	public virtual void ProcessBundle(string bname)
	{
		if (!IsBundleInList(bname))
		{
			_BundleNames.Add(bname);
		}
	}

	public static void SetVisibleRangeStatic(KAUIMenu menu, int sIdx, int numItems)
	{
		int num = sIdx + numItems - 1;
		if (num >= menu.GetNumItems())
		{
			num = menu.GetNumItems() - 1;
		}
		if (!(menu != null))
		{
			return;
		}
		for (int i = sIdx; i <= num; i++)
		{
			KAWidget kAWidget = menu.FindItemAt(i);
			CoBundleItemData coBundleItemData = (CoBundleItemData)kAWidget.GetUserData();
			if (coBundleItemData == null)
			{
				UtDebug.LogError("Item " + kAWidget.name + " at index = " + i + " missing user data");
			}
			else if (coBundleItemData.IsNotLoaded())
			{
				coBundleItemData.LoadResource();
			}
		}
	}

	public virtual void SetVisibleRange(KAUIMenu menu, int sIdx, int numItems)
	{
		SetVisibleRangeStatic(menu, sIdx, numItems);
	}

	public virtual bool IsRecordInMenu(UtTable table, int recIdx)
	{
		return true;
	}

	public static KAWidget GetLoadingItemStatic(CoBundleItemData ud)
	{
		KAWidget component = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUILoadingGears")).GetComponent<KAWidget>();
		if (!component)
		{
			return null;
		}
		component.SetVisibility(inVisible: true);
		if (ud != null)
		{
			component.SetUserData(ud);
		}
		component.SetState(KAUIState.NOT_INTERACTIVE);
		return component;
	}

	public void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			AssetBundle assetBundle = (AssetBundle)inObject;
			if (assetBundle == null)
			{
				UtDebug.LogError("Can not down load bundle ==> " + inURL);
			}
			else
			{
				assetBundle.name = inURL;
				_Bundles.Add(assetBundle);
			}
			mNumBundleLoaded++;
			if (mNumBundleLoaded >= mNumBundles)
			{
				OnAllBundlesLoaded();
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mNumBundleLoaded++;
			break;
		}
	}

	public virtual void OnMainTableLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			_TableParser = new UtTableXMLReader();
			_TableParser.LoadString((string)inObject);
			_MasterTbl = _TableParser[mMasterTblName];
			if (_MasterTbl == null)
			{
				UtDebug.LogError("!! Main table " + mMasterTblName + " load failed.");
			}
			OnMainTableLoaded();
		}
	}

	public virtual void OnAllBundlesLoaded()
	{
	}

	public virtual void CreateAndLoadBundleList()
	{
		mNumBundles = _BundleNames.Count;
		_Bundles.Clear();
		if (mNumBundles == 0)
		{
			OnAllBundlesLoaded();
			return;
		}
		mNumBundleLoaded = 0;
		foreach (string bundleName in _BundleNames)
		{
			RsResourceManager.Load(bundleName, OnResLoadingEvent);
		}
	}

	public virtual void OnMainTableLoaded()
	{
		CreateAndLoadBundleList();
	}

	public virtual void LoadMainTable(string xmlName, string tName)
	{
		mMasterTblName = tName;
		RsResourceManager.Load(xmlName, OnMainTableLoadingEvent);
	}
}
