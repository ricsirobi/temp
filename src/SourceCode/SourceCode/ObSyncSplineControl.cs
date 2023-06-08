public class ObSyncSplineControl : SplineControl
{
	public float _DistanceOffset = 20f;

	private bool mCanUpdatePos;

	private float mReceivedTime;

	public void OnGetElapsedTimeEvent(float time)
	{
		mReceivedTime = time;
		mCanUpdatePos = true;
	}

	public override void SetPosOnSpline(float p)
	{
		if (mCanUpdatePos)
		{
			p = (mReceivedTime * Speed + _DistanceOffset) % LinearLength;
			mCanUpdatePos = false;
		}
		base.SetPosOnSpline(p);
	}
}
