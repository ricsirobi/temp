using System;
using UnityEngine;

[Serializable]
public class KAStoreMenuItemData
{
	[Serializable]
	public enum StoreType
	{
		IAPStore,
		GameStore
	}

	public string _Name = "";

	public LocaleString _DisplayText;

	public StoreFilter _Filter;

	public int _MaxItemsLimit = -1;

	public Texture2D _Bkg;

	public AudioClip _RVO;

	public StoreType _StoreType = StoreType.GameStore;

	public StorePreviewCategory _PreviewMode = StorePreviewCategory.Normal3D;

	[NonSerialized]
	public int _MenuIdx = -1;

	[NonSerialized]
	public UnityEngine.Object _UserData;

	[NonSerialized]
	public bool _IsEnabled;
}
