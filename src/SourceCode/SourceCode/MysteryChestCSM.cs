using System;
using UnityEngine;

public class MysteryChestCSM : ObContextSensitive
{
	public ContextSensitiveState[] _Menus;

	public string _PurchaseCSItemName = "Purchase";

	[NonSerialized]
	public MysteryChest _MysteryChest;

	protected override void UpdateData(ref ContextSensitiveState[] inStatesArrData)
	{
		inStatesArrData = _Menus;
	}

	protected override void OnMenuActive(ContextSensitiveStateType inMenuType)
	{
		if (_MysteryChest == null)
		{
			return;
		}
		if (_MysteryChest.pItemData.GetFinalCost() == 0)
		{
			_MysteryChest.Purchase();
			return;
		}
		base.OnMenuActive(inMenuType);
		KAWidget kAWidget = base.pUI.FindItem(_PurchaseCSItemName);
		ItemPreviewScroller componentInChildren = kAWidget.gameObject.GetComponentInChildren<ItemPreviewScroller>();
		if (componentInChildren != null)
		{
			componentInChildren.Init(_MysteryChest.GetContents());
		}
		KAWidget kAWidget2 = kAWidget.FindChildItem("PurchaseButton");
		kAWidget2.SetUserData(kAWidget.GetUserData());
		KAWidget kAWidget3 = kAWidget2.FindChildItem("TxtCost");
		if (_MysteryChest.pMysteryBoxType == ChestType.AdRewardMysteryChest)
		{
			kAWidget3.SetVisibility(inVisible: false);
			kAWidget2.FindChildItem("AdButton").SetVisibility(inVisible: true);
		}
		else
		{
			kAWidget3.SetText(_MysteryChest.pItemData.GetFinalCost().ToString());
			if (_MysteryChest.pItemData.GetPurchaseType() == 2)
			{
				kAWidget2.FindChildItem("CurrencyGems").SetVisibility(inVisible: true);
			}
			else
			{
				kAWidget2.FindChildItem("CurrencyCoins").SetVisibility(inVisible: true);
			}
		}
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			component.OnTakeDamage = (AvAvatarController.OnTakeDamageDelegate)Delegate.Combine(component.OnTakeDamage, new AvAvatarController.OnTakeDamageDelegate(OnAvatarTakeDamage));
		}
	}

	protected override void OnActivate()
	{
		if (!(_MysteryChest == null) && _MysteryChest.pCurrentState != 0)
		{
			base.OnActivate();
			AnalyticMysteryChestEvent.LogViewedEvent(RsResourceManager.pCurrentLevel, _MysteryChest.gameObject.name, _MysteryChest.pItemData.ItemID);
		}
	}

	public virtual void OnContextAction(string inName)
	{
		if (!(inName == _PurchaseCSItemName))
		{
			return;
		}
		if (_MysteryChest != null)
		{
			if (_MysteryChest.pMysteryBoxType == ChestType.MysteryChest)
			{
				_MysteryChest.Purchase();
			}
			else
			{
				_MysteryChest.ShowMysteryChestAd();
			}
		}
		CloseMenu();
	}

	protected override void OnProximityExit()
	{
		SetProximityAlreadyEntered(isEntered: false);
		DestroyMenu(checkProximity: false);
	}

	private void OnAvatarTakeDamage(GameObject go, ref float damage)
	{
		CloseMenu();
	}

	protected override void DestroyMenu(bool checkProximity)
	{
		base.DestroyMenu(checkProximity);
		if (AvAvatar.pObject != null)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.OnTakeDamage = (AvAvatarController.OnTakeDamageDelegate)Delegate.Remove(component.OnTakeDamage, new AvAvatarController.OnTakeDamageDelegate(OnAvatarTakeDamage));
			}
		}
	}
}
