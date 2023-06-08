using UnityEngine;

public struct FakeTouchExt
{
	public bool dirty;

	public float time;

	public int fingerId;

	public Vector2 position;

	public Vector2 deltaPosition;

	public float deltaTime;

	public int tapCount;

	public TouchPhase phase;
}
