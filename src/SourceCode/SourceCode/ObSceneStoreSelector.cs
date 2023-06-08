using UnityEngine;

public class ObSceneStoreSelector : KAMonoBase
{
	public GameObject _StoreCommon;

	public GameObject _Store4x3;

	public GameObject _Store16x9;

	private GameObject mStoreObject;

	private void Awake()
	{
		if (UtPlatform.IsMobile())
		{
			mStoreObject = (UtMobileUtilities.IsWideDisplay() ? _Store16x9 : _Store4x3);
		}
		else
		{
			mStoreObject = (UtMobileUtilities.IsWideDisplay() ? _StoreCommon : _Store4x3);
		}
		if (mStoreObject != null)
		{
			mStoreObject.name = "PfUiStoresDO";
			mStoreObject.SetActive(value: true);
			float safeAreaHeightRatio = UtMobileUtilities.GetSafeAreaHeightRatio();
			mStoreObject.transform.localScale -= new Vector3(safeAreaHeightRatio, safeAreaHeightRatio);
		}
	}
}
