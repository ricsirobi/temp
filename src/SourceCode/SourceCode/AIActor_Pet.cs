using System.Collections.Generic;
using DG.Tweening;
using SWS;
using UnityEngine;

public class AIActor_Pet : AIActor
{
	public AISanctuaryPetFSM _State;

	[HideInInspector]
	public SanctuaryPet SanctuaryPet;

	private Dictionary<AISanctuaryPetFSM, int> mStateMap;

	private AIBehavior mCustomGoal;

	private AIBehavior_Arbiter mPetPlayGoalHolder;

	private AIEvaluator mPetPlayGoal;

	private Task mTask;

	public void SetState(AISanctuaryPetFSM newState)
	{
		_State = newState;
		UpdateCharacterFSM();
		if (mStateMap != null && mStateMap.ContainsKey(newState))
		{
			SetState(mStateMap[newState]);
		}
		else
		{
			SetState(-1);
		}
	}

	public void Awake()
	{
		_State = AISanctuaryPetFSM.CUSTOM;
		SanctuaryPet = GetComponent<SanctuaryPet>();
		if (SanctuaryPet != null)
		{
			SanctuaryPet.AIActor = this;
		}
		else
		{
			Debug.LogError("Error : Pet is null");
		}
	}

	public override void Update()
	{
		if (!(SanctuaryPet == null) && SanctuaryPet.enabled)
		{
			base.Update();
		}
	}

	protected void SetupForTask(Task task)
	{
		if (task == null || task.pData == null)
		{
			return;
		}
		mTask = task;
		if (mTask._Active)
		{
			if (GetComponent<splineMove>() == null)
			{
				base.gameObject.AddComponent<splineMove>().lockRotation = AxisConstraint.X;
			}
			_BehaviorsRoot.BroadcastMessage("SetData", mTask, SendMessageOptions.DontRequireReceiver);
			SetState(AISanctuaryPetFSM.SPLINEFOLLOW);
		}
	}

	protected override void UpdateStateMap()
	{
		if (mStateMap == null)
		{
			mStateMap = new Dictionary<AISanctuaryPetFSM, int>();
		}
		if (mStateMap.Count == 0)
		{
			mStateMap.Add(AISanctuaryPetFSM.NORMAL, mFSMCharacter.FindStateIndex("Normal"));
			mStateMap.Add(AISanctuaryPetFSM.PET_PLAY, mFSMCharacter.FindStateIndex("PetPlay"));
			mStateMap.Add(AISanctuaryPetFSM.MOUNTED, mFSMCharacter.FindStateIndex("Mounted"));
			mStateMap.Add(AISanctuaryPetFSM.SCIENCE_LAB, mFSMCharacter.FindStateIndex("ScienceLab"));
			mStateMap.Add(AISanctuaryPetFSM.REST, mFSMCharacter.FindStateIndex("Rest"));
			mStateMap.Add(AISanctuaryPetFSM.SWIMMING, mFSMCharacter.FindStateIndex("Swim"));
			mStateMap.Add(AISanctuaryPetFSM.NAVMESHFOLLOW, mFSMCharacter.FindStateIndex("NavMeshFollow"));
			mStateMap.Add(AISanctuaryPetFSM.SPLINEFOLLOW, mFSMCharacter.FindStateIndex("SplineFollow"));
			SetState(_State);
		}
	}

	public void PushPetPlayGoal(AIEvaluator Evaluator, bool MakeACopy = false)
	{
		if (UpdatePetPlayGoalHolder())
		{
			if (mPetPlayGoal != null)
			{
				mPetPlayGoalHolder.RemoveEvaluator(mPetPlayGoal, this);
			}
			mPetPlayGoal = Evaluator;
			mPetPlayGoal.CheckForChildEvaluators();
			mPetPlayGoalHolder.PushEvaluator(mPetPlayGoal);
		}
	}

	public void RemovePetPlayGoal(AIEvaluator Evaluator)
	{
		if (UpdatePetPlayGoalHolder())
		{
			if (mPetPlayGoal != null && mPetPlayGoal == Evaluator)
			{
				mPetPlayGoalHolder.RemoveEvaluator(mPetPlayGoal, this);
			}
			mPetPlayGoal = null;
		}
	}

	public bool UpdatePetPlayGoalHolder()
	{
		if (mPetPlayGoalHolder != null)
		{
			return true;
		}
		Transform transform = _BehaviorsRoot.transform.Find("AI_Root_Pet(Clone)/FSM_PetState/PetPlay/Actions");
		if (transform == null)
		{
			return false;
		}
		mPetPlayGoalHolder = transform.GetComponent<AIBehavior_Arbiter>();
		return mPetPlayGoalHolder != null;
	}

	public override Transform GetAvatar()
	{
		if (SanctuaryPet != null)
		{
			return SanctuaryPet.mAvatar;
		}
		return null;
	}

	public override Transform GetAvatarTarget()
	{
		if (SanctuaryPet != null)
		{
			SanctuaryPet.UpdateAvatarTargetPosition();
			return SanctuaryPet.mAvatarTarget;
		}
		return null;
	}

	public override Camera GetCamera()
	{
		if (SanctuaryPet != null)
		{
			return SanctuaryPet.GetCamera();
		}
		return base.GetCamera();
	}

	public override void TeleportTo(Vector3 position, Quaternion rotation, bool spawnTeleportEffect = true)
	{
		if (SanctuaryPet != null)
		{
			SanctuaryPet.TeleportTo(position, rotation, spawnTeleportEffect);
		}
	}

	public override void FallToGround()
	{
		if (SanctuaryPet != null)
		{
			SanctuaryPet.FallToGround();
		}
		else
		{
			base.FallToGround();
		}
	}

	public override void DoAction(string action, params object[] values)
	{
		if (action == "MissionComplete")
		{
			splineMove component = GetComponent<splineMove>();
			if (component != null)
			{
				Object.Destroy(component);
			}
			SetState(AISanctuaryPetFSM.NORMAL);
		}
		base.DoAction(action, values);
	}
}
