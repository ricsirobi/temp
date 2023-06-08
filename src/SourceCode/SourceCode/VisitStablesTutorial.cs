using UnityEngine;

public class VisitStablesTutorial : InteractiveTutManager
{
	private VisitStablesTutorial mInstance;

	private bool mWaitForOtherTut;

	public override void Start()
	{
		mInstance = this;
		if (IsStableRoomLocked())
		{
			Exit();
			return;
		}
		Object.DontDestroyOnLoad(mInstance.gameObject);
		CoCommonLevel._ActivateObjectsOnLoad.Add(base.gameObject.name);
		if (CheckIsInStables())
		{
			KAUIStore.pInstance._ExitMessageObjects.Add(UserNotifyStableTutorial.pInstance.gameObject);
		}
		else
		{
			KAUIStore.pInstance._ExitMessageObjects.Add(mInstance.gameObject);
		}
	}

	private bool IsStableRoomLocked()
	{
		return !SanctuaryManager.IsActivePetDataReady();
	}

	private bool CheckIsInStables()
	{
		if (UserNotifyStableTutorial.pInstance != null)
		{
			ShowTutorial();
			CompleteTut();
			return true;
		}
		return false;
	}

	public override void SetTutDBButtonStates(bool inBtnNext, bool inBtnBack, bool inBtnYes, bool inBtnNo, bool inBtnDone, bool inBtnClose)
	{
		if (GetStepName() == "VisitStables")
		{
			base.SetTutDBButtonStates(inBtnNext: false, inBtnBack: false, inBtnYes: true, inBtnNo: true, inBtnDone: false, inBtnClose: false);
		}
		else
		{
			base.SetTutDBButtonStates(inBtnNext, inBtnBack, inBtnYes, inBtnNo, inBtnDone, inBtnClose);
		}
	}

	public void OnActivate()
	{
		StartTutorial();
	}

	public override void Update()
	{
		base.Update();
		if (mInstance.TutorialComplete() || IsStableRoomLocked())
		{
			Exit();
		}
		else if (mWaitForOtherTut)
		{
			StartTutorial();
		}
	}

	public override void Exit()
	{
		AvAvatar.EnableAllInputs(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		base.Exit();
	}

	private void CompleteTut()
	{
		if (InteractiveTutManager._CurrentActiveTutorialObject != null)
		{
			InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "CompleteTut");
		}
	}

	private void OnGenericInfoBoxYes()
	{
		CompleteTut();
		StableManager.LoadStable(0);
	}

	public void StartTutorial()
	{
		if (InteractiveTutManager._CurrentActiveTutorialObject == null)
		{
			if (CheckIsInStables())
			{
				UserNotifyStableTutorial.pInstance.CheckForNextTut();
			}
			else if (!mInstance.IsShowingTutorial() && !mInstance.TutorialComplete())
			{
				mInstance.ShowTutorial();
				mWaitForOtherTut = false;
			}
		}
		else
		{
			mWaitForOtherTut = true;
		}
	}

	public void OnStoreClosed()
	{
		StartTutorial();
	}
}
