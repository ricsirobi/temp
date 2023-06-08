using UnityEngine;

public class BDayPropLoader : MonoBehaviour
{
	public string _PropName = "RS_DATA/PfGrpMSBirthday.unity3d/PfGrpMSBirthday";

	private string mResName = "";

	private bool mProcessed;

	private void Update()
	{
		if (!mProcessed && UserInfo.pIsReady)
		{
			mProcessed = true;
			if (UserInfo.IsBirthdayWeek())
			{
				string[] array = _PropName.Split('/');
				mResName = array[2];
				RsResourceManager.Load(array[0] + "/" + array[1], OnResLoadingEvent);
			}
		}
	}

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
			Object.Instantiate(CoBundleLoader.LoadGameObject(obj, mResName).transform, Vector3.zero, Quaternion.identity).name = mResName;
			base.gameObject.SetActive(value: false);
			break;
		}
		case RsResourceLoadEvent.PROGRESS:
		case RsResourceLoadEvent.ERROR:
			break;
		}
	}
}
