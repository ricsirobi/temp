using UnityEngine;

public class ObPlatform : MonoBehaviour
{
	public bool _IsOffset;

	public Vector3 _EndPosition;

	public float _Time;

	public ObPlatformParameters _StartParams;

	public ObPlatformParameters _EndParams;

	public GameObject _PlayerPlatform;

	private Vector3 mStartPosition;

	private Vector3 mEndPosition;

	private PlatformState mState;

	private float mTimer;

	private PlatformDirection mDir;

	private void Awake()
	{
		mStartPosition = base.transform.position;
		if (_IsOffset)
		{
			mEndPosition = mStartPosition + base.transform.TransformDirection(_EndPosition);
		}
		else
		{
			mEndPosition = _EndPosition;
		}
	}

	private void Start()
	{
		if (!_StartParams._Activate)
		{
			Activate();
		}
	}

	private void LateUpdate()
	{
		bool flag = false;
		Vector3 position = base.transform.position;
		switch (mState)
		{
		case PlatformState.DELAYED:
			mTimer -= Time.deltaTime;
			if (mTimer <= 0f)
			{
				mState = PlatformState.MOVING;
				mTimer = 0f - mTimer;
			}
			break;
		case PlatformState.MOVING:
			mTimer += Time.deltaTime;
			if (mTimer >= _Time)
			{
				Stop();
				flag = true;
			}
			break;
		}
		if (mState == PlatformState.MOVING)
		{
			flag = true;
			float t = mTimer / _Time;
			if (mDir == PlatformDirection.TO_END)
			{
				base.transform.position = Vector3.Lerp(mStartPosition, mEndPosition, t);
			}
			else
			{
				base.transform.position = Vector3.Lerp(mEndPosition, mStartPosition, t);
			}
		}
		if (flag && AvAvatar.pState != 0 && AvAvatar.mTransform.parent != base.transform && _PlayerPlatform == null)
		{
			Vector3 position2 = AvAvatar.position;
			CharacterController characterController = (CharacterController)AvAvatar.pObject.GetComponent<Collider>();
			position2.y += characterController.height;
			if (base.transform.position.y > position.y)
			{
				position2.y += base.transform.position.y - position.y;
			}
			Ray ray = new Ray(position2, Vector3.up * -1f);
			Renderer renderer = (Renderer)base.transform.GetComponentInChildren(typeof(Renderer));
			if (renderer != null && renderer.bounds.IntersectRay(ray) && (bool)UtUtilities.GetGroundHeight(position2, float.PositiveInfinity, out var groundHeight) && groundHeight > AvAvatar.position.y)
			{
				AvAvatar.position = new Vector3(AvAvatar.position.x, groundHeight + 0.01f, AvAvatar.position.z);
			}
		}
	}

	private void Activate()
	{
		mState = PlatformState.MOVING;
		mTimer = 0f;
		GetComponentInChildren<ObPlatformTrigger>().ToggleTrigger(active: false);
		if (mDir == PlatformDirection.TO_END)
		{
			if (_StartParams._Delay > 0f)
			{
				mState = PlatformState.DELAYED;
				mTimer = _StartParams._Delay;
			}
		}
		else if (_EndParams._Delay > 0f)
		{
			mState = PlatformState.DELAYED;
			mTimer = _EndParams._Delay;
		}
	}

	private void Stop()
	{
		if (mDir == PlatformDirection.TO_END)
		{
			mDir = PlatformDirection.TO_START;
			base.transform.position = mEndPosition;
			if (_EndParams._Activate)
			{
				mState = PlatformState.WAITING;
			}
			else if (_EndParams._Delay > 0f)
			{
				mState = PlatformState.DELAYED;
				mTimer = _EndParams._Delay - (mTimer - _Time);
			}
			else
			{
				mState = PlatformState.MOVING;
				mTimer -= _Time;
			}
			return;
		}
		mDir = PlatformDirection.TO_END;
		base.transform.position = mStartPosition;
		if (_StartParams._Activate)
		{
			mState = PlatformState.WAITING;
			GetComponentInChildren<ObPlatformTrigger>().ToggleTrigger(active: true);
		}
		else if (_StartParams._Delay > 0f)
		{
			mState = PlatformState.DELAYED;
			mTimer = _StartParams._Delay - (mTimer - _Time);
		}
		else
		{
			mState = PlatformState.MOVING;
			mTimer -= _Time;
		}
	}

	private void OnActivate()
	{
		if (mState == PlatformState.WAITING)
		{
			Activate();
		}
	}

	public void OnRestart(bool dummy)
	{
		base.transform.position = mStartPosition;
	}
}
