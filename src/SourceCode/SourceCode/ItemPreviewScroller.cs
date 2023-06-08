using System.Collections.Generic;
using UnityEngine;

public class ItemPreviewScroller : KAMonoBase
{
	public int[] _PreviewItemIDs;

	public float _Delay = 2f;

	public KAWidget _TargetWidget;

	private int mCurrentPreviewIndex;

	private float mPreviewTimer;

	private bool mIsReady;

	private int mDownloadCount;

	private List<Texture> mPreviewImages;

	private bool mDestroyed;

	protected void Start()
	{
		Init(_PreviewItemIDs);
	}

	public void Init(int[] itemIdList)
	{
		_PreviewItemIDs = itemIdList;
		if (_PreviewItemIDs != null && _PreviewItemIDs.Length != 0)
		{
			mDownloadCount = _PreviewItemIDs.Length;
			mPreviewImages = new List<Texture>();
			int[] previewItemIDs = _PreviewItemIDs;
			for (int i = 0; i < previewItemIDs.Length; i++)
			{
				ItemData.Load(previewItemIDs[i], ItemDataLoaded, null);
			}
		}
	}

	private void ItemDataLoaded(int itemID, ItemData dataItem, object inUserData)
	{
		if (dataItem != null)
		{
			LoadImage(dataItem.IconName);
		}
		else
		{
			mDownloadCount--;
		}
	}

	private void LoadImage(string inURL)
	{
		if (string.IsNullOrEmpty(inURL))
		{
			OnImageLoaded(string.Empty, RsResourceLoadEvent.ERROR, 0f, null, null);
			return;
		}
		if (inURL.StartsWith("http://"))
		{
			RsResourceManager.Load(inURL, OnImageLoaded);
			return;
		}
		string[] array = inURL.Split('/');
		if (array.Length >= 3)
		{
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnImageLoaded, typeof(Texture));
		}
	}

	private void OnImageLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inFile, object inUserData)
	{
		if (mDestroyed)
		{
			return;
		}
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (inFile != null)
			{
				mPreviewImages.Add(inFile as Texture);
			}
			mDownloadCount--;
			break;
		case RsResourceLoadEvent.ERROR:
			mDownloadCount--;
			break;
		}
		if (mDownloadCount != 0)
		{
			return;
		}
		Transform transform = base.transform.Find("LoadingGear");
		if (transform != null)
		{
			KAWidget component = transform.GetComponent<KAWidget>();
			if (component != null)
			{
				component.SetVisibility(inVisible: false);
			}
		}
		mIsReady = true;
	}

	protected void Update()
	{
		if (mIsReady && mPreviewImages.Count > 0)
		{
			mPreviewTimer -= Time.deltaTime;
			if (mPreviewTimer <= 0f)
			{
				mPreviewTimer = _Delay;
				_TargetWidget.SetTexture(mPreviewImages[mCurrentPreviewIndex]);
				mCurrentPreviewIndex++;
				mCurrentPreviewIndex %= mPreviewImages.Count;
			}
		}
	}

	private void OnDestroy()
	{
		mDestroyed = true;
	}
}
