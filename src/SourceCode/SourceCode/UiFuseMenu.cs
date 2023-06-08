using UnityEngine;

public class UiFuseMenu : KAUISelectMenu
{
	public KAUIMenu _TargetMenu;

	public string _ItemColorWidget = "CellBackground";

	private Color mItemDefaultColor = Color.white;

	public Color ItemDefaultColor => mItemDefaultColor;

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
	}

	public override void OnDoubleClick(KAWidget inWidget)
	{
		base.OnDoubleClick(inWidget);
		if (!(_TargetMenu == null))
		{
			AddToTargetMenu(inWidget, _TargetMenu);
			ActivateIngredientIcon(inWidget);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_TargetMenu != null)
		{
			_TargetMenu.SetSelectedItem(null);
		}
	}

	public override void SetSelectedItem(KAWidget inWidget)
	{
		if (mSelectedItem != null)
		{
			mSelectedItem.OnSelected(inSelected: false);
		}
		mSelectedItem = inWidget;
		if (mSelectedItem != null)
		{
			mSelectedItem.OnSelected(inSelected: true);
		}
		if (inWidget != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
			KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)inWidget.pChildWidgets[0].GetUserData();
			if ((kAUISelectItemData != null && kAUISelectItemData._ItemID != 0) || (kAUISelectItemData2 != null && kAUISelectItemData2._ItemID != 0))
			{
				UpdateHighlightState(inWidget, _SelectedWidget, inWidget != null);
			}
		}
		else
		{
			UpdateHighlightState(inWidget, _SelectedWidget, inWidget != null);
		}
		if (_ParentUi != null)
		{
			_ParentUi.SetSelectedItem(this, inWidget);
		}
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		if (!UtPlatform.IsMobile() && inWidget != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
			KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)inWidget.pChildWidgets[0].GetUserData();
			if ((kAUISelectItemData != null && kAUISelectItemData._ItemID != 0) || (kAUISelectItemData2 != null && kAUISelectItemData2._ItemID != 0))
			{
				UpdateHighlightState(inWidget, _HighlightWidget, inIsHover);
			}
		}
	}

	public override void OnDragEnd(KAWidget sourceWidget)
	{
		if (!_AllowItemDrag)
		{
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
			KAUISelectMenu kAUISelectMenu = null;
			KAWidget component = hoveredObject.GetComponent<KAWidget>();
			if (component != null)
			{
				KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)component.GetUserData();
				if (kAUISelectItemData2 != null)
				{
					kAUISelectMenu = kAUISelectItemData2._Menu;
				}
			}
			else
			{
				KAUISelect component2 = hoveredObject.GetComponent<KAUISelect>();
				if (component2 != null)
				{
					kAUISelectMenu = component2.pKAUiSelectMenu;
				}
			}
			if (kAUISelectMenu != null && kAUISelectMenu != this)
			{
				foreach (KAWidget item in kAUISelectMenu.GetItems())
				{
					KAUISelectItemData kAUISelectItemData3 = (KAUISelectItemData)item.GetUserData();
					if (kAUISelectItemData3 != null && kAUISelectItemData3._ItemID == 0 && !kAUISelectItemData3._SlotLocked)
					{
						ProcessDrop(item);
						ActivateIngredientIcon(sourceWidget);
						return;
					}
				}
				if (kAUISelectMenu.pKAUISelect != null)
				{
					kAUISelectMenu.pKAUISelect.CheckInventoryFull();
				}
			}
		}
		MoveCursorItemBack(sourceWidget);
	}

	private void ActivateIngredientIcon(KAWidget widget)
	{
		widget.SetInteractive(isInteractive: true);
		KAWidget kAWidget = widget.pChildWidgets[0];
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(inVisible: true);
		}
		KAWidget kAWidget2 = widget.FindChildItem("BattleReadyIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(inVisible: false);
		}
		KAWidget kAWidget3 = widget.FindChildItem("FlightReadyIcon");
		if (kAWidget3 != null)
		{
			kAWidget3.SetVisibility(inVisible: false);
		}
		KAWidget kAWidget4 = widget.FindChildItem(_ItemColorWidget);
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)kAWidget.GetUserData();
		if (kAWidget4 != null && kAUISelectItemData._ItemData.ItemRarity.HasValue)
		{
			UiItemRarityColorSet.SetItemBackgroundColor(kAUISelectItemData._ItemData.ItemRarity.Value, kAWidget4);
		}
		else
		{
			UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget4);
		}
	}

	protected override void AddToTargetMenu(KAWidget inWidget, KAUIMenu targetMenu)
	{
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		if (!kAUISelectItemData._Disabled && AddToTargetMenu(kAUISelectItemData, targetMenu, inventoryCheck: false))
		{
			base.pKAUISelect.AddWidgetData(inWidget, null);
		}
	}
}
