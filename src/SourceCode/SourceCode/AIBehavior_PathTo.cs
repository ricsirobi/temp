using UnityEngine;
using UnityEngine.AI;

public class AIBehavior_PathTo : AIBehavior
{
	public AITarget _Target = new AITarget();

	public float _DisablePathFindingIfFartherThan = 30f;

	public float _ArriveWhenCloserThan = 0.05f;

	public string _BoneName;

	public string _IdleStandAnim = "IdleStand";

	public float _WalkSpeed = 3f;

	public float _RunSpeed = 5f;

	public float _WalkWhenCloserThan = 7f;

	public float _RunWhenFartherThan = 10f;

	public bool _WalkWithAvatarSpeed;

	public string _WalkAnim = "Walk";

	public string _RunAnim = "Run";

	public bool _DisableNavAgentOnTerminate = true;

	public string[] _NavMeshMaskAreas;

	protected NavMeshAgent mAgent;

	protected Transform mBone;

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		if (mAgent == null)
		{
			mAgent = Actor.GetComponent<NavMeshAgent>();
		}
		if (mAgent != null)
		{
			mAgent.enabled = false;
		}
		if (NavMesh.SamplePosition(Actor.Position, out var hit, 0f, 1))
		{
			Actor.transform.position = hit.position;
		}
		else if (NavMesh.SamplePosition(Actor.Position, out hit, 3f, 1))
		{
			Actor.transform.position = hit.position;
		}
		else if (NavMesh.SamplePosition(Actor.Position, out hit, 5200f, 1))
		{
			Actor.transform.position = hit.position;
		}
		if (mAgent != null)
		{
			mAgent.enabled = true;
		}
		if (mAgent == null)
		{
			mAgent = Actor.gameObject.AddComponent<NavMeshAgent>();
			mAgent.acceleration = 1000f;
			mAgent.autoTraverseOffMeshLink = false;
			mAgent.angularSpeed = 300f;
		}
		_Target.Actor = Actor;
		if (!string.IsNullOrEmpty(_BoneName))
		{
			mBone = UtUtilities.FindChildTransform(Actor.gameObject, _BoneName);
		}
		if (!(Actor.animation == null) && !(Actor.animation[_IdleStandAnim] == null))
		{
			Actor.animation.CrossFade(_IdleStandAnim, 0.2f, PlayMode.StopAll);
			Actor.animation[_IdleStandAnim].wrapMode = WrapMode.Loop;
			mAgent.enabled = true;
			if (mAgent.isOnNavMesh)
			{
				mAgent.destination = _Target.GetLocation();
				mAgent.isStopped = false;
			}
		}
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mAgent == null || mAgent.pathStatus == NavMeshPathStatus.PathInvalid || (mAgent.pathStatus == NavMeshPathStatus.PathPartial && !mAgent.pathPending))
		{
			return SetState(AIBehaviorState.FAILED);
		}
		Vector3 location = _Target.GetLocation();
		if (Vector3.Distance(location, Actor.Position) > _DisablePathFindingIfFartherThan)
		{
			mAgent.enabled = false;
			return SetState(AIBehaviorState.FAILED);
		}
		SetSpeed(GetSpeed(Actor));
		SetDestination(location);
		if (HasArrived(Actor))
		{
			return SetState(AIBehaviorState.COMPLETED);
		}
		if (mAgent.isOnOffMeshLink)
		{
			Vector3 position = Vector3.MoveTowards(mAgent.transform.position, mAgent.currentOffMeshLinkData.endPos, mAgent.speed * Actor.DeltaTime);
			mAgent.transform.position = position;
			Vector3 forward = mAgent.transform.forward;
			forward.y = 0f;
			mAgent.transform.rotation = Quaternion.LookRotation(forward);
			if (Vector3.Distance(mAgent.transform.position, mAgent.currentOffMeshLinkData.endPos) < 0.1f)
			{
				mAgent.CompleteOffMeshLink();
			}
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	public bool HasArrived(AIActor Actor)
	{
		float num = _ArriveWhenCloserThan;
		if (mBone != null)
		{
			num += Vector3.Distance(Actor.Position, mBone.position);
		}
		if (mAgent.pathStatus == NavMeshPathStatus.PathComplete && mAgent.hasPath && mAgent.remainingDistance < num)
		{
			return true;
		}
		return false;
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		if (mAgent != null && _DisableNavAgentOnTerminate)
		{
			mAgent.enabled = false;
		}
	}

	public virtual float GetSpeed(AIActor Actor)
	{
		float num = _WalkSpeed;
		float runSpeed = _RunSpeed;
		if (_WalkWithAvatarSpeed)
		{
			AvAvatarController avAvatarController = null;
			if (Actor.GetAvatar() != null)
			{
				avAvatarController = Actor.GetAvatar().GetComponent<AvAvatarController>();
			}
			if (avAvatarController != null)
			{
				float magnitude = avAvatarController.pVelocity.magnitude;
				magnitude = Mathf.Max(magnitude, num / 3f);
				num = Mathf.Min(num, magnitude);
			}
		}
		if (Actor.animation == null)
		{
			return num;
		}
		float num2 = Vector3.Distance(_Target.GetLocation(), Actor.Position);
		float value = Mathf.Max(0f, num2 - _WalkWhenCloserThan) / Mathf.Max(0f, _RunWhenFartherThan - _WalkWhenCloserThan);
		value = Mathf.Clamp01(value);
		bool flag = Actor.animation.IsPlaying(_RunAnim);
		bool flag2 = Actor.animation.IsPlaying(_WalkAnim);
		if ((flag && value < 0.4f) || (!flag2 && value < 0.6f))
		{
			Actor.animation.CrossFade(_WalkAnim, 0.3f);
			AnimationState animationState = Actor.animation[_WalkAnim];
			if (animationState != null)
			{
				animationState.wrapMode = WrapMode.Loop;
			}
		}
		if ((flag2 && value > 0.6f) || (!flag && value > 0.4f))
		{
			Actor.animation.CrossFade(_RunAnim, 0.3f);
			if (Actor.animation[_RunAnim] != null)
			{
				Actor.animation[_RunAnim].wrapMode = WrapMode.Loop;
			}
		}
		value = ((!Actor.animation.IsPlaying(_WalkAnim)) ? 1 : 0);
		return Mathf.Lerp(num, runSpeed, value);
	}

	public void SetSpeed(float Speed)
	{
		if (mAgent != null)
		{
			mAgent.speed = Speed;
		}
	}

	public virtual void SetDestination(Vector3 destination)
	{
		if (mAgent.isOnNavMesh)
		{
			mAgent.destination = destination;
		}
	}
}
