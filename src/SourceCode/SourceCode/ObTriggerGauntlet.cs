using System;
using System.Collections;
using UnityEngine;

public class ObTriggerGauntlet : ObTriggerDragonCheck
{
	public int _StoreID = 93;

	public int _ItemID = 8711;

	public int _TimeDurationInHours = 1;

	public string TIMES_PLAYED = "FF_PLAYED";

	public string UNLOCK_DURATION = "FF_TIMEREMAINING";

	public int _RetryAttemptsForNonMembers = 2;

	public int _RetryAttemptsForMembers;

	public LocaleString _NotEnoughGemsText = new LocaleString("You dont have enough cash to play. You can buy gems from the store.");

	public LocaleString _UnsuccessfulPurchaseText = new LocaleString("Purchase failed. Please try again later.");

	public LocaleString _StoreNotReadyText = new LocaleString("Store initialisation failed. Please try again later.");

	public LocaleString _BuyGemsOrWaitText = new LocaleString("You have exceeded {{NUM_ATTEMPT}} free attempts. You can either play instantly paying {{GEM_COUNT}} gems or wait for {{DURATION}} hours. Do you want to instant play?");

	protected StoreData mStoreData;

	protected ItemData mItemData;

	protected bool mDoAction;

	protected bool mUpdateWaitTimer;

	protected DateTime mLastPlayedTime;

	protected int pRetryAttempts
	{
		get
		{
			if (!SubscriptionInfo.pIsMember)
			{
				return _RetryAttemptsForNonMembers;
			}
			return _RetryAttemptsForMembers;
		}
	}

	protected virtual void Start()
	{
		ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
	}

	protected override bool CanHandleNoRaisedPet()
	{
		return true;
	}

	protected override void ShowPetTiredDialog(bool isLowEnergy)
	{
		base.ShowPetTiredDialog(isLowEnergy);
		if (UiPetEnergyGenericDB.pInstance != null)
		{
			UiPetEnergyGenericDB.pInstance.UpdateCallback(null, "OnCloseDB");
		}
	}

	protected override void ShowDialog(string inText)
	{
		base.ShowDialog(inText);
		if (mGenericDBUi != null && (IsBlockedStage() || !IsPetTooTired() || !IsUnMountableStateAllowed()))
		{
			mGenericDBUi.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: true);
			mGenericDBUi._CloseMessage = "CloseAndShowAgeUpPrompt";
			mGenericDBUi._OKMessage = "CallTriggerAction";
		}
	}

	protected override void OnShowDialogMessageReceived(string message)
	{
		base.OnShowDialogMessageReceived(message);
		if (!(message == "CloseAndShowAgeUpPrompt"))
		{
			if (message == "CallTriggerAction")
			{
				CallTriggerAction();
			}
		}
		else
		{
			CloseAndShowAgeUpPrompt();
		}
	}

	private void CallTriggerAction()
	{
		if (mGenericDBUi != null)
		{
			UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
		}
		mDoAction = true;
		KAUICursorManager.SetDefaultCursor("Loading");
	}

	protected virtual bool IsBlockedStage()
	{
		if (SanctuaryManager.pCurPetData != null && _BlockedStages != null)
		{
			BlockedStages[] blockedStages = _BlockedStages;
			for (int i = 0; i < blockedStages.Length; i++)
			{
				if (blockedStages[i]._PetStage == SanctuaryManager.pCurPetData.pStage)
				{
					return true;
				}
			}
		}
		return false;
	}

	public void OnStoreLoaded(StoreData sd)
	{
		mStoreData = sd;
	}

	public override void Update()
	{
		base.Update();
		if (mDoAction && mStoreData != null)
		{
			mDoAction = false;
			KAUICursorManager.SetDefaultCursor("Arrow");
			TryLoadingGame();
		}
	}

	protected override void CloseAndShowAgeUpPrompt()
	{
		mUpdateWaitTimer = false;
		base.CloseAndShowAgeUpPrompt();
	}

	protected void TryLoadingGame()
	{
		if (ProductData.pPairData.GetIntValue(TIMES_PLAYED, 0) < pRetryAttempts || pRetryAttempts <= 0)
		{
			DoTriggerAction(null);
		}
		else
		{
			CheckForTime();
		}
	}

	protected void CheckForTime()
	{
		if (string.IsNullOrEmpty(ProductData.pPairData.GetStringValue(UNLOCK_DURATION, null)))
		{
			ProductData.pPairData.SetValueAndSave(TIMES_PLAYED, "0");
			TryLoadingGame();
			return;
		}
		if (GetTimeDiff() > (double)_TimeDurationInHours)
		{
			ProductData.pPairData.SetValueAndSave(TIMES_PLAYED, "0");
			TryLoadingGame();
			return;
		}
		if (mItemData == null)
		{
			mItemData = mStoreData.FindItem(_ItemID);
		}
		ShowDB(_BuyGemsOrWaitText, yes: true, no: true, ok: false);
		StartCoroutine(UpdateWaitTimer(0f));
		mUpdateWaitTimer = true;
	}

	protected IEnumerator UpdateWaitTimer(float waitSecs = 1f)
	{
		yield return new WaitForSeconds(waitSecs);
		if (!mUpdateWaitTimer)
		{
			yield break;
		}
		DateTime dateTime = mLastPlayedTime;
		DateTime dateTime2 = dateTime.Add(new TimeSpan(0, _TimeDurationInHours * 60, 0));
		TimeSpan timeSpan = dateTime2 - ServerTime.pCurrentTime;
		if (timeSpan.TotalMinutes < 0.0)
		{
			mUpdateWaitTimer = false;
			if (mGenericDBUi != null)
			{
				UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
			}
			TryLoadingGame();
			yield return null;
		}
		string newValue = ((timeSpan.Hours > 0) ? $"{timeSpan.Hours:D2}h : {timeSpan.Minutes:D2}m : {timeSpan.Seconds:D2}s" : $"{timeSpan.Minutes:D2}m : {timeSpan.Seconds:D2}s");
		string localizedString = _BuyGemsOrWaitText.GetLocalizedString();
		localizedString = localizedString.Replace("{{DURATION}}", newValue);
		localizedString = localizedString.Replace("{{GEM_COUNT}}", mItemData.FinalCashCost.ToString());
		localizedString = localizedString.Replace("{{NUM_ATTEMPT}}", pRetryAttempts.ToString());
		if (mGenericDBUi != null)
		{
			mGenericDBUi.SetText(localizedString, interactive: false);
		}
		StartCoroutine(UpdateWaitTimer());
	}

	protected double GetTimeDiff()
	{
		string stringValue = ProductData.pPairData.GetStringValue(UNLOCK_DURATION, null);
		mLastPlayedTime = DateTime.MinValue;
		mLastPlayedTime = DateTime.Parse(stringValue, UtUtilities.GetCultureInfo("en-US"));
		return (ServerTime.pCurrentTime - mLastPlayedTime).TotalHours;
	}

	protected void PayToPlay()
	{
		mUpdateWaitTimer = false;
		ItemData itemData = mStoreData.FindItem(_ItemID);
		if (itemData != null)
		{
			if (Money.pCashCurrency > itemData.FinalCashCost)
			{
				PurchaseTicket(2);
				return;
			}
			ShowDB(_NotEnoughGemsText, yes: false, no: false, ok: true, close: true);
			mGenericDBUi._OKMessage = "BuyGemsOnline";
		}
		else
		{
			ShowDB(_StoreNotReadyText, yes: false, no: false, ok: true);
		}
	}

	protected void PurchaseTicket(int inCurrencyType)
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		CommonInventoryData.pInstance.AddPurchaseItem(_ItemID, 1, ItemPurchaseSource.OB_TRIGGER_GAUNTLET.ToString());
		CommonInventoryData.pInstance.DoPurchase(inCurrencyType, _StoreID, OnTicketPurchaseDone);
	}

	public void OnTicketPurchaseDone(CommonInventoryResponse response)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (response != null && response.Success)
		{
			ProductData.pPairData.SetValueAndSave(TIMES_PLAYED, "0");
			TryLoadingGame();
		}
		else
		{
			ShowDB(_UnsuccessfulPurchaseText, yes: false, no: false, ok: true);
		}
	}

	protected virtual void ShowDB(LocaleString localeString, bool yes, bool no, bool ok, bool close = false)
	{
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB);
		}
		if (mGenericDBUi != null)
		{
			UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mGenericDBUi = gameObject.GetComponent<KAUIGenericDB>();
		mGenericDBUi._MessageObject = base.gameObject;
		mGenericDBUi._OKMessage = "OkMessage";
		mGenericDBUi._YesMessage = "PayToPlay";
		mGenericDBUi._NoMessage = "OkMessage";
		mGenericDBUi._CloseMessage = "OkMessage";
		mGenericDBUi.SetTextByID(localeString._ID, localeString._Text, interactive: false);
		mGenericDBUi.SetButtonVisibility(yes, no, ok, close);
		mGenericDBUi.SetDestroyOnClick(isDestroy: true);
	}

	public void OnCloseDB()
	{
		if (AvAvatar.pState == AvAvatarState.PAUSED)
		{
			AvAvatar.pState = AvAvatar.pPrevState;
			AvAvatar.SetUIActive(inActive: true);
		}
		if (mUiGenericDB != null)
		{
			UnityEngine.Object.Destroy(mUiGenericDB);
		}
		if (mGenericDBUi != null)
		{
			UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
		}
	}

	protected void BuyGemsOnline()
	{
		if (mGenericDBUi != null)
		{
			UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
		}
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	public void OnIAPStoreClosed()
	{
		PayToPlay();
	}
}
