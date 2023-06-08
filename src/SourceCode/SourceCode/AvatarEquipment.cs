using System;

public class AvatarEquipment
{
	private int mEquipmentPairID = 2013;

	private static AvatarEquipment mInstance;

	private PairData mEquipmentData;

	private bool mIsInventoryDirty;

	public static AvatarEquipment pInstance => mInstance;

	public static bool pIsReady => mInstance.mEquipmentData != null;

	public static void Init()
	{
		if (mInstance == null)
		{
			mInstance = new AvatarEquipment();
			PairData.Load(mInstance.mEquipmentPairID, mInstance.OnPairDataReady, null);
		}
	}

	public void OnPairDataReady(bool success, PairData pData, object inUserData)
	{
		if (pData != null)
		{
			mEquipmentData = pData;
		}
		else
		{
			mEquipmentData = new PairData();
		}
	}

	public void EquipItem(EquipmentParts equipmentPart, int itemID)
	{
		EquipItem(equipmentPart.ToString(), itemID);
	}

	public void EquipItem(string equipmentName, int itemID)
	{
		if (pIsReady)
		{
			equipmentName = equipmentName.ToLower();
			mEquipmentData.SetValue(equipmentName, itemID.ToString());
		}
	}

	public bool IsItemEquipped(int inItemID)
	{
		foreach (EquipmentParts value in Enum.GetValues(typeof(EquipmentParts)))
		{
			UserItemData item = GetItem(value);
			if (item != null && inItemID == item.Item.ItemID)
			{
				return true;
			}
		}
		return false;
	}

	public UserItemData GetItem(EquipmentParts equipmentPart)
	{
		return GetItem(equipmentPart.ToString());
	}

	public UserItemData GetItem(string equipmentName)
	{
		if (pIsReady)
		{
			equipmentName = equipmentName.ToLower();
			Pair pair = mEquipmentData.FindByKey(equipmentName);
			if (pair != null)
			{
				int result = 0;
				if (int.TryParse(pair.PairValue, out result))
				{
					return CommonInventoryData.pInstance.FindItem(result);
				}
			}
		}
		return null;
	}

	public void RemoveItem(EquipmentParts equipmentPart)
	{
		RemoveItem(equipmentPart.ToString());
	}

	public void RemoveItem(string equipmentName, bool removeFromInventory = false)
	{
		if (!pIsReady)
		{
			return;
		}
		equipmentName = equipmentName.ToLower();
		if (removeFromInventory)
		{
			Pair pair = mEquipmentData.FindByKey(equipmentName);
			int result = 0;
			if (pair != null && int.TryParse(pair.PairValue, out result))
			{
				mIsInventoryDirty = true;
				CommonInventoryData.pInstance.RemoveItem(result, updateServer: false);
			}
		}
		mEquipmentData.RemoveByKey(equipmentName);
	}

	public void Save()
	{
		if (pIsReady)
		{
			PairData.Save(mEquipmentPairID);
			if (mIsInventoryDirty)
			{
				mIsInventoryDirty = false;
				CommonInventoryData.pInstance.Save();
			}
		}
	}
}
