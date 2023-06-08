using System;
using SquadTactics;

public class UiItemElements : KAUI
{
	public bool _ShowItemName = true;

	public bool _ShowItemIcon = true;

	private KAWidget mItemName;

	private KAWidget mItemIcon;

	private KAUIMenu mItemElementsMenu;

	private UserItemData mUserItemData;

	private bool mInitialized;

	protected override void Start()
	{
		if (!mInitialized)
		{
			Initialized();
		}
		base.Start();
	}

	private void Initialized()
	{
		mItemName = FindItem("ItemName");
		mItemIcon = FindItem("ItemIcon");
		mItemName.SetVisibility(_ShowItemName);
		mItemIcon.SetVisibility(_ShowItemIcon);
		mItemElementsMenu = _MenuList[0];
		mInitialized = true;
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		if (mItemIcon == inWidget)
		{
			inWidget.SetVisibility(_ShowItemIcon);
		}
		else
		{
			inWidget.SetVisibility(inVisible: true);
		}
	}

	public void ShowElements(UserItemData userItemData)
	{
		mUserItemData = userItemData;
		if (!mInitialized)
		{
			Initialized();
		}
		if (mItemName != null && _ShowItemName)
		{
			mItemName.SetText(userItemData.Item.ItemName);
		}
		if (mItemIcon != null && _ShowItemIcon)
		{
			mItemIcon.SetTextureFromBundle(userItemData.Item.IconName, base.gameObject);
			mItemIcon.SetVisibility(inVisible: true);
		}
		UpdateElements();
	}

	private void UpdateElements()
	{
		mItemElementsMenu.ClearItems();
		string attribute = mUserItemData.Item.GetAttribute("ElementType", string.Empty);
		ElementType type = ElementType.NONE;
		if (!string.IsNullOrEmpty(attribute) && Enum.IsDefined(typeof(ElementType), attribute))
		{
			type = (ElementType)Enum.Parse(typeof(ElementType), attribute.ToUpper());
		}
		AddElement(type);
	}

	private void AddElement(ElementType type)
	{
		AddElement(Settings.pInstance.GetElementInfo(type));
	}

	private void AddElement(ElementInfo info)
	{
		if (info != null)
		{
			KAWidget kAWidget = mItemElementsMenu.AddWidget(info._Element.ToString());
			SetTexture(kAWidget, info._Icon, fromBundle: false);
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	private void SetTexture(KAWidget widget, string texPath, bool fromBundle)
	{
		if (fromBundle)
		{
			widget.SetTextureFromBundle(texPath, base.gameObject);
		}
		else
		{
			widget.pBackground.UpdateSprite(texPath);
		}
	}
}
