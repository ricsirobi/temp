using UnityEngine;

public class LookAtCamera : MonoBehaviour
{
	public bool _LookAtOnXAxis;

	public bool _LookAtOnYAxis;

	public bool _LookAtOnZAxis;

	private Transform mMainCameraTransform;

	private void Start()
	{
		mMainCameraTransform = Camera.main.transform;
	}

	private void Update()
	{
		Vector3 position = mMainCameraTransform.position;
		if (!_LookAtOnXAxis)
		{
			position.x = base.transform.position.x;
		}
		if (!_LookAtOnYAxis)
		{
			position.y = base.transform.position.y;
		}
		if (!_LookAtOnZAxis)
		{
			position.z = base.transform.position.z;
		}
		base.transform.LookAt(position);
	}
}
