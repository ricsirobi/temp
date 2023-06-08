using UnityEngine;

public class UiBattleBackpack : KAUISelect
{
	private KAWidget mBackground;

	private Vector4 mOriginalClipRangeRef;

	private int mOriginalBackgroundHeight;

	private float mOriginalDownArrowOffset;

	private float mBackgroundHeightOffset;

	private bool mInitDone;

	private GameObject mMessageObject;

	public GameObject pMessageObject
	{
		set
		{
			mMessageObject = value;
		}
	}

	protected override void Start()
	{
		base.Start();
		if (!mInitDone)
		{
			InitUi();
		}
	}

	private void InitUi()
	{
		mBackground = FindItem("Background");
		mOriginalClipRangeRef = mKAUiSelectMenu.pPanel.baseClipRegion;
		if (mBackground != null)
		{
			Vector3[] worldCorners = mKAUiSelectMenu.pPanel.worldCorners;
			mOriginalBackgroundHeight = mBackground.pBackground.height;
			mBackgroundHeightOffset = (float)mOriginalBackgroundHeight - (worldCorners[1].y - worldCorners[0].y);
			if (mKAUiSelectMenu.pVerticalScrollbar != null)
			{
				mOriginalDownArrowOffset = Mathf.Abs(mKAUiSelectMenu.pVerticalScrollbar._DownArrow.transform.position.y - (mBackground.transform.position.y - (float)mBackground.pBackground.height));
			}
		}
		mInitDone = true;
	}

	public void Reposition()
	{
		KAUIMenu[] menuList = _MenuList;
		for (int i = 0; i < menuList.Length; i++)
		{
			menuList[i].pMenuGrid.Reposition();
		}
	}

	public override void OnOpen()
	{
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("SelectDefaultItem", SendMessageOptions.DontRequireReceiver);
		}
	}

	public void SetMenuHeight(float inPercentage)
	{
		if (!mInitDone)
		{
			InitUi();
		}
		float num = 1f - inPercentage;
		Vector4 baseClipRegion = mOriginalClipRangeRef;
		baseClipRegion.y = mOriginalClipRangeRef.y + mOriginalClipRangeRef.w * num / 2f;
		baseClipRegion.w = mOriginalClipRangeRef.w - mOriginalClipRangeRef.w * num;
		mKAUiSelectMenu.pPanel.baseClipRegion = baseClipRegion;
		BoxCollider component = mKAUiSelectMenu._ParentUi.GetComponent<BoxCollider>();
		if (component != null)
		{
			component.size = new Vector3(component.size.x, baseClipRegion.w, component.size.z);
			component.center = new Vector3(component.center.x, baseClipRegion.y, component.center.z);
		}
		Vector3[] worldCorners = mKAUiSelectMenu.pPanel.worldCorners;
		if (!(mBackground != null) || !(mKAUiSelectMenu.pVerticalScrollbar != null))
		{
			return;
		}
		int num2 = (int)(worldCorners[1].y - worldCorners[0].y + mBackgroundHeightOffset) - mBackground.pBackground.height;
		UISlicedSprite[] componentsInChildren = mBackground.GetComponentsInChildren<UISlicedSprite>();
		if (componentsInChildren != null && componentsInChildren.Length != 0)
		{
			UISlicedSprite[] array = componentsInChildren;
			for (int i = 0; i < array.Length; i++)
			{
				array[i].height += num2;
			}
		}
		KAWidget downArrow = mKAUiSelectMenu.pVerticalScrollbar._DownArrow;
		KAWidget upArrow = mKAUiSelectMenu.pVerticalScrollbar._UpArrow;
		downArrow.transform.position = new Vector3(downArrow.transform.position.x, mBackground.transform.position.y - (float)mBackground.pBackground.height + mOriginalDownArrowOffset, downArrow.transform.position.z);
		mKAUiSelectMenu.pVerticalScrollbar.backgroundWidget.height = (int)(upArrow.transform.localPosition.y - downArrow.transform.localPosition.y - (float)downArrow.pBackground.height);
		mKAUiSelectMenu.pVerticalScrollbar.foregroundWidget.height = (int)(upArrow.transform.localPosition.y - downArrow.transform.localPosition.y - (float)downArrow.pBackground.height);
	}

	public override void SelectItem(KAWidget item)
	{
		base.SelectItem(item);
		if (item != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)item.GetUserData();
			if (kAUISelectItemData != null && kAUISelectItemData._SlotLocked)
			{
				mKAUiSelectTabMenu.pSelectedTab.pTabData.BuySlot(base.gameObject, mKAUiSelectTabMenu.pSelectedTab.mNumSlotOccupied);
			}
			else if (mMessageObject != null)
			{
				mMessageObject.SendMessage("OnSelectBattleBackpackItem", item, SendMessageOptions.DontRequireReceiver);
			}
		}
		else if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnDeselectItem", SendMessageOptions.DontRequireReceiver);
		}
	}

	public override void AddWidgetData(KAWidget inWidget, KAUISelectItemData widgetData)
	{
		base.AddWidgetData(inWidget, widgetData);
		bool flag = widgetData?._IsBattleReady ?? false;
		KAWidget kAWidget = inWidget.FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(flag);
		}
		KAWidget kAWidget2 = inWidget.FindChildItem("FlightReadyIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(widgetData != null && widgetData._ItemData != null && widgetData._ItemData.HasAttribute("FlightSuit"));
		}
		KAWidget kAWidget3 = inWidget.FindChildItem(((UiBattleBackpackMenu)base.pKAUiSelectMenu)._ItemColorWidget);
		if (kAWidget3 != null)
		{
			if (flag)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((widgetData == null || widgetData._ItemData == null || !widgetData._ItemData.ItemRarity.HasValue) ? ItemRarity.Common : widgetData._ItemData.ItemRarity.Value, kAWidget3);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(((UiBattleBackpackMenu)base.pKAUiSelectMenu).ItemDefaultColor, kAWidget3);
			}
		}
	}

	public override KAWidget AddEmptySlot()
	{
		KAWidget kAWidget = mKAUiSelectMenu.AddWidget("EmptySlot");
		if (kAWidget != null)
		{
			AddWidgetData(kAWidget, null);
		}
		return kAWidget;
	}
}
