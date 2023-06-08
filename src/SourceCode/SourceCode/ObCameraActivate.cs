using UnityEngine;

public class ObCameraActivate : MonoBehaviour
{
	public GameObject[] _Cameras;

	private GameObject mActiveCamera;

	private static int mCameraIndex;

	private void Start()
	{
		if (_Cameras == null || _Cameras.Length == 0)
		{
			return;
		}
		for (int i = 0; i < _Cameras.Length; i++)
		{
			_Cameras[i].SetActive(i == mCameraIndex);
			if (i == mCameraIndex)
			{
				mActiveCamera = _Cameras[i];
			}
		}
	}

	private void Update()
	{
		if (_Cameras != null)
		{
			if (Input.GetKey(KeyCode.Alpha1))
			{
				ActivateCamera(0);
			}
			else if (Input.GetKey(KeyCode.Alpha2))
			{
				ActivateCamera(1);
			}
			else if (Input.GetKey(KeyCode.Alpha3))
			{
				ActivateCamera(2);
			}
			else if (Input.GetKey(KeyCode.Alpha4))
			{
				ActivateCamera(3);
			}
			else if (Input.GetKey(KeyCode.Alpha5))
			{
				ActivateCamera(4);
			}
			else if (Input.GetKey(KeyCode.Alpha6))
			{
				ActivateCamera(5);
			}
			else if (Input.GetKey(KeyCode.Alpha7))
			{
				ActivateCamera(6);
			}
			else if (Input.GetKey(KeyCode.Alpha8))
			{
				ActivateCamera(7);
			}
			else if (Input.GetKey(KeyCode.Alpha9))
			{
				ActivateCamera(8);
			}
			else if (Input.GetKey(KeyCode.Alpha0))
			{
				ActivateCamera(9);
			}
			else if (Input.GetKeyUp(KeyCode.Space))
			{
				ChangeCamera();
			}
		}
	}

	private void ActivateCamera(int index)
	{
		if (_Cameras.Length > index)
		{
			mActiveCamera.SetActive(value: false);
			mCameraIndex = index;
			mActiveCamera = _Cameras[index];
			mActiveCamera.SetActive(value: true);
		}
		else if (index != mCameraIndex)
		{
			mActiveCamera.SetActive(value: false);
			mCameraIndex = 0;
			mActiveCamera = _Cameras[0];
			mActiveCamera.SetActive(value: true);
		}
	}

	public void ChangeCamera()
	{
		ActivateCamera(mCameraIndex + 1);
	}
}
