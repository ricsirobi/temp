using System.Collections.Generic;
using UnityEngine;

public class ObCarriedReceptacle : MonoBehaviour
{
	public AudioClip _Sound;

	public GameObject _ParticleEffect;

	public GameObject _MessageObject;

	public List<string> _AcceptableCarriedObjects;

	private void OnTriggerEnter(Collider coll)
	{
		if (!AvAvatar.IsCurrentPlayer(coll.gameObject))
		{
			return;
		}
		AvAvatarController avc = AvAvatar.pObject.GetComponent<AvAvatarController>();
		if (avc != null && avc.pCarriedObject != null && (_AcceptableCarriedObjects == null || _AcceptableCarriedObjects.Count <= 0 || _AcceptableCarriedObjects.Find((string item) => avc.pCarriedObject.name.Contains(item)) != null))
		{
			if ((bool)_Sound)
			{
				SnChannel.Play(_Sound);
			}
			if ((bool)_ParticleEffect)
			{
				Object.Instantiate(_ParticleEffect, base.transform.position, base.transform.rotation);
			}
			if (_MessageObject != null)
			{
				_MessageObject.BroadcastMessage("OnCarriedCollect", avc.pCarriedObject, SendMessageOptions.DontRequireReceiver);
			}
			avc.RemoveCarriedObject(forceDestroy: true);
		}
	}
}
