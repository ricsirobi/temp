using UnityEngine;

public class WorldEventResetPlayer : MonoBehaviour
{
	public WorldEventTarget WETarget;

	private void OnTriggerEnter(Collider collider)
	{
		if (!(WETarget == null) && !WETarget.pDying && AvAvatar.pObject != null && collider.gameObject == AvAvatar.pObject)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (SanctuaryManager.pCurPetInstance != null)
			{
				SanctuaryManager.pCurPetInstance.SetMeter(SanctuaryPetMeterType.HEALTH, 0f);
			}
			component.TakeHit(component._Stats._CurrentHealth);
		}
	}
}
