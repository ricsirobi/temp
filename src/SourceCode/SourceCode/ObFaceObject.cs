using UnityEngine;

public class ObFaceObject : KAMonoBase
{
	public string _Object = "";

	public GameObject _TargetObject;

	public bool _IgnoreY;

	private Transform mObjectTransform;

	private void LateUpdate()
	{
		if (mObjectTransform == null)
		{
			if (_TargetObject != null)
			{
				mObjectTransform = _TargetObject.transform;
			}
			else
			{
				GameObject gameObject = GameObject.Find(_Object);
				if (gameObject != null)
				{
					mObjectTransform = gameObject.transform;
				}
			}
		}
		if (mObjectTransform != null)
		{
			Vector3 position = mObjectTransform.position;
			if (_IgnoreY)
			{
				position.y = base.transform.position.y;
			}
			base.transform.LookAt(position);
		}
	}
}
