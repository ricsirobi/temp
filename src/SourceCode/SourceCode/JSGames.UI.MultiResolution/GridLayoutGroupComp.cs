using System;
using UnityEngine;
using UnityEngine.UI;

namespace JSGames.UI.MultiResolution;

[Serializable]
public class GridLayoutGroupComp : UIComponent
{
	public const string GridLayoutGroupCompName = "GridLayoutGroup";

	public RectOffset Padding;

	public Vector2 CellSize;

	public Vector2 Spacing;

	public GridLayoutGroupComp(Component comp)
		: base(comp)
	{
		ReadComponentData(comp);
	}

	public bool Equals(GridLayoutGroupComp gridLayoutGroupComp)
	{
		if (gridLayoutGroupComp.Padding.left != Padding.left || gridLayoutGroupComp.Padding.right != Padding.right || gridLayoutGroupComp.Padding.top != Padding.top || gridLayoutGroupComp.Padding.bottom != Padding.bottom)
		{
			return false;
		}
		if (gridLayoutGroupComp.CellSize != CellSize)
		{
			return false;
		}
		if (gridLayoutGroupComp.Spacing != Spacing)
		{
			return false;
		}
		return true;
	}

	public override void ReadComponentData(Component Comp)
	{
		GridLayoutGroup component = Comp.GetComponent<GridLayoutGroup>();
		Padding = component.padding;
		CellSize = component.cellSize;
		Spacing = component.spacing;
	}
}
