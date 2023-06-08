using System.Collections.Generic;
using UnityEngine;

public class UiCogsInventoryMenu : KAUIMenu
{
	private bool mDragged;

	private int mCogItemBundleCount = -1;

	public bool pIsReady => mCogItemBundleCount == 0;

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		if (_ParentUi != null && !mDragged && KAInput.GetMouseButton(0))
		{
			mDragged = true;
			((UiCogs)_ParentUi).MenuItemDragged(this, inWidget, inDelta);
		}
	}

	public void SetupInventory(CogsLevelDetails currLevel)
	{
		ClearMenu();
		mCogItemBundleCount = currLevel.InventoryItems.Length;
		CogsInventoryItemData[] inventoryItems = currLevel.InventoryItems;
		foreach (CogsInventoryItemData cogsInventoryItemData in inventoryItems)
		{
			AddItem(cogsInventoryItemData.InventoryCog, cogsInventoryItemData.Quantity);
		}
	}

	public void AddItem(Cog inCog, int inQuantity = 1)
	{
		KAWidget kAWidget = null;
		CogsWidgetData cogsWidgetData = null;
		List<KAWidget> items = GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			cogsWidgetData = (CogsWidgetData)items[i].GetUserData();
			if (cogsWidgetData._Asset.Equals(inCog.Asset))
			{
				kAWidget = items[i];
				break;
			}
		}
		if (kAWidget == null)
		{
			KAWidget kAWidget2 = AddWidget("TemplateTool");
			CogsWidgetData cogsWidgetData2 = new CogsWidgetData(inCog)
			{
				_Asset = inCog.Asset,
				_Quantity = inQuantity
			};
			kAWidget2.SetTextureFromBundle(inCog.Icon);
			kAWidget2.SetUserData(cogsWidgetData2);
			kAWidget2.name = inCog.AssetName;
			kAWidget2.FindChildItem("Quantity").SetText(cogsWidgetData2._Quantity.ToString());
			kAWidget2.SetState(KAUIState.INTERACTIVE);
			cogsWidgetData2.LoadResource(OnCogItemBundleReady);
		}
		else
		{
			cogsWidgetData._Quantity += inQuantity;
			kAWidget.FindChildItem("Quantity").SetText(cogsWidgetData._Quantity.ToString());
			OnCogItemBundleReady();
		}
	}

	public void RemoveItem(Cog inCog, int inQuantity = 1)
	{
		List<KAWidget> items = GetItems();
		for (int i = 0; i < items.Count; i++)
		{
			CogsWidgetData cogsWidgetData = (CogsWidgetData)items[i].GetUserData();
			if (cogsWidgetData._Asset.Equals(inCog.Asset))
			{
				cogsWidgetData._Quantity -= inQuantity;
				if (cogsWidgetData._Quantity > 0)
				{
					items[i].FindChildItem("Quantity").SetText(cogsWidgetData._Quantity.ToString());
				}
				else
				{
					RemoveWidget(items[i]);
				}
				break;
			}
		}
	}

	public override void ClearItems()
	{
		base.ClearItems();
		CogsWidgetData.UnloadAll();
	}

	public void OnCogItemBundleReady()
	{
		if (mCogItemBundleCount > 0)
		{
			mCogItemBundleCount--;
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mDragged && KAInput.GetMouseButtonUp(0))
		{
			mDragged = false;
		}
	}
}
