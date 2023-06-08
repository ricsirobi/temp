using System;
using System.Collections;
using System.Collections.Generic;
using KA.Framework;
using UnityEngine;

public class IAPManager : MonoBehaviour
{
	[Serializable]
	public class StoreXMLPath
	{
		public ProductPlatform _Platform;

		public string _XMLPath;
	}

	[Serializable]
	public class MembershipUpgradeText
	{
		public ProductPlatform _Platform;

		public LocaleString _Text;
	}

	[Serializable]
	public enum ReceiptStatus
	{
		None,
		Successful,
		PartialSuccessful,
		StoreFailed,
		ServerFailed
	}

	[Serializable]
	public class RestoreReceiptStatus
	{
		public ReceiptStatus _Status;

		public LocaleString _Text;
	}

	public string _IAPStoreSettingsLocal = "RS_DATA/DragonsStoreData.xml";

	public List<StoreXMLPath> _StoreXMLPaths;

	public List<MembershipUpgradeText> _MembershipUpgradeText;

	public List<RestoreReceiptStatus> _RestoreReceiptStatusTexts;

	public LocaleString _StoreUnavailableText = new LocaleString("The store is unavailable at this time. Please try again later.");

	public LocaleString _PurchaseFailedText = new LocaleString("Purchase failed.");

	public LocaleString _ReceiptExpiredProcessTitleText = new LocaleString("Purchase Expired");

	public LocaleString _CurrentReceiptExpiredProcessText = new LocaleString("Last purchase was expired. Do you wish to make a new purchase?");

	public LocaleString _IncompleteCurrentReceiptProcessText = new LocaleString("There was an issue with validating your purchase. Do you wish to try to validate the purchase again?");

	public LocaleString _IncompleteReceiptProcessTitleText = new LocaleString("Purchase Validation");

	public LocaleString _IAPSynchWaitText = new LocaleString("Synchronizing pending purchases. Please Wait...");

	public LocaleString _IAPSynchFailedText = new LocaleString("Synchronizing purchases failed. Try next time...");

	public LocaleString _IAPSynchSuccessText = new LocaleString("Synchronizing purchase successful. Gems will reflect now...");

	public LocaleString _IAPUnAssignedReceiptsText = new LocaleString("You have pending purchases. Please select a Viking to receive the item(s)");

	private static IAPManager mInstance = null;

	private static IAPStoreData mIAPStoreData;

	private static bool mIsReady = false;

	private static bool mProductsReceived = false;

	public float _InitializationTimeLimit = 10f;

	private static int mSyncPendingIAPCount = 0;

	private static bool mInitialized = false;

	private static IAPProvider mIAPProvider = null;

	private static List<GameObject> mMessageObjects = new List<GameObject>();

	private bool mSyncDone;

	private bool mPendingSyncInitiated;

	private bool mPendingSyncDone;

	private int mPendingIAPCount;

	private bool mWaitSubscriptionRefresh;

	private bool mInitiatedRestorePurchases;

	private bool mPurchaseInitiated;

	private int mPendingRestorePurchases;

	private int mRestoreSuccessNum;

	private int mRestoreFailedNum;

	private Receipt mCurrentPendingReceipt;

	private IAPItemData mCurrentIAPItem;

	private List<Receipt> mCachedReceipts;

	private List<Receipt> mUnAssignedReceipts;

	private const string IAPReceiptsKey = "IAPReceipts";

	public static IAPManager pInstance => mInstance;

	public static IAPStoreData pIAPStoreData => mIAPStoreData;

	public static bool pIsReady => mIsReady;

	public static bool pIsAvailable
	{
		get
		{
			if (mIsReady)
			{
				return mProductsReceived;
			}
			return false;
		}
	}

	private event OnSyncComplete OnSyncDone;

	private void Awake()
	{
		if (mInstance != null)
		{
			UnityEngine.Object.Destroy(base.gameObject);
			return;
		}
		mInstance = this;
		UnityEngine.Object.DontDestroyOnLoad(base.gameObject);
	}

	public static void SetProvider(IAPProvider provider)
	{
		if (mIAPProvider != null)
		{
			IAPProvider iAPProvider = mIAPProvider;
			iAPProvider.OnProductsReceived = (Action<bool>)Delegate.Remove(iAPProvider.OnProductsReceived, new Action<bool>(mInstance.OnProductsReceivedCallback));
			IAPProvider iAPProvider2 = mIAPProvider;
			iAPProvider2.OnPurchaseDone = (Action<PurchaseStatus, string, Receipt>)Delegate.Remove(iAPProvider2.OnPurchaseDone, new Action<PurchaseStatus, string, Receipt>(mInstance.OnPurchaseDoneCallback));
		}
		mIAPProvider = provider;
		if (mIAPProvider != null)
		{
			IAPProvider iAPProvider3 = mIAPProvider;
			iAPProvider3.OnProductsReceived = (Action<bool>)Delegate.Combine(iAPProvider3.OnProductsReceived, new Action<bool>(mInstance.OnProductsReceivedCallback));
			IAPProvider iAPProvider4 = mIAPProvider;
			iAPProvider4.OnPurchaseDone = (Action<PurchaseStatus, string, Receipt>)Delegate.Combine(iAPProvider4.OnPurchaseDone, new Action<PurchaseStatus, string, Receipt>(mInstance.OnPurchaseDoneCallback));
		}
	}

	private void OnProductsReceivedCallback(bool inSuccess)
	{
		mIsReady = true;
		mProductsReceived = inSuccess;
	}

	private void OnPurchaseDoneCallback(PurchaseStatus status, string idOrError, Receipt receipt)
	{
		WsTokenMonitor.pReloadSceneAllowed = true;
		Message("OnPurchaseDoneStatusEvent", status);
		switch (status)
		{
		case PurchaseStatus.FAILED:
			mPurchaseInitiated = false;
			if (string.IsNullOrEmpty(idOrError))
			{
				idOrError = _PurchaseFailedText.GetLocalizedString();
			}
			Message("OnPurchaseFailed", idOrError);
			break;
		case PurchaseStatus.CANCELLED:
			mPurchaseInitiated = false;
			Message("OnPurchaseCancelled", idOrError);
			break;
		case PurchaseStatus.SUCCESSFUL:
			if (receipt != null)
			{
				ReceiptRedemptionRequest receiptRedemptionRequest = new ReceiptRedemptionRequest();
				receiptRedemptionRequest.PaymentReceipt = receipt;
				mCurrentPendingReceipt = receipt;
				if (IAPConsole._ReceiptIncomplete)
				{
					IAPConsole._ReceiptIncomplete = false;
					if (mPurchaseInitiated)
					{
						ShowCurrentReceiptValidationPrompt();
					}
					break;
				}
				if (mIAPStoreData.GetIAPCategoryType(receipt.ItemID) == IAPCategoryType.Item)
				{
					if (UserInfo.pInstance != null && !string.IsNullOrEmpty(UserInfo.pInstance.UserID))
					{
						receiptRedemptionRequest.PaymentReceipt.UserID = new Guid(UserInfo.pInstance.UserID);
					}
					AddToPendingReceipts(receipt);
				}
				if (IAPConsole._BlockSyncWithServer)
				{
					UtDebug.Log("*****Sync to server is blocked****");
					OnSyncFailed(null);
					IAPConsole._BlockSyncWithServer = false;
					mPurchaseInitiated = false;
				}
				else
				{
					WsWebService.RedeemReceipt(receiptRedemptionRequest, ServiceEventHandler, receipt);
					Message("OnPurchaseSuccessful", idOrError);
				}
			}
			else if (!mInitiatedRestorePurchases)
			{
				mPurchaseInitiated = false;
				if (pIAPStoreData.GetIAPCategoryType(idOrError) == IAPCategoryType.Membership)
				{
					ParentData.Reset();
					ParentData.Init();
					SubscriptionInfo.Reset();
					SubscriptionInfo.Init();
					UserProfile.Reset();
					UserProfile.Init();
					mSyncDone = true;
					mWaitSubscriptionRefresh = true;
					Message("OnPurchaseSuccessful", idOrError);
				}
				else
				{
					Message("OnPurchaseSuccessful", idOrError);
					OnSyncSuccess(null);
				}
			}
			break;
		}
	}

	public static void RemoveProvider(IAPProvider provider)
	{
		if (mIAPProvider == provider)
		{
			mIAPProvider = null;
		}
	}

	public void Update()
	{
		if (mSyncDone && mWaitSubscriptionRefresh && SubscriptionInfo.pIsReady && ParentData.pIsReady)
		{
			OnSyncSuccess(null);
			mSyncDone = false;
			mWaitSubscriptionRefresh = false;
		}
		if (!mPendingSyncInitiated || !mPendingSyncDone)
		{
			return;
		}
		if (mWaitSubscriptionRefresh)
		{
			if (SubscriptionInfo.pIsReady && ParentData.pIsReady)
			{
				mPendingSyncDone = false;
				mPendingSyncInitiated = false;
				mWaitSubscriptionRefresh = false;
				mCurrentPendingReceipt = null;
				if (mPendingIAPCount == 0)
				{
					OnSyncPendingSuccess();
				}
				else
				{
					OnSyncPendingFailed();
				}
			}
		}
		else
		{
			mPendingSyncDone = false;
			mPendingSyncInitiated = false;
			mCurrentPendingReceipt = null;
			if (mPendingIAPCount == 0)
			{
				OnSyncPendingSuccess();
			}
			else
			{
				OnSyncPendingFailed();
			}
		}
	}

	private void Start()
	{
		if (mInitialized)
		{
			return;
		}
		mPendingIAPCount = 0;
		UtDebug.Log("Load Dragons IAP Store Settings from CDN");
		string text = _IAPStoreSettingsLocal;
		if (_StoreXMLPaths != null)
		{
			ProductPlatform platform = ProductSettings.GetPlatform();
			StoreXMLPath storeXMLPath = _StoreXMLPaths.Find((StoreXMLPath data) => data._Platform == platform);
			if (storeXMLPath != null && !string.IsNullOrEmpty(storeXMLPath._XMLPath))
			{
				text = storeXMLPath._XMLPath;
			}
		}
		UtDebug.Log("Load DragonsStoreData xml from : " + text);
		RsResourceManager.Load(text, XmlLoadEventHandler);
		mInitialized = true;
	}

	public static void Reset()
	{
		if (mInstance != null)
		{
			mMessageObjects.Clear();
			mMessageObjects = new List<GameObject>();
			mSyncPendingIAPCount = 0;
			mInstance.mPendingIAPCount = 0;
			mInstance.mSyncDone = false;
			mInstance.mPendingSyncInitiated = false;
			mInstance.mPendingSyncDone = false;
			mInstance.mWaitSubscriptionRefresh = false;
			mInstance.OnSyncDone = null;
			mInstance.mPendingRestorePurchases = 0;
			mInstance.mRestoreSuccessNum = 0;
			mInstance.mRestoreFailedNum = 0;
			mInstance.mInitiatedRestorePurchases = false;
			mInstance.mCurrentPendingReceipt = null;
			mInstance.mPurchaseInitiated = false;
		}
	}

	public void XmlLoadEventHandler(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			if (inObject == null)
			{
				break;
			}
			UtDebug.Log("Dragons IAP Store settings downloaded!!!");
			mIAPStoreData = UtUtilities.DeserializeFromXml<IAPStoreData>((string)inObject);
			IAPItemCategory[] categoryData = mIAPStoreData.CategoryData;
			for (int i = 0; i < categoryData.Length; i++)
			{
				IAPItemData[] itemDataList = categoryData[i].ItemDataList;
				foreach (IAPItemData obj in itemDataList)
				{
					obj.PriceInUSD = obj.FormattedPrice;
				}
			}
			IAPInit(mIAPStoreData.GetListOfProductIDs());
			break;
		}
		case RsResourceLoadEvent.ERROR:
			mIAPStoreData = null;
			UtDebug.LogError("Dragons IAP Store settings missing!!!");
			break;
		}
	}

	public void IAPInit(string[] productIdentifiers)
	{
		if (mIAPProvider != null)
		{
			mIAPProvider.Init(productIdentifiers, mIAPStoreData);
		}
		else
		{
			mIsReady = true;
		}
	}

	private void UnAssignedPendingServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			mSyncPendingIAPCount--;
			break;
		case WsServiceEvent.COMPLETE:
		{
			Receipt receipt = inUserData as Receipt;
			ReceiptRedemptionResult receiptRedemptionResult = inObject as ReceiptRedemptionResult;
			mSyncPendingIAPCount--;
			if (receipt != null && string.IsNullOrEmpty(receipt.UniqueID))
			{
			}
			break;
		}
		}
		if ((inEvent == WsServiceEvent.ERROR || inEvent == WsServiceEvent.COMPLETE) && mSyncPendingIAPCount <= 0)
		{
			mSyncPendingIAPCount = 0;
			if (this.OnSyncDone != null)
			{
				this.OnSyncDone(success: true);
				this.OnSyncDone = null;
			}
		}
	}

	private void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
		{
			mSyncPendingIAPCount--;
			ReceiptRedemptionResult receiptRedemptionResult2 = inObject as ReceiptRedemptionResult;
			Receipt receipt2 = inUserData as Receipt;
			if (mPurchaseInitiated)
			{
				mCurrentPendingReceipt = receipt2;
				ShowCurrentReceiptValidationPrompt();
			}
			UtDebug.LogError("Sync IAP purchase failed for: " + receipt2.ItemID + " Error status: " + ((receiptRedemptionResult2 != null) ? receiptRedemptionResult2.Status.ToString() : " Null*** "));
			break;
		}
		case WsServiceEvent.COMPLETE:
		{
			mSyncPendingIAPCount--;
			ReceiptRedemptionResult receiptRedemptionResult = inObject as ReceiptRedemptionResult;
			Receipt receipt = inUserData as Receipt;
			if (receiptRedemptionResult == null)
			{
				if (mPurchaseInitiated)
				{
					mCurrentPendingReceipt = receipt;
					ShowCurrentReceiptValidationPrompt();
				}
				UtDebug.LogError("Sync IAP purchase failed response null");
			}
			else if (receiptRedemptionResult.Status == ReceiptRedemptionStatus.Success || receiptRedemptionResult.Status == ReceiptRedemptionStatus.DuplicateReceipt)
			{
				mPendingIAPCount--;
				if (receipt.ItemTypeID == ItemType.Membership)
				{
					ParentData.Reset();
					ParentData.Init();
					SubscriptionInfo.Reset();
					SubscriptionInfo.Init();
					mSyncDone = true;
					mWaitSubscriptionRefresh = true;
				}
				else
				{
					OnSyncSuccess(receiptRedemptionResult);
				}
				mCurrentPendingReceipt = null;
				mPurchaseInitiated = false;
			}
			else
			{
				if (receiptRedemptionResult.Status == ReceiptRedemptionStatus.ReceiptIncomplete && mPurchaseInitiated)
				{
					mCurrentPendingReceipt = receipt;
					ShowCurrentReceiptValidationPrompt(needsValidation: true);
				}
				UtDebug.LogError("Sync IAP purchase failed " + receiptRedemptionResult.Status);
			}
			break;
		}
		}
		if (mPendingSyncInitiated && mSyncPendingIAPCount <= 0)
		{
			mPendingSyncDone = true;
		}
	}

	private void RestorePurchaseServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.ERROR:
			if (mInitiatedRestorePurchases)
			{
				mRestoreFailedNum++;
				mPendingRestorePurchases--;
			}
			break;
		case WsServiceEvent.COMPLETE:
		{
			ReceiptRedemptionResult receiptRedemptionResult = (ReceiptRedemptionResult)inObject;
			if (receiptRedemptionResult != null)
			{
				if (receiptRedemptionResult.Status == ReceiptRedemptionStatus.Success)
				{
					Receipt receipt = (Receipt)inUserData;
					RemovePendingReceipt(receipt);
					mRestoreSuccessNum++;
					if (receipt.ItemTypeID == ItemType.Membership)
					{
						ParentData.Reset();
						ParentData.Init();
						SubscriptionInfo.Reset();
						SubscriptionInfo.Init();
						UserProfile.Reset();
						UserProfile.Init();
					}
				}
				else if (receiptRedemptionResult.Status == ReceiptRedemptionStatus.DuplicateReceipt || receiptRedemptionResult.Status == ReceiptRedemptionStatus.SubscriptionStatusExpired)
				{
					UtDebug.LogError("In Receipt is either duplicate or expired");
				}
			}
			if (mInitiatedRestorePurchases)
			{
				mPendingRestorePurchases--;
			}
			break;
		}
		}
		if ((inEvent == WsServiceEvent.COMPLETE || inEvent == WsServiceEvent.ERROR) && mInitiatedRestorePurchases && mPendingRestorePurchases <= 0)
		{
			UtDebug.LogError("Restore completed with success: " + mRestoreSuccessNum + " and failed: " + mRestoreFailedNum);
			if (mRestoreSuccessNum > 0 && mRestoreFailedNum == 0)
			{
				RestorePurchasesDone(ReceiptStatus.Successful);
			}
			else if (mRestoreSuccessNum > 0 && mRestoreFailedNum > 0)
			{
				RestorePurchasesDone(ReceiptStatus.PartialSuccessful);
			}
			else if (mRestoreSuccessNum == 0 && mRestoreFailedNum > 0)
			{
				RestorePurchasesDone(ReceiptStatus.ServerFailed);
			}
			else
			{
				RestorePurchasesDone(ReceiptStatus.None);
			}
		}
	}

	public void OnSyncFailed(ReceiptRedemptionResult result)
	{
		if (result != null)
		{
			UtDebug.Log("IAPManager OnSyncFailed = " + result.Status);
		}
		else
		{
			result = new ReceiptRedemptionResult();
			result.Status = ReceiptRedemptionStatus.UnknownError;
		}
		Message("OnSyncFailed");
	}

	public void OnSyncSuccess(ReceiptRedemptionResult result)
	{
		UtDebug.Log("IAPManager OnSyncSuccess");
		Message("OnSyncSuccess");
	}

	private void Message(string message, object data = null)
	{
		foreach (GameObject mMessageObject in mMessageObjects)
		{
			if (mMessageObject != null)
			{
				if (data != null)
				{
					mMessageObject.SendMessage(message, data, SendMessageOptions.DontRequireReceiver);
				}
				else
				{
					mMessageObject.SendMessage(message, SendMessageOptions.DontRequireReceiver);
				}
			}
		}
	}

	public void RestorePurchases()
	{
	}

	private void OnRestorePurchaseDoneCallback(bool success)
	{
	}

	private IEnumerator DelayedRestorePurchases()
	{
		yield return null;
		yield return null;
		SyncAllPendingIAPReceipts();
	}

	private void RestorePurchasesDone(ReceiptStatus inStatus)
	{
	}

	public void SyncPendingReceipts(OnSyncComplete callback)
	{
		this.OnSyncDone = callback;
		SyncAllPendingIAPReceipts();
	}

	public bool IsAnyReceiptPending()
	{
		return false;
	}

	public bool UnAssignedReceiptPending()
	{
		return false;
	}

	public void ProcessUnAssignedReceipts(string userId, OnSyncComplete callback)
	{
		if (mUnAssignedReceipts == null || mUnAssignedReceipts.Count == 0)
		{
			callback(success: false);
			return;
		}
		this.OnSyncDone = callback;
		mSyncPendingIAPCount = mUnAssignedReceipts.Count;
		foreach (Receipt mUnAssignedReceipt in mUnAssignedReceipts)
		{
			ReceiptRedemptionRequest receiptRedemptionRequest = new ReceiptRedemptionRequest();
			receiptRedemptionRequest.PaymentReceipt = mUnAssignedReceipt;
			receiptRedemptionRequest.PaymentReceipt.UserID = new Guid(userId);
			WsWebService.RedeemReceipt(receiptRedemptionRequest, UnAssignedPendingServiceEventHandler, mUnAssignedReceipt);
		}
	}

	private void SyncAllPendingIAPReceipts()
	{
	}

	private void PendingReceiptHandler(List<Receipt> inReceipts)
	{
		if (inReceipts != null && inReceipts.Count > 0)
		{
			if (mInitiatedRestorePurchases)
			{
				UtDebug.Log("****Restore receipt receieved for products****: " + inReceipts.Count);
				mPendingRestorePurchases = inReceipts.Count;
				mRestoreSuccessNum = 0;
				mRestoreFailedNum = 0;
			}
			else
			{
				UtDebug.Log("****Pending receipt receieved for products****: " + inReceipts.Count);
				mPendingSyncInitiated = true;
				mPendingIAPCount = inReceipts.Count;
				mSyncPendingIAPCount = inReceipts.Count;
			}
			int num = 1;
			List<Receipt> allPendingReceipts = GetAllPendingReceipts(inReceipts);
			if (mInitiatedRestorePurchases && mUnAssignedReceipts != null)
			{
				allPendingReceipts.AddRange(mUnAssignedReceipts);
			}
			UtDebug.Log("****Pending receipt receieved for products****: " + allPendingReceipts.Count);
			if (allPendingReceipts.Count != 0 || this.OnSyncDone == null)
			{
				foreach (Receipt item in allPendingReceipts)
				{
					ReceiptRedemptionRequest receiptRedemptionRequest = new ReceiptRedemptionRequest();
					receiptRedemptionRequest.PaymentReceipt = item;
					bool flag = false;
					if (IAPConsole._BlockReceiptNum > 0 && IAPConsole._BlockReceiptNum == num)
					{
						flag = true;
						IAPConsole._BlockReceiptNum = 0;
					}
					if (mInitiatedRestorePurchases)
					{
						if (flag)
						{
							RestorePurchaseServiceEventHandler(WsServiceType.REDEEM_RECEIPT, WsServiceEvent.ERROR, 1f, null, item);
						}
						else
						{
							if (mIAPStoreData.GetIAPCategoryType(receiptRedemptionRequest.PaymentReceipt.ItemID) == IAPCategoryType.Item && !receiptRedemptionRequest.PaymentReceipt.UserID.HasValue && UserInfo.pInstance != null && !string.IsNullOrEmpty(UserInfo.pInstance.UserID))
							{
								receiptRedemptionRequest.PaymentReceipt.UserID = new Guid(UserInfo.pInstance.UserID);
							}
							WsWebService.RedeemReceipt(receiptRedemptionRequest, RestorePurchaseServiceEventHandler, item);
						}
					}
					else if (flag)
					{
						ServiceEventHandler(WsServiceType.REDEEM_RECEIPT, WsServiceEvent.ERROR, 1f, null, item);
					}
					else
					{
						WsWebService.RedeemReceipt(receiptRedemptionRequest, ServiceEventHandler, item);
					}
					num++;
				}
				return;
			}
			this.OnSyncDone(success: true);
			this.OnSyncDone = null;
		}
		else if (mInitiatedRestorePurchases)
		{
			UtDebug.Log("****Zero Pending receipts receieved for products****");
			RestorePurchasesDone(ReceiptStatus.None);
		}
		else if (this.OnSyncDone != null)
		{
			this.OnSyncDone(success: false);
			this.OnSyncDone = null;
		}
	}

	private void ShowCurrentReceiptValidationPrompt(bool needsValidation = false)
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _IncompleteCurrentReceiptProcessText.GetLocalizedString(), _IncompleteReceiptProcessTitleText.GetLocalizedString(), base.gameObject, needsValidation ? "ValidateCurrentReceipt" : "SyncCurrentReceipt", "DontValidateCurrentReceipt", "", "", inDestroyOnClick: true);
		kAUIGenericDB.SetPriority(kAUIGenericDB.GetPriority() - 1);
		KAUI.SetExclusive(kAUIGenericDB);
	}

	private void ShowCurrentReceiptExpiredPrompt()
	{
		KAUIGenericDB kAUIGenericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _CurrentReceiptExpiredProcessText.GetLocalizedString(), _ReceiptExpiredProcessTitleText.GetLocalizedString(), base.gameObject, "MakeNewPurchase", "DontValidateCurrentReceipt", "", "", inDestroyOnClick: true);
		kAUIGenericDB.SetPriority(kAUIGenericDB.GetPriority() - 1);
	}

	private void MakeNewPurchase()
	{
		if (mCurrentIAPItem != null)
		{
			PurchaseItem(mCurrentIAPItem);
		}
		else
		{
			DontValidateCurrentReceipt();
		}
	}

	private void ValidateCurrentReceipt()
	{
	}

	private void SyncCurrentReceipt()
	{
	}

	private void DontValidateCurrentReceipt()
	{
		mCurrentIAPItem = null;
		mCurrentPendingReceipt = null;
		mPurchaseInitiated = false;
		Message("OnPurchaseFailed", "");
	}

	public void OnSyncPendingFailed()
	{
		UtDebug.Log("IAPManager OnSyncPendingFailed");
		Message("OnSyncPendingFailed");
		if (this.OnSyncDone != null)
		{
			this.OnSyncDone(success: false);
			this.OnSyncDone = null;
		}
	}

	public void OnSyncPendingSuccess()
	{
		UtDebug.Log("IAPManager OnSyncPendingSuccess");
		Message("OnSyncPendingSuccess");
		if (this.OnSyncDone != null)
		{
			this.OnSyncDone(success: true);
			this.OnSyncDone = null;
		}
	}

	public void AddToMsglist(GameObject go)
	{
		mMessageObjects.Add(go);
	}

	public void RemoveFromMsglist(GameObject go)
	{
		mMessageObjects.Remove(go);
	}

	public static string GetMembershipUpgradeText()
	{
		if (mInstance != null)
		{
			ProductDetails pd = ProductSettings.pInstance.GetProductDetails(SubscriptionInfo.GetProviderProductID());
			if (pd != null)
			{
				MembershipUpgradeText membershipUpgradeText = mInstance._MembershipUpgradeText.Find((MembershipUpgradeText text) => text._Platform == pd._Platform);
				if (membershipUpgradeText != null)
				{
					return membershipUpgradeText._Text.GetLocalizedString();
				}
			}
		}
		return string.Empty;
	}

	private void OnDestroy()
	{
		if (mIAPProvider != null)
		{
			IAPProvider iAPProvider = mIAPProvider;
			iAPProvider.OnProductsReceived = (Action<bool>)Delegate.Remove(iAPProvider.OnProductsReceived, new Action<bool>(OnProductsReceivedCallback));
			IAPProvider iAPProvider2 = mIAPProvider;
			iAPProvider2.OnPurchaseDone = (Action<PurchaseStatus, string, Receipt>)Delegate.Remove(iAPProvider2.OnPurchaseDone, new Action<PurchaseStatus, string, Receipt>(OnPurchaseDoneCallback));
		}
	}

	public void InitPurchase(IAPStoreCategory iapCategory, GameObject inMessageObject = null)
	{
		KAUIIAPStore componentInChildren = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfUiIAPStore")).GetComponentInChildren<KAUIIAPStore>();
		componentInChildren._IAPStoreCategory = iapCategory;
		componentInChildren._MessageObject = inMessageObject;
	}

	public void PurchaseProduct(IAPItemData itemData, IAPStoreCategory iapCategory, GameObject inMessageObject = null)
	{
		if ((UtPlatform.IsMobile() || UtPlatform.IsWSA() || UtPlatform.IsSteam()) && (!pIsAvailable || !UtUtilities.IsConnectedToWWW()))
		{
			KAUIGenericDB kAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			kAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			kAUIGenericDB._OKMessage = "OnIAPStoreClosed";
			kAUIGenericDB._MessageObject = inMessageObject;
			kAUIGenericDB.SetText(_StoreUnavailableText.GetLocalizedString(), interactive: false);
			kAUIGenericDB.SetExclusive();
			kAUIGenericDB.SetDestroyOnClick(isDestroy: true);
		}
		else if (mIAPProvider != null)
		{
			mPurchaseInitiated = true;
			PurchaseItem(itemData);
		}
	}

	private void PurchaseItem(IAPItemData itemData)
	{
		WsTokenMonitor.pReloadSceneAllowed = false;
		mIAPProvider.Purchase(itemData, 1);
		mCurrentIAPItem = null;
	}

	private void AddToPendingReceipts(Receipt receipt)
	{
		if (mCachedReceipts == null)
		{
			mCachedReceipts = new List<Receipt>();
		}
		mCachedReceipts.Add(receipt);
		SavePendingIAPReceipts();
	}

	private void RemovePendingReceipt(Receipt receipt)
	{
		if (mCachedReceipts != null && mCachedReceipts.Count > 0)
		{
			mCachedReceipts.RemoveAll((Receipt r) => r.UniqueID == receipt.UniqueID);
			SavePendingIAPReceipts();
		}
		if (mUnAssignedReceipts != null && mUnAssignedReceipts.Count > 0)
		{
			mUnAssignedReceipts.RemoveAll((Receipt r) => r.UniqueID == receipt.UniqueID);
		}
	}

	private List<Receipt> GetAllPendingReceipts(List<Receipt> obtainedReceipts)
	{
		LoadPendingIAPReceipts();
		List<Receipt> list = new List<Receipt>();
		foreach (Receipt receipt in obtainedReceipts)
		{
			if (mIAPStoreData.GetIAPCategoryType(receipt.ItemID) == IAPCategoryType.Item)
			{
				Receipt receipt2 = null;
				if (mCachedReceipts != null)
				{
					receipt2 = mCachedReceipts.Find((Receipt r) => r.UniqueID == receipt.UniqueID && r.UserID.HasValue);
				}
				if (receipt2 != null)
				{
					list.Add(receipt2);
					continue;
				}
				if (mUnAssignedReceipts == null)
				{
					mUnAssignedReceipts = new List<Receipt>();
				}
				mUnAssignedReceipts.Add(receipt);
			}
			else
			{
				list.Add(receipt);
			}
		}
		UtDebug.Log("IAP Manager UnAssigned receipts count " + ((mUnAssignedReceipts == null) ? "NULL" : mUnAssignedReceipts.Count.ToString()));
		UtDebug.Log("IAP Manager Cached receipts count " + ((mCachedReceipts == null) ? "NULL" : mCachedReceipts.Count.ToString()));
		return list;
	}

	private void SavePendingIAPReceipts()
	{
		string playerPrefKey = GetPlayerPrefKey();
		if (mCachedReceipts != null && !string.IsNullOrEmpty(playerPrefKey))
		{
			if (mCachedReceipts.Count > 0)
			{
				PlayerPrefs.SetString(playerPrefKey, UtUtilities.SerializeToXml(mCachedReceipts));
			}
			else if (PlayerPrefs.HasKey(playerPrefKey))
			{
				PlayerPrefs.DeleteKey(playerPrefKey);
			}
		}
	}

	private string GetPlayerPrefKey()
	{
		if (UiLogin.pParentInfo == null || string.IsNullOrEmpty(UiLogin.pParentInfo.UserID))
		{
			return null;
		}
		return "IAPReceipts" + UiLogin.pParentInfo.UserID;
	}

	public void LoadPendingIAPReceipts()
	{
		string playerPrefKey = GetPlayerPrefKey();
		if (!string.IsNullOrEmpty(playerPrefKey))
		{
			string @string = PlayerPrefs.GetString(playerPrefKey);
			if (!string.IsNullOrEmpty(@string))
			{
				mCachedReceipts = UtUtilities.DeserializeFromXml<List<Receipt>>(@string);
			}
		}
	}

	public static bool IsMembershipRecurring()
	{
		if (SubscriptionInfo.pIsMember && !SubscriptionInfo.pIsTrialMember)
		{
			if (SubscriptionInfo.pInstance.Recurring.HasValue && SubscriptionInfo.pInstance.Recurring.Value)
			{
				return true;
			}
			if (pIsReady && SubscriptionInfo.pInstance.SubscriptionProvider != null && !string.IsNullOrEmpty(SubscriptionInfo.pInstance.SubscriptionProvider.ItemID))
			{
				return pIAPStoreData.GetRecurring(SubscriptionInfo.pInstance.SubscriptionProvider.ItemID);
			}
		}
		return false;
	}

	public static bool IsMembershipExpiring(int days)
	{
		if (!IsMembershipRecurring() && SubscriptionInfo.pIsReady && SubscriptionInfo.GetProviderProductID() == ProductConfig.pProductID && SubscriptionInfo.pInstance.SubscriptionEndDate.HasValue && ServerTime.pIsReady)
		{
			TimeSpan timeSpan = SubscriptionInfo.pInstance.SubscriptionEndDate.Value - ServerTime.pCurrentTime;
			if (timeSpan.TotalDays <= (double)days && timeSpan.TotalDays >= 0.0)
			{
				return true;
			}
		}
		return false;
	}

	public static bool IsMembershipUpgradeable()
	{
		if (SubscriptionInfo.pIsMember && (SubscriptionInfo.pIsTrialMember || (SubscriptionInfo.GetProviderProductID() == ProductConfig.pProductID && (!IsMembershipRecurring() || SubscriptionInfo.GetBillFrequency() < 12))))
		{
			return true;
		}
		return false;
	}
}
