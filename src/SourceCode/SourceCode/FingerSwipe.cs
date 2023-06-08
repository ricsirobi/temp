using UnityEngine;

public class FingerSwipe : MonoBehaviour
{
	private const int FINGERS = 5;

	public int swipeLength = 25;

	public int swipeVariance = 5;

	public GameObject _MessageObj;

	private Vector2[] fingerTrackArray;

	private bool[] swipeCompleteArray;

	private int activeTouch = -1;

	private void Start()
	{
		if (Application.platform != RuntimePlatform.IPhonePlayer && Application.platform != RuntimePlatform.Android)
		{
			Object.Destroy(this);
			return;
		}
		fingerTrackArray = new Vector2[5];
		swipeCompleteArray = new bool[5];
	}

	private void Update()
	{
		if (Input.touchCount <= 0 || Input.touchCount >= 6)
		{
			return;
		}
		Touch[] touches = Input.touches;
		for (int i = 0; i < touches.Length; i++)
		{
			Touch touch = touches[i];
			if (touch.fingerId < 0 || touch.fingerId >= 5)
			{
				continue;
			}
			if (touch.phase == TouchPhase.Began)
			{
				fingerTrackArray[touch.fingerId] = touch.position;
			}
			if (touch.position.y > fingerTrackArray[touch.fingerId].y + (float)swipeVariance)
			{
				fingerTrackArray[touch.fingerId] = touch.position;
			}
			if (touch.position.y < fingerTrackArray[touch.fingerId].y - (float)swipeVariance)
			{
				fingerTrackArray[touch.fingerId] = touch.position;
			}
			if (touch.position.x > fingerTrackArray[touch.fingerId].x + (float)swipeLength && !swipeCompleteArray[touch.fingerId] && activeTouch == -1)
			{
				activeTouch = touch.fingerId;
				swipeCompleteArray[touch.fingerId] = true;
				_MessageObj.SendMessage("OnSwipeRight");
			}
			if (touch.position.x < fingerTrackArray[touch.fingerId].x - (float)swipeLength && !swipeCompleteArray[touch.fingerId] && activeTouch == -1)
			{
				activeTouch = touch.fingerId;
				swipeCompleteArray[touch.fingerId] = true;
				_MessageObj.SendMessage("OnSwipeLeft");
			}
			if (touch.fingerId != activeTouch || touch.phase != TouchPhase.Ended)
			{
				continue;
			}
			Touch[] touches2 = Input.touches;
			for (int j = 0; j < touches2.Length; j++)
			{
				Touch touch2 = touches2[j];
				if (touch2.fingerId >= 0 && touch2.fingerId < 5)
				{
					fingerTrackArray[touch2.fingerId] = touch2.position;
				}
			}
			swipeCompleteArray[touch.fingerId] = false;
			activeTouch = -1;
		}
	}
}
