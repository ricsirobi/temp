using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

public class AIBehavior_DrawSearchZone : AIBehavior
{
	[Serializable]
	public enum DisplayType
	{
		Spotlight,
		ProceduralMesh
	}

	[Serializable]
	public class PlatformDisplayConfig
	{
		public DisplayType _DisplayType = DisplayType.ProceduralMesh;

		public List<UtPlatform.PlatformType> _PlatformType;
	}

	[Serializable]
	public class MeshColorMapping
	{
		public string _PropertyName;

		public Color _AlertedColor;

		public Color _SafeColor;
	}

	public Color _SpotLightAlertedColor = Color.white;

	public Color _SpotLightSafeColor = Color.white;

	public DisplayType _DefaultDisplayType;

	public List<PlatformDisplayConfig> _PlatformDisplayConfig;

	public Material _LineRenderMaterial;

	public Renderer _MeshZoneRenderer;

	public GameObject _MeshZoneObject;

	public MeshColorMapping[] _MeshColorMapping;

	private const int mLineRendererCurveSegments = 8;

	private LineRenderer mOuterLineRenderer;

	private LineRenderer mInnerLineRenderer;

	private AIActor_NPC.SearchZoneData.SpotlightInfo mSpotlightInfo;

	private AIActor_NPC.SearchZoneData mSearchZoneData;

	private bool mShowLineRenderer;

	private float mSpotlightIntensity;

	public LineRenderer InitLineRenderer()
	{
		GameObject gameObject = new GameObject();
		gameObject.name = "Zone";
		gameObject.transform.parent = base.transform;
		LineRenderer lineRenderer = gameObject.AddComponent<LineRenderer>();
		lineRenderer.material = _LineRenderMaterial;
		lineRenderer.startColor = _SpotLightSafeColor;
		lineRenderer.endColor = _SpotLightSafeColor;
		lineRenderer.positionCount = 10;
		lineRenderer.loop = true;
		lineRenderer.receiveShadows = false;
		lineRenderer.shadowCastingMode = ShadowCastingMode.Off;
		lineRenderer.alignment = LineAlignment.Local;
		gameObject.transform.eulerAngles = new Vector3(90f, 0f, 0f);
		lineRenderer.widthMultiplier = 0.1f;
		return lineRenderer;
	}

	public void SetAlert(bool alerted)
	{
		if (_DefaultDisplayType == DisplayType.Spotlight)
		{
			SetSpotLightColor(alerted ? _SpotLightAlertedColor : _SpotLightSafeColor);
		}
		else
		{
			SetMeshZoneColor(alerted);
		}
	}

	private void SetSpotLightColor(Color inColor)
	{
		if (mOuterLineRenderer != null)
		{
			mOuterLineRenderer.startColor = inColor;
			mOuterLineRenderer.endColor = inColor;
		}
		if (mInnerLineRenderer != null)
		{
			mInnerLineRenderer.startColor = inColor;
			mInnerLineRenderer.endColor = inColor;
		}
		if (mSpotlightInfo._Spotlight != null)
		{
			mSpotlightInfo._Spotlight.color = inColor;
		}
	}

	private void SetMeshZoneColor(bool alerted)
	{
		if (!(_MeshZoneRenderer != null))
		{
			return;
		}
		for (int i = 0; i < _MeshColorMapping.Length; i++)
		{
			if (alerted)
			{
				if (_MeshZoneRenderer.material.HasProperty(_MeshColorMapping[i]._PropertyName))
				{
					_MeshZoneRenderer.material.SetColor(_MeshColorMapping[i]._PropertyName, _MeshColorMapping[i]._AlertedColor);
				}
			}
			else if (_MeshZoneRenderer.material.HasProperty(_MeshColorMapping[i]._PropertyName))
			{
				_MeshZoneRenderer.material.SetColor(_MeshColorMapping[i]._PropertyName, _MeshColorMapping[i]._SafeColor);
			}
		}
	}

	private void CreateMeshZone(AIActor Actor)
	{
		if (_MeshZoneObject != null)
		{
			FieldOfViewMesh fieldOfViewMesh = new FieldOfViewMesh();
			fieldOfViewMesh.Initialize(_MeshZoneObject.GetComponent<MeshFilter>());
			_MeshZoneObject.transform.rotation = Actor.transform.rotation;
			_MeshZoneObject.transform.position = Actor.transform.position + Actor.transform.rotation * mSearchZoneData._OuterZone._Offset;
			fieldOfViewMesh.GenerateFOV(mSearchZoneData._OuterZone._Angle, mSearchZoneData._OuterZone._Length, mSearchZoneData._OuterZone._CurveSegments, mSearchZoneData._OuterZone._ZoneYPolygonLevel);
		}
	}

	public override void OnStart(AIActor Actor)
	{
		base.OnStart(Actor);
		mSearchZoneData = ((AIActor_NPC)Actor)._SearchZoneData;
		UpdateDisplayType();
		if (mSearchZoneData != null)
		{
			if (_DefaultDisplayType == DisplayType.ProceduralMesh)
			{
				CreateMeshZone(Actor);
			}
			mSpotlightInfo = mSearchZoneData._SpotlightInfo;
			if (mSpotlightInfo != null && mSpotlightInfo._Spotlight != null)
			{
				if (_DefaultDisplayType == DisplayType.ProceduralMesh)
				{
					mSpotlightInfo._Spotlight.enabled = false;
				}
				else if (mSpotlightInfo._SpotlightFadeRate > 0f)
				{
					mSpotlightIntensity = mSpotlightInfo._Spotlight.intensity;
					mSpotlightInfo._Spotlight.intensity = 0f;
				}
			}
		}
		SetAlert(alerted: false);
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (mSearchZoneData != null)
		{
			if (_DefaultDisplayType == DisplayType.Spotlight)
			{
				UpdateSpotlight(Actor);
			}
			else if (mSearchZoneData._TriggerDistance > 0f && AvAvatar.pObject != null)
			{
				bool active = Vector3.Distance(AvAvatar.pObject.transform.position, Actor.Position) <= mSearchZoneData._TriggerDistance;
				_MeshZoneObject.SetActive(active);
			}
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	private void UpdateDisplayType()
	{
		if (_PlatformDisplayConfig == null || _PlatformDisplayConfig.Count == 0)
		{
			return;
		}
		foreach (PlatformDisplayConfig item in _PlatformDisplayConfig)
		{
			if (item._PlatformType == null || item._PlatformType.Count == 0)
			{
				break;
			}
			foreach (UtPlatform.PlatformType item2 in item._PlatformType)
			{
				if (item2 == UtPlatform.GetPlatformType())
				{
					_DefaultDisplayType = item._DisplayType;
					return;
				}
			}
		}
	}

	private void UpdateSpotlight(AIActor actor)
	{
		if (mSpotlightInfo == null || !(mSpotlightInfo._Spotlight != null) || !(mSearchZoneData._TriggerDistance > 0f) || !(AvAvatar.pObject != null))
		{
			return;
		}
		bool flag = Vector3.Distance(AvAvatar.pObject.transform.position, actor.Position) <= mSearchZoneData._TriggerDistance;
		if (mSpotlightInfo._SpotlightFadeRate > 0f)
		{
			if (!flag && mSpotlightInfo._Spotlight.intensity > 0f)
			{
				mSpotlightInfo._Spotlight.intensity -= mSpotlightInfo._SpotlightFadeRate * Time.deltaTime;
				if (mSpotlightInfo._Spotlight.intensity <= 0f)
				{
					mSpotlightInfo._Spotlight.intensity = 0f;
					mSpotlightInfo._Spotlight.enabled = false;
				}
			}
			if (flag && mSpotlightInfo._Spotlight.intensity < mSpotlightIntensity)
			{
				if (!mSpotlightInfo._Spotlight.enabled)
				{
					mSpotlightInfo._Spotlight.enabled = true;
				}
				mSpotlightInfo._Spotlight.intensity += mSpotlightInfo._SpotlightFadeRate * Time.deltaTime;
				if (mSpotlightInfo._Spotlight.intensity >= mSpotlightIntensity)
				{
					mSpotlightInfo._Spotlight.intensity = mSpotlightIntensity;
				}
			}
		}
		else if (mSpotlightInfo._Spotlight.enabled != flag)
		{
			mSpotlightInfo._Spotlight.enabled = flag;
		}
	}

	private void DrawLineRendererShape(AIActor_NPC.ZoneData zoneData, Transform source, LineRenderer lineRenderer)
	{
		Vector3 vector = source.rotation * zoneData._Offset + source.position;
		Vector3 vector2 = Quaternion.AngleAxis(zoneData._Angle / 2f, source.up) * source.forward * zoneData._Length;
		Vector3 position = vector2 + vector;
		Vector3 b = Quaternion.AngleAxis((0f - zoneData._Angle) / 2f, source.up) * source.forward * zoneData._Length;
		lineRenderer.positionCount = 10;
		lineRenderer.SetPosition(0, vector);
		lineRenderer.SetPosition(1, position);
		for (int i = 1; i <= 8; i++)
		{
			Vector3 position2 = Vector3.Slerp(vector2, b, (float)i / 8f) + vector;
			lineRenderer.SetPosition(i + 1, position2);
		}
	}

	public override void OnTerminate(AIActor Actor)
	{
		if (mOuterLineRenderer != null)
		{
			UnityEngine.Object.Destroy(mOuterLineRenderer.gameObject);
		}
		if (mInnerLineRenderer != null)
		{
			UnityEngine.Object.Destroy(mInnerLineRenderer.gameObject);
		}
		if (mSpotlightInfo != null && mSpotlightInfo._Spotlight != null)
		{
			mSpotlightInfo._Spotlight.enabled = false;
		}
	}
}
