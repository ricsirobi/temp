using System;
using UnityEngine;

public class KAUIIAPStoreSyncPopUp : KAUIPopup
{
	[NonSerialized]
	public GameObject _MessageObject;

	public string _ExitLevelName = "ProfileSelectionDO";

	public LocaleString _InitialStatusText = new LocaleString("Please wait while your purchase is being processed.");

	public LocaleString _PurchaseFailedText = new LocaleString("Your purchase failed, please try again later.");

	public LocaleString _PurchaseConfirmationText = new LocaleString("Do you want to proceed to purchase this item?");

	public LocaleString _NotEnoughCashCurrencyText = new LocaleString("You do not have enough Gems.");

	public LocaleString _NotEnoughCurrencyTitleText = new LocaleString("Error!");

	public LocaleString _PurchaseSuccessText = new LocaleString("Your purchase is completed. Started syncing to the server");

	public LocaleString _SyncFailedText = new LocaleString("Server is down!. Your purchase can sync later.");

	public LocaleString _SyncSuccessText = new LocaleString("Your purchase is completed. Synced to Server");

	public LocaleString _GuestUserRegisterText = new LocaleString("IAP purchases cannot be restored for guest accounts. Register an account to sync your IAP purchases.");

	protected KAWidget mProgress;

	protected KAWidget mContinue;

	protected KAWidget mRegister;

	protected KAWidget mStatus;

	private IAPPurchaseStatus mCurrentStatus;

	protected override void Start()
	{
		base.Start();
		mProgress = FindItem("aniProgress");
		mContinue = FindItem("btnContinue");
		mRegister = FindItem("BtnRegister");
		mStatus = FindItem("txtSyncStatus");
	}

	public void ShowPopup(IAPPurchaseStatus inPurchaseStatus)
	{
		switch (inPurchaseStatus)
		{
		case IAPPurchaseStatus.INITIATED:
			mProgress.SetVisibility(inVisible: true);
			mContinue.SetVisibility(inVisible: false);
			mStatus.SetTextByID(_InitialStatusText._ID, _InitialStatusText._Text);
			mRegister.SetVisibility(inVisible: false);
			break;
		case IAPPurchaseStatus.SUCCEDED:
			mStatus.SetTextByID(_PurchaseSuccessText._ID, _PurchaseSuccessText._Text);
			mProgress.SetVisibility(inVisible: true);
			mContinue.SetVisibility(inVisible: false);
			break;
		case IAPPurchaseStatus.FAILED:
			mStatus.SetTextByID(_PurchaseFailedText._ID, _PurchaseFailedText._Text);
			mProgress.SetVisibility(inVisible: false);
			mContinue.SetVisibility(inVisible: true);
			break;
		case IAPPurchaseStatus.SYNC_FAILED:
			mStatus.SetTextByID(_SyncFailedText._ID, _SyncFailedText._Text);
			mProgress.SetVisibility(inVisible: false);
			mContinue.SetVisibility(inVisible: true);
			break;
		case IAPPurchaseStatus.SYNC_SUCCEDED:
			mStatus.SetTextByID(_SyncSuccessText._ID, _SyncSuccessText._Text);
			mProgress.SetVisibility(inVisible: false);
			mContinue.SetVisibility(inVisible: true);
			break;
		case IAPPurchaseStatus.FORCE_REGISTER:
			mStatus.SetTextByID(_GuestUserRegisterText._ID, _GuestUserRegisterText._Text);
			mProgress.SetVisibility(inVisible: false);
			mContinue.SetVisibility(inVisible: true);
			mRegister.SetVisibility(inVisible: true);
			break;
		}
		mCurrentStatus = inPurchaseStatus;
	}

	public void ExitPopUp()
	{
		SetVisibility(t: false);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("SyncPopupClosed", null, SendMessageOptions.DontRequireReceiver);
		}
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mContinue)
		{
			if (mCurrentStatus == IAPPurchaseStatus.SYNC_SUCCEDED && UiLogin.pIsGuestUser)
			{
				ShowPopup(IAPPurchaseStatus.FORCE_REGISTER);
			}
			else if (_MessageObject != null)
			{
				SetVisibility(t: false);
				_MessageObject.SendMessage("SyncPopupClosed", null, SendMessageOptions.DontRequireReceiver);
			}
			if (mCurrentStatus == IAPPurchaseStatus.SYNC_FAILED && (UtPlatform.IsMobile() || UtPlatform.IsWSA()))
			{
				SetVisibility(t: false);
			}
		}
		else if (item == mRegister)
		{
			SetVisibility(t: false);
			GameUtilities.LoadLoginLevel(showRegstration: true);
		}
	}
}
