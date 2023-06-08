using System;
using UnityEngine;

public class PreviewItemData
{
	public delegate void Item3DResLoadedEventHandler(GameObject inPrefab, bool inSuccess);

	public delegate void ItemTextureResLoadedEventHandler(Texture inTexture, bool inSuccess);

	private ItemData mItemData;

	private RsResourceLoadEvent mLoadStatus;

	public RsResourceLoadEvent mItemDataLoadStatus;

	private RsResourceLoadEvent m3DPreabLoadStatus;

	private RsResourceLoadEvent mTextureLoadStatus;

	private PreviewItemDataEventHandler mLoadItemDataCallback;

	private Item3DResLoadedEventHandler m3DPrefabResLoadCallback;

	private ItemTextureResLoadedEventHandler mTextureResCallback;

	private PreviewItemDataEventHandler mItemDataResCallback;

	private GameObject m3DPrefab;

	private Texture mTexture;

	private bool m3DPrefabLoadWaiting;

	private bool mTextureLoadWaiting;

	public ItemData pItemData => mItemData;

	public GameObject p3DPrefab => m3DPrefab;

	public Texture pTexture => mTexture;

	public RsResourceLoadEvent pLoadStatus => mLoadStatus;

	public int Quantity { get; set; }

	public PreviewItemData(ItemData inItemData, int quantity)
	{
		mItemData = inItemData;
		mItemDataLoadStatus = RsResourceLoadEvent.COMPLETE;
		Quantity = quantity;
	}

	public PreviewItemData(int inItemID, int quantity)
	{
		mItemData = new ItemData();
		mItemData.ItemID = inItemID;
		mItemDataLoadStatus = RsResourceLoadEvent.NONE;
		Quantity = quantity;
	}

	public void Load(PreviewItemDataEventHandler inCallback)
	{
		if (mItemDataResCallback == null)
		{
			mItemDataResCallback = inCallback;
		}
		else
		{
			mItemDataResCallback = (PreviewItemDataEventHandler)Delegate.Combine(mItemDataResCallback, inCallback);
		}
		if (mLoadStatus == RsResourceLoadEvent.COMPLETE)
		{
			if (mItemDataResCallback != null)
			{
				mItemDataResCallback(this, inSuccess: true);
			}
		}
		else if (mLoadStatus == RsResourceLoadEvent.ERROR)
		{
			if (mItemDataResCallback != null)
			{
				mItemDataResCallback(null, inSuccess: false);
			}
		}
		else if (mLoadStatus == RsResourceLoadEvent.NONE)
		{
			mLoadStatus = RsResourceLoadEvent.PROGRESS;
			Load3DPrefab(Item3DResLoadedCallback);
			LoadTexture(ItemTextureResLoadedEventCallback);
		}
	}

	private void ItemLoadedEventCallback(PreviewItemData inItemData, bool inSuccess)
	{
		if (inSuccess)
		{
			if (m3DPrefabLoadWaiting)
			{
				Load3DPrefab(null);
			}
			if (mTextureLoadWaiting)
			{
				LoadTexture(null);
			}
		}
		else
		{
			if (m3DPrefabLoadWaiting && m3DPrefabResLoadCallback != null)
			{
				m3DPrefabResLoadCallback(null, inSuccess: false);
			}
			if (mTextureLoadWaiting && mTextureResCallback != null)
			{
				mTextureResCallback(null, inSuccess: false);
			}
		}
	}

	private void Item3DResLoadedCallback(GameObject inPrefab, bool inSuccess)
	{
		if (mItemDataLoadStatus == RsResourceLoadEvent.ERROR || m3DPreabLoadStatus == RsResourceLoadEvent.ERROR || mTextureLoadStatus == RsResourceLoadEvent.ERROR)
		{
			mLoadStatus = RsResourceLoadEvent.ERROR;
			mItemDataResCallback(this, inSuccess: false);
		}
		else if (mItemDataLoadStatus == RsResourceLoadEvent.COMPLETE && m3DPreabLoadStatus == RsResourceLoadEvent.COMPLETE && mTextureLoadStatus == RsResourceLoadEvent.COMPLETE)
		{
			mLoadStatus = RsResourceLoadEvent.COMPLETE;
			mItemDataResCallback(this, inSuccess: true);
		}
	}

	public void ItemTextureResLoadedEventCallback(Texture inTexture, bool inSuccess)
	{
		if (mItemDataLoadStatus == RsResourceLoadEvent.ERROR || m3DPreabLoadStatus == RsResourceLoadEvent.ERROR || mTextureLoadStatus == RsResourceLoadEvent.ERROR)
		{
			mLoadStatus = RsResourceLoadEvent.ERROR;
			mItemDataResCallback(this, inSuccess: false);
		}
		else if (mItemDataLoadStatus == RsResourceLoadEvent.COMPLETE && m3DPreabLoadStatus == RsResourceLoadEvent.COMPLETE && mTextureLoadStatus == RsResourceLoadEvent.COMPLETE)
		{
			mLoadStatus = RsResourceLoadEvent.COMPLETE;
			mItemDataResCallback(this, inSuccess: true);
		}
	}

	public void LoadItemData(PreviewItemDataEventHandler inCallback)
	{
		if (inCallback != null)
		{
			if (mLoadItemDataCallback == null)
			{
				mLoadItemDataCallback = inCallback;
			}
			else
			{
				mLoadItemDataCallback = (PreviewItemDataEventHandler)Delegate.Combine(mLoadItemDataCallback, inCallback);
			}
		}
		switch (mItemDataLoadStatus)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (mLoadItemDataCallback != null)
			{
				mLoadItemDataCallback(this, inSuccess: true);
			}
			break;
		case RsResourceLoadEvent.ERROR:
			if (mLoadItemDataCallback != null)
			{
				mLoadItemDataCallback(null, inSuccess: false);
			}
			break;
		case RsResourceLoadEvent.NONE:
		{
			mItemDataLoadStatus = RsResourceLoadEvent.PROGRESS;
			ItemData item = ItemStoreDataLoader.GetItem(mItemData.ItemID);
			if (item == null)
			{
				ItemData.Load(mItemData.ItemID, ItemDataEventHandler, null);
				break;
			}
			mItemData = item;
			mItemDataLoadStatus = RsResourceLoadEvent.COMPLETE;
			if (mLoadItemDataCallback != null)
			{
				mLoadItemDataCallback(this, inSuccess: true);
			}
			break;
		}
		case RsResourceLoadEvent.PROGRESS:
			break;
		}
	}

	public void Load3DPrefab(Item3DResLoadedEventHandler inCallback)
	{
		if (inCallback != null)
		{
			if (m3DPrefabResLoadCallback == null)
			{
				m3DPrefabResLoadCallback = inCallback;
			}
			else
			{
				m3DPrefabResLoadCallback = (Item3DResLoadedEventHandler)Delegate.Combine(m3DPrefabResLoadCallback, inCallback);
			}
		}
		m3DPrefabLoadWaiting = true;
		switch (mItemDataLoadStatus)
		{
		case RsResourceLoadEvent.NONE:
			LoadItemData(ItemLoadedEventCallback);
			break;
		case RsResourceLoadEvent.ERROR:
			m3DPrefabResLoadCallback(null, inSuccess: false);
			break;
		case RsResourceLoadEvent.COMPLETE:
			switch (m3DPreabLoadStatus)
			{
			case RsResourceLoadEvent.NONE:
			{
				m3DPrefabLoadWaiting = false;
				m3DPreabLoadStatus = RsResourceLoadEvent.PROGRESS;
				ItemPrefabResData itemPrefabResData = new ItemPrefabResData();
				itemPrefabResData.Init(Item3DEventHandler, mItemData.AssetName);
				if (string.IsNullOrEmpty(mItemData.AssetName) || mItemData.AssetName == "NULL" || string.IsNullOrEmpty(itemPrefabResData._ResBundleName))
				{
					Item3DEventHandler(itemPrefabResData);
				}
				else
				{
					itemPrefabResData.LoadData();
				}
				break;
			}
			case RsResourceLoadEvent.COMPLETE:
				m3DPrefabResLoadCallback(m3DPrefab, inSuccess: true);
				break;
			case RsResourceLoadEvent.ERROR:
				m3DPrefabResLoadCallback(null, inSuccess: false);
				break;
			case RsResourceLoadEvent.PROGRESS:
				break;
			}
			break;
		case RsResourceLoadEvent.PROGRESS:
			break;
		}
	}

	public void LoadTexture(ItemTextureResLoadedEventHandler inCallback)
	{
		if (m3DPrefabResLoadCallback == null)
		{
			mTextureResCallback = inCallback;
		}
		else
		{
			mTextureResCallback = (ItemTextureResLoadedEventHandler)Delegate.Combine(mTextureResCallback, inCallback);
		}
		mTextureLoadWaiting = true;
		switch (mItemDataLoadStatus)
		{
		case RsResourceLoadEvent.NONE:
			LoadItemData(ItemLoadedEventCallback);
			break;
		case RsResourceLoadEvent.ERROR:
			mTextureResCallback(null, inSuccess: false);
			break;
		case RsResourceLoadEvent.COMPLETE:
			switch (mTextureLoadStatus)
			{
			case RsResourceLoadEvent.NONE:
			{
				mTextureLoadWaiting = false;
				mTextureLoadStatus = RsResourceLoadEvent.PROGRESS;
				ItemTextureResData itemTextureResData = new ItemTextureResData();
				if (mItemData.Texture != null)
				{
					itemTextureResData.Init(ItemTextureEventHandler, mItemData.Texture[0].TextureName);
					itemTextureResData.LoadData();
				}
				else
				{
					itemTextureResData.Init("");
					ItemTextureEventHandler(null);
				}
				break;
			}
			case RsResourceLoadEvent.COMPLETE:
				mTextureResCallback(mTexture, inSuccess: true);
				break;
			case RsResourceLoadEvent.ERROR:
				mTextureResCallback(null, inSuccess: false);
				break;
			case RsResourceLoadEvent.PROGRESS:
				break;
			}
			break;
		case RsResourceLoadEvent.PROGRESS:
			break;
		}
	}

	private void ItemTextureEventHandler(ItemResNameData nameResData)
	{
		mTextureLoadStatus = RsResourceLoadEvent.COMPLETE;
		if (mTextureResCallback != null)
		{
			if (!(nameResData is ItemTextureResData itemTextureResData) || itemTextureResData._Texture == null)
			{
				mTextureResCallback(null, inSuccess: true);
			}
			else if (mTextureResCallback != null)
			{
				mTexture = itemTextureResData._Texture;
				mTextureResCallback(itemTextureResData._Texture, inSuccess: true);
			}
		}
	}

	private void Item3DEventHandler(ItemResNameData inResNameData)
	{
		if (!(inResNameData is ItemPrefabResData itemPrefabResData))
		{
			m3DPreabLoadStatus = RsResourceLoadEvent.ERROR;
			if (m3DPrefabResLoadCallback != null)
			{
				m3DPrefabResLoadCallback(null, inSuccess: false);
			}
			return;
		}
		m3DPreabLoadStatus = RsResourceLoadEvent.COMPLETE;
		if (itemPrefabResData._Prefab != null)
		{
			m3DPrefab = itemPrefabResData._Prefab.gameObject;
		}
		if (m3DPrefabResLoadCallback != null)
		{
			m3DPrefabResLoadCallback(m3DPrefab, inSuccess: true);
		}
	}

	public void ItemDataEventHandler(int itemID, ItemData dataItem, object inUserData)
	{
		if (mItemDataLoadStatus != RsResourceLoadEvent.PROGRESS)
		{
			return;
		}
		mItemData = dataItem;
		if (mItemData == null)
		{
			mItemDataLoadStatus = RsResourceLoadEvent.ERROR;
			if (mItemDataResCallback != null)
			{
				mItemDataResCallback(null, inSuccess: false);
			}
		}
		else
		{
			mItemDataLoadStatus = RsResourceLoadEvent.COMPLETE;
			if (mLoadItemDataCallback != null)
			{
				mLoadItemDataCallback(this, inSuccess: true);
			}
		}
	}
}
