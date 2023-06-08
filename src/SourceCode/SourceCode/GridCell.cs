using System.Collections.Generic;
using UnityEngine;

public class GridCell
{
	public Rect _GridCellRect;

	public int _GridCellIndex;

	public bool _IsUnlocked;

	public List<GridItemData> _ItemOnGrids = new List<GridItemData>();

	public void ResetItem(GameObject inObject)
	{
		if (inObject == null || _ItemOnGrids == null || _ItemOnGrids.Count == 0)
		{
			return;
		}
		List<GridItemData> list = new List<GridItemData>();
		foreach (GridItemData itemOnGrid in _ItemOnGrids)
		{
			if (itemOnGrid != null && itemOnGrid._Object == inObject)
			{
				list.Add(itemOnGrid);
				break;
			}
		}
		if (list.Count <= 0)
		{
			return;
		}
		foreach (GridItemData item in list)
		{
			_ItemOnGrids.Remove(item);
		}
	}
}
