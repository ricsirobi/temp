using System;
using UnityEngine;

public class EventTrigger : MonoBehaviour
{
	public float _ActiveTime = 20f;

	public float _DelayForActivate;

	public GameObject[] _ActivateObject;

	private GameObject[] mActivateObjectContainer;

	private bool mIsRunning;

	public TriggerAt[] _TriggerAt;

	public EnvironmentEvent _EnvironmentEvent;

	private float mPrevTriggerHitTime = -60f;

	public int _IntervalTime = 10;

	public int _SoundFxTimeBefore = 2;

	public SnSound _SoundFx;

	private bool mSoundFxPlayed;

	private void Start()
	{
		mActivateObjectContainer = new GameObject[_ActivateObject.Length];
		mIsRunning = false;
		_EnvironmentEvent.Start();
	}

	private void Update()
	{
		if (!mIsRunning && Time.realtimeSinceStartup - mPrevTriggerHitTime > 60f)
		{
			if (_IntervalTime > 0)
			{
				if (DateTime.Now.Minute % _IntervalTime == 0)
				{
					Invoke("ActivateEvent", _DelayForActivate);
					mIsRunning = true;
					_EnvironmentEvent.Activate();
					mPrevTriggerHitTime = Time.realtimeSinceStartup;
				}
				if ((DateTime.Now.Minute + _SoundFxTimeBefore) % _IntervalTime == 0 && !mSoundFxPlayed)
				{
					UtDebug.Log("_SoundFx at : " + DateTime.Now, 200);
					mSoundFxPlayed = true;
					if (_SoundFx != null && _SoundFx._AudioClip != null)
					{
						_SoundFx.Play();
					}
				}
			}
			if (!mIsRunning && _TriggerAt != null)
			{
				TriggerAt[] triggerAt = _TriggerAt;
				foreach (TriggerAt triggerAt2 in triggerAt)
				{
					if (triggerAt2.IsTriggered())
					{
						Invoke("ActivateEvent", _DelayForActivate);
						mIsRunning = true;
						_EnvironmentEvent.Activate();
						mPrevTriggerHitTime = Time.realtimeSinceStartup;
						break;
					}
					if (mSoundFxPlayed)
					{
						continue;
					}
					DateTime dateTime = DateTime.Now + new TimeSpan(0, _SoundFxTimeBefore, 0);
					if (triggerAt2._Hour == dateTime.Hour && triggerAt2._Min == dateTime.Minute)
					{
						mSoundFxPlayed = true;
						UtDebug.Log("_SoundFx at : " + DateTime.Now, 200);
						if (_SoundFx != null && _SoundFx._AudioClip != null)
						{
							_SoundFx.Play();
						}
					}
				}
			}
		}
		_EnvironmentEvent.Update();
	}

	public void ActivateEvent()
	{
		UtDebug.Log("Activate Event at : " + DateTime.Now, 200);
		for (int i = 0; i < _ActivateObject.Length; i++)
		{
			mActivateObjectContainer[i] = UnityEngine.Object.Instantiate(_ActivateObject[i], _ActivateObject[i].transform.position, _ActivateObject[i].transform.rotation);
			mActivateObjectContainer[i].transform.parent = base.gameObject.transform;
		}
		Invoke("DeActivateEvent", _ActiveTime);
	}

	public void DeActivateEvent()
	{
		UtDebug.Log("DeActivate Event at : " + DateTime.Now, 200);
		GameObject[] array = mActivateObjectContainer;
		for (int i = 0; i < array.Length; i++)
		{
			UnityEngine.Object.Destroy(array[i]);
		}
		_EnvironmentEvent.DeActivate();
		mIsRunning = false;
		mSoundFxPlayed = false;
	}
}
