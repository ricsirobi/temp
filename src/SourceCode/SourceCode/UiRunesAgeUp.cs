using UnityEngine;

public class UiRunesAgeUp : KAUI
{
	public delegate void OnRunesAgeUpDone();

	public LocaleString _RunesAvailableText = new LocaleString("Collect runes in Stable Quests to age up to a Titan!");

	public LocaleString _AgeUpConfirmationText = new LocaleString("Are you sure you want to age up your dragon {{DragonName}} to a Titan by spending {{RunesCount}} Rune stones?");

	public int _AdultToTitanItemID = 12823;

	public int _AgeUpStoreID = 102;

	private int mRuneID;

	private KAWidget mTxtRequiredRunes;

	private KAWidget mTxtAvailRunes;

	private KAWidget mBtnAgeUp;

	private KAWidget mBtnAgeUpUpsell;

	private KAWidget mBtnClose;

	private KAWidget mBtnStableQuest;

	private KAWidget mAvailRunesProgressBar;

	private int mAvailableRunes;

	private int mRequiredRunes = 200;

	private OnRunesAgeUpDone mCallback;

	private KAUINPCGenericDB mKAUIGenericDB;

	private bool mDestroyUI;

	private RaisedPetStage mAgeUpStage = RaisedPetStage.ADULT;

	private RaisedPetStage mFromStage;

	public string pNPCName { get; set; }

	public void Init(int inRuneID, int inRequiredRune, RaisedPetStage inStage, OnRunesAgeUpDone inCallback)
	{
		mRuneID = inRuneID;
		mRequiredRunes = inRequiredRune;
		mAgeUpStage = inStage;
		mCallback = inCallback;
		mFromStage = SanctuaryManager.pCurPetData.pStage;
	}

	protected override void Start()
	{
		base.Start();
		if (CommonInventoryData.pIsReady)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(mRuneID);
			if (userItemData != null)
			{
				mAvailableRunes = userItemData.Quantity;
			}
		}
		mTxtRequiredRunes = FindItem("TxtRunesRequire");
		mTxtAvailRunes = FindItem("TxtRunesAvailable");
		mBtnAgeUp = FindItem("BtnAgeUp");
		mBtnAgeUpUpsell = FindItem("BtnAgeUpUpsell");
		mBtnStableQuest = FindItem("BtnStableQuest");
		mBtnClose = FindItem("BtnClose");
		mAvailRunesProgressBar = FindItem("ProgressBar");
		if (mTxtRequiredRunes != null)
		{
			mTxtRequiredRunes.SetText(_RunesAvailableText.GetLocalizedString());
		}
		if (mTxtAvailRunes != null)
		{
			mTxtAvailRunes.SetText(mAvailableRunes + " / " + mRequiredRunes);
		}
		if (mAvailRunesProgressBar != null)
		{
			mAvailRunesProgressBar.SetProgressLevel((mAvailableRunes > mRequiredRunes) ? 1f : ((float)mAvailableRunes / (float)mRequiredRunes));
		}
		if (mBtnAgeUp != null)
		{
			if (mAvailableRunes < mRequiredRunes)
			{
				mBtnAgeUp.SetVisibility(inVisible: false);
				mBtnAgeUpUpsell.SetVisibility(inVisible: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				ItemStoreDataLoader.Load(_AgeUpStoreID, OnAgeUpStoreLoaded);
			}
			else
			{
				mBtnAgeUp.SetVisibility(inVisible: true);
				mBtnAgeUpUpsell.SetVisibility(inVisible: false);
			}
		}
		if (SanctuaryManager.pCurPetInstance != null)
		{
			SanctuaryManager.pInstance.TakePicture(SanctuaryManager.pCurPetInstance.gameObject, base.gameObject, inSendPicture: false);
			if (SanctuaryManager.pInstance.pPetMeter != null)
			{
				SanctuaryManager.pInstance.pPetMeter.RefreshAll();
			}
		}
	}

	private void OnAgeUpStoreLoaded(StoreData sd)
	{
		if (sd == null)
		{
			return;
		}
		ItemData itemData = sd.FindItem(_AdultToTitanItemID);
		if (itemData != null)
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			KAWidget kAWidget = mBtnAgeUpUpsell.FindChildItem("TxtAgeUpCost");
			if (kAWidget != null)
			{
				kAWidget.SetText(itemData.FinalCashCost.ToString());
			}
			mBtnAgeUpUpsell.SetVisibility(inVisible: true);
		}
	}

	private void OnPetPictureDone(object inImage)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (inImage != null)
		{
			KAWidget kAWidget = FindItem("IcoDragonPic");
			if (kAWidget != null)
			{
				kAWidget.SetTexture(inImage as Texture);
			}
		}
		if (mDestroyUI)
		{
			DestroyUI();
		}
	}

	private void OnPetPictureDoneFailed()
	{
		UtDebug.LogError("Failed to get Pet Picture");
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mDestroyUI)
		{
			DestroyUI();
		}
	}

	private void DestroyUI()
	{
		if (mCallback != null)
		{
			mCallback();
		}
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
	}

	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
		if (inItem == mBtnClose)
		{
			if (mCallback != null)
			{
				mCallback();
			}
			KAUI.RemoveExclusive(this);
			Object.Destroy(base.gameObject);
		}
		else if (inItem == mBtnAgeUp)
		{
			string text = _AgeUpConfirmationText.GetLocalizedString().Replace("{{RunesCount}}", mRequiredRunes.ToString());
			text = text.Replace("{{DragonName}}", SanctuaryManager.pCurPetData.Name);
			ShowKAUIDialog("PfKAUINPCGenericDB", "NPCAgeUp", "ConfirmAgeUp", "DestroyDB", "", "", destroyDB: true, text, base.gameObject);
		}
		else if (inItem == mBtnAgeUpUpsell)
		{
			if (mCallback != null)
			{
				mCallback();
			}
			SetVisibility(inVisible: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			AvAvatar.SetUIActive(inActive: false);
			DragonAgeUpConfig.ShowAgeUpUI(OnDragonAgeUpDone);
		}
		else if (inItem == mBtnStableQuest)
		{
			if (mCallback != null)
			{
				mCallback();
			}
			KAUI.RemoveExclusive(this);
			Object.Destroy(base.gameObject);
			StableData byPetID = StableData.GetByPetID(SanctuaryManager.pCurPetData.RaisedPetID);
			if (byPetID != null)
			{
				StableManager.LoadStableWithJobBoard(byPetID.ID);
			}
			else
			{
				StableManager.LoadStableWithJobBoard(0);
			}
		}
	}

	private void OnDragonAgeUpDone()
	{
		AvAvatar.pState = AvAvatarState.IDLE;
		AvAvatar.SetUIActive(inActive: true);
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
	}

	private void ConfirmAgeUp()
	{
		if (!(SanctuaryManager.pCurPetInstance == null))
		{
			SetInteractive(interactive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			SanctuaryManager.pInstance.SetAge(SanctuaryManager.pCurPetData, RaisedPetData.GetAgeIndex(mAgeUpStage));
			CommonInventoryRequest[] array = new CommonInventoryRequest[1]
			{
				new CommonInventoryRequest()
			};
			array[0].ItemID = mRuneID;
			array[0].Quantity = mRequiredRunes * -1;
			SanctuaryManager.pCurPetData.SaveDataReal(OnSetAgeDone, array);
		}
	}

	public void OnSetAgeDone(SetRaisedPetResponse response)
	{
		if (response != null && response.RaisedPetSetResult == RaisedPetSetResult.Success)
		{
			SanctuaryManager.pInstance.CreateCurrentPet(SanctuaryManager.pCurPetInstance.pData, RaisedPetData.GetAgeIndex(SanctuaryManager.pCurPetInstance.pData.pStage), base.gameObject);
			SanctuaryManager.pInstance.pSetFollowAvatar = true;
			if (CommonInventoryData.pInstance.RemoveItem(mRuneID, updateServer: false, mRequiredRunes) >= 0)
			{
				CommonInventoryData.pInstance.ClearSaveCache();
			}
			if (mAgeUpStage == RaisedPetStage.TITAN)
			{
				UserAchievementTask.Set(SanctuaryManager.pInstance._DragonTitanAchievemetID);
			}
			mDestroyUI = true;
		}
		else
		{
			SanctuaryManager.pInstance.SetAge(SanctuaryManager.pCurPetData, RaisedPetData.GetAgeIndex(mFromStage), inSave: false);
			KAUICursorManager.SetDefaultCursor("Arrow");
			DestroyUI();
		}
	}

	public void ShowKAUIDialog(string assetName, string dbName, string yesMessage, string noMessage, string okMessage, string closeMessage, bool destroyDB, string messageTxt, GameObject msgObject = null)
	{
		mKAUIGenericDB = (KAUINPCGenericDB)GameUtilities.CreateKAUIGenericDB(assetName, dbName);
		if (mKAUIGenericDB != null)
		{
			if (msgObject == null)
			{
				msgObject = base.gameObject;
			}
			mKAUIGenericDB.SetMessage(msgObject, yesMessage, noMessage, okMessage, closeMessage);
			mKAUIGenericDB.SetDestroyOnClick(destroyDB);
			mKAUIGenericDB.SetButtonVisibility(!string.IsNullOrEmpty(yesMessage), !string.IsNullOrEmpty(noMessage), !string.IsNullOrEmpty(okMessage), !string.IsNullOrEmpty(closeMessage));
			mKAUIGenericDB.SetText(messageTxt, interactive: false);
			mKAUIGenericDB.SetNPCIcon(pNPCName);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void DestroyDB()
	{
		if (!(mKAUIGenericDB == null))
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}
}
