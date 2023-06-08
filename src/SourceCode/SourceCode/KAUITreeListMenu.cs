using System.Collections.Generic;
using UnityEngine;

public class KAUITreeListMenu : KAUIMenu
{
	public string _ItemExpandTemplateName = "BtnExpand";

	public string _LeftScrollBtnName = "BtnAmScrollLt";

	public string _RightScrollBtnName = "BtnAmScrollRt";

	public Color _SelectedItemColor = Color.yellow;

	public string _ExpandSpriteName = "BtnMbAmScrollLtMask";

	public string _CollapseSpriteName = "BtnMbAmScrollRtMask";

	public float _GroupChildItemOffsetX = 20f;

	protected List<KAUITreeListItemData> mTreeGroupData = new List<KAUITreeListItemData>();

	private int mCurrentSelectedItemIndex;

	private KAUITreeListEvents mTreeEvents;

	public KAUITreeListEvents pTreeEvents
	{
		get
		{
			return mTreeEvents;
		}
		set
		{
			mTreeEvents = value;
		}
	}

	protected override void Awake()
	{
		base.Awake();
		mTreeEvents = new KAUITreeListEvents();
	}

	protected virtual KAWidget GetTemplateItem(string itemName)
	{
		return _Template;
	}

	protected virtual KAWidget CreateWidgetFromData(KAUITreeListItemData inItem)
	{
		KAWidget kAWidget = DuplicateWidget(GetTemplateItem(inItem._Name));
		kAWidget.name = inItem._Name;
		kAWidget.SetText(inItem._DisplayText.GetLocalizedString());
		kAWidget.SetUserData(inItem);
		kAWidget.SetVisibility(inVisible: true);
		kAWidget.SetState(KAUIState.INTERACTIVE);
		return kAWidget;
	}

	public void PopulateTreeNode(KAUITreeListItemData inItem)
	{
		KAWidget kAWidget = CreateWidgetFromData(inItem);
		if (string.IsNullOrEmpty(_ItemExpandTemplateName))
		{
			UtDebug.LogError("No Expand button provided!!!!");
		}
		else if (!inItem.IsLeafItem())
		{
			KAWidget kAWidget2 = kAWidget.FindChildItem(_ItemExpandTemplateName);
			if (kAWidget2 != null)
			{
				kAWidget2.SetVisibility(inVisible: true);
				kAWidget2.SetState(KAUIState.INTERACTIVE);
				string sprite = (inItem._Collapsed ? _ExpandSpriteName : _CollapseSpriteName);
				kAWidget2.SetSprite(sprite);
			}
		}
		AddWidget(kAWidget);
		if (!inItem._Collapsed && !inItem.IsLeafItem())
		{
			for (int i = 0; i < inItem._ChildList.Count; i++)
			{
				PopulateTreeNode(inItem._ChildList[i]);
			}
		}
	}

	protected virtual void IndentTreeNode(KAUITreeListItemData inItemData, float inParentIndent)
	{
		KAWidget item = inItemData._Item;
		if (item != null)
		{
			item.SetPosition(inParentIndent + _GroupChildItemOffsetX, item.GetPosition().y);
		}
		foreach (KAUITreeListItemData child in inItemData._ChildList)
		{
			IndentTreeNode(child, inParentIndent + _GroupChildItemOffsetX);
		}
	}

	public void ClearData()
	{
		mTreeGroupData.Clear();
	}

	public void PopulateTreeList()
	{
		ClearItems();
		foreach (KAUITreeListItemData mTreeGroupDatum in mTreeGroupData)
		{
			PopulateTreeNode(mTreeGroupDatum);
		}
		foreach (KAUITreeListItemData mTreeGroupDatum2 in mTreeGroupData)
		{
			IndentTreeNode(mTreeGroupDatum2, 0f - _GroupChildItemOffsetX);
		}
		if (GetNumItems() > 0)
		{
			mCurrentSelectedItemIndex = 0;
			SelectWidget(GetItemAt(mCurrentSelectedItemIndex));
		}
	}

	public virtual void ExpandCollapseList(KAWidget parentItem)
	{
		KAUITreeListItemData kAUITreeListItemData = FindFirstItem(mTreeGroupData, parentItem.name);
		if (kAUITreeListItemData != null)
		{
			kAUITreeListItemData._Collapsed = !kAUITreeListItemData._Collapsed;
			PopulateTreeList();
		}
	}

	public override void OnClick(KAWidget item)
	{
		if (item.name == _ItemExpandTemplateName)
		{
			ExpandCollapseList(item.GetParentItem());
		}
		else if (item is KAButton && (IsChildItem(item) || item.FindChildItem(_ItemExpandTemplateName) != null))
		{
			SelectWidget(item);
		}
	}

	public void CreateTreeFromList<TYPE>(KAUITreeListItemData inParentItem, List<TYPE> inList)
	{
		foreach (TYPE @in in inList)
		{
			KAUITreeListItemData itemFor = GetItemFor(@in);
			if (itemFor != null)
			{
				AddItem(inParentItem._Name, itemFor, inRefreshTree: false);
			}
			List<TYPE> childList = GetChildList(@in);
			if (childList != null && childList.Count > 0)
			{
				CreateTreeFromList(itemFor, childList);
			}
		}
		PopulateTreeList();
	}

	protected virtual List<TYPE> GetChildList<TYPE>(TYPE inItem)
	{
		UtDebug.LogError("GetChildList method needs to be overridden.");
		return null;
	}

	protected virtual KAUITreeListItemData GetItemFor<TYPE>(TYPE inItemData)
	{
		UtDebug.LogError("GetItemFor method needs to be overridden.");
		return null;
	}

	public KAUITreeListItemData AddItem(string inParentName, string inName, LocaleString inLocaleString, bool inCollapsed, bool inRefreshTree)
	{
		KAUITreeListItemData kAUITreeListItemData = mTreeGroupData.Find((KAUITreeListItemData a) => a._Name == inParentName);
		KAUITreeListItemData kAUITreeListItemData2 = new KAUITreeListItemData(kAUITreeListItemData, inName, inLocaleString, inCollapsed, null);
		if (kAUITreeListItemData != null)
		{
			kAUITreeListItemData.AddChild(kAUITreeListItemData2);
		}
		else
		{
			mTreeGroupData.Add(kAUITreeListItemData2);
		}
		if (inRefreshTree)
		{
			PopulateTreeList();
		}
		return kAUITreeListItemData2;
	}

	public KAUITreeListItemData FindFirstItem(List<KAUITreeListItemData> inItemList, string inName)
	{
		KAUITreeListItemData kAUITreeListItemData = inItemList.Find((KAUITreeListItemData a) => a._Name == inName);
		if (kAUITreeListItemData == null)
		{
			foreach (KAUITreeListItemData inItem in inItemList)
			{
				kAUITreeListItemData = FindFirstItem(inItem._ChildList, inName);
				if (kAUITreeListItemData != null)
				{
					return kAUITreeListItemData;
				}
			}
		}
		return kAUITreeListItemData;
	}

	public KAUITreeListItemData AddItem(string inParentName, KAUITreeListItemData inNewItem, bool inRefreshTree)
	{
		KAUITreeListItemData kAUITreeListItemData = FindFirstItem(mTreeGroupData, inParentName);
		if (kAUITreeListItemData != null)
		{
			kAUITreeListItemData.AddChild(inNewItem);
		}
		else
		{
			mTreeGroupData.Add(inNewItem);
		}
		if (inRefreshTree)
		{
			PopulateTreeList();
		}
		return inNewItem;
	}

	public void SetDefaultSelectedWidget(string inWidgetName)
	{
		KAWidget kAWidget = FindItem(inWidgetName);
		if (kAWidget != null)
		{
			SelectWidget(kAWidget);
		}
	}

	protected void SelectWidget(KAWidget item)
	{
		base.SetSelectedItem(item);
		mCurrentSelectedItemIndex = GetSelectedItemIndex();
		ResetColorSelection();
		SetSelectWidgetColor(item, isRestoreOrginalColor: false);
		if (mTreeEvents != null)
		{
			object obj = null;
			KAWidgetUserData userData = item.GetUserData();
			if (userData != null && userData._Item != null && userData._Item is KAButton)
			{
				obj = GetSelectedItemData(userData._Item.name, item.name);
			}
			if (obj == null)
			{
				obj = GetSelectedItemData(item.name, "");
			}
			mTreeEvents.ProcessClickEvent(item, obj);
		}
	}

	private object GetSelectedItemData(string groupName, string childName)
	{
		foreach (KAUITreeListItemData mTreeGroupDatum in mTreeGroupData)
		{
			if (!(mTreeGroupDatum._Name == groupName))
			{
				continue;
			}
			if (!string.IsNullOrEmpty(childName))
			{
				foreach (KAUITreeListItemData child in mTreeGroupDatum._ChildList)
				{
					if (child._Name == childName)
					{
						return child;
					}
				}
				continue;
			}
			return mTreeGroupDatum;
		}
		return null;
	}

	private void ResetColorSelection()
	{
		foreach (KAWidget item in GetItems())
		{
			SetSelectWidgetColor(item, isRestoreOrginalColor: true);
		}
	}

	private void SetSelectWidgetColor(KAWidget inWidget, bool isRestoreOrginalColor)
	{
		if (inWidget.GetLabel() != null)
		{
			inWidget.GetLabel().color = (isRestoreOrginalColor ? inWidget.GetLabel().pOrgColorTint : _SelectedItemColor);
		}
	}

	private bool IsChildItem(KAWidget inItem)
	{
		if (inItem.GetUserData() != null && inItem.GetUserData()._Item != null)
		{
			return inItem.GetUserData()._Item is KAButton;
		}
		return false;
	}
}
