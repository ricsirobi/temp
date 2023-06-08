using System;
using System.Collections.Generic;
using UnityEngine;

[Serializable]
public class OptimizedMenu
{
	public bool _Enable;

	private bool mIsHorizontallyArranged = true;

	private KAUIMenu mMenu;

	private const int ITEM_BUFFER_COUNT = 4;

	private const int CLIPPING_BUFFER = 100;

	private int mOptimizedItemCount;

	private List<KAWidgetUserData> mUserData = new List<KAWidgetUserData>();

	private Bounds mMenuBounds;

	public List<KAWidgetUserData> pUserData
	{
		get
		{
			return mUserData;
		}
		set
		{
			mUserData = value;
		}
	}

	public Bounds pMenuBounds => mMenuBounds;

	private List<KAWidget> mMenuWidgets => mMenu.GetItems();

	public bool pCanAddWidget => mUserData.Count < mOptimizedItemCount;

	public int GetDataCount()
	{
		return mUserData.Count;
	}

	public void Initialize(KAUIMenu menu)
	{
		mMenu = menu;
		int numItemsPerPage = mMenu.GetNumItemsPerPage();
		mOptimizedItemCount = numItemsPerPage + 4;
		mMenu.pMenuGrid._IsGrid = true;
		mIsHorizontallyArranged = mMenu.pMenuGrid.arrangement == UIGrid.Arrangement.Vertical;
	}

	public void CheckClippedWidgets(Vector2 dragDelta)
	{
		if (mMenu == null || mMenuWidgets == null || mMenuWidgets.Count == 0)
		{
			return;
		}
		bool flag = dragDelta.y > 0f;
		if (mIsHorizontallyArranged)
		{
			flag = dragDelta.x < 0f;
		}
		Vector3[] worldCorners = mMenu.pPanel.worldCorners;
		float x = worldCorners[0].x;
		float x2 = worldCorners[2].x;
		float y = worldCorners[2].y;
		float y2 = worldCorners[0].y;
		Vector3 zero = Vector3.zero;
		zero = ((!mIsHorizontallyArranged) ? new Vector3(0f, -100f, 0f) : new Vector3(100f, 0f, 0f));
		bool flag2 = false;
		int i = mMenuWidgets.Count - 1;
		int num = -1;
		if (flag)
		{
			i = 0;
			num = 1;
		}
		for (; i >= 0 && i < mMenuWidgets.Count; i += num)
		{
			KAWidget kAWidget = mMenuWidgets[i];
			Vector3 position = kAWidget.transform.position;
			if (((mIsHorizontallyArranged && flag && position.x < x) || (!flag && position.x > x2) || (!mIsHorizontallyArranged && flag && position.y > y) || (!flag && position.y < y2)) && ((flag && !mMenu.pPanel.IsVisible(position + zero)) || (!flag && !mMenu.pPanel.IsVisible(position - zero))))
			{
				UpdateClippedWidget(kAWidget, flag);
				flag2 = true;
			}
		}
		if (flag2)
		{
			SortWidgetByIndex();
		}
	}

	private void UpdateClippedWidget(KAWidget clippedWidget, bool isUp)
	{
		mMenu.UnloadItem(clippedWidget);
		Vector3 localPosition = Vector3.zero;
		int num = -1;
		if (isUp)
		{
			int num2 = int.MinValue;
			KAWidget kAWidget = clippedWidget;
			foreach (KAWidget mMenuWidget in mMenuWidgets)
			{
				if (mMenuWidget._MenuItemIndex > num2)
				{
					kAWidget = mMenuWidget;
					num2 = kAWidget._MenuItemIndex;
				}
			}
			if (kAWidget != clippedWidget && kAWidget._MenuItemIndex < mUserData.Count - 1)
			{
				localPosition = ((!mIsHorizontallyArranged) ? new Vector3(0f, 0f - mMenu.pMenuGrid.cellHeight, 0f) : new Vector3(mMenu.pMenuGrid.cellWidth, 0f, 0f));
				num = kAWidget._MenuItemIndex + 1;
				localPosition = kAWidget.transform.localPosition + localPosition;
			}
		}
		else
		{
			int num3 = int.MaxValue;
			KAWidget kAWidget2 = clippedWidget;
			foreach (KAWidget mMenuWidget2 in mMenuWidgets)
			{
				if (mMenuWidget2._MenuItemIndex < num3)
				{
					kAWidget2 = mMenuWidget2;
					num3 = kAWidget2._MenuItemIndex;
				}
			}
			if (kAWidget2 != clippedWidget && kAWidget2._MenuItemIndex > 0)
			{
				localPosition = ((!mIsHorizontallyArranged) ? new Vector3(0f, mMenu.pMenuGrid.cellHeight, 0f) : new Vector3(0f - mMenu.pMenuGrid.cellWidth, 0f, 0f));
				num = kAWidget2._MenuItemIndex - 1;
				localPosition = kAWidget2.transform.localPosition + localPosition;
			}
		}
		if (num >= 0 && num < mUserData.Count)
		{
			clippedWidget.transform.localPosition = localPosition;
			clippedWidget.SetUserData(mUserData[num]);
			clippedWidget._MenuItemIndex = num;
		}
	}

	private void SortWidgetByIndex()
	{
		mMenuWidgets.Sort((KAWidget firstItem, KAWidget secondItem) => firstItem._MenuItemIndex.CompareTo(secondItem._MenuItemIndex));
	}

	public void AddUserData(KAWidget widget, KAWidgetUserData userData)
	{
		mUserData.Add(userData);
	}

	public void CalculateBounds()
	{
		if (!_Enable)
		{
			return;
		}
		mMenuBounds = NGUIMath.CalculateRelativeWidgetBounds(mMenu.transform, mMenu.pMenuGrid.transform, considerInactive: true);
		if (mUserData.Count < mOptimizedItemCount)
		{
			return;
		}
		int num = mUserData.Count - mOptimizedItemCount;
		Vector3 zero = Vector3.zero;
		if (mIsHorizontallyArranged)
		{
			zero.x = mMenu.pMenuGrid.cellWidth * (float)num;
		}
		else
		{
			zero.y = mMenu.pMenuGrid.cellHeight * (float)num;
		}
		mMenuBounds.Expand(zero);
		Vector3 vector = zero / 2f;
		mMenuBounds.center += new Vector3(vector.x, 0f - vector.y, vector.z);
		int num2 = int.MaxValue;
		foreach (KAWidget mMenuWidget in mMenuWidgets)
		{
			if (mMenuWidget._MenuItemIndex < num2)
			{
				num2 = mMenuWidget._MenuItemIndex;
			}
		}
		if (num2 > 0)
		{
			Vector3 zero2 = Vector3.zero;
			if (mIsHorizontallyArranged)
			{
				zero2.x = 0f - (float)num2 * mMenu.pMenuGrid.cellWidth;
			}
			else
			{
				zero2.y = (float)num2 * mMenu.pMenuGrid.cellHeight;
			}
			mMenuBounds.center += zero2;
		}
	}

	public void SnapTo(Vector2 scrollValue)
	{
		if (mMenuWidgets.Count == 0)
		{
			return;
		}
		int num = 0;
		Vector3 zero = Vector3.zero;
		if (mIsHorizontallyArranged)
		{
			zero = new Vector3(mMenu.pMenuGrid.cellWidth, 0f, 0f);
			num = Mathf.RoundToInt(scrollValue.x * (float)mUserData.Count) - 1;
		}
		else
		{
			zero = new Vector3(0f, 0f - mMenu.pMenuGrid.cellHeight, 0f);
			num = Mathf.RoundToInt(scrollValue.y * (float)mUserData.Count) - 1;
		}
		GameObject gameObject = new GameObject();
		gameObject.transform.parent = mMenuWidgets[0].transform.parent;
		do
		{
			gameObject.transform.localPosition = num * zero;
			num--;
		}
		while (mMenu.pPanel.IsVisible(gameObject.transform.position));
		UnityEngine.Object.Destroy(gameObject);
		num = Mathf.Clamp(num, 0, mUserData.Count - mMenuWidgets.Count);
		foreach (KAWidget mMenuWidget in mMenuWidgets)
		{
			if (mMenuWidget._MenuItemIndex != num)
			{
				mMenuWidget.transform.localPosition = num * zero;
				mMenuWidget.SetUserData(mUserData[num]);
				mMenuWidget._MenuItemIndex = num;
			}
			num++;
			if (num >= mUserData.Count)
			{
				break;
			}
		}
	}

	public void Clear()
	{
		mUserData.Clear();
		if (mMenu != null)
		{
			mMenuWidgets.Clear();
		}
	}
}
