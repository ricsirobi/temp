using UnityEngine;

public class ObTriggerSetEnvironmentalEffects : MonoBehaviour
{
	public EnvironmentalEffects _EnvironmentalEffects;

	private static ObTriggerSetEnvironmentalEffects mEnteredTrigger;

	private void OnTriggerEnter(Collider collider)
	{
		if (!(collider.gameObject != AvAvatar.pObject))
		{
			if (mEnteredTrigger != null)
			{
				CharacterController collider2 = (CharacterController)AvAvatar.pObject.GetComponent<Collider>();
				mEnteredTrigger.OnTriggerExit(collider2);
			}
			mEnteredTrigger = this;
			if (_EnvironmentalEffects != null)
			{
				_EnvironmentalEffects.EnableEffects(enable: true);
			}
		}
	}

	private void OnTriggerExit(Collider collider)
	{
		if (!(collider.gameObject != AvAvatar.pObject) && !(mEnteredTrigger != this))
		{
			mEnteredTrigger = null;
			if (_EnvironmentalEffects != null)
			{
				_EnvironmentalEffects.EnableEffects(enable: false);
			}
		}
	}
}
