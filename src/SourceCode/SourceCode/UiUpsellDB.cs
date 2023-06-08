using System;
using System.Collections.Generic;
using UnityEngine;

public class UiUpsellDB : KAUI
{
	[Serializable]
	public class VideoData
	{
		public string _Name;

		public string _Path;
	}

	private enum PurchaseType
	{
		None,
		Item,
		Membership,
		Gems
	}

	public delegate void UpsellCompleteCallback(bool purchased);

	public delegate void UpsellLoadDoneCallback(RsResourceLoadEvent inEvent, UiUpsellDB upsellDB);

	private const int AsciiValOfChara = 97;

	private const int AsciiValOfCharz = 122;

	private const int AsciiValOfChar0 = 48;

	private const int AsciiValOfChar9 = 57;

	private const int AlphabetsCount = 26;

	private const int NumbersCount = 10;

	private string mSourceName;

	private int mTicketID;

	public VideoData[] _VideoData;

	public string _ExpansionName;

	public int _StoreID = 91;

	public int _ItemTicketID = 8631;

	public int _MissionID = 1003;

	public int _PairDataID = 2014;

	public LocaleString _NotEnoughGemsText = new LocaleString("You dont have enough cash to play. You can buy gems from the store.");

	public LocaleString _NotEnoughCoinsText = new LocaleString("You dont have enough cash to play. You can buy coins from the store.");

	public LocaleString _UnsuccessfulPurchaseText = new LocaleString("Purchase failed. Please try again later.");

	public LocaleString _StoreNotReadyText = new LocaleString("Store initialisation failed. Please try again later.");

	public LocaleString _PurchaseConfirmationText = new LocaleString("This will cost you {cost} Gems. Do you wish to continue?");

	public LocaleString _PurchaseConfirmationCoinText = new LocaleString("This will cost you {cost} Coins. Do you wish to continue?");

	public LocaleString _PurchaseDBTitle = new LocaleString("Expansion Unlocked!!");

	private StoreData mStoreData;

	private KAUIGenericDB mGenericDBUi;

	private int mTimesShown;

	private PairData mPairData;

	private PurchaseType mPurchaseType;

	private bool mStatus;

	public UpsellCompleteCallback OnUpsellComplete;

	public static UpsellLoadDoneCallback OnUpsellLoadDoneCallback;

	private KAUIStore mActiveStore;

	public KAUIStore pActiveStore
	{
		get
		{
			return mActiveStore;
		}
		set
		{
			mActiveStore = value;
		}
	}

	public void SetSource(string purchaseSource)
	{
		mSourceName = purchaseSource;
	}

	public static void Init(string inURL, UpsellLoadDoneCallback inLoadDoneCallback, UpsellCompleteCallback inCompleteCallback)
	{
		OnUpsellLoadDoneCallback = inLoadDoneCallback;
		AvAvatar.pState = AvAvatarState.PAUSED;
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		string[] array = inURL.Split('/');
		RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnUpsellScreenLoaded, typeof(GameObject), inDontDestroy: false, inCompleteCallback);
	}

	private static void OnUpsellScreenLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)inObject);
			gameObject.name = gameObject.name.Replace("(Clone)", "");
			UiUpsellDB upsellDB = gameObject.GetComponent<UiUpsellDB>();
			upsellDB.OnUpsellComplete = delegate(bool p)
			{
				upsellDB.OnUpsellCompleteHandler(p, (UpsellCompleteCallback)inUserData);
			};
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (OnUpsellLoadDoneCallback != null)
			{
				OnUpsellLoadDoneCallback(inEvent, upsellDB);
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.SetUIActive(inActive: true);
			AvAvatar.pState = AvAvatarState.IDLE;
			KAUICursorManager.SetDefaultCursor("Arrow");
			if (OnUpsellLoadDoneCallback != null)
			{
				OnUpsellLoadDoneCallback(inEvent, null);
			}
			break;
		}
	}

	private void OnUpsellCompleteHandler(bool purchased, UpsellCompleteCallback inCompleteCallback)
	{
		AvAvatar.SetUIActive(inActive: true);
		AvAvatar.pState = AvAvatarState.IDLE;
		inCompleteCallback?.Invoke(purchased);
	}

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		KAUICursorManager.SetDefaultCursor("Loading");
		SetInteractive(interactive: false);
		PairData.Load(_PairDataID, PairDataLoadCallback, null);
		mStatus = false;
	}

	private void PairDataLoadCallback(bool success, PairData pData, object inUserData)
	{
		ItemStoreDataLoader.Load(_StoreID, OnStoreLoaded);
		if (pData != null)
		{
			mPairData = pData;
			if (mPairData.FindByKey(_ItemTicketID + "ShownCount") != null)
			{
				int.TryParse(mPairData.FindByKey(_ItemTicketID + "ShownCount")?.PairValue, out mTimesShown);
				mTimesShown++;
			}
			else
			{
				mPairData.SetValueAndSave(_ItemTicketID + "ShownCount", 0.ToString());
			}
		}
	}

	private void OnStoreLoaded(StoreData sd)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		SetInteractive(interactive: true);
		if (sd != null)
		{
			mStoreData = sd;
			mTicketID = GetTicketID(_ItemTicketID);
		}
	}

	private void ShowDB(string inText, string inTitle, bool yes, bool no, bool ok, bool close = false)
	{
		if (mGenericDBUi != null)
		{
			UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
		}
		GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
		mGenericDBUi = gameObject.GetComponent<KAUIGenericDB>();
		mGenericDBUi._MessageObject = base.gameObject;
		mGenericDBUi._YesMessage = "PurchaseItem";
		mGenericDBUi._OKMessage = "OnCloseUpSellDB";
		mGenericDBUi.SetText(inText, interactive: false);
		mGenericDBUi.SetTitle(inTitle);
		mGenericDBUi.SetButtonVisibility(yes, no, ok, close);
		mGenericDBUi.SetDestroyOnClick(isDestroy: true);
		KAUI.SetExclusive(mGenericDBUi);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		mPurchaseType = PurchaseType.None;
		switch (inWidget.name)
		{
		case "BtnPurchaseItem":
			ShowPurchaseConfirmationDB();
			break;
		case "BtnPurchaseMembership":
			ShowPurchaseMembership();
			break;
		case "BtnViewItemVideo":
			PlayUpsellMovie();
			SetVisibility(inVisible: false);
			break;
		case "BtnClose":
			OnCloseUpSellDB();
			break;
		}
	}

	private void OnCloseUpSellDB()
	{
		Dictionary<string, object> inParameter = new Dictionary<string, object>
		{
			{
				"name",
				string.IsNullOrEmpty(_ExpansionName) ? base.gameObject.name : _ExpansionName
			},
			{
				"purchaseType",
				mPurchaseType.ToString()
			},
			{ "count", mTimesShown },
			{
				"sodUserID",
				UserInfo.pInstance.UserID
			}
		};
		mPairData.SetValueAndSave(_ItemTicketID + "ShownCount", (mTimesShown + 1).ToString());
		AnalyticAgent.LogEvent(AnalyticEvent.UPSELL_SHOWN, inParameter);
		KAUI.RemoveExclusive(this);
		if (OnUpsellComplete != null)
		{
			OnUpsellComplete(mStatus);
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}

	private void ShowPurchaseConfirmationDB()
	{
		if (mGenericDBUi != null)
		{
			UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
		}
		if (mStoreData != null)
		{
			ItemData itemData = mStoreData.FindItem(mTicketID);
			GameObject gameObject = UnityEngine.Object.Instantiate((GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB"));
			mGenericDBUi = gameObject.GetComponent<KAUIGenericDB>();
			mGenericDBUi._MessageObject = base.gameObject;
			mGenericDBUi._YesMessage = "PurchaseItem";
			mGenericDBUi._NoMessage = "DestroyGenericDb";
			KAUI.SetExclusive(mGenericDBUi);
			if (itemData.GetPurchaseType() == 2)
			{
				mGenericDBUi.SetText(_PurchaseConfirmationText.GetLocalizedString().Replace("{cost}", itemData.FinalCashCost.ToString()), interactive: false);
			}
			else
			{
				mGenericDBUi.SetText(_PurchaseConfirmationCoinText.GetLocalizedString().Replace("{cost}", itemData.FinalCost.ToString()), interactive: false);
			}
			mGenericDBUi.SetButtonVisibility(inYesBtn: true, inNoBtn: true, inOKBtn: false, inCloseBtn: false);
			mGenericDBUi.SetDestroyOnClick(isDestroy: true);
		}
	}

	private void DestroyGenericDb()
	{
		if (mGenericDBUi != null)
		{
			KAUI.RemoveExclusive(mGenericDBUi);
			UnityEngine.Object.Destroy(mGenericDBUi.gameObject);
		}
	}

	private void PurchaseItem()
	{
		SetVisibility(inVisible: false);
		DestroyGenericDb();
		if (mStoreData == null)
		{
			return;
		}
		ItemData itemData = mStoreData.FindItem(mTicketID);
		if (itemData != null)
		{
			if (itemData.GetPurchaseType() == 2)
			{
				if (Money.pCashCurrency >= itemData.FinalCashCost)
				{
					PurchaseTicket(2);
					return;
				}
				ShowDB(_NotEnoughGemsText.GetLocalizedString(), "", yes: false, no: false, ok: true);
				mGenericDBUi._OKMessage = "BuyGemsOnline";
			}
			else if (Money.pGameCurrency >= itemData.FinalCost)
			{
				PurchaseTicket(1);
			}
			else
			{
				ShowDB(_NotEnoughCoinsText.GetLocalizedString(), "", yes: false, no: false, ok: true);
			}
		}
		else
		{
			ShowDB(_StoreNotReadyText.GetLocalizedString(), "", yes: false, no: false, ok: true);
		}
	}

	private int GetTicketID(int itemTicketID)
	{
		ItemData itemData = mStoreData.FindItem(itemTicketID);
		if (itemData == null)
		{
			return -1;
		}
		if (itemData.Relationship == null)
		{
			return itemTicketID;
		}
		ItemDataRelationship[] relationship = itemData.Relationship;
		foreach (ItemDataRelationship itemDataRelationship in relationship)
		{
			if (itemDataRelationship.Type == "Override")
			{
				int ticketID = GetTicketID(itemDataRelationship.ItemId);
				if (ticketID != -1)
				{
					return ticketID;
				}
			}
			else if (itemDataRelationship.Type == "Prereq")
			{
				if (CommonInventoryData.pInstance.FindItem(itemDataRelationship.ItemId) != null)
				{
					return itemTicketID;
				}
				return -1;
			}
		}
		return itemTicketID;
	}

	protected void PurchaseTicket(int inCurrencyType)
	{
		mPurchaseType = PurchaseType.Item;
		KAUICursorManager.SetDefaultCursor("Loading");
		ItemData itemData = mStoreData.FindItem(mTicketID);
		if (inCurrencyType == 2 && itemData != null && itemData.HasAttribute("Parent"))
		{
			ParentData.pInstance.pInventory.AddPurchaseItem(mTicketID, 1, mSourceName);
			ParentData.pInstance.pInventory.DoPurchase(inCurrencyType, _StoreID, OnTicketPurchaseDone);
		}
		else
		{
			CommonInventoryData.pInstance.AddPurchaseItem(mTicketID, 1, mSourceName);
			CommonInventoryData.pInstance.DoPurchase(inCurrencyType, _StoreID, OnTicketPurchaseDone);
		}
	}

	public void OnTicketPurchaseDone(CommonInventoryResponse response)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (response != null && response.Success)
		{
			mStatus = true;
			ShowPurchaseConfirmation();
		}
		else
		{
			ShowDB(_UnsuccessfulPurchaseText.GetLocalizedString(), "", yes: false, no: false, ok: true);
		}
	}

	private void ShowPurchaseConfirmation()
	{
		ExpansionUnlock.UpsellDataMissionMap upsellInfo = ExpansionUnlock.pInstance.GetUpsellInfo(_MissionID);
		if (upsellInfo != null)
		{
			string upsellConfirmationMsg = upsellInfo.GetUpsellConfirmationMsg();
			if (!string.IsNullOrEmpty(upsellConfirmationMsg))
			{
				SetVisibility(inVisible: false);
				ShowDB(upsellConfirmationMsg, _PurchaseDBTitle.GetLocalizedString(), yes: false, no: false, ok: true);
			}
			else
			{
				OnCloseUpSellDB();
			}
		}
		else
		{
			OnCloseUpSellDB();
		}
	}

	protected void BuyGemsOnline()
	{
		mPurchaseType = PurchaseType.Gems;
		IAPManager.pInstance.InitPurchase(IAPStoreCategory.GEMS, base.gameObject);
	}

	public void OnIAPStoreClosed()
	{
		switch (mPurchaseType)
		{
		case PurchaseType.Gems:
			SetVisibility(inVisible: true);
			break;
		case PurchaseType.Membership:
			OnPurchaseMembership();
			break;
		case PurchaseType.Item:
			break;
		}
	}

	private void ShowPurchaseMembership()
	{
		if ((bool)mActiveStore)
		{
			mActiveStore.LoadStore("Membership", "Membership");
			mActiveStore.EnableStoreMenu(inEnable: false);
			OnCloseUpSellDB();
		}
		else
		{
			SetVisibility(inVisible: false);
			DestroyGenericDb();
			mPurchaseType = PurchaseType.Membership;
			IAPManager.pInstance.InitPurchase(IAPStoreCategory.MEMBERSHIP, base.gameObject);
		}
	}

	private void OnPurchaseMembership()
	{
		if (ExpansionUnlock.pInstance.IsUnlocked(mTicketID))
		{
			mStatus = true;
			ShowPurchaseConfirmation();
		}
		else
		{
			SetVisibility(inVisible: true);
		}
	}

	private void PlayUpsellMovie()
	{
		VideoData videoData = GetVideoData();
		if (videoData != null)
		{
			MovieManager.SetBackgroundColor(Color.black);
			MovieManager.Play(videoData._Path, OnMovieStarted, OnMoviePlayed, skipMovie: true);
		}
	}

	private VideoData GetVideoData()
	{
		if (_VideoData.Length == 0)
		{
			UtDebug.LogError("No videos found!!!!");
			return null;
		}
		string userID = UserInfo.pInstance.UserID;
		int videoIndex = GetVideoIndex(userID[userID.Length - 1], _VideoData.Length);
		return _VideoData[videoIndex];
	}

	private int GetVideoIndex(char inCh, int inNumDivisions)
	{
		int num = -1;
		int num2 = char.ToLower(inCh);
		int num3 = 0;
		if (num2 >= 97 && num2 <= 122)
		{
			num = 97;
			num3 = 26;
		}
		else if (num2 >= 48 && num2 <= 57)
		{
			num = 48;
			num3 = 10;
		}
		int num4 = num2 - num;
		int result = 0;
		if (inNumDivisions < num3)
		{
			int num5 = (int)Mathf.Ceil((float)num3 / (float)inNumDivisions);
			result = num4 / num5;
		}
		return result;
	}

	private void OnMovieStarted()
	{
	}

	private void OnMoviePlayed()
	{
		SetVisibility(inVisible: true);
	}
}
