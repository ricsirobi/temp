using System;
using System.Collections.Generic;
using UnityEngine;

public class UiDreadfall : KAUI
{
	[Serializable]
	public class RewardIconMap
	{
		public int _AchievementInfoID;

		public string _IconName;

		public string _AchievementRewardName;
	}

	public delegate void OnExit();

	public GameObject _HelpScreen;

	public KAWidget _ProgressionTxt;

	public string _RedeemUiResourcePath = "RS_DATA/pfUidreadfallredeem.unity3d/PfUiDreadfallRedeem";

	public RewardIconMap[] _RewardIconMap;

	private UiDreadfallProgressionMenu mUiPrizeMenu;

	private KAWidget mCloseBtn;

	private KAWidget mBtnHowToPlay;

	private KAWidget mBtnExchange;

	private KAWidget mEventEndDays;

	private KAWidget mItemQuantityTxt;

	private UiHelpScreen mHelpScreen;

	private ArrayOfItemData mItemDataArray;

	private UserAchievementTaskRedeemableRewards mRedeemableRewards;

	private List<AchievementTaskInfo> mRedeemableRewardsList;

	private UserAchievementTask mAchievementTask;

	private bool mRedeemReady;

	private bool mAchTaskReady;

	private bool mRewardsDisplayed;

	private AvAvatarState mLastAvatarState;

	public static event OnExit OnExitPressed;

	public static void Load()
	{
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("DreadfallPrizeProgressionAsset"), OnBundleLoaded, typeof(GameObject));
	}

	private static void OnBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject).name = ((GameObject)inObject).name;
			break;
		case RsResourceLoadEvent.ERROR:
			UiDreadfall.OnExitPressed?.Invoke();
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Dreadfall Prize Progression Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected void OnEnable()
	{
		UiDreadfallRedeem.RedeemFinished += RedeemFinished;
	}

	protected void OnDisable()
	{
		if (mHelpScreen != null)
		{
			mHelpScreen.OnClicked -= HelpUiButtonClicked;
		}
		UiDreadfallRedeem.RedeemFinished -= RedeemFinished;
	}

	protected override void Start()
	{
		base.Start();
		mCloseBtn = FindItem("CloseBtn");
		mBtnHowToPlay = FindItem("BtnHowToPlay");
		mBtnExchange = FindItem("BtnExchange");
		mEventEndDays = FindItem("TxtTimer");
		mItemQuantityTxt = FindItem("TxtCandyCount");
		mUiPrizeMenu = (UiDreadfallProgressionMenu)_MenuList[0];
		mRedeemReady = false;
		mAchTaskReady = false;
		mRewardsDisplayed = false;
		SetVisibility(inVisible: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		if (DreadfallAchievementManager.pInstance != null)
		{
			DreadfallAchievementManager.pInstance.GetRedeemableRewards(RedeemInfoReady);
			DreadfallAchievementManager.pInstance.GetAchievementTask(AchievementTaskDataReady);
		}
		if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
		{
			mLastAvatarState = AvAvatarState.PAUSED;
			AvAvatar.pState = AvAvatarState.PAUSED;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: false);
			}
		}
		DisplayEndDate();
		DisplayDreadfallItemQuantity();
	}

	protected override void Update()
	{
		base.Update();
		if (mAchTaskReady && mRedeemReady && !mRewardsDisplayed)
		{
			if (mAchievementTask == null)
			{
				KAUICursorManager.SetDefaultCursor("Arrow");
				ExitUi();
				UtDebug.Log("Achievement Task in null");
			}
			else
			{
				mRewardsDisplayed = true;
				ProcessRewardsDisplay();
			}
		}
	}

	private void AchievementTaskDataReady(UserAchievementTask achTask)
	{
		mAchievementTask = achTask;
		mAchTaskReady = true;
	}

	private void RedeemInfoReady(UserAchievementTaskRedeemableRewards redeemableRewards)
	{
		mRedeemableRewards = redeemableRewards;
		mRedeemReady = true;
	}

	private void ProcessRewardsDisplay()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mRedeemableRewards != null && mRedeemableRewards.RedeemableRewards != null && mRedeemableRewards.RedeemableRewards.Length != 0)
		{
			mRedeemableRewardsList = new List<AchievementTaskInfo>();
			mRedeemableRewardsList = GetRedeemableRewards(mRedeemableRewards);
			if (mRedeemableRewardsList.Count > 0)
			{
				ShowRedeemUi();
				return;
			}
		}
		RedeemFinished(null);
	}

	private void DisplayEndDate()
	{
		int num = ((DreadfallAchievementManager.pInstance != null) ? DreadfallAchievementManager.pInstance.GetEventRemainingTime().Days : 0);
		mEventEndDays.SetText(num.ToString());
	}

	private void DisplayDreadfallItemQuantity()
	{
		if (mItemQuantityTxt != null)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(DreadfallAchievementManager.pInstance._DreadfallItemId);
			mItemQuantityTxt.SetText((userItemData != null) ? userItemData.Quantity.ToString() : "0");
		}
	}

	private void ExitUi()
	{
		if (mHelpScreen != null)
		{
			UnityEngine.Object.Destroy(mHelpScreen.gameObject);
		}
		if (mLastAvatarState == AvAvatarState.PAUSED)
		{
			AvAvatar.pState = AvAvatar.pPrevState;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: true);
			}
		}
		KAUI.RemoveExclusive(this);
		UiDreadfall.OnExitPressed?.Invoke();
		UnityEngine.Object.Destroy(base.gameObject);
	}

	public void OpenHelp()
	{
		if (mHelpScreen == null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_HelpScreen);
			mHelpScreen = gameObject.GetComponent<UiHelpScreen>();
			mHelpScreen.OnClicked += HelpUiButtonClicked;
		}
		SetVisibility(inVisible: false);
		mHelpScreen.SetVisibility(inVisible: true);
		KAUI.SetExclusive(mHelpScreen);
	}

	private List<AchievementTaskInfo> GetRedeemableRewards(UserAchievementTaskRedeemableRewards rewards)
	{
		List<AchievementTaskInfo> list = new List<AchievementTaskInfo>();
		if (rewards != null)
		{
			for (int i = 0; i < DreadfallAchievementManager.pInstance.AchievementTaskInfoList.AchievementTaskInfo.Length; i++)
			{
				AchievementTaskInfo info = DreadfallAchievementManager.pInstance.AchievementTaskInfoList.AchievementTaskInfo[i];
				if (DreadfallAchievementManager.pInstance.AchievementVisible(info) && Array.Find(rewards.RedeemableRewards, (UserAchievementTaskRedeemableReward x) => x.AchievementInfoID == info.AchievementInfoID) != null)
				{
					list.Add(info);
				}
			}
		}
		return list;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mCloseBtn)
		{
			ExitUi();
		}
		else if (inWidget == mBtnHowToPlay)
		{
			OpenHelp();
		}
		else if (inWidget == mBtnExchange)
		{
			ExitUi();
			UiDreadfallItemExchange.Load();
		}
	}

	private void ShowRedeemUi()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = _RedeemUiResourcePath.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUiLoaded, typeof(GameObject));
	}

	private void OnUiLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			UiDreadfallRedeemMenu componentInChildren = obj.GetComponentInChildren<UiDreadfallRedeemMenu>();
			obj.GetComponentInChildren<UiDreadfallRedeem>().SetRedeemData(mRedeemableRewardsList);
			componentInChildren.PopulateUi(_RewardIconMap, mRedeemableRewardsList);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			RedeemFinished(null);
			break;
		}
	}

	private void RedeemFinished(List<int> redeemFailList)
	{
		if (DreadfallAchievementManager.pInstance.EventInProgress())
		{
			KAUI.SetExclusive(this);
			SetVisibility(inVisible: true);
			mUiPrizeMenu.Populate(mAchievementTask, redeemFailList);
		}
		else
		{
			ExitUi();
		}
	}

	private void HelpUiButtonClicked(string buttonName)
	{
		if (!(buttonName == "Back"))
		{
			if (buttonName == "Exit")
			{
				ExitUi();
			}
		}
		else
		{
			SetVisibility(inVisible: true);
			KAUI.SetExclusive(this);
		}
	}
}
