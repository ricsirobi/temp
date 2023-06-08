using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using UnityEngine;

[Serializable]
[XmlRoot(ElementName = "StableData", Namespace = "")]
public class StableData
{
	[XmlElement(ElementName = "Name")]
	public string Name;

	[XmlElement(ElementName = "ID")]
	public int ID;

	[XmlElement(ElementName = "ItemID")]
	public int ItemID;

	[XmlElement(ElementName = "InventoryID")]
	public int InventoryID;

	[XmlElement(ElementName = "Nests")]
	public List<NestData> NestList;

	private const int mPairDataID = 2014;

	private const string mPairDataKey = "Stables";

	private const string mStablesCountKeyName = "NumStables";

	private const string mStableDataKeyPrefix = "Stable";

	private static StableData mInstance = null;

	private static bool mIsReady = false;

	private static PairData mPairData = null;

	public static List<StableData> pStableList = new List<StableData>();

	private string mLocaleName;

	private const int LOG_PRIORITY = 100;

	[XmlIgnore]
	public string pLocaleName
	{
		get
		{
			return mLocaleName;
		}
		set
		{
			mLocaleName = value;
		}
	}

	public static StableData pInstance => mInstance;

	public static bool pIsReady
	{
		get
		{
			return mIsReady;
		}
		set
		{
			mIsReady = value;
		}
	}

	public static void UpdateInfo()
	{
		if (mIsReady && CommonInventoryData.pIsReady)
		{
			List<UserItemData> userItems = new List<UserItemData>();
			CheckInventory(CommonInventoryData.pInstance, ref userItems);
			CheckInventory(ParentData.pInstance.pInventory.pData, ref userItems);
			if (userItems.Count > 0)
			{
				AddData(userItems);
			}
			UpdateLocaleNames();
		}
	}

	private static void CheckInventory(CommonInventoryData cid, ref List<UserItemData> userItems)
	{
		if (cid == null)
		{
			return;
		}
		UserItemData[] items = cid.GetItems(455);
		if (items == null)
		{
			return;
		}
		UserItemData[] array = items;
		foreach (UserItemData item in array)
		{
			List<StableData> list = pStableList.FindAll((StableData s) => s.InventoryID == item.UserInventoryID);
			int num = 0;
			if (list == null)
			{
				num = item.Quantity;
			}
			else if (list.Count < item.Quantity)
			{
				num = item.Quantity - list.Count;
			}
			for (int j = 0; j < num; j++)
			{
				userItems.Add(item);
			}
		}
	}

	public void SetStableName(string nameString)
	{
		if (!string.IsNullOrEmpty(nameString) && !nameString.Equals(mLocaleName))
		{
			Name = nameString;
			pLocaleName = nameString;
		}
	}

	public static void Init()
	{
		UtDebug.Log("Init stables ... ", 100);
		mIsReady = false;
		pStableList.Clear();
		PairData.Load(2014, PairDataLoadHandler, null);
	}

	public static void SaveData()
	{
		if (mPairData != null)
		{
			string text = UtUtilities.SerializeToXml(pStableList);
			UtDebug.Log("saving stables " + text, 100);
			mPairData.SetValue("NumStables", pStableList.Count.ToString());
			for (int i = 0; i < pStableList.Count; i++)
			{
				mPairData.SetValue("Stable" + i, UtUtilities.SerializeToXml(pStableList[i]));
			}
			mPairData.RemoveByKey("Stables");
			PairData.Save(2014);
		}
		else
		{
			Debug.LogError("Invalid pair data for stables!");
		}
	}

	private static void PairDataLoadHandler(bool success, PairData pData, object inUserData)
	{
		mPairData = pData;
		if (mPairData != null)
		{
			mPairData.Init();
			UtDebug.Log("Deserializing stables data...", 100);
			string value = mPairData.GetValue("Stables");
			int intValue = mPairData.GetIntValue("NumStables", 0);
			UtDebug.Log(value, 100);
			if (intValue > 0)
			{
				pStableList.Clear();
				for (int i = 0; i < intValue; i++)
				{
					if (UtUtilities.DeserializeFromXml(mPairData.GetValue("Stable" + i), typeof(StableData)) is StableData item)
					{
						pStableList.Add(item);
					}
					else
					{
						UtDebug.LogError("Could not load data for stable " + i);
					}
				}
			}
			else if (value != "___VALUE_NOT_FOUND___")
			{
				pStableList.Clear();
				pStableList = UtUtilities.DeserializeFromXml(value, typeof(List<StableData>)) as List<StableData>;
				UtDebug.Log("Stables ready!", 100);
			}
		}
		else
		{
			UtDebug.LogError("Invalid pair data for stables!", 100);
		}
		mIsReady = true;
		UtDebug.Log("Stables initialized!", 100);
	}

	public static void UpdateLocaleNames()
	{
		if (pStableList == null)
		{
			return;
		}
		foreach (StableData pStable in pStableList)
		{
			UserItemData userItemData = CommonInventoryData.pInstance.FindItem(pStable.ItemID);
			if (userItemData == null)
			{
				userItemData = ParentData.pInstance.pInventory.pData.FindItem(pStable.ItemID);
			}
			if (string.IsNullOrEmpty(pStable.Name))
			{
				if (userItemData != null)
				{
					pStable.pLocaleName = userItemData.Item.ItemName;
				}
				else
				{
					ItemData.Load(pStable.ItemID, OnStableItemDataLoaded, pStable);
				}
			}
			else
			{
				pStable.pLocaleName = pStable.Name;
			}
		}
	}

	public static void OnStableItemDataLoaded(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null && inUserData is StableData stableData)
		{
			stableData.pLocaleName = dataItem.ItemName;
		}
	}

	public static List<StableData> GetByInventoryID(int inventoryID)
	{
		List<StableData> result = null;
		if (pStableList != null)
		{
			result = pStableList.FindAll((StableData stable) => stable.InventoryID == inventoryID);
		}
		return result;
	}

	public static StableData GetByID(int id)
	{
		StableData result = null;
		if (pStableList != null)
		{
			result = pStableList.Find((StableData stable) => stable.ID == id);
		}
		return result;
	}

	public static StableData GetByName(string name)
	{
		StableData result = null;
		if (pStableList != null)
		{
			result = pStableList.Find((StableData stable) => stable.pLocaleName == name);
		}
		return result;
	}

	public static List<StableData> GetByType(string stableType)
	{
		if (pStableList != null)
		{
			List<StableData> list = new List<StableData>();
			{
				foreach (StableData pStable in pStableList)
				{
					UserItemData userItemData = CommonInventoryData.pInstance.FindItem(pStable.ItemID);
					if (userItemData == null || userItemData.Item == null || userItemData.Item.Attribute == null)
					{
						continue;
					}
					ItemAttribute[] attribute = userItemData.Item.Attribute;
					foreach (ItemAttribute itemAttribute in attribute)
					{
						if (itemAttribute.Key.Equals("StableType", StringComparison.OrdinalIgnoreCase) && itemAttribute.Value.Equals(stableType, StringComparison.OrdinalIgnoreCase))
						{
							list.Add(pStable);
						}
					}
				}
				return list;
			}
		}
		return null;
	}

	public bool IsType(string stableType)
	{
		UserItemData userItemData = CommonInventoryData.pInstance.FindItem(ItemID);
		bool result = false;
		if (userItemData != null && userItemData.Item != null && userItemData.Item.Attribute != null)
		{
			ItemAttribute[] attribute = userItemData.Item.Attribute;
			foreach (ItemAttribute itemAttribute in attribute)
			{
				if (itemAttribute.Key.Equals("StableType", StringComparison.OrdinalIgnoreCase) && itemAttribute.Value.Equals(stableType, StringComparison.OrdinalIgnoreCase))
				{
					result = true;
					break;
				}
			}
		}
		return result;
	}

	public static StableData GetByPetID(int petID)
	{
		StableData result = null;
		foreach (StableData pStable in pStableList)
		{
			if (pStable.NestList.Find((NestData nd) => nd.PetID == petID) != null)
			{
				result = pStable;
				break;
			}
		}
		return result;
	}

	public static RaisedPetData GetPetInNest(int stableID, int nestID)
	{
		StableData stableData = pStableList.Find((StableData sd) => sd.ID == stableID);
		if (stableData == null)
		{
			return null;
		}
		NestData nestData = stableData.NestList.Find((NestData nd) => nd.ID == nestID);
		if (nestData == null)
		{
			return null;
		}
		return RaisedPetData.GetByID(nestData.PetID);
	}

	private static void AddData(List<UserItemData> userItems)
	{
		foreach (UserItemData userItem in userItems)
		{
			StableData stableData = new StableData();
			stableData.ItemID = userItem.Item.ItemID;
			stableData.InventoryID = userItem.UserInventoryID;
			stableData.ID = pStableList.Count;
			stableData.NestList = new List<NestData>();
			stableData.pLocaleName = userItem.Item.ItemName;
			int attribute = userItem.Item.GetAttribute("NestCount", 2);
			for (int i = 0; i < attribute; i++)
			{
				NestData nestData = new NestData();
				nestData.PetID = 0;
				nestData.ID = i;
				stableData.NestList.Add(nestData);
			}
			pStableList.Add(stableData);
		}
		SaveData();
	}

	public static void AddPetToNest(int stableID, int nestID, int petID)
	{
		bool flag = false;
		StableData byPetID = GetByPetID(petID);
		NestData nestData = null;
		if (byPetID != null)
		{
			nestData = byPetID.GetNestByPetID(petID);
		}
		StableData stableData = pStableList.Find((StableData sd) => sd.ID == stableID);
		if (stableData != null)
		{
			NestData nestByID = stableData.GetNestByID(nestID);
			if (nestByID != null)
			{
				flag = true;
				nestByID.PetID = petID;
				if (nestData != null)
				{
					nestData.PetID = 0;
				}
			}
		}
		if (flag)
		{
			SaveData();
		}
	}

	public NestData GetEmptyNest()
	{
		return NestList.Find((NestData n) => n.PetID == 0);
	}

	public NestData GetNestByPetID(int petID)
	{
		return NestList.Find((NestData n) => n.PetID == petID);
	}

	public NestData GetNestByID(int nestID)
	{
		return NestList.Find((NestData n) => n.ID == nestID);
	}
}
