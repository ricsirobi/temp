using UnityEngine;

public class ObFaceCamera180 : KAMonoBase
{
	public GameObject _Camera;

	public bool _IgnoreY = true;

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
			Vector3 forward = mCameraTransform.forward;
			if (_IgnoreY)
			{
				forward.y = 0f;
				forward.Normalize();
			}
			base.transform.forward = forward;
		}
	}
}
