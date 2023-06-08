using System;

public class UserNotifyStableTutorial : UserNotify
{
	public StableQuestTutorial _StableQuestTutorial;

	public StableIncubatorTutorial _StableIncubatorTutorial;

	public StableTutorial _StableMoveTutorial;

	public StableHatchTutorial _StableHatchTutorial;

	private static UserNotifyStableTutorial mInstance;

	private bool mInitialized;

	private bool mWaitForPendingTasks;

	private bool mIsShowingStableUi;

	public static UserNotifyStableTutorial pInstance
	{
		get
		{
			return mInstance;
		}
		set
		{
			mInstance = value;
		}
	}

	public override void OnWaitBeginImpl()
	{
		mInitialized = true;
		WsUserMessage.pBlockMessages = true;
		CheckForNextTut();
	}

	private void OnStableUILoad(bool isSuccess)
	{
		mIsShowingStableUi = true;
	}

	public void OnStableUIClose()
	{
		mIsShowingStableUi = false;
	}

	public void OnDestroy()
	{
		WsUserMessage.pBlockMessages = false;
		UiDragonsStable.pOnStablesUILoadHandler = (OnStablesUILoad)Delegate.Remove(UiDragonsStable.pOnStablesUILoadHandler, new OnStablesUILoad(OnStableUILoad));
		UiDragonsStable.pOnStablesUIClosed = (OnStablesUIClosed)Delegate.Remove(UiDragonsStable.pOnStablesUIClosed, new OnStablesUIClosed(OnStableUIClose));
	}

	private void Start()
	{
		UiDragonsStable.pOnStablesUILoadHandler = (OnStablesUILoad)Delegate.Combine(UiDragonsStable.pOnStablesUILoadHandler, new OnStablesUILoad(OnStableUILoad));
		UiDragonsStable.pOnStablesUIClosed = (OnStablesUIClosed)Delegate.Combine(UiDragonsStable.pOnStablesUIClosed, new OnStablesUIClosed(OnStableUIClose));
		mInstance = this;
	}

	private void Update()
	{
		if (mInitialized && CommonInventoryData.pIsReady)
		{
			CheckForNextTut();
		}
	}

	public void SetToWaitState(bool flag)
	{
		mWaitForPendingTasks = flag;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.enabled = !flag;
		}
	}

	private new void OnWaitEnd()
	{
		WsUserMessage.pBlockMessages = false;
		ObClickable.pGlobalActive = true;
		KAUICursorManager.SetDefaultCursor("Arrow");
		AvAvatar.pState = AvAvatarState.IDLE;
		base.OnWaitEnd();
	}

	public void StartHatchTutorial()
	{
		if (!(_StableHatchTutorial == null))
		{
			StableHatchTutorial stableHatchTutorial = _StableHatchTutorial;
			if (!stableHatchTutorial.IsShowingTutorial() && !stableHatchTutorial.TutorialComplete())
			{
				stableHatchTutorial.gameObject.SetActive(value: true);
				stableHatchTutorial.ShowTutorial();
				SetToWaitState(flag: true);
			}
		}
	}

	public void CheckForNextTut()
	{
		if (!(KAUIStore.pInstance == null) || !(KAUI._GlobalExclusiveUI == null) || AvAvatar.pState == AvAvatarState.PAUSED || mWaitForPendingTasks)
		{
			return;
		}
		bool flag = InteractiveTutManager._CurrentActiveTutorialObject != null;
		if (flag)
		{
			flag = InteractiveTutManager._CurrentActiveTutorialObject.GetComponent<InteractiveTutManager>().IsShowingTutorial();
		}
		if (flag)
		{
			return;
		}
		if (StableManager.IsPlayerHasEggs() && _StableIncubatorTutorial != null && !_StableIncubatorTutorial.TutorialComplete())
		{
			StableIncubatorTutorial stableIncubatorTutorial = _StableIncubatorTutorial;
			stableIncubatorTutorial.gameObject.SetActive(value: true);
			stableIncubatorTutorial.ShowTutorial();
			SetToWaitState(flag: true);
		}
		else if (_StableMoveTutorial != null && !_StableMoveTutorial.TutorialComplete())
		{
			StableTutorial stableMoveTutorial = _StableMoveTutorial;
			stableMoveTutorial.gameObject.SetActive(value: true);
			stableMoveTutorial.ShowTutorial();
			SetToWaitState(flag: true);
		}
		else if (IsMultipleDragonTutPlayable() && !mIsShowingStableUi)
		{
			if (TimedMissionManager.pInstance.pIsEnabled && _StableQuestTutorial != null && !_StableQuestTutorial.TutorialComplete())
			{
				_StableQuestTutorial.gameObject.SetActive(value: true);
				_StableQuestTutorial.ShowTutorial();
				SetToWaitState(flag: true);
			}
			else if (StableManager.pInstance.pStableQuestUIInstance == null)
			{
				OnWaitEnd();
			}
		}
		else if (!mIsShowingStableUi)
		{
			OnWaitEnd();
		}
	}

	private bool IsMultipleDragonTutPlayable()
	{
		if (RaisedPetData.pActivePets != null)
		{
			int num = 0;
			foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
			{
				if (value == null)
				{
					continue;
				}
				RaisedPetData[] array = value;
				foreach (RaisedPetData raisedPetData in array)
				{
					if (raisedPetData.pStage >= RaisedPetStage.BABY && raisedPetData.IsPetCustomized() && StableData.GetByPetID(raisedPetData.RaisedPetID) != null && !SanctuaryManager.IsPetLocked(raisedPetData))
					{
						num++;
					}
				}
			}
			if (num > 1)
			{
				return true;
			}
		}
		return false;
	}

	public void OnStoreClosed()
	{
		CheckForNextTut();
	}
}
