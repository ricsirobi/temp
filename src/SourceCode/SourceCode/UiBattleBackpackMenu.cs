using System.Collections;
using UnityEngine;

public class UiBattleBackpackMenu : KAUISelectMenu
{
	public LocaleString _FuseFailText;

	public LocaleString _DismantleFailText;

	private bool mWidgetClicked;

	public string _ItemColorWidget = "CellBackground";

	private Color mItemDefaultColor = Color.white;

	public KAUIMenu pTargetMenu { get; set; }

	public Color ItemDefaultColor => mItemDefaultColor;

	public BluePrint BlueprintFuseMap { get; set; }

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
	}

	public override void OnDoubleClick(KAWidget inWidget)
	{
		if (pTargetMenu == null || !pTargetMenu.GetVisibility())
		{
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (!IsTaskComplete(kAUISelectItemData))
		{
			ShowTaskItemPopUp();
			return;
		}
		if (kAUISelectItemData._Disabled)
		{
			ShowMessage();
		}
		mWidgetClicked = false;
		StopCoroutine("DelayedClick");
		AddToTargetMenu(inWidget, pTargetMenu);
		base.OnDoubleClick(inWidget);
		SelectItem(null);
	}

	private void ShowMessage()
	{
		if (BlueprintFuseMap != null)
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _FuseFailText.GetLocalizedString(), "", null, "", "", "Ok", "", inDestroyOnClick: true);
		}
		else
		{
			GameUtilities.DisplayGenericDB("PfKAUIGenericDB", _DismantleFailText.GetLocalizedString(), "", null, "", "", "Ok", "", inDestroyOnClick: true);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!mWidgetClicked)
		{
			if (pTargetMenu != null)
			{
				pTargetMenu.SetSelectedItem(null);
			}
			StartCoroutine("DelayedClick", inWidget);
		}
	}

	public override void OnDragStart(KAWidget inWidget)
	{
		KAUISelectItemData data = (KAUISelectItemData)inWidget.GetUserData();
		if (!IsTaskComplete(data))
		{
			ShowTaskItemPopUp();
		}
		else
		{
			base.OnDragStart(inWidget);
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		if (!(pTargetMenu == null) && pTargetMenu.GetVisibility())
		{
			if (((KAUISelectItemData)inWidget.GetUserData())._Disabled)
			{
				ShowMessage();
			}
			base.OnDrag(inWidget, inDelta);
			SelectItem(null);
		}
	}

	private IEnumerator DelayedClick(KAWidget inWidget)
	{
		mWidgetClicked = true;
		float waitTime = 0.4f;
		float deltaTime = 0f;
		while (deltaTime < waitTime && UICamera.hoveredObject == inWidget.gameObject)
		{
			deltaTime += Time.deltaTime;
			yield return null;
		}
		if (mWidgetClicked)
		{
			base.OnClick(inWidget);
			mWidgetClicked = false;
		}
	}

	public override void UpdateWidget(KAUISelectItemData widgetData)
	{
		base.UpdateWidget(widgetData);
		if (widgetData == null)
		{
			return;
		}
		KAWidget kAWidget = widgetData.GetItem().FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(widgetData._IsBattleReady);
		}
		KAWidget kAWidget2 = widgetData.GetItem().FindChildItem("FlightReadyIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(widgetData._ItemData != null && widgetData._ItemData.HasAttribute("FlightSuit"));
		}
		KAWidget kAWidget3 = widgetData.GetItem().FindChildItem(_ItemColorWidget);
		if (kAWidget3 != null)
		{
			if (widgetData._IsBattleReady)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((widgetData._ItemData == null || !widgetData._ItemData.ItemRarity.HasValue) ? ItemRarity.Common : widgetData._ItemData.ItemRarity.Value, kAWidget3);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget3);
			}
		}
	}

	public override void AddInvMenuItem(UserItemData userItem, int quantity = 1)
	{
		if (BlueprintFuseMap != null && BlueprintFuseMap.Ingredients != null)
		{
			foreach (BluePrintSpecification ingredient in BlueprintFuseMap.Ingredients)
			{
				if (ingredient.ItemID.HasValue)
				{
					if (ingredient.ItemID == userItem.Item.ItemID)
					{
						base.AddInvMenuItem(userItem, ingredient.Quantity);
						break;
					}
				}
				else if ((!ingredient.Tier.HasValue || userItem.ItemTier == ingredient.Tier) && (!ingredient.CategoryID.HasValue || userItem.Item.HasCategory(ingredient.CategoryID.Value)) && (!ingredient.ItemRarity.HasValue || userItem.Item.ItemRarity == ingredient.ItemRarity))
				{
					base.AddInvMenuItem(userItem, ingredient.Quantity);
					break;
				}
			}
			return;
		}
		base.AddInvMenuItem(userItem, quantity);
	}

	public override void OnDragEnd(KAWidget sourceWidget)
	{
		if (BlueprintFuseMap == null)
		{
			base.OnDragEnd(sourceWidget);
			return;
		}
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)sourceWidget.GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._Disabled)
		{
			return;
		}
		GameObject hoveredObject = UICamera.hoveredObject;
		if (hoveredObject != null)
		{
			KAWidget component = hoveredObject.GetComponent<KAWidget>();
			if (component != null)
			{
				KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)component.GetUserData();
				if (kAUISelectItemData2._Menu._ParentUi.GetType() == typeof(UiFuseInfo))
				{
					switch (((UiFuseInfo)kAUISelectItemData2._Menu._ParentUi).ValidateDrop(component, (KAUISelectItemData)KAUIManager.pInstance.pDragItem.GetUserData()))
					{
					case UiFuse.FuseInteraction.ValidItem:
						ProcessDrop(component);
						MoveItemToBottom(sourceWidget);
						((UiFuseInfo)kAUISelectItemData2._Menu._ParentUi).IngredientAdded(component);
						return;
					case UiFuse.FuseInteraction.InvalidItem:
						((UiFuseInfo)kAUISelectItemData2._Menu._ParentUi).ShowIncorrectItemDB();
						break;
					}
				}
			}
		}
		MoveCursorItemBack(sourceWidget);
	}

	public override bool AddToTargetMenu(KAUISelectItemData widgetData, KAUIMenu targetMenu, bool inventoryCheck = true)
	{
		if (BlueprintFuseMap != null)
		{
			if (targetMenu._ParentUi == null)
			{
				return false;
			}
			if (widgetData != null && widgetData._ItemData != null && widgetData._ItemData.ItemID != 0)
			{
				foreach (KAWidget item in targetMenu.GetItems())
				{
					KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
					if (kAUISelectItemData._Menu._ParentUi.GetType() == typeof(UiFuseInfo) && ((UiFuseInfo)kAUISelectItemData._Menu._ParentUi).ValidateDrop(item, widgetData) == UiFuse.FuseInteraction.ValidItem)
					{
						widgetData._Menu = kAUISelectItemData._Menu;
						if (widgetData._Item == null)
						{
							widgetData._Item = kAUISelectItemData._Item;
						}
						((KAUISelect)targetMenu._ParentUi).AddWidgetData(item, widgetData);
						((UiFuseInfo)kAUISelectItemData._Menu._ParentUi).IngredientAdded(item);
						targetMenu.SetSelectedItem(item);
						((KAUISelect)targetMenu._ParentUi).SelectItem(item);
						return true;
					}
				}
			}
			return false;
		}
		return base.AddToTargetMenu(widgetData, targetMenu);
	}

	public override void SetTopItemIdx(int idx)
	{
		SetDefaultFocusIndex(idx);
		base.SetTopItemIdx(idx);
	}
}
