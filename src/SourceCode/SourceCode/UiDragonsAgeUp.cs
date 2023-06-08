using System.Collections.Generic;
using UnityEngine;

public class UiDragonsAgeUp : KAUI
{
	private static bool mAgeUpLoading;

	private static bool mCloseOnAgeUp;

	private static GameObject mTriggerObject;

	private static GameObject mStoreTriggerObject;

	private static bool mIsTicketPurchased;

	[SerializeField]
	private int m_AgeUpStoreId = 102;

	[SerializeField]
	private StoreLoader.Selection m_AgeUpStoreInfo;

	[SerializeField]
	private PetAgeUpData[] m_AgeUpData;

	[SerializeField]
	private KAWidget m_AdultUpgrade;

	[SerializeField]
	private KAWidget m_TitanUpgrade;

	[SerializeField]
	private string m_UpgradeTextWidget = "TxtUpgrade";

	[SerializeField]
	private string m_ColonTextWidget = "TxtColon";

	[SerializeField]
	private KAWidget m_TxtGems;

	[SerializeField]
	private KAWidget m_TxtCoins;

	[SerializeField]
	private LocaleString m_FreeGrowDragonText = new LocaleString("This dragon is ready to be an {0}, for free! Age them to an {1}?");

	[SerializeField]
	private LocaleString m_FreeGrowDragonTitleText = new LocaleString("Grow Dragon Free");

	[SerializeField]
	private Color m_NoTicketCountColor = Color.red;

	[SerializeField]
	private Color m_NormalTicketCountColor = Color.black;

	[SerializeField]
	private AudioClip m_AgeUpSound;

	public static DragonAgeUpConfig.OnDragonAgeUpDone pBundleDownloadCallback;

	private UiDragonsAgeUpMenu mUiDragonsAgeUpMenu;

	private AgeUpUserData mFreeAgeUpUserData;

	private RaisedPetStage mFreeAgeUpToAge;

	private RaisedPetStage mFreeAgeUpFromAge;

	public static UiDragonsAgeUp pInstance { get; private set; }

	public static bool pIsTicketPurchased => mIsTicketPurchased;

	public PetAgeUpData[] pAgeUpData => m_AgeUpData;

	public UiDragonAgeUpConfirm pUiDragonAgeUpConfirm { get; private set; }

	public UiAgeUpBuy pUiAgeUpBuy { get; private set; }

	public StoreData pAgeUpStoreData { get; private set; }

	public DragonAgeUpConfig.OnDragonAgeUpDone pCloseCallback { get; set; }

	public static void Init(DragonAgeUpConfig.OnDragonAgeUpDone onCloseAgeup = null, bool closeOnAgeUp = false, GameObject triggerObj = null, bool isTicketPurchased = false, GameObject storeTriggerObj = null, DragonAgeUpConfig.OnDragonAgeUpDone onBundleLoaded = null)
	{
		if (pInstance == null && !mAgeUpLoading)
		{
			mAgeUpLoading = true;
			mCloseOnAgeUp = closeOnAgeUp;
			mTriggerObject = triggerObj;
			mIsTicketPurchased = isTicketPurchased;
			mStoreTriggerObject = storeTriggerObj;
			pBundleDownloadCallback = onBundleLoaded;
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = GameConfig.GetKeyData("DragonsAgeUpAsset").Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnDragonAgeUpLoaded, typeof(GameObject), inDontDestroy: false, onCloseAgeup);
		}
	}

	private static void OnDragonAgeUpLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			pInstance = Object.Instantiate((GameObject)inObject).GetComponent<UiDragonsAgeUp>();
			mAgeUpLoading = false;
			pBundleDownloadCallback?.Invoke();
			if (inUserData != null)
			{
				pInstance.pCloseCallback = (DragonAgeUpConfig.OnDragonAgeUpDone)inUserData;
			}
			RsResourceManager.ReleaseBundleData(inURL);
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			((DragonAgeUpConfig.OnDragonAgeUpDone)inUserData)?.Invoke();
			break;
		}
	}

	protected override void Start()
	{
		base.Start();
		pUiAgeUpBuy = (UiAgeUpBuy)_UiList[0];
		pUiDragonAgeUpConfirm = (UiDragonAgeUpConfirm)_UiList[1];
		mUiDragonsAgeUpMenu = (UiDragonsAgeUpMenu)GetMenuByIndex(0);
		KAUI.SetExclusive(this);
		AvAvatar.pInputEnabled = false;
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		ItemStoreDataLoader.Load(m_AgeUpStoreId, OnAgeUpStoreLoaded);
	}

	protected virtual void OnAgeUpStoreLoaded(StoreData sd)
	{
		pAgeUpStoreData = sd;
		UpdateUpgradeAvailibility();
		ShowDragons();
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetVisibility(inVisible: true);
	}

	public void UpdateUpgradeAvailibility()
	{
		m_TxtGems.SetText(Money.pCashCurrency.ToString());
		m_TxtCoins.SetText(Money.pGameCurrency.ToString());
		if (m_AgeUpData == null || m_AgeUpData.Length == 0)
		{
			return;
		}
		int num = 0;
		int num2 = 0;
		List<int> list = new List<int>();
		for (int i = 0; i < m_AgeUpData.Length; i++)
		{
			if (!list.Contains(m_AgeUpData[i]._AgeUpTicketID))
			{
				list.Add(m_AgeUpData[i]._AgeUpTicketID);
				if (m_AgeUpData[i]._ToPetStage == RaisedPetStage.ADULT)
				{
					num += GetAgeupItemQuantity(m_AgeUpData[i]._AgeUpTicketID);
				}
				else if (m_AgeUpData[i]._ToPetStage == RaisedPetStage.TITAN)
				{
					num2 += GetAgeupItemQuantity(m_AgeUpData[i]._AgeUpTicketID);
				}
			}
			if (!list.Contains(m_AgeUpData[i]._AgeUpItemID))
			{
				list.Add(m_AgeUpData[i]._AgeUpItemID);
				if (m_AgeUpData[i]._ToPetStage == RaisedPetStage.ADULT)
				{
					num += GetAgeupItemQuantity(m_AgeUpData[i]._AgeUpItemID);
				}
				else if (m_AgeUpData[i]._ToPetStage == RaisedPetStage.TITAN)
				{
					num2 += GetAgeupItemQuantity(m_AgeUpData[i]._AgeUpItemID);
				}
			}
		}
		SetUpgradeTextAndColor(m_AdultUpgrade, num);
		SetUpgradeTextAndColor(m_TitanUpgrade, num2);
	}

	private void SetUpgradeTextAndColor(KAWidget widget, int count)
	{
		KAWidget kAWidget = widget.FindChildItem(m_UpgradeTextWidget);
		KAWidget kAWidget2 = widget.FindChildItem(m_ColonTextWidget);
		Color color = m_NoTicketCountColor;
		if (count > 0)
		{
			color = m_NormalTicketCountColor;
		}
		widget.GetLabel().color = color;
		if (kAWidget != null)
		{
			kAWidget.GetLabel().color = color;
		}
		if (kAWidget2 != null)
		{
			kAWidget2.GetLabel().color = color;
		}
		widget.SetText(count.ToString());
	}

	public int GetAgeupItemQuantity(int id)
	{
		int num = 0;
		if (id > 0)
		{
			if (ParentData.pIsReady)
			{
				num = ParentData.pInstance.pInventory.pData.GetQuantity(id);
			}
			if (CommonInventoryData.pIsReady)
			{
				num += CommonInventoryData.pInstance.GetQuantity(id);
			}
		}
		return num;
	}

	public void ShowDragons()
	{
		mUiDragonsAgeUpMenu.ClearItems();
		StableData.UpdateInfo();
		ShowCurrentDragon();
		ShowOtherDragons();
	}

	private void ShowCurrentDragon()
	{
		if ((bool)SanctuaryManager.pCurPetInstance)
		{
			RaisedPetData pData = SanctuaryManager.pCurPetInstance.pData;
			UiDragonsAgeUpMenuItem uiDragonsAgeUpMenuItem = (UiDragonsAgeUpMenuItem)mUiDragonsAgeUpMenu.AddWidget(SanctuaryManager.pCurPetInstance.pData.Name);
			uiDragonsAgeUpMenuItem.SetUserData(new AgeUpUserData(pData));
			uiDragonsAgeUpMenuItem.SetupWidget();
			if (FUEManager.pInstance != null && FUEManager.pIsFUERunning && (bool)FUEManager.pInstance._AgeUpTutorial && FUEManager.pInstance._AgeUpTutorial.IsShowingTutorial())
			{
				uiDragonsAgeUpMenuItem.PlayAnimOnBtns("Flash");
			}
		}
	}

	private void ShowOtherDragons()
	{
		foreach (RaisedPetData[] value in RaisedPetData.pActivePets.Values)
		{
			if (value == null)
			{
				continue;
			}
			RaisedPetData[] array = value;
			foreach (RaisedPetData raisedPetData in array)
			{
				if (raisedPetData.RaisedPetID != SanctuaryManager.pCurPetInstance.pData.RaisedPetID && raisedPetData.pStage >= RaisedPetStage.BABY && raisedPetData.IsPetCustomized())
				{
					UiDragonsAgeUpMenuItem obj = (UiDragonsAgeUpMenuItem)mUiDragonsAgeUpMenu.AddWidget(raisedPetData.Name);
					obj.SetUserData(new AgeUpUserData(raisedPetData));
					obj.SetupWidget();
				}
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (FUEManager.pInstance != null && FUEManager.pIsFUERunning && (bool)FUEManager.pInstance._AgeUpTutorial && FUEManager.pInstance._AgeUpTutorial.IsShowingTutorial())
		{
			return;
		}
		base.OnClick(inWidget);
		if (inWidget == null)
		{
			return;
		}
		if (inWidget.name == _BackButtonName)
		{
			Exit();
			if (mTriggerObject != null)
			{
				mTriggerObject.SendMessage("DoTriggerAction", base.gameObject, SendMessageOptions.DontRequireReceiver);
				mTriggerObject = null;
			}
		}
		else if (inWidget == m_TitanUpgrade || inWidget == m_AdultUpgrade)
		{
			OpenStore();
		}
		else if (inWidget.name == "BtnMissionLocked")
		{
			KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "NotEnoughGemsDB");
			kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
			kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			kAUIGenericDB.SetText(inWidget._TooltipInfo._Text.GetLocalizedString(), interactive: false);
			kAUIGenericDB.SetTitle("");
			KAUI.SetExclusive(kAUIGenericDB);
		}
	}

	private void Exit(bool invokeCallback = true)
	{
		KAUI.RemoveExclusive(this);
		if (KAUIStore.pInstance == null && UiStableQuestMain.pInstance == null)
		{
			AvAvatar.pInputEnabled = true;
			AvAvatar.SetUIActive(inActive: true);
		}
		Object.Destroy(base.gameObject);
		pInstance = null;
		if (invokeCallback && pCloseCallback != null)
		{
			pCloseCallback();
			pCloseCallback = null;
		}
	}

	private void OpenStore()
	{
		SetVisibility(inVisible: false);
		KAUI.RemoveExclusive(this);
		if (mTriggerObject != null)
		{
			mTriggerObject.SendMessage("OnStoreOpened", base.gameObject, SendMessageOptions.DontRequireReceiver);
			mTriggerObject = null;
		}
		if (KAUIStore.pInstance == null)
		{
			StoreLoader.Load(setDefaultMenuItem: true, m_AgeUpStoreInfo._Category, m_AgeUpStoreInfo._Store, (mStoreTriggerObject == null) ? base.gameObject : mStoreTriggerObject);
		}
		if (KAUIStore.pInstance == null)
		{
			AvAvatar.pInputEnabled = true;
			AvAvatar.SetUIActive(inActive: true);
		}
		Object.Destroy(base.gameObject);
		pInstance = null;
		if (pCloseCallback != null)
		{
			pCloseCallback();
			pCloseCallback = null;
		}
	}

	private void OnStoreClosed()
	{
		SetVisibility(inVisible: true);
		KAUI.SetExclusive(this);
		AvAvatar.pInputEnabled = false;
		AvAvatar.SetUIActive(inActive: false);
		UpdateUpgradeAvailibility();
	}

	private void OnIAPStoreClosed()
	{
		OnStoreClosed();
	}

	public void FinishAgeUp(RaisedPetData data, RaisedPetStage fromStage)
	{
		SetVisibility(inVisible: false);
		KAUI.RemoveExclusive(this);
		DragonAgeUpConfig.ShowAgeUpCutscene(data, fromStage, OnDragonCustomizationDone);
		if ((bool)m_AgeUpSound && data.pStage != RaisedPetStage.TITAN)
		{
			SnChannel.Play(m_AgeUpSound);
		}
	}

	private void OnDragonCustomizationDone()
	{
		if (!mCloseOnAgeUp)
		{
			SetVisibility(inVisible: true);
			KAUI.SetExclusive(this);
			AvAvatar.pInputEnabled = false;
			AvAvatar.SetUIActive(inActive: false);
			ShowDragons();
			return;
		}
		mCloseOnAgeUp = false;
		Exit(invokeCallback: false);
		if (mTriggerObject != null)
		{
			mTriggerObject.SendMessage("DoTriggerAction", base.gameObject, SendMessageOptions.DontRequireReceiver);
			mTriggerObject = null;
		}
	}

	public void DoFreeAgeUp(AgeUpUserData data)
	{
		SetInteractive(interactive: false);
		mFreeAgeUpUserData = data;
		mFreeAgeUpFromAge = data.pData.pStage;
		mFreeAgeUpToAge = RaisedPetData.GetNextGrowthStage(data.pData.pStage);
		string displayTextFromPetAge = SanctuaryData.GetDisplayTextFromPetAge(mFreeAgeUpToAge);
		GameUtilities.DisplayGenericDB("PfKAUIGenericDB", string.Format(m_FreeGrowDragonText.GetLocalizedString(), displayTextFromPetAge, displayTextFromPetAge), m_FreeGrowDragonTitleText.GetLocalizedString(), base.gameObject, "OnGrowDragonFree", "ResetFreeAgeup", string.Empty, string.Empty, inDestroyOnClick: true);
	}

	private void ResetFreeAgeup()
	{
		SetInteractive(interactive: true);
		KAUICursorManager.SetDefaultCursor("Arrow");
		mFreeAgeUpUserData = null;
		mFreeAgeUpFromAge = RaisedPetStage.NONE;
		mFreeAgeUpToAge = RaisedPetStage.NONE;
	}

	private void OnGrowDragonFree()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		SanctuaryManager.pInstance.SetAge(mFreeAgeUpUserData.pData, RaisedPetData.GetAgeIndex(mFreeAgeUpToAge));
		mFreeAgeUpUserData.pData.SaveDataReal(OnFreeSetAgeDone);
	}

	private void OnFreeSetAgeDone(SetRaisedPetResponse response)
	{
		if (response != null && response.RaisedPetSetResult == RaisedPetSetResult.Success)
		{
			if (mFreeAgeUpToAge == RaisedPetStage.TITAN)
			{
				UserAchievementTask.Set(SanctuaryManager.pInstance._DragonTitanAchievemetID);
			}
			if (MissionManager.pInstance != null)
			{
				MissionManager.pInstance.CheckForTaskCompletion("Action", "AgeUp", mFreeAgeUpToAge.ToString());
			}
			PetRankData.LoadUserRank(mFreeAgeUpUserData.pData, OnUserRankReady, forceLoad: true);
		}
		else
		{
			SanctuaryManager.pInstance.SetAge(mFreeAgeUpUserData.pData, RaisedPetData.GetAgeIndex(mFreeAgeUpFromAge), inSave: false);
			ResetFreeAgeup();
		}
	}

	private void OnUserRankReady(UserRank rank, object userData)
	{
		FinishAgeUp(mFreeAgeUpUserData.pData, mFreeAgeUpFromAge);
		ResetFreeAgeup();
	}
}
