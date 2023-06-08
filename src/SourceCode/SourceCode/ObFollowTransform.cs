using UnityEngine;

public class ObFollowTransform : MonoBehaviour
{
	public string _TargetTransform;

	public Vector3 _Offset;

	public Transform _TargetTransformRef;

	public bool _RetryFind;

	public bool _MatchRotate;

	private void Awake()
	{
		if (_TargetTransformRef == null)
		{
			FindTransform();
		}
	}

	private void LateUpdate()
	{
		if (_TargetTransformRef != null)
		{
			Vector3 position = _TargetTransformRef.position;
			position += _Offset;
			base.transform.position = position;
			if (_MatchRotate)
			{
				base.transform.rotation = _TargetTransformRef.rotation;
			}
		}
		else
		{
			FindTransform();
		}
	}

	public void FindTransform()
	{
		if (!string.IsNullOrEmpty(_TargetTransform))
		{
			GameObject gameObject = GameObject.Find(_TargetTransform);
			if (gameObject != null)
			{
				_TargetTransformRef = gameObject.transform;
			}
			else if (!_RetryFind)
			{
				Debug.LogError("ERROR: ObFollowTransform - UNABLED TO FIND TARGET TRANSFORM: " + _TargetTransform);
				_TargetTransform = "";
			}
		}
	}

	public void ClearTransform()
	{
		_TargetTransformRef = null;
	}
}
