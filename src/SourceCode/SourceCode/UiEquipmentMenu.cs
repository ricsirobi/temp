using UnityEngine;

public class UiEquipmentMenu : KAUISelectMenu
{
	private UiEquipment mUiEquipment;

	private KAWidget mAttachedItem;

	public KAWidget pAttachedItem
	{
		get
		{
			return mAttachedItem;
		}
		set
		{
			mAttachedItem = value;
		}
	}

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mUiEquipment = (UiEquipment)mMainUI;
	}

	public override void SelectItem(KAWidget widget)
	{
		base.SelectItem(widget);
		if (widget != null)
		{
			mUiEquipment.SetItem(widget);
		}
	}

	public override bool IsCurrentDataReady()
	{
		return mItemInitialized;
	}

	public override void OnPressRepeated(KAWidget inWidget, bool isPressed)
	{
		base.OnPressRepeated(inWidget, isPressed);
		if (isPressed && mAttachedItem == null)
		{
			RemoveWidget(inWidget);
			mAttachedItem = DuplicateWidget(inWidget);
			mAttachedItem.SetUserData(inWidget.GetUserData());
			mAttachedItem.AttachToCursor(0f, 0f);
			mAttachedItem.SetState(KAUIState.NOT_INTERACTIVE);
		}
		else if (!isPressed && mAttachedItem != null)
		{
			Object.DestroyImmediate(mAttachedItem.gameObject);
			mAttachedItem = null;
		}
	}
}
