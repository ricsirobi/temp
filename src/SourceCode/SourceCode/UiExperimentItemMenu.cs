using UnityEngine;

public class UiExperimentItemMenu : KAUIMenu
{
	private bool mDragged;

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		base.OnDrag(inWidget, inDelta);
		if (_ParentUi != null && !mDragged && KAInput.GetMouseButton(0))
		{
			mDragged = true;
			((UiScienceExperiment)_ParentUi).MenuItemDragged(this, inWidget, inDelta);
		}
	}

	protected override void Update()
	{
		base.Update();
		if (mDragged && KAInput.GetMouseButtonUp(0))
		{
			mDragged = false;
		}
	}
}
