using System;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Event;

public class ObEventAutoRevert : MonoBehaviour
{
	[Serializable]
	public class AutoRevertObj
	{
		public GameObject _GameObject;

		public ACTIVE _ActiveState;
	}

	public enum ACTIVE
	{
		ActiveAlways,
		ActiveNotGrace,
		Grace
	}

	public string _EventName;

	public List<AutoRevertObj> _AutoRevertObjects;

	public void OnLevelReady()
	{
		EventManager activeEvent = EventManager.GetActiveEvent();
		if (!activeEvent || activeEvent._EventName != _EventName)
		{
			return;
		}
		bool flag = activeEvent.GracePeriodInProgress();
		foreach (AutoRevertObj autoRevertObject in _AutoRevertObjects)
		{
			if (!(autoRevertObject._GameObject == null))
			{
				autoRevertObject._GameObject.SetActive(value: false);
				switch (autoRevertObject._ActiveState)
				{
				case ACTIVE.ActiveAlways:
					autoRevertObject._GameObject.SetActive(value: true);
					break;
				case ACTIVE.ActiveNotGrace:
					autoRevertObject._GameObject.SetActive(!flag);
					break;
				case ACTIVE.Grace:
					autoRevertObject._GameObject.SetActive(flag);
					break;
				}
			}
		}
	}
}
