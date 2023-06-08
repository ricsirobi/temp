using System.Collections.Generic;
using UnityEngine;

public class ObSwitchBase : KAMonoBase
{
	public bool _OneWaySwitch;

	public string[] _DeactivateObject;

	public List<GameObject> _MessageObjects = new List<GameObject>();

	public bool _FindObjectsAtRuntime;

	protected bool mOneWayDone;

	protected bool mSwitchOn;

	private List<GameObject> mDeactivateObjectContainer = new List<GameObject>();

	protected virtual void OnEnable()
	{
		if (_DeactivateObject != null && !_FindObjectsAtRuntime)
		{
			FindObjects();
		}
	}

	protected virtual void FindObjects()
	{
		if (_DeactivateObject == null)
		{
			return;
		}
		for (int i = 0; i < _DeactivateObject.Length; i++)
		{
			GameObject gameObject = GameObject.Find(_DeactivateObject[i]);
			if (gameObject != null)
			{
				mDeactivateObjectContainer.Add(gameObject);
			}
		}
	}

	protected virtual void SwitchOn()
	{
		if (_OneWaySwitch && mOneWayDone)
		{
			return;
		}
		mOneWayDone = true;
		mSwitchOn = true;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "SwitchOn", base.name);
		}
		if (_MessageObjects.Count > 0)
		{
			foreach (GameObject messageObject in _MessageObjects)
			{
				if (messageObject != null)
				{
					messageObject.SendMessage("OnStateChange", true, SendMessageOptions.DontRequireReceiver);
					messageObject.SendMessage("OnSwitchOn", base.name, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (_FindObjectsAtRuntime)
		{
			FindObjects();
		}
		if (mDeactivateObjectContainer.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in mDeactivateObjectContainer)
		{
			if (item != null)
			{
				item.SetActive(value: false);
			}
		}
	}

	protected virtual void SwitchOff(bool message = true)
	{
		if (_OneWaySwitch && mOneWayDone)
		{
			return;
		}
		mOneWayDone = true;
		mSwitchOn = false;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "SwitchOff", base.name);
		}
		if (_MessageObjects.Count > 0 && message)
		{
			foreach (GameObject messageObject in _MessageObjects)
			{
				if (messageObject != null)
				{
					messageObject.SendMessage("OnStateChange", false, SendMessageOptions.DontRequireReceiver);
					messageObject.SendMessage("OnSwitchOff", base.name, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
		if (_FindObjectsAtRuntime)
		{
			FindObjects();
		}
		if (mDeactivateObjectContainer.Count <= 0)
		{
			return;
		}
		foreach (GameObject item in mDeactivateObjectContainer)
		{
			if (item != null)
			{
				item.SetActive(value: true);
			}
		}
	}
}
