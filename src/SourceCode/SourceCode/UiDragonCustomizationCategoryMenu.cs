using UnityEngine;

public class UiDragonCustomizationCategoryMenu : KAUIMenu
{
	public override void OnClick(KAWidget inItem)
	{
		base.OnClick(inItem);
	}

	public void OnDrag(Vector2 inDelta)
	{
		if (KAInput.pInstance.IsTouchInput())
		{
			if (KAUI.GetDirection(inDelta) == SwipeDirection.LEFT)
			{
				OnClick(FindItem("BtnSaddles"));
			}
			else if (KAUI.GetDirection(inDelta) == SwipeDirection.RIGHT)
			{
				OnClick(FindItem("BtnSkins"));
			}
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		OnDrag(inDelta);
	}
}
