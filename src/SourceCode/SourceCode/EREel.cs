using UnityEngine;

public class EREel : KAMonoBase
{
	public bool _Active;

	public Animation _EelAnim;

	public Transform _RenderPath;

	public GameObject _PrtParent;

	public AudioClip _SndWaterSplash;

	public GameObject _WaterHeightMarker;

	public GameObject[] _WaterSplashEffects;

	public Color[] _EelBlastColors;

	public GameObject _EelBlastEffectObj;

	public TargetHit3DScore _EelHit3DScore;

	public EREelTargetable _EelTargetable;

	public DragonFiringCSM _DragonFiringCSM;

	public SnChannel _Channel;

	public Transform _StartPoint;

	public Transform _EndPoint;

	public string _LaunchAnimName = "Launch";

	public string _IdleAnimName = "Idle";

	public float _Speed;

	public float _DelayOnTop;

	public float _DestroyDelay = 1f;

	public float _LauchCrossFadeTime = 0.3f;

	public float _JumpIntervalTime = 3f;

	public float _MinHeight;

	public float _MaxHeight;

	public float _PetHappiness;

	public LocaleString _PetHappinessText = new LocaleString("Happiness");

	public Vector3 _HappinessTextDragonOffset;

	private bool mIsDestroying;

	private Vector3 mControlPoint = Vector3.zero;

	private float mLamdaPosition;

	private float mDelayTime;

	private bool mPlayedPartForGoingAbove;

	private bool mPlayedPartForGoingBelow;

	private bool mIdleAnimationPlayed;

	private Vector3 _TopPoint;

	private Quaternion mRotFrom;

	private Quaternion mRotTo;

	private bool mStartTimer;

	private float mJumpTimer;

	private Transform mEelTransform => _RenderPath.transform;

	private void Start()
	{
		if (_SndWaterSplash != null)
		{
			_Channel.pClip = _SndWaterSplash;
		}
		Initialize();
	}

	public void Initialize()
	{
		_RenderPath.gameObject.SetActive(_Active);
		mEelTransform.position = _StartPoint.position;
		mEelTransform.rotation = _StartPoint.rotation;
		mRotFrom.eulerAngles = mEelTransform.rotation.eulerAngles;
		Vector3 eulerAngles = mEelTransform.rotation.eulerAngles;
		eulerAngles.x = 0f - mEelTransform.rotation.eulerAngles.x;
		mRotTo.eulerAngles = eulerAngles;
		_EelTargetable._Active = false;
		_DragonFiringCSM.enabled = false;
		StartJumpIntervalTimer();
	}

	private void Update()
	{
		if (base.gameObject == null || mIsDestroying || !_Active)
		{
			return;
		}
		Animation eelAnim = _EelAnim;
		if (!mPlayedPartForGoingAbove && MultiplyVectors(mEelTransform.position, Vector3.up) > MultiplyVectors(_WaterHeightMarker.transform.position, Vector3.up))
		{
			PlaySplashEffects();
			eelAnim.CrossFade(_LaunchAnimName, 0.3f);
			eelAnim[_LaunchAnimName].wrapMode = WrapMode.Once;
			mPlayedPartForGoingAbove = true;
		}
		if (mPlayedPartForGoingAbove && !mPlayedPartForGoingBelow)
		{
			if (!mIdleAnimationPlayed && eelAnim.IsPlaying(_LaunchAnimName) && eelAnim[_LaunchAnimName].time + _LauchCrossFadeTime > eelAnim[_LaunchAnimName].length)
			{
				mIdleAnimationPlayed = true;
				eelAnim.CrossFade(_IdleAnimName, _LauchCrossFadeTime);
				eelAnim[_IdleAnimName].wrapMode = WrapMode.Loop;
			}
			if (MultiplyVectors(mEelTransform.position, Vector3.up) < MultiplyVectors(_WaterHeightMarker.transform.position, Vector3.up))
			{
				PlaySplashEffects();
				mPlayedPartForGoingBelow = true;
			}
		}
		if (mLamdaPosition < 1f)
		{
			mEelTransform.rotation = Quaternion.Lerp(mRotFrom, mRotTo, mLamdaPosition);
			mEelTransform.position = DoBezier(_StartPoint.position, mControlPoint, _EndPoint.position, mLamdaPosition);
			if (mLamdaPosition > 0.5f && mDelayTime < _DelayOnTop)
			{
				mDelayTime += Time.deltaTime;
			}
			else
			{
				mLamdaPosition += _Speed * Time.deltaTime;
			}
			if (mLamdaPosition >= 1f)
			{
				StartJumpIntervalTimer();
				_EelTargetable._Active = false;
				_DragonFiringCSM.enabled = false;
			}
		}
		if (mStartTimer)
		{
			mJumpTimer -= Time.deltaTime;
			if (mJumpTimer < 0f)
			{
				MakeAJump();
			}
		}
	}

	private void PlaySplashEffects()
	{
		if (_WaterSplashEffects != null)
		{
			GameObject gameObject = _WaterSplashEffects[Random.Range(0, _WaterSplashEffects.Length)];
			Object.Instantiate(gameObject, mEelTransform.position, gameObject.transform.rotation).transform.parent = _PrtParent.transform;
			_Channel?.Play();
		}
	}

	private void StartJumpIntervalTimer()
	{
		mLamdaPosition = 1f;
		mJumpTimer = Random.Range(1f, _JumpIntervalTime);
		mStartTimer = true;
	}

	private void MakeAJump()
	{
		_TopPoint = (_StartPoint.position + _EndPoint.position) / 2f + _StartPoint.up * Random.Range(_MinHeight, _MaxHeight);
		mControlPoint = DoBezierReverse(_StartPoint.position, _TopPoint, _EndPoint.position);
		mStartTimer = (mPlayedPartForGoingAbove = (mPlayedPartForGoingBelow = (mIdleAnimationPlayed = false)));
		mEelTransform.LookAt(_TopPoint);
		mDelayTime = (mLamdaPosition = 0f);
		_EelTargetable._Active = true;
		_DragonFiringCSM.enabled = true;
	}

	public void OnEelHit()
	{
		if (mIsDestroying)
		{
			return;
		}
		_Active = false;
		_RenderPath.gameObject.SetActive(value: false);
		mIsDestroying = true;
		SanctuaryManager.pCurPetInstance.UpdateMeter(SanctuaryPetMeterType.HAPPINESS, _PetHappiness);
		if (_EelBlastColors != null && _EelBlastColors.Length != 0 && _EelBlastEffectObj != null)
		{
			int num = Random.Range(0, _EelBlastColors.Length);
			GameObject obj = Object.Instantiate(_EelBlastEffectObj, mEelTransform.position, _EelBlastEffectObj.transform.rotation);
			obj.transform.parent = _PrtParent.transform;
			ParticleSystem.MainModule main = obj.GetComponent<ParticleSystem>().main;
			main.startColor = _EelBlastColors[num];
			if (_EelHit3DScore != null)
			{
				Vector3 position = SanctuaryManager.pCurPetInstance.GetHeadPosition() + _HappinessTextDragonOffset;
				GameObject obj2 = Object.Instantiate(_EelHit3DScore.gameObject, position, _EelHit3DScore.transform.rotation);
				TargetHit3DScore component = obj2.GetComponent<TargetHit3DScore>();
				component.mDisplayScore = (int)_PetHappiness;
				component.mDisplayText = _PetHappinessText._Text;
				obj2.transform.parent = _PrtParent.transform;
			}
		}
		if (base.gameObject != null)
		{
			Object.Destroy(base.gameObject, _DestroyDelay);
		}
	}

	public static Vector3 DoBezier(Vector3 Start, Vector3 Control, Vector3 End, float lamda)
	{
		if (lamda <= 0f)
		{
			return Start;
		}
		if (lamda >= 1f)
		{
			return End;
		}
		return (1f - lamda) * (1f - lamda) * Start + 2f * lamda * (1f - lamda) * Control + lamda * lamda * End;
	}

	public static Vector3 DoBezierReverse(Vector3 Start, Vector3 Mid, Vector3 End)
	{
		return (Mid - 0.25f * Start - 0.25f * End) / 0.5f;
	}

	private float MultiplyVectors(Vector3 a, Vector3 b)
	{
		return a.x * b.x + a.y * b.y + a.z * b.z;
	}
}
