using System;
using UnityEngine;

public class KAUISelectDragonMenu : KAUISelectMenu
{
	public DragonPartTab _CurrentTab;

	private KAUISelectDragon mUiSelectDragon;

	public const string DRAGON_PART_SADDLES = "Saddles";

	public const string DRAGON_PART_WARPAINT = "WarPaint";

	public const string DRAGON_PART_SKINS = "Skins";

	public const string DRAGON_GLOW = "Glow";

	[NonSerialized]
	public bool mModified;

	public LocaleString _EquipmentNotSupportedText = new LocaleString("The {itemName} cannot be used by your {currentDragonType}.");

	public LocaleString _IncorrectDragonRankText = new LocaleString("Your dragon needs to be at rank {requiredRank} to use {itemName}.");

	public LocaleString _IncorrectDragonAgeText = new LocaleString("[REVIEW]You cannot access this till your dragon is of teen age or above.");

	public LocaleString _IncorrectDragonAgeShortWingText = new LocaleString("You cannot access this until your dragon is in the Short Wing stage or older");

	public LocaleString _IncorrectDragonAgeBroadWingText = new LocaleString("You cannot access this until your dragon is in the Broad Wing stage or older");

	public LocaleString _IncorrectDragonAgeTitanWingText = new LocaleString("You cannot access this until your dragon is in the Titan Wing stage");

	public LocaleString _PetGlowMatchFailText = new LocaleString("Only the following glow potions can be applied on {dragon} : {color}");

	public LocaleString _AndText = new LocaleString("and");

	public string _ItemColorWidget = "CellBkg";

	private Color mItemDefaultColor = Color.white;

	protected override void Awake()
	{
		base.Awake();
		base.pCategoryID = 0;
	}

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mUiSelectDragon = (KAUISelectDragon)parentInt;
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
	}

	public void ChangeCategory(string pType, bool forceChange)
	{
		int categoryID = GetCategoryID(pType);
		ChangeCategory(categoryID, forceChange);
	}

	public int GetCategoryID(string partType)
	{
		if (partType.Equals("WarPaint", StringComparison.OrdinalIgnoreCase))
		{
			return 423;
		}
		if (partType.Equals("Saddles", StringComparison.OrdinalIgnoreCase))
		{
			return 380;
		}
		if (partType.Equals("Skins", StringComparison.OrdinalIgnoreCase))
		{
			return 424;
		}
		if (partType.Equals("Glow", StringComparison.OrdinalIgnoreCase))
		{
			return 643;
		}
		return 0;
	}

	public override void FinishMenuItems(bool addParentItems = false)
	{
		base.FinishMenuItems(addParentItems: true);
		_CurrentTab.OnItemReady(mDBItemList.Count, mMainUI);
	}

	public override void AddInvMenuItem(UserItemData userItem, int quantity = 1)
	{
		bool flag = true;
		if (!_CurrentTab._ShowForAllDragons)
		{
			int attribute = userItem.Item.GetAttribute("PetTypeID", -1);
			if (attribute > 0 && mUiSelectDragon.pPetData.PetTypeID != attribute)
			{
				flag = false;
			}
		}
		if (flag)
		{
			KAUISelectItemData widgetData = new KAUISelectItemData(this, userItem, _WHSize, quantity);
			AddWidgetItem(widgetData);
		}
	}

	private void ShowDialog(string text)
	{
		KAUICursorManager.SetDefaultCursor("Arrow");
		mKAUIGenericDB = GameUtilities.CreateKAUIGenericDB("PfKAUIGenericDBSm", "Message Box");
		mKAUIGenericDB._MessageObject = base.gameObject;
		mKAUIGenericDB._OKMessage = "KillGenericDB";
		mKAUIGenericDB.SetButtonVisibility(inYesBtn: false, inNoBtn: false, inOKBtn: true, inCloseBtn: false);
		mKAUIGenericDB.SetText(text, interactive: false);
		KAUI.SetExclusive(mKAUIGenericDB);
	}

	private void KillGenericDB()
	{
		if (mKAUIGenericDB != null)
		{
			KAUI.RemoveExclusive(mKAUIGenericDB);
			UnityEngine.Object.Destroy(mKAUIGenericDB.gameObject);
			mKAUIGenericDB = null;
		}
	}

	public override void SelectItem(KAWidget inWidget)
	{
		ItemData itemData = ((KAUISelectItemData)inWidget.GetUserData())._ItemData;
		int attribute = itemData.GetAttribute("PetTypeID", -1);
		int rid = -1;
		RaisedPetStage raisedPetStage = ParseRequiredAgeAttribute(itemData);
		if (itemData.HasCategory(643))
		{
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo = SanctuaryData.FindSanctuaryPetTypeInfo(mUiSelectDragon.pPetData.PetTypeID);
			bool flag = true;
			if (sanctuaryPetTypeInfo._GlowColors.Length != 0)
			{
				string glowName = itemData.GetAttribute("Color", "");
				if (!Array.Exists(sanctuaryPetTypeInfo._GlowColors, (string x) => x == glowName))
				{
					flag = false;
				}
			}
			if (flag)
			{
				mUiSelectDragon.OnSelectMenuItem(inWidget);
			}
			else if (GlowManager.pInstance != null)
			{
				ShowDialog(GetGlowFailFormatText(sanctuaryPetTypeInfo));
			}
		}
		else if (attribute > 0 && mUiSelectDragon.pPetData.PetTypeID != attribute)
		{
			string localizedString = _EquipmentNotSupportedText.GetLocalizedString();
			localizedString = localizedString.Replace("{itemName}", itemData.ItemName);
			SanctuaryPetTypeInfo sanctuaryPetTypeInfo2 = SanctuaryData.FindSanctuaryPetTypeInfo(mUiSelectDragon.pPetData.PetTypeID);
			localizedString = localizedString.Replace("{currentDragonType}", sanctuaryPetTypeInfo2._NameText.GetLocalizedString());
			ShowDialog(localizedString);
		}
		else if (mUiSelectDragon.mPet.pAge < RaisedPetData.GetAgeIndex(raisedPetStage))
		{
			ShowDialog(GetIncorrectPetAgeText(raisedPetStage));
		}
		else if (!IsCorrectPetRank(itemData, out rid))
		{
			string localizedString2 = _IncorrectDragonRankText.GetLocalizedString();
			localizedString2 = localizedString2.Replace("{itemName}", itemData.ItemName);
			localizedString2 = localizedString2.Replace("{requiredRank}", rid.ToString());
			ShowDialog(localizedString2);
		}
		else
		{
			mModified = true;
			mUiSelectDragon.SetAccessoryItem(inWidget, RaisedPetData.GetAccessoryType(base.pCategoryID));
		}
		base.SelectItem(inWidget);
	}

	private RaisedPetStage ParseRequiredAgeAttribute(ItemData itemData)
	{
		RaisedPetStage result = RaisedPetStage.TEEN;
		if (itemData == null || !itemData.HasAttribute("PetStage"))
		{
			return result;
		}
		string attribute = itemData.GetAttribute("PetStage", "");
		if (!string.IsNullOrEmpty(attribute) && Enum.IsDefined(typeof(RaisedPetStage), attribute))
		{
			result = (RaisedPetStage)Enum.Parse(typeof(RaisedPetStage), attribute);
		}
		return result;
	}

	private string GetIncorrectPetAgeText(RaisedPetStage inPetStage)
	{
		return inPetStage switch
		{
			RaisedPetStage.TEEN => _IncorrectDragonAgeShortWingText.GetLocalizedString(), 
			RaisedPetStage.ADULT => _IncorrectDragonAgeBroadWingText.GetLocalizedString(), 
			RaisedPetStage.TITAN => _IncorrectDragonAgeTitanWingText.GetLocalizedString(), 
			_ => _IncorrectDragonAgeText.GetLocalizedString(), 
		};
	}

	private string GetGlowFailFormatText(SanctuaryPetTypeInfo petInfo)
	{
		string localizedString = _PetGlowMatchFailText.GetLocalizedString();
		string text = "\n" + GlowManager.pInstance.GetColorLocalizedText(petInfo._GlowColors[0]);
		if (petInfo._GlowColors.Length > 1)
		{
			for (int i = 1; i < petInfo._GlowColors.Length; i++)
			{
				text = ((i > petInfo._GlowColors.Length - 2) ? (text + " " + _AndText.GetLocalizedString() + " ") : (text + ", "));
				text += GlowManager.pInstance.GetColorLocalizedText(petInfo._GlowColors[i]);
			}
		}
		localizedString = localizedString.Replace("{color}", text);
		return localizedString.Replace("{dragon}", petInfo._NameText.GetLocalizedString());
	}

	public override void SaveSelection()
	{
		base.SaveSelection();
	}

	public bool IsCorrectPetRank(ItemData inItem, out int rid)
	{
		rid = 0;
		if (inItem.RankId.HasValue)
		{
			rid = inItem.RankId.Value;
		}
		UserRank userRank = PetRankData.GetUserRank(mUiSelectDragon.pPetData);
		if (rid > 0 && userRank != null)
		{
			return rid <= userRank.RankID;
		}
		return true;
	}

	public override void UpdateWidget(KAUISelectItemData id)
	{
		base.UpdateWidget(id);
		if (id == null)
		{
			return;
		}
		bool flag = (id._UserItemData != null && id._UserItemData.pIsBattleReady) || (id._ItemData != null && id._ItemData.IsStatAvailable());
		KAWidget kAWidget = id.GetItem().FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(flag);
		}
		KAWidget kAWidget2 = id.GetItem().FindChildItem("FlightReadyIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(id._ItemData != null && id._ItemData.HasAttribute("FlightSuit"));
		}
		KAWidget kAWidget3 = id.GetItem().FindChildItem(_ItemColorWidget);
		if (kAWidget3 != null)
		{
			if (flag)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((id._ItemData == null || !id._ItemData.ItemRarity.HasValue) ? ItemRarity.Common : id._ItemData.ItemRarity.Value, kAWidget3);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget3);
			}
		}
	}
}
