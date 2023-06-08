using System;
using UnityEngine;

[Serializable]
public class KASkinPositionInfo
{
	public KASkinPositionWidget[] _ApplyTo;

	public void ShowPositionEffect(bool inShowEffect)
	{
		if (_ApplyTo == null)
		{
			return;
		}
		KASkinPositionWidget[] applyTo = _ApplyTo;
		foreach (KASkinPositionWidget kASkinPositionWidget in applyTo)
		{
			if (kASkinPositionWidget._Widget != null)
			{
				Vector3 vector = kASkinPositionWidget._Widget.pOrgPosition + kASkinPositionWidget._Offset;
				TweenPosition.Begin(kASkinPositionWidget._Widget.gameObject, kASkinPositionWidget._Time, inShowEffect ? vector : kASkinPositionWidget._Widget.pOrgPosition).method = kASkinPositionWidget._PositionEffect;
			}
		}
	}
}
