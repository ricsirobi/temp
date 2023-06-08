using System.Collections.Generic;
using UnityEngine;

namespace SquadTactics;

public class Weapon : MonoBehaviour
{
	[HideInInspector]
	public List<Ability> _Abilities;

	private void Awake()
	{
		_Abilities.Clear();
		for (int i = 0; i < base.transform.childCount; i++)
		{
			Ability component = base.transform.GetChild(i).GetComponent<Ability>();
			_Abilities.Add(component);
		}
	}
}
