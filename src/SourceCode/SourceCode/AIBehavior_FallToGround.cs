using UnityEngine;

public class AIBehavior_FallToGround : AIBehavior
{
	public float _GroundCheckStartHeight = 2f;

	public float _GroundCheckDist = 20f;

	public float _WaterSinkHeight = 0.35f;

	public float _HeightTweenSpeed = 1.5f;

	protected float mPrevHeight;

	protected Transform mMoveToTransform;

	protected int mWaterLayer = -1;

	private AIActor_NPCCarried mAIActorNPCCarried;

	public override void OnStart(AIActor Actor)
	{
		mWaterLayer = LayerMask.NameToLayer("Water");
		mMoveToTransform = Actor.GetAvatar();
		mAIActorNPCCarried = Actor as AIActor_NPCCarried;
		base.OnStart(Actor);
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (!Actor.IsFlying() && mAIActorNPCCarried == null)
		{
			FallToGround(Actor.transform);
		}
		return AIBehaviorState.ACTIVE;
	}

	public override void OnTerminate(AIActor Actor)
	{
		base.OnTerminate(Actor);
		if (!Actor.IsFlying())
		{
			FallToGround(Actor.transform);
		}
	}

	public virtual void FallToGround(Transform Target)
	{
		Vector3 position = Target.position;
		position.y += _GroundCheckStartHeight;
		mPrevHeight = position.y;
		float groundHeight;
		Vector3 normal;
		Collider groundHeight2 = UtUtilities.GetGroundHeight(position, _GroundCheckDist, out groundHeight, out normal);
		if (groundHeight2 != null)
		{
			position.y = ((groundHeight2.gameObject.layer == mWaterLayer) ? (groundHeight - _WaterSinkHeight) : groundHeight);
		}
		else if (mMoveToTransform != null)
		{
			position.y = TweenY(mMoveToTransform.position.y, _HeightTweenSpeed);
		}
		else
		{
			position.y -= _GroundCheckDist;
		}
		Target.position = position;
	}

	protected float TweenY(float targetY, float tweenspeed)
	{
		float num = mPrevHeight;
		float num2 = targetY - num;
		float num3 = 1f;
		float num4 = num2;
		if (num4 < 0f)
		{
			num4 = 0f - num4;
		}
		if (num4 > 5f)
		{
			num3 += num4 / 10f;
		}
		if (targetY > num)
		{
			num += Time.deltaTime * _HeightTweenSpeed * num3;
			if (num > targetY)
			{
				num = targetY;
			}
		}
		else if (targetY < num)
		{
			num -= Time.deltaTime * _HeightTweenSpeed * num3;
			if (num < targetY)
			{
				num = targetY;
			}
		}
		return num;
	}
}
