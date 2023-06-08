using System;
using UnityEngine;

[Serializable]
public class PropData
{
	public string _FullResName = "";

	public bool _DontDestroyOnLoad;

	public string _LocationMarkerName = "";

	public string _MessageObjectName = "";

	private string mPropName = "";

	private CoPropLoader mPropLoader;

	public void OnResLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			AssetBundle obj = (AssetBundle)inObject;
			if (obj == null)
			{
				UtDebug.LogError("Can not download bundle ==> " + inURL);
			}
			GameObject gameObject = UnityEngine.Object.Instantiate(CoBundleLoader.LoadGameObject(obj, mPropName), Vector3.zero, Quaternion.identity);
			gameObject.name = mPropName;
			if (_LocationMarkerName.Length > 0)
			{
				GameObject gameObject2 = GameObject.Find(_LocationMarkerName);
				gameObject.transform.position = gameObject2.transform.position;
				gameObject.transform.rotation = gameObject2.transform.rotation;
				gameObject.BroadcastMessage("OnPropLoaded", gameObject2, SendMessageOptions.DontRequireReceiver);
				if (gameObject2 != null)
				{
					UnityEngine.Object.Destroy(gameObject2);
				}
			}
			mPropLoader.PropReady();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mPropLoader.PropReady();
			break;
		}
	}

	public void LoadProp(CoPropLoader ploader)
	{
		mPropLoader = ploader;
		string[] array = _FullResName.Split('/');
		mPropName = array[2];
		RsResourceManager.Load(array[0] + "/" + array[1], OnResLoadingEvent, RsResourceType.NONE, _DontDestroyOnLoad);
	}
}
