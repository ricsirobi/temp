using System;
using System.Collections.Generic;
using System.Xml.Serialization;

[Serializable]
[XmlRoot(ElementName = "IAPStoreData", Namespace = "")]
public class IAPStoreData
{
	[XmlElement(ElementName = "CategoryData")]
	public IAPItemCategory[] CategoryData;

	public IAPItemData[] GetDataListInCategory(int inType)
	{
		IAPItemCategory[] categoryData = CategoryData;
		foreach (IAPItemCategory iAPItemCategory in categoryData)
		{
			if (iAPItemCategory.CategoryType == (IAPCategoryType)inType)
			{
				return iAPItemCategory.ItemDataList;
			}
		}
		return null;
	}

	public string[] GetListOfProductIDs()
	{
		List<string> list = new List<string>();
		IAPItemCategory[] categoryData = CategoryData;
		for (int i = 0; i < categoryData.Length; i++)
		{
			IAPItemData[] itemDataList = categoryData[i].ItemDataList;
			foreach (IAPItemData iAPItemData in itemDataList)
			{
				list.Add(iAPItemData.AppStoreID);
			}
		}
		return list.ToArray();
	}

	public IAPCategoryType GetIAPCategoryType(string iapItemID)
	{
		IAPItemCategory[] categoryData = CategoryData;
		foreach (IAPItemCategory iAPItemCategory in categoryData)
		{
			IAPItemData[] itemDataList = iAPItemCategory.ItemDataList;
			foreach (IAPItemData iAPItemData in itemDataList)
			{
				if (iapItemID.Equals(iAPItemData.AppStoreID))
				{
					return iAPItemCategory.CategoryType;
				}
			}
		}
		return IAPCategoryType.Unknown;
	}

	public string GetIAPCategoryName(string iapItemID)
	{
		IAPItemCategory[] categoryData = CategoryData;
		foreach (IAPItemCategory iAPItemCategory in categoryData)
		{
			IAPItemData[] itemDataList = iAPItemCategory.ItemDataList;
			foreach (IAPItemData iAPItemData in itemDataList)
			{
				if (iapItemID.Equals(iAPItemData.AppStoreID))
				{
					return iAPItemCategory.CategoryName;
				}
			}
		}
		return "";
	}

	public bool GetRecurring(string iapItemID)
	{
		IAPItemCategory[] categoryData = CategoryData;
		for (int i = 0; i < categoryData.Length; i++)
		{
			IAPItemData[] itemDataList = categoryData[i].ItemDataList;
			foreach (IAPItemData iAPItemData in itemDataList)
			{
				if (iapItemID.Equals(iAPItemData.AppStoreID))
				{
					return iAPItemData.Recurring;
				}
			}
		}
		return false;
	}
}
