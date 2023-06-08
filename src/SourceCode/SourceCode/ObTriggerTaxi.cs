using System.Collections.Generic;
using UnityEngine;

public class ObTriggerTaxi : ObTrigger
{
	public List<GameObject> _IconObject;

	public override void DoTriggerAction(GameObject other)
	{
		DragonTaxiManager.pInstance.UnMount();
		base.DoTriggerAction(other);
	}

	public void SetIcon(Texture icon)
	{
		if (_IconObject == null)
		{
			return;
		}
		foreach (GameObject item in _IconObject)
		{
			Renderer component = item.GetComponent<Renderer>();
			if (component != null)
			{
				component.material.mainTexture = icon;
			}
		}
	}
}
