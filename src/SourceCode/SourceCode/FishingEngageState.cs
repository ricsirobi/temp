using UnityEngine;

public class FishingEngageState : FishingState
{
	private float mTimer;

	private float f;

	private float t;

	private float mLastTimer;

	private bool mUpdate;

	public override void ShowTutorial()
	{
		mController.StartTutorial();
		mController.pFishingTutDB.Set("", mController._TutMessages[4]._LocaleText.GetLocalizedString());
		mController.pFishingTutDB.SetPosition(mController._TutMessages[4]._Position.x, mController._TutMessages[4]._Position.y);
	}

	public override void Enter()
	{
		mUpdate = true;
		mTimer = Random.Range(mController._EngageDurationMin, mController._EngageDurationMax);
		mLastTimer = mTimer;
		FishingZone._FishingZoneUi.SetStateText(mController._EngageStateText.GetLocalizedString());
		mController.PlayFloatSplash(bPlay: false);
		if (!mController.pIsTutAvailable)
		{
			FishingZone._FishingZoneUi.ShowStrikeButton(show: true);
		}
		base.Enter();
	}

	public override void Exit()
	{
		base.Exit();
	}

	protected override void HandleOkCancel()
	{
		mUpdate = true;
	}

	public override void Execute()
	{
		if (mUpdate)
		{
			base.Execute();
			mTimer -= Time.deltaTime;
			if (mController.mFalseStrikeTimer > 0f)
			{
				mController.mPlayerAnimState = "strike";
			}
			else
			{
				mController.mPlayerAnimState = "castidle";
			}
			if (mTimer < 0f)
			{
				mController.SetState(6);
			}
			if (mLastTimer - mTimer > mController._EngageFrequency)
			{
				t += Random.Range(0.02f, 0.8f);
				t = Mathf.Clamp(t, 0.01f, 1f + mController._FloatOffset);
				mLastTimer = mTimer;
			}
			float floatOffset = mController._FloatOffset;
			f = Mathf.Lerp(f, t, 1f / mController._EngageFloatSpeed * Time.deltaTime);
			f += 0.5f;
			f = Mathf.Clamp(f, 0.3f, 1f + floatOffset);
			mController._ReelFloat.Nibble(f);
			t -= Time.deltaTime;
			t = Mathf.Clamp(t, 0f, 1f + floatOffset);
			if ((KAInput.GetKeyUp("CastRod") || FishingZone._FishingZoneUi.IsReelClicked()) && f < 0.8f + floatOffset && !mController.pIsTutAvailable)
			{
				mController.SetState(5);
				FishingZone._FishingZoneUi.SetReelClicked(isClicked: false);
				mController.mFalseStrikeTimer = 0.25f;
			}
		}
	}
}
