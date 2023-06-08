using System;
using SOD.Event;
using UnityEngine;

public class UiEventIntro : KAUI
{
	public delegate void ClickEvent(string buttonName);

	public delegate void OnLoaded(UiEventIntro inUiEventIntro);

	public Action OnClosed;

	public string _EventName;

	private KAButton mProgressionBtn;

	private KAButton mHelpBtn;

	private KAButton mCloseBtn;

	private KAButton mCloseIcn;

	private KAWidget mTxtTimer;

	private UiHelpScreen mUiHelpScreen;

	[HideInInspector]
	public UserNotifyEvent m_UserNotifyEvent;

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		mProgressionBtn = (KAButton)FindItem("ProgressionBtn");
		mHelpBtn = (KAButton)FindItem("HelpBtn");
		mCloseBtn = (KAButton)FindItem("CloseBtn");
		mCloseIcn = (KAButton)FindItem("CloseIcn");
		mTxtTimer = FindItem("TxtEventTimer");
		DisplayEndDate();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mProgressionBtn || inWidget == mHelpBtn)
		{
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle((inWidget == mProgressionBtn) ? EventManager.Get(_EventName)._AssetName : EventManager.Get(_EventName)._HelpAssetName, OnBundleLoaded, typeof(GameObject), inDontDestroy: false, typeof(GameObject));
			KAUI.RemoveExclusive(this);
			if (inWidget == mProgressionBtn)
			{
				UnityEngine.Object.Destroy(base.gameObject);
			}
			else
			{
				SetVisibility(inVisible: false);
			}
		}
		else if (inWidget == mCloseBtn || inWidget == mCloseIcn)
		{
			OnClose();
		}
	}

	protected virtual void OnClose()
	{
		OnClosed();
		AvAvatar.pState = AvAvatarState.IDLE;
		KAUI.RemoveExclusive(this);
		if ((bool)mUiHelpScreen)
		{
			UnityEngine.Object.Destroy(mUiHelpScreen.gameObject);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void DisplayEndDate()
	{
		if (mTxtTimer != null && mTxtTimer.GetLabel() != null)
		{
			int num = ((EventManager.Get(_EventName) != null) ? EventManager.Get(_EventName).GetRemainingDays() : 0);
			mTxtTimer.SetText(mTxtTimer.GetLabel().text.Replace("{#}", num.ToString()));
		}
	}

	private void OnScreenLoaded(GameObject inObject)
	{
		UiPrizeProgression component = inObject.GetComponent<UiPrizeProgression>();
		if ((bool)component)
		{
			component.m_UserNotifyEvent = m_UserNotifyEvent;
			component.OnClosed = (Action)Delegate.Combine(component.OnClosed, new Action(m_UserNotifyEvent.MarkUserNotifyDone));
			return;
		}
		mUiHelpScreen = inObject.GetComponent<UiHelpScreen>();
		if ((bool)mUiHelpScreen)
		{
			mUiHelpScreen.OnClicked += OnHelpClicked;
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			KAUI.SetExclusive(mUiHelpScreen);
		}
	}

	public void OnHelpClicked(string buttonName)
	{
		if (buttonName == "Back")
		{
			mUiHelpScreen.OnClicked -= OnHelpClicked;
			UnityEngine.Object.Destroy(mUiHelpScreen);
			SetVisibility(inVisible: true);
			KAUI.SetExclusive(this);
		}
		else if (buttonName == "Exit")
		{
			mUiHelpScreen.OnClicked -= OnHelpClicked;
			UnityEngine.Object.Destroy(mUiHelpScreen);
			OnClose();
		}
	}

	private void OnBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = ((GameObject)inObject).name;
			OnScreenLoaded(gameObject);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			((OnLoaded)inUserData)?.Invoke(null);
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Event Intro Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}
}
