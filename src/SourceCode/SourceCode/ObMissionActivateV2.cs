using System;
using System.Collections.Generic;
using UnityEngine;

public class ObMissionActivateV2 : MonoBehaviour
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
	public class Data
	{
		public Type _Type;

		public GameObject _Object;

		public bool _OnActivate;

		public string _Name;

		public GameObject _SplineRoot;
	}

	public ObMissionActivateData[] _Data;

	public bool _Activate;

	public List<Data> _ActivateData;

	private bool mModified;

	private void Awake()
	{
		UtDebug.LogWarning(base.gameObject.name + ": ObMissionActivateV2 is deprecated.  Please replace with the new ObMissionActivate.");
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
				if (_Data[i].mStartTask != null)
				{
					if (_Data[i].mStartTask.pCompleted)
					{
						flag = true;
					}
					else if (_Data[i].mStartTask.pStarted)
					{
						if (_Data[i]._WaitForOffer)
						{
							List<MissionAction> offers = _Data[i].mStartTask.GetOffers(unplayed: true);
							if (offers == null || offers.Count == 0)
							{
								_Data[i]._WaitForOffer = false;
								flag = true;
							}
						}
						else if (_Data[i]._WaitForAvatar)
						{
							if (AvAvatar.pState == AvAvatarState.IDLE)
							{
								_Data[i]._WaitForAvatar = false;
								flag = true;
							}
						}
						else
						{
							flag = true;
						}
					}
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
		if (_ActivateData == null)
		{
			return;
		}
		for (int i = 0; i < _ActivateData.Count; i++)
		{
			GameObject gameObject = ((_ActivateData[i]._Object != null) ? _ActivateData[i]._Object : base.gameObject);
			switch (_ActivateData[i]._Type)
			{
			case Type.OBJECT:
				if (_ActivateData[i]._Object != null)
				{
					_ActivateData[i]._Object.SetActive(activate);
				}
				else
				{
					UtUtilities.SetChildrenActive(base.gameObject, activate);
				}
				if (activate && MissionManager.pInstance != null)
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
						behaviour.enabled = activate;
					}
				}
				break;
			case Type.ANIMATION:
			{
				if (string.IsNullOrEmpty(_ActivateData[i]._Name) || ((!activate || !_ActivateData[i]._OnActivate) && (activate || _ActivateData[i]._OnActivate)))
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
				if ((activate && _ActivateData[i]._OnActivate) || (!activate && !_ActivateData[i]._OnActivate))
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
						components[j].enabled = activate;
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
