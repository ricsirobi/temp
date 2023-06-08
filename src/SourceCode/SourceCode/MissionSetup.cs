using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "MissionSetup", Namespace = "")]
public class MissionSetup
{
	[XmlElement(ElementName = "Scene")]
	public string Scene;

	[XmlElement(ElementName = "Asset")]
	public string Asset;

	[XmlElement(ElementName = "Location")]
	public string Location;

	[XmlElement(ElementName = "Recursive")]
	public bool Recursive;

	[XmlElement(ElementName = "Persistent")]
	public bool Persistent;

	public bool pIsReady;

	private string mLevel = "";

	private Mission mMission;

	private bool mDelete;

	private GameObject mObject;

	public GameObject pObject => mObject;

	public void Setup(Mission mission)
	{
		mMission = mission;
		pIsReady = true;
		if (mLevel.Equals(RsResourceManager.pCurrentLevel))
		{
			return;
		}
		mLevel = RsResourceManager.pCurrentLevel;
		if ((!string.IsNullOrEmpty(Scene) || !(AvAvatar.pToolbar != null)) && (string.IsNullOrEmpty(Scene) || !Scene.Equals(RsResourceManager.pCurrentLevel)))
		{
			return;
		}
		if (Asset == null)
		{
			FinishSetup();
			UtDebug.LogError("Mission Setup Asset is null");
			return;
		}
		string[] array = Asset.Split('/');
		mObject = GameObject.Find(array[^1]);
		if (mObject != null)
		{
			if (Recursive)
			{
				UtUtilities.SetChildrenActive(mObject, active: true);
			}
			FinishSetup();
		}
		else if (array.Length == 1)
		{
			if (!string.Equals(Location, "remove", StringComparison.OrdinalIgnoreCase))
			{
				UtDebug.LogError("MissionManager - Setup Mission " + mission.MissionID + " Asset " + Asset + " not found for marker " + Location);
			}
		}
		else if (array.Length == 3)
		{
			pIsReady = false;
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnLoadEvent, typeof(GameObject));
		}
	}

	public void OnLevelLoaded()
	{
		mLevel = "";
		pIsReady = false;
	}

	private void OnLoadEvent(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			pIsReady = true;
			GameObject gameObject = (GameObject)inObject;
			mObject = UnityEngine.Object.Instantiate(gameObject);
			mObject.name = gameObject.name;
			if (!Persistent && mObject.GetComponent("NPCAvatar") == null)
			{
				mDelete = true;
			}
			FinishSetup();
			mMission.OnDownloadDone();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Asset not found! " + inURL);
			pIsReady = true;
			mMission.OnDownloadDone();
			break;
		}
	}

	private void FinishSetup()
	{
		if (mObject == null)
		{
			return;
		}
		if (!string.IsNullOrEmpty(Location))
		{
			if (string.Equals(Location, "remove", StringComparison.OrdinalIgnoreCase))
			{
				mObject.SetActive(value: false);
			}
			else
			{
				GameObject gameObject = GameObject.Find(Location);
				if (gameObject != null)
				{
					mObject.transform.position = gameObject.transform.position;
					mObject.transform.rotation = gameObject.transform.rotation;
				}
			}
		}
		mObject.SendMessage("SetupForMission", mMission, SendMessageOptions.DontRequireReceiver);
	}

	public void CleanUp()
	{
		if (mObject != null)
		{
			if (mDelete)
			{
				UnityEngine.Object.Destroy(mObject);
			}
			else if (Recursive)
			{
				UtUtilities.SetChildrenActive(mObject, active: false);
			}
		}
	}
}
