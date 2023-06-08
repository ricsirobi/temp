using System;
using UnityEngine;

public class UiFarmToolbar : UiMyRoomsInt
{
	public string _ExitLevelName = "HubSchoolDO";

	private KAWidget mTxtCurrentLevel;

	private KAWidget mExpansionBtn;

	protected KAWidget mTxtCreativePointsLabel;

	protected KAWidget mAniCreativePointsProgress;

	private bool mRevertAvatarState;

	private KAWidget mFarmingXpMeterProgressBar;

	public GameObject _CreativePointsGroup;

	public string _CreativePointsProgressFullSprite = "AniDWDragonsMeterBarCreativePointsFull";

	public string _CreativePointsProgressSprite = "AniDWDragonsMeterBarCreativePoints";

	protected override void Start()
	{
		base.Start();
		mTxtCurrentLevel = FindItem("TxtCurrentLevel");
		mFarmingXpMeterProgressBar = FindItem("FarmingXpMeter");
		mExpansionBtn = FindItem("ExpansionBtn");
		mAniCreativePointsProgress = FindItem("AniCreativeBar");
		mTxtCreativePointsLabel = FindItem("TxtCreativePoints");
		if (MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt() && _CreativePointsGroup != null)
		{
			_CreativePointsGroup.SetActive(value: false);
		}
		if (MyRoomsIntMain.pInstance != null && ((FarmManager)MyRoomsIntMain.pInstance)._BuildmodeTutorial.TutorialComplete())
		{
			mExpansionBtn.SetVisibility(inVisible: true);
		}
		UiToolbar component = AvAvatar.pToolbar.GetComponent<UiToolbar>();
		if (component != null)
		{
			KAWidget kAWidget = component.FindItem("TxtAvatarLevel");
			if (kAWidget != null)
			{
				kAWidget.gameObject.SetActive(value: false);
			}
			KAWidget kAWidget2 = component.FindItem("AvatarXpMeter");
			if (kAWidget2 != null)
			{
				kAWidget2.gameObject.SetActive(value: false);
			}
		}
	}

	public bool UpdateUIData(string inCurrentLevelNumStr, string inPointsStr)
	{
		if (mTxtCurrentLevel != null && mFarmingXpMeterProgressBar != null)
		{
			mTxtCurrentLevel.SetText(inCurrentLevelNumStr);
			float levelandRankProgress = int.Parse(inPointsStr);
			SetLevelandRankProgress(levelandRankProgress);
			return false;
		}
		return true;
	}

	private void OnEnable()
	{
		if (null != MyRoomsIntMain.pInstance)
		{
			(MyRoomsIntMain.pInstance as FarmManager).UpdateUIOnEnable();
		}
	}

	private void SetLevelandRankProgress(float CurrentPoint)
	{
		UserRank userRankByTypeAndValue = UserRankData.GetUserRankByTypeAndValue(9, (int)CurrentPoint);
		UserRank nextRankByType = UserRankData.GetNextRankByType(9, userRankByTypeAndValue.RankID);
		float progressLevel = 1f;
		if (userRankByTypeAndValue.RankID != nextRankByType.RankID)
		{
			progressLevel = (CurrentPoint - (float)userRankByTypeAndValue.Value) / (float)(nextRankByType.Value - userRankByTypeAndValue.Value);
		}
		mFarmingXpMeterProgressBar.SetProgressLevel(progressLevel);
		mFarmingXpMeterProgressBar.SetToolTipText(CurrentPoint - (float)userRankByTypeAndValue.Value + "/" + (nextRankByType.Value - userRankByTypeAndValue.Value));
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "BtnBack")
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			AvAvatar.SetActive(inActive: false);
			RsResourceManager.LoadLevel(_ExitLevelName);
		}
		else if (item.name == "BtnBackpack")
		{
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.pToolbar.GetComponent<UiToolbar>().SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			UiBackpack.Init(1);
		}
		else if (item == base.pBuildModeBtn)
		{
			if (InteractiveTutManager._CurrentActiveTutorialObject != null)
			{
				InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "BuildmodeClick");
			}
		}
		else if (item == mExpansionBtn)
		{
			if (InteractiveTutManager._CurrentActiveTutorialObject != null)
			{
				InteractiveTutManager._CurrentActiveTutorialObject.SendMessage("TutorialManagerAsyncMessage", "ExpansionClick");
			}
			mRevertAvatarState = true;
			AvAvatar.pState = AvAvatarState.PAUSED;
			SetInteractive(interactive: false);
			UiFarms.pOnFarmsUILoadHandler = (OnFarmsUILoad)Delegate.Combine(UiFarms.pOnFarmsUILoadHandler, new OnFarmsUILoad(OnFarmUILoad));
			UiFarms.pOnFarmsUIClosed = (OnFarmsUIClosed)Delegate.Combine(UiFarms.pOnFarmsUIClosed, new OnFarmsUIClosed(OnFarmUIClosed));
			if (MyRoomsIntLevel.pInstance != null && MyRoomsIntLevel.pInstance.IsOthersMyRoomsInt())
			{
				UiFarms.OpenFriendFarmListUI(MainStreetMMOClient.pInstance.GetOwnerIDForCurrentLevel(), null);
			}
			else
			{
				UiFarms.LoadFarmsUI();
			}
		}
	}

	public void OnFarmUILoad(bool isSuccess)
	{
		mRevertAvatarState = true;
		UiFarms.pOnFarmsUILoadHandler = (OnFarmsUILoad)Delegate.Remove(UiFarms.pOnFarmsUILoadHandler, new OnFarmsUILoad(OnFarmUILoad));
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.EnableAllInputs(inActive: false);
	}

	public void OnFarmUIClosed()
	{
		UiFarms.pOnFarmsUIClosed = (OnFarmsUIClosed)Delegate.Remove(UiFarms.pOnFarmsUIClosed, new OnFarmsUIClosed(OnFarmUIClosed));
		RevertAvatarState();
		if (this != null)
		{
			SetInteractive(interactive: true);
		}
	}

	private void RevertAvatarState()
	{
		if (mRevertAvatarState)
		{
			AvAvatar.EnableAllInputs(inActive: true);
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			mRevertAvatarState = false;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		UiFarms.pOnFarmsUILoadHandler = (OnFarmsUILoad)Delegate.Remove(UiFarms.pOnFarmsUILoadHandler, new OnFarmsUILoad(OnFarmUILoad));
		UiFarms.pOnFarmsUIClosed = (OnFarmsUIClosed)Delegate.Remove(UiFarms.pOnFarmsUIClosed, new OnFarmsUIClosed(OnFarmUIClosed));
		RevertAvatarState();
	}

	public static void OnBackpackLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			UnityEngine.Object.Instantiate((GameObject)inObject).SetActive(value: true);
			break;
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load back pack ....");
			break;
		}
	}

	public void UpdateCreativePointsProgress()
	{
		int pCurrentCreativePoints = MyRoomsIntMain.pInstance.pCurrentCreativePoints;
		int pMaxCreativePoints = MyRoomsIntMain.pInstance.pMaxCreativePoints;
		if (mTxtCreativePointsLabel != null && mAniCreativePointsProgress != null)
		{
			if (pCurrentCreativePoints >= pMaxCreativePoints)
			{
				mAniCreativePointsProgress.GetProgressBar().UpdateSprite(_CreativePointsProgressFullSprite);
			}
			else
			{
				mAniCreativePointsProgress.GetProgressBar().UpdateSprite(_CreativePointsProgressSprite);
			}
			mAniCreativePointsProgress.SetProgressLevel((float)pCurrentCreativePoints / (float)pMaxCreativePoints);
			mTxtCreativePointsLabel.SetText(pCurrentCreativePoints + "/" + pMaxCreativePoints);
		}
	}
}
