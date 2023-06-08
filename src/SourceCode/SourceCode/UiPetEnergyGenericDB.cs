using UnityEngine;

public class UiPetEnergyGenericDB : KAUIGenericDB, IAdResult
{
	public LocaleString _NotEnoughFeeText = new LocaleString("You do not have enough gems to pay, Please buy more!");

	public LocaleString _PurchaseProcessingText = new LocaleString("Processing purchase.");

	public LocaleString _EnergyPurchaseSuccessText = new LocaleString("Energy purchase successful.");

	public LocaleString _EnergyRewardSuccessText = new LocaleString("[Review]Energy reward successful.");

	public LocaleString _EnergyPurchaseFailedText = new LocaleString("Energy purchase failed.");

	public LocaleString _HappinessLowText = new LocaleString("[Review]Your pet is too sad to play. Play with him.");

	public LocaleString _EnergyLowText = new LocaleString("Your dragon does not have enough energy would you like to buy {{GEMS}} gems to restore?");

	public LocaleString _EnergyItemLoadFailedText = new LocaleString("[Review]Your pet is too tired to play. Feed him a fish.");

	public string _AdWatchedMessage = "";

	public AdEventType _AdEventType;

	public int _BtnOffsetY;

	public int _TxtOffsetY;

	public int _RechargeEnergyItemID;

	public int _StoreID;

	private bool mGemsPurchase;

	private int mRechargeEnergyCost;

	private string mYesMessage;

	private string mNoMessage;

	private RaisedPetData mPetData;

	private static UiPetEnergyGenericDB mInstance;

	public static UiPetEnergyGenericDB pInstance => mInstance;

	private void ShowAdsWidget(bool show)
	{
		KAWidget kAWidget = FindItem("BtnAds");
		if (kAWidget.GetVisibility() != show)
		{
			kAWidget.SetVisibility(show);
			KAWidget kAWidget2 = FindItem("YesBtn");
			KAWidget kAWidget3 = FindItem("NoBtn");
			KAWidget kAWidget4 = FindItem("TxtDialog");
			if (show)
			{
				kAWidget2.SetPosition(kAWidget2.GetPosition().x, kAWidget2.GetPosition().y + (float)_BtnOffsetY);
				kAWidget3.SetPosition(kAWidget3.GetPosition().x, kAWidget3.GetPosition().y + (float)_BtnOffsetY);
				kAWidget4.SetPosition(kAWidget4.GetPosition().x, kAWidget4.GetPosition().y + (float)_TxtOffsetY);
			}
			else
			{
				kAWidget2.SetPosition(kAWidget2.GetPosition().x, kAWidget2.GetPosition().y - (float)_BtnOffsetY);
				kAWidget3.SetPosition(kAWidget3.GetPosition().x, kAWidget3.GetPosition().y - (float)_BtnOffsetY);
				kAWidget4.SetPosition(kAWidget4.GetPosition().x, kAWidget4.GetPosition().y - (float)_TxtOffsetY);
			}
		}
	}

	public static void Show(GameObject go, string yesMessage, string noMessage, bool isLowEnergy, RaisedPetData petData = null)
	{
		mInstance = Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiPetEnergyDB")).GetComponent<UiPetEnergyGenericDB>();
		mInstance.Init(go, yesMessage, noMessage, isLowEnergy, petData);
	}

	private void Init(GameObject messageObject, string yesMessage, string noMessage, bool isLowEnergy, RaisedPetData petData)
	{
		SetExclusive();
		_MessageObject = messageObject;
		mYesMessage = yesMessage;
		mNoMessage = noMessage;
		mPetData = petData;
		if (isLowEnergy)
		{
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			ItemData.Load(_RechargeEnergyItemID, OnLoadItemDataReady, null);
		}
		else
		{
			SetText(_HappinessLowText.GetLocalizedString(), interactive: false);
			SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			SetVisibility(inVisible: true);
		}
	}

	public void OnLoadItemDataReady(int itemID, ItemData itemData, object inUserData)
	{
		SetVisibility(inVisible: true);
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
		if (itemData == null)
		{
			SetText(_EnergyItemLoadFailedText.GetLocalizedString(), interactive: false);
			SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			mYesMessage = mNoMessage;
			return;
		}
		mRechargeEnergyCost = itemData.FinalCashCost;
		string inText = _EnergyLowText.GetLocalizedString().Replace("{{GEMS}}", mRechargeEnergyCost.ToString());
		SetText(inText, interactive: false);
		SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		if (AdManager.pInstance.AdSupported(_AdEventType, AdType.REWARDED_VIDEO))
		{
			ShowAdsWidget(show: true);
		}
	}

	private void CloseDB()
	{
		RemoveExclusive();
		Object.Destroy(base.gameObject);
		mInstance = null;
	}

	public void OnAdWatched()
	{
		SetInteractive(interactive: true);
		AdManager.pInstance.LogAdWatchedEvent(_AdEventType, "refill energy");
		ShowAdsWidget(show: false);
		AdManager.pInstance.SyncAdAvailableCount(_AdEventType, isConsumed: true);
		SetText(_EnergyRewardSuccessText.GetLocalizedString(), interactive: false);
		SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		RefillEnergy();
	}

	public void OnAdFailed()
	{
		SetInteractive(interactive: true);
		UtDebug.LogError("OnAdFailed for event:- " + _AdEventType);
	}

	public void OnAdSkipped()
	{
		SetInteractive(interactive: true);
	}

	public void OnAdClosed()
	{
	}

	public void OnAdFinished(string eventDataRewardString)
	{
	}

	public void OnAdCancelled()
	{
	}

	private void RefillEnergy()
	{
		if (mPetData == null)
		{
			float maxMeter = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, SanctuaryManager.pCurPetData);
			SanctuaryManager.pCurPetInstance.SetMeter(SanctuaryPetMeterType.ENERGY, maxMeter, forceUpdate: true);
			SanctuaryManager.pCurPetInstance.pData.SaveDataReal(null, null, savePetMeterAlone: true);
		}
		else
		{
			float maxMeter2 = SanctuaryData.GetMaxMeter(SanctuaryPetMeterType.ENERGY, mPetData);
			StateUpdate(mPetData, maxMeter2);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item.name == "BtnAds" && AdManager.pInstance.AdAvailable(_AdEventType, AdType.REWARDED_VIDEO))
		{
			SetInteractive(interactive: false);
			AdManager.DisplayAd(_AdEventType, AdType.REWARDED_VIDEO, base.gameObject);
		}
		else if (item.name == "YesBtn")
		{
			if (mGemsPurchase)
			{
				BuyGemsOnline();
				return;
			}
			ShowAdsWidget(show: false);
			PurchaseEnergy();
		}
		else if (item.name == "NoBtn")
		{
			if (mGemsPurchase)
			{
				mGemsPurchase = false;
				string inText = _EnergyLowText.GetLocalizedString().Replace("{{GEMS}}", mRechargeEnergyCost.ToString());
				SetText(inText, interactive: false);
				if (AdManager.pInstance.AdAvailable(_AdEventType, AdType.REWARDED_VIDEO))
				{
					ShowAdsWidget(show: true);
				}
			}
			else
			{
				if (!string.IsNullOrEmpty(mNoMessage))
				{
					_MessageObject.SendMessage(mNoMessage, SendMessageOptions.DontRequireReceiver);
				}
				CloseDB();
			}
		}
		else if (item.name == "OKBtn")
		{
			if (!string.IsNullOrEmpty(mYesMessage))
			{
				_MessageObject.SendMessage(mYesMessage, SendMessageOptions.DontRequireReceiver);
			}
			CloseDB();
		}
	}

	private void PurchaseEnergy()
	{
		if (Money.pCashCurrency >= mRechargeEnergyCost)
		{
			SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			UICursorManager.SetCursor("Loading", showHideSystemCursor: true);
			SetText(_PurchaseProcessingText.GetLocalizedString(), interactive: false);
			CommonInventoryData.pInstance.AddPurchaseItem(_RechargeEnergyItemID, 1, ItemPurchaseSource.UI_PET_ENERGY.ToString());
			CommonInventoryData.pInstance.DoPurchase(2, _StoreID, EnergyPurchaseDone);
		}
		else
		{
			mGemsPurchase = true;
			SetText(_NotEnoughFeeText.GetLocalizedString(), interactive: false);
		}
	}

	private void BuyGemsOnline()
	{
		SetVisibility(inVisible: false);
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	private void EnergyPurchaseDone(CommonInventoryResponse ret)
	{
		SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		if (ret != null && ret.Success)
		{
			SetText(_EnergyPurchaseSuccessText.GetLocalizedString(), interactive: false);
			RefillEnergy();
		}
		else
		{
			SetText(_EnergyPurchaseFailedText.GetLocalizedString(), interactive: false);
			mYesMessage = mNoMessage;
		}
		UICursorManager.SetCursor("Arrow", showHideSystemCursor: true);
	}

	private void StateUpdate(RaisedPetData petData, float value)
	{
		RaisedPetState[] states = petData.States;
		foreach (RaisedPetState raisedPetState in states)
		{
			if (raisedPetState.Key == SanctuaryPetMeterType.ENERGY.ToString())
			{
				raisedPetState.Value = value;
			}
		}
	}

	private void OnIAPStoreClosed()
	{
		SetVisibility(inVisible: true);
		mGemsPurchase = false;
		PurchaseEnergy();
	}

	public void UpdateCallback(string yesMessage, string noMessage)
	{
		if (yesMessage != null)
		{
			mYesMessage = yesMessage;
		}
		if (mNoMessage != null)
		{
			mNoMessage = noMessage;
		}
	}
}
