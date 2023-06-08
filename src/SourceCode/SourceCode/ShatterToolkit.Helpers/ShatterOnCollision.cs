using UnityEngine;

namespace ShatterToolkit.Helpers;

public class ShatterOnCollision : MonoBehaviour
{
	public float requiredVelocity = 1f;

	public float cooldownTime = 0.5f;

	protected float timeSinceInstantiated;

	public void Update()
	{
		timeSinceInstantiated += Time.deltaTime;
	}

	public virtual void OnCollisionEnter(Collision collision)
	{
		if (!(timeSinceInstantiated >= cooldownTime) || !(collision.relativeVelocity.magnitude >= requiredVelocity))
		{
			return;
		}
		ContactPoint[] contacts = collision.contacts;
		for (int i = 0; i < contacts.Length; i++)
		{
			ContactPoint contactPoint = contacts[i];
			if (contactPoint.otherCollider == collision.collider)
			{
				contactPoint.thisCollider.SendMessage("Shatter", contactPoint.point, SendMessageOptions.DontRequireReceiver);
				break;
			}
		}
	}
}
