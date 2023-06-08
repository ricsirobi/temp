using UnityEngine;

public class MMOSpline : MonoBehaviour
{
	public int _ID = -1;

	private bool mInitialized;

	private SplineControl[] mSplines;

	private float[] mSpeed;

	private float mTimeSent;

	private void Start()
	{
		mSplines = GetComponentsInChildren<SplineControl>();
		if (mSplines.Length != 0)
		{
			mSpeed = new float[mSplines.Length];
			for (int i = 0; i < mSplines.Length; i++)
			{
				mSpeed[i] = mSplines[i].Speed;
				mSplines[i].Speed = 0f;
			}
		}
	}

	private void Update()
	{
		if (!mInitialized && !(MainStreetMMOClient.pInstance == null) && MainStreetMMOClient.pInstance.pState == MMOClientState.IN_ROOM)
		{
			mInitialized = true;
			if (_ID != -1)
			{
				MainStreetMMOClient.pInstance.SendGetPositionEvent(_ID, base.gameObject);
				mTimeSent = Time.realtimeSinceStartup;
			}
		}
	}

	private void OnGetPositionEvent(float position)
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		for (int i = 0; i < mSplines.Length; i++)
		{
			float num = (realtimeSinceStartup - mTimeSent) * 0.5f * mSpeed[i];
			mSplines[i].CurrentPos = (mSplines[i].CurrentPos + position + num) % mSplines[i].LinearLength;
			mSplines[i].Speed = mSpeed[i];
		}
	}
}
