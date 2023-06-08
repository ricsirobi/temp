using UnityEngine;

public class UiRewardScreen : KAUISelect
{
	private GameObject mMessageObject;

	public GameObject pMessageObject
	{
		set
		{
			mMessageObject = value;
		}
	}

	public override void SelectItem(KAWidget item)
	{
		base.SelectItem(item);
		if (mMessageObject != null)
		{
			if (item != null)
			{
				mMessageObject.SendMessage("OnSelectBattleBackpackItem", item, SendMessageOptions.DontRequireReceiver);
			}
			else
			{
				mMessageObject.SendMessage("OnDeselectItem", SendMessageOptions.DontRequireReceiver);
			}
		}
	}

	public override void AddWidgetData(KAWidget targetWidget, KAUISelectItemData widgetData)
	{
		base.AddWidgetData(targetWidget, widgetData);
		bool flag = widgetData?._IsBattleReady ?? false;
		KAWidget kAWidget = targetWidget.FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(flag);
		}
		KAWidget kAWidget2 = targetWidget.FindChildItem("FlightReadyIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(widgetData != null && widgetData._ItemData != null && widgetData._ItemData.HasAttribute("FlightSuit"));
		}
		KAWidget kAWidget3 = targetWidget.FindChildItem(((UiRewardMenu)base.pKAUiSelectMenu)._ItemColorWidget);
		if (kAWidget3 != null)
		{
			if (flag)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((widgetData == null || widgetData._ItemData == null || !widgetData._ItemData.ItemRarity.HasValue) ? ItemRarity.Common : widgetData._ItemData.ItemRarity.Value, kAWidget3);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(((UiRewardMenu)base.pKAUiSelectMenu).pItemDefaultColor, kAWidget3);
			}
		}
	}
}
