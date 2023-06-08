using UnityEngine;

public class UiInventoryCategory : KAUI
{
	[HideInInspector]
	public int _CategoryId;

	public StoreLoader.Selection _StoreInfo;

	public UiInventoryCategoryItemMenu _Menu;

	public bool _DestroyOnClose;

	private KAWidget mText;

	private string mInfoText;

	protected override void Start()
	{
		base.Start();
		mText = FindItem("InfoTxt");
		mText.SetVisibility(inVisible: false);
	}

	public void ShowMenu()
	{
		if (_Menu != null)
		{
			mText.SetVisibility(inVisible: false);
			AddItemsfromInventory(_CategoryId);
			_Menu.SetVisibility(inVisible: true);
			mText.SetText(mInfoText);
		}
		KAWidget kAWidget = FindItem("Close");
		if (kAWidget != null)
		{
			kAWidget.gameObject.transform.position = new Vector3(0f, 0f, kAWidget.gameObject.transform.position.z);
			BoxCollider component = kAWidget.GetComponent<BoxCollider>();
			if (component != null)
			{
				Vector3 size = new Vector3(Screen.width, Screen.height, 0f);
				component.size = size;
			}
		}
	}

	public void AddItemsfromInventory(int inCategory)
	{
		UserItemData[] items = CommonInventoryData.pInstance.GetItems(inCategory);
		if (null != _Menu)
		{
			_Menu.ClearItems();
		}
		if (items != null && null != _Menu)
		{
			UserItemData[] array = items;
			foreach (UserItemData userItemData in array)
			{
				InventoryItemDataGeneric inventoryItemDataGeneric = new InventoryItemDataGeneric();
				inventoryItemDataGeneric.Init(_Menu, userItemData.Item);
				KAWidget kAWidget = _Menu.DuplicateWidget(_Menu._Template);
				if (kAWidget != null)
				{
					kAWidget.name = userItemData.Item.ItemID.ToString();
					kAWidget.SetUserData(inventoryItemDataGeneric);
					inventoryItemDataGeneric.ShowLoadingItem(inShow: true);
					_Menu.AddWidget(kAWidget);
					_Menu.SetVisibility(inVisible: true);
				}
				inventoryItemDataGeneric.LoadResource();
			}
		}
		if (mText != null && items == null)
		{
			mText.SetVisibility(inVisible: true);
		}
		else if (items != null && items.Length == 0)
		{
			mText.SetVisibility(inVisible: true);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == "Close")
		{
			if (_DestroyOnClose)
			{
				Object.Destroy(base.gameObject);
			}
			else
			{
				SetVisibility(inVisible: false);
			}
		}
		else if (inWidget == mText)
		{
			SetVisibility(inVisible: false);
			if (FishingZone._FishingZoneUi != null)
			{
				FishingZone._FishingZoneUi.SetVisibility(inVisible: false);
			}
			base.transform.parent.parent.SendMessage("OnStoreOpened");
			if (_StoreInfo != null)
			{
				StoreLoader.Load(setDefaultMenuItem: true, _StoreInfo._Category, _StoreInfo._Store, base.gameObject);
			}
		}
	}

	public void SetNoBaitText(LocaleString message)
	{
		mInfoText = message.GetLocalizedString();
	}

	private void OnStoreClosed()
	{
		AvAvatar.SetUIActive(inActive: false);
		AvAvatar.pState = AvAvatarState.PAUSED;
		if (FishingZone._FishingZoneUi != null)
		{
			FishingZone._FishingZoneUi.SetVisibility(inVisible: true);
			base.transform.parent.parent.SendMessage("OnStoreClosed");
		}
	}
}
