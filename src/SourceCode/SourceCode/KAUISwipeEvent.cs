using UnityEngine;

public class KAUISwipeEvent : MonoBehaviour
{
	public KAUI _KAUITrigger;

	public void OnPress(bool inPressed)
	{
		if (_KAUITrigger != null)
		{
			_KAUITrigger.OnPress(null, inPressed);
		}
	}

	public void OnDrag(Vector2 inDelta)
	{
		if (_KAUITrigger != null)
		{
			_KAUITrigger.OnDrag(null, inDelta);
		}
	}

	public void OnSwipe(Vector2 inDelta)
	{
		if (_KAUITrigger != null)
		{
			_KAUITrigger.OnSwipe(null, inDelta);
		}
	}
}
