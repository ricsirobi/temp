using System;
using System.Collections.Generic;
using SWS;
using UnityEngine;

public class AIActor_NPC : AIActor
{
	[Serializable]
	public class SearchZoneData
	{
		[Serializable]
		public class PatrolInfo
		{
			public SWS.PathManager _Path;

			public string _PatrolAnimation;

			public string _IdleAnimation;
		}

		[Serializable]
		public class SpotlightInfo
		{
			public Light _Spotlight;

			public float _SpotlightFadeRate = 30f;
		}

		public ZoneData _OuterZone;

		public ZoneData _InnerZone;

		public MinMax _HeightRange;

		public float _TriggerDistance = 25f;

		public SpotlightInfo _SpotlightInfo;

		public float _AvatarRespawnDelay = 1f;

		public Transform _AvatarRespawnMarker;

		public PatrolInfo _PatrolInfo;

		public bool _UseRotationOnRespawnAvatar;
	}

	[Serializable]
	public class ZoneData
	{
		public enum MeterEventType
		{
			Empty,
			Fill,
			Full,
			Drop
		}

		[Serializable]
		public class EventData
		{
			public MeterEventType _MeterEventType;

			public List<EffectData> _EffectData;
		}

		public float _Length = 8f;

		public float _Angle = 78f;

		public int _CurveSegments = 8;

		public int _ZoneYPolygonLevel = 2;

		public Vector3 _Offset;

		public GameObject _MeterObject;

		public float _MeterFillRate = 1f;

		public float _MeterDepletionRate = 1f;

		public List<EventData> _EventData;
	}

	[Serializable]
	public class StateData
	{
		public string _Name;

		public NPC_FSM _State;
	}

	public string _MainRootName = "Main_Root";

	public NPC_FSM _State;

	public ObProximityAnimate _ObProximityAnimate;

	public StateData[] _StateData;

	public bool _ControlledSwitchOn;

	public AIActor_NPC[] _ControllingObjects;

	[Header("Search FSM State specific data")]
	public SearchZoneData _SearchZoneData;

	protected Dictionary<NPC_FSM, int> mStateMap;

	protected Action mAction;

	protected Task mTask;

	protected NPC_FSM mControlledState;

	public Transform MainRoot { get; set; }

	public NPC_FSM pState { get; set; }

	public void Awake()
	{
		pState = _State;
		MainRoot = base.transform.Find(_MainRootName);
		NPCAvatar component = GetComponent<NPCAvatar>();
		if (component != null)
		{
			component.pAIActor = this;
		}
		else
		{
			Debug.LogError("Error : Pet is null");
		}
	}

	protected override void UpdateStateMap()
	{
		if (mStateMap == null)
		{
			mStateMap = new Dictionary<NPC_FSM, int>();
		}
		if (mStateMap.Count != 0 || _StateData.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _StateData.Length; i++)
		{
			int num = mFSMCharacter.FindStateIndex(_StateData[i]._Name);
			if (num != -1)
			{
				mStateMap.Add(_StateData[i]._State, num);
			}
		}
		SetState(pState);
	}

	public virtual void SetState(NPC_FSM newState)
	{
		if (mStateMap != null && mStateMap.ContainsKey(newState))
		{
			pState = newState;
			if (_ObProximityAnimate != null)
			{
				_ObProximityAnimate.enabled = ((newState == NPC_FSM.NORMAL) ? true : false);
			}
			SetState(mStateMap[newState]);
		}
	}

	protected NPC_FSM GetState(string name)
	{
		StateData[] stateData = _StateData;
		foreach (StateData stateData2 in stateData)
		{
			if (stateData2._Name.Equals(name))
			{
				return stateData2._State;
			}
		}
		return NPC_FSM.NORMAL;
	}

	protected virtual void SetupForTask(Task inTask)
	{
		if (inTask == null || inTask.pData == null || inTask.pData.Type == "Meet" || inTask.pData.Type == "Delivery")
		{
			return;
		}
		mTask = inTask;
		string objectiveValue = mTask.GetObjectiveValue<string>("NPC");
		if (_BehaviorsRoot.transform.childCount > 0 && mTask._Active && base.gameObject.name.Equals(objectiveValue))
		{
			_BehaviorsRoot.BroadcastMessage("SetData", mTask, SendMessageOptions.DontRequireReceiver);
			NPC_FSM state = GetState(mTask.pData.Type);
			SetState(state);
			Transform transform = UtUtilities.FindChildTransform(base.gameObject, "RewardIcon", inactive: true);
			if (transform != null)
			{
				transform.gameObject.SetActive(value: false);
			}
			SetupControllingObjects(this, state);
			mAction = null;
		}
		else
		{
			if (mAction != null)
			{
				return;
			}
			if (!mTask._Active)
			{
				Transform transform2 = UtUtilities.FindChildTransform(base.gameObject, "RewardIcon", inactive: true);
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(value: true);
				}
				transform2 = UtUtilities.FindChildTransform(base.gameObject, "QuestIcon", inactive: true);
				if (transform2 != null)
				{
					transform2.gameObject.SetActive(value: false);
				}
			}
			mAction = (Action)Delegate.Combine(mAction, (Action)delegate
			{
				SetupForTask(mTask);
			});
		}
	}

	private void SetupControllingObjects(AIActor_NPC actor, NPC_FSM state)
	{
		if (actor._ControllingObjects == null || actor._ControllingObjects.Length == 0)
		{
			return;
		}
		AIActor_NPC[] controllingObjects = actor._ControllingObjects;
		foreach (AIActor_NPC aIActor_NPC in controllingObjects)
		{
			if (!(aIActor_NPC == null))
			{
				aIActor_NPC.mController = actor;
				aIActor_NPC._BehaviorsRoot.BroadcastMessage("SetData", mTask, SendMessageOptions.DontRequireReceiver);
				if (!aIActor_NPC._ControlledSwitchOn)
				{
					aIActor_NPC.SetState((state == NPC_FSM.FOLLOW && !aIActor_NPC.HasSplineMovement()) ? NPC_FSM.ESCORT : state);
				}
				else
				{
					aIActor_NPC.mControlledState = state;
				}
				SetupControllingObjects(aIActor_NPC, state);
			}
		}
	}

	public void OnStateChange(bool switchOn)
	{
		if (switchOn && _ControlledSwitchOn)
		{
			SetState((mControlledState == NPC_FSM.FOLLOW && !HasSplineMovement()) ? NPC_FSM.ESCORT : mControlledState);
		}
	}

	public override void Update()
	{
		base.Update();
		if (mAction != null && _BehaviorsRoot != null && _BehaviorsRoot.transform.childCount > 0)
		{
			mAction();
		}
	}

	public override void DoAction(string action, params object[] values)
	{
		if (action == "MissionComplete")
		{
			SetState(NPC_FSM.NORMAL);
		}
		base.DoAction(action, values);
	}

	public override bool IsFlying()
	{
		if (base.pMoveState != null)
		{
			return base.pMoveState._State == MoveStates.Air;
		}
		return false;
	}
}
