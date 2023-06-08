using System;
using UnityEngine;

public class ObTriggerActivateComponent : ObTrigger
{
	[Serializable]
	public class ObActivateComponentInfo
	{
		public GameObject _Object;

		public string[] _ComponentNames;

		public bool _Activate = true;
	}

	public ObActivateComponentInfo _ActivateComponent;

	public void OnTrigger()
	{
		if (!(_ActivateComponent._Object != null))
		{
			return;
		}
		string[] componentNames = _ActivateComponent._ComponentNames;
		foreach (string type in componentNames)
		{
			Behaviour behaviour = _ActivateComponent._Object.GetComponent(type) as Behaviour;
			if (behaviour != null)
			{
				behaviour.enabled = _ActivateComponent._Activate;
			}
		}
	}
}
