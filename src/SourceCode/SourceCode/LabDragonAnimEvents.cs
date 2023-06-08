public class LabDragonAnimEvents : AvAnimEvents
{
	private string mCurrentAnim = string.Empty;

	private float mPrevTime;

	private bool mAnimStartTriggered;

	public void StartAnimTrigger(string inName)
	{
		mCurrentAnim = inName;
		mAnimStartTriggered = true;
	}

	public void Update()
	{
		if (string.IsNullOrEmpty(mCurrentAnim) || !base.animation.IsPlaying(mCurrentAnim))
		{
			return;
		}
		if (mAnimStartTriggered)
		{
			mAnimStartTriggered = false;
			StartAnimationEventProcessing(mCurrentAnim);
		}
		else
		{
			float num = base.animation[mCurrentAnim].normalizedTime % 1f;
			if (mPrevTime > num)
			{
				base.animation[mCurrentAnim].time = base.animation[mCurrentAnim].normalizedTime % 1f * base.animation[mCurrentAnim].length;
			}
		}
		mPrevTime = base.animation[mCurrentAnim].normalizedTime % 1f;
	}
}
