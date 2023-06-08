using System;

[Serializable]
public class KASkinColorInfo
{
	public bool _UseColorEffect;

	public float _Time = 0.2f;

	public KASkinColorWidget[] _ApplyTo;

	public void ShowColorEffect(bool inShowEffect, KAWidget widget = null)
	{
		if (!_UseColorEffect || _ApplyTo == null)
		{
			return;
		}
		KASkinColorWidget[] applyTo = _ApplyTo;
		foreach (KASkinColorWidget kASkinColorWidget in applyTo)
		{
			if (kASkinColorWidget != null && kASkinColorWidget._Widget != null)
			{
				TweenColor.Begin(kASkinColorWidget._Widget.gameObject, _Time, inShowEffect ? kASkinColorWidget._Color : kASkinColorWidget._Widget.pOrgColorTint).method = kASkinColorWidget._ColorEffect;
			}
			else
			{
				UtDebug.LogWarning("missing reference in ColorInfo for " + ((widget == null) ? string.Empty : widget.name));
			}
		}
	}
}
