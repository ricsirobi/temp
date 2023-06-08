using UnityEngine;

public class ObFloatRandom : MonoBehaviour
{
	private enum Direction
	{
		Pos,
		Neg
	}

	public Vector3 _SpeedMin;

	public Vector3 _SpeedMax;

	public float _SpeedTime;

	public bool _Active = true;

	public bool _NegateSwitch;

	public Vector3 _DistanceMin;

	public Vector3 _DistanceMax;

	public float _DistanceTime;

	private Vector3 mStartPosition;

	private Vector3 mSpeed;

	private float mSpeedTime;

	private Vector3 mDistance;

	private float mDistanceTime;

	private Direction mDirX;

	private Direction mDirY;

	private Direction mDirZ;

	private void Start()
	{
		mStartPosition = base.transform.position;
		PickSpeed();
		PickDistance();
		mSpeedTime = _SpeedTime;
		mDistanceTime = _DistanceTime;
	}

	public void OnStateChange(bool switchOn)
	{
		if (_NegateSwitch)
		{
			_Active = !_Active;
		}
		else
		{
			_Active = switchOn;
		}
	}

	private void PickSpeed()
	{
		mSpeed.x = Random.Range(_SpeedMin.x, _SpeedMax.x);
		mSpeed.y = Random.Range(_SpeedMin.y, _SpeedMax.y);
		mSpeed.z = Random.Range(_SpeedMin.z, _SpeedMax.z);
	}

	private void PickDistance()
	{
		mDistance.x = Random.Range(_DistanceMin.x, _DistanceMax.x);
		mDistance.y = Random.Range(_DistanceMin.y, _DistanceMax.y);
		mDistance.z = Random.Range(_DistanceMin.z, _DistanceMax.z);
	}

	private void Update()
	{
		if (!_Active)
		{
			return;
		}
		Vector3 position = base.transform.position;
		if (mSpeed.x != 0f && mDistance.x != 0f)
		{
			UpdatePosition(ref position.x, mStartPosition.x, mSpeed.x, mDistance.x, ref mDirX);
		}
		if (mSpeed.y != 0f && mDistance.y != 0f)
		{
			UpdatePosition(ref position.y, mStartPosition.y, mSpeed.y, mDistance.y, ref mDirY);
		}
		if (mSpeed.z != 0f && mDistance.z != 0f)
		{
			UpdatePosition(ref position.z, mStartPosition.z, mSpeed.z, mDistance.z, ref mDirZ);
		}
		base.transform.position = position;
		if (_SpeedTime > 0f)
		{
			mSpeedTime -= Time.deltaTime;
			if (mSpeedTime <= 0f)
			{
				mSpeedTime = _SpeedTime;
				PickSpeed();
			}
		}
		if (_DistanceTime > 0f)
		{
			mDistanceTime -= Time.deltaTime;
			if (mDistanceTime <= 0f)
			{
				mDistanceTime = _DistanceTime;
				PickDistance();
			}
		}
	}

	private void UpdatePosition(ref float pos, float startPos, float speed, float distance, ref Direction dir)
	{
		if (dir == Direction.Pos)
		{
			pos += speed * Time.deltaTime;
			if (pos - startPos > distance)
			{
				dir = Direction.Neg;
			}
		}
		else
		{
			pos -= speed * Time.deltaTime;
			if (pos - startPos < 0f - distance)
			{
				dir = Direction.Pos;
			}
		}
	}
}
