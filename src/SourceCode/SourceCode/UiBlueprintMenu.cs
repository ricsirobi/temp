using System;
using System.Collections.Generic;
using UnityEngine;

public class UiBlueprintMenu : KAUISelectMenu
{
	public LocaleString _AvailableTilText = new LocaleString("Til XXXX");

	public LocaleString _TimeLeftMinText = new LocaleString("< XXXX min");

	public LocaleString _TimeLeftMinsText = new LocaleString("< XXXX mins");

	public LocaleString _TimeLeftHoursText = new LocaleString("< XXXX hrs");

	public string _ItemColorWidget = "CellBackground";

	public string _LevelWidget = "Level";

	public LocaleString _LevelPrefix;

	private Color mItemDefaultColor = Color.white;

	private List<KAUISelectItemData> mTimedBluePrints = new List<KAUISelectItemData>();

	public Color ItemDefaultColor => mItemDefaultColor;

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
	}

	public override void AddInvMenuItem(ItemData item, int quantity = 1)
	{
		if (!item.IsOutdated())
		{
			base.AddInvMenuItem(item, quantity);
		}
	}

	protected override void Update()
	{
		base.Update();
		HandleTimedBlueprints();
	}

	private string GetCustomizedTimerString(TimeSpan inTime)
	{
		if (inTime.TotalMinutes < 1.0)
		{
			return _TimeLeftMinText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt((float)inTime.TotalMinutes).ToString());
		}
		if (inTime.TotalMinutes < 60.0)
		{
			return _TimeLeftMinsText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt((float)inTime.TotalMinutes).ToString());
		}
		if (inTime.TotalHours < 24.0)
		{
			return _TimeLeftHoursText.GetLocalizedString().Replace("XXXX", Mathf.CeilToInt((float)inTime.TotalHours).ToString());
		}
		return string.Empty;
	}

	public override void OnItemLoaded(KAUISelectItemData widgetData)
	{
		base.OnItemLoaded(widgetData);
		if (widgetData == null)
		{
			return;
		}
		bool flag = UserRankData.pInstance.RankID < widgetData._ItemData.RankId;
		KAWidget kAWidget = widgetData.GetItem().FindChildItem(_LockedIconName);
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(flag);
		}
		KAWidget kAWidget2 = widgetData.GetItem().FindChildItem(_DisabledWidgetName);
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(flag);
		}
		KAWidget kAWidget3 = widgetData.GetItem().FindChildItem(_LevelWidget);
		if (kAWidget3 != null)
		{
			kAWidget3.SetVisibility(flag);
			if (flag)
			{
				kAWidget3.SetText(_LevelPrefix.GetLocalizedString() + widgetData._ItemData.RankId);
			}
		}
		if (widgetData._ItemData != null && widgetData._ItemData.BluePrint != null)
		{
			BluePrintSpecification bluePrintSpecification = widgetData._ItemData.BluePrint.Outputs[0];
			ItemData.Load(bluePrintSpecification.ItemID.Value, OnOutputReady, widgetData);
			KAWidget kAWidget4 = widgetData.GetItem().FindChildItem(_ItemColorWidget);
			if (kAWidget4 != null && bluePrintSpecification != null)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((!bluePrintSpecification.ItemRarity.HasValue) ? ItemRarity.Common : bluePrintSpecification.ItemRarity.Value, kAWidget4);
			}
		}
		ItemAvailability availability = widgetData._ItemData.GetAvailability();
		KAWidget kAWidget5 = widgetData.GetItem().FindChildItem("TimedIcon");
		if (kAWidget5 != null && availability != null)
		{
			DateTime value = availability.EndDate.Value;
			DateTime dateTime = availability.EndDate.Value.ToLocalTime();
			string text = _AvailableTilText.GetLocalizedString().Replace("XXXX", dateTime.ToString("MM/dd"));
			TimeSpan inTime = value.Subtract(ServerTime.pCurrentTime);
			if (inTime.TotalSeconds > 0.0 && inTime.TotalHours < 24.0)
			{
				text = GetCustomizedTimerString(inTime);
			}
			kAWidget5.SetText(text);
			kAWidget5.SetVisibility(inVisible: true);
			mTimedBluePrints.Add(widgetData);
		}
	}

	private void OnOutputReady(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null)
		{
			KAWidget item = ((KAUISelectItemData)inUserData).GetItem();
			if (item != null)
			{
				item.SetTextureFromBundle(dataItem.IconName);
			}
		}
	}

	private void OnTextureLoaded(KAWidget inWidget)
	{
		inWidget.SetVisibility(inVisible: true);
	}

	private void HandleTimedBlueprints()
	{
		if (mTimedBluePrints.Count == 0)
		{
			return;
		}
		List<KAUISelectItemData> list = null;
		foreach (KAUISelectItemData mTimedBluePrint in mTimedBluePrints)
		{
			if (mTimedBluePrint._ItemData.IsOutdated())
			{
				if (list == null)
				{
					list = new List<KAUISelectItemData>();
				}
				list.Add(mTimedBluePrint);
				RemoveWidget(mTimedBluePrint.GetItem());
			}
		}
		if (list == null || list.Count <= 0)
		{
			return;
		}
		foreach (KAUISelectItemData item in list)
		{
			mTimedBluePrints.Remove(item);
		}
	}
}
