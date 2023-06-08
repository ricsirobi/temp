using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;

public class ProfileSelection : MonoBehaviour
{
	public const string _LoginSessionCount = "SessionCount";

	public string _StartLevel;

	public string[] _FirstTimeTutorialName;

	public UiSelectProfile _UiChooseProfile;

	public string _FirstTimeTutStartMarker = "PfMarker_AvatarGameStart01";

	public int _InitialStable = 8977;

	private bool mMissionManagerLoading;

	private bool mBeginLoad;

	private string mQuickLaunchScene;

	private bool mIsBundleToBeLoaded;

	private object mQuickLaunchBundleObject;

	private string mCurrentUserID = string.Empty;

	private bool mCommonInventoryReady;

	private void Start()
	{
		KAInput.pInstance.ShowInputs(inShow: false);
		_UiChooseProfile.SetVisibility(inVisible: false);
		_UiChooseProfile._ProfileSelectedMessage = "OnProfileSelected";
		_UiChooseProfile._MessageObject = base.gameObject;
		_UiChooseProfile.Init();
	}

	private void OnMovieStarted()
	{
		_UiChooseProfile.SetVisibility(inVisible: false);
	}

	private void OnMoviePlayed()
	{
		if (SanctuaryData.pInstance != null)
		{
			Object.Destroy(SanctuaryData.pInstance.gameObject);
			SanctuaryData.pInstance = null;
		}
		mBeginLoad = false;
		_UiChooseProfile.SetVisibility(inVisible: false);
		AvatarData.SetDisplayNameVisible(AvAvatar.pObject, inVisible: true, SubscriptionInfo.pIsMember);
		AvAvatar.SetActive(inActive: false);
		UiToolbar.pAvatarModified = true;
		if (!IsFirstTutComplete())
		{
			AvAvatar.pStartLocation = _FirstTimeTutStartMarker;
		}
		string text = ProductData.GetSavedScene();
		string text2 = ExpansionUnlock.pInstance.IsSceneUnlocked(text);
		if (text2 != null)
		{
			text = text2;
		}
		int defaultVal = 1;
		if (ProductData.pPairData.FindByKey("SessionCount") != null)
		{
			defaultVal = ProductData.pPairData.GetIntValue("SessionCount", defaultVal);
			defaultVal++;
		}
		ProductData.pPairData.SetValueAndSave("SessionCount", defaultVal.ToString());
		if (UiLogin.pIsGuestUser)
		{
			RsResourceManager.DestroyLoadScreen();
		}
		if (!string.IsNullOrEmpty(mQuickLaunchScene))
		{
			RsResourceManager.LoadLevel(mQuickLaunchScene);
		}
		else if (!string.IsNullOrEmpty(text) && text != RsResourceManager.pCurrentLevel)
		{
			RsResourceManager.LoadLevel(text);
		}
		else
		{
			RsResourceManager.LoadLevel(_StartLevel);
		}
		AnalyticAgent.LogFTUEEvent(FTUEEvent.ONMOVIEPLAYED);
	}

	public void OnProfileSelected(LoadInfo loadInfo)
	{
		if (loadInfo != null)
		{
			mQuickLaunchBundleObject = null;
			if (loadInfo._LoadType == LoadType.SCENE)
			{
				mQuickLaunchScene = loadInfo._LoadValue;
			}
			else if (loadInfo._LoadType == LoadType.BUNDLE && !string.IsNullOrEmpty(loadInfo._LoadValue))
			{
				RsResourceManager.LoadAssetFromBundle(loadInfo._LoadValue, OnBundleLoaded, typeof(GameObject));
				mIsBundleToBeLoaded = true;
			}
		}
		mBeginLoad = true;
		KAUICursorManager.SetDefaultCursor("Loading");
		StartCoroutine("SendAnalyticsOnGameLoadFailure");
		if (!mCurrentUserID.Equals(UserInfo.pInstance.UserID))
		{
			mCurrentUserID = UserInfo.pInstance.UserID;
			mMissionManagerLoading = false;
			MissionManager.Reset();
			ProductData.Init(forcePairDataLoad: true);
			StartCoroutine(InitCommonInventory());
			ChallengeInfo.Init();
			ServerTime.Init();
			AvatarEquipment.Init();
			FishingData.Init();
			DragonAgeUpConfig.Init();
			ConsumableData.Init();
			StableData.Init();
			UserRoom.Init();
			ImageData.Reset();
			UiChatHistory.Init();
			RewardMultiplierManager.Init();
			FUEManager.pIsFUERunning = false;
			UserProfileData pProfileData = UserProfile.pProfileData;
			object obj;
			if (pProfileData == null)
			{
				obj = null;
			}
			else
			{
				UserProfileGroupData[] groups = pProfileData.Groups;
				obj = ((groups == null) ? null : groups[0]?.Name);
			}
			if (obj != null)
			{
				Group.Get(UserProfile.pProfileData?.ID, OnGetGroup);
			}
			else
			{
				Group.Init(includeMemberCount: false, mCurrentUserID, skipServerCall: true);
			}
			if (AdManager.pInstance != null)
			{
				AdManager.pInstance.Init();
			}
		}
	}

	private IEnumerator SendAnalyticsOnGameLoadFailure()
	{
		float timeout = 30f;
		while (timeout > 0f && mBeginLoad)
		{
			timeout -= Time.deltaTime;
			yield return new WaitForEndOfFrame();
		}
		if (mBeginLoad)
		{
			SendFailedModules();
		}
		yield return null;
	}

	private void SendFailedModules()
	{
		Dictionary<string, object> dictionary = new Dictionary<string, object>();
		StringBuilder stringBuilder = new StringBuilder();
		if (!MissionManager.pIsReady)
		{
			stringBuilder.Append("MM, ");
		}
		if (!ProductData.pIsReady)
		{
			stringBuilder.Append("PD, ");
		}
		if (!CommonInventoryData.pIsReady)
		{
			stringBuilder.Append("CID, ");
		}
		if (!ChallengeInfo.pIsReady)
		{
			stringBuilder.Append("CI, ");
		}
		if (!ServerTime.pIsReady)
		{
			stringBuilder.Append("ST, ");
		}
		if (AvatarEquipment.pInstance == null || (AvatarEquipment.pInstance != null && !AvatarEquipment.pIsReady))
		{
			stringBuilder.Append("AE, ");
		}
		if (!FishingData.pIsReady)
		{
			stringBuilder.Append("FD, ");
		}
		if (!DragonAgeUpConfig.pIsReady)
		{
			stringBuilder.Append("DAUC, ");
		}
		if (!ConsumableData.pIsReady)
		{
			stringBuilder.Append("CD, ");
		}
		if (!StableData.pIsReady)
		{
			stringBuilder.Append("SD, ");
		}
		if (!UserRoom.pIsReady)
		{
			stringBuilder.Append("UR, ");
		}
		if (!RewardMultiplierManager.pIsReady)
		{
			stringBuilder.Append("RN, ");
		}
		if (!Group.pIsReady)
		{
			stringBuilder.Append("G");
		}
		dictionary.Add("failedModules", stringBuilder);
		AnalyticAgent.LogFTUEEvent(FTUEEvent.ONPROFILESELECTED_FAILED, dictionary);
	}

	private static void OnGetGroup(GetGroupsResult result, object userData)
	{
		if (result != null && result.Success && result.Groups.Length != 0 && result.Groups[0] != null)
		{
			Group.AddGroup(result.Groups[0]);
			if (UserProfile.pProfileData.InGroup(result.Groups[0].GroupID))
			{
				AvatarData.SetGroupName(result.Groups[0]);
			}
		}
	}

	public void OnBundleLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			mQuickLaunchBundleObject = inObject;
			break;
		case RsResourceLoadEvent.ERROR:
			UtDebug.LogError("Bundle failed to load");
			break;
		}
	}

	private IEnumerator InitCommonInventory()
	{
		int[] waitTimes = new int[4] { 1, 4, 7, 15 };
		int currentWaitTime = 0;
		mCommonInventoryReady = false;
		if (CommonInventoryData.pIsReady)
		{
			CommonInventoryData.Init();
		}
		while (!_UiChooseProfile.mIsPetAvailable)
		{
			while (!CommonInventoryData.pIsReady)
			{
				yield return new WaitForEndOfFrame();
			}
			if (CommonInventoryData.pInstance.FindItem(_InitialStable) != null || currentWaitTime >= waitTimes.Length)
			{
				break;
			}
			yield return new WaitForSeconds(waitTimes[currentWaitTime]);
			CommonInventoryData.ReInit();
			currentWaitTime++;
		}
		mCommonInventoryReady = true;
	}

	private bool IsFirstTutComplete()
	{
		string[] firstTimeTutorialName = _FirstTimeTutorialName;
		for (int i = 0; i < firstTimeTutorialName.Length; i++)
		{
			if (ProductData.TutorialComplete(firstTimeTutorialName[i]))
			{
				return true;
			}
		}
		return false;
	}

	private void LoadMovie()
	{
		if (!IsFirstTutComplete())
		{
			MovieManager.SetBackgroundColor(Color.black);
			MovieManager.Play("NewUser", OnMovieStarted, OnMoviePlayed, skipMovie: true);
		}
		else
		{
			OnMoviePlayed();
		}
	}

	private void Update()
	{
		if (!mBeginLoad)
		{
			return;
		}
		if (UserRankData.pIsReady && !mMissionManagerLoading)
		{
			mMissionManagerLoading = true;
			UserNotifyCustomizePet.pHatchDragonQuestValidated = false;
			MissionManager.Init();
			TimedMissionManager.Init();
		}
		if (!AvatarData.pIsReady || !SubscriptionInfo.pIsReady || !UserInfo.pIsReady || !WsTokenMonitor.pHaveCheckedToken || !ProductData.pIsReady || !Money.pIsReady || !ServerTime.pIsReady || !LocaleData.pIsReady || !mCommonInventoryReady || !ChallengeInfo.pIsReady || !MissionManager.pIsReady || !AvatarEquipment.pIsReady || !DragonAgeUpConfig.pIsReady || !ConsumableData.pIsReady || !StableData.pIsReady || !TimedMissionManager.pIsReady || !RewardMultiplierManager.pIsReady || !Group.pIsReady || (mIsBundleToBeLoaded && mQuickLaunchBundleObject == null))
		{
			return;
		}
		CheckAvatarAttributes();
		AvatarData.pInstanceInfo.UpdatePartsInventoryIds();
		KAUICursorManager.SetDefaultCursor("Arrow");
		mBeginLoad = false;
		if (mQuickLaunchBundleObject != null)
		{
			mIsBundleToBeLoaded = false;
			GameObject gameObject = Object.Instantiate((GameObject)mQuickLaunchBundleObject);
			UiWorldMap component = gameObject.GetComponent<UiWorldMap>();
			if (component != null)
			{
				_UiChooseProfile.SetVisibility(inVisible: false);
				component._MessageObject = base.gameObject;
				component._EnableInputsOnExit = false;
				gameObject.name = "PfUiWorldMap";
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		else
		{
			LoadMovie();
		}
	}

	public void OnLocationSelected(string scene)
	{
		mQuickLaunchScene = scene;
		OnMoviePlayed();
	}

	public void OnMapClosed()
	{
		_UiChooseProfile.SetVisibility(inVisible: true);
		_UiChooseProfile.SetInteractive(interactive: true);
	}

	private void OnDestroy()
	{
		_UiChooseProfile = null;
	}

	public void CheckAvatarAttributes()
	{
		Vector3 version = AvatarData.pInstanceInfo.GetVersion();
		if (!(version.x < 6f) && (version.x != 6f || !(version.y < 1f)))
		{
			return;
		}
		for (int i = 0; i < AvatarData.pInstanceInfo.mInstance.Part.Length; i++)
		{
			AvatarDataPart avatarDataPart = AvatarData.pInstanceInfo.mInstance.Part[i];
			string partType = avatarDataPart.PartType.Replace("DEFAULT_", "");
			if (avatarDataPart != null && IsValidPart(partType))
			{
				UserItemData userItemDataFromGeometryAndTexture = CommonInventoryData.pInstance.GetUserItemDataFromGeometryAndTexture(avatarDataPart.Geometries, avatarDataPart.Textures, AvatarData.GetCategoryID(partType));
				if (userItemDataFromGeometryAndTexture != null)
				{
					AvatarData.SetAttributes(AvatarData.pInstanceInfo, avatarDataPart.PartType, userItemDataFromGeometryAndTexture.Item.Attribute);
				}
			}
		}
	}

	private bool IsValidPart(string partType)
	{
		if (partType == "Version" || partType == AvatarData.pPartSettings.AVATAR_PART_BACK || partType == AvatarData.pPartSettings.AVATAR_PART_WING || partType == AvatarData.pPartSettings.AVATAR_PART_EYES || partType == AvatarData.pPartSettings.AVATAR_PART_MOUTH || partType == AvatarData.pPartSettings.AVATAR_PART_SKIN)
		{
			return false;
		}
		return true;
	}
}
