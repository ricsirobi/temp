using UnityEngine;

public class CarCamera : KAMonoBase
{
	public enum State
	{
		NONE = -1,
		TRACK,
		ORBIT,
		SPECTATING
	}

	public const float VELOCITY_MIN_THRESHOLD = 1.5f;

	public Transform _Target;

	public float _Height = 1f;

	public float _HeightDisplacement = 1f;

	public float _PositionDamp = 3f;

	public float _VelocityDamp = 3f;

	public float _Distance = 4f;

	public float _MinFOV = 50f;

	public float _MaxFOV = 90f;

	public float _FarClipValue = 500f;

	private RaycastHit mHit;

	private Vector3 mPrevVelocity = Vector3.zero;

	private LayerMask mRaycastLayers = -1;

	private Vector3 mCurrentVelocity = Vector3.zero;

	private State mState;

	private float mPrevSpeedFactor = 1f;

	private Vector3 mPrevFrameTargetFwd;

	public float _OrbitSpeed = 5f;

	public float _Time = -1.7f;

	public Transform mSpectatingTarget;

	private AudioListener mAudioListener;

	private CharacterController mCharacterController;

	private bool mLookBack;

	public AudioListener pAudioListener => mAudioListener;

	public Transform pSpectatingTarget
	{
		set
		{
			mSpectatingTarget = value;
		}
	}

	public State pState
	{
		get
		{
			return mState;
		}
		set
		{
			mState = value;
			_Time = -1.57f / _OrbitSpeed;
		}
	}

	public bool LookBack
	{
		get
		{
			return mLookBack;
		}
		set
		{
			mLookBack = value;
		}
	}

	private void Start()
	{
		mRaycastLayers = 1 << LayerMask.NameToLayer("Ground");
		if (_MaxFOV > 120f)
		{
			_MaxFOV = 120f;
		}
	}

	private void Update()
	{
		if (mCharacterController == null || _Target == null)
		{
			mCharacterController = _Target.GetComponent<CharacterController>();
		}
		else if (mState == State.TRACK)
		{
			Vector3 vector = mCharacterController.velocity;
			vector.y = 0f;
			if (vector.sqrMagnitude < 1.5f)
			{
				vector = GetTarget().forward;
			}
			float num = Vector3.Dot(vector, GetTarget().forward);
			if ((mLookBack && num > 0f) || (!mLookBack && num < 0f))
			{
				vector *= -1f;
			}
			mCurrentVelocity = Vector3.Lerp(mPrevVelocity, vector, _VelocityDamp * Time.deltaTime);
			mCurrentVelocity.y = 0f;
			mPrevVelocity = mCurrentVelocity;
		}
	}

	private void LateUpdate()
	{
		if (mState == State.TRACK)
		{
			float b = Mathf.Clamp01(mCharacterController.velocity.magnitude / 120f);
			float t = (mPrevSpeedFactor = Mathf.Lerp(mPrevSpeedFactor, b, Time.deltaTime));
			base.camera.farClipPlane = _FarClipValue;
			base.camera.fieldOfView = Mathf.Lerp(_MinFOV, _MaxFOV, t);
			float num = 1f * ((base.camera.fieldOfView - _MinFOV) / (_MaxFOV - _MinFOV));
			float num2 = Mathf.Lerp(_Distance * num + _PositionDamp, _Distance * num, t);
			Vector3 vector = (mPrevFrameTargetFwd = Vector3.Slerp(mPrevFrameTargetFwd, GetTarget().forward, 2f * Time.deltaTime));
			mCurrentVelocity = mCurrentVelocity.normalized;
			Vector3 vector2 = GetTarget().position + Vector3.up * _Height;
			Vector3 vector3 = vector2 - mCurrentVelocity * num2;
			vector3.y = vector2.y + _HeightDisplacement - vector.y * 3f;
			Vector3 direction = vector3 - vector2;
			if (Physics.Raycast(vector2, direction, out mHit, num2, mRaycastLayers))
			{
				vector3 = mHit.point;
			}
			base.transform.position = vector3;
			base.transform.LookAt(vector2);
		}
	}

	private Transform GetTarget()
	{
		if (mState == State.SPECTATING)
		{
			return mSpectatingTarget;
		}
		return _Target;
	}

	public void Reset()
	{
		Vector3 position = GetTarget().position + Vector3.up * _HeightDisplacement;
		position -= GetTarget().forward * _Distance;
		base.transform.position = position;
		base.transform.LookAt(GetTarget().position + Vector3.up * _Height);
		mCurrentVelocity = (mPrevVelocity = base.transform.forward * 0.01f);
	}
}
