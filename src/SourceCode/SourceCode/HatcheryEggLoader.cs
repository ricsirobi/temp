using System;
using UnityEngine;

[Serializable]
public class HatcheryEggLoader
{
	public GameObject _InObject;

	public Vector3 _EggPosition = Vector3.zero;

	public GameObject _InReceiver;

	private HatcheryManager mHatcheryManager;

	private GameObject mPickedUpEgg;

	public HatcheryEggLoader(HatcheryManager inMan)
	{
		mHatcheryManager = inMan;
	}

	public void OnAssetLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (!(_InObject == null))
			{
				if (mPickedUpEgg != null)
				{
					UnityEngine.Object.Destroy(mPickedUpEgg);
					mPickedUpEgg = null;
				}
				mPickedUpEgg = UnityEngine.Object.Instantiate((GameObject)inObject, Vector3.zero, Quaternion.identity);
				ObClickableDragonEgg component = mPickedUpEgg.GetComponent<ObClickableDragonEgg>();
				if (component != null)
				{
					component._Active = false;
					mHatcheryManager.IsFromStables = false;
				}
				mHatcheryManager.pEgg = mPickedUpEgg;
				_InReceiver?.SendMessage("StartHatching");
			}
			break;
		case RsResourceLoadEvent.ERROR:
			_InReceiver?.SendMessage("StartHatching");
			break;
		}
	}
}
