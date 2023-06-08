using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UiFuseInfo : KAUISelect
{
	[Serializable]
	public class CategoryData
	{
		public int _CatergoryID = -1;

		public Texture _Icon;
	}

	public LocaleString _InfoText = new LocaleString("Tier: {x} {rarity}");

	public LocaleString _QuantityText = new LocaleString("[REVIEW] Quantity: {x} ");

	public LocaleString _IncorrectText = new LocaleString("[REVIEW] Incorrect Item Added ");

	public string _ItemColorWidget = "CellBackground";

	public UiFuse _UiFuse;

	public float _MessageDuration = 1f;

	public UiBlacksmith _UiBlackSmith;

	public List<CategoryData> _CategoryInfo;

	private KAWidget mItemName;

	private KAWidget mItemTier;

	private KAWidget mItemIcon;

	private KAWidget mItemSellPrice;

	private KAWidget mItemShardsTotal;

	private KAWidget mTxtCost;

	private Color mItemDefaultColor = Color.white;

	private int mIngredientsCounter;

	public KAUISelectItemData BlueprintWidgetData { get; set; }

	protected override void Start()
	{
		mItemName = FindItem("ItemName");
		mItemTier = FindItem("ItemTier");
		mItemIcon = FindItem("ItemIcon");
		mItemSellPrice = FindItem("ItemSellPrice");
		mItemShardsTotal = FindItem("ItemShardsCount");
		mTxtCost = FindItem("TxtCost");
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(mItemIcon, _ItemColorWidget);
		base.Start();
	}

	public override void OnOpen()
	{
	}

	public override void Initialize()
	{
	}

	public override void AddWidgetData(KAWidget inWidget, KAUISelectItemData widgetData)
	{
		base.AddWidgetData(inWidget, widgetData);
		_UiFuse.EnableFuseButton(CanEnableFuse());
	}

	public override void SelectItem(KAWidget inWidget)
	{
		base.SelectItem(inWidget);
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)inWidget.GetUserData();
		KAUISelectItemData kAUISelectItemData2 = (KAUISelectItemData)inWidget.pChildWidgets[0].GetUserData();
		if (kAUISelectItemData != null && kAUISelectItemData._ItemID != 0)
		{
			_UiFuse.ShowItemStats(inWidget);
		}
		else if (kAUISelectItemData2 != null && kAUISelectItemData2._ItemID != 0)
		{
			_UiFuse.ShowItemStats(inWidget.pChildWidgets[0]);
		}
	}

	public void ShowIncorrectItemDB()
	{
		StartCoroutine(ShowTimedMessage(_IncorrectText.GetLocalizedString()));
	}

	private IEnumerator ShowTimedMessage(string message)
	{
		KAUIGenericDB genericDB = GameUtilities.DisplayGenericDB("PfKAUIGenericDB", message, "", null, "", "", "", "");
		yield return new WaitForSeconds(_MessageDuration);
		if (genericDB != null)
		{
			UnityEngine.Object.Destroy(genericDB.gameObject);
		}
	}

	public UiFuse.FuseInteraction ValidateDrop(KAWidget targetWidget, KAUISelectItemData widgetData)
	{
		if (((KAUISelectItemData)targetWidget.GetUserData())._ItemID != 0)
		{
			return UiFuse.FuseInteraction.SlotOccupied;
		}
		KAWidget kAWidget = targetWidget.pChildWidgets[0];
		if (targetWidget.GetState() != 0)
		{
			return UiFuse.FuseInteraction.SlotDisabled;
		}
		if (kAWidget != null)
		{
			KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)kAWidget.GetUserData();
			if (kAUISelectItemData._ItemID > 0 && kAUISelectItemData._ItemID != widgetData._ItemID)
			{
				return UiFuse.FuseInteraction.InvalidItem;
			}
			if (kAUISelectItemData._ItemData.ItemStatsMap != null && kAUISelectItemData._ItemData.ItemStatsMap.ItemTier != widgetData._UserItemData.ItemTier)
			{
				return UiFuse.FuseInteraction.InvalidItem;
			}
			if (kAUISelectItemData._ItemData.ItemRarity.HasValue && widgetData._ItemData.ItemRarity.Value != kAUISelectItemData._ItemData.ItemRarity.Value)
			{
				return UiFuse.FuseInteraction.InvalidItem;
			}
			if (widgetData._ItemData.Category != null && kAUISelectItemData._ItemData.Category != null)
			{
				bool flag = false;
				ItemDataCategory[] category = widgetData._ItemData.Category;
				foreach (ItemDataCategory itemDataCategory in category)
				{
					if (kAUISelectItemData._ItemData.HasCategory(itemDataCategory.CategoryId))
					{
						flag = true;
						break;
					}
				}
				if (!flag)
				{
					return UiFuse.FuseInteraction.InvalidItem;
				}
			}
		}
		return UiFuse.FuseInteraction.ValidItem;
	}

	public void IngredientAdded(KAWidget widget)
	{
		KAWidget kAWidget = widget.pChildWidgets[0];
		KAWidget kAWidget2 = widget.FindChildItem(_ItemColorWidget);
		KAUISelectItemData kAUISelectItemData = (KAUISelectItemData)widget.GetUserData();
		if (kAWidget2 != null && kAUISelectItemData._ItemData.ItemRarity.HasValue)
		{
			UiItemRarityColorSet.SetItemBackgroundColor(kAUISelectItemData._ItemData.ItemRarity.Value, kAWidget2);
		}
		else
		{
			UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget2);
		}
		bool visibility = kAUISelectItemData?._IsBattleReady ?? false;
		KAWidget kAWidget3 = widget.FindChildItem("BattleReadyIcon");
		if (kAWidget3 != null)
		{
			kAWidget3.SetVisibility(visibility);
		}
		KAWidget kAWidget4 = widget.FindChildItem("FlightReadyIcon");
		if (kAWidget4 != null)
		{
			kAWidget4.SetVisibility(kAUISelectItemData._ItemData.HasAttribute("FlightSuit"));
		}
		kAWidget.SetVisibility(inVisible: false);
	}

	public void ShowInfo(KAUISelectItemData bluePrintwidgetData)
	{
		BlueprintWidgetData = bluePrintwidgetData;
		if (mKAUiSelectMenu != null)
		{
			KAUICursorManager.SetDefaultCursor("Loading");
			mIngredientsCounter = 0;
			mKAUiSelectMenu.ClearItems();
			if (bluePrintwidgetData != null && bluePrintwidgetData._ItemData != null && bluePrintwidgetData._ItemData.BluePrint != null && bluePrintwidgetData._ItemData.BluePrint.Ingredients != null)
			{
				foreach (BluePrintSpecification ingredient in bluePrintwidgetData._ItemData.BluePrint.Ingredients)
				{
					if (ingredient.ItemID.HasValue)
					{
						ItemData.Load(ingredient.ItemID.Value, OnItemDataReady, ingredient);
						continue;
					}
					KAUIFuseItemData kAUIFuseItemData = new KAUIFuseItemData(mKAUiSelectMenu, ingredient.BluePrintSpecID, new ItemData(), mKAUiSelectMenu._WHSize, ingredient.Quantity);
					if (ingredient.CategoryID.HasValue)
					{
						kAUIFuseItemData._ItemData.Category = new ItemDataCategory[1];
						kAUIFuseItemData._ItemData.Category[0] = new ItemDataCategory();
						kAUIFuseItemData._ItemData.Category[0].CategoryId = ingredient.CategoryID.Value;
					}
					if (ingredient.Tier.HasValue)
					{
						kAUIFuseItemData._ItemData.ItemStatsMap = new ItemStatsMap();
						kAUIFuseItemData._ItemData.ItemStatsMap.ItemTier = ingredient.Tier.Value;
					}
					if (ingredient.ItemRarity.HasValue)
					{
						kAUIFuseItemData._ItemData.ItemRarity = ingredient.ItemRarity.Value;
					}
					AddSlots(kAUIFuseItemData);
				}
				if (mIngredientsCounter == mKAUiSelectMenu.GetItemCount())
				{
					KAUICursorManager.SetDefaultCursor("Arrow");
				}
			}
		}
		if (mItemShardsTotal != null)
		{
			mItemShardsTotal.SetVisibility(inVisible: false);
		}
		if (mItemSellPrice != null)
		{
			mItemSellPrice.SetVisibility(inVisible: false);
		}
		if (mTxtCost != null)
		{
			mTxtCost.SetVisibility(inVisible: false);
		}
		if (bluePrintwidgetData._ItemData.BluePrint != null && bluePrintwidgetData._ItemData.BluePrint.Deductibles.Count > 0)
		{
			foreach (BluePrintDeductibleConfig deductible in bluePrintwidgetData._ItemData.BluePrint.Deductibles)
			{
				if (mTxtCost != null)
				{
					mTxtCost.SetVisibility(inVisible: true);
				}
				if (deductible.DeductibleType == DeductibleType.Item && deductible.ItemID == _UiBlackSmith._ShardItemId && deductible.Quantity > 0)
				{
					if (mItemShardsTotal != null)
					{
						mItemShardsTotal.SetVisibility(inVisible: true);
						mItemShardsTotal.SetText(deductible.Quantity.ToString());
					}
				}
				else if (deductible.DeductibleType == DeductibleType.Coins && deductible.Quantity > 0 && mItemSellPrice != null)
				{
					mItemSellPrice.SetVisibility(inVisible: true);
					mItemSellPrice.SetText(deductible.Quantity.ToString());
				}
			}
		}
		if (mItemName != null)
		{
			mItemName.SetText(bluePrintwidgetData._ItemData.ItemName);
		}
		if (bluePrintwidgetData._ItemData.BluePrint != null && bluePrintwidgetData._ItemData.BluePrint.Outputs != null && bluePrintwidgetData._ItemData.BluePrint.Outputs.Count > 0)
		{
			BluePrintSpecification bluePrintSpecification = bluePrintwidgetData._ItemData.BluePrint.Outputs[0];
			if (mItemTier != null)
			{
				if (bluePrintSpecification.Tier.HasValue)
				{
					string localizedString = _InfoText.GetLocalizedString();
					localizedString = localizedString.Replace("{x}", ((int)bluePrintSpecification.Tier.Value).ToString());
					mItemTier.SetText(localizedString);
				}
				else
				{
					mItemTier.SetText("");
				}
			}
			if (mItemIcon != null)
			{
				KAWidget kAWidget = mItemIcon.FindChildItem(_ItemColorWidget);
				if (kAWidget != null)
				{
					UiItemRarityColorSet.SetItemBackgroundColor((!bluePrintSpecification.ItemRarity.HasValue) ? ItemRarity.Common : bluePrintSpecification.ItemRarity.Value, kAWidget);
				}
			}
			ItemData.Load(bluePrintSpecification.ItemID.Value, OnOutputReady, null);
		}
		SetVisibility(inVisible: true);
	}

	private void OnOutputReady(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null && mItemIcon != null)
		{
			mItemIcon.SetTextureFromBundle(dataItem.IconName);
		}
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		inWidget.SetVisibility(inVisible: true);
	}

	private void OnItemDataReady(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null)
		{
			AddSlots(new KAUIFuseItemData(mKAUiSelectMenu, ((BluePrintSpecification)inUserData).BluePrintSpecID, dataItem, mKAUiSelectMenu._WHSize, ((BluePrintSpecification)inUserData).Quantity));
			return;
		}
		KAUICursorManager.SetDefaultCursor("Arrow");
		_UiFuse.ShowBlueprintFailDB();
		SetVisibility(inVisible: false);
	}

	private bool CanEnableFuse()
	{
		foreach (KAWidget item in mKAUiSelectMenu.GetItems())
		{
			if (((KAUISelectItemData)item.GetUserData())._ItemData.ItemID == 0)
			{
				return false;
			}
		}
		return true;
	}

	private Texture GetTextureByCategory(int cid)
	{
		return _CategoryInfo.Find((CategoryData x) => x._CatergoryID == cid)?._Icon;
	}

	private void AddSlots(KAUIFuseItemData data)
	{
		for (int i = 0; i < data._Quantity; i++)
		{
			mIngredientsCounter++;
			Texture texture = null;
			if (data._ItemData.ItemID == 0)
			{
				texture = ((data._ItemData.Category == null) ? GetTextureByCategory(-1) : GetTextureByCategory(data._ItemData.Category[0].CategoryId));
			}
			KAWidget kAWidget = mKAUiSelectMenu.AddWidget("EmptySlot");
			if (kAWidget != null)
			{
				AddWidgetData(kAWidget, null);
				kAWidget.SetInteractive(isInteractive: true);
				KAWidget kAWidget2 = kAWidget.FindChildItem("Icon");
				if (kAWidget2 != null)
				{
					kAWidget2.SetVisibility(inVisible: false);
				}
			}
			KAWidget kAWidget3 = kAWidget.FindChildItem("Ingredient");
			if (kAWidget3 != null)
			{
				AddWidgetData(kAWidget3, null);
				kAWidget3.SetUserData(data);
				if (texture != null)
				{
					kAWidget3.SetTexture(texture);
				}
				else
				{
					data.LoadResource();
					kAWidget3.FindChildItem("GreyMask").SetVisibility(inVisible: true);
				}
				kAWidget3.SetVisibility(inVisible: true);
				kAWidget.AddChild(kAWidget3);
			}
			if (BlueprintWidgetData._ItemData.RankId.HasValue && UserRankData.pInstance.RankID < BlueprintWidgetData._ItemData.RankId.Value)
			{
				kAWidget.SetDisabled(isDisabled: true);
				kAWidget3.FindChildItem("GreyMask").SetVisibility(inVisible: true);
			}
			KAWidget kAWidget4 = kAWidget.FindChildItem(_ItemColorWidget);
			if (kAWidget4 != null && data._ItemData.ItemRarity.HasValue)
			{
				UiItemRarityColorSet.SetItemBackgroundColor(data._ItemData.ItemRarity.Value, kAWidget4);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget4);
			}
		}
	}
}
