using UnityEngine;
using UnityEngine.SceneManagement;

public class ObScale : MonoBehaviour
{
	public enum State
	{
		INACTIVE,
		SCALING,
		ACTIVE,
		RESTORING,
		REMOVED
	}

	public Vector3 _Scale = new Vector3(0.7f, 0.7f, 0.7f);

	public float _ChangeTime = 2f;

	public bool _RemoveOnLevelLoad = true;

	public bool _RequireOnScaleUpdate = true;

	private State mState;

	private float mTimer;

	private bool mOnScaleUpdate;

	private string mCurrentLevel;

	private Vector3 mOriginalScale = Vector3.one;

	public State pState => mState;

	public Vector3 pOriginalScale
	{
		get
		{
			return mOriginalScale;
		}
		set
		{
			mOriginalScale = value;
		}
	}

	public void Reset()
	{
		mState = State.INACTIVE;
		mTimer = 0f;
		mOnScaleUpdate = false;
	}

	public static ObScale Set(GameObject inObject, float inScale, float inChangeTime, bool inStartScale, bool inRequireOnScaleUpdate)
	{
		return Set(inObject, new Vector3(inScale, inScale, inScale), inChangeTime, inStartScale, inRequireOnScaleUpdate);
	}

	public static ObScale Set(GameObject inObject, Vector3 inScale, float inChangeTime, bool inStartScale, bool inRequireOnScaleUpdate)
	{
		ObScale obScale = inObject.GetComponent<ObScale>();
		if (obScale == null)
		{
			obScale = inObject.AddComponent<ObScale>();
		}
		UtDebug.Assert(obScale.mState != State.REMOVED);
		obScale._Scale = inScale;
		obScale._ChangeTime = inChangeTime;
		if (inStartScale)
		{
			obScale.OnScale();
		}
		obScale._RequireOnScaleUpdate = inRequireOnScaleUpdate;
		obScale.mCurrentLevel = RsResourceManager.pCurrentLevel;
		return obScale;
	}

	public static bool Move(GameObject inSource, GameObject inDestination)
	{
		ObScale component = inSource.GetComponent<ObScale>();
		if (component == null)
		{
			return false;
		}
		ObScale obScale = inDestination.GetComponent<ObScale>();
		if (obScale == null)
		{
			obScale = inDestination.AddComponent<ObScale>();
		}
		obScale._Scale = component._Scale;
		obScale._ChangeTime = component._ChangeTime;
		obScale.mState = component.mState;
		obScale.mTimer = component.mTimer;
		obScale.mOriginalScale = component.mOriginalScale;
		component.OnRemoveScaleImmediate();
		return true;
	}

	public void Awake()
	{
		mOriginalScale = base.transform.localScale;
	}

	private void Update()
	{
		switch (mState)
		{
		case State.SCALING:
			if (mTimer < _ChangeTime)
			{
				base.transform.localScale = Vector3.Lerp(mOriginalScale, _Scale, mTimer / _ChangeTime);
				mTimer += Time.deltaTime;
			}
			else
			{
				base.transform.localScale = _Scale;
				mState = State.ACTIVE;
			}
			break;
		case State.ACTIVE:
			if (_RequireOnScaleUpdate && !mOnScaleUpdate)
			{
				OnRemoveScale();
			}
			break;
		case State.RESTORING:
			if (mTimer > 0f)
			{
				base.transform.localScale = Vector3.Lerp(mOriginalScale, _Scale, mTimer / _ChangeTime);
				mTimer -= Time.deltaTime;
			}
			else
			{
				base.transform.localScale = mOriginalScale;
				mState = State.INACTIVE;
			}
			break;
		}
		mOnScaleUpdate = false;
		if (_RemoveOnLevelLoad && mCurrentLevel != RsResourceManager.pCurrentLevel)
		{
			mCurrentLevel = RsResourceManager.pCurrentLevel;
			OnRemoveScale();
		}
	}

	private void OnEnable()
	{
		SceneManager.sceneLoaded += OnSceneLoaded;
	}

	private void OnDisable()
	{
		SceneManager.sceneLoaded -= OnSceneLoaded;
	}

	private void OnSceneLoaded(Scene newScene, LoadSceneMode loadSceneMode)
	{
		if (_RemoveOnLevelLoad)
		{
			OnRemoveScale();
		}
	}

	private void OnScale()
	{
		mOnScaleUpdate = true;
		switch (mState)
		{
		case State.INACTIVE:
			mState = State.SCALING;
			mTimer = 0f;
			break;
		case State.RESTORING:
			mState = State.SCALING;
			break;
		}
	}

	private void OnRemoveScale()
	{
		switch (mState)
		{
		case State.SCALING:
			mState = State.RESTORING;
			break;
		case State.ACTIVE:
			mState = State.RESTORING;
			mTimer = _ChangeTime;
			break;
		case State.INACTIVE:
			break;
		}
	}

	public void OnRemoveScaleImmediate()
	{
		base.transform.localScale = mOriginalScale;
		mState = State.INACTIVE;
	}
}
