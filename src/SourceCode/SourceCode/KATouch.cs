using UnityEngine;

public struct KATouch
{
	public int fingerId;

	public Vector2 position;

	public Vector2 deltaPosition;

	public float deltaTime;

	public int tapCount;

	public TouchPhase phase;

	public static KATouch FromTouch(Touch inTouch)
	{
		KATouch result = default(KATouch);
		result.fingerId = inTouch.fingerId;
		result.deltaPosition = inTouch.deltaPosition;
		result.position = inTouch.position;
		result.deltaTime = inTouch.deltaTime;
		result.tapCount = inTouch.tapCount;
		result.phase = inTouch.phase;
		return result;
	}

	public static KATouch FromMouse(ref Vector2? lastMousePosition)
	{
		KATouch result = default(KATouch);
		result.fingerId = 2;
		if (lastMousePosition.HasValue)
		{
			result.deltaPosition = KAInput.mousePosition - (Vector3)lastMousePosition.Value;
		}
		if (KAInput.GetMouseButtonUp(0))
		{
			result.phase = TouchPhase.Ended;
			result.tapCount = 1;
			lastMousePosition = null;
		}
		else if (KAInput.GetMouseButtonDown(0))
		{
			result.phase = TouchPhase.Began;
			lastMousePosition = KAInput.mousePosition;
		}
		else
		{
			result.phase = TouchPhase.Moved;
			lastMousePosition = KAInput.mousePosition;
		}
		result.position = new Vector2(KAInput.mousePosition.x, KAInput.mousePosition.y);
		return result;
	}
}
