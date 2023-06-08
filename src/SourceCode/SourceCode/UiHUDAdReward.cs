using UnityEngine;

public class UiHUDAdReward : KAUI
{
	public LocaleString _RewardBoxHeaderText = new LocaleString("Possible Rewards");

	private MysteryBoxStoreInfo mRewardInfo;

	private KAUIMenu mHudRewardMenu;

	private KAWidget mCloseBtn;

	private KAWidget mYesBtn;

	private ItemData mItemData;

	private GameObject mMessageObject;

	protected override void Start()
	{
		base.Start();
		KAUI.SetExclusive(this);
		mCloseBtn = FindItem("BtnClose");
		mYesBtn = FindItem("YesBtn");
		FindItem("TxtTitle").SetText(_RewardBoxHeaderText.GetLocalizedString());
		mHudRewardMenu = _MenuList[0];
		KAUICursorManager.SetDefaultCursor("Loading");
		ItemStoreDataLoader.Load(mRewardInfo._StoreID, OnStoreLoaded);
	}

	public void Init(MysteryBoxStoreInfo rewardInfo, GameObject messageObject)
	{
		mRewardInfo = rewardInfo;
		mMessageObject = messageObject;
	}

	private void OnStoreLoaded(StoreData sd)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		if (sd != null)
		{
			mItemData = sd.FindItem(mRewardInfo._ItemID);
			ItemDataRelationship[] array = mItemData.GetPrizeItemsSorted().ToArray();
			if (array != null)
			{
				LoadRewards(array);
			}
		}
	}

	private void LoadRewards(ItemDataRelationship[] itemList)
	{
		foreach (ItemDataRelationship itemDataRelationship in itemList)
		{
			ItemData.Load(itemDataRelationship.ItemId, ItemDataLoaded, itemDataRelationship);
		}
	}

	private void ItemDataLoaded(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem == null)
		{
			return;
		}
		KAWidget kAWidget = mHudRewardMenu.AddWidget(mHudRewardMenu._Template.name);
		kAWidget.name = dataItem.ItemName;
		kAWidget.FindChildItem("TxtItem").SetText(dataItem.ItemName);
		KAWidget kAWidget2 = kAWidget.FindChildItem("TxtQuantity");
		if (kAWidget2 != null)
		{
			ItemDataRelationship itemDataRelationship = (ItemDataRelationship)inUserData;
			if (itemDataRelationship.Quantity > 1)
			{
				kAWidget2.SetText(itemDataRelationship.Quantity.ToString());
			}
			kAWidget2.SetVisibility(itemDataRelationship.Quantity > 1);
		}
		kAWidget.FindChildItem("Icon").SetTextureFromBundle(dataItem.IconName);
	}

	public override void OnClick(KAWidget item)
	{
		base.OnClick(item);
		if (item == mCloseBtn)
		{
			Exit(accepted: false);
		}
		else if (item == mYesBtn)
		{
			Exit(accepted: true);
		}
	}

	private void Exit(bool accepted)
	{
		KAUI.RemoveExclusive(this);
		Object.Destroy(base.gameObject);
		if (mMessageObject != null)
		{
			mMessageObject.SendMessage("OnAdRewardDBAccepted", accepted, SendMessageOptions.DontRequireReceiver);
		}
	}
}
