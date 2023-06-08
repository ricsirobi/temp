using System;
using UnityEngine;

[Serializable]
public class ContextSensitiveUIStyleData
{
	public enum SUBMENU_DIRECTION
	{
		BOTTOM,
		TOP
	}

	public UI_STYLE_TYPE _Type;

	public SUBMENU_DIRECTION _SubMenuDirection;

	public Vector2 _WidgetOffsetInPixels = new Vector2(0f, 10f);

	public Vector2 _MenuBackgroundExtraScalePixels = new Vector2(40f, 10f);
}
