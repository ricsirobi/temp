using System.Collections.Generic;
using UnityEngine;

public class ItemCustomizationSettings : ScriptableObject
{
	[SerializeField]
	private List<ItemCustomizationUiData> _UiDataList;

	private static ItemCustomizationSettings mInstance;

	public static ItemCustomizationSettings pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (ItemCustomizationSettings)RsResourceManager.LoadAssetFromResources("ItemCustomizationSettings.asset", isPrefab: false);
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<ItemCustomizationSettings>();
				}
			}
			return mInstance;
		}
	}

	public ItemCustomizationData[] GetCustomizationConfig(ItemData itemData)
	{
		ItemCustomizationUiData uiData = GetUiData(itemData);
		if (uiData != null)
		{
			List<ItemCustomizationData> list = new List<ItemCustomizationData>();
			ItemCustomizationData[] customizationConfigArray = uiData._CustomizationConfigArray;
			foreach (ItemCustomizationData itemCustomizationData in customizationConfigArray)
			{
				list.Add(itemCustomizationData.GetCopy());
			}
			return list.ToArray();
		}
		return null;
	}

	public ItemCustomizationData[] GetCustomizationConfig(int categoryID)
	{
		ItemCustomizationUiData uiData = GetUiData(categoryID);
		if (uiData != null)
		{
			List<ItemCustomizationData> list = new List<ItemCustomizationData>();
			ItemCustomizationData[] customizationConfigArray = uiData._CustomizationConfigArray;
			foreach (ItemCustomizationData itemCustomizationData in customizationConfigArray)
			{
				list.Add(itemCustomizationData.GetCopy());
			}
			return list.ToArray();
		}
		return null;
	}

	public string GetResourcePath(ItemData itemData)
	{
		return GetUiData(itemData)?._ResourcePath;
	}

	public string GetResourcePath(int categoryID)
	{
		return GetUiData(categoryID)?._ResourcePath;
	}

	public Vector3 GetThumbnailCameraOffset(ItemData itemData)
	{
		return GetUiData(itemData)?._ThumbnailCameraOffset ?? Vector3.zero;
	}

	public Vector3 GetThumbnailCameraOffset(int categoryID)
	{
		return GetUiData(categoryID)?._ThumbnailCameraOffset ?? Vector3.zero;
	}

	public ItemCustomizationUiData GetUiData(ItemData itemData)
	{
		ItemCustomizationUiData result = null;
		int[] customizableCategories = GetCustomizableCategories();
		foreach (int categoryID in customizableCategories)
		{
			if (itemData.HasCategory(categoryID))
			{
				result = GetUiData(categoryID);
				break;
			}
		}
		return result;
	}

	private ItemCustomizationUiData GetUiData(int categoryID)
	{
		if (_UiDataList != null && _UiDataList.Count > 0)
		{
			return _UiDataList.Find((ItemCustomizationUiData data) => data._ItemCategoryID == categoryID);
		}
		return null;
	}

	public int[] GetCustomizableCategories()
	{
		int[] array = new int[_UiDataList.Count];
		for (int i = 0; i < _UiDataList.Count; i++)
		{
			array[i] = _UiDataList[i]._ItemCategoryID;
		}
		return array;
	}

	public bool IsCustomizableItem(ItemData itemData)
	{
		if (itemData == null)
		{
			return false;
		}
		int[] customizableCategories = GetCustomizableCategories();
		foreach (int categoryID in customizableCategories)
		{
			if (itemData.HasCategory(categoryID))
			{
				return true;
			}
		}
		return false;
	}

	public int GetCustomizableCategory(ItemData itemData)
	{
		if (itemData == null)
		{
			return 0;
		}
		int[] customizableCategories = GetCustomizableCategories();
		foreach (int num in customizableCategories)
		{
			if (itemData.HasCategory(num))
			{
				return num;
			}
		}
		return 0;
	}

	public ItemCustomizationUiData GetItemCustomizationUiData(int categoryId, bool multiItemCustomizationUI)
	{
		for (int i = 0; i < _UiDataList.Count; i++)
		{
			if (_UiDataList[i]._ItemCategoryID == categoryId && _UiDataList[i]._MultiItemCustomizationUI == multiItemCustomizationUI)
			{
				return _UiDataList[i];
			}
		}
		return null;
	}

	public ItemCustomizationUiData GetItemCustomizationUiData(ItemData itemData, bool multiItemCustomizationUI)
	{
		for (int i = 0; i < _UiDataList.Count; i++)
		{
			if (itemData.HasCategory(_UiDataList[i]._ItemCategoryID) && _UiDataList[i]._MultiItemCustomizationUI == multiItemCustomizationUI)
			{
				return _UiDataList[i];
			}
		}
		return null;
	}

	public bool MultiItemPays(ItemData itemData)
	{
		return GetItemCustomizationUiData(itemData, multiItemCustomizationUI: true)?._PaidCustomization ?? false;
	}
}
