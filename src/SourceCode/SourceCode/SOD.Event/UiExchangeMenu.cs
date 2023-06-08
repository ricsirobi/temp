using UnityEngine;

namespace SOD.Event;

public class UiExchangeMenu : KAUIMenu
{
	private UiExchange mUiExchange;

	protected override void Start()
	{
		base.Start();
		mUiExchange = _ParentUi as UiExchange;
	}

	public override void OnDragStart(KAWidget inWidget)
	{
		base.OnDragStart(inWidget);
		if (mUiExchange != null)
		{
			mUiExchange.ItemPicked(inWidget);
		}
	}

	public override void OnDragEnd(KAWidget inWidget)
	{
		base.OnDragEnd(inWidget);
		if (inWidget.GetUserData() != null && mUiExchange != null)
		{
			ClearDragItem();
			mUiExchange.ItemDropped(inWidget);
		}
	}

	public void SetWidgetIndex(KAWidget widget, int index)
	{
		if (FindItemIndex(widget) != index)
		{
			mItemInfo.Remove(widget);
			if (index < mItemInfo.Count)
			{
				mItemInfo.Insert(index, widget);
			}
			else
			{
				mItemInfo.Add(widget);
			}
		}
	}

	protected override void UpdateVisibility(bool inVisible)
	{
		base.UpdateVisibility(inVisible);
		if (!inVisible)
		{
			ClearDragItem();
		}
	}

	protected override void OnDestroy()
	{
		base.OnDestroy();
		ClearDragItem();
	}

	private void OnApplicationFocus(bool hasFocus)
	{
		UICamera.MouseOrTouch mouseOrTouch = ((UICamera.currentScheme == UICamera.ControlScheme.Mouse) ? UICamera.mouse0 : UICamera.currentTouch);
		if (!hasFocus && mouseOrTouch != null && mouseOrTouch.dragged != null && mouseOrTouch.dragged.GetComponent<KAWidget>() != null)
		{
			ClearDragItem();
		}
	}

	private void ClearDragItem()
	{
		if (KAUIManager.pInstance.pDragItem != null)
		{
			KAUIManager.pInstance.pDragItem.DetachFromCursor();
			Object.Destroy(KAUIManager.pInstance.pDragItem.gameObject);
		}
	}
}
