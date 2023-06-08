using System;
using UnityEngine;

public class AIBehavior_DrawProximity : AIBehavior
{
	[Serializable]
	public class MaterialColorInfo
	{
		public string _Property;

		public Color _Color;
	}

	public float _Radius;

	public Color _InRangeColor = Color.green;

	public Color _OutRangeColor = Color.red;

	public MaterialColorInfo[] _InRangeColorInfo;

	public MaterialColorInfo[] _OutRangeColorInfo;

	public float _Width = 1f;

	public int _Segments = 20;

	public Vector3 _Offset = new Vector3(0f, 1f, 0f);

	public Material _Material;

	public GameObject _ProximityProjector;

	public GameObject _ProximitySphere;

	protected ProximitySphere mProximitySphere;

	protected LineRenderer mLineRenderer;

	protected Projector mProjector;

	public bool _UseProjectorCircle;

	private bool mInRange = true;

	public void InitLineRenderer(AIActor Actor)
	{
		mLineRenderer = Actor.gameObject.AddComponent<LineRenderer>();
		if (_Material == null)
		{
			_Material = new Material(Shader.Find("Particles/Additive"));
		}
		mLineRenderer.material = _Material;
		mLineRenderer.startColor = (mInRange ? _InRangeColor : _OutRangeColor);
		mLineRenderer.endColor = (mInRange ? _InRangeColor : _OutRangeColor);
		mLineRenderer.startWidth = _Width;
		mLineRenderer.endWidth = _Width;
		mLineRenderer.positionCount = _Segments + 1;
	}

	public void Init(AIActor Actor)
	{
		if (_UseProjectorCircle)
		{
			GameObject gameObject = UnityEngine.Object.Instantiate(_ProximityProjector);
			if (gameObject != null)
			{
				gameObject.transform.parent = Actor.transform;
				gameObject.transform.position = Actor.transform.position + Vector3.up * 3f;
				mProjector = gameObject.GetComponent<Projector>();
			}
			if (mProjector != null)
			{
				mProjector.orthographicSize = _Radius;
			}
		}
		else if (_ProximitySphere != null)
		{
			GameObject gameObject2 = UnityEngine.Object.Instantiate(_ProximitySphere);
			gameObject2.transform.parent = Actor.transform;
			gameObject2.transform.position = Actor.transform.position;
			mProximitySphere = gameObject2.GetComponent<ProximitySphere>();
			mProximitySphere.ScaleWithFactor(_Radius);
		}
		UpdateColor(mInRange);
	}

	public void SetProximity(float radius)
	{
		_Radius = radius;
		if (_UseProjectorCircle)
		{
			if (mProjector != null)
			{
				mProjector.orthographicSize = radius;
			}
		}
		else if (mProximitySphere != null)
		{
			mProximitySphere.ScaleWithFactor(_Radius);
		}
	}

	public void UpdateColor(bool inRange)
	{
		mInRange = inRange;
		if (_UseProjectorCircle)
		{
			if (mLineRenderer != null)
			{
				mLineRenderer.startColor = (mInRange ? _InRangeColor : _OutRangeColor);
				mLineRenderer.endColor = (mInRange ? _InRangeColor : _OutRangeColor);
			}
			else
			{
				if (!(mProjector != null))
				{
					return;
				}
				MaterialColorInfo[] array = (mInRange ? _InRangeColorInfo : _OutRangeColorInfo);
				foreach (MaterialColorInfo materialColorInfo in array)
				{
					if (mProjector.material.HasProperty(materialColorInfo._Property))
					{
						mProjector.material.SetColor(materialColorInfo._Property, materialColorInfo._Color);
					}
				}
			}
		}
		else if (mProximitySphere != null)
		{
			MaterialColorInfo[] array = (mInRange ? _InRangeColorInfo : _OutRangeColorInfo);
			foreach (MaterialColorInfo materialColorInfo2 in array)
			{
				mProximitySphere.SetColor(materialColorInfo2._Property, materialColorInfo2._Color);
			}
		}
	}

	public override AIBehaviorState Think(AIActor Actor)
	{
		if (Actor.pHasController)
		{
			OnTerminate(Actor);
			return AIBehaviorState.INACTIVE;
		}
		if (_Radius <= 0f)
		{
			return SetState(AIBehaviorState.ACTIVE);
		}
		if (_UseProjectorCircle)
		{
			if (Actor.IsFlying())
			{
				DrawProximityInFlying(Actor);
			}
			else
			{
				DrawProximityCircle(Actor);
			}
		}
		else
		{
			DrawProximitySphere(Actor);
		}
		return SetState(AIBehaviorState.ACTIVE);
	}

	public override void OnTerminate(AIActor Actor)
	{
		if (_UseProjectorCircle)
		{
			UnityEngine.Object.Destroy(mLineRenderer);
			if (mProjector != null)
			{
				mProjector.enabled = false;
			}
		}
		else if (mProximitySphere != null)
		{
			mProximitySphere.gameObject.SetActive(value: false);
		}
		base.OnTerminate(Actor);
	}

	public void DrawProximityInFlying(AIActor Actor)
	{
		if (mLineRenderer == null)
		{
			InitLineRenderer(Actor);
		}
		if (mProjector != null && mProjector.enabled)
		{
			mProjector.enabled = false;
		}
		if (mLineRenderer != null)
		{
			if (!mLineRenderer.enabled)
			{
				mLineRenderer.enabled = true;
			}
			float num = MathF.PI * 2f / (float)_Segments;
			float num2 = 0f;
			for (int i = 0; i < _Segments + 1; i++)
			{
				float x = _Radius * Mathf.Cos(num2);
				float z = _Radius * Mathf.Sin(num2);
				Vector3 position = new Vector3(x, 0f, z);
				position = Actor.transform.TransformPoint(position);
				mLineRenderer.SetPosition(i, position);
				num2 += num;
			}
		}
	}

	public void DrawProximityCircle(AIActor Actor)
	{
		if (mProjector == null)
		{
			Init(Actor);
		}
		if (mLineRenderer != null && mLineRenderer.enabled)
		{
			mLineRenderer.enabled = false;
		}
		if (mProjector != null && !mProjector.enabled)
		{
			mProjector.enabled = true;
		}
	}

	public void DrawProximitySphere(AIActor Actor)
	{
		if (mProximitySphere == null)
		{
			Init(Actor);
		}
		if (mProximitySphere != null && !mProximitySphere.gameObject.activeSelf)
		{
			mProximitySphere.gameObject.SetActive(value: true);
		}
	}
}
