using UnityEngine;

public class ObActivateRandom : MonoBehaviour
{
	public bool _AutoStart = true;

	public bool _Random;

	public MinMax _Interval;

	public MinMax _DisableInterval;

	public GameObject[] _Objects;

	public bool _EnableKeyInput;

	public bool _DisableInBetween;

	protected GameObject mActiveObject;

	private float mTimer;

	private int mCurrentIndex;

	private void Start()
	{
		mActiveObject = null;
		mTimer = 0f;
		mCurrentIndex = 0;
		if (_Objects != null && _Objects.Length != 0)
		{
			GameObject[] objects = _Objects;
			for (int i = 0; i < objects.Length; i++)
			{
				objects[i].SetActive(value: false);
			}
			if (_AutoStart)
			{
				ActivateObject(mCurrentIndex);
				mTimer = _Interval.GetRandomValue();
			}
		}
	}

	public void StartSequence(bool t)
	{
		ActivateObject(mCurrentIndex);
		mTimer = _Interval.GetRandomValue();
	}

	private void Update()
	{
		if (mTimer > 0f)
		{
			mTimer -= Time.deltaTime;
			if (mTimer <= 0f)
			{
				if (_DisableInBetween && mActiveObject != null)
				{
					mTimer = _DisableInterval.GetRandomValue();
					DeactivateObject();
				}
				else
				{
					mTimer = _Interval.GetRandomValue();
					if (_Random)
					{
						mCurrentIndex = Random.Range(0, _Objects.Length);
					}
					else
					{
						mCurrentIndex++;
						if (mCurrentIndex >= _Objects.Length)
						{
							mCurrentIndex = 0;
						}
					}
					ActivateObject(mCurrentIndex);
				}
			}
		}
		if (_EnableKeyInput)
		{
			if (Input.GetKey(KeyCode.Alpha1))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 0;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha2))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 1;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha3))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 2;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha4))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 3;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha5))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 4;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha6))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 5;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha7))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 6;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha8))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 7;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha9))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 8;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKey(KeyCode.Alpha0))
			{
				mTimer = _Interval.GetRandomValue();
				mCurrentIndex = 9;
				ActivateObject(mCurrentIndex);
			}
			else if (Input.GetKeyUp(KeyCode.Space))
			{
				NextObject();
			}
		}
	}

	public void StartRandomObject(MinMax interval)
	{
		_Interval = interval;
		_Random = true;
		mTimer = 0f;
	}

	public void StopRandomObject()
	{
		_Random = false;
	}

	public virtual void ActivateObject(int index)
	{
		if (base.enabled && _Objects != null && _Objects.Length != 0 && _Objects.Length > index)
		{
			if (mActiveObject != null)
			{
				mActiveObject.SetActive(value: false);
			}
			mActiveObject = _Objects[index];
			mActiveObject.SetActive(value: true);
			SplineControl component = mActiveObject.GetComponent<SplineControl>();
			if (component != null)
			{
				component.ResetSpline();
			}
		}
	}

	public virtual void DeactivateObject()
	{
		if (mActiveObject != null)
		{
			mActiveObject.SetActive(value: false);
		}
		mActiveObject = null;
	}

	public void NextObject()
	{
		mCurrentIndex++;
		if (mCurrentIndex > _Objects.Length)
		{
			mCurrentIndex = 0;
		}
		mTimer = _Interval.GetRandomValue();
		ActivateObject(mCurrentIndex);
	}
}
