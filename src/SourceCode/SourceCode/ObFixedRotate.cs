using UnityEngine;

public class ObFixedRotate : MonoBehaviour
{
	public float _MinTime;

	public float _MaxTime;

	public float _Speed;

	public Vector3 _Amount;

	private float mTime;

	private Vector3 mDest;

	private Vector3 mCurrent;

	private void Start()
	{
		mTime = Random.Range(_MinTime, _MaxTime);
		mCurrent = Vector3.zero;
	}

	private void Update()
	{
		mTime -= Time.deltaTime;
		if (mTime <= 0f)
		{
			if (_Amount.x != 0f)
			{
				mDest.x = ((float)(int)Random.Range(0f, 360f / (_Amount.x / 2f)) - 360f / _Amount.x) * _Amount.x;
			}
			if (_Amount.y != 0f)
			{
				mDest.y = ((float)(int)Random.Range(0f, 360f / (_Amount.y / 2f)) - 360f / _Amount.y) * _Amount.y;
			}
			if (_Amount.z != 0f)
			{
				mDest.z = ((float)(int)Random.Range(0f, 360f / (_Amount.z / 2f)) - 360f / _Amount.z) * _Amount.z;
			}
			mTime = Random.Range(_MinTime, _MaxTime);
		}
		if (mCurrent != mDest)
		{
			mCurrent = Vector3.Lerp(mCurrent, mDest, _Speed);
			base.transform.localEulerAngles = mCurrent;
		}
	}
}
