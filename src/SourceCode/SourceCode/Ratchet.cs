using UnityEngine;

public class Ratchet : KAMonoBase
{
	private Vector3 mInitialPosition;

	private Quaternion mInitialRotation;

	private void Start()
	{
		mInitialPosition = base.transform.position;
		mInitialRotation = base.transform.rotation;
	}

	private void LateUpdate()
	{
		base.transform.position = mInitialPosition;
		base.transform.rotation = mInitialRotation;
	}
}
