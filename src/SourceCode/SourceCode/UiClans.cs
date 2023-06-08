using System.Collections.Generic;
using UnityEngine;

public class UiClans : KAUI
{
	public UiClansCreate _UiClansCreate;

	public UiClansJoin _UiClansJoin;

	public UiClansSearch _UiClansSearch;

	public UiClansDetails _UiClansDetails;

	public UiClansMessages _UiClansMessages;

	public UiAchievements _UiAchievements;

	public KAWidget[] _ClanTabs;

	public LocaleString[] _ClanTypeText;

	public LocaleString[] _UserRoleText;

	private static UiClans mInstance;

	private static string mUserID;

	private static Group mClan;

	private static ClanTabs mDefaultTab;

	private bool mUpdate;

	private KAWidget mCloseBtn;

	private KAToggleButton mJoinClanBtn;

	private KAToggleButton mCreateClanBtn;

	private KAToggleButton mSearchClanBtn;

	private KAToggleButton mClanMessageBtn;

	private KAToggleButton mMyClanBtn;

	private KAToggleButton mTopClanBtn;

	private KAToggleButton mAchievementsBtn;

	private KAWidget mBackBtn;

	private List<GameObject> mBackBtnMessageObjects = new List<GameObject>();

	private KAToggleButton mSelectedTab;

	private KAUIGenericDB mKAUIGenericDB;

	public static UiClans pInstance => mInstance;

	public static string pUserID => mUserID;

	public static Group pClan => mClan;

	public KAWidget pCloseBtn => mCloseBtn;

	public static void ShowClan(string inUserID, Group inGroup = null, ClanTabs tab = ClanTabs.MESSAGES)
	{
		if (mInstance != null && mUserID == inUserID)
		{
			return;
		}
		mUserID = inUserID;
		mClan = inGroup;
		mDefaultTab = tab;
		if (MissionManager.pInstance != null)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", "OpenClan");
		}
		if (UtMobileUtilities.CanLoadInCurrentScene(UiType.Clans, UILoadOptions.AUTO))
		{
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			if (UiProfile.pInstance != null)
			{
				UiProfile.pInstance.gameObject.SetActive(value: false);
			}
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("ClansAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnAssetLoadingEvent, typeof(GameObject));
		}
		else
		{
			AvAvatar.SetStartPositionAndRotation();
			AvAvatar.SetActive(inActive: false);
			RsResourceManager.LoadLevel(GameConfig.GetKeyData("ClansScene"));
		}
	}

	public static void OnAssetLoadingEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			Object.Instantiate((GameObject)inObject).name = "PfUiClans";
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		case RsResourceLoadEvent.ERROR:
			if (UiProfile.pInstance != null)
			{
				UiProfile.pInstance.gameObject.SetActive(value: true);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected override void Start()
	{
		base.Start();
		mInstance = this;
		SetVisibility(inVisible: false);
		mCloseBtn = FindItem("CloseBtn");
		mJoinClanBtn = (KAToggleButton)FindItem("JoinClan");
		mTopClanBtn = (KAToggleButton)FindItem("TopClan");
		mCreateClanBtn = (KAToggleButton)FindItem("CreateClan");
		mSearchClanBtn = (KAToggleButton)FindItem("SearchClan");
		mClanMessageBtn = (KAToggleButton)FindItem("ClanMessages");
		mMyClanBtn = (KAToggleButton)FindItem("MyClan");
		mAchievementsBtn = (KAToggleButton)FindItem("Achievements");
		mBackBtn = FindItem("BackBtn");
		if (string.IsNullOrEmpty(mUserID))
		{
			mUserID = UserInfo.pInstance.UserID;
		}
		Group.Reset();
		Group.Init(includeMemberCount: true);
		if (UtPlatform.IsMobile() && !BuddyList.pIsReady)
		{
			BuddyList.Init();
		}
	}

	protected override void Update()
	{
		base.Update();
		if (Group.pIsReady && !mUpdate)
		{
			mUpdate = true;
			if (mClan == null)
			{
				mClan = Group.GetGroup(UserProfile.pProfileData.GetGroupID());
			}
			OnClanInfoReady();
		}
	}

	private void OnClanInfoReady()
	{
		SetVisibility(inVisible: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (RsResourceManager.pCurrentLevel != GameConfig.GetKeyData("ClansScene"))
		{
			KAUI.SetExclusive(this);
		}
		RsResourceManager.DestroyLoadScreen();
		ShowTabs();
	}

	public void SetClan(Group inClan, bool switchTabs = true)
	{
		mClan = inClan;
		ShowTabs(switchTabs);
	}

	public void ShowTabs(bool switchTabs = true, bool loadDefaultTab = true)
	{
		bool flag = mClan != null && UserProfile.pProfileData.HasGroup();
		mJoinClanBtn.SetVisibility(!flag);
		mCreateClanBtn.SetVisibility(!flag);
		mMyClanBtn.SetVisibility(flag);
		mClanMessageBtn.SetVisibility(flag);
		mAchievementsBtn.SetVisibility(flag);
		if (!switchTabs)
		{
			return;
		}
		if (flag)
		{
			if (!UserProfile.pProfileData.InGroup(mClan.GroupID))
			{
				_UiClansDetails.Show(show: true, mClan);
				return;
			}
			if (mSelectedTab == null)
			{
				_ClanTabs[(int)mDefaultTab].OnClick();
			}
			OnClick(_ClanTabs[(int)mDefaultTab]);
		}
		else if (mClan != null)
		{
			_UiClansDetails.Show(show: true, mClan);
		}
		else
		{
			if (mSelectedTab == null)
			{
				mJoinClanBtn.OnClick();
			}
			OnClick(mJoinClanBtn);
			_UiClansCreate.InitClanUI(mClan);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mJoinClanBtn || inWidget == mCreateClanBtn || inWidget == mSearchClanBtn || inWidget == mMyClanBtn || inWidget == mClanMessageBtn || inWidget == mTopClanBtn || inWidget == mAchievementsBtn)
		{
			mUserID = UserInfo.pInstance.UserID;
			if (UserProfile.pProfileData.HasGroup())
			{
				mClan = Group.GetGroup(UserProfile.pProfileData.Groups[0].GroupID);
			}
			if (inWidget == mTopClanBtn && _UiClansJoin.gameObject.activeSelf && _UiClansDetails.GetVisibility())
			{
				_UiClansJoin.ShowClanDetails(show: false);
			}
			_UiClansJoin.Show(inWidget == mJoinClanBtn || inWidget == mTopClanBtn);
			_UiClansCreate.gameObject.SetActive(inWidget == mCreateClanBtn);
			_UiClansSearch.gameObject.SetActive(inWidget == mSearchClanBtn);
			_UiClansMessages.Show(inWidget == mClanMessageBtn);
			_UiClansDetails.Show(inWidget == mMyClanBtn, mClan);
			_UiAchievements.Show(inWidget == mAchievementsBtn);
			mBackBtn.SetVisibility(inVisible: false);
			mBackBtnMessageObjects.Clear();
			if (mSelectedTab != null)
			{
				mSelectedTab.SetInteractive(isInteractive: true);
			}
			mSelectedTab = (KAToggleButton)inWidget;
			mSelectedTab.SetInteractive(isInteractive: false);
		}
		else if (inWidget == mCloseBtn)
		{
			if (UtPlatform.IsAndroid() && Input.GetKeyUp(KeyCode.Escape))
			{
				if (_UiClansDetails != null && _UiClansDetails._UiClanMemberDB != null && _UiClansDetails._UiClanMemberDB.gameObject.activeSelf)
				{
					_UiClansDetails._UiClanMemberDB.gameObject.SetActive(value: false);
				}
				else if (mBackBtn.IsActive())
				{
					OnClick(mBackBtn);
				}
				else
				{
					Exit();
				}
			}
			else
			{
				Exit();
			}
		}
		else if (inWidget == mBackBtn)
		{
			if (mBackBtnMessageObjects.Count > 0)
			{
				GameObject gameObject = mBackBtnMessageObjects[mBackBtnMessageObjects.Count - 1];
				mBackBtnMessageObjects.Remove(gameObject);
				gameObject.SendMessage("OnBackBtnClicked", SendMessageOptions.DontRequireReceiver);
			}
			if (mBackBtnMessageObjects.Count == 0)
			{
				mBackBtn.SetVisibility(inVisible: false);
			}
		}
	}

	public void OnClickBackBtn()
	{
		if (mBackBtn != null && mBackBtn.GetVisibility())
		{
			OnClick(mBackBtn);
		}
	}

	public void Exit(bool isVistingMember = false)
	{
		if (RsResourceManager.pCurrentLevel != GameConfig.GetKeyData("ClansScene"))
		{
			KAUI.RemoveExclusive(this);
		}
		Object.Destroy(base.gameObject);
		if (RsResourceManager.pCurrentLevel == GameConfig.GetKeyData("ClansScene"))
		{
			AvAvatar.pStartLocation = AvAvatar.pSpawnAtSetPosition;
			if (!isVistingMember)
			{
				RsResourceManager.LoadLevel(RsResourceManager.pLastLevel);
			}
		}
		else if (UiProfile.pInstance != null)
		{
			UiProfile.pInstance.gameObject.SetActive(value: true);
		}
		else
		{
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
		}
	}

	public void ShowBackBtn(bool isVisible, GameObject inMessageObject = null)
	{
		if (isVisible)
		{
			mBackBtn.SetVisibility(isVisible);
			if (inMessageObject != null)
			{
				mBackBtnMessageObjects.Add(inMessageObject);
			}
			return;
		}
		if (inMessageObject != null)
		{
			mBackBtnMessageObjects.Remove(inMessageObject);
		}
		if (mBackBtnMessageObjects.Count == 0)
		{
			mBackBtn.SetVisibility(inVisible: false);
		}
	}

	private void KillGenericDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		mInstance = null;
	}
}
