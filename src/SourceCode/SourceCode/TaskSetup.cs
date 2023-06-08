using System;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "TaskSetup", Namespace = "")]
public class TaskSetup
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

	private Task mTask;

	private bool mDelete;

	private GameObject mObject;

	public GameObject pObject => mObject;

	public void Setup(Task task)
	{
		mTask = task;
		pIsReady = true;
		mObject = null;
		if (Asset != null && ((string.IsNullOrEmpty(Scene) && AvAvatar.pToolbar != null) || (!string.IsNullOrEmpty(Scene) && Scene.Equals(RsResourceManager.pCurrentLevel))))
		{
			string[] array = Asset.Split('/');
			mObject = GameObject.Find(array[^1]);
			if (mObject != null && (Persistent || array.Length == 1 || mObject.GetComponent("NPCAvatar") != null))
			{
				if (string.Equals(Location, "remove", StringComparison.OrdinalIgnoreCase))
				{
					mObject.SetActive(value: false);
				}
				else if (Recursive)
				{
					UtUtilities.SetChildrenActive(mObject, active: true);
				}
				mTask.FinishSetup();
			}
			else if (array.Length == 1)
			{
				if (!string.Equals(Location, "remove", StringComparison.OrdinalIgnoreCase))
				{
					UtDebug.LogError("MissionManager - Setup Task " + task.TaskID + " Asset " + Asset + " not found for marker " + Location);
				}
				mTask.FinishSetup();
			}
			else if (array.Length == 3)
			{
				pIsReady = false;
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnLoadEvent, typeof(GameObject));
			}
		}
		else
		{
			mTask.FinishSetup();
		}
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
			mTask.FinishSetup();
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Asset not found! " + inURL);
			pIsReady = true;
			mTask.FinishSetup();
			break;
		}
	}

	public void FinishSetup()
	{
		if (!(mObject != null) || !mObject.activeInHierarchy)
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
					Collider[] componentsInChildren = mObject.GetComponentsInChildren<Collider>();
					if (componentsInChildren != null)
					{
						for (int i = 0; i < componentsInChildren.Length; i++)
						{
							if (componentsInChildren[i].enabled)
							{
								componentsInChildren[i].enabled = false;
								componentsInChildren[i].enabled = true;
							}
						}
					}
				}
			}
		}
		if (mTask.pData.Type == "Collect")
		{
			foreach (TaskObjective objective in mTask.pData.Objectives)
			{
				string text = objective.Get<string>("Name");
				string text2 = mTask.pPayload.Get<string>(text + "Collected");
				if (mTask.pIsTimedTask)
				{
					UtUtilities.SetChildrenActive(mObject, mTask._Active, recursive: false, text);
				}
				else
				{
					if (string.IsNullOrEmpty(text2))
					{
						continue;
					}
					string[] array = text2.Split('|');
					foreach (string text3 in array)
					{
						Transform transform = mObject.transform.Find(text + text3);
						if (transform != null)
						{
							transform.gameObject.SendMessage("Collected", SendMessageOptions.DontRequireReceiver);
						}
					}
					mObject.SendMessage("SetCollected", objective, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		mObject.BroadcastMessage("SetupForTask", mTask, SendMessageOptions.DontRequireReceiver);
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
			else if (mTask.pIsTimedTask && mTask.pData.Type == "Collect")
			{
				string objectiveValue = mTask.GetObjectiveValue<string>("Name");
				UtUtilities.SetChildrenActive(mObject, active: false, recursive: false, objectiveValue);
			}
		}
	}
}
