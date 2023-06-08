using UnityEngine;

public class LtColorAnim : LtAnimBase
{
	public Color _StartColor;

	public Color _EndColor;

	public override void UpdateValue(float f)
	{
		base.light.color = Color.Lerp(_StartColor, _EndColor, f);
	}
}
