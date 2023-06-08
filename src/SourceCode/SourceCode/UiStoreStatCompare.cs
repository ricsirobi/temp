using System;
using System.Collections.Generic;
using System.Linq;
using SquadTactics;
using UnityEngine;

public class UiStoreStatCompare : KAUI
{
	public class StatDataContainer
	{
		public string _StatName;

		public string _AbvStatName;

		public float _EquippedStat;

		public float _ModifiedStat;

		public float _DiffStat;

		public int _StatID;
	}

	private UiStoreStatPopUp mUiStatPopUp;

	private KAUIMenu mContentMenu;

	private KAWidget mPopUpBtn;

	public int mStoreID = 95;

	private StoreData mStoreData;

	private List<StatDataContainer> mStatDataList;

	private ItemStat[] mUpgradedItemStat;

	private ItemStat[] mEquippedItemStat;

	private ItemData mEquippedItem;

	private UserItemData mUserItemDataforPart;

	private bool mShowEquippedStats;

	protected override void Start()
	{
		base.Start();
		mPopUpBtn = FindItem("BtnShowStats");
		mContentMenu = _MenuList[0];
		mUiStatPopUp = (UiStoreStatPopUp)_UiList[0];
		ItemStoreDataLoader.Load(mStoreID, OnStoreLoaded);
	}

	public void OnStoreLoaded(StoreData sd)
	{
		mStoreData = sd;
	}

	public void UpdateStatsCompareData(int previewIndex, List<PreviewItemData> previewList)
	{
		mContentMenu.ClearItems();
		mEquippedItem = null;
		mUserItemDataforPart = null;
		mShowEquippedStats = false;
		mStatDataList = new List<StatDataContainer>();
		mEquippedItemStat = AvatarData.pInstanceInfo.GetPartsCombinedStats();
		ItemData itemData = null;
		itemData = ((previewIndex != -1) ? previewList[previewIndex].pItemData : previewList[0].pItemData);
		mUpgradedItemStat = GetUpgradedPartsCombinedStats(previewIndex, previewList);
		CharacterData characterData = new CharacterData(CharacterDatabase.pInstance.GetAvatar());
		characterData._Stats.SetInitialValues(UserRankData.pInstance.RankID, mEquippedItemStat);
		CharacterData characterData2 = new CharacterData(CharacterDatabase.pInstance.GetAvatar());
		characterData2._Stats.SetInitialValues(UserRankData.pInstance.RankID, mUpgradedItemStat);
		List<StStatInfo> list = Settings.pInstance._StatInfo.OrderBy((StStatInfo x) => x._StatID).ToList();
		for (int i = 0; i < list.Count; i++)
		{
			if (!list[i]._Display)
			{
				continue;
			}
			StatDataContainer statDataContainer = new StatDataContainer();
			statDataContainer._StatName = list[i]._DisplayText.GetLocalizedString();
			statDataContainer._AbvStatName = list[i]._AbbreviationText.GetLocalizedString();
			if (list[i]._Stat == SquadTactics.Stat.CRITICALCHANCE || list[i]._Stat == SquadTactics.Stat.DODGE)
			{
				statDataContainer._EquippedStat = characterData._Stats.GetStat(list[i]._Stat).GetMultipliedValue();
				statDataContainer._ModifiedStat = characterData2._Stats.GetStat(list[i]._Stat).GetMultipliedValue();
			}
			else
			{
				statDataContainer._EquippedStat = characterData._Stats.GetStat(list[i]._Stat).pCurrentValue;
				statDataContainer._ModifiedStat = characterData2._Stats.GetStat(list[i]._Stat).pCurrentValue;
			}
			statDataContainer._DiffStat = statDataContainer._ModifiedStat - statDataContainer._EquippedStat;
			mStatDataList.Add(statDataContainer);
			int previewEquippedStat = GetPreviewEquippedStat(previewIndex, list[i]._StatID, previewList);
			int previewModifiedStat = GetPreviewModifiedStat(previewIndex, list[i]._StatID, previewList, itemData);
			if (previewEquippedStat != 0 || previewModifiedStat != 0)
			{
				KAWidget kAWidget = mContentMenu.AddWidget(mContentMenu._Template.name);
				kAWidget.FindChildItem("AbvStatWidget").SetText(statDataContainer._AbvStatName);
				KAWidget kAWidget2 = kAWidget.FindChildItem("StatDiffWidget");
				string text = Mathf.Abs(previewEquippedStat - previewModifiedStat).ToString();
				if (previewEquippedStat == previewModifiedStat)
				{
					text = previewEquippedStat.ToString();
				}
				kAWidget2.SetText(text);
				KAWidget kAWidget3 = kAWidget.FindChildItem("ArrowWidget");
				kAWidget3.SetVisibility(inVisible: true);
				kAWidget3.SetRotation(Quaternion.Euler(0f, 0f, 0f));
				if (statDataContainer._DiffStat == 0f)
				{
					kAWidget3.SetVisibility(inVisible: false);
				}
				else if (statDataContainer._DiffStat < 0f)
				{
					kAWidget3.pBackground.color = Color.red;
					kAWidget3.SetRotation(Quaternion.Euler(0f, 0f, 180f));
				}
				else
				{
					kAWidget3.pBackground.color = Color.green;
				}
				kAWidget.SetVisibility(inVisible: true);
			}
		}
		if (mEquippedItem != null)
		{
			mShowEquippedStats = true;
		}
		if (previewIndex == -1 && CheckMultiItemList(previewList))
		{
			mShowEquippedStats = true;
			mEquippedItem = null;
			itemData = null;
		}
		UiStoreStatPopUp.ItemInfoDetails itemInfoDetails = new UiStoreStatPopUp.ItemInfoDetails();
		itemInfoDetails.pItemData = mEquippedItem;
		itemInfoDetails.pUserItemData = mUserItemDataforPart;
		mUiStatPopUp.LoadStatCompareIcons(itemInfoDetails, itemData);
		mPopUpBtn.SetVisibility(inVisible: true);
	}

	private ItemStat[] GetUpgradedPartsCombinedStats(int previewItemIndex, List<PreviewItemData> previewList)
	{
		List<ItemStat> list = new List<ItemStat>();
		AvatarDataPart[] part = AvatarData.pInstance.Part;
		string text = null;
		for (int j = 0; j < part.Length; j++)
		{
			ItemStat[] partStats = null;
			int num = 0;
			ItemData itemData = null;
			bool flag = false;
			if (previewItemIndex == -1)
			{
				itemData = GetItemInPreviewList(part[j].PartType, previewList);
				if (itemData != null)
				{
					flag = true;
				}
			}
			else
			{
				itemData = previewList[previewItemIndex].pItemData;
				string partName = AvatarData.GetPartName(itemData);
				if (part[j].PartType.Equals(partName) || part[j].PartType.Equals("DEFAULT_" + partName))
				{
					flag = true;
				}
			}
			if (flag)
			{
				text = AvatarData.GetPartName(itemData);
				if (part[j].UserInventoryId.HasValue && part[j].UserInventoryId.Value <= 0 && mStoreData != null && part[j].Textures != null && part[j].Textures.Length != 0)
				{
					mEquippedItem = mStoreData.FindItem(part[j].Textures[0], AvatarData.GetCategoryID(part[j].PartType));
				}
				if (part[j].UserInventoryId.HasValue && part[j].UserInventoryId.Value > 0)
				{
					num = part[j].UserInventoryId.Value;
					mUserItemDataforPart = CommonInventoryData.pInstance.FindItemByUserInventoryID(num);
					if (mUserItemDataforPart != null)
					{
						mEquippedItem = mUserItemDataforPart.Item;
					}
					if (part[j].PartType.Equals(text) && itemData.ItemStatsMap != null)
					{
						partStats = itemData.ItemStatsMap.ItemStats;
					}
				}
				else if (part[j].PartType.Equals(text) && itemData.ItemStatsMap != null)
				{
					partStats = itemData.ItemStatsMap.ItemStats;
				}
			}
			else if (part[j].UserInventoryId.HasValue && part[j].UserInventoryId.Value > 0)
			{
				num = part[j].UserInventoryId.Value;
				partStats = CommonInventoryData.pInstance.GetItemStatsByUserInventoryID(num);
			}
			if (partStats == null || partStats.Length == 0)
			{
				continue;
			}
			int i;
			for (i = 0; i < partStats.Length; i++)
			{
				ItemStat itemStat = list.Find((ItemStat x) => x.ItemStatID == partStats[i].ItemStatID);
				if (itemStat != null)
				{
					int result = 0;
					int result2 = 0;
					int.TryParse(partStats[i].Value, out result2);
					int.TryParse(itemStat.Value, out result);
					itemStat.Value = (result2 + result).ToString();
				}
				else
				{
					list.Add(new ItemStat(partStats[i]));
				}
			}
		}
		return list.ToArray();
	}

	private ItemData GetItemInPreviewList(string inPartType, List<PreviewItemData> previewList)
	{
		foreach (PreviewItemData preview in previewList)
		{
			if (preview != null)
			{
				string partName = AvatarData.GetPartName(preview.pItemData);
				if (inPartType.Equals(partName) || inPartType.Equals("DEFAULT_" + partName))
				{
					return preview.pItemData;
				}
			}
		}
		return null;
	}

	private string GetItemStat(int statID, UserItemData userItemData)
	{
		ItemStat itemStat = Array.Find(userItemData.ItemStats, (ItemStat x) => x.ItemStatID == statID);
		if (itemStat != null)
		{
			return itemStat.Value;
		}
		return "0";
	}

	private string GetItemStat(int statID, ItemData itemData)
	{
		if (itemData != null && itemData.ItemStatsMap != null && itemData.ItemStatsMap.ItemStats != null)
		{
			for (int i = 0; i < itemData.ItemStatsMap.ItemStats.Length; i++)
			{
				if (statID == itemData.ItemStatsMap.ItemStats[i].ItemStatID)
				{
					return itemData.ItemStatsMap.ItemStats[i].Value;
				}
			}
		}
		return "0";
	}

	private bool CheckMultiItemList(List<PreviewItemData> previewList)
	{
		int num = 0;
		foreach (PreviewItemData preview in previewList)
		{
			if (preview != null)
			{
				num++;
			}
		}
		if (num > 1)
		{
			return true;
		}
		return false;
	}

	private int GetPreviewEquippedStat(int previewIndex, int statID, List<PreviewItemData> previewList)
	{
		int num = 0;
		if (previewIndex == -1 && CheckMultiItemList(previewList))
		{
			num = GetTotalStatsFromAvatar(statID, previewList);
		}
		else if (mUserItemDataforPart != null && mUserItemDataforPart.ItemStats != null)
		{
			num = int.Parse(GetItemStat(statID, mUserItemDataforPart));
		}
		StStatInfo statInfoByID = Settings.pInstance.GetStatInfoByID(statID);
		if (statInfoByID._Stat == SquadTactics.Stat.HEALTH)
		{
			num *= (int)statInfoByID._Value;
		}
		return num;
	}

	private int GetPreviewModifiedStat(int previewIndex, int statID, List<PreviewItemData> previewList, ItemData itemdata)
	{
		int num = 0;
		num = ((previewIndex != -1 || !CheckMultiItemList(previewList)) ? int.Parse(GetItemStat(statID, itemdata)) : GetTotalStatsFromPreviewList(statID, previewList));
		StStatInfo statInfoByID = Settings.pInstance.GetStatInfoByID(statID);
		if (statInfoByID._Stat == SquadTactics.Stat.HEALTH)
		{
			num *= (int)statInfoByID._Value;
		}
		return num;
	}

	public void RemoveStatPreview()
	{
		SetVisibility(inVisible: false);
		mStatDataList = null;
	}

	private int GetTotalStatsFromPreviewList(int statId, List<PreviewItemData> previewList)
	{
		ItemStat[] array = null;
		int num = 0;
		for (int i = 0; i < previewList.Count; i++)
		{
			if (previewList[i].pItemData == null || previewList[i].pItemData.ItemStatsMap == null || previewList[i].pItemData.ItemStatsMap.ItemStats == null)
			{
				continue;
			}
			array = previewList[i].pItemData.ItemStatsMap.ItemStats;
			if (array == null)
			{
				continue;
			}
			ItemStat[] array2 = Array.FindAll(array, (ItemStat x) => x.ItemStatID == statId);
			if (array2 != null)
			{
				ItemStat[] array3 = array2;
				for (int j = 0; j < array3.Length; j++)
				{
					int.TryParse(array3[j].Value, out var result);
					num += result;
				}
			}
		}
		return num;
	}

	private int GetTotalStatsFromAvatar(int statId, List<PreviewItemData> previewList)
	{
		AvatarDataPart[] part = AvatarData.pInstance.Part;
		List<AvatarDataPart> list = new List<AvatarDataPart>();
		for (int i = 0; i < previewList.Count; i++)
		{
			string text = null;
			if (previewList[i].pItemData != null)
			{
				text = AvatarData.GetPartName(previewList[i].pItemData);
			}
			for (int j = 0; j < part.Length; j++)
			{
				if (text.Equals(part[j].PartType) || part[j].PartType.Equals("DEFAULT_" + text))
				{
					list.Add(part[j]);
				}
			}
		}
		ItemStat[] partsCombinedStats = AvatarData.pInstanceInfo.GetPartsCombinedStats(list.ToArray());
		int result = 0;
		ItemStat itemStat = Array.Find(partsCombinedStats, (ItemStat x) => x.ItemStatID == statId);
		if (itemStat != null)
		{
			int.TryParse(itemStat.Value, out result);
		}
		return result;
	}

	public bool ShowStatsOnCategory(ItemData itemdata)
	{
		if (InventorySetting.pInstance._TabData == null)
		{
			return false;
		}
		InventorySetting.TabData tabData = InventorySetting.pInstance._TabData.Find((InventorySetting.TabData x) => x._TabID == "BattleReadyItems");
		if (tabData != null && tabData._Categories != null)
		{
			for (int i = 0; i < tabData._Categories.Length; i++)
			{
				if (itemdata.HasCategory(tabData._Categories[i]))
				{
					return true;
				}
			}
		}
		return false;
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget == mPopUpBtn)
		{
			mUiStatPopUp.SetVisibility(inVisible: true);
			mUiStatPopUp.SetStats(mStatDataList, mShowEquippedStats);
		}
	}
}
