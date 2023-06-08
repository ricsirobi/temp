using UnityEngine;

public class ClansCrestSelectorData : KAWidgetUserData
{
	public string _CrestUrl;

	private bool mLoaded;

	private Texture mTexture;

	private bool mIsReady;

	public bool pIsReady => mIsReady;

	public ClansCrestSelectorData(string inCrestUrl)
	{
		_CrestUrl = inCrestUrl;
	}

	public void Load()
	{
		if (!mLoaded)
		{
			mLoaded = true;
			if (!string.IsNullOrEmpty(_CrestUrl))
			{
				string[] array = _CrestUrl.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(Texture));
			}
		}
	}

	public void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			mIsReady = true;
			mTexture = (Texture)inObject;
			_Item.SetInteractive(isInteractive: true);
			KAWidget kAWidget = _Item.FindChildItem("Loading");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			KAWidget kAWidget2 = _Item.FindChildItem("ClanCrestTemplate");
			if (!(kAWidget2 != null))
			{
				break;
			}
			kAWidget2.SetVisibility(inVisible: true);
			Transform transform = kAWidget2.transform.Find("Logo");
			if (transform != null)
			{
				UITexture component = transform.GetComponent<UITexture>();
				if (component != null)
				{
					component.mainTexture = mTexture;
				}
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mIsReady = true;
			UtDebug.Log("Error! " + inURL);
			break;
		}
	}

	public Texture GetTexture()
	{
		return mTexture;
	}
}
