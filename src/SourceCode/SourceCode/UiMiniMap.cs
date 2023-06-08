using UnityEngine;

public class UiMiniMap : KAUI
{
	public GameObject _MiniMapCamera;

	public GameObject _PlayerBlip;

	public GameObject _PlaneObject;

	public Texture _WorldTexture;

	public float _ZoomSpeed = 200f;

	public MinMax _ZoomFOV = new MinMax(30f, 60f);

	public float _DefaultFOVFactor = 1f;

	public UiSceneMap _SceneMap;

	public string _SceneMapBGTextureBundle = "RS_DATA/AniDWDragonsMapBerk.Unity3d/AniDWDragonsMapBerk";

	private Vector3 mPosition;

	private Vector3 mEulerAngles;

	private MeshFilter mFilter;

	protected override void Start()
	{
		base.Start();
		if (!(null == _WorldTexture))
		{
			Shader shader = Shader.Find("Unlit/TransparentNoFog");
			if (shader == null)
			{
				Debug.LogError("Shader not found : Unlit/TransparentNoFog");
			}
			else
			{
				Renderer component = _PlaneObject.GetComponent<Renderer>();
				component.material = new Material(shader);
				component.material.SetTextureScale("Tiling", new Vector2(0f, 0f));
				component.material.mainTexture = _WorldTexture;
			}
			if (null != _PlaneObject)
			{
				UiToolbar component2 = AvAvatar.pToolbar.GetComponent<UiToolbar>();
				_PlaneObject.transform.localPosition -= new Vector3(0f, 0f, component2.GetPriority());
				_MiniMapCamera.transform.localPosition -= new Vector3(0f, 0f, component2.GetPriority());
			}
			Camera component3 = _MiniMapCamera.GetComponent<Camera>();
			if (component3 != null)
			{
				float num = component3.orthographicSize * _DefaultFOVFactor;
				component3.orthographicSize = ((num > _ZoomFOV.Max) ? _ZoomFOV.Max : num);
			}
			if (null != _SceneMap)
			{
				_SceneMap.Show(show: false);
			}
		}
	}

	protected override void Update()
	{
		base.Update();
		if (_MiniMapCamera != null && AvAvatar.pObject != null)
		{
			mPosition.x = AvAvatar.position.x;
			mPosition.y = _MiniMapCamera.transform.position.y;
			mPosition.z = AvAvatar.position.z;
			_MiniMapCamera.transform.position = mPosition;
		}
		if (_PlayerBlip != null)
		{
			mEulerAngles.z = 180f - AvAvatar.mTransform.localEulerAngles.y;
			_PlayerBlip.transform.localEulerAngles = mEulerAngles;
		}
	}

	public override void OnClick(KAWidget inWidget)
	{
		base.OnClick(inWidget);
		if ("BtnOpenMap" == inWidget.name && !string.IsNullOrEmpty(_SceneMapBGTextureBundle))
		{
			AvAvatar.SetUIActive(inActive: false);
			AvAvatar.pState = AvAvatarState.PAUSED;
			KAUICursorManager.SetDefaultCursor("Loading");
			string[] array = _SceneMapBGTextureBundle.Split('/');
			RsResourceManager.LoadAssetFromBundle(array[0] + "/" + array[1], array[2], OnSceneMapLoaded, typeof(Texture2D));
		}
	}

	public override void OnPressRepeated(KAWidget inWidget, bool inPressed)
	{
		base.OnPressRepeated(inWidget, inPressed);
		if ("BtnDWDragonsMiniMapZoomIn" == inWidget.name)
		{
			Camera component = _MiniMapCamera.GetComponent<Camera>();
			if (component != null)
			{
				float num = component.orthographicSize - _ZoomSpeed * Time.deltaTime;
				component.orthographicSize = ((num < _ZoomFOV.Min) ? _ZoomFOV.Min : num);
			}
		}
		else if ("BtnDWDragonsMiniMapZoomOut" == inWidget.name)
		{
			Camera component2 = _MiniMapCamera.GetComponent<Camera>();
			if (component2 != null)
			{
				float num2 = component2.orthographicSize + _ZoomSpeed * Time.deltaTime;
				component2.orthographicSize = ((num2 > _ZoomFOV.Max) ? _ZoomFOV.Max : num2);
			}
		}
	}

	public void OnSceneMapLoaded(string inURL, RsResourceLoadEvent inEvent, float inProgress, object inObject, object inUserData)
	{
		switch (inEvent)
		{
		case RsResourceLoadEvent.COMPLETE:
			if (_SceneMap != null)
			{
				Texture2D bGTexture = (Texture2D)inObject;
				_SceneMap.SetBGTexture(bGTexture);
				_SceneMap.Show(show: true);
			}
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		case RsResourceLoadEvent.ERROR:
			AvAvatar.pState = AvAvatarState.IDLE;
			AvAvatar.SetUIActive(inActive: true);
			KAUICursorManager.SetDefaultCursor("Arrow");
			break;
		}
	}
}
