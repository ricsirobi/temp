using UnityEngine;

public class FishingReadyState : FishingState
{
	public override void Enter()
	{
		mController.mPlayerAnimState = "idle";
		mController.ShowFishingButton(show: true);
		UtDebug.Log("ENTERING: READY_STATE :Click fishing button and go to nibble mode");
		base.Enter();
		Vector3 position = mController._ReelFloat.transform.position;
		position.y = AvAvatar.position.y;
		AvAvatar.mTransform.LookAt(position);
	}

	public override void Exit()
	{
		base.Exit();
		UtDebug.Log("Hide fishing button");
		mController.ShowFishingButton(show: false);
	}

	public override void Execute()
	{
		base.Execute();
		if (KAInput.GetKeyUp("CastRod"))
		{
			mController.SetState(4);
		}
	}

	public override void Initialize(FishingZone controller, int nStateId)
	{
		base.Initialize(controller, nStateId);
	}

	public override void ShowTutorial()
	{
		mController.StartTutorial();
		mController.pFishingTutDB.Set("", mController._TutMessages[3]._LocaleText.GetLocalizedString());
		mController.pFishingTutDB.SetPosition(mController._TutMessages[3]._Position.x, mController._TutMessages[3]._Position.y);
		FishingZone._FishingZoneUi.ShowCastPointer(show: true);
		if (mController.pIsTutAvailable)
		{
			FishingZone._FishingZoneUi.AnimateStartFishingButton(bAnim: true);
		}
	}
}
