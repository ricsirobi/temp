using UnityEngine;

public class Eel : MonoBehaviour
{
	public Color[] _EelColor;

	public Renderer _EelRender;

	public ParticleSystem _ParticleOnStart;

	public ParticleSystem _ParticleOnDestroy;

	public MinMax _SpeedRange;

	public MinMax _HeightRange;

	private bool mDestroyEel;

	private float mMaxHeight;

	private float mMovingSpeed;

	private float mMovementLamda;

	private Vector3 mStartPoint;

	private Vector3 mEndPoint;

	private float mPingPongLength;

	private Transform mTransform;

	private void Start()
	{
		_EelRender.material.color = _EelColor[Random.Range(0, _EelColor.Length)];
		mMaxHeight = _HeightRange.GetRandomValue();
		mMovingSpeed = _SpeedRange.GetRandomValue();
		mMovementLamda = 0f;
		mTransform = base.transform;
		mPingPongLength = mMaxHeight * 2f;
		mStartPoint = mTransform.position;
		mEndPoint = mTransform.position + Vector3.up * mMaxHeight;
		if (_ParticleOnStart != null)
		{
			Object.Instantiate(_ParticleOnStart, mTransform.position, mTransform.rotation);
		}
	}

	private void Update()
	{
		if (mDestroyEel)
		{
			if (_ParticleOnDestroy != null)
			{
				Object.Instantiate(_ParticleOnDestroy, mTransform.position, mTransform.rotation);
			}
			Object.Destroy(base.gameObject);
			return;
		}
		mMovementLamda += mMovingSpeed * Time.deltaTime;
		mTransform.position = Vector3.Lerp(mStartPoint, mEndPoint, Mathf.PingPong(mMovementLamda, mMaxHeight) / mMaxHeight);
		Vector3 normalized = (mStartPoint - mEndPoint).normalized;
		if (mMovementLamda < mPingPongLength / 2f)
		{
			normalized = (mEndPoint - mStartPoint).normalized;
		}
		mTransform.rotation = Quaternion.Slerp(mTransform.rotation, Quaternion.LookRotation(normalized), Time.deltaTime * mMovingSpeed);
		if (mMovementLamda >= mPingPongLength)
		{
			mDestroyEel = true;
		}
	}

	private void OnAmmoHit(ObAmmo ammo)
	{
		mDestroyEel = true;
	}
}
