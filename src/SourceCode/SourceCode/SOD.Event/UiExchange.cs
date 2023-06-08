using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace SOD.Event;

public class UiExchange : UiItemExchange
{
	[Serializable]
	public class EffectsData
	{
		public GameObject _FX;

		public float _Duration;

		public Vector3 _Offset;

		public void Play(Vector3 position)
		{
			if (_FX != null)
			{
				_FX.SetActive(value: true);
				_FX.transform.position = position + _Offset;
			}
		}

		public void Stop()
		{
			if (_FX != null)
			{
				_FX.SetActive(value: false);
			}
		}
	}

	[Header("Event Name")]
	public string _EventName = "";

	public int _CategoryId;

	public LocaleString _TouchInputText = new LocaleString("Tap");

	public LocaleString _NonTouchInputText = new LocaleString("Click");

	public Color _ClaimRewardActiveColor = Color.red;

	public float _CountUpdateTime = 1f;

	public MinMax _FlowTime;

	public List<KAWidget> _Items;

	public float _ProgressBarUpdateTime = 1f;

	public float _ProgressBarNextLevelDelay = 0.5f;

	public GameObject _ExchangeFX;

	[Header("Mission Data")]
	public Color _ExchangeAvailableColor = Color.green;

	public EffectsData _ProcessingFXInfo;

	public EffectsData _CompletedFXInfo;

	public LocaleString _ExchangeMaxLimitText = new LocaleString("You can exchange Max [COUNT]");

	[Header("Widgets")]
	[SerializeField]
	private KAWidget m_BtnClaimRewards;

	[SerializeField]
	private KAWidget m_BtnInfo;

	[SerializeField]
	private KAWidget m_BtnHowToPlay;

	[SerializeField]
	private KAWidget m_TxtInfo;

	[SerializeField]
	private KAWidget m_TxtTotal;

	[SerializeField]
	private KAWidget m_EventEndDays;

	[SerializeField]
	private KAWidget m_RedeemEndDays;

	[SerializeField]
	private KAWidget m_ExchangeArea;

	[SerializeField]
	private KAWidget m_IcoItem;

	[SerializeField]
	private KAWidget m_ProgressBar;

	[SerializeField]
	private KAToggleButton m_BtnOwned;

	[SerializeField]
	private KAToggleButton m_BtnAll;

	[Header("UIs")]
	[SerializeField]
	private UiExchangeMenu m_ExchangeMenu;

	[SerializeField]
	private KAUIMenu m_CurrencyMenu;

	[SerializeField]
	private KAUI m_TutorialUI;

	[SerializeField]
	private GameObject m_HelpScreen;

	[SerializeField]
	private LocaleString m_EventEndedText = new LocaleString("Event has ended.");

	private EventManager mEventManager;

	private UiHelpScreen mUiHelpScreen;

	private bool mOwnedItemsOnly;

	private bool mClaimableRewards;

	private int mLoadIndex;

	private float mTimer;

	private int mItemsToAdd;

	private int mCurrentItems;

	private int mPrevItemsAdded;

	private bool mRunCounter;

	private bool mPlayProgressBar;

	private UserAchievementTask mAchievementTask;

	private ArrayOfAchievementTaskInfo mAchievementTaskInfoList;

	private int mPrevAcquiredQuantity;

	private int mAcquiredQuantity;

	private Dictionary<int, KAWidget> mCurrencyMap;

	private EffectsData mEffectsData;

	private Coroutine mEffectsCoroutine;

	public static void Load(string eventName)
	{
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(EventManager.Get(eventName)._ExchangeAssetName, OnBundleLoaded, typeof(GameObject));
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

	public override void GetExchangeItems()
	{
		KAUICursorManager.SetDefaultCursor("Loading");
		if (mEventManager._ItemId > 0)
		{
			WsWebService.GetExchangeItemListByItem(new int[1] { mEventManager._ItemId }, base.GetExchangeItemsEventHandler, null);
		}
		else
		{
			WsWebService.GetAllExchangeItems(base.GetExchangeItemsEventHandler, null);
		}
	}

	private void OnUiProgressionLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			UiPrizeProgression component = obj.GetComponent<UiPrizeProgression>();
			if ((bool)component && m_UserNotifyEvent != null)
			{
				component.m_UserNotifyEvent = m_UserNotifyEvent;
				component.OnClosed = (Action)Delegate.Combine(component.OnClosed, new Action(m_UserNotifyEvent.MarkUserNotifyDone));
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading Dreadfall Item Exchange Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected override void Start()
	{
		mEventManager = EventManager.Get(_EventName);
		if (mEventManager == null || !mEventManager.EventInProgress() || mEventManager.AchievementTaskInfoList == null)
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemUnavailableText.GetLocalizedString(), null, "");
			CloseUi();
			return;
		}
		base.Start();
		if (m_BtnOwned != null)
		{
			mOwnedItemsOnly = m_BtnOwned._StartChecked;
		}
		if (m_TxtInfo != null)
		{
			string text = m_TxtInfo.GetText().Replace("{{Input}}", KAInput.pInstance.IsTouchInput() ? _TouchInputText.GetLocalizedString() : _NonTouchInputText.GetLocalizedString());
			m_TxtInfo.SetText(text);
		}
		ShowTutorial(show: false);
		mLoadIndex = 2;
		switch (mEventManager._Type)
		{
		case Type.ACHIEVEMENT_TASK:
			KAUICursorManager.SetDefaultCursor("Loading");
			mAchievementTaskInfoList = mEventManager.AchievementTaskInfoList;
			mEventManager.GetAchievementTask(AchievementTaskReady);
			mEventManager.GetRedeemableRewards(RedeemableAchievementReady);
			break;
		case Type.MISSION:
			mCurrencyMap = new Dictionary<int, KAWidget>();
			if (m_ExchangeMenu != null)
			{
				m_ExchangeMenu.pEvents.OnClick += OnMenuItemClick;
			}
			GetExchangeItems();
			break;
		}
		DisplayEndDate();
	}

	protected override void Update()
	{
		base.Update();
		if (mRunCounter)
		{
			int num = Mathf.FloorToInt((float)mItemsToAdd * (mTimer / _CountUpdateTime));
			if (num > mItemsToAdd)
			{
				num = mItemsToAdd;
			}
			num = Mathf.Min(num, mItemsToAdd);
			mCurrentItems += num - mPrevItemsAdded;
			if (m_TxtTotal != null)
			{
				m_TxtTotal.SetText(mCurrentItems.ToString());
			}
			mPrevItemsAdded = num;
			if (mTimer >= _CountUpdateTime)
			{
				mRunCounter = false;
				mTimer = 0f;
			}
			else
			{
				mTimer += Time.deltaTime;
			}
		}
	}

	private void DisplayEndDate()
	{
		int num = ((mEventManager != null) ? mEventManager.GetEventRemainingTime().Days : 0);
		if (m_EventEndDays != null)
		{
			m_EventEndDays.SetText(mEventManager.GracePeriodInProgress() ? m_EventEndedText.GetLocalizedString() : string.Format(m_EventEndDays.GetText(), num));
		}
		if (m_RedeemEndDays != null)
		{
			m_RedeemEndDays.SetText(string.Format(m_RedeemEndDays.GetText(), mEventManager.GracePeriodInProgress() ? num : (num + mEventManager._GracePeriodDays)));
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
			m_ProgressBar.SetProgressLevel((currentValue - (float)start) / (float)(end - start));
			m_ProgressBar.SetText((int)currentValue + "/" + end);
			if ((int)currentValue == end)
			{
				yield return new WaitForSeconds(_ProgressBarNextLevelDelay);
				GetLevelInfo(out start, out end, (int)currentValue);
				amount = ((mAcquiredQuantity >= end && GetNextPointValue((int)currentValue) != mAcquiredQuantity) ? ((float)end - currentValue) : ((float)mAcquiredQuantity - currentValue));
				m_ProgressBar.SetText((int)currentValue + "/" + end);
				m_ProgressBar.SetProgressLevel((currentValue - (float)start) / (float)(end - start));
			}
			yield return null;
		}
		mPlayProgressBar = false;
		SetInteractive(interactive: true);
	}

	private void PlayFX(EffectsData data, Vector3 position)
	{
		StopFX();
		if (data != null)
		{
			mEffectsData = data;
			mEffectsCoroutine = StartCoroutine(ProcessFX(position));
		}
	}

	private void StopFX()
	{
		if (mEffectsCoroutine != null)
		{
			StopCoroutine(mEffectsCoroutine);
			mEffectsCoroutine = null;
		}
		if (mEffectsData != null)
		{
			mEffectsData.Stop();
			mEffectsData = null;
		}
	}

	private IEnumerator ProcessFX(Vector3 offset)
	{
		if (mEffectsData != null)
		{
			mEffectsData.Play(offset);
			if (mEffectsData._Duration > 0f)
			{
				yield return new WaitForSeconds(mEffectsData._Duration);
				mEffectsData.Stop();
			}
		}
	}

	public override void EndMoveTo(KAWidget widget)
	{
		base.EndMoveTo(widget);
		mRunCounter = true;
		if (!mPlayProgressBar && m_ProgressBar != null)
		{
			StartCoroutine("PlayProgressBar", mCurrentItems);
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
				if (mEventManager.AchievementVisible(info) && Array.Find(rewards.RedeemableRewards, (UserAchievementTaskRedeemableReward x) => x.AchievementInfoID == info.AchievementInfoID) != null)
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
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(mEventManager._ItemId);
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
		for (int i = 0; i < num && mEventManager.AchievementVisible(mAchievementTaskInfoList.AchievementTaskInfo[i]); i++)
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
			if (!(m_TxtTotal != null))
			{
				return;
			}
			bool flag = false;
			foreach (KAWidget item in _Items)
			{
				if (!(item == null) && m_IcoItem != null)
				{
					flag = true;
					item.SetVisibility(inVisible: true);
					item.MoveTo(m_IcoItem.GetPosition(), _FlowTime.GetRandomFloat());
				}
			}
			mRunCounter = !flag;
			if (!mPlayProgressBar && !flag && m_ProgressBar != null)
			{
				StartCoroutine("PlayProgressBar", mCurrentItems);
			}
			mItemsToAdd = mAcquiredQuantity - mPrevAcquiredQuantity;
			mCurrentItems = mPrevAcquiredQuantity;
			mPrevItemsAdded = 0;
		}
		else
		{
			if (m_TxtTotal != null)
			{
				m_TxtTotal.SetText(mAcquiredQuantity.ToString());
			}
			if (m_ProgressBar != null)
			{
				GetLevelInfo(out var start, out var end, mAcquiredQuantity);
				m_ProgressBar.SetText(mAcquiredQuantity + "/" + end);
				m_ProgressBar.SetProgressLevel((float)(mAcquiredQuantity - start) / (float)(end - start));
			}
		}
	}

	private int GetNextPointValue(int value)
	{
		if (mAchievementTaskInfoList.AchievementTaskInfo != null && mAchievementTaskInfoList.AchievementTaskInfo.Length != 0)
		{
			for (int i = 0; i < mAchievementTaskInfoList.AchievementTaskInfo.Length && mEventManager.AchievementVisible(mAchievementTaskInfoList.AchievementTaskInfo[i]); i++)
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
		if (mClaimableRewards && m_BtnClaimRewards != null && m_BtnClaimRewards.pBackground != null)
		{
			m_BtnClaimRewards.pBackground.color = _ClaimRewardActiveColor;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == m_BtnOwned || inWidget == m_BtnAll)
		{
			mOwnedItemsOnly = m_BtnOwned.IsChecked();
			DisplayItems();
		}
		else if (inWidget == m_BtnClaimRewards)
		{
			CloseUi(invokeOnClosed: false);
			AvAvatar.SetUIActive(inActive: false);
			KAUICursorManager.SetDefaultCursor("Loading");
			RsResourceManager.LoadAssetFromBundle(EventManager.Get(_EventName)._AssetName, OnUiProgressionLoaded, typeof(GameObject));
		}
		else if (inWidget == m_ExchangeArea)
		{
			ShowTutorial(show: true);
		}
		else if (m_ExchangeMenu != null && m_ExchangeMenu.GetItems() != null && m_ExchangeMenu.GetItems().Contains(inWidget))
		{
			ShowTutorial(show: true);
		}
		else if (inWidget == m_BtnInfo)
		{
			m_TxtInfo?.SetVisibility(!m_TxtInfo.GetVisibility());
		}
		else if (inWidget == m_BtnHowToPlay)
		{
			OpenHelp();
		}
	}

	public override void OnSelect(KAWidget widget, bool inSelected)
	{
		widget.OnSelect(inSelected);
		if (widget.name == "TxtQuantityValue" && widget.pParentWidget != null)
		{
			UpdateExchangeCount(widget.pParentWidget);
		}
	}

	public override void OnSubmit(KAWidget inWidget)
	{
		inWidget.OnSubmit();
		if (inWidget.name == "TxtQuantityValue" && inWidget.pParentWidget != null)
		{
			UpdateExchangeCount(inWidget.pParentWidget);
		}
	}

	private void OnMenuItemClick(KAWidget widget)
	{
		if (!(widget.pParentWidget != null))
		{
			return;
		}
		if (widget.name == "BtnPositive")
		{
			UpdateExchangeCount(widget.pParentWidget, 1);
		}
		else if (widget.name == "BtnNegative")
		{
			UpdateExchangeCount(widget.pParentWidget, -1);
		}
		else
		{
			if (!(widget.name == "BtnExchange") || !(widget.pParentWidget != null) || widget.pParentWidget.GetUserData() == null)
			{
				return;
			}
			PlayFX(_ProcessingFXInfo, widget.pParentWidget.transform.position);
			ProcessItemExchange(widget.pParentWidget.GetUserData() as ItemExchangeUserData);
			foreach (KAWidget item in m_ExchangeMenu.GetItems())
			{
				KAWidget kAWidget = item.FindChildItem("BtnExchange");
				if (kAWidget != null)
				{
					kAWidget.SetDisabled(isDisabled: true);
				}
			}
		}
	}

	public void ItemDropped(KAWidget droppedWidget)
	{
		GameObject hoveredObject = UICamera.hoveredObject;
		ItemExchangeUserData itemExchangeUserData = droppedWidget.GetUserData() as ItemExchangeUserData;
		if (hoveredObject != null && hoveredObject.GetComponent<KAWidget>() == m_ExchangeArea && itemExchangeUserData != null)
		{
			if (_ExchangeFX != null)
			{
				_ExchangeFX.SetActive(value: true);
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
		if (!ExchangeAllowed(pickedWidget.GetUserData() as ItemExchangeUserData) || !(m_ExchangeMenu != null))
		{
			return;
		}
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(itemExchangeUserData._FromItem.Item.ItemID);
		if (userItemData == null)
		{
			return;
		}
		KAWidget kAWidget = CreateDragObject(pickedWidget, m_ExchangeMenu.pPanel.depth + 1);
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
		if (m_ExchangeMenu != null)
		{
			ShowTutorial(show: true);
			DisplayItems();
		}
	}

	protected override void ItemExchangeDone(ItemExchangeUserData itemExchangeUserData)
	{
		if (_ExchangeFX != null)
		{
			_ExchangeFX.SetActive(value: false);
		}
		StopFX();
		if (itemExchangeUserData == null)
		{
			return;
		}
		if (mEventManager._ItemId > 0)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(mEventManager._ItemId);
			if (userItemData != null)
			{
				mAcquiredQuantity = userItemData.Quantity;
			}
			mClaimableRewards = mAcquiredQuantity > GetNextPointValue(mPrevAcquiredQuantity);
			UpdateProgress(animate: true);
			SyncFromItemCount(itemExchangeUserData._Item);
		}
		else
		{
			if (itemExchangeUserData.GetItem() != null)
			{
				PlayFX(_CompletedFXInfo, itemExchangeUserData.GetItem().transform.position);
			}
			UpdateCurrency(mCurrencyMap[itemExchangeUserData._ToItem.Item.ItemID], itemExchangeUserData._ToItem.Item.ItemID);
			UpdateCurrency(mCurrencyMap[itemExchangeUserData._FromItem.Item.ItemID], itemExchangeUserData._FromItem.Item.ItemID);
			Reset();
		}
	}

	protected void Reset()
	{
		foreach (KAWidget item in m_ExchangeMenu.GetItems())
		{
			UpdateExchangeCount(item, 0, 1);
		}
	}

	protected override void ItemExchangeFailed(ItemExchangeUserData itemExchangeUserData)
	{
		if (_ExchangeFX != null)
		{
			_ExchangeFX.SetActive(value: false);
		}
		StopFX();
		if (itemExchangeUserData != null)
		{
			SyncFromItemCount(itemExchangeUserData._Item);
		}
	}

	private void DisplayItems()
	{
		m_ExchangeMenu.ClearItems();
		m_CurrencyMenu?.ClearItems();
		int num = -1;
		ItemExchange[] array = mItemExchange;
		foreach (ItemExchange exchangeItem in array)
		{
			if (exchangeItem.Item == null || (_CategoryId > 0 && !exchangeItem.Item.HasCategory(_CategoryId)))
			{
				continue;
			}
			if (mEventManager._ItemId > 0)
			{
				if (exchangeItem.Type != ExchangeType.From || !AvatarData.ValidGenderItem(exchangeItem.Item))
				{
					continue;
				}
				ItemExchange itemExchange = Array.Find(mItemExchange, (ItemExchange x) => x.Type == ExchangeType.To && x.ExchangeGroupID == exchangeItem.ExchangeGroupID && x.Item.ItemID == mEventManager._ItemId);
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
						CreateWidget(new ItemExchangeUserData(exchangeItem.ExchangeGroupID, exchangeItem, itemExchange), num, GetInventoryItemCount(userItemData));
					}
					else if (!mOwnedItemsOnly)
					{
						CreateWidget(new ItemExchangeUserData(exchangeItem.ExchangeGroupID, exchangeItem, itemExchange), m_ExchangeMenu.GetItemCount());
					}
				}
			}
			else if (exchangeItem.Type == ExchangeType.To)
			{
				ItemExchange itemExchange2 = Array.Find(mItemExchange, (ItemExchange x) => x.Type == ExchangeType.From && x.ExchangeGroupID == exchangeItem.ExchangeGroupID);
				if (itemExchange2 != null && itemExchange2.Item != null && itemExchange2.Quantity != 0 && AvatarData.ValidGenderItem(itemExchange2.Item))
				{
					DisplayCurrencyItem(itemExchange2);
					DisplayCurrencyItem(exchangeItem);
					ItemExchangeUserData userData = new ItemExchangeUserData(exchangeItem.ExchangeGroupID, itemExchange2, exchangeItem);
					DisplayExchangeItem(userData);
				}
			}
		}
		if (m_ExchangeMenu.GetItems().Count > 0)
		{
			m_ExchangeMenu.GetItemAt(m_ExchangeMenu.GetItems().Count - 1).FindChildItem("Divider")?.SetVisibility(inVisible: false);
		}
	}

	private void CreateWidget(ItemExchangeUserData widgetData, int insertIndex, int availableCount = 0)
	{
		KAWidget kAWidget = m_ExchangeMenu.AddWidget("ExchangeItem", (availableCount > 0) ? widgetData : null);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtItemName");
		KAWidget kAWidget3 = kAWidget.FindChildItem("TxtOutputCount");
		KAWidget kAWidget4 = kAWidget.FindChildItem("TxtAvailableCount");
		KAWidget kAWidget5 = kAWidget.FindChildItem("BkgIcon");
		kAWidget.FindChildItem("GreyMask")?.SetVisibility(availableCount == 0);
		kAWidget2?.SetText(widgetData._FromItem.Item.ItemName);
		kAWidget3?.SetText(widgetData._ToItem.Quantity.ToString());
		kAWidget4?.SetVisibility(availableCount > 0);
		kAWidget4?.SetText((availableCount > 0) ? availableCount.ToString() : "");
		kAWidget5?.SetTextureFromBundle(widgetData._FromItem.Item.IconName);
		m_ExchangeMenu.SetWidgetIndex(kAWidget, insertIndex);
	}

	private void DisplayCurrencyItem(ItemExchange itemExchange)
	{
		if (m_CurrencyMenu != null && !mCurrencyMap.ContainsKey(itemExchange.Item.ItemID))
		{
			KAWidget kAWidget = m_CurrencyMenu.AddWidget("Currency_" + itemExchange.Item.ItemID);
			mCurrencyMap[itemExchange.Item.ItemID] = kAWidget;
			kAWidget.SetTextureFromBundle(itemExchange.Item.IconName);
			SetCurrencyWidgetColors(kAWidget, itemExchange.Item);
			UpdateCurrency(kAWidget, itemExchange.Item.ItemID);
		}
	}

	private void SetCurrencyWidgetColors(KAWidget parentWidget, ItemData itemData)
	{
		string attribute = itemData.GetAttribute("BGColor", "");
		KAWidget kAWidget = parentWidget.FindChildItem("Background");
		if (kAWidget != null && kAWidget.pBackground != null && !string.IsNullOrEmpty(attribute))
		{
			kAWidget.pBackground.color = HexUtil.HexToRGB(attribute);
		}
		attribute = itemData.GetAttribute("TextColor", "");
		kAWidget = parentWidget.FindChildItem("TxtQuantityValue");
		if (kAWidget != null && kAWidget.GetLabel() != null && !string.IsNullOrEmpty(attribute))
		{
			kAWidget.GetLabel().color = HexUtil.HexToRGB(attribute);
		}
	}

	private void UpdateCurrency(KAWidget widget, int itemID)
	{
		UserItemData userItemData = null;
		if (CommonInventoryData.pIsReady)
		{
			userItemData = CommonInventoryData.pInstance.FindItem(itemID);
		}
		KAWidget kAWidget = widget.FindChildItem("TxtQuantityValue");
		if (kAWidget != null && kAWidget.GetLabel() != null)
		{
			kAWidget.SetText((userItemData == null) ? "0" : userItemData.Quantity.ToString());
		}
	}

	private void DisplayExchangeItem(ItemExchangeUserData userData)
	{
		if (m_ExchangeMenu != null)
		{
			KAWidget kAWidget = m_ExchangeMenu.AddWidget("Exchange_" + userData._FromItem.Item.ItemID, userData);
			SetExchangeWidgetColors(kAWidget, userData._ToItem.Item);
			KAWidget kAWidget2 = kAWidget.FindChildItem("TxtQuantityValue");
			if (kAWidget2 != null)
			{
				kAWidget2.pUI = this;
			}
			KAWidget kAWidget3 = kAWidget.FindChildItem("TxtExchangeName");
			if (kAWidget3 != null)
			{
				kAWidget3.SetText(userData._ToItem.Item.ItemName);
			}
			KAWidget kAWidget4 = kAWidget.FindChildItem("IconExchange");
			if (kAWidget4 != null)
			{
				kAWidget4.SetTextureFromBundle(userData._ToItem.Item.IconName);
			}
			KAWidget kAWidget5 = kAWidget.FindChildItem("IconExchangeReq");
			if (kAWidget5 != null)
			{
				kAWidget5.SetTextureFromBundle(userData._FromItem.Item.IconName);
			}
			UpdateExchangeCount(kAWidget);
		}
	}

	private void SetExchangeWidgetColors(KAWidget parentWidget, ItemData itemData)
	{
		string attribute = itemData.GetAttribute("BGColor", "");
		KAWidget kAWidget = parentWidget.FindChildItem("Background");
		if (kAWidget != null && kAWidget.pBackground != null && !string.IsNullOrEmpty(attribute))
		{
			kAWidget.pBackground.color = HexUtil.HexToRGB(attribute);
		}
		attribute = itemData.GetAttribute("FGColor", "");
		kAWidget = parentWidget.FindChildItem("Foreground");
		if (kAWidget != null && kAWidget.pBackground != null && !string.IsNullOrEmpty(attribute))
		{
			kAWidget.pBackground.color = HexUtil.HexToRGB(attribute);
		}
		attribute = itemData.GetAttribute("TextColor", "");
		kAWidget = parentWidget.FindChildItem("TxtExchangeName");
		if (kAWidget != null && kAWidget.GetLabel() != null && !string.IsNullOrEmpty(attribute))
		{
			kAWidget.GetLabel().color = HexUtil.HexToRGB(attribute);
		}
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
				m_ExchangeMenu.RemoveWidget(widget);
			}
			else
			{
				DisplayItems();
			}
			return;
		}
		if (kAWidget != null)
		{
			kAWidget.SetText(GetInventoryItemCount(userItemData).ToString());
		}
		if (!widget.GetVisibility())
		{
			widget.SetVisibility(inVisible: true);
		}
	}

	private void UpdateExchangeCount(KAWidget widget, int addition = 0, int initVal = 0)
	{
		int result = 1;
		int num = 0;
		UserItemData userItemData = null;
		ItemExchangeUserData itemExchangeUserData = widget.GetUserData() as ItemExchangeUserData;
		if (CommonInventoryData.pIsReady && itemExchangeUserData != null)
		{
			userItemData = CommonInventoryData.pInstance.FindItem(itemExchangeUserData._FromItem.Item.ItemID);
		}
		if (userItemData != null)
		{
			num = userItemData.Quantity;
		}
		KAWidget kAWidget = widget.FindChildItem("TxtQuantityValue");
		if (initVal == 0)
		{
			if (kAWidget != null && !string.IsNullOrEmpty(kAWidget.GetText()))
			{
				int.TryParse(kAWidget.GetText(), out result);
			}
		}
		else
		{
			result = initVal;
		}
		if (num >= itemExchangeUserData._FromItem.Quantity && result > num / itemExchangeUserData._FromItem.Quantity)
		{
			DisplayOKMessage(_ExchangeMaxLimitText.GetLocalizedString().Replace("[COUNT]", (num / itemExchangeUserData._FromItem.Quantity).ToString()));
			result = num / itemExchangeUserData._FromItem.Quantity;
		}
		if (result > 0 && num / (itemExchangeUserData._FromItem.Quantity * (result + addition)) > 0)
		{
			result += addition;
		}
		itemExchangeUserData._Repeat = result;
		if (kAWidget != null)
		{
			kAWidget.SetDisabled(num / itemExchangeUserData._FromItem.Quantity <= 1);
			kAWidget.SetText(result.ToString());
		}
		KAWidget kAWidget2 = widget.FindChildItem("TxtExchangeReq");
		if (kAWidget2 != null)
		{
			if (kAWidget2.GetLabel() != null)
			{
				kAWidget2.GetLabel().color = ((num / itemExchangeUserData._FromItem.Quantity == 0) ? kAWidget2.GetLabel().pOrgColorTint : _ExchangeAvailableColor);
			}
			kAWidget2.SetText(num + "/" + itemExchangeUserData._FromItem.Quantity * result);
		}
		KAWidget kAWidget3 = widget.FindChildItem("BtnPositive");
		if (kAWidget3 != null)
		{
			kAWidget3.SetDisabled(result >= num / itemExchangeUserData._FromItem.Quantity);
		}
		KAWidget kAWidget4 = widget.FindChildItem("BtnNegative");
		if (kAWidget4 != null)
		{
			kAWidget4.SetDisabled(result <= 1);
		}
		KAWidget kAWidget5 = widget.FindChildItem("BtnExchange");
		if (kAWidget5 != null)
		{
			kAWidget5.SetDisabled(num / itemExchangeUserData._FromItem.Quantity == 0);
		}
	}

	private void ShowTutorial(bool show)
	{
		if (m_TutorialUI != null)
		{
			m_TutorialUI.SetVisibility(show);
		}
	}

	private void DisplayOKMessage(string inText)
	{
		GameObject gameObject = (GameObject)RsResourceManager.LoadAssetFromResources("PfKAUIGenericDB");
		if (gameObject != null)
		{
			GameObject obj = UnityEngine.Object.Instantiate(gameObject);
			obj.name = "UiGenericDB";
			KAUIGenericDB component = obj.GetComponent<KAUIGenericDB>();
			component.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
			component._MessageObject = base.gameObject;
			component._OKMessage = "OnDBClose";
			component.SetText(inText, interactive: false);
			component.SetDestroyOnClick(isDestroy: true);
			KAUI.SetExclusive(component, new Color(0.5f, 0.5f, 0.5f, 0.5f));
		}
	}

	public void OnDBClose()
	{
		KAUI.RemoveExclusive(KAUI._GlobalExclusiveUI);
	}

	public void OpenHelp()
	{
		if (!(m_HelpScreen == null))
		{
			if (mUiHelpScreen == null)
			{
				GameObject gameObject = UnityEngine.Object.Instantiate(m_HelpScreen);
				mUiHelpScreen = gameObject.GetComponent<UiHelpScreen>();
				mUiHelpScreen.OnClicked += HelpUiButtonClicked;
			}
			SetVisibility(inVisible: false);
			mUiHelpScreen.SetVisibility(inVisible: true);
			KAUI.SetExclusive(mUiHelpScreen);
		}
	}

	private void HelpUiButtonClicked(string buttonName)
	{
		if (buttonName == "Back")
		{
			SetVisibility(inVisible: true);
			KAUI.SetExclusive(this);
		}
		else if (buttonName == "Exit")
		{
			CloseUi();
		}
	}

	protected void OnDisable()
	{
		if (mUiHelpScreen != null)
		{
			mUiHelpScreen.OnClicked -= HelpUiButtonClicked;
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
