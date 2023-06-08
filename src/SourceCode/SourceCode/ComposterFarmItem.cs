using System.Collections.Generic;

public class ComposterFarmItem : FarmItem
{
	public string _CompostFeedAction = "CompostFeed";

	public List<ItemStateCriteriaConsumable> _CompostConsumables;

	public LocaleString _FeedInsufficientText = new LocaleString("You need %reqd_amount% %reqd_item% to get the compostor started, you only have %available_amount%");

	public LocaleString _FeedUnavailableText = new LocaleString("You need %reqd_amount% %reqd_item% to get the compostor started, you have none");

	private bool mIsHarvestParamsSet;

	private Dictionary<string, int> mContextDataName_InventoryID_Map = new Dictionary<string, int>();

	private ItemStateCriteriaConsumable mCurrentUsedConsumableCriteria;

	public override void ProcessCurrentStage()
	{
		if (!(base.pFarmManager == null) && base.pFarmManager.pIsReady && mInitialized && !base.pFarmManager.pIsBuildMode && base.pIsRuleSetInitialized && base.pIsStateSet)
		{
			base.ProcessCurrentStage();
			if (!(GetTimeLeftInCurrentStage() > 0f) && base.pCurrentStage != null && !mIsHarvestParamsSet && base.pCurrentStage._Name.Equals(GetHarvestStageName()))
			{
				mIsHarvestParamsSet = true;
				UpdateItemWithStage(base.pCurrentStage);
				UpdateContextIcon();
			}
		}
	}

	public override void UpdateContextIcon()
	{
		if (base.pIsBuildMode && base.pClickable != null)
		{
			base.pClickable._RollOverCursorName = "Activate";
		}
	}

	protected override void OnContextAction(string inActionName)
	{
		base.OnContextAction(inActionName);
		if (!(inActionName == "Harvest"))
		{
			if (!(inActionName == "Boost"))
			{
				foreach (ItemStateCriteriaConsumable compostConsumable in _CompostConsumables)
				{
					UserItemData userItemData = CommonInventoryData.pInstance.FindItem(compostConsumable.ItemID);
					if (userItemData == null)
					{
						ItemData itemData = base.pFarmManager.GetItemData(compostConsumable.ItemID);
						if (itemData != null && itemData.ItemName == inActionName)
						{
							string localizedString = _FeedUnavailableText.GetLocalizedString();
							localizedString = localizedString.Replace("%reqd_amount%", compostConsumable.Amount.ToString());
							localizedString = localizedString.Replace("%reqd_item%", itemData.ItemName);
							base.pFarmManager.ShowDialog(base.pFarmManager._DialogAssetName, "PfUiNoFeed", base.pFarmManager._FarmingDBTitleText, string.Empty, string.Empty, "OnOK", string.Empty, destroyDB: true, localizedString, base.gameObject);
						}
					}
					if (userItemData != null && userItemData.Item.ItemName == inActionName)
					{
						if (compostConsumable.Amount <= userItemData.Quantity)
						{
							mCurrentUsedConsumableCriteria = compostConsumable;
							GotoNextStage();
						}
						else
						{
							string localizedString2 = _FeedInsufficientText.GetLocalizedString();
							localizedString2 = localizedString2.Replace("%reqd_amount%", compostConsumable.Amount.ToString());
							localizedString2 = localizedString2.Replace("%reqd_item%", userItemData.Item.ItemName);
							localizedString2 = localizedString2.Replace("%available_amount%", userItemData.Quantity.ToString());
							base.pFarmManager.ShowDialog(base.pFarmManager._DialogAssetName, "PfUiNoFeed", base.pFarmManager._FarmingDBTitleText, string.Empty, string.Empty, "OnOK", string.Empty, destroyDB: true, localizedString2, base.gameObject);
						}
						break;
					}
				}
				return;
			}
			GotoNextStage(speedup: true);
		}
		else
		{
			GotoNextStage();
		}
	}

	protected override bool CanProcessUpdateData()
	{
		return CanActivate();
	}

	protected override void ProcessSensitiveData(ref List<string> menuItemNames)
	{
		if (!base.pIsBuildMode)
		{
			if (menuItemNames.Contains("Store"))
			{
				menuItemNames.Remove("Store");
			}
			UpdateCompostFeedMenu(ref menuItemNames);
		}
	}

	private void UpdateCompostFeedMenu(ref List<string> inNames)
	{
		if (base.pCurrentStage == null || _StateDetails == null || _StateDetails.Count <= 0 || _StateDetails[0]._ID != base.pCurrentStage._ID)
		{
			return;
		}
		ContextData contextData = GetContextData(_CompostFeedAction);
		if (contextData == null || _CompostConsumables == null)
		{
			return;
		}
		foreach (ItemStateCriteriaConsumable compostConsumable in _CompostConsumables)
		{
			ItemData itemData = base.pFarmManager.GetItemData(compostConsumable.ItemID);
			if (itemData != null)
			{
				inNames.Add(itemData.ItemName);
				AddChildContextDataToParent(contextData, itemData);
				mContextDataName_InventoryID_Map[itemData.ItemName] = itemData.ItemID;
			}
		}
	}

	public override string GetStatusText()
	{
		string empty = string.Empty;
		float timeTillHarvest = GetTimeTillHarvest();
		if (timeTillHarvest > 0f)
		{
			return UtUtilities.GetTimerString((int)timeTillHarvest);
		}
		return "0";
	}

	protected override string GetHarvestStageName()
	{
		return "Harvest";
	}

	public override void ParseConsumablesInventoryData(OnParseConsumableInventoryData parseDelegate)
	{
		base.ParseConsumablesInventoryData(parseDelegate);
		if (mCurrentUsedConsumableCriteria != null)
		{
			CommonInventoryData.pInstance.RemoveItem(mCurrentUsedConsumableCriteria.ItemID, updateServer: true, mCurrentUsedConsumableCriteria.Amount);
			mCurrentUsedConsumableCriteria = null;
		}
	}

	protected override bool CanDestroyOnHarvest()
	{
		return false;
	}
}
