using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiDreadfallItemExchange : UiItemExchange
{
	public LocaleString _TouchInputText = new LocaleString("Tap");

	public LocaleString _NonTouchInputText = new LocaleString("Click");

	public int _ExchangeItemId = 16667;

	public Color _ClaimRewardActiveColor = Color.red;

	public float _CandyCountUpdateTime = 1f;

	public MinMax _CandyFlowTime;

	public List<KAWidget> _Candies;

	public float _ProgressBarUpdateTime = 1f;

	public float _ProgressBarNextLevelDelay = 0.5f;

	public GameObject _CauldronFX;

	private UiDreadfallItemExchangeMenu mDisplayMenu;

	private KAWidget mBtnClaimRewards;

	private KAWidget mBtnInfo;

	private KAWidget mTxtInfo;

	private KAWidget mTxtCandiesTotal;

	private KAWidget mWidgetCauldron;

	private KAWidget mIcoCandy;

	private KAWidget mProgressBar;

	private KAToggleButton mBtnOwned;

	private KAToggleButton mBtnAll;

	private KAUI mTutorialUI;

	private bool mOwnedItemsOnly;

	private bool mClaimableRewards;

	private int mLoadIndex;

	private float mCandiesTimer;

	private int mCandiesToAdd;

	private int mCurrentCandies;

	private int mPrevCandiesAdded;

	private bool mRunCandiesCounter;

	private bool mPlayProgressBar;

	private UserAchievementTask mAchievementTask;

	private ArrayOfAchievementTaskInfo mAchievementTaskInfoList;

	private int mPrevAcquiredQuantity;

	private int mAcquiredQuantity;

	private static UiDreadfallItemExchange mInstance;

	public static void Load()
	{
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("DreadfallItemExchangeAsset"), OnBundleLoaded, typeof(GameObject));
	}

	private static void OnBundleLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject).name = ((GameObject)inObject).name;
			break;
		case RsResourceLoadEvent.ERROR:
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Dreadfall Item Exchange Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected override void Start()
	{
		if (mInstance == null)
		{
			mInstance = this;
			if (DreadfallAchievementManager.pInstance == null || !DreadfallAchievementManager.pInstance.EventInProgress() || DreadfallAchievementManager.pInstance.AchievementTaskInfoList == null)
			{
				GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemUnavailableText.GetLocalizedString(), null, "");
				CloseUi();
				return;
			}
			base.Start();
			mDisplayMenu = _MenuList[0] as UiDreadfallItemExchangeMenu;
			mProgressBar = FindItem("ProgressBar");
			mWidgetCauldron = FindItem("Cauldron");
			mBtnClaimRewards = FindItem("BtnClaimRewards");
			mBtnOwned = FindItem("BtnOwned") as KAToggleButton;
			mBtnAll = FindItem("BtnAll") as KAToggleButton;
			mBtnInfo = FindItem("BtnInfo");
			mTxtInfo = FindItem("TxtInfo");
			mTxtCandiesTotal = FindItem("TxtCandyCount");
			mIcoCandy = FindItem("IcoCandy");
			mTutorialUI = _UiList[0];
			if (mBtnOwned != null)
			{
				mOwnedItemsOnly = mBtnOwned._StartChecked;
			}
			if (mTxtInfo != null)
			{
				string text = mTxtInfo.GetText().Replace("{{Input}}", KAInput.pInstance.IsTouchInput() ? _TouchInputText.GetLocalizedString() : _NonTouchInputText.GetLocalizedString());
				mTxtInfo.SetText(text);
			}
			ShowTutorial(show: false);
			mLoadIndex = 2;
			KAUICursorManager.SetDefaultCursor("Loading");
			mAchievementTaskInfoList = DreadfallAchievementManager.pInstance.AchievementTaskInfoList;
			DreadfallAchievementManager.pInstance.GetAchievementTask(AchievementTaskReady);
			DreadfallAchievementManager.pInstance.GetRedeemableRewards(RedeemableAchievementReady);
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mRunCandiesCounter)
		{
			int num = Mathf.FloorToInt((float)mCandiesToAdd * (mCandiesTimer / _CandyCountUpdateTime));
			if (num > mCandiesToAdd)
			{
				num = mCandiesToAdd;
			}
			num = Mathf.Min(num, mCandiesToAdd);
			mCurrentCandies += num - mPrevCandiesAdded;
			if (mTxtCandiesTotal != null)
			{
				mTxtCandiesTotal.SetText(mCurrentCandies.ToString());
			}
			mPrevCandiesAdded = num;
			if (mCandiesTimer >= _CandyCountUpdateTime)
			{
				mRunCandiesCounter = false;
				mCandiesTimer = 0f;
			}
			else
			{
				mCandiesTimer += Time.deltaTime;
			}
		}
	}

	private IEnumerator PlayProgressBar(int currentProgress = 0)
	{
		mPlayProgressBar = true;
		SetInteractive(interactive: false);
		float currentValue = currentProgress;
		GetLevelInfo(out var start, out var end, (int)currentValue);
		float amount = ((mAcquiredQuantity >= end && GetNextPointValue((int)currentValue) != mAcquiredQuantity) ? ((float)end - currentValue) : ((float)mAcquiredQuantity - currentValue));
		while ((int)currentValue < mAcquiredQuantity)
		{
			currentValue += Time.deltaTime * amount / _ProgressBarUpdateTime;
			currentValue = ((!(currentValue >= (float)end) || GetNextPointValue((int)currentValue) == mAcquiredQuantity) ? Mathf.Clamp(currentValue, start, mAcquiredQuantity) : Mathf.Clamp(currentValue, start, end));
			mProgressBar.SetProgressLevel((currentValue - (float)start) / (float)(end - start));
			mProgressBar.SetText((int)currentValue + "/" + end);
			if ((int)currentValue == end)
			{
				yield return new WaitForSeconds(_ProgressBarNextLevelDelay);
				GetLevelInfo(out start, out end, (int)currentValue);
				amount = ((mAcquiredQuantity >= end && GetNextPointValue((int)currentValue) != mAcquiredQuantity) ? ((float)end - currentValue) : ((float)mAcquiredQuantity - currentValue));
				mProgressBar.SetText((int)currentValue + "/" + end);
				mProgressBar.SetProgressLevel((currentValue - (float)start) / (float)(end - start));
			}
			yield return null;
		}
		mPlayProgressBar = false;
		SetInteractive(interactive: true);
	}

	public override void EndMoveTo(KAWidget widget)
	{
		base.EndMoveTo(widget);
		mRunCandiesCounter = true;
		if (!mPlayProgressBar && mProgressBar != null)
		{
			StartCoroutine("PlayProgressBar", mCurrentCandies);
		}
		widget.transform.localPosition = widget.pOrgPosition;
		widget.SetVisibility(inVisible: false);
	}

	private void AchievementTaskReady(UserAchievementTask achTask)
	{
		mLoadIndex--;
		mAchievementTask = achTask;
		if (mLoadIndex <= 0)
		{
			EventDataLoaded();
		}
	}

	private void RedeemableAchievementReady(UserAchievementTaskRedeemableRewards redeemableRewards)
	{
		mLoadIndex--;
		mClaimableRewards = AchievementRedeemPending(redeemableRewards);
		if (mLoadIndex <= 0)
		{
			EventDataLoaded();
		}
	}

	private bool AchievementRedeemPending(UserAchievementTaskRedeemableRewards rewards)
	{
		if (rewards != null && rewards.RedeemableRewards != null)
		{
			for (int i = 0; i < mAchievementTaskInfoList.AchievementTaskInfo.Length; i++)
			{
				AchievementTaskInfo info = mAchievementTaskInfoList.AchievementTaskInfo[i];
				if (DreadfallAchievementManager.pInstance.AchievementVisible(info) && Array.Find(rewards.RedeemableRewards, (UserAchievementTaskRedeemableReward x) => x.AchievementInfoID == info.AchievementInfoID) != null)
				{
					return true;
				}
			}
		}
		return false;
	}

	private void EventDataLoaded()
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (mAchievementTask == null)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemUnavailableText.GetLocalizedString(), null, "");
			CloseUi();
			return;
		}
		GetExchangeItems();
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(_ExchangeItemId);
		if (userItemData != null)
		{
			mAcquiredQuantity = userItemData.Quantity;
		}
		UpdateProgress();
	}

	private void UpdateProgress(bool animate = false)
	{
		ActivateClaimButton();
		GetLevelInfo(out var start, out var end, mAcquiredQuantity);
		DisplayProgress(start, end, animate);
		mPrevAcquiredQuantity = mAcquiredQuantity;
	}

	private void GetLevelInfo(out int start, out int end, int level)
	{
		start = 0;
		end = 0;
		if (mAchievementTaskInfoList.AchievementTaskInfo == null || mAchievementTaskInfoList.AchievementTaskInfo.Length == 0)
		{
			return;
		}
		int num = mAchievementTaskInfoList.AchievementTaskInfo.Length;
		for (int i = 0; i < num && DreadfallAchievementManager.pInstance.AchievementVisible(mAchievementTaskInfoList.AchievementTaskInfo[i]); i++)
		{
			start = ((i != 0) ? mAchievementTaskInfoList.AchievementTaskInfo[i - 1].PointValue : 0);
			end = mAchievementTaskInfoList.AchievementTaskInfo[i].PointValue;
			if (level < mAchievementTaskInfoList.AchievementTaskInfo[i].PointValue)
			{
				break;
			}
		}
	}

	private void DisplayProgress(int startValue, int endValue, bool animateCount = false)
	{
		if (animateCount)
		{
			if (!(mTxtCandiesTotal != null))
			{
				return;
			}
			bool flag = false;
			foreach (KAWidget candy in _Candies)
			{
				if (!(candy == null) && mIcoCandy != null)
				{
					flag = true;
					candy.SetVisibility(inVisible: true);
					candy.MoveTo(mIcoCandy.GetPosition(), _CandyFlowTime.GetRandomFloat());
				}
			}
			mRunCandiesCounter = !flag;
			if (!mPlayProgressBar && !flag && mProgressBar != null)
			{
				StartCoroutine("PlayProgressBar", mCurrentCandies);
			}
			mCandiesToAdd = mAcquiredQuantity - mPrevAcquiredQuantity;
			mCurrentCandies = mPrevAcquiredQuantity;
			mPrevCandiesAdded = 0;
		}
		else
		{
			if (mTxtCandiesTotal != null)
			{
				mTxtCandiesTotal.SetText(mAcquiredQuantity.ToString());
			}
			if (mProgressBar != null)
			{
				GetLevelInfo(out var start, out var end, mAcquiredQuantity);
				mProgressBar.SetText(mAcquiredQuantity + "/" + end);
				mProgressBar.SetProgressLevel((float)(mAcquiredQuantity - start) / (float)(end - start));
			}
		}
	}

	private int GetNextPointValue(int value)
	{
		if (mAchievementTaskInfoList.AchievementTaskInfo != null && mAchievementTaskInfoList.AchievementTaskInfo.Length != 0)
		{
			for (int i = 0; i < mAchievementTaskInfoList.AchievementTaskInfo.Length && DreadfallAchievementManager.pInstance.AchievementVisible(mAchievementTaskInfoList.AchievementTaskInfo[i]); i++)
			{
				if (value < mAchievementTaskInfoList.AchievementTaskInfo[i].PointValue)
				{
					return mAchievementTaskInfoList.AchievementTaskInfo[i].PointValue;
				}
			}
		}
		return mAcquiredQuantity;
	}

	private void ActivateClaimButton()
	{
		if (mClaimableRewards && mBtnClaimRewards != null && mBtnClaimRewards.pBackground != null)
		{
			mBtnClaimRewards.pBackground.color = _ClaimRewardActiveColor;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mBtnOwned || inWidget == mBtnAll)
		{
			mOwnedItemsOnly = mBtnOwned.IsChecked();
			DisplayItems();
		}
		else if (inWidget == mBtnClaimRewards)
		{
			CloseUi();
			UiDreadfall.Load();
		}
		else if (inWidget == mWidgetCauldron)
		{
			ShowTutorial(show: true);
		}
		else if (mDisplayMenu != null && mDisplayMenu.GetItems() != null && mDisplayMenu.GetItems().Contains(inWidget))
		{
			ShowTutorial(show: true);
		}
		else if (inWidget == mBtnInfo && mTxtInfo != null)
		{
			mTxtInfo.SetVisibility(!mTxtInfo.GetVisibility());
		}
	}

	public void ItemDropped(KAWidget droppedWidget)
	{
		GameObject hoveredObject = UICamera.hoveredObject;
		ItemExchangeUserData itemExchangeUserData = droppedWidget.GetUserData() as ItemExchangeUserData;
		if (hoveredObject != null && hoveredObject.GetComponent<KAWidget>() == mWidgetCauldron && itemExchangeUserData != null)
		{
			if (_CauldronFX != null)
			{
				_CauldronFX.SetActive(value: true);
			}
			ShowTutorial(show: false);
			ProcessItemExchange(itemExchangeUserData);
		}
		else if (itemExchangeUserData != null)
		{
			SyncFromItemCount(itemExchangeUserData._Item);
		}
	}

	public void ItemPicked(KAWidget pickedWidget)
	{
		ItemExchangeUserData itemExchangeUserData = pickedWidget.GetUserData() as ItemExchangeUserData;
		if (!ExchangeAllowed(pickedWidget.GetUserData() as ItemExchangeUserData) || !(mDisplayMenu != null))
		{
			return;
		}
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(itemExchangeUserData._FromItem.Item.ItemID);
		if (userItemData == null)
		{
			return;
		}
		KAWidget kAWidget = CreateDragObject(pickedWidget, mDisplayMenu.pPanel.depth + 1);
		KAWidgetUserData userData = kAWidget.GetUserData();
		if (userData != null)
		{
			userData._Item = pickedWidget;
		}
		if (userItemData.Quantity == itemExchangeUserData._FromItem.Quantity)
		{
			pickedWidget.SetVisibility(inVisible: false);
			return;
		}
		KAWidget kAWidget2 = pickedWidget.FindChildItem("TxtAvailableCount");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText((userItemData.Quantity - itemExchangeUserData._FromItem.Quantity).ToString());
		}
		kAWidget2 = kAWidget.FindChildItem("TxtAvailableCount");
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(itemExchangeUserData._FromItem.Quantity.ToString());
		}
	}

	protected override void ExchangeItemsLoaded()
	{
		if (mDisplayMenu != null)
		{
			ShowTutorial(show: true);
			DisplayItems();
		}
	}

	protected override void ItemExchangeDone(ItemExchangeUserData itemExchangeUserData)
	{
		if (_CauldronFX != null)
		{
			_CauldronFX.SetActive(value: false);
		}
		if (itemExchangeUserData != null)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(_ExchangeItemId);
			if (userItemData != null)
			{
				mAcquiredQuantity = userItemData.Quantity;
			}
			mClaimableRewards = mAcquiredQuantity > GetNextPointValue(mPrevAcquiredQuantity);
			UpdateProgress(animate: true);
			SyncFromItemCount(itemExchangeUserData._Item);
		}
	}

	protected override void ItemExchangeFailed(ItemExchangeUserData itemExchangeUserData)
	{
		if (_CauldronFX != null)
		{
			_CauldronFX.SetActive(value: false);
		}
		if (itemExchangeUserData != null)
		{
			SyncFromItemCount(itemExchangeUserData._Item);
		}
	}

	private void DisplayItems()
	{
		mDisplayMenu.ClearItems();
		int num = -1;
		ItemExchange[] array = mItemExchange;
		foreach (ItemExchange exchangeItem in array)
		{
			if (exchangeItem.Type != ExchangeType.From || !AvatarData.ValidGenderItem(exchangeItem.Item))
			{
				continue;
			}
			ItemExchange itemExchange = Array.Find(mItemExchange, (ItemExchange x) => x.Type == ExchangeType.To && x.ExchangeGroupID == exchangeItem.ExchangeGroupID && x.Item.ItemID == _ExchangeItemId);
			if (itemExchange != null)
			{
				UserItemData userItemData = null;
				if (CommonInventoryData.pIsReady)
				{
					userItemData = CommonInventoryData.pInstance.FindItem(exchangeItem.Item.ItemID);
				}
				if (userItemData != null)
				{
					num++;
					CreateWidget(new ItemExchangeUserData(exchangeItem.ExchangeGroupID, exchangeItem, itemExchange), num, userItemData.Quantity);
				}
				else if (!mOwnedItemsOnly)
				{
					CreateWidget(new ItemExchangeUserData(exchangeItem.ExchangeGroupID, exchangeItem, itemExchange), mDisplayMenu.GetItemCount());
				}
			}
		}
	}

	private void CreateWidget(ItemExchangeUserData widgetData, int insertIndex, int availableCount = 0)
	{
		KAWidget kAWidget = mDisplayMenu.AddWidget("ExchangeItem", (availableCount > 0) ? widgetData : null);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtItemName");
		KAWidget kAWidget3 = kAWidget.FindChildItem("TxtOutputCount");
		KAWidget kAWidget4 = kAWidget.FindChildItem("TxtAvailableCount");
		KAWidget kAWidget5 = kAWidget.FindChildItem("BkgIcon");
		KAWidget kAWidget6 = kAWidget.FindChildItem("GreyMask");
		if (kAWidget6 != null)
		{
			kAWidget6.SetVisibility(availableCount == 0);
		}
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(widgetData._FromItem.Item.ItemName);
		}
		if (kAWidget3 != null)
		{
			kAWidget3.SetText(widgetData._ToItem.Quantity.ToString());
		}
		if (kAWidget4 != null)
		{
			kAWidget4.SetVisibility(availableCount > 0);
			kAWidget4.SetText((availableCount > 0) ? availableCount.ToString() : "");
		}
		if (kAWidget5 != null)
		{
			kAWidget5.SetTextureFromBundle(widgetData._FromItem.Item.IconName);
		}
		mDisplayMenu.SetWidgetIndex(kAWidget, insertIndex);
	}

	private void SyncFromItemCount(KAWidget widget)
	{
		if (!(widget.GetUserData() is ItemExchangeUserData itemExchangeUserData) || !CommonInventoryData.pIsReady)
		{
			return;
		}
		KAWidget kAWidget = widget.FindChildItem("TxtAvailableCount");
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(itemExchangeUserData._FromItem.Item.ItemID);
		if (userItemData == null || userItemData.Quantity <= 0)
		{
			if (mOwnedItemsOnly)
			{
				mDisplayMenu.RemoveWidget(widget);
			}
			else
			{
				DisplayItems();
			}
			return;
		}
		if (kAWidget != null)
		{
			kAWidget.SetText(userItemData.Quantity.ToString());
		}
		if (!widget.GetVisibility())
		{
			widget.SetVisibility(inVisible: true);
		}
	}

	private void ShowTutorial(bool show)
	{
		if (mTutorialUI != null)
		{
			mTutorialUI.SetVisibility(show);
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		if (mPlayProgressBar)
		{
			StopCoroutine("PlayProgressBar");
		}
	}
}
