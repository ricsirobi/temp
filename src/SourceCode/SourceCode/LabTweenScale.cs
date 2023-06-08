using UnityEngine;

public class LabTweenScale : MonoBehaviour
{
	private bool mScaling;

	protected bool mDestroyOnTimeout;

	private Vector3 mStartScaleValue = Vector3.zero;

	private Vector3 mEndScaleValue = Vector3.zero;

	private float mScaleTimer;

	private float mScaleTime;

	public void Update()
	{
		if (mScaling)
		{
			UpdateScale();
		}
	}

	public static void Scale(GameObject inObj, bool inDestroyOnTimeout, float inDuration, Vector3 inEndScale)
	{
		if (!(inObj == null))
		{
			LabTweenScale labTweenScale = inObj.GetComponent<LabTweenScale>();
			if (labTweenScale == null)
			{
				labTweenScale = inObj.AddComponent<LabTweenScale>();
			}
			labTweenScale.StartScaling(inDestroyOnTimeout, inDuration, inObj.transform.localScale, inEndScale);
		}
	}

	public static bool IsScaling(GameObject inObj)
	{
		if (inObj == null)
		{
			return false;
		}
		LabTweenScale component = inObj.GetComponent<LabTweenScale>();
		if (component == null)
		{
			return false;
		}
		return component.mScaling;
	}

	public void StartScaling(bool inDestroyOnTimeout, float inDuration, Vector3 inStartScale, Vector3 inEndScale)
	{
		mScaling = true;
		mDestroyOnTimeout = inDestroyOnTimeout;
		mStartScaleValue = inStartScale;
		mEndScaleValue = inEndScale;
		mScaleTimer = 0f;
		mScaleTime = inDuration;
	}

	private void UpdateScale()
	{
		mScaleTimer += Time.deltaTime;
		float t = mScaleTimer / mScaleTime;
		Vector3 localScale = Vector3.Lerp(mStartScaleValue, mEndScaleValue, t);
		base.transform.localScale = localScale;
		if (mScaleTimer > mScaleTime)
		{
			StopScaling();
			if (mDestroyOnTimeout)
			{
				Object.Destroy(base.gameObject);
			}
		}
	}

	protected virtual void StopScaling()
	{
		mScaling = false;
		SendMessage("OnScaleStopped", SendMessageOptions.DontRequireReceiver);
	}
}
