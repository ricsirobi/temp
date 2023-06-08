using System;
using UnityEngine;

public class ObMissionEvent : MonoBehaviour
{
	public enum Action
	{
		ACTIVE,
		VISIBLE
	}

	[Serializable]
	public class Data
	{
		public GameObject _Object;

		public MissionEvent _Event;

		public int _ID;

		public Action _Action;

		public bool _Flag;
	}

	public Data[] _EventData;

	private void Start()
	{
		MissionManager.AddMissionEventHandler(OnMissionEvent);
	}

	private void OnDestroy()
	{
		MissionManager.RemoveMissionEventHandler(OnMissionEvent);
	}

	public void OnMissionEvent(MissionEvent inEvent, object inObject)
	{
		if (_EventData == null)
		{
			return;
		}
		for (int i = 0; i < _EventData.Length; i++)
		{
			if (inEvent != _EventData[i]._Event)
			{
				continue;
			}
			if (inObject.GetType() == typeof(MissionManager.Action))
			{
				MissionManager.Action action = inObject as MissionManager.Action;
				if (action._Object == null)
				{
					continue;
				}
				if (action._Object.GetType() == typeof(Task))
				{
					if ((action._Object as Task).TaskID == _EventData[i]._ID)
					{
						DoAction(_EventData[i]._Object, _EventData[i]._Action, _EventData[i]._Flag);
					}
				}
				else if (action._Object.GetType() == typeof(Mission) && (action._Object as Mission).MissionID == _EventData[i]._ID)
				{
					DoAction(_EventData[i]._Object, _EventData[i]._Action, _EventData[i]._Flag);
				}
			}
			else if (inObject.GetType() == typeof(Task))
			{
				if ((inObject as Task).TaskID == _EventData[i]._ID)
				{
					DoAction(_EventData[i]._Object, _EventData[i]._Action, _EventData[i]._Flag);
				}
			}
			else if (inObject.GetType() == typeof(Mission) && (inObject as Mission).MissionID == _EventData[i]._ID)
			{
				DoAction(_EventData[i]._Object, _EventData[i]._Action, _EventData[i]._Flag);
			}
		}
	}

	private void DoAction(GameObject inObject, Action inAction, bool inFlag)
	{
		if (inObject == null)
		{
			inObject = base.gameObject;
		}
		switch (inAction)
		{
		case Action.ACTIVE:
			inObject.SetActive(inFlag);
			break;
		case Action.VISIBLE:
			UtUtilities.SetObjectVisibility(inObject, inFlag);
			break;
		}
	}
}
