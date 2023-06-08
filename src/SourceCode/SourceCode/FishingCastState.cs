using UnityEngine;

public class FishingCastState : FishingState
{
	private float mEnagageTimer;

	private bool mDone;

	private Animator mAvatarAnimator;

	private float mSoundTimer;

	private bool mSoundPlayed;

	public override void Enter()
	{
		mController.mPlayerAnimState = "cast";
		UtDebug.Log("ENTERING : Cast state ");
		mEnagageTimer = Random.Range(mController._EngageDelayMin, mController._EngageDelayMax);
		mController.StartFishingCam();
		mController._CurrentFishingRod.GetComponent<FishingRod>()._FloatObject = mController._ReelFloat._Float;
		mAvatarAnimator = AvAvatar.pObject.GetComponentInChildren<Animator>();
		mController.SendMessage("CastLine", SendMessageOptions.DontRequireReceiver);
		if (!mController.pIsTutAvailable)
		{
			FishingZone._FishingZoneUi.ShowStrikeButton(show: true);
		}
		mSoundPlayed = false;
		mSoundTimer = 0.6f;
		base.Enter();
	}

	public override void Exit()
	{
		base.Exit();
	}

	public override void Execute()
	{
		base.Execute();
		if (!mSoundPlayed)
		{
			mSoundTimer -= Time.deltaTime;
			if (mSoundTimer <= 0.01f)
			{
				if (null != mController._SndCastLine)
				{
					SnChannel.Play(mController._SndCastLine, "", inForce: true);
				}
				mSoundPlayed = true;
			}
		}
		mEnagageTimer -= Time.deltaTime;
		if (mController.mFalseStrikeTimer > 0f)
		{
			mController.mPlayerAnimState = "strike";
		}
		else
		{
			mController.mPlayerAnimState = "cast";
		}
		if (null != mAvatarAnimator && !mDone)
		{
			AnimatorStateInfo currentAnimatorStateInfo = mAvatarAnimator.GetCurrentAnimatorStateInfo(2);
			if (currentAnimatorStateInfo.IsName("Fishing.FishingCastIdle") || currentAnimatorStateInfo.IsName("Fishing.DragonFishingCastIdle"))
			{
				FishingRod component = mController._CurrentFishingRod.GetComponent<FishingRod>();
				mController._ReelFloat.Setup(component._ReelDrag, component._ReelMax);
				component.LineSetVisible(visible: true);
				mController.PlayRipple(bPlay: true);
				mController.PlayFloatSplash(bPlay: true);
				mDone = true;
			}
		}
		if (mEnagageTimer < 0.001f)
		{
			FishingZone._FishingZoneUi.SetReelClicked(isClicked: false);
			mController.SetState(5);
		}
		if ((KAInput.GetKeyUp("CastRod") || FishingZone._FishingZoneUi.IsReelClicked()) && !mController.pIsTutAvailable)
		{
			FishingZone._FishingZoneUi.SetReelClicked(isClicked: false);
			mController.mFalseStrikeTimer = 0.25f;
		}
	}
}
