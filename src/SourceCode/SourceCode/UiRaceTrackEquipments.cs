using System.Collections.Generic;

public class UiRaceTrackEquipments : KAUI
{
	public string _ConsumableHeader = "Consumable";

	public string _SkillHeader = "Skill";

	private KAWidget[] mConsumable = new KAWidget[3];

	private KAWidget[] mSkills = new KAWidget[3];

	private List<ConsumableItems.ConsumableData> mConsumableList;

	protected override void Start()
	{
		base.Start();
		for (int i = 0; i < mConsumable.Length; i++)
		{
			mConsumable[i] = FindItem(_ConsumableHeader + (i + 1));
		}
		for (int j = 0; j < mSkills.Length; j++)
		{
			mSkills[j] = FindItem(_SkillHeader + (j + 1));
		}
		SetVisibility(inVisible: false);
		SetConsumables();
	}

	private void SetConsumables()
	{
		if (ConsumableItems.mInstance == null)
		{
			return;
		}
		mConsumableList = ConsumableItems.mInstance.GetConsumablesList();
		if (mConsumableList == null || mConsumableList.Count <= 0)
		{
			return;
		}
		for (int i = 0; i < mConsumable.Length && i < mConsumableList.Count; i++)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(mConsumableList[i]._ItemID);
			if (userItemData == null)
			{
				continue;
			}
			int quantity = CommonInventoryData.pInstance.GetQuantity(userItemData.Item.ItemID);
			mConsumable[i].SetTextureFromBundle(userItemData.Item.IconName);
			KAWidget kAWidget = mConsumable[i].FindChildItem("TxtQuantity");
			if (kAWidget != null)
			{
				if (quantity > 0)
				{
					kAWidget.SetText(quantity.ToString());
					kAWidget.SetVisibility(inVisible: true);
				}
				else
				{
					kAWidget.SetVisibility(inVisible: false);
				}
			}
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (!inWidget.name.Contains(_ConsumableHeader))
		{
			return;
		}
		for (int i = 0; i < mConsumableList.Count; i++)
		{
			if (inWidget.name == _ConsumableHeader + (i + 1) && SanctuaryManager.pCurPetInstance != null)
			{
				UserItemData userItemData = CommonInventoryData.pInstance.FindItem(mConsumableList[i]._ItemID);
				if (userItemData != null && userItemData.Quantity >= 1)
				{
					CommonInventoryData.pInstance.RemoveItem(userItemData.Item.ItemID, updateServer: true);
					SanctuaryManager.pCurPetInstance.DoEat(userItemData.Item);
					UpdateConsumableQuantity(mConsumableList[i]._ItemID, inWidget);
				}
			}
		}
	}

	private void UpdateConsumableQuantity(int inItemID, KAWidget inParent)
	{
		int quantity = CommonInventoryData.pInstance.GetQuantity(inItemID);
		if (!(inParent != null))
		{
			return;
		}
		KAWidget kAWidget = inParent.FindChildItem("TxtQuantity");
		if (kAWidget != null)
		{
			if (quantity > 0)
			{
				kAWidget.SetText(quantity.ToString());
				kAWidget.SetVisibility(inVisible: true);
			}
			else
			{
				kAWidget.SetVisibility(inVisible: false);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (AvAvatar.pInputEnabled)
		{
			if (KAInput.GetButtonUp("Skill1"))
			{
				OnClick(FindItem(_SkillHeader + "1"));
			}
			else if (KAInput.GetButtonUp("Skill2"))
			{
				OnClick(FindItem(_SkillHeader + "2"));
			}
			else if (KAInput.GetButtonUp("Skill3"))
			{
				OnClick(FindItem(_SkillHeader + "3"));
			}
			else if (KAInput.GetButtonUp("Consume1"))
			{
				OnClick(FindItem(_ConsumableHeader + "1"));
			}
			else if (KAInput.GetButtonUp("Consume2"))
			{
				OnClick(FindItem(_ConsumableHeader + "2"));
			}
			else if (KAInput.GetButtonUp("Consume2"))
			{
				OnClick(FindItem(_ConsumableHeader + "3"));
			}
		}
	}
}
