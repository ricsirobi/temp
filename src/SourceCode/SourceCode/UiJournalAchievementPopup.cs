using System.Collections.Generic;
using UnityEngine;

public class UiJournalAchievementPopup : KAUIPopup
{
	public class AchievementPopupCoBundleItemData : CoBundleItemData
	{
		public AchievementPopupCoBundleItemData(string tn, string sn)
			: base(tn, sn)
		{
		}

		public override void OnAllDownloaded()
		{
			KAWidget item = GetItem();
			if (item != null)
			{
				string name = item.name;
				base.OnAllDownloaded();
				item.name = name;
			}
		}
	}

	public Vector2 _AwardWidgetPosition = new Vector2(-195f, 94f);

	public Vector3 _AwardWidgetSize = Vector3.one;

	public RewardWidget _RewardWidget;

	public LocaleString _ProgressText = new LocaleString("Progress:");

	public UiAchievements _AchievementUI;

	private KAWidget mTxtDetails;

	private KAWidget mPlayBtn;

	private string mSceneToLaunch = "";

	private string mTeleportToMarker = "";

	private KAWidget mAwardWidget;

	private List<KAWidget> mIconsLoading = new List<KAWidget>();

	protected override void Start()
	{
		base.Start();
		Initialize();
	}

	private void Initialize()
	{
		mTxtDetails = FindItem("TxtDetails");
		mPlayBtn = FindItem("PlayBtn");
	}

	protected override void Update()
	{
		base.Update();
		if (mIconsLoading.Count <= 0)
		{
			return;
		}
		for (int num = mIconsLoading.Count - 1; num >= 0; num--)
		{
			AchievementPopupCoBundleItemData achievementPopupCoBundleItemData = (AchievementPopupCoBundleItemData)mIconsLoading[num].GetUserData();
			if (achievementPopupCoBundleItemData != null && achievementPopupCoBundleItemData.pIsReady)
			{
				mIconsLoading.RemoveAt(num);
			}
		}
		if (mIconsLoading.Count == 0)
		{
			mCloseItem.SetInteractive(isInteractive: true);
		}
	}

	public void Show(bool visibility)
	{
		SetVisibility(visibility);
		if (visibility)
		{
			KAUI.SetExclusive(this);
			return;
		}
		KAUI.RemoveExclusive(this);
		if (mAwardWidget != null)
		{
			RemoveWidget(mAwardWidget);
			mAwardWidget = null;
		}
	}

	public void Show(string title, string msg, UiAchievements.AchievementTitleInfo selectedAch)
	{
		if (mTxtDetails != null)
		{
			mTxtDetails.SetText(msg);
		}
		if (mAwardWidget != null)
		{
			RemoveWidget(mAwardWidget);
			mAwardWidget = null;
		}
		mAwardWidget = DuplicateWidget(selectedAch._Template);
		mAwardWidget.SetScale(_AwardWidgetSize);
		mAwardWidget.SetInteractive(isInteractive: false);
		AddWidget(mAwardWidget, UIAnchor.Side.Center);
		mAwardWidget.SetVisibility(inVisible: true);
		mAwardWidget.transform.localPosition = new Vector3(_AwardWidgetPosition.x, _AwardWidgetPosition.y, 0f);
		_AchievementUI.ShowAwards(mAwardWidget, Mathf.Max(0, selectedAch._MaxRewardCount - selectedAch.pLevel));
		KAWidget kAWidget = FindItem("TxtAwardType");
		if (kAWidget != null)
		{
			kAWidget.SetText(title);
		}
		KAWidget kAWidget2 = FindItem("TxtTrophyName");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(title);
		}
		KAWidget kAWidget3 = FindItem("TxtCounter");
		if (kAWidget3 != null)
		{
			kAWidget3.SetText(_ProgressText.GetLocalizedString() + " " + selectedAch.pCounter + "/" + (selectedAch.pCounter + selectedAch.pCountRemForNext));
		}
		mSceneToLaunch = selectedAch._SceneName;
		mTeleportToMarker = selectedAch._TeleportToMarker;
		if (selectedAch.pLevel >= 4)
		{
			_RewardWidget.ClearWidgetsCreated();
		}
		else
		{
			_RewardWidget.SetRewards(selectedAch.pRewards, MissionManager.pInstance._RewardData);
		}
		Show(visibility: true);
	}

	public void OnLoadItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		KAWidget obj = (KAWidget)inUserData;
		AchievementPopupCoBundleItemData achievementPopupCoBundleItemData = new AchievementPopupCoBundleItemData(dataItem.IconName, "");
		obj.SetUserData(achievementPopupCoBundleItemData);
		achievementPopupCoBundleItemData.LoadResource();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mPlayBtn)
		{
			AvAvatar.SetActive(inActive: false);
			if (RsResourceManager.pCurrentLevel == mSceneToLaunch)
			{
				AvAvatar.TeleportToObject(mTeleportToMarker);
				return;
			}
			AvAvatar.pStartLocation = mTeleportToMarker;
			RsResourceManager.LoadLevel(mSceneToLaunch);
		}
		else if (inWidget == mCloseItem)
		{
			Show(visibility: false);
		}
	}
}
