internal class AIBehavior_PetPickItem : AIBehavior
{
	public AIBehavior _Behavior_Goto;

	public AIBehavior _Behavior_Refuse;

	public AIBehavior _Behavior_PickObject;

	public AIEvaluator _Evaluator;

	public AIEvaluator _EvaluatorAngry;

	public AIBeacon _Beacon;

	private AIActor_Pet mActorPet;

	private int mCurrentMode;

	private ObCollectDragon mCollectable;

	public override AIBehaviorState Think(AIActor Actor)
	{
		switch (mCurrentMode)
		{
		case 0:
		{
			if (mCollectable == null || mCollectable.gameObject == null)
			{
				mCurrentMode = 2;
				_Behavior_PickObject.SetState(AIBehaviorState.COMPLETED);
				return SetState(AIBehaviorState.COMPLETED);
			}
			AIBehaviorState aIBehaviorState = _Behavior_Goto.Think(Actor);
			if (aIBehaviorState == AIBehaviorState.COMPLETED)
			{
				mCurrentMode = 1;
				_Behavior_Goto.OnTerminate(Actor);
				if (IsPetHappy())
				{
					mCurrentMode = 2;
					_Behavior_PickObject.OnStart(Actor);
				}
				else
				{
					mCurrentMode = 1;
					_Behavior_Refuse.OnStart(Actor);
				}
				return SetState(AIBehaviorState.ACTIVE);
			}
			return SetState(aIBehaviorState);
		}
		case 1:
			_Behavior_Refuse.Think(Actor);
			if (_Behavior_Refuse.State == AIBehaviorState.COMPLETED || _Behavior_Refuse.State == AIBehaviorState.INACTIVE)
			{
				_Evaluator._Priority = 0f;
				mCurrentMode = 4;
			}
			return SetState(AIBehaviorState.ACTIVE);
		case 2:
			if (_Behavior_PickObject.State != AIBehaviorState.COMPLETED)
			{
				_Behavior_PickObject.Think(Actor);
			}
			else
			{
				_Evaluator._Priority = 0f;
			}
			return SetState(AIBehaviorState.ACTIVE);
		case 4:
			if (IsPetHappy())
			{
				_Behavior_Refuse.OnTerminate(Actor);
				mCurrentMode = 2;
				_Evaluator._Priority = 1f;
				_Behavior_PickObject.OnStart(Actor);
			}
			break;
		}
		return SetState(AIBehaviorState.COMPLETED);
	}

	public void Update()
	{
		if (mCurrentMode == 4 && _Evaluator != null)
		{
			_Evaluator._Priority = ((IsPetHappy() || _Behavior_Refuse.State != AIBehaviorState.COMPLETED) ? 1 : 0);
		}
	}

	public bool IsPetHappy()
	{
		if (mActorPet == null || mActorPet.SanctuaryPet == null)
		{
			return false;
		}
		_EvaluatorAngry._Priority = 1f;
		if (mActorPet.SanctuaryPet.HasMood(Character_Mood.angry))
		{
			return false;
		}
		if (mCollectable == null)
		{
			return false;
		}
		if (mActorPet.SanctuaryPet.GetMeterValue(SanctuaryPetMeterType.HAPPINESS) < mCollectable._HappinessCost)
		{
			return false;
		}
		if (mActorPet.SanctuaryPet.GetMeterValue(SanctuaryPetMeterType.ENERGY) < mCollectable._EnergyCost)
		{
			_EvaluatorAngry._Priority = 0f;
			return false;
		}
		return true;
	}

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		if (mActorPet == null)
		{
			mActorPet = Actor as AIActor_Pet;
		}
		_Evaluator._Priority = 1f;
		if (_Beacon != null && _Beacon.transform.parent != null)
		{
			mCollectable = _Beacon.transform.parent.GetComponent<ObCollectDragon>();
			if (_Beacon.transform.root == Actor.transform.root)
			{
				mCurrentMode = 4;
				_Behavior_Goto.OnTerminate(Actor);
				return;
			}
		}
		mCurrentMode = 0;
		_Behavior_Goto.OnStart(Actor);
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		switch (mCurrentMode)
		{
		case 0:
			_Behavior_Goto.OnTerminate(Actor);
			break;
		case 1:
			_Behavior_Refuse.OnTerminate(Actor);
			break;
		case 2:
			_Behavior_PickObject.OnTerminate(Actor);
			break;
		}
	}
}
