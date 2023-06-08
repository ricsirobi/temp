using System;
using System.Collections.Generic;
using UnityEngine;

public class UiDailyBonus : KAUI, IAdResult
{
	public class ImageLoader : KAWidgetUserData
	{
		public delegate void ImageLoaderCallback(bool inLoaded);

		private string mImageRes = string.Empty;

		private KAWidget mLoadingGear;

		private bool mSetImage;

		private Texture mTexture;

		private int mStoreID;

		private int mItemID;

		private ItemData mItemData;

		private ImageLoaderCallback mEvent;

		public ItemData pItemData => mItemData;

		public ImageLoader(string inImageRes)
		{
			mStoreID = 0;
			mItemID = 0;
			mImageRes = inImageRes;
		}

		public ImageLoader(int inStoreID, int inItemID, string inImageRes)
		{
			mStoreID = inStoreID;
			mItemID = inItemID;
			mImageRes = inImageRes;
		}

		public void ShowLoadingGear(KAWidget inLoadingGear)
		{
			if (inLoadingGear == null)
			{
				return;
			}
			mLoadingGear = _Item.pUI.DuplicateWidget(inLoadingGear);
			if (!(mLoadingGear == null) && _Item != null)
			{
				if (_Item.pUI != null)
				{
					_Item.pUI.AddWidget(mLoadingGear);
				}
				if (_Item.pParentWidget != null)
				{
					_Item.pParentWidget.AddChild(mLoadingGear);
					mLoadingGear.transform.localPosition = Vector3.zero;
				}
				mLoadingGear.SetPosition(_Item.GetPosition().x, _Item.GetPosition().y);
				mLoadingGear.SetVisibility(inVisible: true);
			}
		}

		public void LoadImage(KAWidget inLoadingGear, ImageLoaderCallback inCallback)
		{
			mEvent = inCallback;
			if (_Item != null)
			{
				_Item.SetVisibility(inVisible: false);
			}
			ShowLoadingGear(inLoadingGear);
			if (mStoreID > 0)
			{
				ItemStoreDataLoader.Load(mStoreID, OnStoreDataLoaded);
			}
			if (string.IsNullOrEmpty(mImageRes))
			{
				if (mItemID <= 0 || mStoreID <= 0)
				{
					OnImageLoaded(string.Empty, RsResourceLoadEvent.ERROR, 0f, null, null);
				}
			}
			else
			{
				LoadImage(mImageRes);
			}
		}

		private void LoadImage(string inURL)
		{
			if (string.IsNullOrEmpty(inURL))
			{
				OnImageLoaded(string.Empty, RsResourceLoadEvent.ERROR, 0f, null, null);
				return;
			}
			if (inURL.StartsWith("http://"))
			{
				RsResourceManager.Load(inURL, OnImageLoaded);
				return;
			}
			string[] array = inURL.Split('/');
			if (array.Length >= 3)
			{
				RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnImageLoaded, typeof(Texture));
			}
		}

		public void SetImage()
		{
			if (_Item == null || mTexture == null)
			{
				return;
			}
			_Item.SetTexture(mTexture);
			if (mLoadingGear != null && _Item.pUI != null)
			{
				if (_Item.pParentWidget != null)
				{
					_Item.pParentWidget.RemoveChildItem(mLoadingGear);
				}
				_Item.pUI.RemoveWidget(mLoadingGear);
			}
			_Item.SetVisibility(inVisible: true);
		}

		public void LoadAndSetImage(KAWidget inLoadingGear)
		{
			mSetImage = true;
			LoadImage(inLoadingGear, null);
		}

		private void OnImageLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
		{
			switch (inEvent)
			{
			case RsResourceLoadEvent.COMPLETE:
				if (_Item != null)
				{
					mTexture = inFile as Texture;
					if (mEvent != null)
					{
						mEvent(inLoaded: true);
					}
					if (mSetImage)
					{
						SetImage();
					}
				}
				break;
			case RsResourceLoadEvent.ERROR:
				if (mLoadingGear != null && _Item.pUI != null)
				{
					if (_Item.pParentWidget != null)
					{
						_Item.pParentWidget.RemoveChildItem(mLoadingGear);
					}
					_Item.pUI.RemoveWidget(mLoadingGear);
				}
				mEvent(inLoaded: false);
				break;
			}
		}

		private void OnStoreDataLoaded(StoreData inStoreData)
		{
			if (inStoreData != null)
			{
				mItemData = inStoreData.FindItem(mItemID);
				if (_Item != null && _Item.pUI != null)
				{
					UiDailyBonus component = _Item.pUI.GetComponent<UiDailyBonus>();
					if (component != null && component.UpdateLimitedItem(mItemData, _Item))
					{
						component.AddLimitedItem(this);
					}
				}
			}
			if (string.IsNullOrEmpty(mImageRes))
			{
				bool flag = false;
				if (inStoreData != null && mItemData != null && !string.IsNullOrEmpty(mItemData.IconName))
				{
					flag = true;
					LoadImage(mItemData.IconName);
				}
				if (!flag)
				{
					OnImageLoaded(string.Empty, RsResourceLoadEvent.ERROR, 0f, null, null);
				}
			}
		}
	}

	public class PromoFrameImageLoader
	{
		private RsResourceLoadEvent mIconResLoadEvent;

		private RsResourceLoadEvent mBkgResLoadEvemt;

		private ImageLoader mBkgRes;

		private ImageLoader mIconRes;

		private KAWidget mLoadingGear;

		public PromoFrameImageLoader(ImageLoader inBkgRes, ImageLoader inIconRes)
		{
			mBkgRes = inBkgRes;
			mIconRes = inIconRes;
		}

		public void LoadAndSetImages(KAWidget inLoadingGear)
		{
			mLoadingGear = inLoadingGear;
			mBkgResLoadEvemt = RsResourceLoadEvent.PROGRESS;
			if (mBkgRes != null)
			{
				mBkgRes.LoadImage(mLoadingGear, BkgImageEventHandler);
			}
			else
			{
				BkgImageEventHandler(inSuccess: false);
			}
			mIconResLoadEvent = RsResourceLoadEvent.PROGRESS;
			if (mIconRes != null)
			{
				mIconRes.LoadImage(null, IconImageEventHandler);
			}
			else
			{
				IconImageEventHandler(inSuccess: false);
			}
		}

		private void BkgImageEventHandler(bool inSuccess)
		{
			mBkgResLoadEvemt = (inSuccess ? RsResourceLoadEvent.COMPLETE : RsResourceLoadEvent.ERROR);
			if (mBkgResLoadEvemt != RsResourceLoadEvent.COMPLETE)
			{
				return;
			}
			if (mBkgRes != null)
			{
				mBkgRes.SetImage();
			}
			if (mIconRes != null)
			{
				mIconRes.ShowLoadingGear(mLoadingGear);
				if (mIconResLoadEvent == RsResourceLoadEvent.COMPLETE)
				{
					mIconRes.SetImage();
				}
			}
		}

		private void IconImageEventHandler(bool inSuccess)
		{
			mIconResLoadEvent = (inSuccess ? RsResourceLoadEvent.COMPLETE : RsResourceLoadEvent.ERROR);
			if (mIconResLoadEvent == RsResourceLoadEvent.COMPLETE && mBkgResLoadEvemt == RsResourceLoadEvent.COMPLETE && mIconRes != null)
			{
				mIconRes.SetImage();
			}
		}
	}

	public GameObject _MessageObject;

	public LocaleString _UnexpectedPromoErrorText = new LocaleString("You cannot use this for now.");

	public LocaleString _TodayText = new LocaleString("TODAY");

	public LocaleString _TomorrowText = new LocaleString("TOMORROW");

	public LocaleString _DailyBonusText = new LocaleString("Congratulations!  Collect the Day %day% bonus.");

	public LocaleString _DailyBonusClaimText = new LocaleString("Don't forget to come back tomorrow and claim your bonus!");

	public LocaleString _AvailableTilText = new LocaleString("Til XXXX");

	public LocaleString _TimeLeftMinText = new LocaleString("< XXXX min");

	public LocaleString _TimeLeftMinsText = new LocaleString("< XXXX mins");

	public LocaleString _TimeLeftHoursText = new LocaleString("< XXXX hrs");

	public LocaleString _SaleText = new LocaleString("Sale!");

	public LocaleString _FreeText = new LocaleString("Free!");

	public LocaleString _AdRewardSuccessText = new LocaleString("You earned %gems% gems!");

	public LocaleString _AdRewardFailedText = new LocaleString("Reward failed to load. Please try again later.");

	public Texture _CoinsTexture;

	public Texture _GemsTexture;

	public KAWidget _Gear;

	public Color _OfferPriceColor = Color.green;

	private bool mInitialized;

	private KAWidget mTodayBonusText;

	private KAWidget mBonusClaimText;

	private KAUIGenericDB mMsgBox;

	private List<ImageLoader> mLimitedItems;

	private bool mReInitInventory;

	private AdEventType mAdEventType;

	protected override void Start()
	{
		base.Start();
		CollectBonus();
		mTodayBonusText = FindItem("TxtTodaysBonus");
		if (mTodayBonusText != null)
		{
			mTodayBonusText.SetText(string.Empty);
		}
		mBonusClaimText = FindItem("TxtBonusClaim");
		if (mBonusClaimText != null)
		{
			mBonusClaimText.SetText(string.Empty);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (!mInitialized)
		{
			if (DailyBonusAndPromo.pIsReady)
			{
				mInitialized = true;
				Initialize();
			}
			else
			{
				DailyBonusAndPromo.Init();
			}
		}
		if (mReInitInventory && CommonInventoryData.pIsReady)
		{
			mReInitInventory = false;
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
		}
		HandleLimitedItems();
	}

	private void Initialize()
	{
		if (DailyBonusAndPromo.pIsReady && DailyBonusAndPromo.pInstance != null)
		{
			InitializeDailyBonus();
			InitializePromo();
		}
	}

	private void InitializeDailyBonus()
	{
		if (!DailyBonusAndPromo.pIsReady || DailyBonusAndPromo.pInstance == null)
		{
			return;
		}
		for (int i = 1; i <= DailyBonusAndPromo.pInstance.MaxDayCount; i++)
		{
			KAWidget kAWidget = FindItem("Day" + i);
			if (kAWidget == null)
			{
				continue;
			}
			DailyBonus bonusInfo = DailyBonusAndPromo.pInstance.GetBonusInfo(i);
			if (bonusInfo == null)
			{
				kAWidget.SetVisibility(inVisible: false);
				continue;
			}
			if (i < DailyBonusAndPromo.pInstance.RewardSequenceDay)
			{
				kAWidget.SetDisabled(isDisabled: true);
			}
			Transform transform = kAWidget.transform.Find("TxtStateDay");
			if (transform != null)
			{
				UILabel component = transform.GetComponent<UILabel>();
				if (component != null)
				{
					Transform transform2 = kAWidget.transform.Find("Selected");
					Transform transform3 = kAWidget.transform.Find("Check");
					if (i == DailyBonusAndPromo.pInstance.RewardSequenceDay)
					{
						component.text = _TodayText.GetLocalizedString();
						if (transform2 != null)
						{
							transform2.gameObject.SetActive(value: true);
						}
						if (transform3 != null)
						{
							transform3.gameObject.SetActive(value: true);
						}
					}
					else
					{
						if (transform2 != null)
						{
							transform2.gameObject.SetActive(value: false);
						}
						if (i == DailyBonusAndPromo.pInstance.RewardSequenceDay + 1)
						{
							component.text = _TomorrowText.GetLocalizedString();
						}
						else
						{
							component.text = string.Empty;
						}
						if (i < DailyBonusAndPromo.pInstance.RewardSequenceDay && transform3 != null)
						{
							transform3.gameObject.SetActive(value: true);
						}
					}
				}
			}
			Transform transform4 = kAWidget.transform.Find("TxtCoins");
			if (transform4 != null)
			{
				UILabel component2 = transform4.GetComponent<UILabel>();
				if (component2 != null)
				{
					component2.text = bonusInfo.DisplayText.GetLocalizedString();
				}
			}
			KAWidget kAWidget2 = kAWidget.FindChildItem("BkgIcon");
			if (!(kAWidget2 != null))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(bonusInfo.IconRes))
			{
				ImageLoader imageLoader = new ImageLoader(bonusInfo.IconRes);
				kAWidget2.SetUserData(imageLoader);
				imageLoader.LoadAndSetImage(_Gear);
				continue;
			}
			string type = bonusInfo.Type;
			if (!(type == "Coin"))
			{
				if (type == "Gems")
				{
					kAWidget2.SetTexture(_GemsTexture);
				}
				else
				{
					kAWidget2.SetTexture(null);
				}
			}
			else
			{
				kAWidget2.SetTexture(_CoinsTexture);
			}
		}
		if (mTodayBonusText != null && DailyBonusAndPromo.pInstance.RewardSequenceDay != 0)
		{
			mTodayBonusText.SetText(_DailyBonusText.GetLocalizedString().Replace("%day%", DailyBonusAndPromo.pInstance.RewardSequenceDay.ToString()));
		}
		if (mBonusClaimText != null)
		{
			mBonusClaimText.SetText(_DailyBonusClaimText.GetLocalizedString());
		}
	}

	private void InitializePromo()
	{
		if (!DailyBonusAndPromo.pIsReady || DailyBonusAndPromo.pInstance == null || DailyBonusAndPromo.pInstance.PromoData == null)
		{
			return;
		}
		Promo[] promoData = DailyBonusAndPromo.pInstance.PromoData;
		foreach (Promo promo in promoData)
		{
			if (promo == null || string.IsNullOrEmpty(promo.SlotName))
			{
				break;
			}
			KAWidget kAWidget = FindItem(promo.SlotName);
			if (!(kAWidget == null))
			{
				KAWidget kAWidget2 = kAWidget.FindChildItem("TxtSubtitle");
				if (kAWidget2 != null)
				{
					kAWidget2.SetText(promo.SubTitleText.GetLocalizedString());
					kAWidget2.SetVisibility(inVisible: true);
				}
				ImageLoader imageLoader = null;
				KAWidget kAWidget3 = kAWidget.FindChildItem("BkgIcon");
				DealOfTheDayPromo bestDeal = DailyBonusAndPromo.pInstance.GetBestDeal(promo);
				if (kAWidget3 != null && bestDeal != null && bestDeal.StoreID > 0 && bestDeal.ItemID > 0)
				{
					kAWidget3.SetVisibility(inVisible: true);
					imageLoader = new ImageLoader(bestDeal.StoreID, bestDeal.ItemID, promo.IconRes);
					kAWidget3.SetUserData(imageLoader);
				}
				ImageLoader imageLoader2 = null;
				KAWidget kAWidget4 = kAWidget.FindChildItem("Bkg");
				if (kAWidget4 != null && !string.IsNullOrEmpty(promo.BkgIconRes))
				{
					kAWidget4.SetVisibility(inVisible: true);
					imageLoader2 = new ImageLoader(promo.BkgIconRes);
					kAWidget4.SetUserData(imageLoader2);
				}
				KAWidget kAWidget5 = kAWidget.FindChildItem("BtnGO");
				new PromoFrameImageLoader(imageLoader2, imageLoader).LoadAndSetImages(_Gear);
				if (kAWidget5 != null)
				{
					kAWidget5.SetText(promo.ButtonText.GetLocalizedString());
					kAWidget5.SetVisibility(inVisible: true);
					kAWidget.SetVisibility(inVisible: true);
				}
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		switch (inWidget.name)
		{
		case "EarnGemsBtn":
			SetInteractive(interactive: false);
			UiEarnGems.Show(base.gameObject);
			break;
		case "CloseBtn":
			Close();
			break;
		case "BtnCollectBonus":
			Close();
			break;
		case "BtnAds":
			if (AdManager.pInstance.AdAvailable(mAdEventType, AdType.REWARDED_VIDEO))
			{
				SetInteractive(interactive: false);
				AdManager.DisplayAd(mAdEventType, AdType.REWARDED_VIDEO, base.gameObject);
			}
			break;
		case "BtnGO":
		{
			Promo promoBySlotName = DailyBonusAndPromo.pInstance.GetPromoBySlotName(inWidget.transform.parent.name);
			if (promoBySlotName != null)
			{
				string outMessage = string.Empty;
				if (!CanUsePromo(promoBySlotName, out outMessage))
				{
					mMsgBox = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDB", "WarningDB");
					if (mMsgBox != null)
					{
						mMsgBox._OKMessage = "OnMsgOkClicked";
						mMsgBox._MessageObject = base.gameObject;
						mMsgBox.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
						mMsgBox.SetTextByID(0, outMessage, interactive: false);
						KAUI.SetExclusive(mMsgBox);
						break;
					}
				}
				DealOfTheDayPromo bestDeal = DailyBonusAndPromo.pInstance.GetBestDeal(promoBySlotName);
				if (bestDeal != null)
				{
					KAUIStore._EnterItemID = bestDeal.ItemID;
					Close(pWaitEnd: false);
					StoreLoader.Load(setDefaultMenuItem: true, bestDeal.Category, bestDeal.Store, _MessageObject);
					if (AvAvatar.mTransform != null)
					{
						AvAvatar.pStartPosition = AvAvatar.mTransform.position;
					}
					break;
				}
				if (promoBySlotName.SceneTransition != null)
				{
					if (!string.IsNullOrEmpty(promoBySlotName.SceneTransition.Scene))
					{
						if (promoBySlotName.SceneTransition.IsRace)
						{
							RacingManager.pIsSinglePlayer = true;
							MainStreetMMOClient.pInstance.Disconnect();
						}
						RsResourceManager.LoadLevel(promoBySlotName.SceneTransition.Scene);
					}
				}
				else if (promoBySlotName.Location != null)
				{
					AvAvatar.SetPosition(new Vector3(promoBySlotName.Location.PosX, promoBySlotName.Location.PosY, promoBySlotName.Location.PosZ));
				}
			}
			Close();
			break;
		}
		}
	}

	private void OnMsgOkClicked()
	{
		if (mMsgBox != null)
		{
			UnityEngine.Object.Destroy(mMsgBox.gameObject);
		}
	}

	public void OnInviteFriendClosed()
	{
		SetInteractive(interactive: true);
	}

	public void OnEarnGemsClose()
	{
		SetInteractive(interactive: true);
	}

	private void CollectBonus()
	{
		if (DailyBonusAndPromo.pIsReady && DailyBonusAndPromo.pInstance != null)
		{
			DailyBonus bonusInfoForToday = DailyBonusAndPromo.pInstance.GetBonusInfoForToday();
			if (bonusInfoForToday != null)
			{
				SetInteractive(interactive: false);
				KAUICursorManager.SetDefaultCursor("Loading");
				WsWebService.SetAchievementAndGetReward(bonusInfoForToday.AchID, "", ServiceEventHandler, null);
			}
		}
	}

	private void Close(bool pWaitEnd = true)
	{
		UnityEngine.Object.Destroy(base.gameObject);
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage("OnDailyBonusUIExit", pWaitEnd, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void ServiceEventHandler(WsServiceType inType, WsServiceEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case WsServiceEvent.COMPLETE:
		{
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			int num = -1;
			bool flag = inUserData != null && (bool)inUserData;
			AchievementReward[] array = (AchievementReward[])inObject;
			if (array != null)
			{
				GameUtilities.AddRewards(array);
				AchievementReward[] array2 = array;
				foreach (AchievementReward achievementReward in array2)
				{
					switch (achievementReward.PointTypeID)
					{
					case 6:
						if (CommonInventoryData.pInstance != null)
						{
							CommonInventoryData.ReInit();
							SetInteractive(interactive: false);
							KAUICursorManager.SetDefaultCursor("Loading");
							mReInitInventory = true;
						}
						break;
					case 5:
						if (flag && achievementReward.Amount.HasValue)
						{
							num = achievementReward.Amount.Value;
						}
						break;
					}
				}
			}
			if (flag)
			{
				if (num > 0)
				{
					string localizedString = _AdRewardSuccessText.GetLocalizedString();
					localizedString = localizedString.Replace("%gems%", num.ToString());
					GameUtilities.DisplayOKMessage("PfKAUIGenericDBSm", localizedString, null, "");
				}
				else
				{
					GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _AdRewardFailedText.GetLocalizedString(), null, "");
				}
				AdManager.pInstance.SyncAdAvailableCount(mAdEventType, num > 0);
			}
			break;
		}
		case WsServiceEvent.ERROR:
			SetInteractive(interactive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			if ((bool)inUserData)
			{
				AdManager.pInstance.SyncAdAvailableCount(mAdEventType, isConsumed: false);
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _AdRewardFailedText.GetLocalizedString(), null, "");
			}
			break;
		}
	}

	private bool CanUsePromo(Promo inPromo, out string outMessage)
	{
		outMessage = string.Empty;
		if (inPromo == null || mMsgBox != null)
		{
			outMessage = _UnexpectedPromoErrorText.GetLocalizedString();
			return false;
		}
		if (SanctuaryManager.pCurPetInstance == null)
		{
			if (inPromo.DragonCheck)
			{
				outMessage = inPromo.DragonCheckErrorText.GetLocalizedString();
				return false;
			}
		}
		else
		{
			if (inPromo.BlockedStages != null && inPromo.BlockedStages.Length != 0)
			{
				if (SanctuaryManager.pCurPetData == null)
				{
					outMessage = _UnexpectedPromoErrorText.GetLocalizedString();
					return false;
				}
				string[] blockedStages = inPromo.BlockedStages;
				for (int i = 0; i < blockedStages.Length; i++)
				{
					if (!(blockedStages[i].ToUpper() != SanctuaryManager.pCurPetData.pStage.ToString()))
					{
						outMessage = inPromo.DragonStageErrorText.GetLocalizedString();
						return false;
					}
				}
			}
			if (SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.ENERGY) < (float)inPromo.MinDragonEnergy || SanctuaryManager.pCurPetInstance.GetMeterValue(SanctuaryPetMeterType.HAPPINESS) < (float)inPromo.MinDragonHappiness)
			{
				outMessage = inPromo.NoHappyOrEnergyErrorText.GetLocalizedString();
				return false;
			}
		}
		if (inPromo.MissionID > 0)
		{
			Mission mission = MissionManager.pInstance.GetMission(inPromo.MissionID);
			if (mission == null || !mission.pCompleted)
			{
				outMessage = inPromo.MissionErrorText.GetLocalizedString();
				return false;
			}
		}
		if (inPromo.TaskID <= 0)
		{
			return true;
		}
		Task task = MissionManager.pInstance.GetTask(inPromo.TaskID);
		if (task != null && task.pCompleted)
		{
			return true;
		}
		outMessage = inPromo.TaskErrorText.GetLocalizedString();
		return false;
	}

	public bool UpdateLimitedItem(ItemData itemData, KAWidget _Item)
	{
		if (_Item == null)
		{
			return false;
		}
		KAWidget parentItem = _Item.GetParentItem();
		if (parentItem == null)
		{
			return false;
		}
		parentItem.SetText(string.Empty);
		if (itemData == null)
		{
			return false;
		}
		parentItem.SetText(itemData.ItemName);
		return UpdateWidgetState(itemData, parentItem);
	}

	public void AddLimitedItem(ImageLoader inLoader)
	{
		if (inLoader != null)
		{
			if (mLimitedItems == null)
			{
				mLimitedItems = new List<ImageLoader>();
			}
			mLimitedItems.Add(inLoader);
		}
	}

	public void HandleLimitedItems()
	{
		if (mLimitedItems == null || mLimitedItems.Count == 0)
		{
			return;
		}
		List<ImageLoader> list = null;
		foreach (ImageLoader mLimitedItem in mLimitedItems)
		{
			if (!UpdateLimitedItem(mLimitedItem.pItemData, mLimitedItem._Item))
			{
				if (list == null)
				{
					list = new List<ImageLoader>();
				}
				list.Add(mLimitedItem);
			}
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (ImageLoader item in list)
		{
			mLimitedItems.Remove(item);
		}
	}

	public bool UpdateWidgetState(ItemData inItemData, KAWidget inItemWidget)
	{
		if (inItemData == null || inItemWidget == null)
		{
			return false;
		}
		KAWidget kAWidget = inItemWidget.FindChildItem("AniSaleBurst");
		KAWidget kAWidget2 = inItemWidget.FindChildItem("TxtSaleBurst");
		KAWidget kAWidget3 = inItemWidget.FindChildItem("AniNewBurst");
		KAWidget kAWidget4 = inItemWidget.FindChildItem("AniTimeBurst");
		KAWidget kAWidget5 = inItemWidget.FindChildItem("TxtTimeBurst");
		KAWidget kAWidget6 = inItemWidget.FindChildItem("AniPopularBurst");
		KAWidget kAWidget7 = inItemWidget.FindChildItem("AniFreeBurst");
		if (kAWidget == null || kAWidget3 == null || kAWidget4 == null || kAWidget6 == null || kAWidget7 == null)
		{
			return false;
		}
		UpdateCredits(inItemData, inItemWidget);
		ItemAvailability availability = inItemData.GetAvailability();
		if (inItemData.IsFree())
		{
			if (kAWidget3.GetVisibility())
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
			if (kAWidget4.GetVisibility())
			{
				kAWidget4.SetVisibility(inVisible: false);
			}
			if (kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			if (kAWidget6.GetVisibility())
			{
				kAWidget6.SetVisibility(inVisible: false);
			}
			if (!kAWidget7.GetVisibility())
			{
				kAWidget7.SetVisibility(inVisible: true);
			}
			if (availability != null && availability.EndDate.HasValue)
			{
				return true;
			}
			return false;
		}
		ItemData.ItemSale currentSale = inItemData.GetCurrentSale();
		bool num = currentSale != null;
		if (!kAWidget7.GetVisibility())
		{
			kAWidget7.SetVisibility(inVisible: false);
		}
		if (num)
		{
			DateTime mEndDate = currentSale.mEndDate;
			TimeSpan inTime = mEndDate.Subtract(ServerTime.pCurrentTime);
			if (inTime.TotalSeconds > 0.0 && inTime.TotalHours < 24.0)
			{
				string customizedTimerString = GetCustomizedTimerString(inTime);
				if (!string.IsNullOrEmpty(customizedTimerString))
				{
					kAWidget2.SetText(kAWidget.GetText() + "\n" + customizedTimerString);
				}
			}
			if (kAWidget3.GetVisibility())
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
			if (kAWidget4.GetVisibility())
			{
				kAWidget4.SetVisibility(inVisible: false);
			}
			if (!kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: true);
			}
			if (kAWidget6.GetVisibility())
			{
				kAWidget6.SetVisibility(inVisible: false);
			}
			if (kAWidget7.GetVisibility())
			{
				kAWidget7.SetVisibility(inVisible: false);
			}
			return true;
		}
		if (kAWidget.GetVisibility())
		{
			kAWidget.SetVisibility(inVisible: false);
		}
		if (availability != null && availability.EndDate.HasValue)
		{
			DateTime value = availability.EndDate.Value;
			DateTime dateTime = availability.EndDate.Value.ToLocalTime();
			string text = _AvailableTilText.GetLocalizedString().Replace("XXXX", dateTime.ToString("MM/dd"));
			TimeSpan inTime2 = value.Subtract(ServerTime.pCurrentTime);
			if (inTime2.TotalSeconds > 0.0 && inTime2.TotalHours < 24.0)
			{
				text = GetCustomizedTimerString(inTime2);
			}
			kAWidget5.SetText(text);
			if (kAWidget3.GetVisibility())
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
			if (!kAWidget4.GetVisibility())
			{
				kAWidget4.SetVisibility(inVisible: true);
			}
			if (kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			if (kAWidget6.GetVisibility())
			{
				kAWidget6.SetVisibility(inVisible: false);
			}
			if (kAWidget7.GetVisibility())
			{
				kAWidget7.SetVisibility(inVisible: false);
			}
			return true;
		}
		if (kAWidget4.GetVisibility())
		{
			kAWidget4.SetVisibility(inVisible: false);
		}
		if (inItemData.IsNew)
		{
			if (!kAWidget3.GetVisibility())
			{
				kAWidget3.SetVisibility(inVisible: true);
			}
			if (kAWidget4.GetVisibility())
			{
				kAWidget4.SetVisibility(inVisible: false);
			}
			if (kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			if (kAWidget6.GetVisibility())
			{
				kAWidget6.SetVisibility(inVisible: false);
			}
			if (kAWidget7.GetVisibility())
			{
				kAWidget7.SetVisibility(inVisible: false);
			}
			return false;
		}
		if (kAWidget3.GetVisibility())
		{
			kAWidget3.SetVisibility(inVisible: false);
		}
		if (inItemData.pIsPopular)
		{
			if (kAWidget3.GetVisibility())
			{
				kAWidget3.SetVisibility(inVisible: false);
			}
			if (kAWidget4.GetVisibility())
			{
				kAWidget4.SetVisibility(inVisible: false);
			}
			if (kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: false);
			}
			if (!kAWidget6.GetVisibility())
			{
				kAWidget6.SetVisibility(inVisible: true);
			}
			if (kAWidget7.GetVisibility())
			{
				kAWidget7.SetVisibility(inVisible: false);
			}
			return false;
		}
		if (kAWidget6.GetVisibility())
		{
			kAWidget6.SetVisibility(inVisible: false);
		}
		return false;
	}

	private string GetCustomizedTimerString(TimeSpan inTime)
	{
		if (inTime.TotalMinutes < 1.0)
		{
			return _TimeLeftMinText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt((float)inTime.TotalMinutes).ToString());
		}
		if (inTime.TotalMinutes < 60.0)
		{
			return _TimeLeftMinsText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt((float)inTime.TotalMinutes).ToString());
		}
		if (inTime.TotalHours < 24.0)
		{
			return _TimeLeftHoursText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt(inTime.Hours).ToString());
		}
		return string.Empty;
	}

	public void UpdateCredits(ItemData inItemData, KAWidget inWidget)
	{
		if (inItemData == null || inWidget == null)
		{
			return;
		}
		KAWidget kAWidget = inWidget.FindChildItem("AniCreditCard");
		KAWidget kAWidget2 = inWidget.FindChildItem("TxtCurrency");
		KAWidget kAWidget3 = inWidget.FindChildItem("TxtStriked");
		if (kAWidget3 == null)
		{
			kAWidget3 = inWidget.FindChildItem("TxtStrikedCurrency");
		}
		UILabel label = kAWidget3.GetLabel();
		float num = -1f;
		Texture texture = null;
		int num2 = 0;
		int num3 = 0;
		if (inItemData.GetPurchaseType() == 1)
		{
			num2 = inItemData.FinalCost;
			texture = _CoinsTexture;
			num3 = (SubscriptionInfo.pIsMember ? inItemData.pMemberCost : inItemData.pNonMemberCost);
		}
		else
		{
			num2 = inItemData.FinalCashCost;
			texture = _GemsTexture;
			num3 = (SubscriptionInfo.pIsMember ? inItemData.pMemberCashCost : inItemData.pNonMemberCashCost);
		}
		if (kAWidget != null)
		{
			if (!kAWidget.GetVisibility())
			{
				kAWidget.SetVisibility(inVisible: true);
			}
			kAWidget.SetTexture(texture);
		}
		if (kAWidget2 != null)
		{
			if (!kAWidget2.GetVisibility())
			{
				kAWidget2.SetVisibility(inVisible: true);
			}
			kAWidget2.SetText(num2.ToString());
		}
		if (kAWidget3 != null)
		{
			if (num3 > num2)
			{
				kAWidget3.SetVisibility(inVisible: true);
				kAWidget3.SetText(num3.ToString());
				if (label != null)
				{
					num = label.transform.localScale.x * (float)label.width + 5f;
				}
				if (kAWidget2 != null && kAWidget2.GetLabel() != null)
				{
					kAWidget2.GetLabel().color = _OfferPriceColor;
				}
			}
			else
			{
				if (kAWidget2 != null)
				{
					if (kAWidget2.GetLabel() != null)
					{
						kAWidget2.GetLabel().color = Color.black;
					}
					Vector3 position = kAWidget3.GetPosition();
					kAWidget2.SetPosition(position.x, position.y);
				}
				kAWidget3.SetVisibility(inVisible: false);
				kAWidget3.SetText(string.Empty);
			}
		}
		if (!(num <= 0f) && !(kAWidget3.pBackground == null))
		{
			Vector3 localScale = kAWidget3.pBackground.transform.localScale;
			localScale.x = num;
			kAWidget3.pBackground.transform.localScale = localScale;
			Vector3 localPosition = kAWidget3.pBackground.transform.localPosition;
			localPosition.x = num / 2f;
			kAWidget3.pBackground.transform.localPosition = localPosition;
		}
	}

	public void SetAdData(AdEventType adType)
	{
		mAdEventType = adType;
		KAWidget kAWidget = FindItem("BtnAds");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	public void OnAdWatched()
	{
		AdManager.pInstance.LogAdWatchedEvent(mAdEventType, "RewardGems");
		string adReward = AdManager.pInstance.GetAdReward(mAdEventType);
		int adRewardForCurrentDay = GetAdRewardForCurrentDay(adReward);
		if (adRewardForCurrentDay != -1)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			WsWebService.SetAchievementAndGetReward(adRewardForCurrentDay, "", ServiceEventHandler, true);
		}
		else
		{
			SetInteractive(interactive: true);
		}
	}

	public void OnAdFailed()
	{
		UtDebug.LogError("OnAdFailed for event:- " + mAdEventType);
		SetInteractive(interactive: true);
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

	private int GetAdRewardForCurrentDay(string rewardData)
	{
		if (string.IsNullOrEmpty(rewardData))
		{
			return -1;
		}
		string[] array = rewardData.Split(',');
		if (array.Length == 0)
		{
			return -1;
		}
		int dayOfWeek = (int)ServerTime.pCurrentTime.ToLocalTime().DayOfWeek;
		string[] array2 = array;
		for (int i = 0; i < array2.Length; i++)
		{
			string[] array3 = array2[i].Split(':');
			if (array3.Length > 1 && int.TryParse(array3[0], out var result) && dayOfWeek == result && int.TryParse(array3[1], out var result2) && result2 != -1)
			{
				return result2;
			}
		}
		return -1;
	}
}
