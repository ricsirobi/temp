using UnityEngine;

public class FishingNibbleState : FishingState
{
	private bool mWaitForNibble;

	private Bar mNibbleBar;

	private Fish genFish;

	private float mTimer;

	private UiFishing mUi;

	private float mFloatValue;

	private float mTargetFloatValue;

	private float mInZoneTimer;

	public override void Enter()
	{
		mController.mPlayerAnimState = "castidle";
		UtDebug.Log("ENTERING: NIBBSTATE");
		mWaitForNibble = false;
		mFloatValue = mController._ReelFloat.pNibbleValue;
		mTargetFloatValue = 1f;
		base.Enter();
	}

	public override void Exit()
	{
		if (null != mNibbleBar)
		{
			Object.Destroy(mNibbleBar.gameObject);
			mNibbleBar = null;
		}
		base.Exit();
	}

	public void OnFishNibble()
	{
	}

	public override void Execute()
	{
		base.Execute();
		if (genFish == null)
		{
			genFish = mController.SpawnFish(mTimer, 0.001f);
			mTimer += Time.deltaTime;
			if (genFish != null)
			{
				UtDebug.Log("Fish weight = " + genFish._Weight);
				FishingRod component = mController._CurrentFishingRod.GetComponent<FishingRod>();
				UtDebug.Log("Found Fish: " + genFish.pName + "\nFish Weight: " + genFish._Weight + "\nFish Level: " + genFish._Rank + "\nRod Power = " + component._RodPower + "\nReel Max = " + component._ReelMax);
				mTimer = 0f;
				mController._CurrentFish = genFish;
				mController.LoadFish();
				if (null != mController._SndFishFloatTug)
				{
					SnChannel.Play(mController._SndFishFloatTug, "DEFAULT_POOL", inForce: true, null);
				}
			}
		}
		else
		{
			if (mController.pCurrentFishGameObject == null)
			{
				return;
			}
			if (!mWaitForNibble)
			{
				mFloatValue = Mathf.MoveTowards(mFloatValue, mTargetFloatValue, Time.deltaTime);
				mController._ReelFloat.Nibble(mFloatValue + mController._StrikeFloatOffset);
				if (mFloatValue >= 1f)
				{
					mWaitForNibble = true;
					mTimer = mController._StrikeDuration;
					FishingZone._FishingZoneUi.ShowStrikePopupText(show: true);
					if (mController.pIsTutAvailable)
					{
						FishingZone._FishingZoneUi.ShowStrikeButton(show: true);
						FishingZone._FishingZoneUi.AnimateStrikeButton(bAnim: true);
						FishingZone._FishingZoneUi.ShowCastPointer(show: true);
					}
				}
			}
			if (!mWaitForNibble || genFish == null)
			{
				return;
			}
			mTimer -= Time.deltaTime;
			if (mTimer > 0.01f)
			{
				if (KAInput.GetKeyUp("CastRod") || FishingZone._FishingZoneUi.IsReelClicked())
				{
					float num = 1f / (float)genFish._Rank;
					FishingRod component2 = mController._CurrentFishingRod.GetComponent<FishingRod>();
					mController.ShowReelbar(show: true, num * mController._BaitLoseZoneWidth * component2._BaitLoseModifier, mController._LineSnapZoneWidth * component2._LineSnapModifier);
					mController.SetState(7);
					FishingZone._FishingZoneUi.SetReelClicked(isClicked: false);
					mController.gameObject.SendMessage("Strike", SendMessageOptions.DontRequireReceiver);
					mController.PlayStopStrikeMusic(Play: true);
				}
			}
			else if (mController.pIsTutorialRunning)
			{
				if (KAInput.GetKeyUp("CastRod") || FishingZone._FishingZoneUi.IsReelClicked())
				{
					float num2 = 1f / (float)genFish._Rank;
					FishingRod component3 = mController._CurrentFishingRod.GetComponent<FishingRod>();
					mController.ShowReelbar(show: true, num2 * mController._BaitLoseZoneWidth * component3._BaitLoseModifier, mController._LineSnapZoneWidth * component3._LineSnapModifier);
					mController.SetState(7);
					FishingZone._FishingZoneUi.SetReelClicked(isClicked: false);
					mController.gameObject.SendMessage("Strike", SendMessageOptions.DontRequireReceiver);
					mController.PlayStopStrikeMusic(Play: true);
				}
			}
			else
			{
				mTargetFloatValue = 0.5f;
				mFloatValue = Mathf.MoveTowards(mFloatValue, mTargetFloatValue, Time.deltaTime);
				mController._ReelFloat.Nibble(mFloatValue);
				if (Mathf.Abs(mFloatValue - mTargetFloatValue) < 0.01f)
				{
					UtDebug.Log("strike time is over lose bait");
					mController._StrikeFailed = true;
					mController.SetState(10);
				}
			}
		}
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}
}
