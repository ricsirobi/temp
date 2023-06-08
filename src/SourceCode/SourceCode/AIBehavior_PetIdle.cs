using UnityEngine;

public class AIBehavior_PetIdle : AIBehavior_Idle
{
	public bool _AllowGoToSleep = true;

	public bool _AllowPetFallToGround;

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (_AllowPetFallToGround)
		{
			((AIActor_Pet)Actor).SanctuaryPet.FallToGround();
		}
		return base.Think(Actor);
	}

	public override void OnTerminate(AIActor Actor)
	{
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		if (aIActor_Pet.SanctuaryPet != null && SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.HandleZzzParticles(emit: false, aIActor_Pet.SanctuaryPet);
		}
		base.OnTerminate(Actor);
	}

	public override void GotoNextIdleState(AIActor Actor)
	{
		AIActor_Pet aIActor_Pet = Actor as AIActor_Pet;
		int num = mIdleState;
		mIdleState = Mathf.Min(2, mIdleState + 1);
		if (mIdleState >= 2 && (aIActor_Pet.SanctuaryPet != SanctuaryManager.pCurPetInstance || !_AllowGoToSleep))
		{
			mIdleState = 1;
		}
		mNumAnimsRemaining = Random.Range(4, 7);
		if (num == 2 && num != mIdleState && SanctuaryManager.pInstance != null)
		{
			SanctuaryManager.pInstance.HandleZzzParticles(emit: false, aIActor_Pet.SanctuaryPet);
		}
		if (num < mIdleState)
		{
			switch (mIdleState)
			{
			case 0:
			case 1:
				PlayNextAnimation(Actor);
				break;
			case 2:
				PlayNextAnimation(Actor);
				if (SanctuaryManager.pInstance != null)
				{
					SanctuaryManager.pInstance.HandleZzzParticles(emit: true, aIActor_Pet.SanctuaryPet);
				}
				break;
			}
		}
		if (_LookAtToDisable != null && mIdleState >= 2)
		{
			_LookAtToDisable.enabled = false;
		}
	}
}
