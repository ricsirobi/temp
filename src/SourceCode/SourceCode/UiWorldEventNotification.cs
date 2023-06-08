using System;
using System.Collections.Generic;
using UnityEngine;

public class UiWorldEventNotification : KAUI
{
	[Serializable]
	public class SceneInfo
	{
		public string _SceneName;

		public LocaleString _DisplayName;
	}

	public SceneInfo[] _SceneInfos;

	public static UiWorldEventNotification pInstance;

	private string mLevelToLoad;

	public LocaleString _AboutToBeginAnouncementText = new LocaleString("A world event is about to begin in");

	public LocaleString _EventRunningAnouncementText = new LocaleString("A world event has started");

	public LocaleString _EventLocationText = new LocaleString("Go to [[place]]");

	public LocaleString _AboutToBeginText = new LocaleString("World event is about to begin in [[time]] at [[place]]");

	public LocaleString _EventRunningText = new LocaleString("There is an event going on at the [[place]]");

	[HideInInspector]
	public string _InstructionsString;

	public static string _PrefabName = "PfUiWorldEventNotification";

	public string _AmbMusicPool = "AmbMusic_Pool";

	public AudioClip _TimerNotifySFX;

	public List<string> _EventSceneList = new List<string>();

	public float _AnnouncementMsgDuration = 4f;

	private float mAnnouncementTimer;

	public float _SnoozeBefore = 30f;

	public KAWidget _TextHeader;

	public KAWidget _TextMessage;

	public KAWidget _TextMessageFull;

	public KAWidget _TextInstructions;

	public KAWidget _MessageLocation;

	public KAWidget _TxtTimer;

	public Color _TimerNormalColor = Color.green;

	public Color _TimerWarningColor = Color.yellow;

	[HideInInspector]
	public string _EventKey;

	private bool mCountDownSndPlayed;

	private bool mCountDownSndStopped;

	private KAWidget mExpandBtn;

	private KAWidget mMinimizeBtn;

	public GameObject _PanelObject;

	public KAWidget _AnnouncementText;

	public GameObject _MessageObject;

	private float mExpireTime;

	private float mTimeToEvent;

	private DateTime mTimeToStart;

	private DateTime mEventEndTime;

	private bool mWarned;

	private bool mEventStarted;

	private UITweener mPanelTweener;

	private UiToolbar mUIToolbar;

	protected override void Start()
	{
		if (AvAvatar.pToolbar != null)
		{
			mUIToolbar = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		}
		if (mUIToolbar == null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		base.Start();
		mExpandBtn = FindItem("ExpandBtn");
		mMinimizeBtn = FindItem("MinimizeBtn");
		mAnnouncementTimer = float.NegativeInfinity;
		mPanelTweener = _PanelObject.GetComponent<UITweener>();
	}

	private string GetSceneDisplayName(string inSceneName)
	{
		SceneInfo[] sceneInfos = _SceneInfos;
		foreach (SceneInfo sceneInfo in sceneInfos)
		{
			if (sceneInfo._SceneName == inSceneName)
			{
				return sceneInfo._DisplayName.GetLocalizedString();
			}
		}
		return null;
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (mMinimizeBtn == inWidget)
		{
			mExpandBtn.SetVisibility(inVisible: true);
			mMinimizeBtn.SetVisibility(inVisible: false);
			if (mPanelTweener != null)
			{
				mPanelTweener.Play(forward: false);
			}
		}
		else if (mExpandBtn == inWidget)
		{
			mExpandBtn.SetVisibility(inVisible: false);
			mMinimizeBtn.SetVisibility(inVisible: true);
			if (mPanelTweener != null)
			{
				mPanelTweener.Play(forward: true);
			}
		}
		else if ("BtnAccept" == inWidget.name)
		{
			AvAvatar.SetActive(inActive: false);
			RsResourceManager.LoadLevel(mLevelToLoad);
		}
		else if ("BtnClose" == inWidget.name)
		{
			CloseUI();
		}
	}

	private void UpdateAnnouncement()
	{
		if (null != _AnnouncementText && _AnnouncementText.GetVisibility() && mAnnouncementTimer > 0.01f)
		{
			mAnnouncementTimer -= Time.deltaTime;
			if (mAnnouncementTimer < 0.01f)
			{
				_AnnouncementText.SetVisibility(inVisible: false);
				mAnnouncementTimer = float.NegativeInfinity;
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (RsResourceManager.pLevelLoadingScreen)
		{
			CloseUI();
		}
		if (null == MMOTimeManager.pInstance || !MMOTimeManager.pInstance.pIsTimeSynced)
		{
			return;
		}
		UpdateAnnouncement();
		DateTime serverDateTimeMilliseconds = MMOTimeManager.pInstance.GetServerDateTimeMilliseconds();
		mTimeToEvent = (float)(mTimeToStart - serverDateTimeMilliseconds).TotalSeconds;
		if (RsResourceManager.pCurrentLevel == mLevelToLoad)
		{
			mExpireTime = mTimeToEvent;
		}
		else
		{
			mExpireTime = (float)(mEventEndTime - serverDateTimeMilliseconds).TotalSeconds;
		}
		if (mExpireTime <= 0f)
		{
			CloseUI();
		}
		if (mEventStarted)
		{
			return;
		}
		if (!mCountDownSndPlayed && mTimeToEvent > 0f)
		{
			SnChannel.PausePool(_AmbMusicPool);
			SnChannel.Play(_TimerNotifySFX, "Music_Pool", inForce: true);
			mCountDownSndPlayed = true;
		}
		string text;
		if (mTimeToEvent <= _SnoozeBefore)
		{
			text = UtUtilities.GetKAUIColorString(_TimerNormalColor) + UtUtilities.GetTimerString((int)mTimeToEvent) + "[-]";
			if (mTimeToEvent < 0f)
			{
				if (!mCountDownSndStopped)
				{
					if (!_EventSceneList.Contains(RsResourceManager.pCurrentLevel))
					{
						SnChannel.StopPool("Music_Pool");
						SnChannel.PlayPool(_AmbMusicPool);
					}
					mCountDownSndStopped = false;
				}
				KAWidget txtTimer = _TxtTimer;
				if (null != txtTimer)
				{
					txtTimer.SetVisibility(inVisible: false);
				}
				string text2 = _EventRunningText.GetLocalizedString().Replace("[[place]]", GetSceneDisplayName(mLevelToLoad));
				SetText(text2);
				mEventStarted = true;
				if (null != _TextInstructions)
				{
					_TextInstructions.SetVisibility(inVisible: false);
				}
			}
			if (!mWarned)
			{
				mWarned = true;
				mExpandBtn.SetVisibility(inVisible: false);
				mMinimizeBtn.SetVisibility(inVisible: true);
				if (RsResourceManager.pCurrentLevel != mLevelToLoad)
				{
					KAWidget kAWidget = FindItem("BtnAccept");
					if (null != kAWidget)
					{
						kAWidget.SetVisibility(inVisible: true);
					}
				}
				if (mPanelTweener != null)
				{
					mPanelTweener.Play(forward: true);
				}
			}
		}
		else
		{
			text = UtUtilities.GetKAUIColorString(_TimerWarningColor) + UtUtilities.GetTimerString((int)mTimeToEvent) + "[-]";
		}
		if (_TxtTimer != null)
		{
			_TxtTimer.SetText(text);
		}
		if (_TextInstructions != null)
		{
			_TextInstructions.SetText(_InstructionsString);
		}
	}

	public void CloseUI()
	{
		pInstance = null;
		UnityEngine.Object.Destroy(base.gameObject);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnAlertClose", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	private void HideAnnouncementMsg()
	{
		if (null != _AnnouncementText)
		{
			_AnnouncementText.SetVisibility(inVisible: false);
		}
	}

	public void ShowAnnouncementMsg(string inMessage)
	{
		if (null != _AnnouncementText)
		{
			_AnnouncementText.SetText(inMessage);
			_AnnouncementText.SetVisibility(inVisible: true);
		}
		CancelInvoke("HideAnnouncementMsg");
		Invoke("HideAnnouncementMsg", _AnnouncementMsgDuration);
	}

	public void SetText(string inText)
	{
		if (_TxtTimer != null && _TxtTimer.GetVisibility())
		{
			if (null != _TextMessage)
			{
				_TextMessage.SetInteractive(isInteractive: true);
				_TextMessage.SetVisibility(inVisible: true);
				_TextMessage.SetText(inText);
			}
			if (null != _TextMessageFull)
			{
				_TextMessageFull.SetVisibility(inVisible: false);
			}
		}
		else
		{
			if (null != _TextMessageFull)
			{
				_TextMessageFull.SetInteractive(isInteractive: true);
				_TextMessageFull.SetVisibility(inVisible: true);
				_TextMessageFull.SetText(inText);
			}
			if (null != _TextMessage)
			{
				_TextMessage.SetVisibility(inVisible: false);
			}
		}
	}

	public void SetPosition(float posX, float posY)
	{
		base.transform.position += new Vector3(posX, posY, 0f);
	}

	private static bool ShowNotification(string inEventKey, string inWhichScene, DateTime timeToStart, DateTime eventEndTime, float inTimeToEvent, float inExpiryTime = 5f)
	{
		if (pInstance != null)
		{
			pInstance.CloseUI();
		}
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources(_PrefabName);
		if (null == gameObject)
		{
			return false;
		}
		UiWorldEventNotification uiWorldEventNotification = (pInstance = UnityEngine.Object.Instantiate(gameObject).GetComponent<UiWorldEventNotification>());
		if (AvAvatar.pToolbar != null)
		{
			pInstance.transform.parent = AvAvatar.pToolbar.transform;
		}
		uiWorldEventNotification.mLevelToLoad = inWhichScene;
		uiWorldEventNotification._EventKey = inEventKey;
		uiWorldEventNotification.mExpireTime = inExpiryTime;
		uiWorldEventNotification.mTimeToEvent = inTimeToEvent;
		uiWorldEventNotification.mTimeToStart = timeToStart;
		uiWorldEventNotification.mEventEndTime = eventEndTime;
		KAWidget kAWidget = uiWorldEventNotification.FindItem("BtnAccept");
		if (null != kAWidget)
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		string localizedString = pInstance._AboutToBeginText.GetLocalizedString();
		string localizedString2 = pInstance._EventLocationText.GetLocalizedString();
		if (inTimeToEvent <= 0f)
		{
			localizedString = pInstance._EventRunningText.GetLocalizedString();
		}
		if (inTimeToEvent < 0f)
		{
			KAWidget txtTimer = uiWorldEventNotification._TxtTimer;
			if (null != txtTimer)
			{
				txtTimer.SetVisibility(inVisible: false);
			}
			if (null != uiWorldEventNotification._TextInstructions)
			{
				uiWorldEventNotification._TextInstructions.SetVisibility(inVisible: false);
			}
			uiWorldEventNotification.mEventStarted = true;
			uiWorldEventNotification.ShowAnnouncementMsg(uiWorldEventNotification._EventRunningAnouncementText.GetLocalizedString() + " " + pInstance.GetSceneDisplayName(inWhichScene));
		}
		else
		{
			uiWorldEventNotification.ShowAnnouncementMsg(uiWorldEventNotification._AboutToBeginAnouncementText.GetLocalizedString() + " " + pInstance.GetSceneDisplayName(inWhichScene));
		}
		if (inTimeToEvent <= uiWorldEventNotification._SnoozeBefore)
		{
			uiWorldEventNotification.mWarned = true;
			if (RsResourceManager.pCurrentLevel != inWhichScene)
			{
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		bool visibility = true;
		if (inTimeToEvent < 0f || RsResourceManager.pCurrentLevel == inWhichScene)
		{
			visibility = false;
		}
		if (null != uiWorldEventNotification._TextInstructions)
		{
			uiWorldEventNotification._TextInstructions.SetVisibility(visibility);
		}
		uiWorldEventNotification._InstructionsString = localizedString2;
		uiWorldEventNotification._InstructionsString = uiWorldEventNotification._InstructionsString.Replace("[[place]]", pInstance.GetSceneDisplayName(inWhichScene));
		string text = localizedString.Replace("[[place]]", pInstance.GetSceneDisplayName(inWhichScene));
		uiWorldEventNotification.SetText(text);
		return true;
	}

	public static void Show(DateTime timeToStart, DateTime eventEndTime, float inTimeToEvent, float inEvenDuration, string inEventKey, string inWhichScene)
	{
		string inWhichScene2 = inWhichScene.Trim();
		if (pInstance == null || pInstance._EventKey != inEventKey)
		{
			ShowNotification(inEventKey, inWhichScene2, timeToStart, eventEndTime, inTimeToEvent, inTimeToEvent + inEvenDuration);
		}
	}
}
