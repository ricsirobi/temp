using UnityEngine;

public struct FakeTouch
{
	public int fingerId;

	public Vector2 position;

	public Vector2 deltaPosition;

	public float deltaTime;

	public int tapCount;

	public TouchPhase phase;

	public static FakeTouch FromTouch(Touch touch)
	{
		FakeTouch result = default(FakeTouch);
		result.fingerId = touch.fingerId;
		result.position = touch.position;
		result.deltaPosition = touch.deltaPosition;
		result.deltaTime = touch.deltaTime;
		result.phase = touch.phase;
		return result;
	}

	public static FakeTouch FromInput(ref Vector2? lastMousePosition)
	{
		FakeTouch result = default(FakeTouch);
		result.fingerId = 2;
		if (lastMousePosition.HasValue)
		{
			result.deltaPosition = KAInput.mousePosition - (Vector3)lastMousePosition.Value;
		}
		if (KAInput.GetMouseButtonDown(0))
		{
			result.phase = TouchPhase.Began;
			lastMousePosition = KAInput.mousePosition;
		}
		else if (KAInput.GetMouseButtonUp(0))
		{
			result.phase = TouchPhase.Ended;
			lastMousePosition = null;
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
