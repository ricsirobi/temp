using UnityEngine;

public class UtNumberTicker : MonoBehaviour
{
	public int _StartValue;

	public int _EndValue = 100;

	public float _MaxTime = 5f;

	public float _ChangeInterval = 0.2f;

	public GameObject _MessageObject;

	public string _UpdateMessageString = "";

	public int _Step = 7;

	private bool mActive;

	private int mCurrentValue;

	private bool mIncrease = true;

	private float mUpdateTimer;

	private void Start()
	{
	}

	private void Update()
	{
		if (!mActive)
		{
			return;
		}
		mUpdateTimer -= Time.deltaTime;
		if (!(mUpdateTimer <= 0f))
		{
			return;
		}
		mUpdateTimer = _ChangeInterval;
		if (mIncrease)
		{
			mCurrentValue += _Step;
			if (mCurrentValue >= _EndValue)
			{
				mCurrentValue = _EndValue;
				mActive = false;
			}
		}
		else
		{
			mCurrentValue -= _Step;
			if (mCurrentValue <= _EndValue)
			{
				mCurrentValue = _EndValue;
				mActive = false;
			}
		}
		if (_MessageObject != null)
		{
			_MessageObject.SendMessage(_UpdateMessageString, mCurrentValue, SendMessageOptions.DontRequireReceiver);
		}
	}

	public void StartTicker()
	{
		mCurrentValue = _StartValue;
		if (_StartValue < _EndValue)
		{
			mIncrease = true;
		}
		else
		{
			mIncrease = false;
		}
		mUpdateTimer = _ChangeInterval;
		mActive = true;
	}

	public void StopTicker()
	{
		mActive = false;
	}
}
