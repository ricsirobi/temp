using System.Collections.Generic;
using UnityEngine;

public class ExclusiveUIInfo
{
	public KAUI _UI;

	public Texture2D _Texture;

	public Color _Color;

	public List<UIPanel> _PanelList = new List<UIPanel>();

	public ExclusiveUIInfo(KAUI iFace, Texture2D texture, Color color)
	{
		_UI = iFace;
		_Texture = texture;
		_Color = color;
	}
}
