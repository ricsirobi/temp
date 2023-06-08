using UnityEngine;

public class AIBehavior_TeleportByFlying : AIBehavior
{
	private enum TeleportFlyState
	{
		FLYING_UP,
		FLYING_DOWN,
		LANDING,
		DONE
	}

	public AITarget _Target = new AITarget();

	public string _LandAnim = "IdleStand";

	public string _FlyingUpAnim = "GainingAltitudeFlap";

	public string _FlyingDownAnim = "FlyForward";

	public AIEvaluator EvaluatorIdle;

	private Vector3 mStartPos;

	private Vector3 mMiddlePos;

	private Vector3 mEndPos;

	private TeleportFlyState mFlyState;

	private float mMovementPct;

	private float mPctToMoveEachFrame;

	private float mPctToMoveEachFrameAtTheEnd;

	private float mRemainingAnimTime;

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (State == AIBehaviorState.FAILED)
		{
			return State;
		}
		ComputeMiddleAndEnd(Actor);
		float num = mPctToMoveEachFrame;
		if (mFlyState == TeleportFlyState.FLYING_DOWN)
		{
			float t = Mathf.Clamp01(mMovementPct / 0.85f);
			num = Mathf.Lerp(mPctToMoveEachFrame, mPctToMoveEachFrameAtTheEnd, t);
		}
		mMovementPct += num * Actor.DeltaTime;
		switch (mFlyState)
		{
		case TeleportFlyState.FLYING_UP:
			if (mMovementPct >= 1f)
			{
				mFlyState = TeleportFlyState.FLYING_DOWN;
				if (Actor.animation[_FlyingDownAnim] != null)
				{
					Actor.animation[_FlyingDownAnim].wrapMode = WrapMode.Loop;
					Actor.animation.CrossFade(_FlyingDownAnim, 0.2f, PlayMode.StopAll);
				}
				mMovementPct -= 1f;
			}
			break;
		case TeleportFlyState.FLYING_DOWN:
			if (mMovementPct >= 1f)
			{
				Actor.transform.position = mEndPos;
				mFlyState = TeleportFlyState.LANDING;
				AnimationState animationState = Actor.animation[_LandAnim];
				if (animationState != null)
				{
					Actor.animation.CrossFade(_LandAnim, 0.2f, PlayMode.StopAll);
					animationState.wrapMode = WrapMode.ClampForever;
					animationState.time = 0f;
					mRemainingAnimTime = 0.5f;
				}
				else
				{
					mRemainingAnimTime = 0f;
				}
			}
			break;
		case TeleportFlyState.LANDING:
			mRemainingAnimTime -= Actor.DeltaTime;
			if (mRemainingAnimTime < 0f)
			{
				mFlyState = TeleportFlyState.DONE;
				if (EvaluatorIdle != null)
				{
					EvaluatorIdle._Priority = 0f;
				}
				return SetState(AIBehaviorState.COMPLETED);
			}
			break;
		case TeleportFlyState.DONE:
			if (EvaluatorIdle != null)
			{
				EvaluatorIdle._Priority = 0f;
			}
			return SetState(AIBehaviorState.COMPLETED);
		}
		mMovementPct = Mathf.Clamp01(mMovementPct);
		if (mFlyState < TeleportFlyState.LANDING)
		{
			Actor.transform.position = GetCurrentTrayectoryPos();
		}
		if (AvAvatar.pObject != null)
		{
			Vector3 forward = AvAvatar.position - Actor.Position;
			forward.y = 0f;
			if (forward.x != 0f || forward.z != 0f)
			{
				Actor.transform.rotation = Quaternion.RotateTowards(Actor.transform.rotation, Quaternion.LookRotation(forward), 860f * Actor.DeltaTime);
			}
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnStart(AIActor Actor)
	{
		mStartPos = Actor.Position;
		ComputeMiddleAndEnd(Actor);
		if (Physics.Linecast(mStartPos + Vector3.up * 2f, mStartPos + Vector3.up * 10f, UtUtilities.GetGroundRayCheckLayers()))
		{
			SetState(AIBehaviorState.FAILED);
			return;
		}
		if (Physics.Linecast(mEndPos + Vector3.up * 2f, mEndPos + Vector3.up * 10f, UtUtilities.GetGroundRayCheckLayers()))
		{
			SetState(AIBehaviorState.FAILED);
			return;
		}
		mFlyState = TeleportFlyState.FLYING_UP;
		mMovementPct = 0f;
		SetState(AIBehaviorState.ACTIVE);
		if (EvaluatorIdle != null)
		{
			EvaluatorIdle._Priority = 1f;
		}
		Actor.animation.CrossFade(_FlyingUpAnim, 0.2f, PlayMode.StopAll);
	}

	private void DrawSection(Vector3 vStart, Vector3 vEnd, bool bUp)
	{
		Vector3 start = vStart;
		for (int i = 0; i < 20; i++)
		{
			float num = (float)i / 19f;
			Vector3 vector = Vector3.Lerp(vStart, vEnd, num);
			if (bUp)
			{
				vector.y = Mathf.Lerp(vStart.y, vEnd.y, 1f - (1f - num) * (1f - num));
			}
			else
			{
				vector.y = Mathf.Lerp(vStart.y, vEnd.y, num * num);
			}
			Debug.DrawLine(start, vector, Color.blue, 20f);
			start = vector;
		}
	}

	public override void OnTerminate(AIActor Actor)
	{
		Actor.FallToGround();
		base.OnTerminate(Actor);
		if (EvaluatorIdle != null)
		{
			EvaluatorIdle._Priority = 0f;
		}
	}

	private void ComputeMiddleAndEnd(AIActor Actor)
	{
		_Target.Actor = Actor;
		mEndPos = _Target.GetLocation();
		if (UtUtilities.GetGroundHeight(mEndPos + Vector3.up * 2f, 10f, out var groundHeight) != null)
		{
			mEndPos.y = groundHeight;
		}
		float num = Mathf.Max(mStartPos.y, mEndPos.y);
		float a = AIEvaluator.Dist2D(mStartPos, mEndPos) / 2f;
		a = Mathf.Min(a, 40f);
		mMiddlePos = (mStartPos + mEndPos) / 2f;
		mMiddlePos.y = num + a * 1.3f;
		mPctToMoveEachFrame = 0.7f;
		mPctToMoveEachFrameAtTheEnd = 3.21f / a;
	}

	private Vector3 GetCurrentTrayectoryPos()
	{
		float num = Mathf.Clamp01(mMovementPct);
		if (mFlyState == TeleportFlyState.FLYING_UP)
		{
			Vector3 result = Vector3.Lerp(mStartPos, mMiddlePos, num);
			result.y = Mathf.Lerp(mStartPos.y, mMiddlePos.y, 1f - (1f - num) * (1f - num));
			return result;
		}
		Vector3 result2 = Vector3.Lerp(mMiddlePos, mEndPos, num);
		result2.y = Mathf.Lerp(mMiddlePos.y, mEndPos.y, num * num);
		return result2;
	}
}
