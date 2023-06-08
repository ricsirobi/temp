using UnityEngine.AI;

public class AIEvaluator_NavPathAvailable : AIEvaluator
{
	public float _StoppingDistance = 3f;

	private NavMeshAgent mAgent;

	public override float OnGetDesirability(AIActor Actor, bool IsActive, AIBehavior Behavior)
	{
		mAgent = Actor.GetComponent<NavMeshAgent>();
		if (mAgent == null)
		{
			mAgent = Actor.gameObject.AddComponent<NavMeshAgent>();
		}
		if (mAgent != null)
		{
			mAgent.acceleration = 100f;
			mAgent.autoTraverseOffMeshLink = false;
			mAgent.angularSpeed = 300f;
			mAgent.stoppingDistance = _StoppingDistance;
			mAgent.speed = 0f;
			mAgent.destination = ((AIActor_Pet)Actor).SanctuaryPet.mAvatar.position;
			if (mAgent.hasPath && mAgent.remainingDistance > mAgent.stoppingDistance)
			{
				return _Priority;
			}
		}
		return 0f;
	}
}
