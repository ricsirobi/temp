using UnityEngine;
using UnityEngine.AI;

public class ObLeadingParticle : MonoBehaviour
{
	public enum MoveState
	{
		Moving,
		Moving_Done
	}

	public float _HeightFromGround = 0.5f;

	[Tooltip("Once the target is reached how long until the particle is destroy.")]
	public float _SelfDestructTime;

	private Transform mTarget;

	private MoveState mState;

	private ParticleSystem[] mParticleSystems;

	private NavMeshAgent mNavMeshAgent;

	private float mTimer;

	public void Update()
	{
		if (mTarget == null)
		{
			SetState(MoveState.Moving_Done);
		}
		switch (mState)
		{
		case MoveState.Moving:
		{
			Vector3 vector = base.transform.position - mTarget.position;
			vector.y = 0f;
			if (vector.magnitude <= mNavMeshAgent.stoppingDistance)
			{
				SetState(MoveState.Moving_Done);
			}
			break;
		}
		case MoveState.Moving_Done:
			mTimer -= Time.deltaTime;
			if (mTimer <= 0f)
			{
				Object.Destroy(base.gameObject);
			}
			break;
		}
	}

	private void LateUpdate()
	{
		if (mState == MoveState.Moving)
		{
			Vector3 zero = Vector3.zero;
			zero = base.transform.position;
			zero.y += _HeightFromGround;
			base.transform.position = zero;
		}
	}

	private void SetState(MoveState inState)
	{
		mState = inState;
		switch (inState)
		{
		case MoveState.Moving:
		{
			for (int j = 0; j < mParticleSystems.Length; j++)
			{
				mParticleSystems[j].Play();
			}
			if (!GenerateMoveToPath())
			{
				SetState(MoveState.Moving_Done);
			}
			break;
		}
		case MoveState.Moving_Done:
		{
			mTimer = _SelfDestructTime;
			for (int i = 0; i < mParticleSystems.Length; i++)
			{
				mParticleSystems[i].Stop();
			}
			break;
		}
		}
	}

	public bool GenerateMoveToPath()
	{
		mNavMeshAgent.destination = mTarget.position;
		NavMeshPath navMeshPath = new NavMeshPath();
		mNavMeshAgent.CalculatePath(mTarget.position, navMeshPath);
		return navMeshPath.status == NavMeshPathStatus.PathComplete;
	}

	public static void Initialize(Transform inStartObj, Transform inTarget, GameObject inParticle, float inParticleDist)
	{
		if (inStartObj == null || inTarget == null || inParticle == null)
		{
			return;
		}
		Vector3 a = inStartObj.position + inStartObj.forward * inParticleDist;
		Vector3 position = inTarget.position;
		a.y = 0f;
		position.y = 0f;
		if (!(Vector3.Distance(a, position) < 1f))
		{
			GameObject gameObject = Object.Instantiate(inParticle, inStartObj.position + inStartObj.forward * inParticleDist, Quaternion.identity);
			ObLeadingParticle component = gameObject.GetComponent<ObLeadingParticle>();
			component.mParticleSystems = component.GetComponentsInChildren<ParticleSystem>();
			component.mTarget = inTarget;
			if (component.mNavMeshAgent == null)
			{
				component.mNavMeshAgent = gameObject.GetComponent<NavMeshAgent>();
			}
			component.mNavMeshAgent.enabled = true;
			for (int i = 0; i < component.mParticleSystems.Length; i++)
			{
				component.mParticleSystems[i].Stop();
			}
			component.SetState(MoveState.Moving);
		}
	}
}
