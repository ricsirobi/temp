using UnityEngine;

public class UiMarketMenu : KAUISelectMenu
{
	public KAUIMenu _TargetMenu;

	public string _ItemColorWidget = "CellBackground";

	private Color mItemDefaultColor = Color.white;

	public Color pItemDefaultColor => mItemDefaultColor;

	public override void Initialize(KAUI parentInt)
	{
		base.Initialize(parentInt);
		mItemDefaultColor = UiItemRarityColorSet.GetItemBackgroundColor(_Template, _ItemColorWidget);
	}

	public override void OnDoubleClick(KAWidget inWidget)
	{
		base.OnDoubleClick(inWidget);
		if (!(_TargetMenu == null))
		{
			AddToTargetMenu(inWidget, _TargetMenu);
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if (_TargetMenu != null)
		{
			_TargetMenu.SetSelectedItem(null);
		}
	}
}
