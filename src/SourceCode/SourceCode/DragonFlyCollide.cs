using UnityEngine;

public class DragonFlyCollide : KAMonoBase
{
	private AvAvatarController mAvatarController;

	public GameObject _OnHitMsgObject;

	public LayerMask _CollisionMask = 12308;

	private void Start()
	{
		if (base.gameObject.layer == LayerMask.NameToLayer("IgnoreGroundRay"))
		{
			base.gameObject.layer = LayerMask.NameToLayer("Ignore Raycast");
		}
		if (base.gameObject.transform.parent.gameObject.GetComponent<SanctuaryPet>() != SanctuaryManager.pCurPetInstance)
		{
			base.gameObject.SetActive(value: false);
		}
	}

	private void OnTriggerEnter(Collider other)
	{
		if (!HasCollided(other))
		{
			return;
		}
		if (mAvatarController == null)
		{
			mAvatarController = AvAvatar.pObject.GetComponent<AvAvatarController>();
		}
		if (mAvatarController != null)
		{
			bool flag = mAvatarController.CheckCollision(collider, other);
			if (_OnHitMsgObject != null)
			{
				_OnHitMsgObject.SendMessage("OnDragonHit", flag);
			}
		}
	}

	private void OnTriggerStay(Collider other)
	{
		if (HasCollided(other) && mAvatarController != null)
		{
			mAvatarController.CheckCollision(collider, other, onCollisionStay: true);
		}
	}

	private bool HasCollided(Collider other)
	{
		if (!other.gameObject.CompareTag("IgnoreFlyCollider") && !other.gameObject.CompareTag("FishingZone") && ((int)_CollisionMask & (1 << other.gameObject.layer)) != 1 << other.gameObject.layer)
		{
			return !(base.gameObject.name == other.gameObject.name);
		}
		return false;
	}
}
