using UnityEngine;

public class ObFaceCamera : KAMonoBase
{
	public GameObject _Camera;

	private Transform mCameraTransform;

	private void Awake()
	{
		if (_Camera == null)
		{
			mCameraTransform = Camera.main.transform;
		}
		else
		{
			mCameraTransform = _Camera.transform;
		}
	}

	private void LateUpdate()
	{
		if (mCameraTransform != null)
		{
			base.transform.LookAt(mCameraTransform);
		}
	}
}
