using UnityEngine;

[RequireComponent(typeof(CreateShadows))]
public class CollectibleShadow : MonoBehaviour
{
	public bool _GenerateShadowsOnStart;

	private Vector3 mStartPosition;

	private bool mShadowsOn;

	private bool mStopFloat;

	private bool mInitShadow;

	private bool mStartFloatNextCycle;

	private void Start()
	{
		mStartPosition = base.transform.position;
		if (_GenerateShadowsOnStart)
		{
			mStopFloat = true;
		}
	}

	private void OnEnable()
	{
		if (!mShadowsOn)
		{
			mStopFloat = true;
		}
	}

	private void Update()
	{
		if (mStartFloatNextCycle)
		{
			TurnOnFloat(bEnable: true);
		}
		if (mInitShadow)
		{
			TurnOnShadow();
			mStartFloatNextCycle = true;
			mInitShadow = false;
		}
		if (mStopFloat)
		{
			TurnOnFloat(bEnable: false);
			base.transform.position = mStartPosition;
			mInitShadow = true;
			mStopFloat = false;
		}
	}

	private void TurnOnFloat(bool bEnable)
	{
		ObFloat obFloat = (ObFloat)GetComponent("ObFloat");
		if (obFloat == null)
		{
			Debug.LogError("Don't have ObFloat!!");
		}
		else
		{
			obFloat.enabled = bEnable;
		}
	}

	private void TurnOnShadow()
	{
		if (!mShadowsOn)
		{
			CreateShadows createShadows = (CreateShadows)GetComponent("CreateShadows");
			if (createShadows == null)
			{
				Debug.LogError("Don't have CreateShadows!!");
				return;
			}
			createShadows.TurnOnShadow();
			mShadowsOn = true;
		}
	}
}
