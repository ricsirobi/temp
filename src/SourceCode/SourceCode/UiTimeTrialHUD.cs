public class UiTimeTrialHUD : KAUI
{
	private KAWidget mTimerTxt;

	private KAWidget mChallengeInfoTxt;

	private KAWidget mTimerNeedle;

	private float mTimerNeedleStartRot;

	protected override void Start()
	{
		mTimerTxt = FindItem("BtnTimer");
		mChallengeInfoTxt = FindItem("ChallengeInfo");
		mTimerNeedle = mTimerTxt.FindChildItem("AniNeedle");
		mTimerNeedleStartRot = mTimerNeedle._RotateSpeed;
	}

	public void StopTimer()
	{
		PauseTimer();
		ResetTimer();
	}

	public void ResetTimer()
	{
	}

	public void PauseTimer()
	{
		mTimerNeedle.SetRotateSpeed(0f);
	}

	public void StartTimer(bool showChallengeTime)
	{
		mTimerTxt.SetVisibility(inVisible: true);
		if (showChallengeTime && (bool)mChallengeInfoTxt)
		{
			mChallengeInfoTxt.SetVisibility(inVisible: true);
		}
		mTimerNeedle.SetRotateSpeed(mTimerNeedleStartRot);
	}

	public void UpdateTimeDisplay(float time)
	{
		mTimerTxt.SetText(GameUtilities.FormatTime(time));
	}

	public void UpdateChallengeTimeDisplay(float time, string challengeText = "")
	{
		KAWidget kAWidget = FindItem("ChallengePoints");
		kAWidget.SetText(GameUtilities.FormatTime(time));
		kAWidget.SetVisibility(inVisible: true);
		FindItem("ChallengeText").SetVisibility(inVisible: true);
		if (!string.IsNullOrEmpty(challengeText))
		{
			FindItem("ChallengeText").SetText(challengeText);
		}
	}

	public string GetTimeDisplay()
	{
		return mTimerTxt.GetText();
	}
}
