using UnityEngine;

public class ObClickableCreateInstance : ObClickable
{
	public string _AssetName;

	private bool mIsInstanceLoading;

	private bool mActivate;

	public override void OnActivate()
	{
		base.OnActivate();
		mActivate = true;
	}

	public override void Update()
	{
		base.Update();
		if (mActivate)
		{
			mActivate = false;
			if (!mIsInstanceLoading && !string.IsNullOrEmpty(_AssetName))
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				AvAvatar.pState = AvAvatarState.PAUSED;
				AvAvatar.SetUIActive(inActive: false);
				mIsInstanceLoading = true;
				string[] array = _AssetName.Split('/');
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadObjectEvent, typeof(GameObject));
			}
		}
	}

	private void LoadObjectEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			mIsInstanceLoading = false;
			GameObject inObject2 = Object.Instantiate((GameObject)inObject);
			OnCreateInstance(inObject2);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mIsInstanceLoading = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			break;
		}
	}

	public virtual void OnCreateInstance(GameObject inObject)
	{
	}
}
