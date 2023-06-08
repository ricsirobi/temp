using UnityEngine;

public class ReelFloat : MonoBehaviour
{
	public float _DistanceMin = 0.4f;

	public float _DistanceMax = 0.8f;

	public Transform _Min;

	public Transform _Max;

	public GameObject _FloatObject;

	[HideInInspector]
	public Transform _Float;

	private float mRadius = 5f;

	private Vector3 mDirection;

	private float mReelIn;

	private float mReelDrag;

	private float mReelMax;

	private bool mCaptured;

	private float mDistance;

	private float mMaxDistance;

	private float mTravelledDistance;

	private Vector3 mFloatStartingPos;

	private float mFloatHeight;

	private Transform mFish;

	private Animation mFishAnimation;

	private ParticleSystem mFishSplashPS;

	private GameObject mFishSplashGO;

	private float mMaxY;

	private float mNibbleValue;

	private FishingZone mZone;

	private bool mIsReeling;

	public float mTimeLastSplash;

	public bool pIsCaptured => mCaptured;

	public float pNibbleValue => mNibbleValue;

	private void Start()
	{
		if (Physics.Raycast(_Min.position, -Vector3.up, out var hitInfo))
		{
			_Min.position = hitInfo.point;
		}
		if (Physics.Raycast(_Max.position, -Vector3.up, out hitInfo))
		{
			_Max.position = hitInfo.point;
		}
		GameObject gameObject = Object.Instantiate(_FloatObject, Vector3.zero, Quaternion.identity);
		_Float = gameObject.transform;
		_Float.parent = base.transform;
		_Float.gameObject.SetActive(value: false);
		mZone = base.transform.parent.GetComponent<FishingZone>();
	}

	public void Setup(float ReelDrag, float ReelMax, bool nibble = false)
	{
		_Float.gameObject.SetActive(value: true);
		mRadius = Random.Range(_DistanceMin, _DistanceMax);
		mDirection = base.transform.position - AvAvatar.position;
		mDirection.y = 0f;
		mMaxDistance = mDirection.magnitude;
		mDirection.Normalize();
		mDistance = mMaxDistance * mRadius;
		Vector3 position = base.transform.position - mDirection.normalized * mMaxDistance * 0.9f;
		position.y = _Min.position.y;
		_Min.position = position;
		Vector3 position2 = _Min.position;
		position2.y = 0f;
		_Float.position = position2 + mDirection * mDistance;
		mFloatStartingPos = _Float.position;
		mReelIn = 0f;
		mTravelledDistance = 0f;
		mReelDrag = ReelDrag;
		mReelMax = ReelMax;
		mCaptured = false;
		mTravelledDistance = 0f;
		mIsReeling = false;
		SphereCollider component = _Float.GetComponent<SphereCollider>();
		if (null != component)
		{
			mFloatHeight = component.radius * 1f * _Float.localScale.y;
		}
		Transform transform = base.transform.Find("WaterLevel");
		if (null != transform)
		{
			mFloatStartingPos.y = transform.position.y - component.center.y;
		}
		else
		{
			mFloatStartingPos.y = 2.466f;
		}
		mMaxY = mFloatStartingPos.y;
		if (!nibble)
		{
			Vector3 position3 = _Float.position;
			position3.y = mFloatStartingPos.y;
			_Float.transform.position = position3;
			Nibble(0.5f);
		}
	}

	public void StartReel()
	{
		Vector3 position = _Float.position;
		position.y = mFloatStartingPos.y;
		_Float.transform.position = position;
		mFishSplashGO = Object.Instantiate(mZone._FishHitWaterSplash);
		mFishSplashPS = mFishSplashGO.GetComponent<ParticleSystem>();
		mIsReeling = true;
	}

	public void StopReel()
	{
		mIsReeling = false;
	}

	public void ReelIn(float ds)
	{
		mReelIn = ds;
	}

	public void Nibble(float nibbleValue)
	{
		Vector3 position = _Float.position;
		position.y = mMaxY - mFloatHeight * nibbleValue;
		_Float.position = position;
		mNibbleValue = nibbleValue;
	}

	private void Update()
	{
		if (mIsReeling)
		{
			mReelIn -= mReelDrag * Time.deltaTime;
			mReelIn = Mathf.Clamp(mReelIn, 0f, mReelMax);
			if (mTravelledDistance >= mDistance)
			{
				_Float.gameObject.SetActive(value: false);
				mCaptured = true;
			}
			else
			{
				_Float.position -= mDirection * mReelIn * Time.deltaTime;
				mTravelledDistance += mReelIn * Time.deltaTime;
				mCaptured = false;
			}
		}
		if (!(null != mFish))
		{
			return;
		}
		if (mFishAnimation.IsPlaying("Jump"))
		{
			Transform transform = mFish.Find("MainRoot/Root/Tail");
			if (null != transform && transform.position.y <= _Float.position.y && Time.time - mTimeLastSplash > 1.3f && null != mFishSplashPS)
			{
				mFishSplashPS.gameObject.transform.position = transform.position;
				mFishSplashPS.time = 0f;
				mFishSplashPS.Play();
				mTimeLastSplash = Time.time;
			}
		}
		else if (null != mFishSplashPS)
		{
			mFishSplashPS.Stop();
		}
	}

	public void AttachFish(Transform fish)
	{
		mFish = fish;
		mFishAnimation = mFish.GetComponent<Animation>();
		fish.parent = _Float;
		fish.localPosition = mDirection * 1.1f;
		Vector3 position = fish.position;
		position.y = mFloatStartingPos.y;
		fish.position = position;
	}

	public void RemoveFish()
	{
		if (null != mFish)
		{
			Object.Destroy(mFish.gameObject);
		}
		mFish = null;
		if (null != mFishSplashGO)
		{
			Object.Destroy(mFishSplashGO);
		}
		mFishSplashPS = null;
	}

	private void OnDrawGizmos()
	{
		Gizmos.color = Color.green;
		Gizmos.DrawWireSphere(base.transform.position, _DistanceMin);
		Gizmos.color = Color.red;
		Gizmos.DrawWireSphere(base.transform.position, _DistanceMax);
	}

	public void HideFloat()
	{
		if (null != _Float)
		{
			_Float.gameObject.SetActive(value: false);
		}
	}
}
