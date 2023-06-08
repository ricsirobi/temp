using UnityEngine;

public class AIBehavior_AttachObjectTo : AIBehavior
{
	public Transform _Object;

	public bool _UseObjectParent;

	public bool _Attach = true;

	public Vector3 _OffsetPosition = Vector3.zero;

	public Quaternion _OffsetRotation = Quaternion.identity;

	public string _BoneName;

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (_Object == null)
		{
			return SetState(AIBehaviorState.FAILED);
		}
		Transform bone = GetBone(Actor.transform);
		if (bone == null)
		{
			return SetState(AIBehaviorState.FAILED);
		}
		Transform transform = _Object;
		if (_UseObjectParent && _Object.parent != null && _Object.parent != bone)
		{
			transform = _Object.parent;
		}
		Vector3 position = transform.position;
		Quaternion rotation = transform.rotation;
		if (_Attach)
		{
			Rigidbody component = transform.GetComponent<Rigidbody>();
			if (component != null)
			{
				component.isKinematic = true;
			}
			transform.parent = bone;
			transform.position = position;
			transform.rotation = rotation;
			transform.localPosition = _OffsetPosition;
			transform.localRotation = _OffsetRotation;
		}
		else
		{
			transform.parent = null;
			transform.position = position;
			transform.rotation = rotation;
		}
		return SetState(AIBehaviorState.COMPLETED);
	}

	private Transform GetBone(Transform Root)
	{
		if (Root.name == _BoneName)
		{
			return Root;
		}
		for (int i = 0; i < Root.childCount; i++)
		{
			Transform bone = GetBone(Root.GetChild(i));
			if (bone != null)
			{
				return bone;
			}
		}
		ObLookAt componentInChildren = Root.GetComponentInChildren<ObLookAt>();
		if (componentInChildren != null)
		{
			return componentInChildren.transform;
		}
		return null;
	}
}
