using UnityEngine;

public class UiSceneMiniMap : KAMonoBase
{
	public Vector3 _TerrainCenterPosition = Vector3.zero;

	public float _TerrainWidth = 800f;

	public float _TerrainLength = 800f;

	public Transform _Map;

	public UITexture _MapTexture;

	private float mHeightRatio = 1f;

	private float mWidthRatio = 1f;

	private Vector3 mMapDisplacementVect = Vector3.zero;

	private void Awake()
	{
		if (AvAvatar.pToolbar != null)
		{
			base.transform.parent = AvAvatar.pToolbar.transform;
		}
	}

	private void Start()
	{
		if (_MapTexture != null && _Map != null && _TerrainWidth > 0f && _TerrainLength > 0f)
		{
			mHeightRatio = (float)_MapTexture.height / _TerrainLength;
			mWidthRatio = (float)_MapTexture.width / _TerrainWidth;
			mMapDisplacementVect = _Map.localPosition;
		}
	}

	private void Update()
	{
		if (AvAvatar.mTransform != null && _Map != null)
		{
			mMapDisplacementVect.x = (_TerrainCenterPosition.x - AvAvatar.mTransform.localPosition.x) * mWidthRatio;
			mMapDisplacementVect.y = (_TerrainCenterPosition.z - AvAvatar.mTransform.localPosition.z) * mHeightRatio;
			_Map.localPosition = mMapDisplacementVect;
		}
	}
}
