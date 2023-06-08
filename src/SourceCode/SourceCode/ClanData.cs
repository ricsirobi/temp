using UnityEngine;

public class ClanData : KAWidgetUserData
{
	public Group _Group;

	public Texture _Texture;

	private bool mLoaded;

	private Color mColorFG = Color.white;

	private Color mColorBG = Color.white;

	private KAWidget mCrestBanner;

	public ClanData(Group inGroup, KAWidget inCrestBanner = null)
	{
		_Group = inGroup;
		mCrestBanner = inCrestBanner;
	}

	public void Load(bool forceLoad = false)
	{
		if (!mLoaded || forceLoad)
		{
			mLoaded = true;
			if (forceLoad || mCrestBanner == null)
			{
				mCrestBanner = _Item.FindChildItem("ClanCrestTemplate");
			}
			if (!string.IsNullOrEmpty(_Group.Logo))
			{
				string[] array = _Group.Logo.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(Texture));
			}
		}
	}

	public void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent != RsResourceLoadEvent.COMPLETE)
		{
			return;
		}
		bool flag = true;
		_Texture = (Texture)inObject;
		if (_Item != null)
		{
			KAWidget kAWidget = _Item.FindChildItem("Loading");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			object userData = _Item.GetUserData();
			if (userData != null && userData.GetType() == typeof(ClanData))
			{
				ClanData clanData = (ClanData)_Item.GetUserData();
				if (clanData != null && clanData._Group.GroupID != _Group.GroupID)
				{
					flag = false;
				}
			}
		}
		if (!(mCrestBanner != null && flag))
		{
			return;
		}
		mCrestBanner.SetVisibility(inVisible: false);
		UITexture component = mCrestBanner.transform.Find("Logo").GetComponent<UITexture>();
		if (component != null)
		{
			component.mainTexture = _Texture;
			if (_Group.GetFGColor(out mColorFG))
			{
				component.color = mColorFG;
			}
		}
		component = mCrestBanner.transform.Find("Background").GetComponent<UITexture>();
		if (component != null && _Group.GetBGColor(out mColorBG))
		{
			component.color = mColorBG;
		}
		mCrestBanner.SetVisibility(inVisible: true);
	}
}
