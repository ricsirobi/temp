using UnityEngine;

public class KAUIStoreFullPreview : KAUI
{
	public Camera _StorePreviewCamera;

	public Camera _StoreFullPreviewCamera;

	public GameObject _StoreBackground;

	public GameObject _FullPreviewBackground;

	public Transform _AvatarZoomMarker;

	public Transform _AvatarMarker;

	public Transform _PetMarker;

	public Transform _ItemMarker;

	public KAWidget _DragWidget;

	[Header("Multipliers")]
	public float _AvatarZoomInScale = 1f;

	public float _AvatarZoomOutScale = 1f;

	public float _PetScale = 1f;

	public float _ItemScale = 1f;

	[Space(15f)]
	public string _ItemViewInfoName = "StoreFullPreview";

	public Vector3 _ItemOffsetPosition;

	public float _RotateSpeed = 100f;

	private Vector3 mPrevPos;

	private Vector3 mPrevScale;

	private Quaternion mPrevRot;

	private StorePreviewCategory mCatergory;

	private Transform mFullPreviewTransform;

	private KAWidget mBtnClose;

	private KAWidget mBtnRotateLeft;

	private KAWidget mBtnRotateRight;

	private bool mZoom;

	private bool mDisableStat;

	private bool mDisableDragonStat;

	protected override void Start()
	{
		mBtnClose = FindItem("BtnPreviewClose");
		mBtnRotateLeft = FindItem("BtnPreviewPopupRotateLt");
		mBtnRotateRight = FindItem("BtnPreviewPopupRotateRt");
		base.Start();
	}

	public void SetFullPreviewInfo(Transform previewTransform, StorePreviewCategory category, bool isZoomed = false)
	{
		mFullPreviewTransform = previewTransform;
		mCatergory = category;
		mZoom = isZoomed;
	}

	public override void OnClick(KAWidget item)
	{
		if (item == mBtnClose)
		{
			CloseFullPreview();
		}
		base.OnClick(item);
	}

	public override void OnPressRepeated(KAWidget item, bool inPressed)
	{
		if (item == mBtnRotateLeft)
		{
			RotatePreview(_RotateSpeed);
		}
		else if (item == mBtnRotateRight)
		{
			RotatePreview(0f - _RotateSpeed);
		}
	}

	public override void OnDrag(KAWidget inWidget, Vector2 inDelta)
	{
		if (inWidget == _DragWidget)
		{
			RotatePreview((0f - _RotateSpeed) * inDelta.x);
		}
		base.OnDrag(inWidget, inDelta);
	}

	public void RotatePreview(float rotationDelta)
	{
		mFullPreviewTransform.Rotate(0f, rotationDelta * Time.deltaTime, 0f, Space.World);
	}

	public void ShowFullPreview()
	{
		if (mFullPreviewTransform == null || mCatergory == StorePreviewCategory.None)
		{
			return;
		}
		EnableFullPreview(enable: true);
		mPrevPos = mFullPreviewTransform.position;
		mPrevRot = mFullPreviewTransform.rotation;
		mPrevScale = mFullPreviewTransform.localScale;
		Vector3 position = Vector3.zero;
		float num = 1f;
		if (mCatergory == StorePreviewCategory.Avatar)
		{
			position = (mZoom ? _AvatarZoomMarker.position : _AvatarMarker.position);
			num = (mZoom ? _AvatarZoomInScale : _AvatarZoomOutScale);
		}
		else if (mCatergory == StorePreviewCategory.RaisedPet)
		{
			position = _PetMarker.position;
			num = _PetScale;
		}
		else if (mCatergory == StorePreviewCategory.Normal3D)
		{
			ObInfo component = mFullPreviewTransform.GetComponent<ObInfo>();
			if (component != null && component.ApplyViewInfo(_ItemViewInfoName))
			{
				return;
			}
			position = mFullPreviewTransform.position + _ItemOffsetPosition;
			num = _ItemScale;
		}
		mFullPreviewTransform.localScale *= num;
		mFullPreviewTransform.position = position;
	}

	private void CloseFullPreview()
	{
		mFullPreviewTransform.position = mPrevPos;
		mFullPreviewTransform.rotation = mPrevRot;
		mFullPreviewTransform.localScale = mPrevScale;
		EnableFullPreview(enable: false);
	}

	private void EnableFullPreview(bool enable)
	{
		_StorePreviewCamera.enabled = !enable;
		_StoreFullPreviewCamera.enabled = enable;
		_StoreBackground.SetActive(!enable);
		_FullPreviewBackground.SetActive(enable);
		KAUIStore.pInstance.HideStoreUIs(enable);
		SetVisibility(enable);
		KAUIStoreChoose3D kAUIStoreChoose3D = (KAUIStoreChoose3D)KAUIStore.pInstance.pChooseUI;
		if (kAUIStoreChoose3D != null && mCatergory == StorePreviewCategory.Avatar)
		{
			kAUIStoreChoose3D._AllowZoomInZoomOut = !enable;
			if (enable)
			{
				kAUIStoreChoose3D.FinalizeZoomInZoomOut();
			}
		}
		if (KAUIStore.pInstance.pStatCompareMenu != null)
		{
			if (KAUIStore.pInstance.pStatCompareMenu.GetVisibility() && !mDisableStat)
			{
				mDisableStat = true;
				KAUIStore.pInstance.pStatCompareMenu.SetVisibility(inVisible: false);
			}
			else if (mDisableStat)
			{
				mDisableStat = false;
				KAUIStore.pInstance.pStatCompareMenu.SetVisibility(inVisible: true);
			}
		}
		if (KAUIStore.pInstance.pDragonStatMenu != null)
		{
			if (KAUIStore.pInstance.pDragonStatMenu.GetVisibility() && !mDisableDragonStat)
			{
				mDisableDragonStat = true;
				KAUIStore.pInstance.pDragonStatMenu.SetVisibility(inVisible: false);
			}
			else if (mDisableDragonStat)
			{
				mDisableDragonStat = false;
				KAUIStore.pInstance.pDragonStatMenu.SetVisibility(inVisible: true);
			}
		}
	}
}
