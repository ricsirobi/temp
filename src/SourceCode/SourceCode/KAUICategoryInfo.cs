using System.Collections.Generic;

public class KAUICategoryInfo
{
	public int _CategoryID;

	public KAUIMenuGrid _Grid;

	public UICenterOnChild _CenterOnChild;

	public List<CategoryWidgetInfo> _CategoryWidgets = new List<CategoryWidgetInfo>();

	public bool _IsLoaded;

	public int _CategoryIndex = -1;

	public void AddWidget(string inWidgetName, KAWidgetUserData inUserdata)
	{
		CategoryWidgetInfo item = new CategoryWidgetInfo(inWidgetName, inUserdata);
		_CategoryWidgets.Add(item);
	}

	public List<KAWidget> GetWidgets()
	{
		List<KAWidget> list = new List<KAWidget>();
		foreach (CategoryWidgetInfo categoryWidget in _CategoryWidgets)
		{
			if (categoryWidget._WidgetUserData._Item != null && !list.Contains(categoryWidget._WidgetUserData._Item))
			{
				list.Add(categoryWidget._WidgetUserData._Item);
			}
		}
		return list;
	}

	public List<KAWidgetUserData> GetWidgetsData()
	{
		List<KAWidgetUserData> list = new List<KAWidgetUserData>();
		foreach (CategoryWidgetInfo categoryWidget in _CategoryWidgets)
		{
			list.Add(categoryWidget._WidgetUserData);
		}
		return list;
	}
}
