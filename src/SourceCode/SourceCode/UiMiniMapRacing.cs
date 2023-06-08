using System;
using System.Collections.Generic;
using UnityEngine;

public class UiMiniMapRacing : MonoBehaviour
{
	public Vector3 _TrackCenter = Vector3.zero;

	public Rect _MapPosition = new Rect(0f, 0f, 128f, 128f);

	public Vector2 _DevScale = new Vector2(1024f, 768f);

	public int _BlipHeight = 8;

	public int _BlipWidth = 8;

	public float _TerrainWidth = 100f;

	public float _TerrainHeight = 100f;

	public float _OffsetX;

	public float _OffsetY;

	public float _AngleOffset;

	public KAUI _MapUI;

	public KAWidget _MapWidget;

	public KAWidget _PlayerBlip;

	public KAWidget _OtherPlayerBlipTemplate;

	private Vector2 mMapCenter = Vector2.zero;

	private float mMapScaleX = 0.3f;

	private float mMapScaleY = 0.3f;

	private Rect mBlipDrawPos;

	private int mHalfWidthAndHeight;

	private bool mUpdateMapCenter;

	private LevelManager mLevelManager;

	private Dictionary<string, KAWidget> mOpponentBlips = new Dictionary<string, KAWidget>();

	public void Init(int numPlayers, LevelManager levelManager)
	{
		mBlipDrawPos = new Rect(0f, 0f, _BlipWidth, _BlipHeight);
		mHalfWidthAndHeight = _BlipWidth / 2;
		_MapPosition.height = _MapPosition.width / (_TerrainWidth / _TerrainHeight);
		SetMapPosition(numPlayers);
		mMapScaleX = _MapPosition.width / _TerrainWidth;
		mMapScaleY = _MapPosition.height / _TerrainHeight;
		mMapScaleX *= (float)Screen.width / _DevScale.x;
		mMapScaleY *= (float)Screen.height / _DevScale.y;
		mLevelManager = levelManager;
	}

	private void SetMapPosition(int numPlayers)
	{
		_MapPosition.x = (float)Screen.width - _MapPosition.width;
		_MapPosition.y = (float)(Screen.height / 2) - _MapPosition.height / 2f;
	}

	private void Update()
	{
		UpdateBlips();
	}

	private void OnDisable()
	{
		_PlayerBlip.SetVisibility(inVisible: false);
		foreach (KAWidget value in mOpponentBlips.Values)
		{
			if (value != null)
			{
				value.SetVisibility(inVisible: false);
			}
		}
	}

	private void UpdateBlips()
	{
		if (mLevelManager == null || mLevelManager.pPlayerData == null || mLevelManager.pPlayerData.Count <= 0)
		{
			return;
		}
		int count = mLevelManager.pPlayerData.Count;
		PlayerData playerData = null;
		for (int i = 0; i < count; i++)
		{
			PlayerData playerData2 = mLevelManager.pPlayerData[i];
			if (playerData2.mAvatarRacing.pGameCompleted)
			{
				continue;
			}
			if (playerData2.mAvatarRacing.pType == AvatarRacing.Type.USER)
			{
				playerData = playerData2;
			}
			else if (playerData2.mAvatarRacing.pType != AvatarRacing.Type.END && playerData2.mAvatar.pObject != null)
			{
				bool flag = mOpponentBlips.ContainsKey(playerData2.mUserId);
				if (!flag)
				{
					mOpponentBlips[playerData2.mUserId] = _MapUI.DuplicateWidget(_OtherPlayerBlipTemplate);
					flag = true;
				}
				if (flag)
				{
					UpdateBlip(playerData2.mAvatar.pObject.transform.position, mOpponentBlips[playerData2.mUserId]);
				}
			}
			else if (mOpponentBlips.ContainsKey(playerData2.mUserId))
			{
				mOpponentBlips[playerData2.mUserId].SetVisibility(inVisible: false);
			}
		}
		if (playerData != null && !playerData.mAvatarRacing.pGameCompleted)
		{
			UpdateBlip(AvAvatar.GetPosition(), _PlayerBlip);
		}
		else
		{
			_PlayerBlip.SetVisibility(inVisible: false);
		}
	}

	private void UpdateBlip(Vector3 position, KAWidget blip)
	{
		position.y = _TrackCenter.y;
		float num = Vector3.Distance(_TrackCenter, position);
		float y = _TrackCenter.x - position.x;
		float x = _TrackCenter.z - position.z;
		float num2 = Mathf.Atan2(y, x) * 57.29578f - _AngleOffset;
		float num3 = num * Mathf.Cos(num2 * (MathF.PI / 180f));
		float num4 = num * Mathf.Sin(num2 * (MathF.PI / 180f));
		num3 *= mMapScaleX;
		num4 *= mMapScaleY;
		if (KAUIManager.pInstance != null && KAUIManager.pInstance.camera != null && (bool)_MapWidget && !mUpdateMapCenter)
		{
			mUpdateMapCenter = true;
			mMapCenter = KAUIManager.pInstance.camera.WorldToScreenPoint(_MapWidget.transform.position);
			mMapCenter.y = (float)Screen.height - mMapCenter.y;
		}
		mBlipDrawPos.x = mMapCenter.x + num3 + _OffsetX;
		mBlipDrawPos.y = mMapCenter.y + num4 + _OffsetY;
		mBlipDrawPos.x -= mHalfWidthAndHeight;
		mBlipDrawPos.y -= mHalfWidthAndHeight;
		if ((bool)blip)
		{
			blip.SetVisibility(inVisible: true);
			blip.SetToScreenPosition(mBlipDrawPos.x, mBlipDrawPos.y);
		}
	}
}
