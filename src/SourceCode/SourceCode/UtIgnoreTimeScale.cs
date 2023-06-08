using UnityEngine;

public class UtIgnoreTimeScale : MonoBehaviour
{
	private float mTime;

	private float mDelta;

	private float mTotalTime;

	public static UtIgnoreTimeScale mInstance;

	public static float pRealTimeDelta => mInstance.mDelta;

	public static float pTotalTime => mInstance.mTotalTime;

	private void OnEnable()
	{
		mTime = Time.realtimeSinceStartup;
	}

	private void Start()
	{
		if (mInstance == null)
		{
			mInstance = this;
			mTime = Time.realtimeSinceStartup;
			Object.DontDestroyOnLoad(base.gameObject);
			UtMobileUtilities.AddToPersistentScriptList(this);
		}
		else
		{
			Object.Destroy(base.gameObject);
		}
	}

	protected void Update()
	{
		float realtimeSinceStartup = Time.realtimeSinceStartup;
		mDelta = Mathf.Max(0f, realtimeSinceStartup - mTime);
		mTotalTime += mDelta;
		mTime = realtimeSinceStartup;
	}
}
