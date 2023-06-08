using UnityEngine;

public class PetMountedCheck : KAMonoBase
{
	public Transform _ResetMarker;

	private void OnTriggerStay(Collider other)
	{
		if (other.gameObject == AvAvatar.pObject)
		{
			AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
			if (component != null && !component.pPlayerMounted)
			{
				AvAvatar.TeleportTo(_ResetMarker.position, _ResetMarker.forward);
			}
		}
	}
}
