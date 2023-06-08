using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StorePreview
{
	public enum MoveDir
	{
		LEFT,
		RIGHT
	}

	private List<StorePreviewPage> mPages;

	private int mIndex = -1;

	public KAUIStoreChoose3D mStoreChooseUI;

	private static StorePreview mInstance;

	public KAWidget mLoadingWidget;

	private Transform m3DMarker;

	private KAWidget m2DViewer;

	private bool mAllowScaling;

	private float mScaleLength;

	private bool mLoading;

	private int mCurrentParentID;

	public bool pIsDistinctItem { get; set; }

	public int pLeftLimitScroll { get; set; }

	public static StorePreview pInstance => mInstance;

	public bool pEnableLeftButton
	{
		get
		{
			if (mIndex <= 0)
			{
				if (mPages != null)
				{
					return mPages[mIndex].mPreviewItemIndex >= pLeftLimitScroll;
				}
				return false;
			}
			return true;
		}
	}

	public bool pEnableRightButton
	{
		get
		{
			if (mPages != null)
			{
				if (mIndex >= mPages.Count - 1)
				{
					if (mIndex >= 0 && mPages[mIndex].mPreviewItemIndex < mPages[mIndex].pPreviewItemList.pList.Count - 1)
					{
						return mPages[mIndex].pPreviewItemList.pList.Count > 1;
					}
					return false;
				}
				return true;
			}
			return false;
		}
	}

	public ItemData pCurItemData
	{
		get
		{
			if (mIndex >= 0 && mPages != null && mIndex < mPages.Count)
			{
				if (mPages[mIndex].pCurrent3DObject == null)
				{
					return null;
				}
				return mPages[mIndex].pCurrentItemData;
			}
			return null;
		}
	}

	public GameObject pCur3DInstance
	{
		get
		{
			if (mIndex >= 0 && mPages != null && mIndex < mPages.Count)
			{
				if (mPages[mIndex].pCurrent3DObject == null)
				{
					return null;
				}
				return mPages[mIndex].pCurrent3DObject.gameObject;
			}
			return null;
		}
	}

	public StorePreview(KAWidget inLoading, KAUIStoreChoose3D inStoreChooseUI)
	{
		mLoadingWidget = inLoading;
		mStoreChooseUI = inStoreChooseUI;
	}

	public static void Start(KAWidget inLoading, KAUIStoreChoose3D inChooseUI)
	{
		if (mInstance == null)
		{
			mInstance = new StorePreview(inLoading, inChooseUI);
		}
	}

	public static void ScalePreview(KAWidget widget, string previewSize)
	{
		UITexture uITexture = widget.GetUITexture();
		if (uITexture != null && previewSize != null)
		{
			string[] array = previewSize.Split(',');
			if (array.Length > 1)
			{
				uITexture.width = int.Parse(array[0]);
				uITexture.height = int.Parse(array[1]);
			}
		}
	}

	public static void OnPreviewLoaded(KAWidget widget, bool success)
	{
		widget.SetVisibility(inVisible: true);
	}

	public void ShowPreview(int inParentItemID, int inItemID, Transform in3DMarker, KAWidget in2DViewer, bool inAllowScaling, float inScaleLength)
	{
		ShowPreview(inParentItemID, new int[1] { inItemID }, in3DMarker, in2DViewer, inAllowScaling, inScaleLength);
	}

	public void ShowPreview(int inParentItemID, int[] inItemIDs, Transform in3DMarker, KAWidget in2DViewer, bool inAllowScaling, float inScaleLength, ItemDataRelationship[] relationships = null)
	{
		if (mLoadingWidget != null)
		{
			mLoadingWidget.SetVisibility(inVisible: true);
		}
		mCurrentParentID = inParentItemID;
		mLoading = true;
		m3DMarker = in3DMarker;
		m2DViewer = in2DViewer;
		mAllowScaling = inAllowScaling;
		mScaleLength = inScaleLength;
		PreviewItemDataList previewItemDataList = new PreviewItemDataList(inParentItemID);
		if (relationships != null)
		{
			foreach (ItemDataRelationship itemDataRelationship in relationships)
			{
				if (itemDataRelationship.Type == "Bundle")
				{
					previewItemDataList.AddItem(itemDataRelationship.ItemId, itemDataRelationship.Quantity);
				}
			}
		}
		else
		{
			foreach (int inItemID in inItemIDs)
			{
				previewItemDataList.AddItem(inItemID);
			}
		}
		previewItemDataList.LoadItemDataList(PreviewItemLoadedEventHandler);
	}

	public void PreviewItemLoadedEventHandler(PreviewItemDataList inDataList, int inParentItemID, bool inSuccess)
	{
		if (!inSuccess || inDataList.pList == null || inDataList == null)
		{
			if (mLoadingWidget != null)
			{
				mLoadingWidget.SetVisibility(inVisible: false);
			}
		}
		else
		{
			if (mCurrentParentID != inParentItemID)
			{
				return;
			}
			mLoading = false;
			StorePreviewPage storePreviewPage = null;
			foreach (PreviewItemData p in inDataList.pList)
			{
				if (p == null || p.pItemData == null || p.pItemData.ItemID <= 0)
				{
					continue;
				}
				string attribute = p.pItemData.GetAttribute("Gender", "U");
				if (attribute != mStoreChooseUI._StoreUI.pChooseMenu._GenderFilter && attribute != "U" && mStoreChooseUI._StoreUI.pChooseMenu._GenderFilter != "U")
				{
					continue;
				}
				switch (KAUIStore.GetPreviewCategory(p.pItemData))
				{
				case StorePreviewCategory.Avatar:
					if (storePreviewPage == null)
					{
						storePreviewPage = new StorePreviewPage(mCurrentParentID, p.pItemData.ItemID, m3DMarker, null, ItemReady, p.Quantity);
					}
					else
					{
						storePreviewPage.AddItem(p.pItemData.ItemID, p.Quantity);
					}
					storePreviewPage.mScaleLength = mScaleLength;
					storePreviewPage.mAllowScaling = mAllowScaling;
					break;
				case StorePreviewCategory.Normal3D:
				case StorePreviewCategory.RaisedPet:
				{
					StorePreviewPage storePreviewPage2 = new StorePreviewPage(mCurrentParentID, p.pItemData.ItemID, m3DMarker, m2DViewer, ItemReady, p.Quantity);
					storePreviewPage2.mScaleLength = mScaleLength;
					storePreviewPage2.mAllowScaling = mAllowScaling;
					AddPage(storePreviewPage2);
					break;
				}
				}
			}
			if (storePreviewPage != null)
			{
				AddPage(storePreviewPage);
			}
			ShowFirstItemPreview();
			mStoreChooseUI.SetPreviewButtonsVisible(pEnableLeftButton || pEnableRightButton);
			mStoreChooseUI.EnablePreviewButtons(pEnableLeftButton, pEnableRightButton);
		}
	}

	private void AddPage(StorePreviewPage inPage)
	{
		if (inPage == null)
		{
			return;
		}
		if (mPages == null)
		{
			mPages = new List<StorePreviewPage>();
		}
		bool flag = true;
		if (mPages.Count > 0 || (inPage.pPreviewItemList != null && inPage.pPreviewItemList.pList != null && inPage.pPreviewItemList.pList.Count > 0))
		{
			foreach (StorePreviewPage mPage in mPages)
			{
				foreach (PreviewItemData p in inPage.pPreviewItemList.pList)
				{
					if (p != null && p.pItemData != null && mPage.HasItem(p.pItemData.ItemID))
					{
						flag = false;
						mPage.IncrementCount();
					}
				}
			}
		}
		if (flag)
		{
			if (inPage.pPreviewItemList.pList.Count > 0)
			{
				inPage.IncrementCount(inPage.pPreviewItemList.pList[0].Quantity);
			}
			mPages.Add(inPage);
		}
	}

	private void ShowFirstItemPreview()
	{
		ShowPreview(0, MoveDir.LEFT);
	}

	private void ShowPreview(int inNewPageIndex, MoveDir moveDir, bool force = false)
	{
		if (mPages == null || (!force && mIndex == inNewPageIndex))
		{
			return;
		}
		if (mIndex >= 0 && mIndex < mPages.Count)
		{
			if (mPages[mIndex].pCurrent3DObject != null)
			{
				mPages[mIndex].pCurrent3DObject.gameObject.SetActive(value: false);
			}
			mPages[mIndex].StopLoading();
		}
		if (mIndex == -1)
		{
			mIndex = Mathf.Min(mPages.Count - 1, inNewPageIndex);
		}
		else if (mPages[mIndex].pPreviewItemList != null && mPages[mIndex].pPreviewItemList.pList != null)
		{
			switch (moveDir)
			{
			case MoveDir.RIGHT:
				if (mPages[mIndex].mPreviewItemIndex >= mPages[mIndex].pPreviewItemList.pList.Count - 1 || mPages[mIndex].pPreviewItemList.pList.Count == 1)
				{
					mIndex = Mathf.Min(mPages.Count - 1, inNewPageIndex);
					if (!pIsDistinctItem)
					{
						mPages[mIndex].mPreviewItemIndex = 0;
					}
				}
				else
				{
					mPages[mIndex].mPreviewItemIndex++;
				}
				break;
			case MoveDir.LEFT:
				if (mPages[mIndex].mPreviewItemIndex < pLeftLimitScroll)
				{
					mIndex = Mathf.Min(mPages.Count - 1, inNewPageIndex);
					if (!pIsDistinctItem)
					{
						mPages[mIndex].mPreviewItemIndex = 0;
					}
				}
				else
				{
					mPages[mIndex].mPreviewItemIndex--;
				}
				break;
			}
		}
		if (mIndex < 0)
		{
			mIndex = 0;
		}
		if (mLoadingWidget != null)
		{
			mLoadingWidget.SetVisibility(inVisible: true);
		}
		mPages[mIndex].ShowItem();
	}

	public bool CheckIfDistinctItems(PreviewItemDataList pdataList)
	{
		List<string> list = new List<string>();
		for (int i = 0; i < pdataList.pList.Count; i++)
		{
			string itemPartType = AvatarData.GetItemPartType(pdataList.pList[i].pItemData);
			list.Add(itemPartType);
		}
		return list.Distinct().Count() == pdataList.pList.Count;
	}

	private void ItemReady(StorePreviewPage inPageItem, bool inSuccess)
	{
		if (inPageItem != null && inSuccess && mIndex >= 0 && inPageItem.pParentItemID == mPages[mIndex].pParentItemID && mPages[mIndex] != null)
		{
			if (mLoadingWidget != null)
			{
				mLoadingWidget.SetVisibility(inVisible: false);
			}
			mStoreChooseUI.PreviewItemReady(KAUIStore.GetPreviewCategory(inPageItem.pCurrentItemData), inPageItem);
		}
	}

	public void GotoNextPage()
	{
		if (!mLoading)
		{
			ShowPreview(mIndex + 1, MoveDir.RIGHT);
		}
	}

	public void GotoPrevPage()
	{
		if (!mLoading)
		{
			ShowPreview(mIndex - 1, MoveDir.LEFT);
		}
	}

	public void Reset()
	{
		if (mPages == null || mPages.Count == 0)
		{
			return;
		}
		foreach (StorePreviewPage mPage in mPages)
		{
			if (mPage.pCurrent3DObject != null)
			{
				Object.Destroy(mPage.pCurrent3DObject.gameObject);
			}
			mPage.StopLoading();
		}
		mPages.Clear();
		mIndex = -1;
	}
}
