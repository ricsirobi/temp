using System.Collections.Generic;
using UnityEngine;

public class UiStoreStatPopUp : KAUI
{
	public class ItemInfoDetails
	{
		public ItemData pItemData { get; set; }

		public UserItemData pUserItemData { get; set; }
	}

	public LocaleString _InfoText = new LocaleString("Tier: {x} {rarity}");

	public string _ItemColorWidget = "IconBG";

	public Color _LowColor = Color.red;

	public Color _HighColor = Color.green;

	private KAUIMenu mContentMenu;

	private KAWidget mUpgradeIcon;

	private KAWidget mEquippedIcon;

	private KAWidget mEquippedTierInfo;

	private KAWidget mUpgradeTierInfo;

	private Color mItemDefaultColor = Color.white;

	protected override void Start()
	{
		mContentMenu = _MenuList[0];
		base.Start();
		mEquippedIcon = FindItem("EquippedIcon", recursive: false);
		mUpgradeIcon = FindItem("UpgradeIcon", recursive: false);
		if (mEquippedIcon != null)
		{
			mEquippedTierInfo = mEquippedIcon.FindChildItem("TxtInfo");
		}
		if (mUpgradeIcon != null)
		{
			mUpgradeTierInfo = mUpgradeIcon.FindChildItem("TxtInfo");
		}
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(mEquippedIcon, _ItemColorWidget);
	}

	private void UpdateInfo(KAWidget widget, ItemData itemData, ItemTier? itemTier)
	{
		if (widget != null)
		{
			if (itemTier.HasValue && itemData.ItemRarity.HasValue)
			{
				string localizedString = _InfoText.GetLocalizedString();
				localizedString = localizedString.Replace("{x}", ((int)itemTier.Value).ToString());
				localizedString = localizedString.Replace("{rarity}", InventorySetting.pInstance.GetItemRarityText(itemData.ItemRarity.Value));
				widget.SetText(localizedString);
				widget.SetVisibility(inVisible: true);
			}
			else
			{
				widget.SetText("");
				widget.SetVisibility(inVisible: false);
			}
		}
	}

	private void ShowBattleReadyIcon(KAWidget widget, bool show)
	{
		if (!(widget == null))
		{
			KAWidget kAWidget = widget.FindChildItem("BattleReadyIcon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(show);
			}
		}
	}

	private void ShowFlightReadyIcon(KAWidget widget, bool show)
	{
		if (!(widget == null))
		{
			KAWidget kAWidget = widget.FindChildItem("FlightReadyIcon");
			if (kAWidget != null)
			{
				kAWidget.SetVisibility(show);
			}
		}
	}

	private void UpdateWidgetBackground(KAWidget widget, ItemData itemData, bool isBattleReady)
	{
		KAWidget kAWidget = widget.FindChildItem(_ItemColorWidget);
		if (kAWidget != null)
		{
			if (isBattleReady)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((itemData == null || !itemData.ItemRarity.HasValue) ? ItemRarity.Common : itemData.ItemRarity.Value, kAWidget);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget);
			}
		}
	}

	public void LoadStatCompareIcons(ItemInfoDetails equippedItem, ItemData upgradeItem)
	{
		ClearStatIcons();
		if (mEquippedIcon != null)
		{
			if (equippedItem.pItemData != null)
			{
				mEquippedIcon.SetVisibility(inVisible: true);
				mEquippedIcon.SetTextureFromBundle(equippedItem.pItemData.IconName);
				mEquippedIcon.SetText(equippedItem.pItemData.ItemName);
				bool flag = false;
				ItemTier? itemTier = null;
				if (equippedItem.pUserItemData != null)
				{
					itemTier = equippedItem.pUserItemData.ItemTier;
					flag = equippedItem.pUserItemData.pIsBattleReady;
				}
				else if (equippedItem.pItemData.ItemStatsMap != null && equippedItem.pItemData.ItemStatsMap.ItemStats != null)
				{
					flag = true;
				}
				UpdateInfo(mEquippedTierInfo, equippedItem.pItemData, itemTier);
				UpdateWidgetBackground(mEquippedIcon, equippedItem.pItemData, flag);
				ShowBattleReadyIcon(mEquippedIcon, flag);
				ShowFlightReadyIcon(mEquippedIcon, equippedItem.pItemData.HasAttribute("FlightSuit"));
			}
			else
			{
				mEquippedIcon.SetVisibility(inVisible: false);
			}
		}
		if (!(mUpgradeIcon != null))
		{
			return;
		}
		if (upgradeItem != null)
		{
			mUpgradeIcon.SetVisibility(inVisible: true);
			mUpgradeIcon.SetTextureFromBundle(upgradeItem.IconName);
			mUpgradeIcon.SetText(upgradeItem.ItemName);
			ItemTier? itemTier2 = null;
			if (upgradeItem.ItemStatsMap != null)
			{
				itemTier2 = upgradeItem.ItemStatsMap.ItemTier;
			}
			UpdateInfo(mUpgradeTierInfo, upgradeItem, itemTier2);
			bool flag2 = false;
			if (upgradeItem.ItemStatsMap != null && upgradeItem.ItemStatsMap.ItemStats != null)
			{
				flag2 = true;
			}
			UpdateWidgetBackground(mUpgradeIcon, upgradeItem, flag2);
			ShowBattleReadyIcon(mUpgradeIcon, flag2);
			ShowFlightReadyIcon(mUpgradeIcon, upgradeItem.HasAttribute("FlightSuit"));
		}
		else
		{
			mUpgradeIcon.SetVisibility(inVisible: false);
		}
	}

	public void ClearStatIcons()
	{
		if (mEquippedIcon != null)
		{
			mEquippedIcon.SetTexture(null);
		}
		if (mUpgradeIcon != null)
		{
			mUpgradeIcon.SetTexture(null);
		}
	}

	public void SetStats(List<UiStoreStatCompare.StatDataContainer> statDataList, bool showEquippedStats)
	{
		KAUI.SetExclusive(this);
		mContentMenu.ClearItems();
		for (int i = 0; i < statDataList.Count; i++)
		{
			KAWidget kAWidget = mContentMenu.AddWidget(mContentMenu._Template.name);
			KAWidget kAWidget2 = kAWidget.FindChildItem("StatNameWidget");
			kAWidget2.SetText(statDataList[i]._StatName);
			KAWidget kAWidget3 = kAWidget.FindChildItem("StatDiffWidget");
			kAWidget3.SetText(Mathf.Abs(statDataList[i]._DiffStat).ToString());
			KAWidget kAWidget4 = kAWidget.FindChildItem("StatArrowWidget");
			kAWidget4.SetVisibility(inVisible: true);
			kAWidget4.SetRotation(Quaternion.Euler(0f, 0f, 0f));
			if (statDataList[i]._DiffStat > 0f)
			{
				kAWidget4.pBackground.color = _HighColor;
				kAWidget2.GetLabel().color = _HighColor;
				kAWidget3.GetLabel().color = _HighColor;
			}
			else if (statDataList[i]._DiffStat < 0f)
			{
				kAWidget4.SetRotation(Quaternion.Euler(0f, 0f, 180f));
				kAWidget4.pBackground.color = _LowColor;
				kAWidget2.GetLabel().color = _LowColor;
				kAWidget3.GetLabel().color = _LowColor;
			}
			else
			{
				kAWidget3.GetLabel().color = Color.white;
				kAWidget3.SetText("--");
				kAWidget4.SetVisibility(inVisible: false);
			}
			KAWidget kAWidget5 = kAWidget.FindChildItem("EquippedStatWidget");
			if (showEquippedStats)
			{
				kAWidget5.SetText(statDataList[i]._EquippedStat.ToString());
			}
			else
			{
				kAWidget5.SetText("--");
			}
			kAWidget.FindChildItem("UpgradedStatWidget").SetText(statDataList[i]._ModifiedStat.ToString());
			kAWidget.SetVisibility(inVisible: true);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (inWidget.name == _BackButtonName)
		{
			KAUI.RemoveExclusive(this);
			mContentMenu.ClearItems();
			SetVisibility(inVisible: false);
		}
	}
}
