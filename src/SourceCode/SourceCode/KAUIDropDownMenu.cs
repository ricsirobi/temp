using UnityEngine;

public class KAUIDropDownMenu : KAUIMenu
{
	public delegate void OnItemSelected(KAWidget widget, KAUIDropDownMenu dropDown);

	private bool mIsDropped;

	public KAWidget _UpdateItem;

	protected KAUIDropDown mDropDownUI;

	public OnItemSelected onItemSelected;

	protected override void Start()
	{
		base.Start();
		UpdateState(mIsDropped);
	}

	public virtual void UpdateState(bool isDropped)
	{
		mIsDropped = isDropped;
		SetVisibility(mIsDropped);
		_ = mIsDropped;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		SetSelectedWidget(inWidget);
		UpdateState(!GetVisibility());
	}

	public bool BoundsCheck()
	{
		Bounds bounds = NGUIMath.CalculateAbsoluteWidgetBounds(_BackgroundObject.transform);
		Vector3 min = bounds.min;
		Vector3 max = bounds.max;
		min = UICamera.mainCamera.WorldToScreenPoint(min);
		max = UICamera.mainCamera.WorldToScreenPoint(max);
		Vector2 cursorPosition = UICursorManager.GetCursorPosition();
		if (cursorPosition.x > min.x && cursorPosition.x < max.x && cursorPosition.y < max.y && cursorPosition.y > min.y)
		{
			return true;
		}
		return false;
	}

	public virtual void SetSelectedWidget(KAWidget inWidget)
	{
		if (inWidget != null)
		{
			if (_UpdateItem != null && _UpdateItem.GetLabel() != null && inWidget.GetLabel() != null)
			{
				_UpdateItem.SetText(inWidget.GetText());
			}
			if (onItemSelected != null)
			{
				onItemSelected(inWidget, this);
			}
			if (mDropDownUI != null)
			{
				mDropDownUI.ProcessMenuSelection(inWidget, GetSelectedItemIndex());
			}
		}
	}
}
