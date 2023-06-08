using System;
using UnityEngine;

public class UiWEScoutAttack : KAUI
{
	public float _AnnouncementMessageDuration = 3f;

	public KAWidget _TextTimeLeft;

	public KAWidget _TextHealth;

	public AudioClip _WarnSFX;

	public float _WarnTime = 15f;

	private float mHealthBarDefaultValue = 1f;

	private KAWidget mExpandBtn;

	private KAWidget mMinimizeBtn;

	public KAWidget _PanelObject;

	public GameObject _MessageObject;

	private DateTime mEndTime;

	private bool mWarnSndPlayed;

	private float mSnoozeBefore;

	private KAWidget mHealthBar;

	private KAWidget mAnnounceTxt;

	private UITweener mPanelTweener;

	protected override void Start()
	{
		base.Start();
		mExpandBtn = FindItem("ExpandBtn");
		mMinimizeBtn = FindItem("MinimizeBtn");
		mHealthBar = FindItem("HealthBar");
		mAnnounceTxt = FindItem("AccounceTxt");
		mPanelTweener = _PanelObject.gameObject.GetComponent<UITweener>();
		SetEventTrackerVisiblity(show: false);
		if (mPanelTweener != null)
		{
			mPanelTweener.Play(forward: false);
		}
		ShowAnnouncementMessage("", show: false);
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (mMinimizeBtn.name == inWidget.name)
		{
			mExpandBtn.SetVisibility(inVisible: true);
			mMinimizeBtn.SetVisibility(inVisible: false);
			if (mPanelTweener != null)
			{
				mPanelTweener.Play(forward: false);
			}
		}
		else if (mExpandBtn.name == inWidget.name)
		{
			mExpandBtn.SetVisibility(inVisible: false);
			mMinimizeBtn.SetVisibility(inVisible: true);
			if (mPanelTweener != null)
			{
				mPanelTweener.Play(forward: true);
			}
		}
	}

	public override void OnSlideEnd()
	{
		base.OnSlideEnd();
	}

	protected override void Update()
	{
		base.Update();
		if (null != MMOTimeManager.pInstance && MMOTimeManager.pInstance.pIsTimeSynced)
		{
			float num = (float)(mEndTime - MMOTimeManager.pInstance.GetServerDateTimeMilliseconds()).TotalSeconds;
			if (!mWarnSndPlayed && Math.Floor(num) == (double)_WarnTime)
			{
				SnChannel.Play(_WarnSFX);
				mWarnSndPlayed = true;
			}
			SetTime((num < 0f) ? 0f : num);
		}
		if (!AvAvatar.pInputEnabled || AvAvatar.pState == AvAvatarState.PAUSED || (AvAvatar.pToolbar != null && !AvAvatar.pToolbar.activeInHierarchy))
		{
			SetVisibility(inVisible: false);
		}
		else
		{
			SetVisibility(inVisible: true);
		}
		if (_PanelObject.GetVisibility() && mMinimizeBtn.GetVisibility())
		{
			OnClick(mMinimizeBtn);
		}
	}

	public void SetTime(float inDuration)
	{
		if (null != _TextTimeLeft)
		{
			string timerString = UtUtilities.GetTimerString((int)inDuration);
			_TextTimeLeft.SetText(timerString);
		}
	}

	public void UpdateHealth(string inHealth, float inHealthVal)
	{
		if (null != _TextHealth)
		{
			_TextHealth.SetText(inHealth);
		}
		mHealthBar.SetProgressLevel(inHealthVal);
	}

	public void ResetHealthBar()
	{
		mHealthBar.SetProgressLevel(mHealthBarDefaultValue);
	}

	public void SetPosition(float posX, float posY)
	{
		base.transform.position += new Vector3(posX, posY, 0f);
	}

	public void ShowEventTracker(DateTime EndTime)
	{
		ResetHealthBar();
		SetEventTrackerVisiblity(show: true);
		mEndTime = EndTime;
		mWarnSndPlayed = false;
		if (mPanelTweener != null)
		{
			mPanelTweener.Play(forward: true);
		}
	}

	public void SetEventTrackerVisiblity(bool show)
	{
		if (null != _PanelObject)
		{
			_PanelObject.SetVisibility(show);
		}
	}

	public void ShowAnnouncementMessage(string inMessage, bool show)
	{
		if (mAnnounceTxt != null)
		{
			mAnnounceTxt.SetText(inMessage);
			mAnnounceTxt.SetVisibility(show);
		}
		Invoke("HideAnnouncement", _AnnouncementMessageDuration);
	}

	private void HideAnnouncement()
	{
		if (mAnnounceTxt != null)
		{
			mAnnounceTxt.SetVisibility(inVisible: false);
		}
	}

	public bool IsEventEndTxtVisible()
	{
		if (mAnnounceTxt == null)
		{
			return false;
		}
		return mAnnounceTxt.GetVisibility();
	}
}
