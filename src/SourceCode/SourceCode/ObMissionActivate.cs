using System.Collections.Generic;
using UnityEngine;

public class ObMissionActivate : MonoBehaviour
{
	public ObMissionActivateData[] _Data;

	public bool _Activate;

	public GameObject _Object;

	public List<string> _ComponentNames;

	public string _AnimationName;

	public GameObject _SplineRoot;

	private bool mModified;

	private void Awake()
	{
		UtDebug.LogWarning(base.gameObject.name + ": ObMissionActivate is deprecated.  Please replace with the new ObMissionActivate.");
	}

	private void Start()
	{
		if (!(MissionManager.pInstance != null))
		{
			return;
		}
		for (int i = 0; i < _Data.Length; i++)
		{
			if (_Data[i]._StartTask > 0)
			{
				_Data[i].mStartTask = MissionManager.pInstance.GetTask(_Data[i]._StartTask);
			}
			if (_Data[i]._CompleteTask > 0)
			{
				_Data[i].mCompleteTask = MissionManager.pInstance.GetTask(_Data[i]._CompleteTask);
			}
			if (_Data[i]._EndTask > 0)
			{
				_Data[i].mEndTask = MissionManager.pInstance.GetTask(_Data[i]._EndTask);
			}
			if (_Data[i]._CompleteMission > 0)
			{
				_Data[i].mCompleteMission = MissionManager.pInstance.GetMission(_Data[i]._CompleteMission);
			}
		}
	}

	private void Update()
	{
		if (MissionManager.pInstance != null)
		{
			for (int i = 0; i < _Data.Length; i++)
			{
				bool flag = false;
				if (_Data[i].mStartTask != null && (_Data[i].mStartTask.pStarted || _Data[i].mStartTask.pCompleted))
				{
					flag = true;
				}
				else if (_Data[i].mCompleteTask != null && _Data[i].mCompleteTask.pCompleted)
				{
					flag = true;
				}
				if (flag && _Data[i].mEndTask != null && (_Data[i].mEndTask.pStarted || _Data[i].mEndTask.pCompleted))
				{
					flag = false;
				}
				if (_Data[i].mCompleteMission != null && _Data[i].mCompleteMission.pCompleted)
				{
					flag = true;
				}
				if (flag)
				{
					if (!mModified)
					{
						mModified = true;
						Activate(_Activate);
					}
					return;
				}
			}
		}
		if (mModified)
		{
			mModified = false;
			Activate(!_Activate);
		}
	}

	public virtual void Activate(bool activate)
	{
		if (_Object != null)
		{
			_Object.SetActive(activate);
		}
		else
		{
			UtUtilities.SetChildrenActive(base.gameObject, activate);
		}
		GameObject gameObject = ((_Object != null) ? _Object : base.gameObject);
		string inObjectName = gameObject.name;
		if (activate && MissionManager.pInstance != null)
		{
			MissionManager.pInstance.ObjectActivated(inObjectName);
		}
		if (_ComponentNames != null)
		{
			foreach (string componentName in _ComponentNames)
			{
				Behaviour behaviour = gameObject.GetComponent(componentName) as Behaviour;
				if (behaviour != null)
				{
					behaviour.enabled = activate;
				}
			}
		}
		if (!string.IsNullOrEmpty(_AnimationName))
		{
			Animation componentInChildren = gameObject.GetComponentInChildren<Animation>();
			if (componentInChildren != null)
			{
				componentInChildren.CrossFade(_AnimationName);
			}
		}
		if (activate)
		{
			if (_SplineRoot != null)
			{
				gameObject.SendMessage("SetSplineRoot", _SplineRoot, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				gameObject.SendMessage("SetEmptySplineRoot", SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
