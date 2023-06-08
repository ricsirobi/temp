using System.Collections.Generic;
using UnityEngine;

public class ObAmmoHit : MonoBehaviour
{
	public string _Action;

	public List<string> _ActionAmmoName;

	protected virtual void OnAmmoHit(ObAmmo ammo)
	{
		if (string.IsNullOrEmpty(_Action) || !(MissionManager.pInstance != null))
		{
			return;
		}
		bool flag = true;
		if (_ActionAmmoName != null && _ActionAmmoName.Count > 0)
		{
			flag = false;
			for (int i = 0; i < _ActionAmmoName.Count; i++)
			{
				if (ammo.gameObject.name.Contains(_ActionAmmoName[i]))
				{
					flag = true;
					break;
				}
			}
		}
		if (flag)
		{
			MissionManager.pInstance.CheckForTaskCompletion("Action", _Action, base.gameObject.name);
		}
	}
}
