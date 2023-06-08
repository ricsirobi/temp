using System;
using UnityEngine;

public class EquipmentCheck : ScriptableObject
{
	[Serializable]
	public class EquipmentCheckInfo
	{
		[Serializable]
		public class CategoryMap
		{
			public int _CategoryID = -1;

			public string _PartType;
		}

		public string _SceneName;

		public CategoryMap[] _CategoryMap;

		public LocaleString _EquipmentNeededText = new LocaleString("You need some equipment to enter.");
	}

	private static EquipmentCheck mInstance;

	public EquipmentCheckInfo[] _EquippedItemsInfo;

	public static EquipmentCheck pInstance
	{
		get
		{
			if (mInstance == null)
			{
				mInstance = (EquipmentCheck)RsResourceManager.LoadAssetFromResources("EquipmentCheck.asset", isPrefab: false);
				if (mInstance == null)
				{
					mInstance = ScriptableObject.CreateInstance<EquipmentCheck>();
				}
			}
			return mInstance;
		}
	}

	public bool CheckEquippedItemsCriteria(string sceneName, ref LocaleString text)
	{
		for (int i = 0; i < _EquippedItemsInfo.Length; i++)
		{
			if (!string.Equals(_EquippedItemsInfo[i]._SceneName, sceneName))
			{
				continue;
			}
			text = _EquippedItemsInfo[i]._EquipmentNeededText;
			if (_EquippedItemsInfo[i]._CategoryMap.Length == 0)
			{
				return true;
			}
			EquipmentCheckInfo.CategoryMap[] categoryMap = _EquippedItemsInfo[i]._CategoryMap;
			foreach (EquipmentCheckInfo.CategoryMap categoryMap2 in categoryMap)
			{
				if (!AvatarData.HasItemOfCategory(categoryMap2._CategoryID, categoryMap2._PartType))
				{
					return false;
				}
			}
		}
		return true;
	}
}
