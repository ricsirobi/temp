using System;
using UnityEngine;

namespace JSGames.UI.MultiResolution;

[Serializable]
public class UIComponent
{
	public string Type;

	public UIComponent(Component comp)
	{
		Type = comp.GetType().Name;
		ReadComponentData(comp);
	}

	public virtual void ReadComponentData(Component comp)
	{
	}
}
