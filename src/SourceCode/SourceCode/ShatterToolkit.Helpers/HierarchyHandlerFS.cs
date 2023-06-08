using UnityEngine;

namespace ShatterToolkit.Helpers;

[RequireComponent(typeof(ShatterTool))]
public class HierarchyHandlerFS : HierarchyHandler
{
	public float _ShatterVelocity = 5f;

	public override void PostSplit(GameObject[] newGameObjects)
	{
		base.PostSplit(newGameObjects);
		foreach (GameObject gameObject in newGameObjects)
		{
			gameObject.transform.parent = base.transform.parent;
			gameObject.transform.localPosition = Vector3.zero;
			gameObject.transform.localRotation = Quaternion.identity;
			gameObject.GetComponent<Rigidbody>().isKinematic = false;
			ApplyForce(gameObject.transform);
		}
	}

	private void ApplyForce(Transform inTransform)
	{
		AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (component != null)
		{
			inTransform.GetComponent<Rigidbody>().velocity = AvAvatar.forward * component.pVelocity.magnitude * _ShatterVelocity;
		}
	}
}
