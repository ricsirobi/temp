using SquadTactics;
using UnityEngine;

public class UiItemStats : KAUI
{
	public LocaleString _InfoText = new LocaleString("Tier: {x} {rarity}");

	public string _ItemColorWidget = "CellBackground";

	private KAWidget mItemName;

	private KAWidget mItemTierInfo;

	private KAWidget mItemIcon;

	private KAWidget mBattleReadyIcon;

	private KAWidget mFlightReadyIcon;

	private KAWidget mItemSellPrice;

	private KAWidget mItemShardsTotal;

	private KAUIMenu mItemStatsMenu;

	private Color mItemDefaultColor = Color.white;

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
		mItemTierInfo = FindItem("ItemTierInfo");
		mItemIcon = FindItem("ItemIcon");
		mBattleReadyIcon = FindItem("BattleReadyIcon");
		mFlightReadyIcon = FindItem("FlightReadyIcon");
		mItemSellPrice = FindItem("ItemSellPrice");
		mItemShardsTotal = FindItem("ItemShardsCount");
		mItemStatsMenu = _MenuList[0];
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(mItemIcon, _ItemColorWidget);
		mInitialized = true;
	}

	public void ShowStats(ItemData itemData)
	{
		if (!mInitialized)
		{
			Initialized();
		}
		if (mItemName != null)
		{
			mItemName.SetText(itemData.ItemName);
		}
		if (mItemIcon != null)
		{
			mItemIcon.SetTextureFromBundle(itemData.IconName, base.gameObject);
			mItemIcon.SetVisibility(inVisible: true);
			if (mBattleReadyIcon != null)
			{
				mBattleReadyIcon.SetVisibility(inVisible: true);
			}
			if (mFlightReadyIcon != null)
			{
				mFlightReadyIcon.SetVisibility(itemData.HasAttribute("FlightSuit"));
			}
			KAWidget kAWidget = mItemIcon.FindChildItem(_ItemColorWidget);
			if (kAWidget != null)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((!itemData.ItemRarity.HasValue) ? ItemRarity.Common : itemData.ItemRarity.Value, kAWidget);
			}
		}
		if (mItemTierInfo != null)
		{
			string localizedString = _InfoText.GetLocalizedString();
			localizedString = localizedString.Replace("{x}", "");
			localizedString = localizedString.Replace("{rarity}", InventorySetting.pInstance.GetItemRarityText(itemData.ItemRarity.Value));
			mItemTierInfo.SetText(localizedString);
			mItemTierInfo.SetVisibility(inVisible: true);
		}
		mItemStatsMenu.ClearItems();
		SetVisibility(inVisible: true);
	}

	public void ShowStats(UserItemData userItemData, Texture inTexture = null, int price = 0, int shards = 0)
	{
		if (!mInitialized)
		{
			Initialized();
		}
		mUserItemData = userItemData;
		if (mItemName != null)
		{
			mItemName.SetText(userItemData.Item.ItemName);
		}
		if (mItemTierInfo != null)
		{
			if (userItemData.ItemTier.HasValue && userItemData.Item.ItemRarity.HasValue)
			{
				string localizedString = _InfoText.GetLocalizedString();
				localizedString = localizedString.Replace("{x}", ((int)userItemData.ItemTier.Value).ToString());
				localizedString = localizedString.Replace("{rarity}", InventorySetting.pInstance.GetItemRarityText(userItemData.Item.ItemRarity.Value));
				mItemTierInfo.SetText(localizedString);
				mItemTierInfo.SetVisibility(inVisible: true);
			}
			else
			{
				mItemTierInfo.SetVisibility(inVisible: false);
			}
		}
		if (mItemSellPrice != null)
		{
			mItemSellPrice.SetVisibility(price > 0);
			mItemSellPrice.SetText(price.ToString());
		}
		if (mItemShardsTotal != null)
		{
			mItemShardsTotal.SetVisibility(shards > 0);
			mItemShardsTotal.SetText(shards.ToString());
		}
		if (mItemIcon != null)
		{
			if (inTexture == null && mUserItemData != null)
			{
				mItemIcon.SetTextureFromBundle(mUserItemData.Item.IconName, base.gameObject);
				mItemIcon.SetVisibility(inVisible: false);
			}
			else if (inTexture != null)
			{
				mItemIcon.SetTexture(inTexture);
				mItemIcon.SetVisibility(inVisible: true);
			}
			else
			{
				mItemIcon.SetVisibility(inVisible: false);
			}
			if (mBattleReadyIcon != null)
			{
				mBattleReadyIcon.SetVisibility(mUserItemData.pIsBattleReady);
			}
			if (mFlightReadyIcon != null)
			{
				mFlightReadyIcon.SetVisibility(mUserItemData.Item.HasAttribute("FlightSuit"));
			}
			KAWidget kAWidget = mItemIcon.FindChildItem(_ItemColorWidget);
			if (kAWidget != null)
			{
				if (mUserItemData.pIsBattleReady)
				{
					UiItemRarityColorSet.SetItemBackgroundColor((!mUserItemData.Item.ItemRarity.HasValue) ? ItemRarity.Common : mUserItemData.Item.ItemRarity.Value, kAWidget);
				}
				else
				{
					UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget);
				}
			}
		}
		UpdateStats();
		SetVisibility(inVisible: true);
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		inWidget.SetVisibility(inVisible: true);
	}

	public void ShowPossibleStats(UserItemData userItemData)
	{
		mUserItemData = userItemData;
		UpdatePossibleStats();
	}

	public void UpdateStats()
	{
		mItemStatsMenu.ClearItems();
		ItemStat[] itemStats = mUserItemData.ItemStats;
		if (itemStats != null && itemStats.Length != 0 && mUserItemData.ItemStats != null && mUserItemData.Item.PossibleStatsMap != null && mUserItemData.ItemStats.Length != 0)
		{
			ItemStat[] array = itemStats;
			foreach (ItemStat itemStat in array)
			{
				AddStat(itemStat);
			}
		}
	}

	private void UpdatePossibleStats()
	{
		mItemStatsMenu.ClearItems();
		if (mUserItemData.Item.PossibleStatsMap == null || mUserItemData.Item.PossibleStatsMap.Stats == null)
		{
			return;
		}
		foreach (Stat stat in mUserItemData.Item.PossibleStatsMap.Stats)
		{
			AddPossibleStat(stat);
		}
	}

	private void AddStat(ItemStat itemStat)
	{
		KAWidget kAWidget = mItemStatsMenu.AddWidget(itemStat.Name);
		kAWidget.SetUserDataInt(itemStat.ItemStatID);
		KAWidget kAWidget2 = kAWidget.FindChildItem("Name");
		if (kAWidget2 != null && Settings.pInstance != null)
		{
			kAWidget2.SetText(Settings.pInstance.GetStatEffectName(itemStat.ItemStatID));
		}
		KAWidget kAWidget3 = kAWidget.FindChildItem("CurrentValue");
		StStatInfo statInfoByID = Settings.pInstance.GetStatInfoByID(itemStat.ItemStatID);
		int result = 0;
		int.TryParse(itemStat.Value, out result);
		if (statInfoByID._Stat == SquadTactics.Stat.HEALTH)
		{
			result *= (int)statInfoByID._Value;
		}
		if (kAWidget3 != null)
		{
			kAWidget3.SetText(result.ToString());
		}
		KAWidget kAWidget4 = kAWidget.FindChildItem("Range");
		string text = "-";
		foreach (Stat stat in mUserItemData.Item.PossibleStatsMap.Stats)
		{
			if (stat.ItemStatsID == itemStat.ItemStatID && mUserItemData.ItemTier.HasValue)
			{
				StatRangeMap statRangeMap = GetStatRangeMap(stat, (int)mUserItemData.ItemTier.Value);
				StStatInfo statInfoByID2 = Settings.pInstance.GetStatInfoByID(stat.ItemStatsID);
				text = ((statInfoByID2._Stat != SquadTactics.Stat.HEALTH) ? ((statRangeMap == null) ? "-" : (statRangeMap.StartRange + "-" + statRangeMap.EndRange)) : ((statRangeMap == null) ? "-" : ((float)statRangeMap.StartRange * statInfoByID2._Value + "-" + (float)statRangeMap.EndRange * statInfoByID2._Value)));
				break;
			}
		}
		if (kAWidget4 != null)
		{
			kAWidget4.SetText(text);
		}
		kAWidget.SetVisibility(inVisible: true);
	}

	private void AddPossibleStat(Stat stat)
	{
		KAWidget kAWidget = mItemStatsMenu.AddWidget(stat.ItemStatsID.ToString());
		KAWidget kAWidget2 = kAWidget.FindChildItem("Range");
		StatRangeMap statRangeMap = GetStatRangeMap(stat, (int)mUserItemData.ItemTier.Value);
		StStatInfo statInfoByID = Settings.pInstance.GetStatInfoByID(stat.ItemStatsID);
		string text = "";
		text = ((statInfoByID._Stat != SquadTactics.Stat.HEALTH) ? ((statRangeMap == null) ? "-" : (statRangeMap.StartRange + "-" + statRangeMap.EndRange)) : ((statRangeMap == null) ? "-" : ((float)statRangeMap.StartRange * statInfoByID._Value + "-" + (float)statRangeMap.EndRange * statInfoByID._Value)));
		if (kAWidget2 != null)
		{
			kAWidget2.SetText(text);
		}
		KAWidget kAWidget3 = kAWidget.FindChildItem("Name");
		if (kAWidget3 != null && Settings.pInstance != null)
		{
			kAWidget3.SetText(Settings.pInstance.GetStatEffectName(stat.ItemStatsID));
		}
		kAWidget.SetVisibility(inVisible: true);
	}

	private StatRangeMap GetStatRangeMap(Stat possibleStat, int tier)
	{
		return possibleStat.ItemStatsRangeMaps.Find((StatRangeMap m) => m.ItemTierID == tier);
	}
}
