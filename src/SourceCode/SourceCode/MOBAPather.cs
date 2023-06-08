using UnityEngine;

public class MOBAPather : MOBAMovingEntity
{
	protected MOBAPathTarget mPathTarget;

	protected MOBAPathTarget mCurNode;

	[Replicated]
	private int rNID = -1;

	protected Vector3 mPathDest = Vector3.zero;

	protected float mPathVelocity;

	public override void Init()
	{
		base.Init();
	}

	protected override void EntityUpdate(bool bIsAuthority)
	{
		base.EntityUpdate(bIsAuthority);
		if (!bIsAuthority || !PathUpdate(mPathVelocity) || !(mCurNode != null))
		{
			return;
		}
		if (mCurNode._NextPathObject != null)
		{
			mCurNode = mCurNode._NextPathObject.GetComponent<MOBAPathTarget>();
			if (mCurNode != null)
			{
				mPathDest = mCurNode.GetRandomLocation();
				rNID++;
			}
		}
		else
		{
			mPathDest = Vector3.zero;
		}
	}

	public override void OnGainAuthority()
	{
		if (rNID <= 0)
		{
			return;
		}
		mCurNode = mPathTarget;
		for (int i = 0; i < rNID; i++)
		{
			if (mCurNode != null && mCurNode._NextPathObject != null)
			{
				mCurNode = mCurNode._NextPathObject.GetComponent<MOBAPathTarget>();
			}
		}
		if (mCurNode != null)
		{
			mPathDest = mCurNode.GetRandomLocation();
		}
		else
		{
			mPathDest = Vector3.zero;
		}
	}

	protected void PathReset()
	{
		if (mPathTarget != null)
		{
			mCurNode = mPathTarget;
			mPathDest = mCurNode.GetRandomLocation();
			rNID = 0;
		}
	}

	private bool PathUpdate(float speed)
	{
		Vector3 position = base.transform.position;
		if (mPathTarget != null && mPathDest != Vector3.zero)
		{
			Vector3 forward = mPathDest - position;
			forward.y = 0f;
			float y = Quaternion.LookRotation(forward).eulerAngles.y;
			rFor = Mathf.LerpAngle(rFor, y, Time.deltaTime);
			rVel = speed;
		}
		else
		{
			rVel = 0f;
		}
		position.y = 0f;
		Vector3 vector = mPathDest;
		vector.y = 0f;
		if ((vector - position).magnitude < speed)
		{
			return true;
		}
		return false;
	}
}
