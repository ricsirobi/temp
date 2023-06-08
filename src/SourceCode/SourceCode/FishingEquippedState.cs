using UnityEngine;

public class FishingEquippedState : FishingState
{
	private bool mShowBaitArrow;

	private GameObject mBaitBucket;

	public override void Enter()
	{
		mController.mPlayerAnimState = "idle";
		UtDebug.Log("ENTERING: EQUIPPED_STATE Show fishing button");
		if (mController.pIsTutAvailable)
		{
			AvatarEquipment.pInstance.RemoveItem(EquipmentParts.BAIT);
		}
		if (!mController.IsBaitEquipped())
		{
			FishingZone._FishingZoneUi.ShowStopFishingButton(show: false);
			string[] array = mController._BaitBarrelPath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnBaitBucketLoadingEvent, typeof(GameObject));
		}
		if (null != FishingZone._FishingZoneUi)
		{
			FishingZone._FishingZoneUi.SetStateText("");
		}
		base.Enter();
	}

	public void OnBaitBucketLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			FishingZone._FishingZoneUi.ShowStopFishingButton(show: true);
			if (inObject != null)
			{
				mBaitBucket = Object.Instantiate((GameObject)inObject);
				mBaitBucket.name = "PfBaitBucket";
				mController.SetupBaitBucket(mBaitBucket, OnBaitMenuVisible);
				Collider component = mBaitBucket.GetComponent<Collider>();
				if (component != null)
				{
					component.enabled = false;
					component.enabled = true;
				}
			}
			break;
		case RsResourceLoadEvent.ERROR:
			FishingZone._FishingZoneUi.ShowStopFishingButton(show: true);
			break;
		}
	}

	private void OnBaitMenuVisible(bool show)
	{
		if (mController.pIsTutAvailable)
		{
			SecondStep();
		}
	}

	public override void Exit()
	{
		base.Exit();
		if (null != mBaitBucket)
		{
			FishingZone._FishingZoneUi.ShowBaitPointer(show: false);
			mController.DestroyBaitBucket(mBaitBucket, OnBaitMenuVisible);
		}
		UtDebug.Log("Hide fishing button");
	}

	public override void Execute()
	{
		base.Execute();
		if (null != mBaitBucket && mController.pIsTutorialRunning)
		{
			if (mShowBaitArrow)
			{
				mShowBaitArrow = false;
				FishingZone._FishingZoneUi.ShowBaitPointer(show: true);
			}
			Transform transform = mBaitBucket.transform.Find("FishingBaitBox/Lid");
			FishingZone._FishingZoneUi.UpdateBaitPointer(transform.position);
		}
		if (mController.IsEquipped())
		{
			mController.SetState(2);
		}
	}

	public override void ShowTutorial()
	{
		mController.pIsTutorialRunning = true;
		FirstStep();
	}

	protected void FirstStep()
	{
		mController.StartTutorial();
		mController.pFishingTutDB.Set("", mController._TutMessages[1]._LocaleText.GetLocalizedString());
		mController.pFishingTutDB.SetPosition(mController._TutMessages[1]._Position.x, mController._TutMessages[1]._Position.y);
		mController.pFishingTutDB.ShowHideBigDB(show: false);
		mShowBaitArrow = true;
		if (mController.pIsTutAvailable && !FUEManager.pIsFUERunning)
		{
			MainStreetMMOClient.pInstance.ActivateAll(active: false);
		}
	}

	protected void SecondStep()
	{
		mController.StartTutorial();
		mController.pFishingTutDB.Set("", mController._TutMessages[2]._LocaleText.GetLocalizedString());
		mController.pFishingTutDB.SetPosition(mController._TutMessages[2]._Position.x, mController._TutMessages[2]._Position.y);
		FishingZone._FishingZoneUi.ShowBaitPointer(show: false);
	}

	protected override void HandleYesNo(bool yes)
	{
		if (yes)
		{
			FirstStep();
			return;
		}
		FishingZone.pIsTutDone = true;
		mController.SetState(0);
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}
}
