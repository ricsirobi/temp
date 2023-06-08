using SWS;
using UnityEngine;

public class ObWaitForPlayerOnSpline : KAMonoBase
{
	public float _TargetProximityRadius = 6f;

	public AIActor _Actor;

	private splineMove mSplineMove;

	private bool mTargetFollowing = true;

	private float mPrevSpeed;

	public void Start()
	{
		mSplineMove = _Actor.GetComponent<splineMove>();
	}

	private void Update()
	{
		if (!mTargetFollowing)
		{
			CheckTargetFollowing();
		}
	}

	private void PauseActor()
	{
		if (mTargetFollowing)
		{
			mPrevSpeed = mSplineMove.speed;
			mSplineMove.speed = 0f;
			mSplineMove.Pause();
			mTargetFollowing = false;
		}
	}

	private void ResumeActor()
	{
		if (!mTargetFollowing)
		{
			mSplineMove.speed = mPrevSpeed;
			mSplineMove.Resume();
			mTargetFollowing = true;
		}
	}

	public void CheckTargetFollowing()
	{
		if (Vector3.Distance(AvAvatar.mTransform.position, _Actor.transform.position) > _TargetProximityRadius)
		{
			PauseActor();
		}
		else if (mSplineMove.speed == 0f)
		{
			ResumeActor();
		}
	}

	public void SetSplineRoot(GameObject splineRoot)
	{
		ResumeActor();
	}
}
