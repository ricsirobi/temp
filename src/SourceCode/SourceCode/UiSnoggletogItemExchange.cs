using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiSnoggletogItemExchange : UiItemExchange
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

	public string _SnoggletogJobBoardAssetPath;

	public GameObject _HelpScreen;

	public int _CategoryId;

	public int _MissionGroupID = 57;

	public Color _ExchangeAvailableColor = Color.green;

	public EffectsData _ProcessingFXInfo;

	public EffectsData _CompletedFXInfo;

	public LocaleString _ExchangeMaxLimitText = new LocaleString("You can exchange Max [COUNT]");

	private KAUIMenu mExchangeMenu;

	private KAUIMenu mCurrencyMenu;

	private UiHelpScreen mUiHelpScreen;

	private Dictionary<int, KAWidget> mCurrencyMap;

	private EffectsData mEffectsData;

	private Coroutine mEffectsCouroutine;

	private KAWidget mTxtTimer;

	public static void Load()
	{
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("SnoggletogItemExchangeAsset"), OnBundleLoaded, typeof(GameObject));
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
			Debug.LogError("Error loading Snoggletog Item Exchange Prefab!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	private void LoadJobBoard()
	{
		if (!string.IsNullOrEmpty(_SnoggletogJobBoardAssetPath))
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _SnoggletogJobBoardAssetPath.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], LoadObjectEvent, typeof(GameObject));
		}
	}

	private void LoadObjectEvent(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			CloseUi();
			KAUICursorManager.SetDefaultCursor("Arrow");
			UnityEngine.Object.Instantiate((GameObject)inObject);
			break;
		case RsResourceLoadEvent.ERROR:
			KAUICursorManager.SetDefaultCursor("Arrow");
			UtDebug.LogError("Failed to load" + _SnoggletogJobBoardAssetPath);
			break;
		}
	}

	private void PlayFX(EffectsData data, Vector3 position)
	{
		StopFX();
		if (data != null)
		{
			mEffectsData = data;
			mEffectsCouroutine = StartCoroutine(ProcessFX(position));
		}
	}

	private void StopFX()
	{
		if (mEffectsCouroutine != null)
		{
			StopCoroutine(mEffectsCouroutine);
			mEffectsCouroutine = null;
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

	protected override void Start()
	{
		if (SnoggletogManager.pInstance == null || !SnoggletogManager.pInstance.IsEventInProgress())
		{
			GameUtilities.DisplayOKMessage("PfKAUIGenericDB", _ExchangeItemUnavailableText.GetLocalizedString(), null, "");
			CloseUi();
			return;
		}
		base.Start();
		mCurrencyMap = new Dictionary<int, KAWidget>();
		mExchangeMenu = _MenuList[0];
		mCurrencyMenu = _MenuList[1];
		mTxtTimer = FindItem("TxtTimer");
		if (mExchangeMenu != null)
		{
			mExchangeMenu.pEvents.OnClick += OnMenuItemClick;
		}
		GetExchangeItems();
		DisplayEndDate();
	}

	public override void OnClick(KAWidget widget)
	{
		base.OnClick(widget);
		if (widget.name == "BtnClaimRewards")
		{
			LoadJobBoard();
		}
		else if (widget.name == "BtnHowToPlay")
		{
			OpenHelp();
		}
	}

	private void DisplayEndDate()
	{
		if (mTxtTimer != null && mTxtTimer.GetLabel() != null)
		{
			int num = ((SnoggletogManager.pInstance != null) ? SnoggletogManager.pInstance.GetRemainingDays() : 0);
			mTxtTimer.SetText(mTxtTimer.GetLabel().text.Replace("{#}", num.ToString()));
		}
	}

	private void OpenHelp()
	{
		if (mUiHelpScreen == null && _HelpScreen != null)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_HelpScreen);
			mUiHelpScreen = gameObject.GetComponent<UiHelpScreen>();
			if (mUiHelpScreen != null)
			{
				mUiHelpScreen.OnClicked += HelpUiButtonClicked;
			}
		}
		if (!(mUiHelpScreen == null))
		{
			SetVisibility(inVisible: false);
			mUiHelpScreen.SetVisibility(inVisible: true);
			KAUI.SetExclusive(mUiHelpScreen);
		}
	}

	private void HelpUiButtonClicked(string buttonName)
	{
		if (!(buttonName == "Back"))
		{
			if (buttonName == "Exit")
			{
				CloseUi();
			}
		}
		else
		{
			DestroyHelpScreen();
			SetVisibility(inVisible: true);
			KAUI.SetExclusive(this);
		}
	}

	protected override void ExchangeItemsLoaded()
	{
		if (mExchangeMenu != null)
		{
			DisplayItems();
		}
	}

	protected override void ItemExchangeDone(ItemExchangeUserData itemExchangeUserData)
	{
		StopFX();
		if (itemExchangeUserData != null)
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
		foreach (KAWidget item in mExchangeMenu.GetItems())
		{
			UpdateExchangeCount(item, 0, 1);
		}
	}

	protected override void ItemExchangeFailed(ItemExchangeUserData userData)
	{
		StopFX();
	}

	private void DisplayItems()
	{
		mExchangeMenu.ClearItems();
		mCurrencyMenu.ClearItems();
		ItemExchange[] array = mItemExchange;
		foreach (ItemExchange exchangeItem in array)
		{
			if (exchangeItem.Type == ExchangeType.To && exchangeItem.Item != null && (_CategoryId <= 0 || exchangeItem.Item.HasCategory(_CategoryId)))
			{
				ItemExchange itemExchange = Array.Find(mItemExchange, (ItemExchange x) => x.Type == ExchangeType.From && x.ExchangeGroupID == exchangeItem.ExchangeGroupID);
				if (itemExchange != null && itemExchange.Item != null && itemExchange.Quantity != 0 && AvatarData.ValidGenderItem(itemExchange.Item))
				{
					DisplayCurrencyItem(itemExchange);
					DisplayCurrencyItem(exchangeItem);
					ItemExchangeUserData userData = new ItemExchangeUserData(exchangeItem.ExchangeGroupID, itemExchange, exchangeItem);
					DisplayExchangeItem(userData);
				}
			}
		}
		if (mExchangeMenu.GetItems().Count > 0)
		{
			KAWidget kAWidget = mExchangeMenu.GetItemAt(mExchangeMenu.GetItems().Count - 1).FindChildItem("Divider");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}

	private void DisplayCurrencyItem(ItemExchange itemExchange)
	{
		if (!(mCurrencyMenu == null) && !mCurrencyMap.ContainsKey(itemExchange.Item.ItemID))
		{
			KAWidget kAWidget = mCurrencyMenu.AddWidget("Currency_" + itemExchange.Item.ItemID);
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
		kAWidget = parentWidget.FindChildItem("TxtExchangeName");
		attribute = itemData.GetAttribute("TextColor", "");
		kAWidget = parentWidget.FindChildItem("TxtQuantityValue");
		if (kAWidget != null && kAWidget.GetLabel() != null && !string.IsNullOrEmpty(attribute))
		{
			kAWidget.GetLabel().color = HexUtil.HexToRGB(attribute);
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
		if (mExchangeMenu != null)
		{
			KAWidget kAWidget = mExchangeMenu.AddWidget("Exchange_" + userData._FromItem.Item.ItemID, userData);
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
		if (widget.pParentWidget == null)
		{
			return;
		}
		switch (widget.name)
		{
		case "BtnPositive":
			UpdateExchangeCount(widget.pParentWidget, 1);
			break;
		case "BtnNegative":
			UpdateExchangeCount(widget.pParentWidget, -1);
			break;
		case "BtnExchange":
			if (!(widget.pParentWidget != null) || widget.pParentWidget.GetUserData() == null)
			{
				break;
			}
			PlayFX(_ProcessingFXInfo, widget.pParentWidget.transform.position);
			ProcessItemExchange(widget.pParentWidget.GetUserData() as ItemExchangeUserData);
			{
				foreach (KAWidget item in mExchangeMenu.GetItems())
				{
					KAWidget kAWidget = item.FindChildItem("BtnExchange");
					if (kAWidget != null)
					{
						kAWidget.SetDisabled(isDisabled: true);
					}
				}
				break;
			}
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

	private void DestroyHelpScreen()
	{
		if (mUiHelpScreen != null)
		{
			mUiHelpScreen.OnClicked -= HelpUiButtonClicked;
			UnityEngine.Object.Destroy(mUiHelpScreen.gameObject);
		}
	}

	protected override void CloseUi(bool invokeOnClosed = true)
	{
		DestroyHelpScreen();
		base.CloseUi();
	}
}
