using UnityEngine;

public class UiMissionTaskTimer : KAUI
{
	public float _StartFlashTime = 10f;

	public Color _NormalTextColor = Color.white;

	public Color _FlashTextColor = Color.red;

	public LocaleString _AbortTaskTitleText = new LocaleString("Abort Task");

	public LocaleString _AbortTaskText = new LocaleString("Do you want to abort the task?");

	public GameObject _MessageObject;

	private KAWidget mTxtTaskTimer;

	private KAWidget mAbortBtn;

	private AvAvatarState mCachedAvatarState;

	private bool mFlashStarted;

	private string mLevel = string.Empty;

	protected override void Start()
	{
		base.Start();
		mTxtTaskTimer = FindItem("TxtMissionTaskTimer");
		mAbortBtn = FindItem("AbortBtn");
		SetVisibility(inVisible: false);
	}

	protected override void Update()
	{
		base.Update();
		if (!(MissionManager.pInstance == null))
		{
			Task activeTimerTask = GetActiveTimerTask();
			if (CheckMissionTimerTask(activeTimerTask) && activeTimerTask != null)
			{
				SetTimer(activeTimerTask.pRemainingTime);
			}
		}
	}

	private void SetTimer(float inTime)
	{
		if (!mFlashStarted && inTime < _StartFlashTime)
		{
			mFlashStarted = true;
			mTxtTaskTimer.GetLabel().color = _FlashTextColor;
			mTxtTaskTimer.PlayAnim("Flash");
		}
		if (inTime <= 0f)
		{
			mTxtTaskTimer.PlayAnim("Normal");
		}
		mTxtTaskTimer.SetText(GetTimeFormat(inTime));
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mAbortBtn)
		{
			mCachedAvatarState = AvAvatar.pState;
			AvAvatar.pState = AvAvatarState.PAUSED;
			if (MissionManager.pInstance.pIsForceUpdateTimedTask)
			{
				MissionManager.pInstance.SetTimedTaskUpdate(inState: false, inForceUpdate: true);
			}
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _AbortTaskText.GetLocalizedString(), _AbortTaskTitleText.GetLocalizedString(), base.gameObject, "AbortTimedTask", "SetBackAvatarState", null, null, inDestroyOnClick: true);
		}
	}

	private void SetBackAvatarState()
	{
		AvAvatar.pState = mCachedAvatarState;
		if (MissionManager.pInstance.pIsForceUpdateTimedTask)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true, inForceUpdate: true);
		}
	}

	private void AbortTimedTask()
	{
		Task activeTimerTask = GetActiveTimerTask();
		SetBackAvatarState();
		if (MissionManager.pInstance.pIsForceUpdateTimedTask)
		{
			MissionManager.pInstance.SetTimedTaskUpdate(inState: true);
		}
		if (_MessageObject != null && activeTimerTask != null && activeTimerTask.pIsTimedTask && activeTimerTask.GetObjectiveValue<string>("Level") == mLevel)
		{
			_MessageObject.SendMessage("AbortTimerTask", SendMessageOptions.DontRequireReceiver);
		}
		MissionManager.pInstance.AbortTimedTask();
	}

	private Task GetActiveTimerTask()
	{
		if (MissionManager.pInstance.pActiveChaseTask != null && MissionManager.pInstance.pActiveChaseTask._Active)
		{
			if (mAbortBtn.GetVisibility())
			{
				ResetFlash();
				mAbortBtn.SetVisibility(inVisible: false);
			}
			return MissionManager.pInstance.pActiveChaseTask;
		}
		if (MissionManager.pInstance.pActiveTimedTask != null && MissionManager.pInstance.pActiveTimedTask._Active)
		{
			if (!mAbortBtn.GetVisibility())
			{
				ResetFlash();
				mAbortBtn.SetVisibility(inVisible: true);
			}
			return MissionManager.pInstance.pActiveTimedTask;
		}
		return null;
	}

	private bool CheckMissionTimerTask(Task inTask)
	{
		if (inTask == null || inTask.Failed)
		{
			if (GetVisibility())
			{
				SetVisibility(inVisible: false);
			}
			return false;
		}
		if (!GetVisibility())
		{
			ResetFlash();
			SetVisibility(inVisible: true);
		}
		return true;
	}

	private void ResetFlash()
	{
		mFlashStarted = false;
		mTxtTaskTimer.GetLabel().color = _NormalTextColor;
		mTxtTaskTimer.PlayAnim("Normal");
	}

	public bool IsMissionTimedGame(string inGameName, string inLevel = "")
	{
		if (MissionManager.pInstance != null && MissionManager.pInstance.pActiveTimedTask != null)
		{
			Task pActiveTimedTask = MissionManager.pInstance.pActiveTimedTask;
			if (inGameName == pActiveTimedTask.GetObjectiveValue<string>("Name") && RsResourceManager.pCurrentLevel == pActiveTimedTask.GetObjectiveValue<string>("Scene"))
			{
				if (!string.IsNullOrEmpty(inLevel) && inLevel != pActiveTimedTask.GetObjectiveValue<string>("Level"))
				{
					return false;
				}
				mLevel = inLevel;
				return true;
			}
		}
		return false;
	}

	public string GetTimeFormat(float displayTime)
	{
		int num = (int)(displayTime / 60f);
		int num2 = (int)(displayTime % 60f);
		return $" {num:00}" + ":" + $"{num2:00}";
	}
}
