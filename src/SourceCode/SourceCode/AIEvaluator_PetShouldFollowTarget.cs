using System;
using UnityEngine.AI;

public class AIEvaluator_PetShouldFollowTarget : AIEvaluator_FartherThan
{
	[Serializable]
	public enum FollowType
	{
		Navmesh,
		Distance
	}

	public string _FlyForwardAnim = "FlyForward";

	public string _FlyIdleAnim = "FlyIdle";

	public float _StoppingDistance = 3f;

	public FollowType _FollowType = FollowType.Distance;

	public bool _UsePetFollowDistance = true;

	private NavMeshAgent mAgent;

	private AIBehavior_PathTo mAIBehavior_PathTo;

	public override float OnGetDesirability(AIActor Actor, bool IsActive, AIBehavior Behavior)
	{
		switch (_FollowType)
		{
		case FollowType.Distance:
		{
			float disableWhen_NearThan = _DisableWhen_NearThan;
			float enableWhen_FartherThan = _EnableWhen_FartherThan;
			if (_UsePetFollowDistance)
			{
				if (Actor is AIActor_Pet aIActor_Pet2)
				{
					if (!Actor.animation.IsPlaying(_FlyForwardAnim) && !Actor.animation.IsPlaying(_FlyIdleAnim))
					{
						SanctuaryPet sanctuaryPet = aIActor_Pet2.SanctuaryPet;
						if ((bool)sanctuaryPet)
						{
							_EnableWhen_FartherThan += sanctuaryPet._FollowFrontDistance;
							_DisableWhen_NearThan += sanctuaryPet._FollowFrontDistance;
						}
					}
				}
				else
				{
					UtDebug.LogWarning("Actor could not be cast as AIActor_Pet for Actor " + Actor._Character._Name + ". Actor is a " + Actor.GetType().Name);
				}
			}
			float result = base.OnGetDesirability(Actor, IsActive, Behavior);
			if (_UsePetFollowDistance)
			{
				_EnableWhen_FartherThan = enableWhen_FartherThan;
				_DisableWhen_NearThan = disableWhen_NearThan;
			}
			return result;
		}
		case FollowType.Navmesh:
		{
			mAgent = Actor.GetComponent<NavMeshAgent>();
			if (!mAgent)
			{
				mAgent = Actor.gameObject.AddComponent<NavMeshAgent>();
			}
			if (mAIBehavior_PathTo == null)
			{
				mAIBehavior_PathTo = GetComponentInChildren<AIBehavior_PathTo>();
			}
			if (!mAgent)
			{
				break;
			}
			int num = -1;
			if (mAIBehavior_PathTo != null)
			{
				num = 1;
				string[] navMeshMaskAreas = mAIBehavior_PathTo._NavMeshMaskAreas;
				foreach (string text in navMeshMaskAreas)
				{
					if (text != null)
					{
						int areaFromName = NavMesh.GetAreaFromName(text);
						if (areaFromName >= 0)
						{
							num |= 1 << areaFromName;
						}
					}
				}
			}
			mAgent.acceleration = 100f;
			mAgent.autoTraverseOffMeshLink = false;
			mAgent.angularSpeed = 300f;
			mAgent.stoppingDistance = _StoppingDistance;
			mAgent.speed = 0f;
			mAgent.areaMask = num;
			if (Actor is AIActor_Pet aIActor_Pet)
			{
				mAgent.destination = aIActor_Pet.SanctuaryPet.mAvatar.position;
			}
			if (mAgent.hasPath && mAgent.remainingDistance > mAgent.stoppingDistance)
			{
				return _Priority;
			}
			break;
		}
		}
		return 0f;
	}
}
