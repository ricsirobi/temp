using System;
using UnityEngine;

public class FishingReelState : FishingState
{
	private float mReelTimer;

	private float mReelDuration;

	private float mWaitTimer;

	private float mCaptureTimer;

	private float mLoseTimer;

	private float mLineSnapTimer;

	private float mLoseBaitTension;

	private float mLineSnapTension;

	private float mMarkerValue;

	private float mFishStateTimer;

	private FishingZone.FishState mFishState;

	private float mStateSpeed;

	private float mReelValue;

	private int[] mStateProbability = new int[3];

	private float mCurrentTension;

	private float mTargetTension;

	private float mMaxTargetTension;

	private float mVal;

	private FishingRod mRod;

	private bool mUpdate;

	private bool mSnapped;

	private bool mFistTutStep;

	private readonly string[] mAnimStates = new string[4] { "FranticLow", "FranticHigh", "Idle", "Jump" };

	private float mFishJumpDelay = 3f;

	private float mFishJumpTimer;

	private const int mJumpIndex = 3;

	private SnChannel mSndReelIn;

	private SnChannel mSndReelOut;

	private SnChannel mSndFishPull;

	public override void ShowTutorial()
	{
		mUpdate = false;
		mFistTutStep = true;
		mController.StartTutorial();
		mController.pFishingTutDB.Set("", mController._TutMessages[5]._LocaleText.GetLocalizedString());
		mController.pFishingTutDB.SetPosition(mController._TutMessages[5]._Position.x, mController._TutMessages[5]._Position.y);
		if (mController.pIsTutAvailable)
		{
			FishingZone._FishingZoneUi.AnimateReelButton(bAnim: true);
		}
	}

	protected override void HandleOkCancel()
	{
		mController._StrikeFailed = true;
		if (mMarkerValue > mLineSnapTension)
		{
			mController.SetState(11);
		}
		else
		{
			mController.SetState(10);
		}
	}

	private void UpdateZonesSize()
	{
		float baitLoseWidth = 1f / (float)mController._CurrentFish._Rank * mController._BaitLoseZoneWidth * mRod._BaitLoseModifier;
		float lineSnapWidth = mController._LineSnapZoneWidth * mRod._LineSnapModifier;
		mController.UpdateZoneSize(baitLoseWidth, lineSnapWidth);
		mLoseBaitTension = 0f + mController._BaitLoseZoneWidth * mRod._BaitLoseModifier;
		mLineSnapTension = 1f - mController._LineSnapZoneWidth * mRod._LineSnapModifier;
	}

	public override void Enter()
	{
		mController.mPlayerAnimState = "strike";
		mUpdate = true;
		UtDebug.Log("ENTERING: REEL_MODE");
		mReelDuration = mController._ReelTimeout;
		mReelTimer = 0f;
		mMarkerValue = 0f;
		mRod = mController._CurrentFishingRod.GetComponent<FishingRod>();
		mLoseBaitTension = 0f + mController._BaitLoseZoneWidth * mRod._BaitLoseModifier;
		mLineSnapTension = 1f - mController._LineSnapZoneWidth * mRod._LineSnapModifier;
		mCurrentTension = (mLoseBaitTension + mLineSnapTension) * 0.5f;
		mTargetTension = mCurrentTension;
		mFishState = FishingZone.FishState.STATIC;
		for (int i = 0; i < mController._CurrentFish._StateData.Length; i++)
		{
			if (mController._CurrentFish._StateData[i]._State == FishingZone.FishState.STATIC)
			{
				mFishStateTimer = mController._CurrentFish._StateData[i]._Duration;
				mTargetTension = mController._CurrentFish._StateData[i]._Tension;
				mStateSpeed = mTargetTension;
			}
		}
		mReelValue = 0f;
		string fishName = FishingData.pInstance.GetFishName(mController._CurrentFish.pItemID);
		FishingZone._FishingZoneUi.SetFishName(fishName);
		mStateProbability[0] = mController._CurrentFish._NormalStateChance;
		mStateProbability[1] = mController._CurrentFish._IdleStateChance;
		mStateProbability[2] = mController._CurrentFish._FastStateChance;
		mController._ReelFloat.StartReel();
		FishingZone._FishingZoneUi.SetZoneColors(mController._LoseBaitColor, mController._LineSnapColor);
		FishingZone._FishingZoneUi.SetStateText(mController._ReelStateText.GetLocalizedString());
		FishingZone._FishingZoneUi.ShowStrikeButton(show: false);
		FishingZone._FishingZoneUi.ShowReelButton(show: true);
		FishingZone._FishingZoneUi.ShowStrikePopupText(show: false);
		mController.PlayRipple(bPlay: false);
		mController.StartReelCam();
		base.Enter();
	}

	public override void Exit()
	{
		mController._ReelFloat.StopReel();
		mController.ShowReelbar(show: false, 0f, 0f);
		mController.PlaySplash(bPlay: false);
		mController.PlayRodAnim(0f);
		FishingZone._FishingZoneUi.ShowStrikeButton(show: false);
		FishingZone._FishingZoneUi.ShowReelButton(show: false);
		mController.PlayFloatMoveRipple(bPlay: false);
		mController.PlayStopStrikeMusic(Play: false);
		if (mController.pIsTutAvailable && !FUEManager.pIsFUERunning)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: true);
		}
		if (null != mSndReelIn)
		{
			mSndReelIn.pLoop = false;
			mSndReelIn.Stop();
		}
		if (null != mSndReelOut)
		{
			mSndReelOut.pLoop = false;
			mSndReelOut.Stop();
		}
		if (null != mSndFishPull)
		{
			mSndFishPull.pLoop = false;
			mSndFishPull.Stop();
		}
		base.Exit();
	}

	private bool IsInCaptureZone(float aValue)
	{
		if (mLoseBaitTension < aValue && mLineSnapTension > aValue)
		{
			return true;
		}
		return false;
	}

	public static int GetSelection(int[] weights)
	{
		int num = 0;
		for (int i = 0; i < weights.Length; i++)
		{
			num += weights[i];
		}
		int num2 = UnityEngine.Random.Range(0, num);
		int num3 = 0;
		for (int i = 0; i < weights.Length - 1; i++)
		{
			num3 += weights[i];
			if (num2 < num3)
			{
				return i;
			}
		}
		return weights.Length - 1;
	}

	public override void Execute()
	{
		if (mController.pIsTutAvailable)
		{
			if (!mUpdate && mFistTutStep)
			{
				if (KAInput.GetKeyUp("CastRod") || FishingZone._FishingZoneUi.IsReelClicked())
				{
					mUpdate = true;
					mFistTutStep = false;
					FishingZone._FishingZoneUi.SetReelClicked(isClicked: false);
					if (null != mController.pFishingTutDB)
					{
						mController.pFishingTutDB.SetVisibility(inVisible: false);
					}
					FishingZone._FishingZoneUi.AnimateStrikeButton(bAnim: false);
					FishingZone._FishingZoneUi.AnimateReelButton(bAnim: false);
					FishingZone._FishingZoneUi.AnimateStartFishingButton(bAnim: false);
					FishingZone._FishingZoneUi.ShowBaitPointer(show: false);
					FishingZone._FishingZoneUi.ShowCastPointer(show: false);
				}
				return;
			}
			if (!mUpdate)
			{
				return;
			}
		}
		base.Execute();
		mController.MoveFishUpToSurface(0.25f);
		if (Application.isEditor)
		{
			UpdateZonesSize();
		}
		mReelTimer += Time.deltaTime;
		if (mReelTimer >= mReelDuration)
		{
			UtDebug.Log("Reeling timed out ..lost bait... going back to equip state");
			mReelTimer = 0f;
			mController._StrikeFailed = true;
			mController.SetState(10);
			return;
		}
		float num = mRod._RodPower / (float)mController._CurrentFish._Rank;
		if (mWaitTimer > 0f)
		{
			mWaitTimer -= Time.deltaTime;
		}
		if (mFishStateTimer < 0.01f)
		{
			FishingZone.FishStateData fishStateData = mController._CurrentFish._StateData[GetSelection(mStateProbability)];
			mFishState = fishStateData._State;
			mStateSpeed = fishStateData._Tension;
			mFishStateTimer = fishStateData._Duration;
			if (mFishState == FishingZone.FishState.STATIC)
			{
				mController.PlaySplash(bPlay: true);
				if (null != mController._SndFishRodPull && (null == mSndFishPull || mSndFishPull.pClip == null))
				{
					mSndFishPull = SnChannel.Play(mController._SndFishRodPull, "DEFAULT_POOL", inForce: true);
					mSndFishPull.pLoop = true;
				}
			}
			else if (mFishState == FishingZone.FishState.NORMAL)
			{
				mController.PlaySplash(bPlay: false);
				if (null != mSndFishPull && mSndFishPull.pIsPlaying)
				{
					mSndFishPull.pLoop = false;
					mSndFishPull.Stop();
				}
			}
			else if (mFishState == FishingZone.FishState.FAST)
			{
				mController.PlaySplash(bPlay: true);
				if (null != mController._SndFishRodPull && (null == mSndFishPull || mSndFishPull.pClip == null))
				{
					mSndFishPull = SnChannel.Play(mController._SndFishRodPull, "DEFAULT_POOL", inForce: true);
					mSndFishPull.pLoop = true;
				}
			}
		}
		mFishJumpTimer -= Time.deltaTime;
		mFishJumpDelay -= Time.deltaTime;
		if (mFishJumpTimer < 0f && mFishJumpDelay < 0f && mFishState == FishingZone.FishState.FAST)
		{
			mFishJumpTimer = mController.PlayFishAnim(mAnimStates[3]);
			mFishJumpDelay = UnityEngine.Random.Range(mFishJumpTimer, 8f);
		}
		else if (mFishJumpTimer <= 0f)
		{
			mController.PlayFishAnim(mAnimStates[(int)mFishState]);
			FishingZone._FishingZoneUi.PlayFishAnim(mAnimStates[(int)mFishState]);
		}
		float num2 = mStateSpeed;
		float num3 = 0f;
		mFishStateTimer -= Time.deltaTime;
		if ((KAInput.IsPressed("CastRod") || FishingZone._FishingZoneUi.IsReelPressed()) && !FishingZone._FishingZoneUi.pDragPressed)
		{
			mReelValue = mRod._RodPower * mController._PlayerTensionPercentage;
			if (null != mController._ReelFloat)
			{
				mController._ReelFloat.ReelIn(mReelValue * mController._ReelFloatSpeed * mRod._FloatSpeedModifier);
			}
			mController.RotateReel(mController._PlayerTensionPercentage);
			mController.mReelAnimHigh = true;
			mController.PlayFloatMoveRipple(bPlay: true);
			if (null != mSndReelOut && mSndReelOut.pIsPlaying)
			{
				mSndReelOut.Stop();
				mSndReelOut = null;
			}
			if (null != mController._SndFishReelIn && (null == mSndReelIn || mSndReelIn.pClip == null))
			{
				mSndReelIn = SnChannel.Play(mController._SndFishReelIn, "DEFAULT_POOL", inForce: true);
				mSndReelIn.pLoop = true;
			}
		}
		else if (FishingZone._FishingZoneUi.pDragPressed)
		{
			num3 = -1f * mRod._DragSpeed * Time.deltaTime;
			mController.RotateReel(1f / MathF.PI * num3);
			mController.mReelAnimHigh = false;
			mController.PlayFloatMoveRipple(bPlay: false);
			if (null != mSndReelIn && mSndReelIn.pIsPlaying)
			{
				mSndReelIn.Stop();
				mSndReelIn = null;
			}
			if (null != mController._SndFishReelOut && (null == mSndReelOut || mSndReelOut.pClip == null))
			{
				mSndReelOut = SnChannel.Play(mController._SndFishReelOut, "DEFAULT_POOL", inForce: true);
				mSndReelOut.pLoop = true;
			}
		}
		else
		{
			mReelValue = 0f;
			float force = -1f * num * 0.02f;
			mController.RotateReel(force);
			mController.mReelAnimHigh = false;
			mController.PlayFloatMoveRipple(bPlay: false);
			if (null != mSndReelIn && mSndReelIn.pIsPlaying)
			{
				mSndReelIn.Stop();
			}
			if (null != mSndReelOut && mSndReelOut.pIsPlaying)
			{
				mSndReelOut.Stop();
			}
		}
		mTargetTension = mReelValue + num2 + num3;
		if (mCurrentTension < mTargetTension)
		{
			mCurrentTension = Mathf.Lerp(mCurrentTension, mTargetTension, mController._ReelInterpolationRate * mController._CurrentFish._RightInterpolationModifier * Time.deltaTime);
		}
		else
		{
			mCurrentTension = Mathf.Lerp(mCurrentTension, mTargetTension, mController._ReelLeftInterpolationRate * mController._CurrentFish._LeftInterpolationModifier * Time.deltaTime);
		}
		mController.PlayRodAnim(Mathf.Clamp(mCurrentTension, 0f, 1f));
		mMarkerValue = Mathf.Clamp01(mCurrentTension);
		mController.DoReel(mMarkerValue);
		if (IsInCaptureZone(mMarkerValue))
		{
			mCaptureTimer += Time.deltaTime;
			mLoseTimer = 0f;
			mLineSnapTimer = 0f;
			return;
		}
		mCaptureTimer = 0f;
		if (mMarkerValue < mLoseBaitTension)
		{
			if (!mSnapped)
			{
				mSnapped = true;
			}
			if (mMarkerValue < 0.01f)
			{
				mController._StrikeFailed = false;
				if (mController.pIsTutAvailable)
				{
					mController.StartTutorial();
					mController.pFishingTutDB.SetVisibility(inVisible: true);
					mController.pFishingTutDB.SetPosition(mController._TutMessages[7]._Position.x, mController._TutMessages[7]._Position.y);
					mController.pFishingTutDB.SetOk("", mController._TutMessages[7]._LocaleText.GetLocalizedString());
					FishingZone._FishingZoneUi.ShowCastPointer(show: false);
					mUpdate = false;
				}
				else
				{
					mController.SetState(10);
				}
			}
			if (mLoseTimer > mController._BaitLoseTime)
			{
				mController._StrikeFailed = false;
				if (mController.pIsTutAvailable)
				{
					mController.StartTutorial();
					mController.pFishingTutDB.SetVisibility(inVisible: true);
					mController.pFishingTutDB.SetPosition(mController._TutMessages[7]._Position.x, mController._TutMessages[7]._Position.y);
					mController.pFishingTutDB.SetOk("", mController._TutMessages[7]._LocaleText.GetLocalizedString());
					FishingZone._FishingZoneUi.ShowCastPointer(show: false);
					mUpdate = false;
				}
				else
				{
					mController.SetState(10);
				}
			}
			mLoseTimer += Time.deltaTime;
		}
		else
		{
			if (!(mMarkerValue > mLineSnapTension))
			{
				return;
			}
			if (!mSnapped)
			{
				mSnapped = true;
			}
			if (mMarkerValue > 0.98f)
			{
				if (mController.pIsTutAvailable)
				{
					mController.StartTutorial();
					mController.pFishingTutDB.SetVisibility(inVisible: true);
					mController.pFishingTutDB.SetPosition(mController._TutMessages[6]._Position.x, mController._TutMessages[6]._Position.y);
					mController.pFishingTutDB.SetOk("", mController._TutMessages[6]._LocaleText.GetLocalizedString());
					FishingZone._FishingZoneUi.ShowCastPointer(show: false);
					mUpdate = false;
				}
				else
				{
					mController.SetState(11);
				}
			}
			if (mLineSnapTimer > mController._LineSnapTime)
			{
				if (mController.pIsTutAvailable)
				{
					mController.StartTutorial();
					mController.pFishingTutDB.SetVisibility(inVisible: true);
					mController.pFishingTutDB.SetPosition(mController._TutMessages[6]._Position.x, mController._TutMessages[6]._Position.y);
					mController.pFishingTutDB.SetOk("", mController._TutMessages[6]._LocaleText.GetLocalizedString());
					FishingZone._FishingZoneUi.ShowCastPointer(show: false);
					mUpdate = false;
				}
				else
				{
					mController.SetState(11);
				}
			}
			mLineSnapTimer += Time.deltaTime;
		}
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}
}
