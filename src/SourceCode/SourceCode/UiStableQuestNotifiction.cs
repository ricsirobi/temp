using UnityEngine;

public class UiStableQuestNotifiction : KAUI
{
	public float _NotificationSelfDestructTimeInSeconds = 5f;

	private KAWidget mMinimizeBtn;

	private KAWidget mGoToStablesBtn;

	private bool mHasPoppedBack;

	private UITweener mPanelTweener;

	public GameObject _PanelObject;

	protected override void Start()
	{
		base.Start();
		mMinimizeBtn = FindItem("MinimizeBtn");
		mGoToStablesBtn = FindItem("StableQuestBtn");
		mPanelTweener = _PanelObject.GetComponent<UITweener>();
		mPanelTweener.eventReceiver = base.gameObject;
		mPanelTweener.callWhenFinished = "OnMinimized";
		if (mPanelTweener != null)
		{
			mPanelTweener.Play(forward: false);
		}
	}

	public void PopIn(string message, float selfDestructTime)
	{
		mHasPoppedBack = false;
		mMinimizeBtn.SetVisibility(inVisible: true);
		mGoToStablesBtn.SetVisibility(TimedMissionManager.pInstance.pInteractiveNotificationLevel);
		FindItem("TxtInfoMissionStatus").SetText(message);
		if (mPanelTweener != null)
		{
			mPanelTweener.Play(forward: true);
		}
		Invoke("PopOut", selfDestructTime);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (mMinimizeBtn == inWidget)
		{
			CancelInvoke("PopOut");
			PopOut();
		}
		else if (inWidget == mGoToStablesBtn)
		{
			StableManager.LoadStableWithJobBoard(0);
		}
	}

	public void PopOut()
	{
		mMinimizeBtn.SetVisibility(inVisible: false);
		if (mPanelTweener != null)
		{
			mPanelTweener.Play(forward: false);
		}
	}

	private void OnMinimized(UITweener tween)
	{
		if (!mMinimizeBtn.GetVisibility())
		{
			mHasPoppedBack = true;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (TimedMissionManager.pInstance.pValidNotificationLevel && TimedMissionManager.pInstance.GetNextNotification() != null && mHasPoppedBack)
		{
			PopIn(TimedMissionManager.pInstance.GetNextNotification(), _NotificationSelfDestructTimeInSeconds);
			TimedMissionManager.pInstance.RemoveNotification();
		}
	}
}
