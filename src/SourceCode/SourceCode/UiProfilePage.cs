using UnityEngine;

public class UiProfilePage : KAUI
{
	public KAWidget _PageInBtn;

	public KAWidget _PageOutBtn;

	public override void OnClick(KAWidget inWidget)
	{
		if (!(inWidget == null))
		{
			base.OnClick(inWidget);
			base.gameObject.transform.parent.GetComponent<KAUI>().OnClick(inWidget);
		}
	}

	public void OnDrag(Vector2 inDelta)
	{
		base.pEvents.ProcessDragEvent(null, inDelta);
	}

	public void OnPress(bool pressed)
	{
		base.pEvents.ProcessPressEvent(null, pressed);
	}

	public void OnTransitionDone(bool visible)
	{
		if ((bool)_PageInBtn)
		{
			_PageInBtn.SetVisibility(!visible);
		}
		if ((bool)_PageOutBtn)
		{
			_PageOutBtn.SetVisibility(visible);
		}
	}
}
