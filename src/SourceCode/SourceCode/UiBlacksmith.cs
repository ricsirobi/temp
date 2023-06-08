using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiBlacksmith : KAUI
{
	public int _ShardItemId = 13711;

	public int _ShardStoreId = 91;

	public static Action OnClosed;

	public Mode _DefaultMode;

	public List<ModeData> _Data;

	public float _DelayToShowItemCustomization = 0.3f;

	private UiBattleBackpack mUiBattleBackpack;

	private ModeData mCurrentModeData;

	private KAWidget mBtnMarket;

	private KAWidget mBtnEnhance;

	private KAWidget mBtnFuse;

	private KAWidget mGemsTotal;

	private KAWidget mCoinsTotal;

	private KAWidget mShardsTotal;

	private KAWidget mTxtTitle;

	private AvAvatarState mLastAvatarState;

	private static UiBlacksmith mInstance;

	public UserItemData ShardItemData { get; set; }

	public static void Init(Mode mode)
	{
		AvAvatar.SetUIActive(inActive: false);
		KAUICursorManager.SetDefaultCursor("Loading");
		RsResourceManager.LoadAssetFromBundle(GameConfig.GetKeyData("BlacksmithAsset"), OnBlacksmithLoaded, typeof(GameObject), inDontDestroy: false, mode);
	}

	private static void OnBlacksmithLoaded(string inURL, RsResourceLoadEvent inLoadEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inLoadEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
		{
			KAUICursorManager.SetDefaultCursor("Arrow");
			GameObject obj = UnityEngine.Object.Instantiate((GameObject)inObject);
			obj.name = ((GameObject)inObject).name;
			UiBlacksmith component = obj.GetComponent<UiBlacksmith>();
			if (component != null && inUserData != null)
			{
				component._DefaultMode = (Mode)inUserData;
			}
			break;
		}
		case RsResourceLoadEvent.ERROR:
			AvAvatar.SetUIActive(inActive: true);
			Debug.LogError("Error loading BlacksmithDO!" + inURL);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}

	protected override void Start()
	{
		if (mInstance == null)
		{
			mInstance = this;
			base.Start();
			mUiBattleBackpack = (UiBattleBackpack)_UiList[0];
			mBtnMarket = FindItem("BtnMarket");
			mBtnEnhance = FindItem("BtnEnhance");
			mBtnFuse = FindItem("BtnFuse");
			mGemsTotal = FindItem("GemsTotal");
			mCoinsTotal = FindItem("CoinsTotal");
			mTxtTitle = FindItem("TxtTitle");
			mShardsTotal = FindItem("ShardsTotal");
			SetMode(_DefaultMode);
			if (mUiBattleBackpack != null)
			{
				mUiBattleBackpack.pMessageObject = base.gameObject;
			}
			Money.AddNotificationObject(base.gameObject);
			KAUI.SetExclusive(this);
			if (AvAvatar.pState != AvAvatarState.PAUSED && AvAvatar.pState != 0)
			{
				mLastAvatarState = AvAvatarState.PAUSED;
				AvAvatar.pState = AvAvatarState.PAUSED;
				if (AvAvatar.pToolbar != null)
				{
					AvAvatar.SetUIActive(inActive: false);
				}
			}
			UpdateShards();
		}
		else
		{
			UnityEngine.Object.Destroy(base.gameObject);
		}
	}

	public void UpdateShards()
	{
		ShardItemData = CommonInventoryData.pInstance.FindItem(_ShardItemId);
		if (mShardsTotal != null)
		{
			mShardsTotal.SetText((ShardItemData != null) ? ShardItemData.Quantity.ToString() : "0");
		}
	}

	public void SetMode(Mode mode)
	{
		if (mUiBattleBackpack != null)
		{
			mUiBattleBackpack.pKAUiSelectMenu._AllowDrag = mode == Mode.ENHANCE;
			mUiBattleBackpack.pKAUiSelectMenu._AllowItemDrag = mode != Mode.ENHANCE;
		}
		switch (mode)
		{
		case Mode.ENHANCE:
			if (mBtnMarket.transform.localPosition != mBtnMarket.pOrgPosition)
			{
				mBtnMarket.ResetWidget();
			}
			mBtnFuse.SetVisibility(inVisible: true);
			mBtnEnhance.SetVisibility(inVisible: false);
			mBtnMarket.SetVisibility(inVisible: true);
			break;
		case Mode.MARKET:
			if (mBtnMarket.transform.localPosition != mBtnMarket.pOrgPosition)
			{
				mBtnMarket.ResetWidget();
			}
			mBtnFuse.SetVisibility(inVisible: true);
			mBtnEnhance.SetVisibility(inVisible: true);
			mBtnMarket.SetVisibility(inVisible: false);
			break;
		case Mode.FUSE:
			mBtnMarket.SetPosition(mBtnFuse.pOrgPosition.x, mBtnFuse.pOrgPosition.y);
			mBtnFuse.SetVisibility(inVisible: false);
			mBtnMarket.SetVisibility(inVisible: true);
			mBtnEnhance.SetVisibility(inVisible: true);
			break;
		}
		if (_Data.Count == 0)
		{
			return;
		}
		if (mCurrentModeData != null && mCurrentModeData._DisplayUI != null)
		{
			if (mUiBattleBackpack != null)
			{
				mUiBattleBackpack.Initialize();
			}
			mCurrentModeData._DisplayUI.SetVisibility(inVisible: false);
			OnBluePrintSelected(null);
		}
		ModeData modeData = _Data.Find((ModeData m) => m._Mode == mode);
		if (modeData != null)
		{
			if (modeData._DisplayUI != null)
			{
				modeData._DisplayUI.SetVisibility(inVisible: true);
			}
			if (mUiBattleBackpack != null)
			{
				mUiBattleBackpack.SetMenuHeight(modeData._BattleBackpackMenuHeight);
				((UiBattleBackpackMenu)mUiBattleBackpack.pKAUiSelectMenu).pTargetMenu = modeData._TargetMenu;
			}
			mCurrentModeData = modeData;
			mTxtTitle.SetText(modeData._DisplayText.GetLocalizedString());
		}
		SelectDefaultItem();
	}

	public void OnBluePrintSelected(BluePrint data)
	{
		((UiBattleBackpackMenu)mUiBattleBackpack.pKAUiSelectMenu).BlueprintFuseMap = data;
		mUiBattleBackpack.Initialize();
	}

	public void SelectItem(int userinventoryID)
	{
		if (!(mUiBattleBackpack != null))
		{
			return;
		}
		UiBattleBackpackMenu uiBattleBackpackMenu = (UiBattleBackpackMenu)mUiBattleBackpack.pKAUiSelectMenu;
		foreach (KAWidget item in uiBattleBackpackMenu.GetItems())
		{
			if (((KAUISelectItemData)item.GetUserData())._UserInventoryID == userinventoryID)
			{
				mUiBattleBackpack.SelectItem(item);
				uiBattleBackpackMenu.SetSelectedItem(item);
				uiBattleBackpackMenu.SetTopItemIdx(uiBattleBackpackMenu.FindItemIndex(item));
				break;
			}
		}
	}

	public void CheckItemCustomization(UserItemData[] itemData)
	{
		if (itemData == null || itemData.Length == 0)
		{
			return;
		}
		List<UserItemData> list = new List<UserItemData>();
		foreach (UserItemData userItemData in itemData)
		{
			if (userItemData.Item.HasCategory(657))
			{
				list.Add(userItemData);
			}
		}
		if (list.Count > 0)
		{
			SetState(KAUIState.NOT_INTERACTIVE);
			StartCoroutine("ShowItemCustomization", list.ToArray());
		}
		else
		{
			OnCloseItemCustomization(null);
		}
	}

	private IEnumerator ShowItemCustomization(UserItemData[] itemData)
	{
		yield return new WaitForSeconds(_DelayToShowItemCustomization);
		HideBlacksmithUI(hide: true);
		UiAvatarItemCustomization.Init(itemData, null, OnCloseItemCustomization, multiItemCustomizationUI: false);
	}

	public void OnCloseItemCustomization(KAUISelectItemData inItem)
	{
		HideBlacksmithUI(hide: false);
		SetState(KAUIState.INTERACTIVE);
	}

	public void HideBlacksmithUI(bool hide)
	{
		SetVisibility(!hide);
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _BackButtonName)
		{
			CloseUi();
		}
		if (inWidget == mBtnEnhance)
		{
			SetMode(Mode.ENHANCE);
		}
		else if (inWidget == mBtnMarket)
		{
			SetMode(Mode.MARKET);
		}
		else if (inWidget == mBtnFuse)
		{
			SetMode(Mode.FUSE);
		}
	}

	private void SelectDefaultItem()
	{
		if (!(mUiBattleBackpack != null))
		{
			return;
		}
		UiBattleBackpackMenu uiBattleBackpackMenu = (UiBattleBackpackMenu)mUiBattleBackpack.pKAUiSelectMenu;
		if (uiBattleBackpackMenu != null && uiBattleBackpackMenu.GetItemAt(0) != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)uiBattleBackpackMenu.GetItemAt(0).GetUserData();
			if (kAUISelectItemData != null && kAUISelectItemData._ItemID > 0)
			{
				mUiBattleBackpack.SelectItem(kAUISelectItemData._Item);
				uiBattleBackpackMenu.SetSelectedItem(kAUISelectItemData._Item);
			}
		}
	}

	private void OnSelectBattleBackpackItem(KAWidget widget)
	{
		if (mCurrentModeData != null && mCurrentModeData._DisplayUI != null)
		{
			mCurrentModeData._DisplayUI.OnClick(widget);
		}
	}

	private void OnMoneyUpdated()
	{
		if (Money.pIsReady)
		{
			mGemsTotal.SetText(Money.pCashCurrency.ToString());
			mCoinsTotal.SetText(Money.pGameCurrency.ToString());
		}
	}

	private void CloseUi()
	{
		if (mLastAvatarState == AvAvatarState.PAUSED)
		{
			AvAvatar.pState = AvAvatar.pPrevState;
			if (AvAvatar.pToolbar != null)
			{
				AvAvatar.SetUIActive(inActive: true);
			}
		}
		Money.RemoveNotificationObject(base.gameObject);
		KAUI.RemoveExclusive(this);
		if (OnClosed != null)
		{
			OnClosed();
			OnClosed = null;
		}
		UnityEngine.Object.Destroy(base.gameObject);
	}
}
