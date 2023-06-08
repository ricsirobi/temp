using UnityEngine;

public class ObTriggerDisallowDragon : MonoBehaviour
{
	public Transform _DragonStayMarker;

	private static ObTriggerDisallowDragon mEnteredTrigger;

	private void OnTriggerEnter(Collider collider)
	{
		if (!(collider.gameObject != AvAvatar.pObject))
		{
			Enter();
		}
	}

	private void OnTriggerStay(Collider collider)
	{
		if (!(collider.gameObject != AvAvatar.pObject))
		{
			Enter();
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (!(collider.gameObject != AvAvatar.pObject) && !(mEnteredTrigger != this))
		{
			Exit();
		}
	}

	private void Enter()
	{
		mEnteredTrigger = this;
		if (SanctuaryManager.pInstance != null && (!SanctuaryManager.pInstance.pDisablePetSwitch || AvAvatar.pSubState == AvAvatarSubState.GLIDING))
		{
			SanctuaryManager.pInstance.EnableAllPets(enable: false, _DragonStayMarker);
		}
	}

	private void Exit()
	{
		mEnteredTrigger = null;
		if (SanctuaryManager.pInstance != null && AvAvatar.pSubState != AvAvatarSubState.UWSWIMMING)
		{
			SanctuaryManager.pInstance.EnableAllPets(enable: true);
		}
	}

	private void OnDestroy()
	{
		if (mEnteredTrigger == this)
		{
			Exit();
		}
	}
}
