using System;
using UnityEngine;

public class UiSelectGameModes : KAUI
{
	public LocaleString _NoDragonMesaageText = new LocaleString("You need your dragon. Follow the quests to get your own dragon from the Hatchery!");

	public LocaleString _DragonNotAgedText = new LocaleString("Your dragon needs to be Short Wing stage to enter");

	public LocaleString _SelectFlightSuitText = new LocaleString("[Review] Please select a flight suit!");

	public int _MinAgeToGlideMode = 2;

	private ObstacleCourseLevelManager mGameManager;

	private UiAvatarCustomization mAvatarCustomization;

	private KAUIGenericDB mUiGenericDB;

	private static bool mIsMounted;

	private bool mOldRequestSkillSync;

	[Header("Customization")]
	public string _CustomizationAssetPath = "RS_DATA/PfUiAvatarCustomizationDO.unity3d/PfUiAvatarCustomizationDO";

	public static bool pIsMounted
	{
		get
		{
			return mIsMounted;
		}
		set
		{
			mIsMounted = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (RsResourceManager.pCurrentLevel != RsResourceManager.pLastLevel)
		{
			mIsMounted = SanctuaryManager.pMountedState;
		}
		mOldRequestSkillSync = SanctuaryManager.pRequestSkillSync;
	}

	public void Init(ObstacleCourseLevelManager inManager)
	{
		mGameManager = inManager;
		ObstacleCourseLevelManager.mMenuState = FSMenuState.FS_STATE_MODESELECT;
		SetVisibility(inVisible: true);
		mGameManager.PreLoadStore();
		mGameManager.ShowTutorial();
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "BtnDWDragonsClose")
		{
			SetVisibility(inVisible: false);
			ObstacleCourseLevelManager.ScoreData.mCount = 0;
			ObstacleCourseLevelManager.ScoreData.mDataReturnFail = false;
			SanctuaryManager.pRequestSkillSync = mOldRequestSkillSync;
			ExitGame();
		}
		else if (inWidget.name == "BtnDWDragonsFlight")
		{
			SetVisibility(inVisible: false);
			mGameManager.pGameMode = FSGameMode.FLIGHT_MODE;
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.pFlyingGlidingMode = false;
			}
			mGameManager.ShowDragonsUI();
		}
		else if (inWidget.name == "BtnDWDragonsGlide")
		{
			SetVisibility(inVisible: false);
			if (SanctuaryManager.pCurPetInstance == null || (SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetInstance.pAge < _MinAgeToGlideMode))
			{
				ShowWarningDB();
				return;
			}
			mGameManager.pGameMode = FSGameMode.GLIDE_MODE;
			AvAvatarController component2 = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component2 != null)
			{
				component2.pFlyingGlidingMode = true;
			}
			mGameManager.ShowLevelSelectionUI();
		}
		else if (inWidget.name == "BtnDWFlightSuit")
		{
			mGameManager.pGameMode = FSGameMode.FLIGHT_SUIT_MODE;
			AvAvatarController component3 = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component3 != null)
			{
				component3.pFlyingGlidingMode = true;
			}
			SetVisibility(inVisible: false);
			if (IsFlightSuitAvailable() || component3.GetMemberFlightSuit() != null)
			{
				KAUICursorManager.SetDefaultCursor("Loading");
				RsResourceManager.LoadAssetFromBundle(_CustomizationAssetPath, OnAvatarCustomizationLoaded, typeof(GameObject));
			}
			else
			{
				StoreLoader.Load(setDefaultMenuItem: true, "Flight Suits", "Viking Clothes", base.gameObject);
			}
		}
	}

	private void OnStoreClosed()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.SetOnlyAvatarActive(active: false);
		mGameManager.ShowGameModeUI();
	}

	private bool IsFlightSuitAvailable()
	{
		UserItemData[] array = null;
		if (CommonInventoryData.pInstance != null)
		{
			array = CommonInventoryData.pInstance.GetItems(228);
		}
		if (array != null && array.Length != 0)
		{
			for (int i = 0; i < array.Length; i++)
			{
				if (array[i].Item.HasAttribute("FlightSuit"))
				{
					return true;
				}
			}
		}
		return false;
	}

	private void AvatarCreatorClosed()
	{
		SetVisibility(inVisible: true);
	}

	private void OnAvatarCustomizationLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			mAvatarCustomization = gameObject.GetComponentInChildren<UiAvatarCustomization>();
			mAvatarCustomization.pPrevPosition = new Vector3(0f, -5000f, 0f);
			mAvatarCustomization._CloseMsgObject = base.gameObject;
			mAvatarCustomization.pDefaultTabIndex = mAvatarCustomization.BattleReadyTabIndex;
			mAvatarCustomization.Initialize();
			mAvatarCustomization.ShowBackButton(show: true);
			UiAvatarCustomization.OkButtonClicked = (Action)Delegate.Combine(UiAvatarCustomization.OkButtonClicked, new Action(AvatarCustomizationClosed));
			mAvatarCustomization.ShowOkButton(show: true);
			break;
		}
		case RsResourceLoadEvent.ERROR:
			Debug.LogError("Failed to load Avatar Equipment....");
			break;
		}
	}

	private void AvatarCustomizationClosed()
	{
		if (AvatarData.pInstanceInfo.FlightSuitEquipped())
		{
			UiAvatarCustomization.OkButtonClicked = (Action)Delegate.Remove(UiAvatarCustomization.OkButtonClicked, new Action(AvatarCustomizationClosed));
			mAvatarCustomization.OnClose();
			mGameManager.ShowLevelSelectionUI();
		}
		else
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _SelectFlightSuitText.GetLocalizedString(), null, "");
		}
	}

	private void ShowWarningDB()
	{
		mUiGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "WarningDB");
		mUiGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mUiGenericDB._MessageObject = base.gameObject;
		mUiGenericDB._OKMessage = "OnCloseDB";
		if (SanctuaryManager.pCurPetInstance != null)
		{
			mUiGenericDB.SetTextByID(_DragonNotAgedText._ID, _DragonNotAgedText._Text, interactive: false);
		}
		else
		{
			mUiGenericDB.SetTextByID(_NoDragonMesaageText._ID, _NoDragonMesaageText._Text, interactive: false);
		}
		KAUI.SetExclusive(mUiGenericDB);
	}

	private void OnCloseDB()
	{
		KAUI.RemoveExclusive(mUiGenericDB);
		UnityEngine.Object.Destroy(mUiGenericDB.gameObject);
		mUiGenericDB = null;
		DragonAgeUpConfig.ShowAgeUpUI(OnDragonAgeUpDone);
	}

	private void OnDragonAgeUpDone()
	{
		SetVisibility(inVisible: true);
		AvAvatar.SetOnlyAvatarActive(active: false);
	}

	private void ExitGame()
	{
		AvAvatar.pLevelState = AvAvatarLevelState.NORMAL;
		AvAvatarController avAvatarController = (AvAvatarController)AvAvatar.pObject.GetComponent(typeof(AvAvatarController));
		if (avAvatarController != null)
		{
			if (SanctuaryManager.pCurPetInstance != null)
			{
				avAvatarController.pFlyingData = FlightData.GetFlightData(SanctuaryManager.pCurPetInstance.gameObject, FlightDataType.FLYING);
			}
			else
			{
				avAvatarController.pFlyingData = FlightData.GetFlightData(avAvatarController.gameObject, FlightDataType.GLIDING);
			}
			avAvatarController.DisableSoarButtonUpdate = false;
		}
		avAvatarController.pIsPlayerGliding = false;
		avAvatarController.OnGlideEnd();
		AvAvatar.pSubState = AvAvatarSubState.NORMAL;
		if (!string.IsNullOrEmpty(mGameManager._ExitMarker))
		{
			AvAvatar.pStartLocation = mGameManager._ExitMarker;
		}
		if (!mIsMounted)
		{
			SanctuaryManager.pMountedState = false;
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.OnFlyDismountImmediate(AvAvatar.pObject);
				SanctuaryManager.pCurPetInstance.SetFollowAvatar(follow: false);
			}
		}
		mGameManager.pFlightSchoolGameData = null;
		KAInput.pInstance.EnableInputType("Jump", InputType.ALL, inEnable: true);
		if (mGameManager != null)
		{
			UtUtilities.LoadLevel(mGameManager._ExitLevel);
		}
		else
		{
			UtDebug.LogError("GAME MANAGER NULL!!!");
		}
	}
}
