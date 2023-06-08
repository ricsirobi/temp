using System;
using UnityEngine;

public class KAUIDropDown : KAUI
{
	public Color _MaskColor = new Color(0.5f, 0.5f, 0.5f, 0.5f);

	public GameObject _MessageObj;

	public string _BkgItemName = "";

	public string _SelectItemName = "";

	public string _DisplayItemName = "";

	[NonSerialized]
	public KAUIDropDownMenu mDropDownMenu;

	protected KAWidget mBkgItem;

	protected KAWidget mSelectionItem;

	protected KAWidget mDisplayItem;

	protected KAWidget mMouseOverItem;

	protected bool mIsOpen;

	public KAWidget GetBackgroundItem()
	{
		return mBkgItem;
	}

	public virtual void OnDisable()
	{
		if (mIsOpen)
		{
			CloseDropDown();
		}
	}

	protected override void Start()
	{
		base.Start();
		mSelectionItem = FindItem(_SelectItemName);
		mDisplayItem = FindItem(_DisplayItemName);
		mBkgItem = FindItem(_BkgItemName);
		if (mBkgItem != null)
		{
			mBkgItem.SetVisibility(inVisible: false);
		}
		if (_MessageObj != null)
		{
			_MessageObj.SendMessage("OnDropDownInit", this, SendMessageOptions.DontRequireReceiver);
		}
	}

	protected override void Update()
	{
		bool mouseButtonUp = KAInput.GetMouseButtonUp(0);
		bool flag = mIsOpen;
		base.Update();
		if (mouseButtonUp && mIsOpen && flag && !mDropDownMenu.BoundsCheck() && KAUI._GlobalExclusiveUI == this)
		{
			KAInput.ResetInputAxes();
			CloseDropDown();
		}
	}

	public override void OnHover(KAWidget inWidget, bool inIsHover)
	{
		base.OnHover(inWidget, inIsHover);
		if (inIsHover)
		{
			mMouseOverItem = inWidget;
		}
		else
		{
			mMouseOverItem = null;
		}
	}

	public virtual void OpenDropDown()
	{
		bool num = mIsOpen;
		mIsOpen = true;
		if (mSelectionItem != null)
		{
			mSelectionItem.SetState(KAUIState.NOT_INTERACTIVE);
		}
		if (mDropDownMenu != null)
		{
			mDropDownMenu.SetVisibility(inVisible: true);
		}
		if (mBkgItem != null)
		{
			mBkgItem.SetVisibility(inVisible: true);
		}
		if (_MessageObj != null)
		{
			_MessageObj.SendMessage("OnDropDownOpen", this, SendMessageOptions.DontRequireReceiver);
		}
		if (!num)
		{
			KAUI.SetExclusive(this, _MaskColor);
		}
	}

	public virtual void CloseDropDown()
	{
		mIsOpen = false;
		if (mSelectionItem != null)
		{
			mSelectionItem.SetState(KAUIState.INTERACTIVE);
		}
		if (mDropDownMenu != null)
		{
			mDropDownMenu.SetVisibility(inVisible: false);
		}
		if (mBkgItem != null)
		{
			mBkgItem.SetVisibility(inVisible: false);
		}
		if (_MessageObj != null)
		{
			_MessageObj.SendMessage("OnDropDownClosed", this, SendMessageOptions.DontRequireReceiver);
		}
		KAUI.RemoveExclusive(this);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mSelectionItem)
		{
			KAInput.ResetInputAxes();
			ProcessClickedSelection();
		}
	}

	public virtual void ProcessClickedSelection()
	{
		OpenDropDown();
	}

	public virtual void ProcessDropDownSelection(KAWidget item)
	{
		if (mDisplayItem != null)
		{
			if (item.GetTexture() != null)
			{
				mDisplayItem.SetTexture(item.GetTexture());
			}
			else if (!string.IsNullOrEmpty(item.GetText()))
			{
				mDisplayItem.SetText(item.GetText());
			}
		}
	}

	public virtual void ProcessMenuSelection(KAWidget item, int idx)
	{
		ProcessDropDownSelection(item);
		if (_MessageObj != null)
		{
			_MessageObj.SendMessage("OnDropDownSelected", this, SendMessageOptions.DontRequireReceiver);
		}
	}
}
