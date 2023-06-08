using UnityEngine;

public class ObSpeedBooster : MonoBehaviour
{
	public float _BoostTime = 3f;

	private void OnTriggerEnter(Collider inCollider)
	{
		if (inCollider.gameObject == AvAvatar.pObject)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null)
			{
				component.pFlyingBoostTimer = _BoostTime;
				component.pFlyingFlapCooldownTimer = _BoostTime;
			}
		}
	}
}
