using System.Collections;
using UnityEngine;

public class ObAvatarRespawn : MonoBehaviour
{
	public GameObject _DefaultMarker;

	public static GameObject _Marker;

	public bool _IgnoreFlying = true;

	public bool _CheckMMOVisit;

	public virtual void OnTriggerEnter(Collider c)
	{
		DoRespawn(c.gameObject);
	}

	public virtual bool DoRespawn(GameObject obj)
	{
		if (AvAvatar.IsCurrentPlayer(obj))
		{
			bool flag = true;
			if (_IgnoreFlying && AvAvatar.pSubState == AvAvatarSubState.FLYING && SanctuaryManager.pCurPetInstance != null && SanctuaryManager.pCurPetData.pStage >= RaisedPetStage.TEEN)
			{
				AvAvatarController component = AvAvatar.pObject.GetComponent<AvAvatarController>();
				flag = component != null && component.pFlyingGlidingMode;
			}
			if (flag)
			{
				StartCoroutine(Respawn());
				return true;
			}
		}
		return false;
	}

	private IEnumerator Respawn()
	{
		yield return new WaitForEndOfFrame();
		if (_DefaultMarker != null)
		{
			AvAvatar.TeleportToObject(_DefaultMarker);
		}
		else if (_Marker != null)
		{
			AvAvatar.TeleportToObject(_Marker);
		}
	}
}
