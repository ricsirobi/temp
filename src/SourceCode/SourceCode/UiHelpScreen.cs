using System;
using System.Collections.Generic;
using SOD.Event;
using UnityEngine;

public class UiHelpScreen : KAUI
{
	[Serializable]
	public class HelpButtons
	{
		public KAButton _Button;

		public HelpAction _Action;

		public string _SceneName;
	}

	public enum HelpAction
	{
		LoadScene,
		DailyQuest,
		Exchange
	}

	public delegate void ClickEvent(string buttonName);

	[Header("Pages")]
	public List<KAWidget> _Pages;

	[Header("SceneButtons")]
	public List<HelpButtons> _HelpButtons;

	[Header("Analytics")]
	public string _HelpEvent = "Dreadfall2019_EventHelpUI";

	[Header("Daily Quest")]
	public int _MissionGroupID = 55;

	public string _MissionBoardAsset = "RS_DATA/PfUiDailyQuestDO.unity3d/PfUiDailyQuestDO";

	public string _EventName;

	private KAUIMenu mPageMenu;

	private KAButton mNextArrow;

	private KAButton mPrevArrow;

	private KAButton mCloseBtn;

	private KAButton mBackBtn;

	private int mPageIndex;

	public event ClickEvent OnClicked;

	protected override void Start()
	{
		base.Start();
		mNextArrow = (KAButton)FindItem("ArrowRight");
		mPrevArrow = (KAButton)FindItem("ArrowLeft");
		mCloseBtn = (KAButton)FindItem("CloseBtn");
		mBackBtn = (KAButton)FindItem("BtnBack");
		mPageMenu = _MenuList[0];
		CreatePageItems();
		UpdatePage();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mNextArrow)
		{
			mPageIndex++;
			UpdatePage();
		}
		else if (inWidget == mPrevArrow)
		{
			mPageIndex += -1;
			UpdatePage();
		}
		else if (inWidget == mCloseBtn)
		{
			Close();
		}
		else if (inWidget == mBackBtn)
		{
			Back();
		}
		else
		{
			ProcessClick(inWidget);
		}
	}

	private void UpdatePage()
	{
		if (mPageIndex > _Pages.Count - 1)
		{
			mPageIndex = 0;
		}
		else if (mPageIndex < 0)
		{
			mPageIndex = _Pages.Count - 1;
		}
		for (int i = 0; i < _Pages.Count; i++)
		{
			_Pages[i].SetVisibility(i == mPageIndex);
		}
		if (mPageMenu != null && mPageMenu.GetItems().Count > 0)
		{
			mPageMenu.OnSelect(mPageMenu.GetItems()[mPageIndex], inSelected: true);
		}
	}

	private void Back()
	{
		SetVisibility(inVisible: false);
		this.OnClicked?.Invoke("Back");
	}

	private void Close()
	{
		this.OnClicked?.Invoke("Exit");
	}

	private void ProcessClick(KAWidget inWidget)
	{
		if (mPageMenu.GetItems().Contains(inWidget) && int.TryParse(inWidget.name, out mPageIndex))
		{
			UpdatePage();
		}
		HelpButtons helpButtons = _HelpButtons.Find((HelpButtons x) => x._Button == inWidget);
		if (helpButtons == null)
		{
			return;
		}
		switch (helpButtons._Action)
		{
		case HelpAction.LoadScene:
			if (!string.IsNullOrEmpty(helpButtons._SceneName))
			{
				Close();
				AvAvatar.SetActive(inActive: false);
				RsResourceManager.LoadLevel(helpButtons._SceneName);
			}
			break;
		case HelpAction.DailyQuest:
			ShowDailyQuest();
			break;
		case HelpAction.Exchange:
			ShowExchangeUi();
			break;
		}
	}

	private void CreatePageItems()
	{
		mPageMenu.ClearItems();
		for (int i = 0; i < _Pages.Count; i++)
		{
			KAWidget kAWidget = DuplicateWidget(mPageMenu._Template);
			kAWidget.SetVisibility(inVisible: true);
			kAWidget.name = i.ToString();
			mPageMenu.AddWidget(kAWidget);
		}
	}

	private void ShowDailyQuest()
	{
		if (MissionManager.pInstance.pDailyMissionStateResult != null && MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement != null && MissionManager.pInstance.pDailyMissionStateResult.UserTimedAchievement.Count > 0 && !string.IsNullOrEmpty(_MissionBoardAsset))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _MissionBoardAsset.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnDailyQuestUILoaded, typeof(GameObject));
		}
	}

	private void OnDailyQuestUILoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			UiDailyQuests.pMissionGroup = _MissionGroupID;
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = "PfUiDailyQuestDO";
			obj.GetComponent<UiDailyQuests>().RegisterCloseEvent();
			KAUICursorManager.SetDefaultCursor("Arrow");
			Close();
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void ShowExchangeUi()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(EventManager.Get(_EventName)?._ExchangeAssetName, OnExchangeUiLoaded, typeof(GameObject));
	}

	private void OnExchangeUiLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		if (inEvent == RsResourceLoadEvent.COMPLETE)
		{
			Close();
			UnityEngine.Object.Instantiate((GameObject)inObject);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		else
		{
			UtDebug.LogError("Failed to load Exchange asset");
		}
	}
}
