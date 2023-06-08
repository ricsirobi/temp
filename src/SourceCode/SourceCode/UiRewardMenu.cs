using System.Collections;
using UnityEngine;

public class UiRewardMenu : KAUISelectMenu
{
	public KAUIMenu _TargetMenu;

	public string _ItemColorWidget = "CellBackground";

	private bool mWidgetClicked;

	private Color mItemDefaultColor = Color.white;

	public Color pItemDefaultColor => mItemDefaultColor;

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
	}

	public override void OnClick(KAWidget inWidget)
	{
		if (!mWidgetClicked)
		{
			if (_TargetMenu != null)
			{
				_TargetMenu.SetSelectedItem(null);
			}
			StartCoroutine("DelayedClick", inWidget);
		}
	}

	public override void OnDoubleClick(KAWidget inWidget)
	{
		base.OnDoubleClick(inWidget);
		if (!(_TargetMenu == null))
		{
			mWidgetClicked = false;
			StopCoroutine("DelayedClick");
			AddToTargetMenu(inWidget, _TargetMenu);
			SelectItem(null);
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		SelectItem(null);
	}

	private IEnumerator DelayedClick(KAWidget inWidget)
	{
		mWidgetClicked = true;
		float waitTime = 0.4f;
		float deltaTime = 0f;
		while (deltaTime < waitTime && UICamera.hoveredObject == inWidget.gameObject)
		{
			deltaTime += Time.deltaTime;
			yield return null;
		}
		if (mWidgetClicked)
		{
			base.OnClick(inWidget);
			mWidgetClicked = false;
		}
	}

	public override void UpdateWidget(KAUISelectItemData widgetData)
	{
		base.UpdateWidget(widgetData);
		if (widgetData == null)
		{
			return;
		}
		KAWidget kAWidget = widgetData.GetItem().FindChildItem("BattleReadyIcon");
		if (kAWidget != null)
		{
			kAWidget.SetVisibility(widgetData._IsBattleReady);
		}
		KAWidget kAWidget2 = widgetData.GetItem().FindChildItem("FlightReadyIcon");
		if (kAWidget2 != null)
		{
			kAWidget2.SetVisibility(widgetData._ItemData != null && widgetData._ItemData.HasAttribute("FlightSuit"));
		}
		KAWidget kAWidget3 = widgetData.GetItem().FindChildItem(_ItemColorWidget);
		if (kAWidget3 != null)
		{
			if (widgetData._IsBattleReady)
			{
				UiItemRarityColorSet.SetItemBackgroundColor((widgetData._ItemData == null || !widgetData._ItemData.ItemRarity.HasValue) ? ItemRarity.Common : widgetData._ItemData.ItemRarity.Value, kAWidget3);
			}
			else
			{
				UiItemRarityColorSet.SetItemBackgroundColor(mItemDefaultColor, kAWidget3);
			}
		}
	}
}
