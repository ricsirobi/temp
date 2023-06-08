using UnityEngine;

public class ObActivateCameraSwap : MonoBehaviour
{
	public bool _AutoStart = true;

	public bool _UseRandom;

	public float _RandomInterval = 5f;

	public GameObject[] _Cameras;

	public RenderTexture _OutputTexture;

	public bool _RandomizeTime;

	public bool _EnableKeyInput;

	public GameObject[] _Objects;

	private GameObject mActiveCamera;

	private float mRandomTimer;

	private int mMask;

	private int mCurIdx;

	private GameObject mActiveObject;

	private int mCurObjIdx;

	public virtual void Start()
	{
		mActiveCamera = null;
		mRandomTimer = 0f;
		mCurIdx = 0;
		mCurObjIdx = 0;
		if (_Cameras == null || _Cameras.Length == 0)
		{
			return;
		}
		GameObject[] cameras = _Cameras;
		for (int i = 0; i < cameras.Length; i++)
		{
			cameras[i].SetActive(value: false);
		}
		if (_OutputTexture != null)
		{
			SetOutputTexture(_OutputTexture);
		}
		if (_AutoStart)
		{
			ActivateCamera(mCurIdx);
			if (_RandomizeTime)
			{
				mRandomTimer = Random.Range(0f, _RandomInterval);
			}
			else
			{
				mRandomTimer = _RandomInterval;
			}
		}
		if (_Objects != null && _Objects.Length != 0)
		{
			cameras = _Objects;
			for (int i = 0; i < cameras.Length; i++)
			{
				cameras[i].SetActive(value: false);
			}
			if (_AutoStart)
			{
				ActivateObject(mCurObjIdx);
			}
		}
	}

	public virtual void StartCameraSequence(bool t)
	{
		ActivateCamera(mCurIdx);
		ActivateObject(mCurObjIdx);
		if (_RandomizeTime)
		{
			mRandomTimer = Random.Range(0f, _RandomInterval);
		}
		else
		{
			mRandomTimer = _RandomInterval;
		}
	}

	public virtual void DeactivateCamera()
	{
		if (mActiveCamera != null)
		{
			mActiveCamera.SetActive(value: false);
		}
		mActiveCamera = null;
	}

	public virtual void Update()
	{
		if (mActiveCamera == null)
		{
			return;
		}
		if (mRandomTimer > 0f && _Cameras.Length > 1)
		{
			mRandomTimer -= Time.deltaTime;
			if (mRandomTimer <= 0f)
			{
				if (_UseRandom)
				{
					mCurIdx = Random.Range(0, _Cameras.Length);
					mCurObjIdx = Random.Range(0, _Objects.Length);
				}
				else
				{
					mCurIdx++;
					if (mCurIdx > _Cameras.Length)
					{
						mCurIdx = 0;
					}
					mCurObjIdx++;
					if (mCurObjIdx > _Objects.Length)
					{
						mCurObjIdx = 0;
					}
				}
				ActivateCamera(mCurIdx);
				ActivateObject(mCurObjIdx);
				if (_RandomizeTime)
				{
					mRandomTimer = Random.Range(0f, _RandomInterval);
				}
				else
				{
					mRandomTimer = _RandomInterval;
				}
			}
		}
		if (_EnableKeyInput)
		{
			if (Input.GetKey(KeyCode.Alpha1))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 0;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha2))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 1;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha3))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 2;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha4))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 3;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha5))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 4;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha6))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 5;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha7))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 6;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha8))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 7;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha9))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 8;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKey(KeyCode.Alpha0))
			{
				mRandomTimer = _RandomInterval;
				mCurIdx = 9;
				ActivateCamera(mCurIdx);
			}
			else if (Input.GetKeyUp(KeyCode.Space))
			{
				NextCamera();
			}
		}
	}

	public void SetOutputTexture(RenderTexture t)
	{
		_OutputTexture = t;
		GameObject[] cameras = _Cameras;
		for (int i = 0; i < cameras.Length; i++)
		{
			cameras[i].GetComponent<Camera>().targetTexture = t;
		}
	}

	public void StartRandomCamera(float interval)
	{
		_RandomInterval = interval;
		_UseRandom = true;
		mRandomTimer = 0f;
	}

	public void StopRandomCamera()
	{
		_UseRandom = false;
	}

	public void ActivateCamera(float idx)
	{
		if (!base.enabled || _Cameras == null || _Cameras.Length == 0)
		{
			return;
		}
		int num = (int)idx;
		if (_Cameras.Length > num)
		{
			if (mMask == 0)
			{
				mMask = _Cameras[0].GetComponent<Camera>().cullingMask;
			}
			if (mActiveCamera != null)
			{
				mActiveCamera.SetActive(value: false);
			}
			mActiveCamera = _Cameras[num];
			mActiveCamera.GetComponent<Camera>().cullingMask = mMask;
			mActiveCamera.SetActive(value: true);
			SplineControl splineControl = (SplineControl)mActiveCamera.GetComponent(typeof(SplineControl));
			if (splineControl != null)
			{
				splineControl.ResetSpline();
			}
		}
	}

	public void NextCamera()
	{
		mCurIdx++;
		if (mCurIdx > _Cameras.Length)
		{
			mCurIdx = 0;
		}
		mRandomTimer = _RandomInterval;
		ActivateCamera(mCurIdx);
	}

	public void ActivateObject(float idx)
	{
		if (!base.enabled || _Objects == null || _Objects.Length == 0)
		{
			return;
		}
		int num = (int)idx;
		if (_Objects.Length > num)
		{
			if (mActiveObject != null)
			{
				mActiveObject.SetActive(value: false);
			}
			mActiveObject = _Objects[num];
			mActiveObject.SetActive(value: true);
			SplineControl splineControl = (SplineControl)mActiveObject.GetComponent(typeof(SplineControl));
			if (splineControl != null)
			{
				splineControl.ResetSpline();
			}
		}
	}
}
