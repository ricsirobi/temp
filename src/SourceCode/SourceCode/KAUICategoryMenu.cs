using System;
using System.Collections.Generic;
using UnityEngine;

public class KAUICategoryMenu : KAUIMenu
{
	public GameObject _CategoriesParent;

	public float _CategorySnapStrength = 25f;

	private Dictionary<int, KAUICategoryInfo> mCategories = new Dictionary<int, KAUICategoryInfo>();

	private KAUIMenuGrid mCategoryGrid;

	private bool mIsCatGridRepositionOn;

	protected KAUICategoryInfo mCenteredCategory;

	private bool mRecenterCategory;

	private bool mIsLoadingCatWidgets;

	private bool mIsCategoryDragging;

	private bool mIsItemsDragging;

	private bool mIsDragReleased;

	private bool mIsCategoryCentered;

	private Vector3 mCategoryGridLastPosition = new Vector3(100f, 100f, 100f);

	private int mLastCategoryIdx = -1;

	private Vector3 mCategoryParentInitPos;

	private bool mIsCenterCatIDSet;

	protected int mAddWidgetCatID;

	protected override void Start()
	{
		base.Start();
		mCategoryGrid = _CategoriesParent.GetComponent<KAUIMenuGrid>();
		mCategoryParentInitPos = _CategoriesParent.transform.localPosition;
		_DefaultGrid.gameObject.SetActive(value: false);
		if (mCategoryGrid != null)
		{
			KAUIMenuGrid kAUIMenuGrid = mCategoryGrid;
			kAUIMenuGrid.onReposition = (UITable.OnReposition)Delegate.Combine(kAUIMenuGrid.onReposition, new UITable.OnReposition(OnCategoryGridReposition));
		}
	}

	protected override void LateUpdate()
	{
		if (mIsCategoryDragging && Input.GetMouseButtonUp(0))
		{
			mIsDragReleased = true;
		}
		if (mIsDragReleased)
		{
			OnDragReleased();
		}
		if (mRecenterCategory)
		{
			if (mIsCatGridRepositionOn)
			{
				mCategoryGrid.Reposition();
				mIsCatGridRepositionOn = false;
			}
			else
			{
				ReCenter();
			}
		}
		if (mCategoryGrid.transform.localPosition != mCategoryGridLastPosition)
		{
			OnCategoryDrag();
			mCategoryGridLastPosition = mCategoryGrid.transform.localPosition;
		}
		Vector3 vector = mDragPanel.transform.localPosition - mLastPosition;
		if (vector != Vector3.zero)
		{
			foreach (KAUICategoryInfo value in mCategories.Values)
			{
				if (value._Grid != mCurrentGrid)
				{
					value._Grid.transform.localPosition -= vector;
				}
			}
		}
		base.LateUpdate();
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		if (!mIsCategoryCentered)
		{
			return;
		}
		float num = Mathf.Abs(inDelta.x);
		float num2 = Mathf.Abs(inDelta.y);
		if (!(mIsCategoryDragging ^ mIsItemsDragging))
		{
			if (mCategoryGrid.arrangement == UIGrid.Arrangement.Horizontal)
			{
				mIsCategoryDragging = num > num2;
			}
			else
			{
				mIsCategoryDragging = num2 > num;
			}
			mIsItemsDragging = !mIsCategoryDragging;
		}
		if (mIsCategoryDragging)
		{
			_CategoriesParent.transform.localPosition += new Vector3(inDelta.x, 0f, 0f);
			bool flag = false;
			if (mCenteredCategory._CategoryIndex == 0 && inDelta.x > 0f)
			{
				flag = true;
			}
			else if (mCenteredCategory._CategoryIndex == mCategories.Count - 1 && inDelta.x < 0f)
			{
				flag = true;
			}
			if (flag)
			{
				Vector3 vector = base.transform.TransformPoint(mCategoryParentInitPos) - mCurrentGrid.transform.position;
				if ((inDelta.x > 0f && vector.x < 0f) || (inDelta.x < 0f && vector.x > 0f))
				{
					_CategoriesParent.transform.localPosition += new Vector3(vector.x, 0f, 0f);
				}
			}
		}
		else
		{
			base.OnDrag(inWidget, inDelta);
		}
	}

	private void OnCategoryDrag()
	{
		if (mCategories.Count <= 1)
		{
			return;
		}
		Vector3[] worldCorners = mPanel.worldCorners;
		Vector3 vector = (worldCorners[2] + worldCorners[0]) * 0.5f;
		foreach (KAUICategoryInfo value in mCategories.Values)
		{
			float num = Mathf.Abs(value._Grid.transform.position.x - vector.x);
			float num2 = (mPanel.baseClipRegion.z + value._Grid.cellWidth) * 0.5f;
			if (!value._IsLoaded && num <= num2)
			{
				LoadCategoryWidgets(value);
			}
			else if (value._IsLoaded && num >= num2)
			{
				UnloadCategoryWidgets(value);
			}
		}
		FindCenterCategory();
	}

	public override void OnPress(KAWidget inWidget, bool inPressed)
	{
		base.OnPress(inWidget, inPressed);
		if (!inPressed)
		{
			if (mIsCategoryDragging)
			{
				mIsDragReleased = true;
				mIsCategoryDragging = false;
			}
			else
			{
				mRecenterItem = true;
				mIsItemsDragging = false;
			}
		}
	}

	protected virtual void OnDragReleased()
	{
		ReCenter();
		if (_CenterOnItem && mIsItemsDragging)
		{
			mCenteredCategory._CenterOnChild.Recenter();
		}
		mIsDragReleased = false;
	}

	public override KAWidget AddWidget(string widgetName, KAWidgetUserData userData)
	{
		KAUICategoryInfo value = null;
		mCategories.TryGetValue(mAddWidgetCatID, out value);
		if (value == null)
		{
			value = AddCategory(mAddWidgetCatID);
		}
		value.AddWidget(widgetName, userData);
		mRecenterCategory = true;
		return null;
	}

	private void FindCenterCategory()
	{
		Vector3[] worldCorners = mPanel.worldCorners;
		Vector3 vector = (worldCorners[2] + worldCorners[0]) * 0.5f;
		float num = float.MaxValue;
		KAUICategoryInfo kAUICategoryInfo = null;
		foreach (KAUICategoryInfo value in mCategories.Values)
		{
			float num2 = Mathf.Abs(value._Grid.transform.position.x - vector.x);
			if (num2 < num)
			{
				num = num2;
				kAUICategoryInfo = value;
			}
		}
		if (kAUICategoryInfo != mCenteredCategory || mCurrentGrid != mCenteredCategory._Grid)
		{
			if (_CenterOnItem && mCenteredCategory != null)
			{
				mCenteredCategory._CenterOnChild.onFinished = null;
				mCenteredCategory._CenterOnChild.enabled = false;
			}
			mCenteredCategory = kAUICategoryInfo;
			mCurrentGrid = mCenteredCategory._Grid;
			mItemInfo = mCenteredCategory.GetWidgets();
			OnCenterCategoryChange(mCenteredCategory._CategoryID);
			if (base.pIsOptimized)
			{
				_OptimizedMenu.pUserData = mCenteredCategory.GetWidgetsData();
				_OptimizedMenu.CalculateBounds();
				UpdateScrollbars(inVisible: true);
			}
			if (_CenterOnItem)
			{
				mSelectedItem = null;
				CheckClosestWidget();
			}
		}
	}

	protected void ReCenter()
	{
		FindCenterCategory();
		ScrollToCenterCategory();
	}

	protected void ScrollToCenterCategory()
	{
		if (mCenteredCategory != null)
		{
			if (!mCenteredCategory._IsLoaded)
			{
				LoadCategoryWidgets(mCenteredCategory);
			}
			mIsCategoryCentered = false;
			SpringPosition.Begin(_CategoriesParent, new Vector3(mCategoryParentInitPos.x - (float)mCenteredCategory._CategoryIndex * mCategoryGrid.cellWidth, _CategoriesParent.transform.localPosition.y, 0f), _CategorySnapStrength).onFinished = OnCategoryCentered;
		}
		if (_CenterOnItem && mCenteredCategory != null)
		{
			mSelectedItem = null;
			mCenteredCategory._CenterOnChild.onFinished = OnCenterItemFinished;
			mCenteredCategory._CenterOnChild.enabled = true;
			mCenterOnChild = mCenteredCategory._CenterOnChild;
		}
		mRecenterCategory = false;
	}

	public void GotoCategory(int inCatID)
	{
		if (mCategories.ContainsKey(inCatID))
		{
			mCenteredCategory = mCategories[inCatID];
			ScrollToCenterCategory();
			mIsCenterCatIDSet = true;
		}
	}

	private void LoadCategoryWidgets(KAUICategoryInfo inCatInfo)
	{
		if (!inCatInfo._IsLoaded)
		{
			mCurrentGrid = inCatInfo._Grid;
			mItemInfo.Clear();
			if (base.pIsOptimized)
			{
				_OptimizedMenu.pUserData.Clear();
			}
			mIsLoadingCatWidgets = true;
			List<CategoryWidgetInfo> categoryWidgets = inCatInfo._CategoryWidgets;
			for (int i = 0; i < categoryWidgets.Count; i++)
			{
				base.AddWidget(categoryWidgets[i]._WidgetName, categoryWidgets[i]._WidgetUserData);
			}
			inCatInfo._Grid.Reposition();
			if (base.pIsOptimized)
			{
				_OptimizedMenu.CalculateBounds();
			}
			mDragPanel.UpdateScrollbars(recalculateBounds: true);
			if (base.pIsOptimized)
			{
				float scrollValue = GetScrollValue();
				_OptimizedMenu.SnapTo(new Vector2(0f, scrollValue));
			}
			mIsLoadingCatWidgets = false;
			LoadItemsInView();
			inCatInfo._IsLoaded = true;
		}
	}

	private void UnloadCategoryWidgets(KAUICategoryInfo inCatInfo)
	{
		if (!inCatInfo._IsLoaded)
		{
			return;
		}
		mCurrentGrid = inCatInfo._Grid;
		mItemInfo = inCatInfo.GetWidgets();
		if (base.pIsOptimized)
		{
			_OptimizedMenu.pUserData = inCatInfo.GetWidgetsData();
		}
		ClearMenu();
		new List<KAWidgetUserData>();
		foreach (KAWidgetUserData widgetsDatum in inCatInfo.GetWidgetsData())
		{
			widgetsDatum._Item = null;
		}
		inCatInfo._IsLoaded = false;
	}

	protected override void ResetScrollBar()
	{
		if (!mIsLoadingCatWidgets)
		{
			base.ResetScrollBar();
		}
	}

	protected void OnCategoryGridReposition()
	{
		if (!mIsCenterCatIDSet)
		{
			ReCenter();
		}
	}

	protected virtual void OnCategoryCentered()
	{
		if (base.pIsOptimized)
		{
			_OptimizedMenu.CalculateBounds();
		}
		if (_CenterOnItem)
		{
			mCenteredCategory._CenterOnChild.Recenter();
		}
		mIsCategoryCentered = true;
		mIsCenterCatIDSet = false;
	}

	public void SetAddWidgetCatId(int inCatId)
	{
		mAddWidgetCatID = inCatId;
	}

	public virtual KAUICategoryInfo AddCategory(int inCategoryId)
	{
		if (mCategories.ContainsKey(inCategoryId))
		{
			return mCategories[inCategoryId];
		}
		KAUICategoryInfo kAUICategoryInfo = new KAUICategoryInfo();
		KAUIMenuGrid component = UnityEngine.Object.Instantiate(_DefaultGrid.gameObject).GetComponent<KAUIMenuGrid>();
		component.transform.parent = _CategoriesParent.transform;
		component.transform.localPosition = Vector3.zero;
		kAUICategoryInfo._CategoryID = inCategoryId;
		kAUICategoryInfo._Grid = component;
		kAUICategoryInfo._Grid.name = inCategoryId.ToString();
		kAUICategoryInfo._Grid.gameObject.SetActive(value: true);
		kAUICategoryInfo._CategoryIndex = ++mLastCategoryIdx;
		mCategories.Add(inCategoryId, kAUICategoryInfo);
		mCategoryGrid.repositionNow = true;
		mIsCatGridRepositionOn = true;
		if (_CenterOnItem)
		{
			kAUICategoryInfo._CenterOnChild = kAUICategoryInfo._Grid.GetComponent<UICenterOnChild>();
			if (kAUICategoryInfo._CenterOnChild != null)
			{
				kAUICategoryInfo._CenterOnChild.enabled = false;
				kAUICategoryInfo._CenterOnChild.onFinished = null;
			}
		}
		return kAUICategoryInfo;
	}

	public override void ResetPosition()
	{
	}

	public override void ClearItems()
	{
		foreach (KAUICategoryInfo value in mCategories.Values)
		{
			mItemInfo = value.GetWidgets();
			if (base.pIsOptimized)
			{
				_OptimizedMenu.pUserData = value.GetWidgetsData();
			}
			ClearMenu();
			UnityEngine.Object.Destroy(value._Grid.gameObject);
		}
		_CategoriesParent.transform.localPosition = mCategoryParentInitPos;
		mCategoryGridLastPosition = _CategoriesParent.transform.localPosition;
		mLastPosition = mDragPanel.transform.localPosition;
		mLastCategoryIdx = -1;
		mCategories.Clear();
		mCenteredCategory = null;
		mDragPanel.currentMomentum = Vector3.zero;
		mDragPanel.DisableSpring();
		base.transform.localPosition = mInitialPosition;
		mPanel.clipOffset = Vector2.zero;
		base.ClearItems();
	}

	public override List<Transform> GetTransforms(KAUIMenuGrid inGrid = null)
	{
		if (inGrid == mCategoryGrid)
		{
			List<Transform> list = new List<Transform>();
			{
				foreach (KAUICategoryInfo value in mCategories.Values)
				{
					list.Add(value._Grid.transform);
				}
				return list;
			}
		}
		return base.GetTransforms(inGrid);
	}

	public KAUICategoryInfo GetCategory(int inCategoryID)
	{
		if (mCategories.ContainsKey(inCategoryID))
		{
			return mCategories[inCategoryID];
		}
		return null;
	}

	protected virtual void OnCenterCategoryChange(int categoryID)
	{
	}
}
