using UnityEngine;

public class LtRotater : LtAnimBase
{
	public Vector3 _OrientationMin = Vector3.zero;

	public Vector3 _OrientationMax = Vector3.zero;

	private Quaternion mStartOrientation;

	private Quaternion mEndOrientation;

	public override void Start()
	{
		mStartOrientation = base.transform.rotation;
		base.Start();
	}

	public override void OnEndCycle()
	{
		base.OnEndCycle();
		mEndOrientation = mStartOrientation;
		float x = Random.Range(_OrientationMin.x, _OrientationMax.x);
		float y = Random.Range(_OrientationMin.y, _OrientationMax.y);
		float z = Random.Range(_OrientationMin.z, _OrientationMax.z);
		mEndOrientation *= Quaternion.Euler(x, y, z);
	}

	public override void UpdateValue(float f)
	{
		base.transform.rotation = Quaternion.Slerp(mStartOrientation, mEndOrientation, f);
	}
}
