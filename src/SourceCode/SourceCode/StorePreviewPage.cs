using UnityEngine;

public class StorePreviewPage
{
	private int mParentItemID;

	private int mIndex;

	private PreviewItemDataList mPreviewItemList;

	private int mCount;

	private StorePageLoadedEventHandler mCallback;

	private Transform mCurrent3DObject;

	private Transform m3DMarker;

	private KAWidget m2DViewer;

	private bool mActive;

	public bool mAllowScaling;

	public float mScaleLength;

	public int mPreviewItemIndex = -1;

	public int pParentItemID => mParentItemID;

	public PreviewItemDataList pPreviewItemList => mPreviewItemList;

	public Transform pCurrent3DObject => mCurrent3DObject;

	public int pCount => mCount;

	public ItemData pCurrentItemData
	{
		get
		{
			if (mPreviewItemList != null && mPreviewItemList.pList != null && mPreviewItemList.pList.Count > 0 && mPreviewItemList.pList[0].pLoadStatus == RsResourceLoadEvent.COMPLETE)
			{
				return mPreviewItemList.pList[0].pItemData;
			}
			return null;
		}
	}

	public StorePreviewPage(int inParentItemID, int inItemID, Transform in3DMarker, KAWidget in2DViewer, StorePageLoadedEventHandler inCallback, int quantity)
	{
		if (inItemID > 0)
		{
			mParentItemID = inParentItemID;
			mCurrent3DObject = null;
			m3DMarker = in3DMarker;
			m2DViewer = in2DViewer;
			mAllowScaling = false;
			mScaleLength = 0f;
			mCallback = inCallback;
			AddItem(inItemID, quantity);
		}
	}

	public void AddItem(int inItemID, int quantity)
	{
		if (inItemID > 0)
		{
			if (mPreviewItemList == null)
			{
				mPreviewItemList = new PreviewItemDataList(mParentItemID);
			}
			mPreviewItemList.AddItem(inItemID, quantity);
		}
	}

	public void ShowItem()
	{
		RsResourceLoadEvent pStatus = mPreviewItemList.pStatus;
		if (pStatus != RsResourceLoadEvent.PROGRESS && pStatus != RsResourceLoadEvent.ERROR)
		{
			mActive = true;
			if (pStatus == RsResourceLoadEvent.COMPLETE)
			{
				OnAllLoaded();
			}
			else
			{
				mPreviewItemList.LoadResList(LoadPreviewItemResourceList);
			}
		}
	}

	private void LoadPreviewItemResourceList(PreviewItemDataList inPreviewItemDataList, int inParentItemID, bool inSuccess)
	{
		inSuccess = inSuccess && inParentItemID == mParentItemID;
		if (inSuccess)
		{
			OnAllLoaded();
		}
		else
		{
			mCallback(this, inSuccess);
		}
	}

	public void StopLoading()
	{
		mActive = false;
	}

	private void OnAllLoaded()
	{
		StorePreviewCategory storePreviewCategory = StorePreviewCategory.None;
		ItemData itemData = pCurrentItemData;
		if (itemData != null)
		{
			storePreviewCategory = KAUIStore.GetPreviewCategory(itemData);
		}
		if (storePreviewCategory == StorePreviewCategory.Normal3D)
		{
			if (!itemData.GetAttribute("2D", defaultValue: false))
			{
				m2DViewer.SetVisibility(inVisible: false);
				Instantiate();
			}
			else
			{
				string attribute = itemData.GetAttribute<string>("Preview", null);
				string attribute2 = itemData.GetAttribute<string>("2D Size", null);
				Set2DItem(attribute, attribute2);
			}
		}
		if (mCallback != null)
		{
			mCallback(this, inSuccess: true);
		}
	}

	private void Instantiate()
	{
		if (mPreviewItemList == null || mPreviewItemList.pList == null || mPreviewItemList.pList.Count == 0 || mPreviewItemList.pList[0].p3DPrefab == null || !mActive)
		{
			return;
		}
		GameObject p3DPrefab = mPreviewItemList.pList[0].p3DPrefab;
		Texture pTexture = mPreviewItemList.pList[0].pTexture;
		GameObject gameObject = Object.Instantiate(p3DPrefab);
		if (gameObject == null)
		{
			return;
		}
		mCurrent3DObject = gameObject.transform;
		mCurrent3DObject.name = mCurrent3DObject.name + "_" + mIndex;
		Set3DItem(mCurrent3DObject.gameObject, pCurrentItemData);
		if (pTexture != null)
		{
			UtUtilities.SetObjectTexture(mCurrent3DObject.gameObject, 0, pTexture);
		}
		Collider component = mCurrent3DObject.GetComponent<Collider>();
		if (mAllowScaling && component != null)
		{
			float num = Vector3.Distance(component.bounds.max, component.bounds.min);
			if (num > 0f)
			{
				float num2 = mScaleLength / num;
				mCurrent3DObject.localScale = num2 * Vector3.one;
				mCurrent3DObject.position += m3DMarker.position - component.bounds.center;
			}
		}
	}

	private void Set3DItem(GameObject in3DItemInstance, ItemData inItemData)
	{
		if (!(in3DItemInstance == null))
		{
			in3DItemInstance.SetActive(value: true);
			in3DItemInstance.transform.parent = m3DMarker;
			in3DItemInstance.transform.localPosition = new Vector3(0f, 0f, 0f);
			in3DItemInstance.transform.localRotation = Quaternion.identity;
			in3DItemInstance.SendMessage("ApplyViewInfo", "Store", SendMessageOptions.DontRequireReceiver);
			if (inItemData != null && inItemData.HasAttribute("StoreScale"))
			{
				float attribute = inItemData.GetAttribute("StoreScale", 1f);
				in3DItemInstance.transform.localScale = new Vector3(attribute, attribute, attribute);
			}
		}
	}

	private void Set2DItem(string assetName = null, string data = null)
	{
		if (assetName == null)
		{
			assetName = mPreviewItemList.pList[0].pItemData.IconName;
		}
		string[] array = assetName.Split('/');
		if (m2DViewer != null && array != null && array.Length > 2)
		{
			m2DViewer.ResetWidget();
			StorePreview.ScalePreview(m2DViewer, data);
			m2DViewer.SetVisibility(inVisible: false);
			m2DViewer.SetTextureFromBundle(array[0] + "/" + array[1], array[2], null, StorePreview.OnPreviewLoaded);
		}
	}

	public bool HasItem(int inItemID)
	{
		return mPreviewItemList.HasItem(inItemID);
	}

	public void IncrementCount(int quantity = 1)
	{
		mCount += quantity;
	}
}
