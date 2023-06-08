using System;
using System.Collections.Generic;
using UnityEngine;

namespace KA.Framework;

public class ObMissionActivate : MonoBehaviour
{
	public enum Type
	{
		OBJECT,
		COMPONENT,
		ANIMATION,
		SPLINE,
		COLLIDER,
		ANIMATOR
	}

	[Serializable]
	public class TaskWait
	{
		[Tooltip("Wait for all the cutscenes to finish.")]
		public bool _Cutscene;

		[Tooltip("Wait for all the offers to be shown. (Valid only for STARTED)")]
		public bool _Offers;

		[Tooltip("Wait for the avatar to be in IDLE state.")]
		public bool _Avatar;
	}

	[Serializable]
	public class TaskWaitInfo : TaskInfo
	{
		[Tooltip("What must the objects wait for before being activated.")]
		public TaskWait _WaitFor;

		[NonSerialized]
		public Task mTask;
	}

	[Serializable]
	public class TaskData
	{
		public TaskWaitInfo _BeginTask;

		public TaskWaitInfo _EndTask;
	}

	[Serializable]
	public class Data
	{
		public bool _Activate;

		public Type _Type;

		public GameObject _Object;

		[Tooltip("Used by ANIMATION and SPLINE type to determine if the action is taken when Activate is true or false.")]
		public bool _OnActivate;

		public string _Name;

		public GameObject _SplineRoot;
	}

	[Tooltip("Used as INFORMATION ONLY to allow the Designer to input the Mission ID.")]
	public int _MissionID;

	[Tooltip("Used as INFORMATION ONLY to allow the Designer to input Mission Information.")]
	[TextArea]
	public string _MissionBreakdown;

	public List<TaskData> _Data;

	[Tooltip("Used as INFORMATION ONLY to allow the Designer to input Task Information.")]
	[TextArea]
	public string _TaskBreakdown;

	public List<Data> _ActivateData;

	private bool mModified;

	private void Start()
	{
		if (!(MissionManager.pInstance != null))
		{
			return;
		}
		for (int i = 0; i < _Data.Count; i++)
		{
			if (_Data[i]._BeginTask._TaskID > 0)
			{
				Task task = MissionManager.pInstance.GetTask(_Data[i]._BeginTask._TaskID);
				if (task != null)
				{
					Mission rootMission = MissionManager.pInstance.GetRootMission(task);
					if (MissionManager.pInstance.IsLocked(rootMission))
					{
						task = null;
					}
				}
				_Data[i]._BeginTask.mTask = task;
			}
			if (_Data[i]._EndTask._TaskID <= 0)
			{
				continue;
			}
			Task task2 = MissionManager.pInstance.GetTask(_Data[i]._EndTask._TaskID);
			if (task2 != null)
			{
				Mission rootMission2 = MissionManager.pInstance.GetRootMission(task2);
				if (MissionManager.pInstance.IsLocked(rootMission2))
				{
					task2 = null;
				}
			}
			_Data[i]._EndTask.mTask = task2;
		}
	}

	private void Update()
	{
		if (MissionManager.pInstance != null && _Data != null)
		{
			for (int i = 0; i < _Data.Count; i++)
			{
				bool flag = false;
				TaskWaitInfo beginTask = _Data[i]._BeginTask;
				if (beginTask.mTask != null)
				{
					if (beginTask._TaskStatus == TaskState.STARTED && (beginTask.mTask.pCompleted || (beginTask.mTask.pStarted && !CheckStartedWaitFor(beginTask, beginTask.mTask))))
					{
						flag = true;
					}
					else if (beginTask._TaskStatus == TaskState.COMPLETED && beginTask.mTask.pCompleted && !CheckCompletedWaitFor(beginTask, beginTask.mTask))
					{
						flag = true;
					}
				}
				TaskWaitInfo endTask = _Data[i]._EndTask;
				if (flag && endTask.mTask != null)
				{
					if (endTask._TaskStatus == TaskState.STARTED && (endTask.mTask.pCompleted || (endTask.mTask.pStarted && !CheckStartedWaitFor(endTask, endTask.mTask))))
					{
						flag = false;
					}
					else if (endTask._TaskStatus == TaskState.COMPLETED && endTask.mTask.pCompleted && !CheckCompletedWaitFor(endTask, endTask.mTask))
					{
						flag = false;
					}
				}
				if (!flag)
				{
					continue;
				}
				if (!mModified)
				{
					mModified = true;
					Activate(globalActivate: true);
					if (endTask._TaskID == 0)
					{
						base.enabled = false;
					}
				}
				return;
			}
		}
		if (mModified)
		{
			mModified = false;
			Activate(globalActivate: false);
		}
	}

	private bool CheckStartedWaitFor(TaskWaitInfo taskWaitInfo, Task task)
	{
		if (taskWaitInfo._WaitFor._Cutscene)
		{
			List<MissionAction> offers = task.GetOffers(unplayed: false);
			if (offers != null && offers.Count > 0)
			{
				List<MissionAction> list = offers.FindAll((MissionAction offer) => offer.Type == MissionActionType.CutScene);
				if (list.Count > 0)
				{
					for (int i = 0; i < list.Count; i++)
					{
						string[] array = list[i].Asset.Split('/');
						if (MissionManager.pInstance.IsCutScenePlaying(array[^1]))
						{
							return true;
						}
					}
				}
			}
			taskWaitInfo._WaitFor._Cutscene = false;
			return false;
		}
		if (taskWaitInfo._WaitFor._Offers)
		{
			List<MissionAction> offers2 = task.GetOffers(unplayed: true);
			if (offers2 == null || offers2.Count == 0)
			{
				taskWaitInfo._WaitFor._Offers = false;
				return false;
			}
		}
		else
		{
			if (!taskWaitInfo._WaitFor._Avatar)
			{
				return false;
			}
			if (AvAvatar.pState == AvAvatarState.IDLE)
			{
				taskWaitInfo._WaitFor._Avatar = false;
				return false;
			}
		}
		return true;
	}

	private bool CheckCompletedWaitFor(TaskWaitInfo taskWaitInfo, Task task)
	{
		if (taskWaitInfo._WaitFor._Cutscene)
		{
			List<MissionAction> ends = task.GetEnds();
			if (ends != null && ends.Count > 0)
			{
				List<MissionAction> list = ends.FindAll((MissionAction end) => end.Type == MissionActionType.CutScene);
				if (list.Count > 0)
				{
					for (int i = 0; i < list.Count; i++)
					{
						string[] array = list[i].Asset.Split('/');
						if (MissionManager.pInstance.IsCutScenePlaying(array[^1]))
						{
							return true;
						}
					}
				}
			}
			taskWaitInfo._WaitFor._Cutscene = false;
			return false;
		}
		if (taskWaitInfo._WaitFor._Avatar)
		{
			if (AvAvatar.pState == AvAvatarState.IDLE)
			{
				taskWaitInfo._WaitFor._Avatar = false;
				return false;
			}
			return true;
		}
		return false;
	}

	public virtual void Activate(bool globalActivate)
	{
		if (_ActivateData == null)
		{
			return;
		}
		for (int i = 0; i < _ActivateData.Count; i++)
		{
			bool flag = (globalActivate ? _ActivateData[i]._Activate : (!_ActivateData[i]._Activate));
			GameObject gameObject = ((_ActivateData[i]._Object != null) ? _ActivateData[i]._Object : base.gameObject);
			switch (_ActivateData[i]._Type)
			{
			case Type.OBJECT:
				if (_ActivateData[i]._Object != null)
				{
					_ActivateData[i]._Object.SetActive(flag);
				}
				if (flag && MissionManager.pInstance != null)
				{
					string inObjectName = gameObject.name;
					MissionManager.pInstance.ObjectActivated(inObjectName);
				}
				break;
			case Type.COMPONENT:
				if (!string.IsNullOrEmpty(_ActivateData[i]._Name))
				{
					Behaviour behaviour = gameObject.GetComponent(_ActivateData[i]._Name) as Behaviour;
					if (behaviour != null)
					{
						behaviour.enabled = flag;
					}
				}
				break;
			case Type.ANIMATION:
			{
				if (string.IsNullOrEmpty(_ActivateData[i]._Name) || ((!flag || !_ActivateData[i]._OnActivate) && (flag || _ActivateData[i]._OnActivate)))
				{
					break;
				}
				Animation componentInChildren = gameObject.GetComponentInChildren<Animation>();
				if (componentInChildren != null)
				{
					componentInChildren.CrossFade(_ActivateData[i]._Name);
					break;
				}
				Animator componentInChildren2 = gameObject.GetComponentInChildren<Animator>();
				if (componentInChildren2 != null)
				{
					componentInChildren2.Play(_ActivateData[i]._Name);
				}
				break;
			}
			case Type.SPLINE:
				if ((flag && _ActivateData[i]._OnActivate) || (!flag && !_ActivateData[i]._OnActivate))
				{
					if (_ActivateData[i]._SplineRoot != null)
					{
						gameObject.SendMessage("SetSplineRoot", _ActivateData[i]._SplineRoot, SendMessageOptions.DontRequireReceiver);
					}
					else
					{
						gameObject.SendMessage("SetEmptySplineRoot", SendMessageOptions.DontRequireReceiver);
					}
				}
				break;
			case Type.COLLIDER:
			{
				Collider[] components = gameObject.GetComponents<Collider>();
				if (components != null)
				{
					for (int j = 0; j < components.Length; j++)
					{
						components[j].enabled = flag;
					}
				}
				break;
			}
			case Type.ANIMATOR:
			{
				Animator component = gameObject.GetComponent<Animator>();
				if (component != null)
				{
					component.SetTrigger(_ActivateData[i]._Name);
				}
				break;
			}
			}
		}
	}
}
