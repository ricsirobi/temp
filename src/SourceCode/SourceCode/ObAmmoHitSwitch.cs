using System.Collections.Generic;
using UnityEngine;

public class ObAmmoHitSwitch : ObAmmoHit
{
	public List<GameObject> _MessageObjects = new List<GameObject>();

	protected override void OnAmmoHit(ObAmmo ammo)
	{
		base.OnAmmoHit(ammo);
		if (_MessageObjects.Count <= 0)
		{
			return;
		}
		foreach (GameObject messageObject in _MessageObjects)
		{
			if (messageObject != null)
			{
				messageObject.SendMessage("OnStateChange", true, SendMessageOptions.DontRequireReceiver);
				messageObject.SendMessage("OnSwitchOn", base.name, SendMessageOptions.DontRequireReceiver);
			}
		}
	}
}
