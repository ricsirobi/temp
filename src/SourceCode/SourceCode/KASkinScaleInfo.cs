using System;
using UnityEngine;

[Serializable]
public class KASkinScaleInfo
{
	public bool _UseScaleEffect;

	public float _Time = 0.2f;

	public KASkinScaleKAWidget _ApplyToWidget;

	public KASkinScaleWidget[] _ApplyTo;

	public void ShowScaleEffect(bool inShowEffect, KAWidget widget = null)
	{
		if (!_UseScaleEffect)
		{
			return;
		}
		if (_ApplyTo != null)
		{
			KASkinScaleWidget[] applyTo = _ApplyTo;
			foreach (KASkinScaleWidget kASkinScaleWidget in applyTo)
			{
				if (kASkinScaleWidget != null && kASkinScaleWidget._Widget != null)
				{
					TweenScale.Begin(kASkinScaleWidget._Widget.gameObject, _Time, inShowEffect ? Vector3.Scale(kASkinScaleWidget._Widget.pOrgScale, kASkinScaleWidget._Scale) : kASkinScaleWidget._Widget.pOrgScale).method = kASkinScaleWidget._ScaleEffect;
				}
				else
				{
					UtDebug.LogWarning("missing reference in ScaleInfo for " + ((widget == null) ? string.Empty : widget.name));
				}
			}
		}
		if (_ApplyToWidget != null && _ApplyToWidget._Widget != null)
		{
			TweenScale.Begin(_ApplyToWidget._Widget.gameObject, _Time, inShowEffect ? Vector3.Scale(_ApplyToWidget._Widget.pOrgScale, _ApplyToWidget._Scale) : _ApplyToWidget._Widget.pOrgScale).method = _ApplyToWidget._ScaleEffect;
		}
	}
}
