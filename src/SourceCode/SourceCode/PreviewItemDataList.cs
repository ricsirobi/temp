using System;
using System.Collections.Generic;

public class PreviewItemDataList
{
	private PreviewItemDataListEventHandler mLoadItemDataResCallback;

	private PreviewItemDataListEventHandler mLoadItemDataCallback;

	private List<PreviewItemData> mList;

	private int mParentItemID;

	public List<PreviewItemData> pList => mList;

	public RsResourceLoadEvent pStatus
	{
		get
		{
			if (mList == null || mList.Count == 0)
			{
				return RsResourceLoadEvent.NONE;
			}
			foreach (PreviewItemData m in mList)
			{
				if (m.pLoadStatus == RsResourceLoadEvent.ERROR)
				{
					return RsResourceLoadEvent.ERROR;
				}
			}
			foreach (PreviewItemData m2 in mList)
			{
				if (m2.pLoadStatus == RsResourceLoadEvent.PROGRESS)
				{
					return RsResourceLoadEvent.PROGRESS;
				}
			}
			foreach (PreviewItemData m3 in mList)
			{
				if (m3.pLoadStatus != RsResourceLoadEvent.COMPLETE)
				{
					return RsResourceLoadEvent.NONE;
				}
			}
			return RsResourceLoadEvent.COMPLETE;
		}
	}

	public PreviewItemDataList(int inParentItemID)
	{
		mParentItemID = inParentItemID;
	}

	public void AddItem(int inItemID, int quantity = 0)
	{
		if (mList == null)
		{
			mList = new List<PreviewItemData>();
		}
		mList.Add(new PreviewItemData(inItemID, quantity));
	}

	public void LoadResList(PreviewItemDataListEventHandler inCallback)
	{
		if (mLoadItemDataResCallback == null)
		{
			mLoadItemDataResCallback = inCallback;
		}
		else
		{
			mLoadItemDataResCallback = (PreviewItemDataListEventHandler)Delegate.Combine(mLoadItemDataResCallback, inCallback);
		}
		foreach (PreviewItemData m in mList)
		{
			m?.Load(LoadPreviewItemResource);
		}
	}

	public void LoadItemDataList(PreviewItemDataListEventHandler inCallback)
	{
		if (mLoadItemDataCallback == null)
		{
			mLoadItemDataCallback = inCallback;
		}
		else
		{
			mLoadItemDataCallback = (PreviewItemDataListEventHandler)Delegate.Combine(mLoadItemDataCallback, inCallback);
		}
		foreach (PreviewItemData m in mList)
		{
			m?.LoadItemData(LoadItemDataListCallback);
		}
	}

	public void LoadItemDataListCallback(PreviewItemData inPreviewItemData, bool inSuccess)
	{
		if (!inSuccess)
		{
			mLoadItemDataCallback(this, mParentItemID, inSuccess: true);
			return;
		}
		bool flag = true;
		foreach (PreviewItemData m in mList)
		{
			if (m.mItemDataLoadStatus != RsResourceLoadEvent.COMPLETE)
			{
				flag = false;
				break;
			}
		}
		if (flag)
		{
			mLoadItemDataCallback(this, mParentItemID, inSuccess: true);
		}
	}

	public void LoadPreviewItemResource(PreviewItemData inPreviewItemData, bool inSuccess)
	{
		switch (pStatus)
		{
		case RsResourceLoadEvent.ERROR:
			if (mLoadItemDataResCallback != null)
			{
				mLoadItemDataResCallback(this, mParentItemID, inSuccess: false);
			}
			break;
		case RsResourceLoadEvent.COMPLETE:
			if (mLoadItemDataResCallback != null)
			{
				mLoadItemDataResCallback(this, mParentItemID, inSuccess: true);
			}
			break;
		}
	}

	public bool HasItem(int inItemID)
	{
		if (mList == null)
		{
			return false;
		}
		foreach (PreviewItemData m in mList)
		{
			if (m != null && m.pItemData != null && m.pItemData.ItemID == inItemID)
			{
				return true;
			}
		}
		return false;
	}
}
