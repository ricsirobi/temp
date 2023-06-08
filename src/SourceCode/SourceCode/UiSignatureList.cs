using System;
using UnityEngine;

public class UiSignatureList : KAUI
{
	public UiSocialCrateInviteFriendDB _MessageBuddy;

	public int _TicketStoreID = 102;

	public int _SocialBoxSignatureItemID = 8773;

	public LocaleString[] _FakeUserNames;

	public LocaleString _BuySignatureTitleText = new LocaleString("Buy Signature");

	public LocaleString _NotEnoughVCashText = new LocaleString("You dont have enough VCash to buy this signature. Do you want to buy VCash?");

	public LocaleString _BuySignatureConfirmationText = new LocaleString("Do you want to purchase this for {x} gems?");

	public LocaleString _TicketPurchaseProcessing = new LocaleString("Processing purchase...");

	public LocaleString _TicketPurchaseSuccessfulText = new LocaleString("Signature purchase successful.");

	public LocaleString _TicketPurchaseFailed = new LocaleString("Transaction failed. Please try again.");

	private GameObject mMessageObject;

	private KAWidget mBtnClose;

	private UserActivityInstance mUserActivity;

	private string mCurrentUserID;

	private int mSignatureTicketCost;

	private KAUIGenericDB mKAUIGenericDB;

	private bool[] mSelectedIDs;

	private string mFakeUserID = "00000000-0000-0000-0000-000000000000";

	private int mWait;

	protected override void Start()
	{
		base.Start();
		mBtnClose = FindItem("CloseBtn");
		mCurrentUserID = UserInfo.pInstance.UserID;
		mUserActivity = SocialBoxManager.pInstance.pUserActivity;
		KAUICursorManager.SetDefaultCursor("Loading");
		mWait++;
		ItemStoreDataLoader.Load(_TicketStoreID, OnStoreLoaded);
		mSelectedIDs = new bool[_FakeUserNames.Length];
		for (int i = 0; i < _FakeUserNames.Length; i++)
		{
			mSelectedIDs[i] = false;
		}
		SetSignatureStatus();
		int quantity = CommonInventoryData.pInstance.GetQuantity(_SocialBoxSignatureItemID);
		int numberOfSignaturesUnlocked = GetNumberOfSignaturesUnlocked();
		if (quantity > numberOfSignaturesUnlocked)
		{
			for (int j = 0; j < quantity - numberOfSignaturesUnlocked; j++)
			{
				AddSignature();
			}
		}
	}

	private int GetNumberOfSignaturesUnlocked()
	{
		int num = 0;
		if (mUserActivity.pList != null && mUserActivity.pList.Count >= 0)
		{
			foreach (UserActivity p in mUserActivity.pList)
			{
				if (p.UserActivityTypeID == 3 && p.RelatedUserID != new Guid(mCurrentUserID))
				{
					num++;
				}
			}
		}
		return num;
	}

	private void SetSignatureStatus()
	{
		if (mUserActivity.pList == null || mUserActivity.pList.Count < 0 || !BuddyList.pIsReady)
		{
			return;
		}
		int num = 0;
		foreach (UserActivity p in mUserActivity.pList)
		{
			if (p.UserActivityTypeID == 3 && p.RelatedUserID != new Guid(mCurrentUserID))
			{
				Buddy buddy = BuddyList.pInstance.GetBuddy(p.RelatedUserID.ToString());
				string text = "";
				if (buddy != null)
				{
					text = buddy.DisplayName;
				}
				else
				{
					string text2 = p.RelatedUserID.ToString();
					int.TryParse(text2.Substring(text2.Length - 2), out var result);
					text = _FakeUserNames[result].GetLocalizedString();
					mSelectedIDs[result] = true;
				}
				num++;
				if (num > 0 && num <= SocialBoxManager.pInstance._NumOfLocksToOpenBox)
				{
					SetSignatureData(num, text);
				}
			}
		}
	}

	private void SetSignatureData(int index, string name)
	{
		KAWidget kAWidget = FindItem("Player" + index);
		((KAToggleButton)kAWidget.FindChildItem("BtnCheckP" + index)).SetChecked(isChecked: true);
		kAWidget.FindChildItem("BtnMessageFriendP" + index).SetVisibility(inVisible: false);
		kAWidget.FindChildItem("BtnPayGemsP" + index).SetVisibility(inVisible: false);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtNameP" + index);
		kAWidget2.SetText(name);
		kAWidget2.SetVisibility(inVisible: true);
	}

	private void SetGemCost()
	{
		for (int i = 0; i < SocialBoxManager.pInstance._NumOfLocksToOpenBox; i++)
		{
			FindItem("Player" + (i + 1)).FindChildItem("BtnPayGemsP" + (i + 1)).SetText(mSignatureTicketCost.ToString());
		}
	}

	private void OnStoreLoaded(StoreData sd)
	{
		ItemData itemData = sd.FindItem(_SocialBoxSignatureItemID);
		if (itemData != null)
		{
			mSignatureTicketCost = itemData.FinalCashCost;
		}
		SetGemCost();
		mWait--;
		if (mWait <= 0)
		{
			if (!GetVisibility())
			{
				SetVisibility(inVisible: true);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name.StartsWith("BtnMessageFriendP"))
		{
			if (_MessageBuddy != null)
			{
				SetVisibility(inVisible: false);
				KAUI.RemoveExclusive(this);
				_MessageBuddy.SetVisibility(inVisible: true);
				KAUI.SetExclusive(_MessageBuddy);
			}
		}
		else if (inWidget.name.StartsWith("BtnPayGemsP"))
		{
			ProcessBuySignature();
		}
		else if (inWidget == mBtnClose)
		{
			SetVisibility(inVisible: false);
			KAUI.RemoveExclusive(this);
			mMessageObject.SendMessage("OnExit");
		}
	}

	private void ProcessBuySignature()
	{
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Buy Signature");
		mKAUIGenericDB.SetTitle(_BuySignatureTitleText.GetLocalizedString());
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
		string localizedString = _BuySignatureConfirmationText.GetLocalizedString();
		localizedString = localizedString.Replace("{x}", mSignatureTicketCost.ToString());
		mKAUIGenericDB.SetText(localizedString, interactive: false);
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._YesMessage = "PurchaseSignature";
		mKAUIGenericDB._NoMessage = "KillGenericDB";
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void PurchaseSignature()
	{
		if (Money.pCashCurrency < mSignatureTicketCost)
		{
			KillGenericDB();
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Not Enough Gems");
			mKAUIGenericDB.SetTitle(_BuySignatureTitleText.GetLocalizedString());
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(_NotEnoughVCashText.GetLocalizedString(), interactive: false);
			mKAUIGenericDB._MessageObject = base.gameObject;
			mKAUIGenericDB._YesMessage = "ProceedToStore";
			mKAUIGenericDB._NoMessage = "KillGenericDB";
			KAUI.SetExclusive(mKAUIGenericDB);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			CommonInventoryData.pInstance.AddPurchaseItem(_SocialBoxSignatureItemID, 1, ItemPurchaseSource.SOCIAL_BOX.ToString());
			CommonInventoryData.pInstance.DoPurchase(2, _TicketStoreID, TicketPurchaseHandler);
			KillGenericDB();
			mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
			mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: false, inCloseBtn: false);
			mKAUIGenericDB.SetText(_TicketPurchaseProcessing.GetLocalizedString(), interactive: false);
			KAUI.SetExclusive(mKAUIGenericDB);
		}
	}

	public void TicketPurchaseHandler(CommonInventoryResponse ret)
	{
		KillGenericDB();
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "Message");
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		if (ret != null && ret.Success)
		{
			AddSignature();
			mKAUIGenericDB.SetText(_TicketPurchaseSuccessfulText.GetLocalizedString(), interactive: false);
		}
		else
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			mKAUIGenericDB.SetText(_TicketPurchaseFailed.GetLocalizedString(), interactive: false);
		}
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "KillGenericDB";
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void AddSignature()
	{
		UserActivity userActivity = new UserActivity();
		userActivity.UserID = new Guid(mCurrentUserID);
		int num;
		while (mSelectedIDs[num = UnityEngine.Random.Range(0, _FakeUserNames.Length)])
		{
		}
		string text = num.ToString();
		text = mFakeUserID.Substring(0, mFakeUserID.Length - text.Length) + text;
		userActivity.RelatedUserID = new Guid(text);
		userActivity.UserActivityTypeID = 3;
		mUserActivity.Save(userActivity, UserActivityHandler);
		mSelectedIDs[num] = true;
		mWait++;
	}

	private void UserActivityHandler(bool success, UserActivity inData)
	{
		if (success)
		{
			SetSignatureStatus();
		}
		mWait--;
		if (mWait <= 0)
		{
			if (!GetVisibility())
			{
				SetVisibility(inVisible: true);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
	}

	private void ProceedToStore()
	{
		KillGenericDB();
		SetInteractive(interactive: false);
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	public void OnIAPStoreClosed()
	{
		SetInteractive(interactive: true);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	public void SetMessageObject(GameObject msg)
	{
		mMessageObject = msg;
	}
}
